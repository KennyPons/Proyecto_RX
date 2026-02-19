using RayPro.configuraciones;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
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
        private CancellationTokenSource _reconnectCts;

        // Eventos (se publican en hilo UI si es posible)
        public event Action<string> DataReceived;
        public event Action<bool> ConnectionChanged;
        public event Action<string> ErrorOccurred;

        // Dentro de la clase UsbCdcManager: nuevos campos y evento
        private readonly ConcurrentQueue<string> _lineQueue = new ConcurrentQueue<string>();
        private CancellationTokenSource _consumerCts;
        private Task _consumerTask;
        private const int MAX_RX_BUFFER = 64 * 1024; // 64 KB para el StringBuilder
        public event Action<float, DateTime> VoltageReceived; // evento específico para voltaje (V)


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
        public int ReconnectMaxAttempts { get; set; } = 5;
        public TimeSpan ReconnectBaseDelay { get; set; } = TimeSpan.FromSeconds(1);

        public UsbCdcManager()
        {
            // Capturar contexto UI si existe (puede ser null si se crea antes de Application.Run)
            _syncContext = SynchronizationContext.Current;
            LoadSettings();

            // Iniciar consumer que procesará las líneas recibidas
            _consumerCts = new CancellationTokenSource();
            _consumerTask = Task.Run(() => ConsumerLoopAsync(_consumerCts.Token));
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
                var settings = Settings.Default;
                var props = settings.Properties;
                if (props["ComPortName"] != null) PortName = settings.ComPortName;
                if (props["Baudios"] != null && settings.Baudios > 0) BaudRate = settings.Baudios;
                if (props["AutoConnect"] != null) AutoConnect = settings.AutoConnect;
            }
            catch (Exception ex)
            {
                SetError("Error cargando settings: " + ex.Message);
            }
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
            if (string.IsNullOrWhiteSpace(PortName)) return false;
            return Connect();
        }

        public bool Connect()
        {
            lock (_lock)
            {
                if (IsConnected) return true;
                if (string.IsNullOrWhiteSpace(PortName))
                {
                    SetError("Puerto COM no configurado.");
                    return false;
                }

                try
                {
                    _port = new SerialPort(PortName, BaudRate)
                    {
                        DataBits = 8,
                        Parity = Parity.None,
                        StopBits = StopBits.One,
                        Handshake = Handshake.None,
                        Encoding = Encoding.ASCII,
                        NewLine = "\n",
                        ReadTimeout = 500,
                        WriteTimeout = 500,
                        DtrEnable = true,
                        RtsEnable = true,
                        ReceivedBytesThreshold = 1
                    };

                    _port.DataReceived += OnDataReceived;
                    _port.ErrorReceived += OnErrorReceived;
                    _port.Open();

                    // Descartar basura que pudiera haber en el buffer del driver
                    _port.DiscardInBuffer();
                    _port.DiscardOutBuffer();

                    PostToUi(() => ConnectionChanged?.Invoke(true));
                    _reconnectCts?.Cancel();
                    return true;
                }
                catch (Exception ex)
                {
                    CleanupPort();
                    SetError("Error al conectar USB CDC: " + ex.Message);
                    if (AutoConnect) _ = AttemptReconnectAsync();
                    return false;
                }
            }
        }

        public void Disconnect()
        {
            lock (_lock)
            {
                _reconnectCts?.Cancel();
                try
                {
                    if (_port != null && _port.IsOpen)
                    {
                        try { _port.Close(); } catch { }
                    }
                }
                finally
                {
                    CleanupPort();
                    PostToUi(() => ConnectionChanged?.Invoke(false));
                }
            }
        }

        private void CleanupPort()
        {
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

        private async Task AttemptReconnectAsync()
        {
            if (_reconnectCts != null && !_reconnectCts.IsCancellationRequested) return;

            _reconnectCts = new CancellationTokenSource();
            var ct = _reconnectCts.Token;

            for (int attempt = 1; attempt <= ReconnectMaxAttempts && !ct.IsCancellationRequested; attempt++)
            {
                try
                {
                    var delay = TimeSpan.FromMilliseconds(ReconnectBaseDelay.TotalMilliseconds * Math.Pow(2, attempt - 1));
                    await Task.Delay(delay, ct).ConfigureAwait(false);
                    if (ct.IsCancellationRequested) break;
                    if (Connect()) return;
                }
                catch (TaskCanceledException) { break; }
                catch { }
            }

            if (!IsConnected) SetError("No fue posible reconectar automáticamente al dispositivo.");
        }
        #endregion

        #region Envío
        public bool Send(string text, bool appendNewLine = true)
        {
            if (string.IsNullOrEmpty(text)) return true;

            lock (_lock)
            {
                if (!IsConnected)
                {
                    SetError("No hay conexión con el STM32.");
                    return false;
                }

                try
                {
                    if (appendNewLine) _port.WriteLine(text);
                    else _port.Write(text);
                    return true;
                }
                catch (Exception ex)
                {
                    SetError("Error enviando datos: " + ex.Message);
                    return false;
                }
            }
        }

        public bool SendBytes(byte[] buffer)
        {
            if (buffer == null || buffer.Length == 0) return true;

            lock (_lock)
            {
                if (!IsConnected)
                {
                    SetError("No hay conexión con el STM32.");
                    return false;
                }

                try
                {
                    _port.Write(buffer, 0, buffer.Length);
                    return true;
                }
                catch (Exception ex)
                {
                    SetError("Error enviando bytes: " + ex.Message);
                    return false;
                }
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
                // Obtener referencia al puerto fuera del lock pesado
                SerialPort sp;
                lock (_lock)
                {
                    sp = _port;
                    if (sp == null || !sp.IsOpen) return;
                }

                // Leer FUERA del lock — ReadExisting() es thread-safe y no bloquea
                // Esto es exactamente lo que hace Hercules internamente
                string incoming;
                try
                {
                    incoming = sp.ReadExisting();
                }
                catch (IOException ioEx)
                {
                    SetError("IO error en recepción: " + ioEx.Message);
                    HandleUnexpectedDisconnect();
                    return;
                }
                catch (InvalidOperationException) { return; }

                if (string.IsNullOrEmpty(incoming)) return;

                lock (_rxBuffer)
                {
                    // Limitar crecimiento del buffer
                    if (_rxBuffer.Length + incoming.Length > MAX_RX_BUFFER)
                    {
                        _rxBuffer.Remove(0, incoming.Length);
                    }

                    _rxBuffer.Append(incoming);

                    // Extraer líneas completas terminadas en \r, \n, o \r\n
                    // El STM32 envía con \r\n (UART_Printf usa "...\r\n"))
                    string all = _rxBuffer.ToString();
                    int idx;
                    while ((idx = all.IndexOfAny(new[] { '\n', '\r' })) >= 0)
                    {
                        string line = all.Substring(0, idx);
                        // Saltar secuencia \r\n completa
                        int skip = idx;
                        while (skip < all.Length && (all[skip] == '\r' || all[skip] == '\n')) skip++;
                        all = all.Substring(skip);

                        line = line.Trim();
                        if (string.IsNullOrEmpty(line)) continue;

                        // Filtrado temprano (si está configurado)
                        if (!string.IsNullOrEmpty(MessagePrefix) && !line.StartsWith(MessagePrefix, StringComparison.Ordinal)) continue;
                        if (MessageRegex != null && !MessageRegex.IsMatch(line)) continue;

                        _lineQueue.Enqueue(line);
                    }

                    // Guardar fragmento parcial (datos sin terminador aún)
                    _rxBuffer.Clear();
                    if (!string.IsNullOrEmpty(all)) _rxBuffer.Append(all);
                }
            }
            catch (IOException ioEx)
            {
                SetError("IO error en recepción: " + ioEx.Message);
                HandleUnexpectedDisconnect();
            }
            catch (ObjectDisposedException) { }
            catch (Exception ex)
            {
                SetError("Error procesando datos recibidos: " + ex.Message);
            }
        }

        private void HandleUnexpectedDisconnect()
        {
            CleanupPort();
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
                        // Procesar VAC=xxx.xx → publicar número entero ajustado
                        if (line.StartsWith("VAC=", StringComparison.OrdinalIgnoreCase))
                        {
                            var payload = line.Substring(4).Trim();
                            payload = payload.Replace(',', '.');

                            if (float.TryParse(payload, NumberStyles.Float, CultureInfo.InvariantCulture, out var v))
                            {
                                int adjusted = (int)Math.Round(v) - VoltageOffset;
                                PostToUi(() => DataReceived?.Invoke(adjusted.ToString()));
                            }
                            // Si no parseó, ignorar (no publicar basura)
                            continue;
                        }

                        // Cualquier otra línea del STM32: publicar tal cual
                        PostToUi(() => DataReceived?.Invoke(line));
                    }
                    else
                    {
                        await Task.Delay(5, ct).ConfigureAwait(false);
                    }
                }
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                SetError("Error en consumer loop: " + ex.Message);
            }
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            try { _consumerCts?.Cancel(); } catch { }
            try { _consumerTask?.Wait(200); } catch { }
            try { _reconnectCts?.Cancel(); } catch { }
            Disconnect();
            GC.SuppressFinalize(this);
        }
        #endregion

        //FIN - END
    }
}
