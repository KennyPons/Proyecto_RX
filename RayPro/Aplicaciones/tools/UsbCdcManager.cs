using RayPro.configuraciones;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;


namespace RayPro.Aplicaciones.tools
{
    /// <summary>
    /// Servicio USB CDC para STM32F103C8T6.
    /// - Entrega eventos en el hilo UI si existe SynchronizationContext.
    /// - Ensambla líneas desde ReadExisting() para evitar pérdidas por fragmentos.
    /// - Filtrado por prefijo o regex.
    /// - Persistencia en __Settings.settings__ (ComPort, BaudRate, AutoConnect).
    /// - Reintento de reconexión simple si AutoConnect = true.
    /// </summary>
    public sealed class UsbCdcManager : IDisposable
    {
        private SerialPort _port;
        private readonly object _lock = new object();
        private readonly StringBuilder _rxBuffer = new StringBuilder();
        private SynchronizationContext _syncContext;
        private bool _disposed;

        // Watchdog
        private CancellationTokenSource _watchdogCts;
        private static readonly TimeSpan WatchdogInterval = TimeSpan.FromSeconds(5);

        // WMI — detecta inserción de USB en tiempo real
        private ManagementEventWatcher _usbArrivalWatcher;

        // Reconexión — un solo flag atómico evita múltiples tareas paralelas
        private int _reconnecting = 0; // 0 = libre, 1 = en progreso (Interlocked)
        private CancellationTokenSource _reconnectCts;

        // Dentro de la clase UsbCdcManager: nuevos campos y evento
        private readonly ConcurrentQueue<string> _lineQueue = new ConcurrentQueue<string>();
        private CancellationTokenSource _consumerCts;
        private Task _consumerTask;

        // Eventos (se publican en hilo UI si es posible)
        public event Action<string> DataReceived;
        public event Action<bool> ConnectionChanged;
        public event Action<string> ErrorOccurred;

       

        private const int MAX_RX_BUFFER = 64 * 1024; // 64 KB para el StringBuilder

       

        /// <summary>
        /// Voltaje en tiempo real. El ESP32 envía decimales (ej: 35.5).
        /// Se publica como entero redondeado (35.5 → 36, 35.4 → 35).
        /// </summary>
        public event Action<int, DateTime> VoltageReceived;


        public int VoltageOffset { get; set; } = 2;

        // Estado y configuración
        public bool IsConnected
        {
            get
            {
                lock (_lock) { return _port != null && _port.IsOpen; }
            }
        }

        public string LastError { get; private set; }
        public string PortName { get; private set; }
        public int BaudRate { get; private set; } = 115200;
        public bool AutoConnect { get; private set; }

        // Filtrado opcional
        public string MessagePrefix { get; set; }
        public Regex MessageRegex { get; set; }

        // Reintento
        
        public TimeSpan ReconnectBaseDelay { get; set; } = TimeSpan.FromSeconds(1);

        public UsbCdcManager()
        {
            // Capturar contexto UI si existe (puede ser null si se crea antes de Application.Run)
            _syncContext = SynchronizationContext.Current;
            LoadSettings();

            _consumerCts = new CancellationTokenSource();
            _consumerTask = Task.Run(() => ConsumerLoopAsync(_consumerCts.Token));

            StartWatchdog();
            StartUsbWatcher();
        }

        #region UI helpers

        /// <summary>
        /// Permite capturar el SynchronizationContext después de que Application.Run() haya iniciado.
        /// Llamar desde el primer Form que se cargue (ej. Login o MainRayX).
        /// </summary>
        public void CaptureSyncContext()
        {
            var ctx = SynchronizationContext.Current;
            if (ctx != null)
            {
                _syncContext = ctx;
            }
        }

        private void PostToUi(Action action)
        {
            try
            {
                var ctx = _syncContext;
                if (ctx != null)
                {
                    ctx.Post(_ => SafeInvoke(action), null);
                }
                else
                {
                    // Sin contexto UI: ejecutar directo (modo consola o pre-Application.Run)
                    SafeInvoke(action);
                }
            }
            catch
            {
                // prevenir excepciones en context posting
            }
        }

