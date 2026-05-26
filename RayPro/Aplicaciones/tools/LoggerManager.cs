using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayPro.Aplicaciones.tools
{
    /// <summary>
    /// Gestor de logs thread-safe para RayPro.
    /// Guarda logs en: %APPDATA%\RayPro\Logs\
    /// </summary>
    public static class LoggerManager
    {
        private static readonly object _lockFile = new object();
        private static string _logDirectory;

        static LoggerManager()
        {
            try
            {
                _logDirectory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "RayPro", "Logs");

                if (!Directory.Exists(_logDirectory))
                {
                    Directory.CreateDirectory(_logDirectory);
                }
            }
            catch
            {
                _logDirectory = null;
            }
        }

        /// <summary>
        /// Escribe un log de voltaje en el archivo.
        /// Archivo: voltage_YYYY-MM-DD.log
        /// </summary>
        public static void LogVoltage(string message)
        {
            WriteLog("voltage", message);
        }

        /// <summary>
        /// Escribe un log de error en el archivo.
        /// Archivo: errors_YYYY-MM-DD.log
        /// </summary>
        public static void LogError(string message, Exception ex = null)
        {
            string fullMessage = message;
            if (ex != null)
            {
                fullMessage += $" | Exception: {ex.GetType().Name}: {ex.Message}";
                if (ex.InnerException != null)
                    fullMessage += $" | Inner: {ex.InnerException.Message}";
            }
            WriteLog("errors", fullMessage);
        }

        /// <summary>
        /// Escribe un log de conexión en el archivo.
        /// Archivo: connection_YYYY-MM-DD.log
        /// </summary>
        public static void LogConnection(string message)
        {
            WriteLog("connection", message);
        }

        /// <summary>
        /// Escribe un log general en el archivo.
        /// Archivo: general_YYYY-MM-DD.log
        /// </summary>
        public static void LogInfo(string message)
        {
            WriteLog("general", message);
        }

        /// <summary>
        /// Método privado que escribe en el archivo especificado.
        /// </summary>
        private static void WriteLog(string logType, string message)
        {
            if (_logDirectory == null) return;

            try
            {
                lock (_lockFile)
                {
                    string fileName = $"{logType}_{DateTime.Now:yyyy-MM-dd}.log";
                    string filePath = Path.Combine(_logDirectory, fileName);

                    string logLine = $"[{DateTime.Now:HH:mm:ss.fff}] {message}";

                    File.AppendAllText(filePath, logLine + Environment.NewLine, Encoding.UTF8);
                }
            }
            catch
            {
                // Silenciar errores de log para no afectar la aplicación
            }
        }

        /// <summary>
        /// Obtiene la ruta del directorio de logs.
        /// Útil para que el usuario acceda a los logs.
        /// </summary>
        public static string GetLogDirectory()
        {
            return _logDirectory ?? "Logs no disponibles";
        }

        /// <summary>
        /// Limpia logs más antiguos de N días.
        /// Llamar desde MainRayX al inicio o crear un servicio de limpieza.
        /// </summary>
        public static void CleanOldLogs(int daysOld = 30)
        {
            if (_logDirectory == null) return;

            try
            {
                lock (_lockFile)
                {
                    DirectoryInfo di = new DirectoryInfo(_logDirectory);
                    FileInfo[] files = di.GetFiles("*.log");

                    DateTime cutoffDate = DateTime.Now.AddDays(-daysOld);

                    foreach (var file in files)
                    {
                        if (file.LastWriteTime < cutoffDate)
                        {
                            try { file.Delete(); }
                            catch { }
                        }
                    }
                }
            }
            catch
            {
                // Silenciar errores
            }
        }
    }
}
