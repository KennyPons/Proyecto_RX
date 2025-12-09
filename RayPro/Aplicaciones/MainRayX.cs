using RayPro.Aplicaciones;
using RayPro.Aplicaciones.tools;
using RayPro.Persistencia.db;
using RayPro.Vista;
using System;
using System.Drawing;
using System.IO;
using System.Media;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace RayPro
{
    public partial class MainRayX : Form
    {

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);
        private Action valorCambiaAction;

        private int kv = 40, mAs = 1, indiceImgNow = 0;

        private double getTiempo;
        /*Se cambia a Large, estado para ser = True, si cambia a Small va ser estado = False*/
        private bool estadoFoco, NoExecute = false; 

        private HumanSupport hSupport;
        private SerialPortManager sMonitor;

        public MainRayX()
        {
            InitializeComponent();
            InitFirstParametros();
            ControlCambioFlechas();
   
        }

        

        private void InitFirstParametros()
        {
            initRoundedBorders();
            showBodyRay.Image = imgLstBody.Images[indiceImgNow];
            ShowSecuenciaRx.Image = lstSecuenciaRx.Images[0];
            lblmAs.Text = "0" + mAs;
            lblKVp.Text = kv.ToString();

            enableSystemEvents(false);

            // Inicializar la comunicación serial
            //sMonitor = new SerialPortManager(dataExcell.com,dataExcell.baudRate);
            //sMonitor.DataReceived += SerialCommunication_DataReceived;
            hSupport = new HumanSupport(cboProyeccion, cboEstructura, lblKVp, lblmAs);
        }

        /*CONTROL DE TIEMPO O SECUENCIAL DE KV Y MAS*/
        private void ControlCambioFlechas()
        {
            btnUpKv.MouseDown += (s, e) => startValorChange(() => CambiarKv(1));
            btnUpKv.MouseUp += (s, e) => stopValorChange();
            btnDownKv.MouseDown += (s, e) => startValorChange(() => CambiarKv(-1));
            btnDownKv.MouseUp += (s, e) => stopValorChange();

            btnUpMaS.MouseDown += (s, e) => startValorChange(() => CambiarMaS(1));
            btnUpMaS.MouseUp += (s, e) => stopValorChange();
            btnDownMaS.MouseDown += (s, e) => startValorChange(() => CambiarMaS(-1));
            btnDownMaS.MouseUp += (s, e) => stopValorChange();
        }


        private void enableSystemEvents(bool status)
        {
            btnLeft.Enabled = status;
            btnRight.Enabled = status;
            btnPRE.Enabled = status;
            btnRX.Enabled = status;
            btnR.Enabled = status;
            btnFilamento.Enabled = status;
            panelShow.Enabled = status;
        }

        private void parametrosSecuencia(int wight, int high, int pointY)
        {
            ShowSecuenciaRx.Size = new Size(130, 140);
            ShowSecuenciaRx.Location = new Point(478, 40);
        }
        private void visualBtnRx(bool status)
        {
            btnPRE.Visible = status;
            btnFilamento.Visible = status;
            NoExecute = !status;
            
        }

        private void initRoundedBorders()
        {
            txtProyeccion.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, txtProyeccion.Width, txtProyeccion.Height, 20, 20));
            txtEstructura.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, txtEstructura.Width, txtEstructura.Height, 20, 20));
            lblKVp.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, lblKVp.Width, lblKVp.Height, 30, 30));
            lblmAs.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, lblmAs.Width, lblmAs.Height, 30, 30));
            panelFoco.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, panelFoco.Width, panelFoco.Height, 26, 26));
            panelShow.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, panelShow.Width, panelShow.Height, 26, 26));
        }


        /// <summary>
        /// EVENTOS DE CERRA Y MINIMIZAR APP
        /// </summary>
        ///

        private void btnClose_Click(object sender, EventArgs e)
        {         
            if(lblEncender.Text == "ON" && btnON.Visible == true)
            {
                MessageBox.Show("Por favor Asegurese que el equipo este apagado correctamente", "Advertencia!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                Application.Exit();
            }
        }

        private void btnMinimizar_Click(object sender, EventArgs e)
        {
             WindowState = FormWindowState.Minimized;
        }

       

        /// <summary>
        /// METODOS PARA CONTROLAR LA FECHAS DE KV Y MAS
        /// </summary>
        /// 

        private void startValorChange(Action action)
        {
            valorCambiaAction = action;
            changeTimer.Start();
            action();// Ejecuta una vez al presionar
        }

        private void stopValorChange()
        {
            changeTimer.Stop();
            valorCambiaAction = null;
        }

        private void CambiarKv(int value)
        {
            int newKv = kv + value;
            if (newKv >= 40 && newKv <= 110)
            {
                kv = newKv;
                lblKVp.Text = kv.ToString();
            }
        }

        private void CambiarMaS(int value)
        {
            int newMaS = mAs + value;
            if (newMaS >= 1 && newMaS <= 300)
            {
                mAs = newMaS;

                lblmAs.Text = hSupport.formatoStrMaS(mAs);
            }
        }


        /// <summary>
        ///////////////// EVENTOS DE BOTONES DE RAYOS X ///////////////////
        /// </summary>


        private void btnLeft_Click(object sender, EventArgs e)
        {

            if (indiceImgNow > 0)
            {
                indiceImgNow--;
            }
            else
            {
                indiceImgNow = 0;
            }

            hSupport.showBodyRayX(indiceImgNow);
            showBodyRay.Image = imgLstBody.Images[indiceImgNow];
        }


        private void button1_Click(object sender, EventArgs e)
        {

            if (indiceImgNow < imgLstBody.Images.Count - 1)
            {
                indiceImgNow++;
            }

            hSupport.showBodyRayX(indiceImgNow);
            showBodyRay.Image = imgLstBody.Images[indiceImgNow];
          
        }

        private void btnOFF_Click(object sender, EventArgs e)
        {

            btnOFF.Visible = false;
            btnON.Visible = true;
            lblEncender.Text = "ON";
            lblEncender.ForeColor = Color.LimeGreen;
            
            //sMonitor.senDataSerial(lblEncender.Text);
            enableSystemEvents(true);
            
        }

        private void btnON_Click(object sender, EventArgs e)
        {
            btnOFF.Visible = true;
            btnON.Visible = false;
            lblEncender.Text = "OFF";
            lblEncender.ForeColor = Color.Brown;
            //sMonitor.senDataSerial("Cerrar");
            enableSystemEvents(false);
            //Thread.Sleep(989);

            //sMonitor.senDataSerial(lblEncender.Text);
        }

        private void DATE_NOW_Tick(object sender, EventArgs e)
        {
            lblHora.Text = DateTime.Now.ToString("HH:mm:ss");
            lblFecha.Text = DateTime.Now.ToString("dd MMM yyy");
        }


        
        private void btnPRE_Click(object sender, EventArgs e)
        {
            if (cboEstructura.Text == "TORÁX")
            {
                hSupport.playSoundRx("NoRespirar.wav");
            }
            else
            {
                hSupport.playSoundRx("preparando.wav");
            }
            
            visualBtnRx(false);
            //sMonitor.senDataSerial("Pre");


            Thread.Sleep(3500);

            hSupport.playSoundRx("ready.wav");
            lblFoco.Text = "LISTO";
            ShowSecuenciaRx.Image = lstSecuenciaRx.Images[2];
            parametrosSecuencia(100, 100, 78);

            getTiempo = hSupport.sendTimeInput(mAs);
            string sendFactors = "t" + getTiempo;
            //sMonitor.senDataSerial(sendFactors);

        }

        private void btnRX_Click(object sender, EventArgs e)
        {
            if(!NoExecute)
                return;

            hSupport.playSoundRx("disparo.wav");
            parametrosSecuencia(130, 140, 40);
            //sMonitor.senDataSerial("D_RX");

            Thread.Sleep(3000);

            if (cboEstructura.Text == "TORÁX")
            {
                hSupport.playSoundRx("Respirar.wav");
            }
            

            visualBtnRx(true);
            lblFoco.Text = (!estadoFoco)? "SMALL":"LARGE";//Si esta activo el Foco Large, Es True pero se convierte a Falso y muestra LARGE Actual
            ShowSecuenciaRx.Image = (!estadoFoco) ? lstSecuenciaRx.Images[0]: lstSecuenciaRx.Images[1];
   
        }

        private void btnR_Click(object sender, EventArgs e)/*(RESETEAR)*/
        {
            if (!NoExecute)
                return;

            //sMonitor.senDataSerial("Reseteo");
            parametrosSecuencia(130, 140, 40);
            Thread.Sleep(500);

            visualBtnRx(true);
            lblFoco.Text = (!estadoFoco) ? "SMALL" : "LARGE";//Si esta activo el Foco Large, Es True pero se convierte a Falso y muestra LARGE Actual
            ShowSecuenciaRx.Image = (!estadoFoco) ? lstSecuenciaRx.Images[0] : lstSecuenciaRx.Images[1];
            hSupport.showBodyRayX(0);

        }


        private void btnFilamento_Click(object sender, EventArgs e)
        {
            if(lblFoco.Text == "SMALL") 
            {
               Thread.Sleep(2000);
               lblFoco.Text = "LARGE";
               estadoFoco = true;
               ShowSecuenciaRx.Image = lstSecuenciaRx.Images[1];
            }
            else
            {
                Thread.Sleep(2000);
                lblFoco.Text = "SMALL";
                estadoFoco = false;
                ShowSecuenciaRx.Image = lstSecuenciaRx.Images[0];
            }
        }

        private void changeTimer_Tick(object sender, EventArgs e)
        {
            valorCambiaAction?.Invoke();
        }

        

        /*private void btnFoco_small_Click(object sender, EventArgs e)
        {
            var Rs = FrCuadro.Show("¿Está seguro cambiar a Large?", "Configuración del Foco", MessageBoxButtons.YesNo);
            if(Rs == DialogResult.Yes)
            {
                btnFoco_small.Visible = false; btnFoco_large.Visible = true;
                sMonitor.senDataSerial("Filamento");
                Thread.Sleep(4000);
                lblFoco.Text = "LARGE";
                _Hsettings.maSmallOrLarge(lblFoco.Text);
                sMonitor.senDataSerial(lblFoco.Text);
            }
        }*/



        private void cboEstructura_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectEstructura = cboEstructura.SelectedItem.ToString();
            hSupport.changeShowCboProy(selectEstructura);
        }

        /*private void tecla_mAs_Click(object sender, EventArgs e)
        {
            FrKeyBoard formTecla = new FrKeyBoard("amperaje",300,0);
            
            formTecla.StartPosition = FormStartPosition.Manual;
            formTecla.Location = new System.Drawing.Point(
                    this.Left + tecla_mAs.Left,
                    this.Top + tecla_mAs.Top + tecla_mAs.Height);
            formTecla.ShowDialog();

            nmAs = formTecla.SentNumerbs;
            lblmAs.Text = "" + nmAs; 
        }*/

       
        private void MainRayX_FormClosing(object sender, FormClosingEventArgs e)
        {
            //sMonitor.CerrarSerialPort();
        }





        //////FIN DE SOFTWARE/////

    }
}