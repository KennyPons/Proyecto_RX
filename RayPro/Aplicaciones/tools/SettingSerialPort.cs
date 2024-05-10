using System;
using System.Collections.Generic;
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
        public SettingSerialPort() {
            sPuerto = new SerialPort();
        }


        public void bootSerialPort()
        {
            try
            {
                sPuerto.PortName = configuraciones.Settings.Default.Puerto;
                sPuerto.BaudRate = configuraciones.Settings.Default.Baudios;
                sPuerto.DataBits = 8;
                sPuerto.Parity = Parity.None;
                sPuerto.StopBits = StopBits.One;
                sPuerto.Handshake = Handshake.None;

                sPuerto.WriteTimeout = 500;

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
