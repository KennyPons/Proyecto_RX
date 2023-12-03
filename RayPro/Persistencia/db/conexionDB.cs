using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.OleDb;


namespace RayPro.Persistencia.db
{
    
    internal class conexionDB
    {
        private string strConeccion = configuraciones.Settings.Default.DB_USERConnection;
        private OleDbConnection conexion;


        public conexionDB()
        {
            // Inicializar la conexión en el constructor si es necesario
            conexion = new OleDbConnection(strConeccion);
        }

        // Método para abrir la conexión
        public void openDB()
        {
            try
            {
                if (conexion.State != System.Data.ConnectionState.Open)
                {
                    conexion.Open();
                    Console.WriteLine("Conexión abierta exitosamente.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al abrir la conexión: " + ex.Message);
            }
        }

        // Método para cerrar la conexión
        public void closeDB()
        {
            try
            {
                if (conexion.State == System.Data.ConnectionState.Open)
                {
                    conexion.Close();
                    Console.WriteLine("Conexión cerrada exitosamente.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al cerrar la conexión: " + ex.Message);
            }
        }


    }



}
