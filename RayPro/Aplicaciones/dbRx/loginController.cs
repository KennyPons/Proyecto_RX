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



        //-------------------------------------------------------------------------------------------/
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


        ///------------------------------------------------------------------------------------------------------------/
        /* public bool updateUser(string nombreUsuario, string nuevaContraseña)
         {
             bool put = false;

             try
             {
                 conn.openDB();

                 string consulta = "UPDATE Usuarios SET userName = @NuevoNombreUsuario, password = @NuevaContraseña WHERE idUser = @ID";
                 //string consulta = "UPDATE Usuarios SET password = @NuevaContraseña WHERE userName = @NombreUsuario";

                 using (OleDbCommand cmd = new OleDbCommand(consulta, conn.GetConnection()))
                 {
                     cmd.Parameters.AddWithValue("@NuevaContraseña", nuevaContraseña);
                     cmd.Parameters.AddWithValue("@NombreUsuario", nombreUsuario);
                     cmd.Parameters.AddWithValue("@ID", 2);

                     int filasPut= cmd.ExecuteNonQuery(); // Número de filas afectadas por la consulta

                     put = filasPut > 0;
                 }
             }
             catch (Exception ex)
             {
                 Console.WriteLine("Error al modificar usuario: " + ex.Message);
             }
             finally
             {
                 conn.closeDB();
             }

             return put;
         }*/

        public bool createNewUser(string nuevoNombreUsuario, string nuevaContraseña)
        {
            bool creado = false;

            try
            {
                conn.openDB();

                string consulta = "INSERT INTO Usuarios (userName, password) VALUES (@NuevoNombreUsuario, @NuevaContraseña)";

                using (OleDbCommand cmd = new OleDbCommand(consulta, conn.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@NuevoNombreUsuario", nuevoNombreUsuario);
                    cmd.Parameters.AddWithValue("@NuevaContraseña", nuevaContraseña);

                    int filasAfectadas = cmd.ExecuteNonQuery(); // Número de filas afectadas por la consulta

                    creado = filasAfectadas > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al crear nuevo usuario: " + ex.Message);
            }
            finally
            {
                conn.closeDB();
            }

            return creado;
        }

    }



}
