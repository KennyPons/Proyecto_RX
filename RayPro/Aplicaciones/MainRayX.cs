using RayPro.Aplicaciones;
using RayPro.Aplicaciones.tools;
using RayPro.Persistencia.db;
using RayPro.Vista;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Media;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

using System.Timers;

namespace RayPro
{
    public partial class MainRayX : Form
    {

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);
        private Action valorCambiaAction;

        private int kv = 40, mAs = 1, indiceImgNow = 0;
        private bool _rightPressed = false;
        private bool _leftPressed = false;
        private double getTiempo;
        /*Se cambia a Large, estado para ser = True, si cambia a Small va ser estado = False*/
        private bool estadoFoco, NoExecute = false; 

        private HumanSupport hSupport;
        //private SerialPortManager sMonitor;

        public MainRayX()
        {
            InitializeComponent();
            InitFirstParametros();
            ControlCambioFlechas();
   
        }

        

        private void InitFirstParametros()
        {
            setPanelBorders();
            showPartsRx.Image = imgLstBody.Images[indiceImgNow];
            showSecuenciaRx.Image = lstSecuenciaRx.Images[0];
            lblmAs.Text = "0" + mAs;
            lblKVp.Text = kv.ToString();

            enableSystemEvents(false);
            WireEvents();
            WireBodyButtons();

            hSupport = new HumanSupport(cboProyeccion, cboEstructura, lblKVp, lblmAs);
        }


        private void enableSystemEvents(bool status)
        {

            btnPRE.Enabled = status;
            btnRX.Enabled = status;
            btnR.Enabled = status;
            btnFilamento.Enabled = status;
            panelShow.Enabled = status;
        }

        private void parametrosSecuencia(int wight, int high, int pointY)
        {
            showSecuenciaRx.Size = new Size(130, 140);
            showSecuenciaRx.Location = new Point(478, 40);
        }
        private void visualBtnRx(bool status)
        {
            btnPRE.Visible = status;
            btnFilamento.Visible = status;
            NoExecute = !status;
            
        }

