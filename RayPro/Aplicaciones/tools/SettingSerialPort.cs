using RayPro.Persistencia.db;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RayPro.Aplicaciones.tools
{
    internal class SettingSerialPort
    {
        private SerialPort sPuerto;
        private string receivedData;
        private readonly string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt");

        public SettingSerialPort()
        {
            sPuerto = new SerialPort();
            receivedData = string.Empty;
            ConfigureSerialPort(); // Configuración inicial del puerto serial
        }

        private void ConfigureSerialPort()
        {
            sPuerto.DataBits = 8;
            sPuerto.Parity = Parity.None;
            sPuerto.StopBits = StopBits.One;
            sPuerto.Handshake = Handshake.None;
            
            sPuerto.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
        }

        public void OpenSerialPort(string portName, int baudRate)
        {
            try
            {
                if (!sPuerto.IsOpen)
                {
                    sPuerto.PortName = portName;
                    sPuerto.BaudRate = baudRate;
                    sPuerto.Open();
                    LogData($"Puerto serial abierto en {portName} con velocidad {baudRate}.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir el puerto serial: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async Task EnviarDatosASerial(string datos)
        {
            try
            {
                if (sPuerto.IsOpen)
                {
                    sPuerto.Write(datos);
                    LogData($"Enviado: {datos}");
                }
                else
                {
                    MessageBox.Show("El puerto serial no está abierto.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                LogData($"Error al enviar datos por el puerto serial: {ex.Message}");
                MessageBox.Show($"Error al enviar datos por el puerto serial: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (sPuerto.IsOpen)
                {
                    string data = sPuerto.ReadExisting().Trim();
                    if (!string.IsNullOrEmpty(data))
                    {
                        receivedData = data;
                        LogData($"Recibido: {receivedData}");
                    }
                }
            }
            catch (Exception ex)
            {
                LogData($"Error al recibir datos: {ex.Message}");
            }
        }

        public string GetDatoRecibido()
        {
            return receivedData;
        }

        public void CerrarSerialPort()
        {
            try
            {
                if (sPuerto.IsOpen)
                {
                    sPuerto.Close();
                    LogData("Puerto serial cerrado.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cerrar el puerto serial: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LogData(string message)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(logFilePath, true))
                {
                    sw.WriteLine($"{DateTime.Now}: {message}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al escribir en el archivo de registro: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

