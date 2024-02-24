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

        //constructor
        public HumanSettings(ComboBox cboProy, ComboBox cboEstruct, Label lblKVp, Label lblMaS) {
            this.cboEstruct = cboEstruct; this.cboProy = cboProy;
            this.lblKVp = lblKVp; this.lblMaS = lblMaS;
            _datosRadiologia = new DatosRadiologia();
        }

        ///////function private///////////////

        private void LimpiarYConfigurarCombo(ComboBox cboBox, string[] itemsRadiologia, string selectedText)
        {
            cboBox.Items.Clear();
            cboBox.Text = selectedText;
            foreach (var ray in itemsRadiologia)
            {
                cboBox.Items.Add(ray);
            }
        }

        /////////////////METHODS///////////////////////////////////////////////////

        public void mostrarDataRayX(string selectEstructura)
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


        public void mostrarSelectProyeccion(int n_mostrar)
        {
            if (_datosRadiologia.SelectProyeccion.ContainsKey(n_mostrar))
            {
                LimpiarYConfigurarCombo(cboProy, _datosRadiologia.SelectProyeccion[n_mostrar], _datosRadiologia.SelectProyeccion[n_mostrar][0]);
            }

        }

        public void showKVandMAS(int n_mas, int n_kv)
        {
            lblKVp.Text = "" + n_kv;

            lblMaS.Text = (n_mas > 0 && n_mas < 10) ? "0" + n_mas : "" + n_mas;
        }

    }
}
