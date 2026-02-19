using RayPro.Aplicaciones.tools;
using RayPro.configuraciones;
using RayPro.Vista;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RayPro.Aplicaciones
{
    public partial class SettingDev : Form
    {

        private UsbCdcManager _usb => AppSession.Usb; //Una sola instancia, no se debe colocar otra instancia de UsbCdcManager, si no no guarda los datos

        public SettingDev()
        {
            InitializeComponent();
            inicializandoComponentes();
            initAccountSettings();
        }



        //===============================METHODS=========================================================
        private void inicializandoComponentes()
        {
            WireEvents();
            LoadCombos();
            Rx_txt.AppendText("En Espera...");
        }

        private void mensajeDeError(String msge)
        {
            Timer temporizador = new Timer();
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

        private void clearText()
        {
            txtUsuario.Clear();
            txtPassAnt.Clear();
            //txtPassNew.Clear();
            txtUsuario.Focus();

        }

        #region CONECTAR CON EL DISPOSITIVO USB
        private void WireEvents()
        {
            _usb.ConnectionChanged += OnConnectionChanged;
            _usb.DataReceived += OnDataReceived;
            _usb.ErrorOccurred += OnErrorOccurred;
        }

        private void UnwireEvents()
        {
            _usb.ConnectionChanged -= OnConnectionChanged;
            _usb.DataReceived -= OnDataReceived;
            _usb.ErrorOccurred -= OnErrorOccurred;
        }

        //Implementación de los handlers:
        private void OnConnectionChanged(bool connected)
        {
            btnConnect.Text = connected ? "Disconnect" : "Connect";
            lblMensaje.Text = connected ? "Conexión correcta" : "Desconectado";
            lblMensaje.Visible = true;

            // Limpiar RX al conectar para empezar limpio
            if (connected)
            {
                Rx_txt.Clear();
                Rx_txt.AppendText("Conectado. Esperando datos..." + Environment.NewLine);
            }
        }

        private void OnDataReceived(string data)
        {
            // Acumular datos como Hercules (no borrar cada vez)
            if (Rx_txt.TextLength > 32000)
            {
                Rx_txt.Clear();
            }
            Rx_txt.AppendText(data + Environment.NewLine);
        }

        private void OnErrorOccurred(string error)
        {
            mensajeDeError(error);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            UnwireEvents();
            base.OnFormClosing(e);
        }

        #endregion
        // Cargar combos de configuración
        private void LoadCombos()
        {
            cboCom.Items.Clear();
            cboCom.Items.AddRange(UsbCdcManager.GetPorts());

            cboBaudios.Items.Clear();
            foreach (var b in UsbCdcManager.GetBaudRates())
                cboBaudios.Items.Add(b);

            // Restaurar settings guardados
            if (!string.IsNullOrEmpty(Settings.Default.ComPortName))
                cboCom.SelectedItem = Settings.Default.ComPortName;

            if (Settings.Default.Baudios > 0)
                cboBaudios.SelectedItem = Settings.Default.Baudios;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (_usb.IsConnected)
            {
                _usb.Disconnect();
                return;
            }

            if (cboCom.SelectedItem == null || cboBaudios.SelectedItem == null)
            {
                lblMensaje.Text = "Seleccione Compuerta y Baudio";
                lblMensaje.Visible = true;
                return;
            }

            _usb.Configure(
                cboCom.SelectedItem.ToString(),
                Convert.ToInt32(cboBaudios.SelectedItem),
                autoConnect: false
            );

            _usb.Connect();

        }



        /*ACCOUNT*/

        private void initAccountSettings()
        {
            cboOffset.Items.Clear();
            for (int i = 0; i <= 6; i++)
                cboOffset.Items.Add(i);
            cboOffset.SelectedItem = AppSession.Usb?.VoltageOffset ?? 2;
        }

        //========================================================================================

        #region BOTONES DE MINIMIZAR Y CERRAR
        private void btn_mini_picbox_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void btn_cerrar_picbox_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            Login frLogin = new Login();
            frLogin.Show();
            Close();
        }

        #endregion
        private void btnReseteo_Click(object sender, EventArgs e)
        {
            LoadCombos();
            lblMensaje.Text = "Lista de puertos actualizada";
        }

        

        private void btnSend_Click_1(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Tx_txt.Text))
                return;

            _usb.Send(Tx_txt.Text);
            Tx_txt.Clear();
        }
        #region BUTTONS PARA GUARDAR CONFIGURACIONES DE LAS 3 PESTAÑAS
        private void btnGuardarAccount_Click(object sender, EventArgs e)
        {
            if (cboOffset.SelectedItem == null) return;

            // Leer y validar
            int value;
            if (!int.TryParse(cboOffset.SelectedItem.ToString(), out value)) return;
            if (value < 0) value = 0;
            if (value > 6) value = 6;

            // Aplicar a la instancia del manager (si existe)
            if (AppSession.Usb != null)
            {
                AppSession.Usb.VoltageOffset = value;
            }

            // Guardar en Settings (opcional pero recomendado)
            Settings.Default.VoltageOffset = value;
            Settings.Default.Save();
        }


        private void btnSaveUsb_Click(object sender, EventArgs e)
        {
            if (cboCom.SelectedItem == null || cboBaudios.SelectedItem == null)
            {
                lblMensaje.Text = "No hay configuración válida";
                return;
            }

            _usb.Configure(
                cboCom.SelectedItem.ToString(),
                Convert.ToInt32(cboBaudios.SelectedItem),
                autoConnect: true
            );

            QuestionBox.Show("Configuración guardada correctamente.", "Configuración", MessageBoxButtons.YesNo);
        }

        private void btnSaveLicencse_Click(object sender, EventArgs e)
        {

        }



        #endregion
        //////////////////////////////////////////////////////////////////////////////
    }
}
