using RayPro.Persistencia.db;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayPro.Persistencia
{

    
    internal class loginController
    {

        private conexionDB conn;
        public loginController()
        {
            conn = new conexionDB();
        }

        public bool AutenticarUsuario(string nombreUsuario, string contrasenia)
        {
            bool resAuth = false;

            try
            {
                conn.openDB();
                string consulta = "SELECT Count(*) FROM Usuarios WHERE userName = @userName AND password = @password";
                using (OleDbCommand cmd = new OleDbCommand(consulta, conn.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@NombreUsuario", nombreUsuario);
                    cmd.Parameters.AddWithValue("@Contraseña", contrasenia);

                    int count = (int)cmd.ExecuteScalar();  // Obtiene el número de coincidencias

                    resAuth = count > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al autenticar: " + ex.Message);
            }
            finally
            {
                conn.closeDB();
            }

            return resAuth;

        }



        public bool AutenticarAdmin(string nombreUsuario, string contrasenia)
        {
            bool resAuth = false;

            try
            {
                conn.openDB();
                string consulta = "SELECT Count(*) FROM master WHERE userM = @userName OR passwordM = @password";
                using (OleDbCommand cmd = new OleDbCommand(consulta, conn.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@NombreUsuario", nombreUsuario);
                    cmd.Parameters.AddWithValue("@Contraseña", contrasenia);

                    int count = (int)cmd.ExecuteScalar();  // Obtiene el número de coincidencias

                    resAuth = count > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al autenticar: " + ex.Message);
            }
            finally
            {
                conn.closeDB();
            }

            return resAuth;

        }


    }



}
