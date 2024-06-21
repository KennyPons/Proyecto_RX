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
        private bool aumentando = false;

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
        private int indiceImgNow = 0; private int nKVp = 40, nmAs = 20; private double getTiempo;
        //CHRISTIAN
        private string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DbSerial.xlsx");

        private HumanSettings _Hsettings;
        private SettingSerialPort sMonitor;
        private BDExcell obj_db_excell;
        //CONSTRUCTORS
        public MainRayX()
        {
            InitializeComponent();
            initBordeCuadrado();
            InitFirstParametros();
            inhabilitarEvents(false);
            increaseTimer = new System.Windows.Forms.Timer();
            increaseTimer.Interval = 90; // Intervalo de actualización (en milisegundos)
            increaseTimer.Tick += IncreaseTimer_Tick;

            decreaseTimer = new System.Windows.Forms.Timer();
            decreaseTimer.Interval = 90; // Intervalo en milisegundos (ajústalo según tus necesidades)
            decreaseTimer.Tick += DecreaseTimer_Tick;

            btnDownKv.MouseDown += btnDownKv_MouseDown;
            btnDownKv.MouseUp += btnDownKv_MouseUp;
            btnDownKv.MouseLeave += btnDownKv_MouseLeave;
        }

        //==========================================FUNCIONES INICIO AL SYSTEMA PRIVATE============================================================//

        private void InitFirstParametros()
        {
            
            imgBodyRay.Image = imageLista.Images[indiceImgNow];
            imgBodyRay.SizeMode = PictureBoxSizeMode.Zoom;
            /*Excell*///CHRISTIAN este excell
            obj_db_excell = new BDExcell(path);
            var dataExcell = obj_db_excell.GetDataSerialExcell(4);
            /*Serial*///CHRISTIAN 
            sMonitor = new SettingSerialPort(dataExcell.com,dataExcell.baudRate);
            sMonitor.DataReceived += SerialCommunication_DataReceived;
            /*Human*/
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

        /*private void SerialCommunication_DataReceived(object sender, string data)
        {
            // Manejar los datos recibidos, por ejemplo, actualizar un TextBox
            Invoke(new MethodInvoker(delegate
            {
                lblKVp.Text = data;
                //Enviar a Tiempo real el Kv
            }));
        }*/

        private void SerialCommunication_DataReceived(string data)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(SerialCommunication_DataReceived), data);
                return;
            }

            data = data.Trim();

            Console.WriteLine("Data processed: " + data); // Mensaje de depuración

            if (Double.TryParse(data, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double numberKv))
            {
                int roundedNumberKv = (int)Math.Round(numberKv);
                lblKVp.Text = roundedNumberKv.ToString();
                Console.WriteLine($"String convertido a entero redondeado es {roundedNumberKv}"); // Salida: String '4.5' convertido a entero redondeado es 5
            }
            else
            {
                Console.WriteLine("La cadena no se pudo convertir a double.");
            }


        }





        //==============================================================BUTTONS AND EVENTS=============================================//
        private void btnClose_Click(object sender, EventArgs e)//Cerrar App
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
            btnFoco_large.Enabled = estadoAcual;
           btnFoco_small.Enabled = estadoAcual;
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

        //Buttons de prender y apagar

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
            aumentando = true;

            if(nmAs > 300)
            {
                nmAs = 300;
            }

            lblmAs.Text = (nmAs > 0 && nmAs < 10) ? "0" + nmAs : "" + nmAs;

        }
        private void btnUpMaS_MouseDown(object sender, MouseEventArgs e)
        {
            aumentando = true; // Indica que se debe seguir aumentando el valor

            // Inicia un bucle que aumenta continuamente el valor mientras el botón está pulsado
            while (aumentando)
            {
                if (!string.IsNullOrEmpty(lblmAs.Text))
                {
                    int valorActual = int.Parse(lblmAs.Text); // Obtiene el valor actual del TextBox
                    valorActual++; // Incrementa el valor actual en uno
                    lblmAs.Text = valorActual.ToString(); // Actualiza el texto en el TextBox con el nuevo valor
                }

                // Espera un breve periodo para no saturar la interfaz gráfica
                System.Threading.Thread.Sleep(100);
                Application.DoEvents(); // Permite actualizar la interfaz gráfica durante el bucle
            }
        }

        private void btnUpMaS_MouseUp(object sender, MouseEventArgs e)
        {
            aumentando = false; // Detiene el aumento del valor cuando se suelta el botón
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
        
        //BOTONES IMPORTANTES ( PRE _ RX _ R )
        private void btnPRE_Click(object sender, EventArgs e)
        {
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string soundFilePath = Path.Combine(appDirectory, "Resources", "preparando.wav");

            try
            {
                // Imprimir el path absoluto para depuración
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
                // Imprimir el path absoluto para depuración
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

        //Botón para cambiar el Filamento
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

        private void btnFoco_large_Click(object sender, EventArgs e)/*(LARGE)*/
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

        private void MainRayX_FormClosing(object sender, FormClosingEventArgs e)
        {
            sMonitor.CerrarSerialPort();
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
           sMonitor.senDataSerial("l-");
           //Thread.Sleep(89);
        }

        private void btnDownKv_MouseDown(object sender, MouseEventArgs e)
        {
            aumentando = true; // Indica que se debe seguir decrementando el valor
            sMonitor.senDataSerial("l+"); // Enviar comando para iniciar ("l+")
            decreaseTimer.Start(); // Iniciar el timer para decrementar el valor
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
            aumentando = false; // Detiene el decremento del valor
            sMonitor.senDataSerial("l-"); // Enviar comando para detener ("l-")
            decreaseTimer.Stop(); // Detener el timer
        }



        private void DecreaseTimer_Tick(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(lblKVp.Text))
            {
                int valorActual = int.Parse(lblKVp.Text); // Obtiene el valor actual
                valorActual--; // Decrementa el valor
                lblKVp.Text = valorActual.ToString(); // Actualiza el texto

                // Asegurarse de que el valor no sea menor que un mínimo establecido, por ejemplo, 40
                if (valorActual < 40)
                {
                    valorActual = 40;
                    lblKVp.Text = valorActual.ToString();
                }
            }
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
            sMonitor.senDataSerial("r+");
            //Thread.Sleep(89);
        }

        private void btnUpKv_MouseDown(object sender, MouseEventArgs e)
        {
            aumentando = true; // Indica que se debe seguir aumentando el valor
            // Inicia un bucle que aumenta continuamente el valor mientras el botón está pulsado
            sMonitor.senDataSerial("r+");
            increaseTimer.Start();
        }

        private void btnUpKv_MouseUp(object sender, MouseEventArgs e)
        {
            aumentando = false; // Detiene el aumento del valor
            sMonitor.senDataSerial("r-"); // Enviar comando para detener ("r-")
            increaseTimer.Stop(); // Detener el timer
        }

        private void lblKVp_Click(object sender, EventArgs e)
        {

        }

        private void IncreaseTimer_Tick(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(lblKVp.Text))
            {
                int valorActual = int.Parse(lblKVp.Text); // Obtiene el valor actual
                valorActual++; // Incrementa el valor
                lblKVp.Text = valorActual.ToString(); // Actualiza el texto
            }
        }


    }
}
