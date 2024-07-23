using RayPro.Aplicaciones;
using RayPro.Aplicaciones.tools;
using RayPro.Persistencia.db;
using RayPro.Vista;
using System;
using System.Drawing;
using System.IO;
using System.Media;
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
        private static extern IntPtr CreateRoundRectRgn
        (
        int nLeftRect,     
        int nTopRect,      
        int nRightRect,    
        int nBottomRect,   
        int nWidthEllipse, 
        int nHeightEllipse 
        );

        private int indiceImgNow = 0; private int nKVp = 40, nmAs = 20; private double getTiempo;
        private string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DbSerial.xlsx");

        private HumanSettings _Hsettings;
        private SettingSerialPort sMonitor;
        private BDExcell obj_db_excell;
        public MainRayX()
        {
            InitializeComponent();
            initBordeCuadrado();
            InitFirstParametros();
            inhabilitarEvents(false);
            increaseTimer = new System.Windows.Forms.Timer();
            increaseTimer.Interval = 90;
            increaseTimer.Tick += IncreaseTimer_Tick;

            decreaseTimer = new System.Windows.Forms.Timer();
            decreaseTimer.Interval = 90;
            decreaseTimer.Tick += DecreaseTimer_Tick;

            btnDownKv.MouseDown += btnDownKv_MouseDown;
            btnDownKv.MouseUp += btnDownKv_MouseUp;
            btnDownKv.MouseLeave += btnDownKv_MouseLeave;
        }

        private void InitFirstParametros()
        {           
            imgBodyRay.Image = imageLista.Images[indiceImgNow];
            imgBodyRay.SizeMode = PictureBoxSizeMode.Zoom;
            obj_db_excell = new BDExcell(path);
            var dataExcell = obj_db_excell.GetDataSerialExcell(4);
            sMonitor = new SettingSerialPort(dataExcell.com,dataExcell.baudRate);
            sMonitor.DataReceived += SerialCommunication_DataReceived;
            _Hsettings = new HumanSettings(cboProyeccion, cboEstructura, lblKVp, lblmAs);
            _Hsettings.showBodyRayX(0);
        }

        private void initBordeCuadrado()
        { 
            txtProyeccion.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, txtProyeccion.Width, txtProyeccion.Height, 20, 20));
            txtEstructura.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, txtEstructura.Width, txtEstructura.Height, 20, 20));
            lblKVp.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, lblKVp.Width, lblKVp.Height, 30, 30));
            lblmAs.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, lblmAs.Width, lblmAs.Height, 30, 30));
            panelCombo.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, panelCombo.Width, panelCombo.Height, 26, 26));
            panelShow.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, panelShow.Width, panelShow.Height, 26, 26));
        }

        private void SerialCommunication_DataReceived(string data)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(SerialCommunication_DataReceived), data);
                return;
            }

            data = data.Trim();

            Console.WriteLine("Data processed: " + data);

            if (Double.TryParse(data, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double numberKv))
            {
                int roundedNumberKv = (int)Math.Round(numberKv);
                lblKVp.Text = roundedNumberKv.ToString();
                Console.WriteLine($"String convertido a entero redondeado es {roundedNumberKv}");
            }
            else
            {
                Console.WriteLine("La cadena no se pudo convertir a double.");
            }


        }
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
            btnFoco_large.Enabled = estadoAcual;
           btnFoco_small.Enabled = estadoAcual;
        }

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

            this._Hsettings.showBodyRayX(indiceImgNow);
            imgBodyRay.Image = imageLista.Images[indiceImgNow];
        }


        private void button1_Click(object sender, EventArgs e)
        {

            if (indiceImgNow < imageLista.Images.Count - 1)
            {
                indiceImgNow++;
            }

            this._Hsettings.showBodyRayX(indiceImgNow);
            imgBodyRay.Image = imageLista.Images[indiceImgNow];
          
        }

        private void btnOFF_Click(object sender, EventArgs e)
        {

            btnOFF.Visible = false;
            btnON.Visible = true;
            lblEncender.Text = "ON";
            lblEncender.ForeColor = Color.LimeGreen;
            
            sMonitor.senDataSerial(lblEncender.Text);
            inhabilitarEvents(true);
            
        }

        private void btnON_Click(object sender, EventArgs e)
        {
            btnOFF.Visible = true;
            btnON.Visible = false;
            lblEncender.Text = "OFF";
            lblEncender.ForeColor = Color.Brown;
            sMonitor.senDataSerial("Cerrar");
            inhabilitarEvents(false);
            Thread.Sleep(989);
            if (btnFoco_large.Visible == true && lblFoco.Text == "LARGE")
            {
                btnFoco_small.Visible = true; 
                btnFoco_large.Visible = false;
                lblFoco.Text = "SMALL";
            }
            else
            {
                btnFoco_small.Visible = true;
                btnFoco_large.Visible = false;
                lblFoco.Text = "SMALL";
            }
            sMonitor.senDataSerial(lblEncender.Text);
        }

        private void DATE_NOW_Tick(object sender, EventArgs e)
        {
            lblHora.Text = DateTime.Now.ToString("HH:mm:ss");
            lblFecha.Text = DateTime.Now.ToString("dd MMM yyy");
        }

        private void btnUpMaS_Click(object sender, EventArgs e)
        {
            nmAs += 1;
            if(nmAs > 300)
            {
                nmAs = 300;
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
        
        private void btnPRE_Click(object sender, EventArgs e)
        {
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string soundFilePath = Path.Combine(appDirectory, "Resources", "preparando.wav");

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

                sMonitor.senDataSerial("Pre");

                Thread.Sleep(4500);

                btnPRE.BackColor = Color.Transparent;
                soundFilePath = Path.Combine(appDirectory, "Resources", "ready.wav");
                if (File.Exists(soundFilePath))
                {
                    using (var sonido = new SoundPlayer(soundFilePath))
                    {
                        sonido.Play();
                    }
                }
                else
                {
                    MessageBox.Show("El archivo de sonido 'ready.wav' no se encontró en la ubicación especificada.");
                }

                getTiempo = _Hsettings.sendTimeInput(nmAs);
                String sendFactors = "t" + getTiempo;
                sMonitor.senDataSerial(sendFactors);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocurrió un error al intentar reproducir el sonido: " + ex.Message);
            }
        }

        private void btnRX_Click(object sender, EventArgs e)
        {
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string soundFilePath = Path.Combine(appDirectory, "Resources", "disparo.wav");

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

                sMonitor.senDataSerial("D_RX");
                Thread.Sleep(1000);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocurrió un error al intentar reproducir el sonido: " + ex.Message);
            }
        }

        private void btnR_Click(object sender, EventArgs e)/*(RESETEAR)*/
        {
            lblmAs.Text = "20";
            lblKVp.Text = "50";

            sMonitor.senDataSerial("Reseteo");
            Thread.Sleep(2000);// 2 seg
            _Hsettings.showBodyRayX(0);

        }

        private void btnFoco_small_Click(object sender, EventArgs e) /*(SMALL)*/
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
        }

        private void btnFoco_large_Click(object sender, EventArgs e)
        {
            var Rs = FrCuadro.Show("¿Está seguro cambiar a Small?", "Configuración del Foco", MessageBoxButtons.YesNo);
            if(Rs == DialogResult.Yes)
            {
                btnFoco_small.Visible = true; btnFoco_large.Visible = false;
                sMonitor.senDataSerial("Filamento");
                Thread.Sleep(4000);
                lblFoco.Text = "SMALL";
                _Hsettings.maSmallOrLarge(lblFoco.Text);
                sMonitor.senDataSerial(lblFoco.Text);
            }
        }

        private void cboEstructura_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectEstructura = cboEstructura.SelectedItem.ToString();
            this._Hsettings.changeShowCboProy(selectEstructura);
        }

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

        private void tecla_Kv_Click(object sender, EventArgs e)
        {
            FrKeyBoard formTecla = new FrKeyBoard("kVolt", 125, 0);
            formTecla.StartPosition = FormStartPosition.Manual;
            formTecla.Location = new System.Drawing.Point(
                    this.Left + tecla_Kv.Left,
                    this.Top + tecla_Kv.Top + tecla_Kv.Height);

            formTecla.ShowDialog();

            nKVp = formTecla.SentNumerbs;
            lblKVp.Text = "" + nKVp;
        }

        private void MainRayX_FormClosing(object sender, FormClosingEventArgs e)
        {
            sMonitor.CerrarSerialPort();
        }

        private void btnDownKv_Click(object sender, EventArgs e)
        {
           sMonitor.senDataSerial("l-");
        }

        private void btnDownKv_MouseDown(object sender, MouseEventArgs e)
        {
            sMonitor.senDataSerial("l+");
            decreaseTimer.Start();
        }

        private void btnDownKv_MouseUp(object sender, MouseEventArgs e)
        {
            StopDecreasing();
        }

        private void btnDownKv_MouseLeave(object sender, EventArgs e)
        {
            StopDecreasing();
        }

        private void StopDecreasing()
        {
            sMonitor.senDataSerial("l-");
            decreaseTimer.Stop(); 
        }

        private void DecreaseTimer_Tick(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(lblKVp.Text))
            {
                int valorActual = int.Parse(lblKVp.Text); 
                valorActual--; 
                lblKVp.Text = valorActual.ToString(); 

                if (valorActual < 48)
                {
                    valorActual = 48;
                    lblKVp.Text = valorActual.ToString();
                }
            }
        }
        private void btnUpKv_Click(object sender, EventArgs e)
        {
            sMonitor.senDataSerial("r+");
        }

        private void btnUpKv_MouseDown(object sender, MouseEventArgs e)
        {
            sMonitor.senDataSerial("r+");
            increaseTimer.Start();
        }

        private void btnUpKv_MouseUp(object sender, MouseEventArgs e)
        {
            sMonitor.senDataSerial("r-");
            increaseTimer.Stop(); 
        }

        private void lblKVp_Click(object sender, EventArgs e)
        {

        }

        private void IncreaseTimer_Tick(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(lblKVp.Text))
            {
                int valorActual = int.Parse(lblKVp.Text);
                valorActual++;
                lblKVp.Text = valorActual.ToString();

                if (valorActual > 130)
                {
                    valorActual = 130;
                    lblKVp.Text = valorActual.ToString();
                }
            }            
        }
    }
}