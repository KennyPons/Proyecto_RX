using RayPro.Aplicaciones.tools;
using RayPro.configuraciones;
using RayPro.Vista;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
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
        private bool estadoFoco, NoExecute = false;

        private HumanSupport hSupport;
        private Size originalSize;
        private Dictionary<Control, Rectangle> originalControls = new Dictionary<Control, Rectangle>();
        private Dictionary<Control, Font> originalFonts = new Dictionary<Control, Font>();

        /* ⚠️ NUEVO: monitoreo de salud del ESP32 */
        private System.Windows.Forms.Timer _healthTimer;
        private static readonly TimeSpan FrozenThreshold = TimeSpan.FromSeconds(10); // sin PING/VAC en 10s = congelado
        private static readonly TimeSpan VoltageTimeout = TimeSpan.FromSeconds(3);   // sin VAC tras activar relé = error 101
        private DateTime? _relayActivatedAt = null;   // momento en que se activó DER/IZQ
        private bool _voltageReceivedSinceRelay = false;
        private bool _reconnectingByHealth = false;   // evita reconexiones forzadas superpuestas

        public MainRayX()
        {
            InitializeComponent();
            InitFirstParametros();
            ControlCambioFlechas();
            DoubleBuffered = true;
        }

        private void InitFirstParametros()
        {
            showPartsRx.Image = lstPartHuman.Images[indiceImgNow];
            showSecuenciaRx.Image = lstSecuenciaRx.Images[0];
            lblHospital.Text = Settings.Default.NameHospital;
            lblmAs.Text = "0" + mAs;
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

        private void SetFlechasEnabled(bool status)
        {
            btnUpKv.Enabled = status;
            btnDownKv.Enabled = status;
            btnUpMaS.Enabled = status;
            btnDownMaS.Enabled = status;
            btnFilamento.Enabled = status;
            btnON.Visible = status;
            btnOFF.Visible = status;
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
            lblErrorMsg.ForeColor = setColor; // ⚠️ FIX: antes ignoraba setColor y siempre usaba OrangeRed
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
            AppSession.Usb.VoltageReceived += OnVoltageReceived;
        }

        private void OnConnectionChanged(bool connected)
        {
            string msg = connected
                ? "CONEXIÓN CORRECTA"
                : "ERROR DE CONEXIÓN - FAILED TARGET";

            Color color = connected ? Color.LimeGreen : Color.OrangeRed;
            mensajeDeError(msg, color);
        }

        private void OnErrorOccurred(string error)
        {
            /* ⚠️ NUEVO: distinguir el tipo de error real, en vez de un mensaje genérico fijo */
            if (!string.IsNullOrEmpty(error) &&
                error.IndexOf("Voltaje inválido", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                mensajeDeError("ERROR 101 INPUT VOLTAJE", Color.Gold);
                return;
            }

            mensajeDeError("ERROR DE CONEXIÓN - FAILED TARGET", Color.OrangeRed);
        }

        private void SendCommand(string command)
        {
            if (!AppSession.Usb.IsConnected)
            {
                mensajeDeError("Equipo No Conectado - Error 401!", Color.OrangeRed);
                return;
            }

            /* ⚠️ NUEVO: registrar cuándo se activa un relé de medición de voltaje,
               para poder detectar si nunca llega el VAC correspondiente */
            if (command == "DER_ON" || command == "IZQ_ON")
            {
                _relayActivatedAt = DateTime.UtcNow;
                _voltageReceivedSinceRelay = false;
            }
            else if (command == "DER_OFF" || command == "IZQ_OFF")
            {
                _relayActivatedAt = null;
            }

            AppSession.Usb.Send(command);
        }

        private void OnVoltageReceived(int voltaje, DateTime timestamp)
        {
            _voltageReceivedSinceRelay = true; // ⚠️ NUEVO: confirma que sí está llegando voltaje

            int voltajeConfigurado = Settings.Default.VoltageOffset;
            int aumentarVoltaje = voltaje + voltajeConfigurado;
            lblKVp.Text = aumentarVoltaje.ToString();
        }

        #endregion

        #region ⚠️ NUEVO: Monitoreo de salud del ESP32 (congelamiento / sin voltaje / sin conexión)

        private void StartHealthMonitor()
        {
            _healthTimer = new System.Windows.Forms.Timer { Interval = 1000 }; // revisa cada 1s
            _healthTimer.Tick += HealthTimer_Tick;
            _healthTimer.Start();
        }

        private async void HealthTimer_Tick(object sender, EventArgs e)
        {
            // 1) ¿El ESP32 dejó de responder por completo? (ni PING ni VAC en el umbral)
            if (AppSession.Usb.IsFrozen(FrozenThreshold) && !_reconnectingByHealth)
            {
                _reconnectingByHealth = true;
                mensajeDeError("ERROR COLD MICROCONTROLADOR", Color.OrangeRed);

                await ForzarReconexionAsync();

                _reconnectingByHealth = false;
                return;
            }

            // 2) ¿Se activó un relé de voltaje y no llegó ningún VAC dentro del timeout?
            if (_relayActivatedAt.HasValue &&
                !_voltageReceivedSinceRelay &&
                (DateTime.UtcNow - _relayActivatedAt.Value) > VoltageTimeout)
            {
                mensajeDeError("ERROR 101 INPUT VOLTAJE", Color.Gold);
                _relayActivatedAt = null; // evita repetir el mensaje en bucle
            }
        }

        /// <summary>
        /// ⚠️ NUEVO: Reconexión forzada por software — equivale a "sacar y meter el USB"
        /// pero sin que el usuario tenga que tocar el cable físicamente.
        /// </summary>
        private async Task ForzarReconexionAsync()
        {
            await Task.Run(() =>
            {
                AppSession.Usb.Disconnect();
            });

            await Task.Delay(800);

            bool ok = await Task.Run(() => AppSession.Usb.Connect());

            if (!ok)
            {
                mensajeDeError("ERROR DE CONEXIÓN - FAILED TARGET", Color.OrangeRed);
            }
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

        #region Eventos para cambiar los datos de KV y MaS
        private void ControlCambioFlechas()
        {
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
            action();
        }

        private void stopValorChange()
        {
            changeTimer.Stop();
            valorCambiaAction = null;
        }

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
        private void WireBodyButtons()
        {
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


        private async void btnOFF_Click(object sender, EventArgs e)
        {
            btnOFF.Visible = false;
            btnON.Visible = true;
            lblEncender.Text = "ON";
            lblEncender.ForeColor = Color.LimeGreen;

            SetControlsEnabled(true);

            SendCommand("ON");
        }

        private async void btnON_Click(object sender, EventArgs e)
        {
            /* ⚠️ NUEVO: si la conexión está mala al querer encender,
               forzar reconexión automática por software ANTES de mandar OFF */
            if (!AppSession.Usb.IsConnected || AppSession.Usb.IsFrozen(FrozenThreshold))
            {
                mensajeDeError("ERROR DE CONEXIÓN - FAILED TARGET", Color.OrangeRed);
                await ForzarReconexionAsync();

                if (!AppSession.Usb.IsConnected)
                {
                    // Sigue sin conectar tras el intento automático: no continuar con el apagado/encendido
                    return;
                }
            }

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
            hSupport.PlaySoundRx("ready");
            lblFoco.Text = "LISTO";
            showSecuenciaRx.Image = lstSecuenciaRx.Images[2];
            SetFlechasEnabled(false);

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

            SetFlechasEnabled(true);
            visualBtnRx(true);
            lblFoco.Text = (!estadoFoco) ? "SMALL" : "LARGE";
            showSecuenciaRx.Image = (!estadoFoco) ? lstSecuenciaRx.Images[0] : lstSecuenciaRx.Images[1];
        }

        private void btnR_Click(object sender, EventArgs e)
        {
            if (!NoExecute)
                return;

            Thread.Sleep(500);

            visualBtnRx(true);
            lblFoco.Text = (!estadoFoco) ? "SMALL" : "LARGE";
            showSecuenciaRx.Image = (!estadoFoco) ? lstSecuenciaRx.Images[0] : lstSecuenciaRx.Images[1];
            hSupport.showBodyRayX(0);
            SendCommand("RESET");
            SetFlechasEnabled(true);
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
            LoggerManager.CleanOldLogs(30);
            AppSession.Usb.TryAutoConnect();

            StartHealthMonitor(); // ⚠️ NUEVO: inicia el monitoreo continuo de salud

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
            _healthTimer?.Stop();          // ⚠️ NUEVO: detener el monitor al cerrar
            _healthTimer?.Dispose();

            AppSession.Usb.ConnectionChanged -= OnConnectionChanged;
            AppSession.Usb.ErrorOccurred -= OnErrorOccurred;
            AppSession.Usb.VoltageReceived -= OnVoltageReceived;
            AppSession.Usb?.Dispose();
            base.OnFormClosing(e);
        }
        #endregion

        private void MainRayX_FormClosing(object sender, FormClosingEventArgs e)
        {
        }

        //////FIN DE SOFTWARE/////

    }
}