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
        private BDExcell handerExcell;
        public SettingSerialPort() {
            sPuerto = new SerialPort();
            receivedData = string.Empty;
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DbSerial.xlsx");
            handerExcell = new BDExcell(path);
        }


        public void bootSerialPort()
        {
            var dataExcell = handerExcell.GetDataSerialExcell(4);
            try
            {
                sPuerto.PortName = dataExcell.com;
                sPuerto.BaudRate = dataExcell.baudRate;
                sPuerto.DataBits = 8;
                sPuerto.Parity = Parity.None;
                sPuerto.StopBits = StopBits.One;
                sPuerto.Handshake = Handshake.None;


                sPuerto.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
                sPuerto.WriteTimeout = 4900; // 5 segundos para escritura
                sPuerto.ReadTimeout = 500;  // 5 segundos para lectura

                sPuerto.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fallo en:\n", ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void EnviarDatosASerial(string datos)
        {
            try
            {
                // Verifica si el puerto serial está abierto antes de enviar datos
                if (sPuerto.IsOpen)
                {
                    // Envía los datos al puerto serial
                    sPuerto.Write(datos);
                }
                else
                {
                    MessageBox.Show("El puerto serial no está abierto.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al enviar datos por el puerto serial: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (sPuerto.IsOpen)
                {
                    receivedData = sPuerto.ReadLine().Trim(); // Lee la línea de datos del puerto serial y la recorta
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al recibir datos: " + ex.Message);
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
                    sPuerto.Close(); // Cierra el puerto serial
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cerrar el puerto serial: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


    }
}
