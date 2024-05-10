using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RayPro.Aplicaciones.tools
{
    internal class HumanSettings
    {
        
        private DatosRadiologia _datosRadiologia; private ComboBox cboProy, cboEstruct; private Label lblKVp, lblMaS;
        private double mil = Math.Pow(10, 3);
        //constructor
        public HumanSettings(ComboBox cboProy, ComboBox cboEstruct, Label lblKVp, Label lblMaS) {
            this.cboEstruct = cboEstruct; this.cboProy = cboProy;
            this.lblKVp = lblKVp; this.lblMaS = lblMaS;
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

        private void showKVandMAS(int n_mas, int n_kv)
        {
            lblKVp.Text = "" + n_kv;

            lblMaS.Text = (n_mas > 0 && n_mas < 10) ? "0" + n_mas : "" + n_mas;
        }


        /////////////////METHODS - USE MAINS///////////////////////////////////////////////////

        public int initialVoltageInput(int kvNeed)
        {
            
            int kvInicial = kvNeed * Convert.ToInt32(mil), kvFinal = 100 * Convert.ToInt32(mil);

            int result_Entrada_Ini = (250 * kvInicial) / kvFinal;
            
            return result_Entrada_Ini;
            
        }

        public double sendTimeInput(int mAsInput)
        {
            double resultTiempo = (double )mAsInput / 75;

            double redondeoTiempo = Math.Round(resultTiempo, 3, MidpointRounding.AwayFromZero);


            return redondeoTiempo * Convert.ToInt32(mil);

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


        public void changeShowCboProy(string selectEstructura)
        {
            switch(selectEstructura)
            {
                case "ESCAPÚLA":  mostrarSelectProyeccion(10); showKVandMAS(15, 72); break;
                case "CLAVÍCULA": mostrarSelectProyeccion(20); showKVandMAS(15, 72); break;
                case "TORÁX":     mostrarSelectProyeccion(30); showKVandMAS(16, 72); break;
                case "COSTILLAS": mostrarSelectProyeccion(40); showKVandMAS(16, 72); break;
                case "ESTERNÓN":  mostrarSelectProyeccion(50); showKVandMAS(22, 68); break;
                case "ABDOMEN":   mostrarSelectProyeccion(60); showKVandMAS(35, 75); break;
                case "COLUMNA LUMBAR": mostrarSelectProyeccion(70); showKVandMAS(45, 80); break;
                case "PELVIS":    mostrarSelectProyeccion(80);  showKVandMAS(30, 75); break;
                case "SACRO":     mostrarSelectProyeccion(81);  showKVandMAS(25, 75); break;
                case "CADERA":    mostrarSelectProyeccion(90);  showKVandMAS(25, 75); break;
                case "HOMBRO":    mostrarSelectProyeccion(100); showKVandMAS(20, 65); break;
                //TODO LA PARTE DE BRAZO
                case "CODO":      mostrarSelectProyeccion(101); showKVandMAS(10, 56); break;
                case "HÚMERO":    mostrarSelectProyeccion(102); showKVandMAS(18, 68); break;
                case "ANTEBRAZO": mostrarSelectProyeccion(103); showKVandMAS(10, 75); break;
                case "MUÑECA":    mostrarSelectProyeccion(104); showKVandMAS(3, 54); break;
                case "MANO":      mostrarSelectProyeccion(105); showKVandMAS(5, 48); break;
                case "FALANGES MANO": mostrarSelectProyeccion(106); showKVandMAS(5, 48); break;
                //TODO LA PARTE DE BRAZO
                case "FEMUR":     mostrarSelectProyeccion(110); showKVandMAS(20, 72); break;
                case "RODILLA":   mostrarSelectProyeccion(111); showKVandMAS(10, 65); break;
                case "TIBIA Y PERONÉ": mostrarSelectProyeccion(112); showKVandMAS(8, 60); break;
                case "TOBILLO":   mostrarSelectProyeccion(113); showKVandMAS(9, 62); break;
                case "PIE":       mostrarSelectProyeccion(114); showKVandMAS(9, 68); break;
                case "FALANGES DE PIE": mostrarSelectProyeccion(115); showKVandMAS(5, 55); break;
            }
        }
    }
}