        private void setPanelBorders()
        {
            pnlMaS.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlMaS.Width, pnlMaS.Height, 30, 30));
            pnlKvp.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlKvp.Width, pnlKvp.Height, 30, 30));
            panelFoco.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, panelFoco.Width, panelFoco.Height, 26, 26));
            panelShow.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, panelShow.Width, panelShow.Height, 26, 26));
        }

        private void mensajeDeError(String msge)
        {
            System.Windows.Forms.Timer temporizador = new System.Windows.Forms.Timer();
            temporizador.Interval = 5000;

            lblErrorMsg.Text = "   " + msge;
            lblErrorMsg.ForeColor = Color.OrangeRed;
            lblErrorMsg.Visible = true;

            temporizador.Tick += (sender, e) =>
            {
                lblErrorMsg.Visible = false;

                temporizador.Stop();
            };


            temporizador.Start();
        }


        #region COMUNICACION CON EL DISPOSITIVO USB
        private void WireEvents()
        {
            AppSession.Usb.ConnectionChanged += OnConnectionChanged;
            AppSession.Usb.ErrorOccurred += OnErrorOccurred;

            // NUEVO: subscribir DataReceived para actualizar lblKVp con valores numéricos
            AppSession.Usb.DataReceived += OnUsbDataReceived;
        }

        private void OnConnectionChanged(bool connected)
        {
            string msg = connected
                ? "Equipo conectado correctamente"
                : "Equipo desconectado";

            mensajeDeError(msg);
        }

        private void OnErrorOccurred(string error)
        {
            mensajeDeError("No se pudo establecer conexión con el equipo");
        }

        private void SendCommand(string command)
        {
            if (!AppSession.Usb.IsConnected)
            {
  
                mensajeDeError("Equipo no conectado");
                return;
            }

            AppSession.Usb.Send(command);
        }

        private void OnUsbDataReceived(string data)
        {
            // data debería ser SOLO un número como "123" (según UsbCdcManager)
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => OnUsbDataReceived(data)));
                return;
            }

            // sanity: aceptar sólo dígitos (evitar basura)
            if (!string.IsNullOrEmpty(data) && System.Text.RegularExpressions.Regex.IsMatch(data, @"^\d+$"))
            {
                lblKVp.Text = data; // actualiza label con el entero recibido
            }
            // si necesitas mostrar otra info, la puedes procesar aquí
        }

        #endregion

        private void btnClose_Click(object sender, EventArgs e)
        {         
            if(lblEncender.Text == "ON" && btnON.Visible == true)
            {
                QuestionBox.Show("Por favor apague el equipo" , "Advertencia", MessageBoxButtons.YesNo);
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

        #region Eventos para cambiar los datos de KV y MaS (En este caso se usara solo el mAs para mostrar el cambio)
        /*CONTROL DE TIEMPO O SECUENCIAL DE KV Y MAS*/
        private void ControlCambioFlechas()
        {
            /*btnUpKv.MouseDown += (s, e) => startValorChange(() => CambiarKv(1));
            btnUpKv.MouseUp += (s, e) => stopValorChange();
            btnDownKv.MouseDown += (s, e) => startValorChange(() => CambiarKv(-1));
            btnDownKv.MouseUp += (s, e) => stopValorChange();*/

            //Eventos para Kv en Rx Lineal
            btnUpKv.MouseDown += btnUpKv_MouseDown;
            btnUpKv.MouseUp += btnUpKv_MouseUp;
            btnUpKv.MouseLeave += btnUpKv_MouseLeave;

            btnDownKv.MouseDown += btnDownKv_MouseDown;
            btnDownKv.MouseUp += btnDownKv_MouseUp;
            btnDownKv.MouseLeave += btnDownKv_MouseLeave;

            btnUpMaS.MouseDown += (s, e) => startValorChange(() => CambiarMaS(1));
            btnUpMaS.MouseUp += (s, e) => stopValorChange();
            btnDownMaS.MouseDown += (s, e) => startValorChange(() => CambiarMaS(-1));
            btnDownMaS.MouseUp += (s, e) => stopValorChange();
        }


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
         /*AQUÍ LA FUNCION DE LOS CAMBIOS CON LOS NUMEROS EN KV*/
        private void CambiarKv(int value)
        {
            int newKv = kv + value;
            if (newKv >= 40 && newKv <= 110)
            {
                kv = newKv;
                lblKVp.Text = kv.ToString();
            }
        }
        /*AQUÍ LA FUNCION DE LOS CAMBIOS CON LOS NUMEROS EN MAS*/
        private void CambiarMaS(int value)
        {
            int newMaS = mAs + value;
            if (newMaS >= 1 && newMaS <= 300)
            {
                mAs = newMaS;

                lblmAs.Text = hSupport.formatoStrMaS(mAs);
            }
        }
        #endregion

        /// <summary>
        ///////////////// EVENTOS DE BOTONES DE RAYOS X ///////////////////
        /// </summary>


        /// <summary>
        /// Asigna el índice de imagen a cada botón anatómico vía Tag
        /// y conecta un SOLO handler para todos.
        /// </summary>
        private void WireBodyButtons()
        {
            // Tag = índice en imgLstBody  (orden: Craneo=0, Columna=1, Hombro=2, Torax=3, Abdomen=4, Pelvis=5, Femur=6)
            var zonas = new (Button btn, int index)[]
            {
                (btnCraneo,  0),
                (btnColumna, 1),
                (btnHombro,  2),
                (btnTorax,   3),
                (btnAbdomen, 4),
                (btnPelvis,  5),
                (btnFemur,   6),
            };

            foreach (var zona in zonas)
            {
                zona.btn.Tag = zona.index;
                zona.btn.Click += BtnZona_Click;
            }
        }

        /// <summary>
        /// Handler único para todos los botones de zona anatómica.
        /// Lee el índice desde Tag, actualiza imagen y estructura.
        /// </summary>
        private void BtnZona_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            int index = (int)btn.Tag;

            if (index < 0 || index >= imgLstBody.Images.Count) return;

            indiceImgNow = index;
            showPartsRx.Image = imgLstBody.Images[indiceImgNow];
            hSupport.showBodyRayX(indiceImgNow);
        }


    

        private void btnOFF_Click(object sender, EventArgs e)
        {

            btnOFF.Visible = false;
            btnON.Visible = true;
            lblEncender.Text = "ON";
            lblEncender.ForeColor = Color.LimeGreen;
            
            //sMonitor.senDataSerial(lblEncender.Text);
            enableSystemEvents(true);

            SendCommand("ON");

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
            SendCommand("OFF");
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
            SendCommand("PRE");

            Thread.Sleep(3500);
            //cambio de imagen para mostrar la secuencia de disparo
            hSupport.playSoundRx("ready.wav");
            lblFoco.Text = "LISTO";
            showSecuenciaRx.Image = lstSecuenciaRx.Images[2];
            parametrosSecuencia(100, 100, 78);

            getTiempo = hSupport.sendTimeInput(mAs);
            
            string sendFactors = kv + "KV,"+getTiempo+"T";
            //sMonitor.senDataSerial(sendFactors);
            SendCommand(sendFactors);

        }

        private void btnRX_Click(object sender, EventArgs e)
        {
            if(!NoExecute)
                return;

            hSupport.playSoundRx("disparo.wav");
            parametrosSecuencia(130, 140, 40);

            SendCommand("RX");
            Thread.Sleep(3000);

            if (cboEstructura.Text == "TORÁX")
            {
                hSupport.playSoundRx("Respirar.wav");
            }
            

            visualBtnRx(true);
            lblFoco.Text = (!estadoFoco)? "SMALL":"LARGE";//Si esta activo el Foco Large, Es True pero se convierte a Falso y muestra LARGE Actual
            showSecuenciaRx.Image = (!estadoFoco) ? lstSecuenciaRx.Images[0]: lstSecuenciaRx.Images[1];
   
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
            showSecuenciaRx.Image = (!estadoFoco) ? lstSecuenciaRx.Images[0] : lstSecuenciaRx.Images[1];
            hSupport.showBodyRayX(0);
            SendCommand("RESET");
        }


        private void btnFilamento_Click(object sender, EventArgs e)
        {
            if(lblFoco.Text == "SMALL") 
            {
               Thread.Sleep(2000);
               lblFoco.Text = "LARGE";
               estadoFoco = true;
               showSecuenciaRx.Image = lstSecuenciaRx.Images[1];
                SendCommand("LARG");
            }
            else
            {
                Thread.Sleep(2000);
                lblFoco.Text = "SMALL";
                estadoFoco = false;
                showSecuenciaRx.Image = lstSecuenciaRx.Images[0];
                SendCommand("S");
            }
        }

        #region Intentar conectar con el USB
        private void MainRayX_Load(object sender, EventArgs e)
        {
            AppSession.Usb.TryAutoConnect();
        }
        #endregion


        private void changeTimer_Tick(object sender, EventArgs e)
        {
            valorCambiaAction?.Invoke();
        }
        #region Evento Presionar y Soltar para los botones de KV ==> Up y Down
        private void btnUpKv_MouseUp(object sender, MouseEventArgs e)
        {
            if (_rightPressed)
            {
                _rightPressed = false;
                SendCommand("DER_OFF");
            }
        }

        private void btnUpKv_MouseDown(object sender, MouseEventArgs e)
        {
            if (!_rightPressed)
            {
                _rightPressed = true;
                SendCommand("DER_ON");
            }
        }

        private void btnDownKv_MouseUp(object sender, MouseEventArgs e)
        {
            if (_leftPressed)
            {
                _leftPressed = false;
                SendCommand("IZQ_OFF");
            }
        }

        private void btnDownKv_MouseDown(object sender, MouseEventArgs e)
        {
            if (!_leftPressed)
            {
                _leftPressed = true;
                SendCommand("IZQ_ON");
            }
        }
        private void btnUpKv_MouseLeave(object sender, EventArgs e)
        {
            // si arrastras fuera mientras presionas, aseguramos el OFF
            if (_rightPressed)
            {
                _rightPressed = false;
                SendCommand("DER_OFF");
            }
        }

        private void btnDownKv_MouseLeave(object sender, EventArgs e)
        {
            if (_leftPressed)
            {
                _leftPressed = false;
                SendCommand("IZQ_OFF");
            }
        }
        #endregion Final de eventos para los botones de KV




        private void cboEstructura_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectEstructura = cboEstructura.SelectedItem.ToString();
            hSupport.changeShowCboProy(selectEstructura);
        }

        #region Cierre para desconectar USB
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            AppSession.Usb.DataReceived -= OnUsbDataReceived;
            AppSession.Usb?.Disconnect();
            base.OnFormClosing(e);

        }
        #endregion

        private void MainRayX_FormClosing(object sender, FormClosingEventArgs e)
        {
            //sMonitor.CerrarSerialPort();
        }

        //////FIN DE SOFTWARE/////

    }
}