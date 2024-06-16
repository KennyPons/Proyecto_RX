using RayPro.Aplicaciones;
using RayPro.Aplicaciones.tools;
using RayPro.Vista;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace RayPro
{
    public partial class MainRayX : Form
    {
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn//crea borde rectangular
        (
        int nLeftRect,     // x-coordinate of upper-left corner
        int nTopRect,      // y-coordinate of upper-left corner
        int nRightRect,    // x-coordinate of lower-right corner
        int nBottomRect,   // y-coordinate of lower-right corner
        int nWidthEllipse, // height of ellipse
        int nHeightEllipse // width of ellipse
        );

        //PRIMITIVOS DATA
        private int indiceImgNow = 0; private int nKVp = 40, nmAs = 20;
        
        private HumanSettings _Hsettings;
        private SettingSerialPort sMonitor;
        //CONSTRUCTORS
        public MainRayX()
        {
            InitializeComponent();
            initBordeCuadrado();
            InitFirstParametros();
            inhabilitarEvents(false);
            
        }

        //==========================================FUNCIONES INICIO AL SYSTEMA============================================================//

        private void InitFirstParametros()
        {
            imgBodyRay.Image = imageLista.Images[indiceImgNow];
            imgBodyRay.SizeMode = PictureBoxSizeMode.Zoom;
            sMonitor = new SettingSerialPort();
            _Hsettings = new HumanSettings(cboProyeccion, cboEstructura, lblKVp, lblmAs);
            _Hsettings.showBodyRayX(0);
           
        }

        private void initBordeCuadrado()
        { /// BORDAR FIGURA CUADRA DE TEXT BOX
            txtProyeccion.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, txtProyeccion.Width, txtProyeccion.Height, 20, 20));
            txtEstructura.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, txtEstructura.Width, txtEstructura.Height, 20, 20));
            lblKVp.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, lblKVp.Width, lblKVp.Height, 30, 30));
            lblmAs.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, lblmAs.Width, lblmAs.Height, 30, 30));
            //Conexion_txt.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Conexion_txt.Width, Conexion_txt.Height, 29, 29));
            panelCombo.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, panelCombo.Width, panelCombo.Height, 26, 26));
            panelShow.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, panelShow.Width, panelShow.Height, 26, 26));
        }
        

        

        

        //==============================================================BUTTONS AND EVENTS=============================================//
        private void btnClose_Click(object sender, EventArgs e)//Cerrar App
        {
            Application.Exit();
        }

        private void btnMinimizar_Click(object sender, EventArgs e)//Minimizar App
        {
            this.WindowState = FormWindowState.Minimized;
        }


        private void inhabilitarEvents(bool estadoAcual)
        {
            btnLeft.Enabled = estadoAcual;
            btnRight.Enabled = estadoAcual;
            btnPRE.Enabled = estadoAcual;
            btnRX.Enabled = estadoAcual;
            btnR.Enabled = estadoAcual;
            btnDownKv.Enabled = estadoAcual;
            btnUpKv.Enabled = estadoAcual;
            btnDownMaS.Enabled = estadoAcual;
            btnUpMaS.Enabled = estadoAcual;
            tecla_Kv.Enabled = estadoAcual;
            tecla_mAs.Enabled = estadoAcual;
        }

        //BUTTONS CHANGES of IMAGES
        private void btnLeft_Click(object sender, EventArgs e)//Botón Izquierdo para Imagen
        {

            /*indiceImgNow = (indiceImgNow - 1 + imageLista.Images.Count) % imageLista.Images.Count;

            imgBodyRay.Image = imageLista.Images[indiceImgNow];*/
            if (indiceImgNow > 0)
            {
                indiceImgNow--;
            }
            else
            {
                indiceImgNow = 0;
            }

            this._Hsettings.showBodyRayX(indiceImgNow);
            imgBodyRay.Image = imageLista.Images[indiceImgNow];
        }


        private void button1_Click(object sender, EventArgs e)//Botón Derecho para cambiar imagenes
        {
            /* indiceImgNow = (indiceImgNow + 1) % imageLista.Images.Count;

           if (indiceImgNow != 0)
           {
               imgBodyRay.Image = imageLista.Images[indiceImgNow];
           }*/

            if (indiceImgNow < imageLista.Images.Count - 1)
            {
                indiceImgNow++;
            }

            this._Hsettings.showBodyRayX(indiceImgNow);
            imgBodyRay.Image = imageLista.Images[indiceImgNow];
          
        }

        //Botones de prender y apagar

        private void btnOFF_Click(object sender, EventArgs e)
        {

            btnOFF.Visible = false;
            btnON.Visible = true;
            lblEncender.Text = "ON";
            lblEncender.ForeColor = Color.LimeGreen;
            sMonitor.bootSerialPort();
            sMonitor.EnviarDatosASerial("ON");
            Thread.Sleep(400);
            TiempoKv.Enabled = true;
            inhabilitarEvents(true);
            
        }

        private void btnON_Click(object sender, EventArgs e)
        {
            btnOFF.Visible = true;
            btnON.Visible = false;
            lblEncender.Text = "OFF";
            lblEncender.ForeColor = Color.Brown;
            sMonitor.EnviarDatosASerial("APAGAR");
            Thread.Sleep(300);
            TiempoKv.Enabled = false;
            inhabilitarEvents(false);
            sMonitor.CerrarSerialPort();
            sMonitor.EnviarDatosASerial("OFF");            
        }


        // Mostrando en la App el TIME 
        private void DATE_NOW_Tick(object sender, EventArgs e)
        {
            lblHora.Text = DateTime.Now.ToString("HH:mm:ss");
            lblFecha.Text = DateTime.Now.ToString("dd MMM yyy");
        }

        //Flechita Arriba O Up mAs
        private void btnUpMaS_Click(object sender, EventArgs e)
        {
            nmAs += 1;

            if(nmAs > 300)
            {
                nmAs = 300;
            }

            lblmAs.Text = (nmAs > 0 && nmAs < 10) ? "0" + nmAs : "" + nmAs;
        }
        //Flechita Abajo o Down mAs
        private void btnDownMaS_Click(object sender, EventArgs e)
        {
            nmAs -= 1;
            if (nmAs < 0)
            {
                nmAs = 0;
            }
            lblmAs.Text = (nmAs > 0 && nmAs < 10) ? "0" + nmAs : "" + nmAs;
        }
        //Flechita Arriba o up Kv
        private void btnUpKv_Click(object sender, EventArgs e)
        {
            /*nKVp += 1;
            if(nKVp > 100)
            {
                nKVp = 100;
            }
            lblKVp.Text = "" + nKVp;*/
            sMonitor.EnviarDatosASerial("r");
            Thread.Sleep(10);
        }


        //BOTONES IMPORTANTES ( PRE _ RX _ R )
        private void btnPRE_Click(object sender, EventArgs e) /*(PRE)*/
        {
            using (var sonido = new SoundPlayer(@"../../Aplicaciones/tools/sonido/preparando.wav"))
            {
                sonido.Play();

            }
            sMonitor.EnviarDatosASerial("PRE");


            Thread.Sleep(3000);

            btnPRE.BackColor = Color.Transparent;
            using (var sonido = new SoundPlayer(@"../../Aplicaciones/tools/sonido/ready.wav"))
            {
                sonido.Play();
            }

            /*int getVini = _Hsettings.initialVoltageInput(nKVp);*/
            double getTiempo = _Hsettings.sendTimeInput(nmAs);
            String sendFactors = "t" + getTiempo;
            sMonitor.EnviarDatosASerial(sendFactors);
        }

        private void btnRX_Click(object sender, EventArgs e)/*(DISPARO-RX)*/
        {
            using (var sonido = new SoundPlayer(@"../../Aplicaciones/tools/sonido/disparo.wav"))
            {
                sonido.Play();
            }
            sMonitor.EnviarDatosASerial("D_RX");
            Thread.Sleep(1000);
            
        }

        private void btnR_Click(object sender, EventArgs e)/*(RESETEAR)*/
        {
            lblmAs.Text = "20";
            lblKVp.Text = "70";
           
            Thread.Sleep(2000);// 2 seg
            InitFirstParametros();

        }

        //Botón para cambiar el Filamento
        private void btnFoco_small_Click(object sender, EventArgs e) /*(SMALL)*/
        {
            var Rs = FrCuadro.Show("¿Está seguro cambiar a Large?", "Configuración del Foco", MessageBoxButtons.YesNo);
            if(Rs == DialogResult.Yes)
            {
                btnFoco_small.Visible = false; btnFoco_large.Visible = true;
                sMonitor.EnviarDatosASerial("FILAMENTO");
                Thread.Sleep(4000);
                lblFoco.Text = "LARGE";
                _Hsettings.maSmallOrLarge(lblFoco.Text);
                sMonitor.EnviarDatosASerial(lblFoco.Text);
            }
        }

        private void btnFoco_large_Click(object sender, EventArgs e)/*(LARGE)*/
        {
            var Rs = FrCuadro.Show("¿Está seguro cambiar a Small?", "Configuración del Foco", MessageBoxButtons.YesNo);
            if(Rs == DialogResult.Yes)
            {
                btnFoco_small.Visible = true; btnFoco_large.Visible = false;
                sMonitor.EnviarDatosASerial("FILAMENTO");
                Thread.Sleep(4000);
                lblFoco.Text = "SMALL";
                _Hsettings.maSmallOrLarge(lblFoco.Text);
                sMonitor.EnviarDatosASerial(lblFoco.Text);
            }
        }

        //El combo de "Estructura" realizando cambios
        private void cboEstructura_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectEstructura = cboEstructura.SelectedItem.ToString();
            this._Hsettings.changeShowCboProy(selectEstructura);
        }

        //Botones teclado para mAs
        private void tecla_mAs_Click(object sender, EventArgs e)
        {
            FrKeyBoard formTecla = new FrKeyBoard("amperaje",300,0);
            
            formTecla.StartPosition = FormStartPosition.Manual;
            formTecla.Location = new System.Drawing.Point(
                    this.Left + tecla_mAs.Left,
                    this.Top + tecla_mAs.Top + tecla_mAs.Height);
            formTecla.ShowDialog();

            nmAs = formTecla.SentNumerbs;
            lblmAs.Text = "" + nmAs; 
        }

        //Botones teclado para Kv
        private void tecla_Kv_Click(object sender, EventArgs e)
        {
            FrKeyBoard formTecla = new FrKeyBoard("kVolt", 125, 0);//change Kv
            /*Solo para pocisionar el teclado a la Izquierda*/
            formTecla.StartPosition = FormStartPosition.Manual;
            formTecla.Location = new System.Drawing.Point(
                    this.Left + tecla_Kv.Left,
                    this.Top + tecla_Kv.Top + tecla_Kv.Height);

            formTecla.ShowDialog();

            nKVp = formTecla.SentNumerbs;
            lblKVp.Text = "" + nKVp;
        }

        private void TiempoKv_Tick(object sender, EventArgs e)
        {
            string dataKv = sMonitor.GetDatoRecibido();
            if (!string.IsNullOrEmpty(dataKv))
            {
                lblKVp.Text = dataKv; // Actualiza el Label con los datos recibidos
            }
            else
            {
                nKVp = 40;
                lblKVp.Text = nKVp.ToString();
            }
        }

        //Flechita Abajo o Down Kv
        private void btnDownKv_Click(object sender, EventArgs e)
        {
            /*nKVp -= 1;
            if(nKVp < 40)
            {
                nKVp = 40;
            }
            lblKVp.Text = "" + nKVp;*/
            sMonitor.EnviarDatosASerial("l");
            Thread.Sleep(10);
        }





    }
}
