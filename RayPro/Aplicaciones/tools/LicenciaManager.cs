using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RayPro.Aplicaciones.tools
{
    public static class LicenciaManager
    {
        private static readonly string secretKey = "RAYPRO_MASTER_KEY_2026";

        private static string CarpetaLicencia()
        {
            return @"C:\Users\" + Environment.UserName + @"\.syscachemedi\";
        }

        private static string RutaLicencia()
        {
            return Path.Combine(CarpetaLicencia(), "license.lic");
        }

        public static bool LicenciaExiste()
        {
            return File.Exists(RutaLicencia());
        }

        public static bool ValidarAlIniciar()
        {
            if (!LicenciaExiste())
                return false;

            LicenciaDatos datos = LeerLicencia();

            if (!ValidarFirma(datos))
                return false;

            if (datos.Tipo == "Mensual" &&
                DateTime.Today > datos.FechaFin)
                return false;

            return true;
        }

        public static bool ValidarLicenciaPermanente(
            string licenciaIngresada,
            string passwordIngresado,
            string strPermanente)
        {
            LicenciaDatos datos = LeerLicencia();

            if (datos.Tipo != "Permanente")
                return false;

            if (licenciaIngresada != datos.LicenseCode)
                return false;

            if (Hash(passwordIngresado) != datos.PasswordHash)
                return false;

            datos.Tipo = strPermanente;

            Guardar(datos);

            return true;
        }

        public static bool RenovarMensual(
            DateTime nuevaInicio,
            DateTime nuevaFin,
            string securityIngresado)
        {
            LicenciaDatos datos = LeerLicencia();

            if (datos.Tipo != "Mensual")
            {
                return false;
            }
               

            if (securityIngresado != datos.SecurityCode)
            { 
            return false; 
            }

            if (nuevaFin <= nuevaInicio)
                return false;

            datos.FechaInicio = nuevaInicio;
            datos.FechaFin = nuevaFin;

            Guardar(datos);

            return true;
        }

        private static LicenciaDatos LeerLicencia()
        {
            string contenidoCodificado = File.ReadAllText(RutaLicencia());

            // 🔓 Decodificamos
            string contenido = Encoding.UTF8.GetString(
                Convert.FromBase64String(contenidoCodificado));

            string[] parts = contenido.Split('|');

            LicenciaDatos datos = new LicenciaDatos();
            datos.LicenseCode = parts[0];
            datos.SecurityCode = parts[1];
            datos.PasswordHash = parts[2];
            datos.FechaInicio = DateTime.Parse(parts[3]);
            datos.FechaFin = DateTime.Parse(parts[4]);
            datos.Tipo = parts[5];
            datos.Firma = parts[6];

            return datos;
        }
        private static void Guardar(LicenciaDatos datos)
        {
            string data =
                datos.LicenseCode + "|" +
                datos.SecurityCode + "|" +
                datos.PasswordHash + "|" +
                datos.FechaInicio.ToString("yyyy-MM-dd") + "|" +
                datos.FechaFin.ToString("yyyy-MM-dd") + "|" +
                datos.Tipo;

            string firma = GenerarFirma(data);

            string contenidoFinal = data + "|" + firma;

            string oculto = Convert.ToBase64String(
                Encoding.UTF8.GetBytes(contenidoFinal));

            File.WriteAllText(RutaLicencia(), oculto);
        }

        private static bool ValidarFirma(LicenciaDatos datos)
        {
            string data =
                datos.LicenseCode + "|" +
                datos.SecurityCode + "|" +
                datos.PasswordHash + "|" +
                datos.FechaInicio.ToString("yyyy-MM-dd") + "|" +
                datos.FechaFin.ToString("yyyy-MM-dd") + "|" +
                datos.Tipo;

            return GenerarFirma(data) == datos.Firma;
        }

        private static string GenerarFirma(string data)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
            {
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                return Convert.ToBase64String(hash);
            }
        }

        private static string Hash(string input)
        {
            using (var sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
                return Convert.ToBase64String(hash);
            }
        }

        public static bool CampoVacio(TextBox txt, string mensaje)
        {
            if (string.IsNullOrWhiteSpace(txt.Text))
            {
                MessageBox.Show(mensaje,
                                "Campo obligatorio",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                txt.Focus();
                return true;
            }

            return false;
        }
    }
}
