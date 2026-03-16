using RayPro.Aplicaciones.tools;

using RayPro.Vista;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;


namespace RayPro
{
    public partial class MainRayX : Form
    {

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);
        private Action valorCambiaAction;

        private int kv = 40, mAs = 8, indiceImgNow = 0;
        private bool _rightPressed = false;
        private bool _leftPressed = false;
        private double getTiempo;
        /*Se cambia a Large, estado para ser = True, si cambia a Small va ser estado = False*/
        private bool estadoFoco, NoExecute = false;

        private HumanSupport hSupport;
        private Size originalSize;
        private Dictionary<Control, Rectangle> originalControls = new Dictionary<Control, Rectangle>();
        private Dictionary<Control, Font> originalFonts = new Dictionary<Control, Font>();

        public MainRayX()
        {
            InitializeComponent();
            InitFirstParametros();
            ControlCambioFlechas();
            this.DoubleBuffered = true;
            imgLogo.Visible = false;    

        }



        private void InitFirstParametros()
        {
            showPartsRx.Image = lstPartHuman.Images[indiceImgNow];
            showSecuenciaRx.Image = lstSecuenciaRx.Images[0];
            lblmAs.Text = "0" + mAs;
            mAs = 8;
            lblKVp.Text = kv.ToString();

            SetControlsEnabled(false);
            WireEvents();
            WireBodyButtons();

            hSupport = new HumanSupport(cboProyeccion, cboEstructura, lblKVp, lblmAs);
        }


        private void SetControlsEnabled(bool status)
        {
            Control[] controls =
                {
                btnPRE,btnRX,btnR,btnFilamento,panelShow};

            foreach (var control in controls)
            {
                control.Enabled = status;
            }
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

        private void mensajeDeError(string msge, Color setColor)
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
            AppSession.Usb.VoltageReceived += OnVoltageReceived; // ← evento específico del 
        }

        private void OnConnectionChanged(bool connected)
        {
            string msg = connected
                ? "Equipo conectado correctamente"
                : "Equipo desconectado";

            Color color = connected ? Color.LimeGreen : Color.OrangeRed;
            mensajeDeError(msg, color);
        }

        private void OnErrorOccurred(string error)
        {
            mensajeDeError("No se pudo establecer conexión con el equipo 400", Color.OrangeRed);
        }

        private void SendCommand(string command)
        {
            if (!AppSession.Usb.IsConnected)
            {

                mensajeDeError("Equipo No conectado -> Error 401!", Color.OrangeRed);
                return;
            }

            AppSession.Usb.Send(command);
        }

        // ✅ DESPUÉS — limpio y directo
        private void OnVoltageReceived(int voltaje, DateTime timestamp)
        {
            // Ya llega como entero redondeado directo desde UsbCdcManager
            // 35.5 → 36 ✅   35.4 → 35 ✅
            lblKVp.Text = voltaje.ToString();
        }

        #endregion

        private void btnClose_Click(object sender, EventArgs e)
        {
            if (lblEncender.Text == "ON" && btnON.Visible == true)
            {
                QuestionBox.Show("Por favor apague el equipo", "Advertencia", MessageBoxButtons.YesNo);
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
        /*private void CambiarKv(int value)
        {
            int newKv = kv + value;
            if (newKv >= 40 && newKv <= 110)
            {
                kv = newKv;
                lblKVp.Text = kv.ToString();
            }
        }*/
        /*AQUÍ LA FUNCION DE LOS CAMBIOS CON LOS NUMEROS EN MAS*/
        private void CambiarMaS(int value)
        {
            int newMaS = mAs + value;
            if (newMaS >= 1 && newMaS <= 300)
            {
                mAs = newMaS;

                lblmAs.Text = hSupport.getZeroStr_mAs(mAs);
            }
        }
        #endregion

        #region EVENTOS DE BOTONES ENSENCIALES DEL SOFTWARE RX
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
                (btnMano,   3),
                (btnTorax,   4),
                (btnAbdomen, 5),
                (btnPelvis,  6),
                (btnFemur,   7),
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

            if (index < 0 || index >= lstPartHuman.Images.Count) return;

            indiceImgNow = hSupport.getImgInicial(index);
            showPartsRx.Image = lstPartHuman.Images[indiceImgNow];

            var valores = hSupport.showBodyRayX(index);

            mAs = valores.mas;

            lblmAs.Text = hSupport.getZeroStr_mAs(mAs);

        }
        #endregion


        private void btnOFF_Click(object sender, EventArgs e)
        {

            btnOFF.Visible = false;
            btnON.Visible = true;
            lblEncender.Text = "ON";
            lblEncender.ForeColor = Color.LimeGreen;


            SetControlsEnabled(true);

            SendCommand("ON");

        }

        private void btnON_Click(object sender, EventArgs e)
        {
            btnOFF.Visible = true;
            btnON.Visible = false;
            lblEncender.Text = "OFF";
            lblEncender.ForeColor = Color.Brown;

            SetControlsEnabled(false);

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
                hSupport.PlaySoundRx("NoRespirar");
            }
            else
            {
                hSupport.PlaySoundRx("preparando");
            }

            visualBtnRx(false);

            SendCommand("PRE");

            Thread.Sleep(3500);
            //cambio de imagen para mostrar la secuencia de disparo
            hSupport.PlaySoundRx("ready");
            lblFoco.Text = "LISTO";
            showSecuenciaRx.Image = lstSecuenciaRx.Images[2];


            getTiempo = hSupport.sendTimeInput(mAs);

            string sendFactors = getTiempo + "T";

            SendCommand(sendFactors);

        }

