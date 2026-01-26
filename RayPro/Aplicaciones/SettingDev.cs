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

        private UsbCdcManager _usb;

        public SettingDev()
        {
            InitializeComponent();
            inicializandoComponentes();
            // bits de datos
            //string[] bitsDatos = { "8", "7", "otros..." };
            // paridad
            //string[] paridades = { "None", "Odd", "Even", "otros..." };
            
        }



        //===============================METHODS=========================================================
        private void inicializandoComponentes()
        {
            _usb = new UsbCdcManager();
            WireEvents();
            LoadCombos();
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

        private void reset()
        {
            txtUsuario.Clear();
            txtPassAnt.Clear();
            txtPassNew.Clear();
            txtUsuario.Focus();

        }

        private void WireEvents()
        {
            _usb.ConnectionChanged += OnConnectionChanged;
            _usb.DataReceived += OnDataReceived;
            _usb.ErrorOccurred += OnErrorOccurred;
        }

        //Implementación de los handlers:
        private void OnConnectionChanged(bool connected)
        {
            btnConectado.Text = connected ? "Disconnect" : "Connect";
            lblMensaje.Text = connected ? "Conexión correcta" : "Desconectado";
        }

        private void OnDataReceived(string data)
        {
            Rx_txt.AppendText(data + Environment.NewLine);
        }

        private void OnErrorOccurred(string error)
        {
            lblMensaje.Text = error;
        }

        // Cargar combos de configuración
        private void LoadCombos()
        {
            cboComp.Items.Clear();
            cboComp.Items.AddRange(UsbCdcManager.GetPorts());

            cboBaud.Items.Clear();
            foreach (var b in UsbCdcManager.GetBaudRates())
                cboBaud.Items.Add(b);

            // Restaurar settings guardados
            if (!string.IsNullOrEmpty(Settings.Default.ComPortName))
                cboComp.SelectedItem = Settings.Default.ComPortName;

            if (Settings.Default.BaudRate > 0)
                cboBaud.SelectedItem = Settings.Default.BaudRate;
        }


        //========================================================================================
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnMinimizar_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Login frLogin = new Login();
            frLogin.Show();
            this.Close();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            reset();
        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (cboComp.SelectedItem == null || cboBaud.SelectedItem == null)
            {
                lblMensaje.Text = "No hay configuración válida";
                return;
            }

            _usb.Configure(
                cboComp.SelectedItem.ToString(),
                Convert.ToInt32(cboBaud.SelectedItem),
                autoConnect: true
            );

            lblMensaje.Text = "Configuración guardada correctamente";
        }

        private void btnRst_Click(object sender, EventArgs e)
        {
            LoadCombos();
            lblMensaje.Text = "Lista de puertos actualizada";
        }

        private void btnConectado_Click(object sender, EventArgs e)
        {
            if (_usb.IsConnected)
            {
                _usb.Disconnect();
                return;
            }

            if (cboComp.SelectedItem == null || cboBaud.SelectedItem == null)
            {
                lblMensaje.Text = "Seleccione Compuerta y Baudio";
                return;
            }

            _usb.Configure(
                cboComp.SelectedItem.ToString(),
                Convert.ToInt32(cboBaud.SelectedItem),
                autoConnect: false
            );

            _usb.Connect();
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Tx_txt.Text))
                return;

            _usb.Send(Tx_txt.Text);
        }

        //////////////////////////////////////////////////////////////////////////////
    }
}
