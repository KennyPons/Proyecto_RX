using RayPro.Persistencia.db;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt");


        private SerialPort sPuerto;
        private BackgroundWorker serialWorker;

        public event EventHandler<string> DataReceived;

        public SettingSerialPort(string portName, int baudRate)
        {
            sPuerto = new SerialPort(portName, baudRate);
            sPuerto.DataReceived += SerialPort_DataReceived;

            serialWorker = new BackgroundWorker();
            serialWorker.DoWork += SerialWorker_DoWork;
            serialWorker.RunWorkerAsync();

            try
            {
                sPuerto.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al abrir el puerto serial: {ex.Message}");
            }
        }



        //PRIVATE SERIAL//
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string data = sPuerto.ReadLine();
            DataReceived?.Invoke(this, data);
        }

        private void SerialWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                if (sPuerto.IsOpen)
                {
                    try
                    {
                        // Simplemente se mantiene escuchando en segundo plano
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error en el worker serial: {ex.Message}");
                    }
                }
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

        /*private void ConfigureSerialPort()
        {
            sPuerto.DataBits = 8;
            sPuerto.Parity = Parity.None;
            sPuerto.StopBits = StopBits.One;
            sPuerto.Handshake = Handshake.None;
            
            sPuerto.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
        }*/



        public void senDataSerial(string datos)
        {
            try
            {
                if (sPuerto.IsOpen)
                {
                    sPuerto.WriteLine(datos);
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



        public void CerrarSerialPort()
        {
            try
            {
                if (sPuerto != null && sPuerto.IsOpen)
                {
                    sPuerto.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cerrar el puerto serial: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////
    }
}

