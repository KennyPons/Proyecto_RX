using RayPro.Aplicaciones;
using RayPro.Aplicaciones.tools;
using RayPro.Vista;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace RayPro
{
    internal static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            AppSession.Usb = new UsbCdcManager();

            var loginForm = new Login();

            // Capturar SynchronizationContext DESPUÉS de crear el primer Form
            // (WinForms lo instala al crear el primer Form con message loop)
            AppSession.Usb.CaptureSyncContext();

            Application.Run(loginForm);

            // Al salir del software
            AppSession.Usb?.Dispose();
        }
    }
}
