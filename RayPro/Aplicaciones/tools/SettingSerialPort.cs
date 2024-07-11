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
        //private BackgroundWorker serialWorker;
        private StringBuilder _buffer = new StringBuilder();
        //public event EventHandler<string> DataReceived;
        public event Action<string> DataReceived;

        public SettingSerialPort(string portName, int baudRate)
        {
            sPuerto = new SerialPort(portName, baudRate);
            sPuerto.DataReceived += OnDataReceived;

            //sPuerto.DataReceived += SerialPort_DataReceived;

            /*serialWorker = new BackgroundWorker();
            serialWorker.DoWork += SerialWorker_DoWork;
            serialWorker.RunWorkerAsync();*/

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
        /*private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string data = sPuerto.ReadLine();
            DataReceived?.Invoke(this, data);
        }*/

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    while (sPuerto.BytesToRead > 0)
                    {
                        // Leer los datos recibidos
                        string data = sPuerto.ReadExisting();
                        _buffer.Append(data);

                        // Procesar líneas completas
                        string bufferContent = _buffer.ToString();
                        int newlineIndex;
                        while ((newlineIndex = bufferContent.IndexOf('\n')) >= 0)
                        {
                            string line = bufferContent.Substring(0, newlineIndex).Trim();
                            bufferContent = bufferContent.Substring(newlineIndex + 1);
                            _buffer.Clear();
                            _buffer.Append(bufferContent);

                            // Validar y procesar la línea recibida
                            if (float.TryParse(line, out float parsedValue))
                            {
                                Console.WriteLine("Data received: " + line); // Mensaje de depuración
                                DataReceived?.Invoke(line);
                            }
                            else
                            {
                                Console.WriteLine("Invalid data received: " + line); // Manejo de datos inválidos
                            }
                        }
                    }
                        /*string dataLine = sPuerto.ReadLine();
                        DataReceived?.Invoke(dataLine);*/
                    }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);// Manejo de errores
                }
            });
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

