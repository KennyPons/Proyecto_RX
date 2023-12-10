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
using System.Threading.Tasks;
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
        private int indiceImgNow = 0; private int nKVp = 70, nmAs = 20;
        private SerialPort sPuerto;
        public MainRayX()
        {
            InitializeComponent();
            initBordeCuadrado();
            imgBodyRay.Image = imageLista.Images[indiceImgNow];
            imgBodyRay.SizeMode = PictureBoxSizeMode.Zoom;
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
        private void showBodyRayX(int countNow)
        {
            switch(countNow)
            {
                case 0:  ; break; case 1:; break; case 2: break;
                    //indiceImagenActual = (indiceImagenActual + 1) % imageList1.Images.Count;
            }
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
    

        //===========================================================================================================//
        private void btnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnMinimizar_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btnLeft_Click(object sender, EventArgs e)
        {
            
            indiceImgNow = (indiceImgNow - 1 + imageLista.Images.Count) % imageLista.Images.Count;

            imgBodyRay.Image = imageLista.Images[indiceImgNow];
        }

        private void button1_Click(object sender, EventArgs e)
        {
           
            indiceImgNow = (indiceImgNow + 1) % imageLista.Images.Count;

            imgBodyRay.Image = imageLista.Images[indiceImgNow];
        }

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

            lblmAs.Text = "" + nmAs;
        }

        private void btnDownMaS_Click(object sender, EventArgs e)
        {
            nmAs -= 1;
            if (nmAs < 0)
            {
                nmAs = 0;
            }
            lblmAs.Text = "" + nmAs;
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