        private void btnRX_Click(object sender, EventArgs e)
        {
            if (!NoExecute)
                return;

            hSupport.PlaySoundRx("disparo");

            SendCommand("RX");
            Thread.Sleep(3000);

            if (cboEstructura.Text == "TORÁX")
            {
                hSupport.PlaySoundRx("Respirar");
            }


            visualBtnRx(true);
            lblFoco.Text = (!estadoFoco) ? "SMALL" : "LARGE";//Si esta activo el Foco Large, Es True pero se convierte a Falso y muestra LARGE Actual
            showSecuenciaRx.Image = (!estadoFoco) ? lstSecuenciaRx.Images[0] : lstSecuenciaRx.Images[1];

        }

        private void btnR_Click(object sender, EventArgs e)/*(RESETEAR)*/
        {
            if (!NoExecute)
                return;

            Thread.Sleep(500);

            visualBtnRx(true);
            lblFoco.Text = (!estadoFoco) ? "SMALL" : "LARGE";//Si esta activo el Foco Large, Es True pero se convierte a Falso y muestra LARGE Actual
            showSecuenciaRx.Image = (!estadoFoco) ? lstSecuenciaRx.Images[0] : lstSecuenciaRx.Images[1];
            hSupport.showBodyRayX(0);
            SendCommand("RESET");
        }


        private void btnFilamento_Click(object sender, EventArgs e)
        {
            if (lblFoco.Text == "SMALL")
            {
                SendCommand("FILA");
                Thread.Sleep(2000);
                lblFoco.Text = "LARGE";
                estadoFoco = true;
                showSecuenciaRx.Image = lstSecuenciaRx.Images[1];
                SendCommand("LARG");
            }
            else
            {
                SendCommand("FILA");
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
            originalSize = this.ClientSize;
            SaveControlBounds(this);

            WindowState = FormWindowState.Maximized;

            lblKVp.AutoSize = true;
            lblFoco.AutoSize = true;
            lblmAs.AutoSize = true;
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




        #region FRONT PARA ADAPTARSE A PANTALLAS, ESCALABLE

        private void cboEstructura_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectEstructura = cboEstructura.SelectedItem.ToString();
            hSupport.changeShowCboProy(selectEstructura);
        }

        private void SaveControlBounds(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                originalControls[c] = new Rectangle(c.Location, c.Size);
                originalFonts[c] = c.Font;

                if (c.Controls.Count > 0)
                    SaveControlBounds(c);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            // 🔥 evitar escalado cuando está minimizado
            if (WindowState == FormWindowState.Minimized)
                return;

            if (originalSize.Width == 0 || originalSize.Height == 0)
                return;

            float xRatio = (float)this.ClientSize.Width / originalSize.Width;
            float yRatio = (float)this.ClientSize.Height / originalSize.Height;

            ResizeControls(this, xRatio, yRatio);

            setPanelBorders();
        }

        private void ResizeControls(Control parent, float xRatio, float yRatio)
        {
            foreach (Control c in parent.Controls)
            {
                if (!originalControls.ContainsKey(c))
                    continue;

                Rectangle r = originalControls[c];

                c.Location = new Point((int)(r.X * xRatio), (int)(r.Y * yRatio));
                c.Size = new Size((int)(r.Width * xRatio), (int)(r.Height * yRatio));

                // 🔥 ESCALAR FUENTE DESDE EL VALOR ORIGINAL
                if (originalFonts.ContainsKey(c))
                {
                    float newFontSize = originalFonts[c].Size * yRatio;

                    if (newFontSize < 1)
                        newFontSize = 1;

                    c.Font = new Font(originalFonts[c].FontFamily, newFontSize, originalFonts[c].Style);
                }

                if (c.Controls.Count > 0)
                    ResizeControls(c, xRatio, yRatio);
            }
        }
        #endregion


        #region Cierre para desconectar USB
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            AppSession.Usb.ConnectionChanged -= OnConnectionChanged;
            AppSession.Usb.ErrorOccurred -= OnErrorOccurred;

            AppSession.Usb.VoltageReceived -= OnVoltageReceived; // ← correcto
            AppSession.Usb?.Dispose(); // Dispose es más completo que solo Disconnect
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