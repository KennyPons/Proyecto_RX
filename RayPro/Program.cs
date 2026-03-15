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


            var loginForm = new Login();

            AppSession.Usb.CaptureSyncContext();

            try
            {
                Application.Run(loginForm);
            }
            finally
            {
                AppSession.Usb?.Dispose(); // ← siempre se ejecuta, con o sin excepción
            }
        }
    }
}