        private void SafeInvoke(Action action)
        {
            try { action(); } catch { /* el subscriber debe manejar sus errores */ }
        }
        #endregion

        #region Listados para UI
        public static string[] GetPorts()
        {
            try { return SerialPort.GetPortNames(); }
            catch { return new string[0]; }
        }

        public static int[] GetBaudRates() => new[] { 9600, 19200, 38400, 57600, 115200, 230400 };
        #endregion

        #region Configuración persistente
        /// <summary>
        /// Configura parámetros. No permite cambio si ya hay conexión abierta.
        /// Guarda automáticamente en __Settings.settings__.
        /// </summary>
        public void Configure(string portName, int baudRate, bool autoConnect)
        {
            if (IsConnected)
            {
                SetError("No se puede cambiar la configuración con el dispositivo conectado.");
                return;
            }

            PortName = portName;
            BaudRate = baudRate;
            AutoConnect = autoConnect;
            SaveSettings();
        }

        private void LoadSettings()
        {
            try
            {
                var s = Settings.Default;
                var p = s.Properties;
                if (p["ComPortName"] != null) PortName = s.ComPortName;
                if (p["Baudios"] != null && s.Baudios > 0) BaudRate = s.Baudios;
                if (p["AutoConnect"] != null) AutoConnect = s.AutoConnect;
            }
            catch (Exception ex) { SetError("Error cargando settings: " + ex.Message); }
        }


        private void SaveSettings()
        {
            try
            {
                var settings = Settings.Default;
                var props = settings.Properties;
                if (props["ComPortName"] != null) settings.ComPortName = PortName;
                if (props["Baudios"] != null) settings.Baudios = BaudRate;
                if (props["AutoConnect"] != null) settings.AutoConnect = AutoConnect;
                settings.Save();
            }
            catch (Exception ex)
            {
                SetError("Error guardando settings: " + ex.Message);
            }
        }
        #endregion

        #region Conexión / Reconexión
        public bool TryAutoConnect()
        {
            if (!AutoConnect) return false;
            if (!string.IsNullOrWhiteSpace(PortName))
            {
                if (Connect()) return true;
            }

            // Puerto no disponible todavía → esperar en background
            _ = AttemptReconnectAsync();
            return false;
        }

        public bool Connect()
        {
            if (string.IsNullOrWhiteSpace(PortName))
            {
                SetError("Puerto COM no configurado. Contacte al soporte técnico.");
                return false;
            }

            bool success = false;
            bool shouldReconnect = false;

            lock (_lock)
            {
                if (_port != null && _port.IsOpen) return true;

                try
                {
                    _port = new SerialPort(PortName, BaudRate)
                    {
                        DataBits = 8,
                        Parity = Parity.None,
                        StopBits = StopBits.One,
                        Handshake = Handshake.None,
                        Encoding = Encoding.UTF8,
                        NewLine = "\n",
                        ReadTimeout = 500,
                        WriteTimeout = 500,
                        DtrEnable = false,
                        RtsEnable = false,
                        ReceivedBytesThreshold = 1
                    };

                    _port.DataReceived += OnDataReceived;
                    _port.ErrorReceived += OnErrorReceived;
                    _port.Open();
                    _port.DiscardInBuffer();
                    _port.DiscardOutBuffer();

                    success = true;
                }
                catch
                {
                    CleanupPort();
                    shouldReconnect = AutoConnect;
                }
            }

            if (success)
            {
                _reconnectCts?.Cancel();
                Interlocked.Exchange(ref _reconnecting, 0);
                PostToUi(() => ConnectionChanged?.Invoke(true));
            }
            else if (shouldReconnect)
            {
                _ = AttemptReconnectAsync();
            }

            return success;
        }

