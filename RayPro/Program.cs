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

            Application.Run(new MainRayX());
            //Application.Run(new Welcome());

            // Al salir del software
            AppSession.Usb?.Dispose();

        }
    }
}
