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
    internal class SerialPortManager
    {
        private SerialPort sPuerto;
        private StringBuilder _buffer = new StringBuilder();
        public event Action<string> DataReceived;

        public SerialPortManager(string portName, int baudRate)
        {
            sPuerto = new SerialPort(portName, baudRate);
            sPuerto.DataReceived += OnDataReceived;

            try
            {
                sPuerto.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al abrir el puerto serial: {ex.Message}");
            }
        }

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    while (sPuerto.BytesToRead > 0)
                    {
                        string data = sPuerto.ReadExisting();
                        _buffer.Append(data);

                        string bufferContent = _buffer.ToString();
                        int newlineIndex;
                        while ((newlineIndex = bufferContent.IndexOf('\n')) >= 0)
                        {
                            string line = bufferContent.Substring(0, newlineIndex).Trim();
                            bufferContent = bufferContent.Substring(newlineIndex + 1);
                            _buffer.Clear();
                            _buffer.Append(bufferContent);

                            if (float.TryParse(line, out float parsedValue))
                            {
                                Console.WriteLine("Data received: " + line);
                                DataReceived?.Invoke(line);
                            }
                            else
                            {
                                Console.WriteLine("Invalid data received: " + line);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al procesar datos del puerto serial: {ex.Message}");
                }
            });
        }

        public void senDataSerial(string datos)
        {
            try
            {
                if (sPuerto.IsOpen)
                {
                    sPuerto.WriteLine(datos);
                }
                else
                {
                    MessageBox.Show("El puerto serial no está abierto.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
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
    }
}