        public void Disconnect()
        {
            _reconnectCts?.Cancel();
            Interlocked.Exchange(ref _reconnecting, 0);

            lock (_lock)
            {
                try { if (_port != null && _port.IsOpen) _port.Close(); }
                catch { }
                finally { CleanupPort(); }
            }

            PostToUi(() => ConnectionChanged?.Invoke(false));
        }

        private void CleanupPort()
        {
            // Siempre llamar dentro de lock(_portLock)
            try
            {
                if (_port != null)
                {
                    try { _port.DataReceived -= OnDataReceived; } catch { }
                    try { _port.ErrorReceived -= OnErrorReceived; } catch { }
                    try { _port.Dispose(); } catch { }
                }
            }
            finally
            {
                _port = null;
                lock (_rxBuffer) { _rxBuffer.Clear(); }
            }
        }


        // ── Reconexión segura ────────────────────────────────────────────────
        /// <summary>
        /// Reconexión con backoff exponencial.
        /// Interlocked garantiza que solo UNA tarea de reconexión exista a la vez.
        /// </summary>
        private async Task AttemptReconnectAsync()
        {
            // Solo 1 tarea de reconexión a la vez
            if (Interlocked.CompareExchange(ref _reconnecting, 1, 0) != 0) return;

            _reconnectCts?.Dispose();
            _reconnectCts = new CancellationTokenSource();
            var ct = _reconnectCts.Token;

            try
            {
                int attempt = 0;
                while (!ct.IsCancellationRequested)
                {
                    attempt++;

                    // Backoff exponencial: 1s, 2s, 4s, 8s, 16s, 30s (techo)
                    double delaySec = Math.Min(
                        ReconnectBaseDelay.TotalSeconds * Math.Pow(2, Math.Min(attempt - 1, 5)),
                        30.0);

                    try { await Task.Delay(TimeSpan.FromSeconds(delaySec), ct).ConfigureAwait(false); }
                    catch (TaskCanceledException) { break; }

                    if (ct.IsCancellationRequested || string.IsNullOrWhiteSpace(PortName)) continue;

                    bool connected = false;
                    lock (_lock)
                    {
                        if (_port != null && _port.IsOpen)
                        {
                            connected = true;
                        }
                        else
                        {
                            try
                            {
                                _port = new SerialPort(PortName, BaudRate)
                                {
                                    DataBits = 8,
                                    Parity = Parity.None,
                                    StopBits = StopBits.One,
                                    Handshake = Handshake.None,
                                    Encoding = Encoding.UTF8,
                                    NewLine = "\n",
                                    ReadTimeout = 500,
                                    WriteTimeout = 500,
                                    DtrEnable = false,
                                    RtsEnable = false,
                                    ReceivedBytesThreshold = 1
                                };
                                _port.DataReceived += OnDataReceived;
                                _port.ErrorReceived += OnErrorReceived;
                                _port.Open();
                                _port.DiscardInBuffer();
                                _port.DiscardOutBuffer();
                                connected = true;
                            }
                            catch { CleanupPort(); }
                        }
                    }

                    if (connected)
                    {
                        Interlocked.Exchange(ref _reconnecting, 0);
                        PostToUi(() => ConnectionChanged?.Invoke(true));
                        return;
                    }
                }
            }
            finally
            {
                Interlocked.Exchange(ref _reconnecting, 0);
            }
        }
        #endregion

        #region Watchdog 24/7

        private void StartWatchdog()
        {
            _watchdogCts = new CancellationTokenSource();
            Task.Run(() => WatchdogLoopAsync(_watchdogCts.Token));
        }

