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
        private int indiceImgNow = 0; private int nKVp = 70, nmAs = 20;
        private SerialPort sPuerto;
        private HumanSettings _Hsettings;

        //CONSTRUCTORS
        public MainRayX()
        {
            InitializeComponent();
            initBordeCuadrado();
            imgBodyRay.Image = imageLista.Images[indiceImgNow];
            imgBodyRay.SizeMode = PictureBoxSizeMode.Zoom;
            _Hsettings = new HumanSettings(cboProyeccion, cboEstructura, lblKVp,lblmAs);
            _Hsettings.showBodyRayX(0);
        }

        //==========================================FUNCTIONS============================================================//

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
        

        

        private void bootSerialPort()
        {
            try
            {
                sPuerto.PortName = configuraciones.Settings.Default.Puerto;
                sPuerto.BaudRate = configuraciones.Settings.Default.Baudios;
                sPuerto.DataBits = 8;
                sPuerto.Parity = Parity.None;
                sPuerto.StopBits = StopBits.One;
                sPuerto.Handshake = Handshake.None;

                sPuerto.WriteTimeout = 500;

                sPuerto.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fallo en:\n", ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    

        //==============================================================BUTTONS AND EVENTS=============================================//
        private void btnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnMinimizar_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }


        //BUTTONS CHANGES of IMAGES
        private void btnLeft_Click(object sender, EventArgs e)
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


        private void button1_Click(object sender, EventArgs e)
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

        //BUTTONS of OFF AND ON

        private void btnOFF_Click(object sender, EventArgs e)
        {

            btnOFF.Visible = false;
            btnON.Visible = true;
            lblEncender.Text = "ON";
            lblEncender.ForeColor = Color.LimeGreen;
        }

        private void btnON_Click(object sender, EventArgs e)
        {
            btnOFF.Visible = true;
            btnON.Visible = false;
            lblEncender.Text = "OFF";
            lblEncender.ForeColor = Color.Brown;
        }


        // TIME
        private void DATE_NOW_Tick(object sender, EventArgs e)
        {
            lblHora.Text = DateTime.Now.ToString("HH:mm:ss");
            lblFecha.Text = DateTime.Now.ToString("dd MMM yyy");
        }

        private void btnUpMaS_Click(object sender, EventArgs e)
        {
            nmAs += 1;

            if(nmAs > 200)
            {
                nmAs = 200;
            }

            lblmAs.Text = (nmAs > 0 && nmAs < 10) ? "0" + nmAs : "" + nmAs;
        }

        private void btnDownMaS_Click(object sender, EventArgs e)
        {
            nmAs -= 1;
            if (nmAs < 0)
            {
                nmAs = 0;
            }
            lblmAs.Text = (nmAs > 0 && nmAs < 10) ? "0" + nmAs : "" + nmAs;
        }

        private void btnUpKv_Click(object sender, EventArgs e)
        {
            nKVp += 1;
            if(nKVp > 100)
            {
                nKVp = 100;
            }
            lblKVp.Text = "" + nKVp;
        }


        //BUTTONS PRE _ RX _ R
        private void btnPRE_Click(object sender, EventArgs e)
        {
            using (var sonido = new SoundPlayer(@"../../Aplicaciones/tools/sonido/preparando.wav"))
            {
                sonido.Play();
            }
            Thread.Sleep(4000);

            btnPRE.BackColor = Color.Transparent;
            using (var sonido = new SoundPlayer(@"../../Aplicaciones/tools/sonido/ready.wav"))
            {
                sonido.Play();
            }
        }

        private void btnRX_Click(object sender, EventArgs e)
        {
            using (var sonido = new SoundPlayer(@"../../Aplicaciones/tools/sonido/disparo.wav"))
            {
                sonido.Play();
            }
            Thread.Sleep(1500);
        }

        private void btnR_Click(object sender, EventArgs e)
        {

        }

        //BUTTONS FOCOS
        private void btnFoco_small_Click(object sender, EventArgs e)
        {
            var Rs = FrCuadro.Show("¿Está seguro cambiar a Large?", "Configuración del Foco", MessageBoxButtons.YesNo);
            if(Rs == DialogResult.Yes)
            {
                btnFoco_small.Visible = false; btnFoco_large.Visible = true;
                Thread.Sleep(4000);
                lblFoco.Text = "LARGE";
            }
        }

        private void btnFoco_large_Click(object sender, EventArgs e)
        {
            var Rs = FrCuadro.Show("¿Está seguro cambiar a Small?", "Configuración del Foco", MessageBoxButtons.YesNo);
            if(Rs == DialogResult.Yes)
            {
                btnFoco_small.Visible = true; btnFoco_large.Visible = false;
                Thread.Sleep(4000);
                lblFoco.Text = "SMALL";
            }
        }

        private void cboEstructura_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectEstructura = cboEstructura.SelectedItem.ToString();
            this._Hsettings.changeShowCboProy(selectEstructura);
        }

        private void tecla_mAs_Click(object sender, EventArgs e)
        {
            FrKeyBoard formTecla = new FrKeyBoard();
            // Calcular la posición para mostrar el formulario debajo del botón
            Point posicionBoton = tecla_mAs.PointToScreen(Point.Empty);
            int x = posicionBoton.X;
            int y = posicionBoton.Y + tecla_mAs.Height; // Mostrar debajo del botón

            // Establecer la posición del formulario del teclado numérico
            formTecla.Location = new Point(x, y);

            // Mostrar el formulario del teclado numérico
            formTecla.ShowDialog();
        }

        private void btnDownKv_Click(object sender, EventArgs e)
        {
            nKVp -= 1;
            if(nKVp < 40)
            {
                nKVp = 40;
            }
            lblKVp.Text = "" + nKVp;
        }





    }
}
