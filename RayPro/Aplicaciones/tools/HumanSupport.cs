using RayPro.Vista;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RayPro.Aplicaciones.tools
{
    internal class HumanSupport
    {
        
        private DatosRadiologia _datosRadiologia; private ComboBox cboProy, cboEstruct; private Label lblKVp, lblMaS;

        private double mil = Math.Pow(10, 3);

        private int MA = 50;

        //constructor
        public HumanSupport(ComboBox cboProy, ComboBox cboEstruct, Label lblKVp, Label lblMaS) {
            this.cboEstruct = cboEstruct; 
            this.cboProy = cboProy;
            this.lblKVp = lblKVp; 
            this.lblMaS = lblMaS;
            _datosRadiologia = new DatosRadiologia();

        }

       
        private void LimpiarYConfigurarCombo(ComboBox cboBox, string[] itemsRadiologia, string selectedText)
        {
            cboBox.Items.Clear();
            cboBox.Text = selectedText;
            foreach (var ray in itemsRadiologia)
            {
                cboBox.Items.Add(ray);
            }
        }   

        private void mostrarDataRayX(string selectEstructura)
        {
            //COMBO -> EXTRAIDO DEL DICCIONARIO ESTRUCTURA 
            if (_datosRadiologia.Estructuras.ContainsKey(selectEstructura))//Verefica si Existe en el Diccionario True O False
            {
                LimpiarYConfigurarCombo(cboEstruct, _datosRadiologia.Estructuras[selectEstructura], _datosRadiologia.Estructuras[selectEstructura][0]);
                //COMBO -> EXTRAIDO DEL DICCIONARIO PROYECCION 
                if (_datosRadiologia.Proyecciones.ContainsKey(selectEstructura))
                {
                    LimpiarYConfigurarCombo(cboProy, _datosRadiologia.Proyecciones[selectEstructura], _datosRadiologia.Proyecciones[selectEstructura][0]);
                }
            }
        }


        private void mostrarSelectProyeccion(int n_mostrar)
        {
            if (_datosRadiologia.SelectProyeccion.ContainsKey(n_mostrar))
            {
                LimpiarYConfigurarCombo(cboProy, _datosRadiologia.SelectProyeccion[n_mostrar], _datosRadiologia.SelectProyeccion[n_mostrar][0]);
            }

        }

        


        /////////////////METODOS PUBLICOS - AUXILIARES PARA MAIN RX///////////////////////////////////////////////////

        public int initialVoltageInput(int kvNeed)
        {
            
            int kvInicial = kvNeed * Convert.ToInt32(mil), kvFinal = 125 * Convert.ToInt32(mil);

            int result_Entrada_Ini = (250 * kvInicial) / kvFinal;
            
            return result_Entrada_Ini;
            
        }

        /// <summary>
        /// Gestor de Ejecutar Musica
        /// </summary>
        /// 

        public void playSoundRx(string sound)
        {
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string soundFilePath = Path.Combine(appDirectory, "Resources", sound);

            try
            {
                Console.WriteLine("Intentando cargar archivo de sonido desde: " + soundFilePath);

                if (File.Exists(soundFilePath))
                {
                    using (var sonido = new SoundPlayer(soundFilePath))
                    {
                        sonido.Play();
                    }
                }
                else
                {
                    MessageBox.Show("El archivo de sonido no se encontró en la ubicación especificada.");
                }
            }
            catch (Exception ex)
            {
                FrCuadro.Show("Ocurrió un error al intentar reproducir el sonido: " + ex.Message, "Error de Sonido", MessageBoxButtons.YesNo);

            }
        }

        /// <summary>
        /// Gestor de convertir en tiempo la entrada de mAs
        /// </summary>
        public double sendTimeInput(int mAsInput)
        {
            double resultTiempo = (double )mAsInput / MA;

            double redondeoTiempo = Math.Round(resultTiempo, 3, MidpointRounding.AwayFromZero);


            return redondeoTiempo * Convert.ToInt32(mil);

        }


        public void maSmallOrLarge(string foco)
        {
            if (foco.Equals("Small"))
            {
                MA = 50;//MA
            }
            else
            {
                MA = 75;//MA
            }
        }

        public string formatoStrMaS(int MaS)
        {
            return (MaS < 10) ? "0" + MaS.ToString() : MaS.ToString();
        }

        /// <summary>
        /// Mostrar 
        /// </summary>

        private void showKVandMAS(int n_mas, int n_kv)
        {
            lblKVp.Text = "" + n_kv;

            lblMaS.Text = formatoStrMaS(n_mas);
        }

        public void showBodyRayX(int countNow)
        {
            switch (countNow)
            {
                case 0: mostrarDataRayX("Craneo");  showKVandMAS(20, 70); break;
                case 1: mostrarDataRayX("Cuello");  showKVandMAS(18, 68); break;
                case 2: mostrarDataRayX("Brazos");  showKVandMAS(20, 65); break;
                case 3: mostrarDataRayX("Escapula");showKVandMAS(15, 72); break;
                case 4: mostrarDataRayX("Abdomen"); showKVandMAS(35, 75); break;
                case 5: mostrarDataRayX("Pelvis");  showKVandMAS(30, 75); break;
                case 6: mostrarDataRayX("Femur");   showKVandMAS(20, 72); break;

            }
        }

        /// <summary>
        /// SIRVE PARA CAMBIAR LA PROYECCION SEGUN LA ESTRUCTURA SELECCIONADA
        /// </summary>
       
        public void changeShowCboProy(string selectEstructura)
        {
            switch(selectEstructura)
            {
                case "ESCAPÚLA":  mostrarSelectProyeccion(10); showKVandMAS(15, 70); break;
                case "CLAVÍCULA": mostrarSelectProyeccion(20); showKVandMAS(15, 70); break;
                case "TORÁX":     mostrarSelectProyeccion(30); showKVandMAS(4, 110); break;
                case "COSTILLAS": mostrarSelectProyeccion(40); showKVandMAS(32, 70); break;
                case "ESTERNÓN":  mostrarSelectProyeccion(50); showKVandMAS(20, 60); break;
                case "ABDOMEN":   mostrarSelectProyeccion(60); showKVandMAS(35, 75); break;
                case "COLUMNA LUMBAR": mostrarSelectProyeccion(70); showKVandMAS(45, 80); break;
                case "PELVIS":    mostrarSelectProyeccion(80);  showKVandMAS(30, 75); break;
                case "SACRO":     mostrarSelectProyeccion(81);  showKVandMAS(25, 75); break;
                case "CADERA":    mostrarSelectProyeccion(90);  showKVandMAS(20, 70); break;
                case "HOMBRO":    mostrarSelectProyeccion(100); showKVandMAS(20, 65); break;
                //TODO LA PARTE DE BRAZO
                case "CODO":      mostrarSelectProyeccion(101); showKVandMAS(10, 56); break;
                case "HÚMERO":    mostrarSelectProyeccion(102); showKVandMAS(18, 68); break;
                case "ANTEBRAZO": mostrarSelectProyeccion(103); showKVandMAS(10, 75); break;
                case "MUÑECA":    mostrarSelectProyeccion(104); showKVandMAS(3, 54); break;
                case "MANO":      mostrarSelectProyeccion(105); showKVandMAS(5, 50); break;
                case "FALANGES MANO": mostrarSelectProyeccion(106); showKVandMAS(5, 48); break;
                //TODO LA PARTE DE BRAZO
                case "FEMUR":     mostrarSelectProyeccion(110); showKVandMAS(20, 72); break;
                case "RODILLA":   mostrarSelectProyeccion(111); showKVandMAS(10, 65); break;
                case "TIBIA Y PERONÉ": mostrarSelectProyeccion(112); showKVandMAS(8, 60); break;
                case "TOBILLO":   mostrarSelectProyeccion(113); showKVandMAS(12, 65); break;
                case "PIE":       mostrarSelectProyeccion(114); showKVandMAS(8, 60); break;
                case "FALANGES DE PIE": mostrarSelectProyeccion(115); showKVandMAS(6, 55); break;
            }
        }

        ////////////////////////////////////FIN DE METODOS AUXILIARES//////////////////////////////////////////////////////////

    }
}