        private async Task WatchdogLoopAsync(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    await Task.Delay(WatchdogInterval, ct).ConfigureAwait(false);

                    if (!IsConnected)
                    {
                        // Puerto cerrado → lanzar reconexión si no hay una activa
                        if (AutoConnect &&
                            Interlocked.CompareExchange(ref _reconnecting, 0, 0) == 0)
                            _ = AttemptReconnectAsync();
                        continue;
                    }

                    // Verificar que el COM sigue existiendo en Windows
                    bool portExists = SerialPort.GetPortNames()
                        .Any(p => string.Equals(p, PortName, StringComparison.OrdinalIgnoreCase));

                    if (!portExists)
                    {
                        // Cable desconectado → limpiar y reconectar
                        lock (_lock) { CleanupPort(); }
                        PostToUi(() => ConnectionChanged?.Invoke(false));
                        if (AutoConnect) _ = AttemptReconnectAsync();
                    }
                }
            }
            catch (TaskCanceledException) { }
            catch (Exception ex) { SetError("Error en watchdog: " + ex.Message); }
        }


        #endregion

        #region WMI — reconexión inmediata al insertar el USB
        // ── WMI — reconexión inmediata al insertar el USB ────────────────────
        private void StartUsbWatcher()
        {
            try
            {
                _usbArrivalWatcher = new ManagementEventWatcher(
                    new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2"));
                _usbArrivalWatcher.EventArrived += OnUsbDeviceArrived;
                _usbArrivalWatcher.Start();
            }
            catch { }
        }

        private void OnUsbDeviceArrived(object sender, EventArrivedEventArgs e)
        {
            // Dispositivo USB conectado → intentar reconectar si no estamos conectados
            if (!IsConnected && AutoConnect)
            {
                // 1.5s para que Windows asigne el puerto COM
                Task.Delay(1500).ContinueWith(_ =>
                {
                    if (!IsConnected && !string.IsNullOrWhiteSpace(PortName))
                        Connect();
                });
            }
        }

        #endregion

        #region Envío
        public bool Send(string text, bool appendNewLine = true)
        {
            if (string.IsNullOrEmpty(text)) return true;
            lock (_lock)
            {
                if (!IsConnected) { SetError("Sin conexión con el ESP32."); return false; }
                try
                {
                    if (appendNewLine) _port.WriteLine(text);
                    else _port.Write(text);
                    return true;
                }
                catch (Exception ex) { SetError("Error enviando datos: " + ex.Message); return false; }
            }
        }

        public bool SendBytes(byte[] buffer)
        {
            if (buffer == null || buffer.Length == 0) return true;
            lock (_lock)
            {
                if (!IsConnected) { SetError("Sin conexión con el ESP32."); return false; }
                try { _port.Write(buffer, 0, buffer.Length); return true; }
                catch (Exception ex) { SetError("Error enviando bytes: " + ex.Message); return false; }
            }
        }
        #endregion

        #region Recepción — estilo Hercules (leer todo, ensamblar líneas por \r o \n)
        private void OnErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            SetError("Serial error: " + e.EventType);
        }

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                SerialPort sp;
                lock (_lock)
                {
                    sp = _port;
                    if (sp == null || !sp.IsOpen) return;
                }

                string incoming;
                try { incoming = sp.ReadExisting(); }
                catch (IOException ioEx)
                {
                    SetError("IO error recepción: " + ioEx.Message);
                    HandleUnexpectedDisconnect();
                    return;
                }
                catch (InvalidOperationException) { return; }

                if (string.IsNullOrEmpty(incoming)) return;

                lock (_rxBuffer)
                {
                    if (_rxBuffer.Length + incoming.Length > MAX_RX_BUFFER)
                    {
                        int remove = (_rxBuffer.Length + incoming.Length) - MAX_RX_BUFFER;
                        if (remove > _rxBuffer.Length) remove = _rxBuffer.Length;
                        _rxBuffer.Remove(0, remove);
                    }

                    _rxBuffer.Append(incoming);

                    string all = _rxBuffer.ToString();
                    int idx;
                    while ((idx = all.IndexOfAny(new[] { '\n', '\r' })) >= 0)
                    {
                        string line = all.Substring(0, idx);
                        int skip = idx;
                        while (skip < all.Length && (all[skip] == '\r' || all[skip] == '\n')) skip++;
                        all = all.Substring(skip);

                        line = line.Trim();
                        if (string.IsNullOrEmpty(line)) continue;

                        if (!string.IsNullOrEmpty(MessagePrefix) &&
                            !line.StartsWith(MessagePrefix, StringComparison.Ordinal)) continue;
                        if (MessageRegex != null && !MessageRegex.IsMatch(line)) continue;

                        _lineQueue.Enqueue(line);
                    }

                    _rxBuffer.Clear();
                    if (!string.IsNullOrEmpty(all)) _rxBuffer.Append(all);
                }
            }
            catch (IOException ioEx)
            {
                SetError("IO error: " + ioEx.Message);
                HandleUnexpectedDisconnect();
            }
            catch (ObjectDisposedException) { }
            catch (Exception ex) { SetError("Error procesando datos: " + ex.Message); }
        }

        private void HandleUnexpectedDisconnect()
        {
            lock (_lock) { CleanupPort(); }
            PostToUi(() => ConnectionChanged?.Invoke(false));
            if (AutoConnect) _ = AttemptReconnectAsync();
        }
        #endregion

        #region Errores
        private void SetError(string message)
        {
            LastError = message;
            PostToUi(() => ErrorOccurred?.Invoke(message));
        }
        #endregion

        #region Consumer Loop — procesa líneas encoladas y publica eventos
        private async Task ConsumerLoopAsync(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    if (_lineQueue.TryDequeue(out var line))
                    {
                        // Voltaje: ESP32 envía "VAC=35.5"
                        if (line.StartsWith("VAC=", StringComparison.OrdinalIgnoreCase))
                        {
                            var payload = line.Substring(4).Trim().Replace(',', '.');
                            if (float.TryParse(payload, NumberStyles.Float,
                                               CultureInfo.InvariantCulture, out var v))
                            {
                                // 35.5 → 36 ✅   35.4 → 35 ✅
                                int voltaje = (int)Math.Round(v, MidpointRounding.AwayFromZero);
                                var ts = DateTime.Now;
                                PostToUi(() =>
                                {
                                    VoltageReceived?.Invoke(voltaje, ts);
                                    DataReceived?.Invoke($"VAC={voltaje}");
                                });
                            }
                            continue;
                        }

                        // Cualquier otra línea del ESP32
                        PostToUi(() => DataReceived?.Invoke(line));
                    }
                    else
                    {
                        await Task.Delay(5, ct).ConfigureAwait(false);
                    }
                }
            }
            catch (TaskCanceledException) { }
            catch (Exception ex) { SetError("Error en consumer loop: " + ex.Message); }
        }


        #endregion

        #region IDisposable
        // ── IDisposable ──────────────────────────────────────────────────────
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            // Cancelar watchdog
            try
            {
                if (_watchdogCts != null && !_watchdogCts.IsCancellationRequested)
                    _watchdogCts.Cancel();
            }
            catch { }
            finally
            {
                try { _watchdogCts?.Dispose(); } catch { }
                _watchdogCts = null;
            }

            // Cancelar consumer
            try
            {
                if (_consumerCts != null && !_consumerCts.IsCancellationRequested)
                    _consumerCts.Cancel();
            }
            catch { }

            try { _consumerTask?.Wait(300); }
            catch { }

            finally
            {
                try { _consumerCts?.Dispose(); } catch { }
                _consumerCts = null;
            }

            // Cancelar reconexión
            try
            {
                if (_reconnectCts != null && !_reconnectCts.IsCancellationRequested)
                    _reconnectCts.Cancel();
            }
            catch { }
            finally
            {
                try { _reconnectCts?.Dispose(); } catch { }
                _reconnectCts = null;
            }

            // Detener WMI watcher
            try { _usbArrivalWatcher?.Stop(); } catch { }
            try { _usbArrivalWatcher?.Dispose(); } catch { }
            _usbArrivalWatcher = null;

            Disconnect();
            GC.SuppressFinalize(this);
        }
        #endregion

        //FIN - END
    }
}

