
using System;
using System.IO;
using System.Media;
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
        public void PlaySoundRx(string soundName)
        {
            try
            {
                object resource = Properties.Resources.ResourceManager.GetObject(soundName);

                if (resource == null)
                {
                    MessageBox.Show("El recurso '" + soundName + "' no existe.");
                    return;
                }

                Stream stream = resource as Stream;

                if (stream != null)
                {
                    SoundPlayer player = new SoundPlayer(stream);
                    player.Play();
                }
                else
                {
                    MessageBox.Show("El recurso '" + soundName + "' no es un WAV válido.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al reproducir el sonido: " + ex.Message);
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


        public void ChangeSorL_mAs(string foco)
        {
            if (foco.Equals("Small"))
            {
                MA = 45;//MA
            }
            else
            {
                MA = 71;//MA
            }
        }

        public string getZeroStr_mAs(int MaS)
        {
            return (MaS < 10) ? "0" + MaS.ToString() : MaS.ToString();
        }

        /// <summary>
        /// Mostrar 
        /// </summary>


        public int getImgInicial(int idx)
        {
            switch (idx)
            {
                case 0: return 0;//CRANEO
                case 1: return 3;//COLUMNA CERVICAL
                case 2: return 4;//HOMBRO
                case 3: return 7;//MANO
                case 4: return 8;//TORAX
                case 5: return 11;//ABDOMEN
                case 6: return 12;//PELVIS
                case 7: return 14;//PIE
                default: return -1;
            }

        }

        public (int mas, int kv) showBodyRayX(int countNow)
        {
            switch (countNow)
            {
                case 0: mostrarDataRayX("Craneo"); return (20, 70);
                case 1: mostrarDataRayX("Cuello"); return (30, 65);
                case 2: mostrarDataRayX("Brazos"); return (15, 67);
                case 3: mostrarDataRayX("Brazos"); return (9, 47);
                case 4: mostrarDataRayX("Escapula"); return (10, 70);
                case 5: mostrarDataRayX("Abdomen"); return (20, 80);
                case 6: mostrarDataRayX("Pelvis"); return (15, 70);
                case 7: mostrarDataRayX("Femur"); return (10, 42);
                default: return (10, 50);
            }
        }
        /// <summary>
        /// SIRVE PARA CAMBIAR LA PROYECCION SEGUN LA ESTRUCTURA SELECCIONADA
        /// </summary>

        public (int mas, int kv) changeShowCboProy(string selectEstructura)
        {
            switch (selectEstructura)
            {
                case "ESCAPÚLA": mostrarSelectProyeccion(10); return (15, 70);
                case "CLAVÍCULA": mostrarSelectProyeccion(20); return (20, 65);
                case "TORÁX": mostrarSelectProyeccion(30); return (10, 70);
                case "COSTILLAS": mostrarSelectProyeccion(40); return (40, 80);
                case "ESTERNÓN": mostrarSelectProyeccion(50); return (20, 60);
                case "ABDOMEN": mostrarSelectProyeccion(60); return (20, 75);
                case "COLUMNA LUMBAR": mostrarSelectProyeccion(70); return (45, 80);
                case "PELVIS": mostrarSelectProyeccion(80); return (15, 75);
                case "SACRO": mostrarSelectProyeccion(81); return (25, 75);
                case "CADERA": mostrarSelectProyeccion(90); return (20, 70);
                case "HOMBRO": mostrarSelectProyeccion(100); return (20, 65);
                case "CODO": mostrarSelectProyeccion(101); return (10, 56);
                case "HÚMERO": mostrarSelectProyeccion(102); return (18, 68);
                case "ANTEBRAZO": mostrarSelectProyeccion(103); return (10, 75);
                case "MUÑECA": mostrarSelectProyeccion(104); return (7, 45);
                case "MANO": mostrarSelectProyeccion(105); return (9, 47);
                case "FALANGES MANO": return (5, 48);
                case "FEMUR": mostrarSelectProyeccion(110); return (20, 72);
                case "RODILLA": mostrarSelectProyeccion(111); return (10, 65);
                case "TIBIA Y PERONÉ": mostrarSelectProyeccion(112); return (8, 60);
                case "TOBILLO": mostrarSelectProyeccion(113); return (12, 65);
                case "PIE": mostrarSelectProyeccion(114); return (8, 60);
                case "FALANGES DE PIE": return (6, 55);
                default: return (10, 50);
            }
        }

        ////////////////////////////////////FIN DE METODOS AUXILIARES//////////////////////////////////////////////////////////

    }
}
