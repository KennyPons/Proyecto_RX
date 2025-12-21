using RayPro.configuraciones;
using System;
using System.Collections.Generic;
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
        private readonly SynchronizationContext _syncContext;
        private bool _disposed;
        private CancellationTokenSource _reconnectCts;

        // Eventos (se publican en hilo UI si es posible)
        public event Action<string> DataReceived;
        public event Action<bool> ConnectionChanged;
        public event Action<string> ErrorOccurred;

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
            _syncContext = SynchronizationContext.Current;
            LoadSettings();
        }

        #region UI helpers
        private void PostToUi(Action action)
        {
            try
            {
                if (_syncContext != null) _syncContext.Post(_ => SafeInvoke(action), null);
                else SafeInvoke(action);
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
                var settings = Settings.Settings.Default;
                var props = settings.Properties;
                if (props["ComPort"] != null) PortName = settings.ComPort;
                if (props["BaudRate"] != null && settings.BaudRate > 0) BaudRate = settings.BaudRate;
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
                var settings = Properties.Settings.Default;
                var props = settings.Properties;
                if (props["ComPort"] != null) settings.ComPort = PortName;
                if (props["BaudRate"] != null) settings.BaudRate = BaudRate;
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
                        WriteTimeout = 500
                    };

                    _port.DataReceived += OnDataReceived;
                    _port.Open();

                    PostToUi(() => ConnectionChanged?.Invoke(true));
                    // cancelar cualquier ciclo de reconexión en curso
                    _reconnectCts?.Cancel();
                    return true;
                }
                catch (Exception ex)
                {
                    CleanupPort();
                    SetError("Error al conectar USB CDC: " + ex.Message);
                    // iniciar reintento si corresponde
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
                        try { _port.Close(); } catch { /* ignore */ }
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
            // evita múltiples ciclos paralelos
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

                    if (Connect()) return; // éxito
                }
                catch (TaskCanceledException) { break; }
                catch { /* ignorar y continuar */ }
            }

            // al final, notificar fallo si sigue desconectado
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

        #region Recepción (ensamblado de líneas) y filtrado
        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string incoming;
                lock (_lock)
                {
                    if (_port == null || !_port.IsOpen) return;
                    incoming = _port.ReadExisting();
                }

                if (string.IsNullOrEmpty(incoming)) return;

                lock (_rxBuffer)
                {
                    _rxBuffer.Append(incoming);
                    var all = _rxBuffer.ToString();
                    var parts = all.Split(new[] { '\n' }, StringSplitOptions.None);
                    bool endsWithNewline = all.EndsWith("\n");
                    int completeCount = endsWithNewline ? parts.Length : parts.Length - 1;

                    for (int i = 0; i < completeCount; i++)
                    {
                        var line = parts[i].Trim('\r', '\n', '\0').Trim();
                        if (string.IsNullOrEmpty(line)) continue;

                        if (!string.IsNullOrEmpty(MessagePrefix) && !line.StartsWith(MessagePrefix, StringComparison.Ordinal)) continue;
                        if (MessageRegex != null && !MessageRegex.IsMatch(line)) continue;

                        PostToUi(() => DataReceived?.Invoke(line));
                    }

                    _rxBuffer.Clear();
                    if (!endsWithNewline)
                    {
                        _rxBuffer.Append(parts[parts.Length - 1]);
                    }
                }
            }
            catch (IOException ioEx)
            {
                // puerto desconectado físicamente
                SetError("IO error en recepción: " + ioEx.Message);
                HandleUnexpectedDisconnect();
            }
            catch (ObjectDisposedException)
            {
                // ignore during disposal
            }
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

        #region Errores y util
        private void SetError(string message)
        {
            LastError = message;
            PostToUi(() => ErrorOccurred?.Invoke(message));
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            try
            {
                _reconnectCts?.Cancel();
            }
            catch { }
            Disconnect();
            GC.SuppressFinalize(this);
        }
        #endregion

        //FIN - END
    }
}
