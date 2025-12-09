using RayPro.Persistencia;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace RayPro.Vista
{
    public partial class Login : Form
    {

        private loginController objLogin;
        public Login()
        {
            InitializeComponent();
            objLogin = new loginController();
            // Habilitar la propiedad KeyPreview para capturar las pulsaciones de teclas en el formulario
            this.KeyPreview = true;

            // Suscribir el evento KeyDown para manejar las pulsaciones de teclas
            this.KeyDown += Login_KeyDown;
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnMinimizar_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void txtUsuario_Enter(object sender, EventArgs e)
        {
            if (txtUsuario.Text.Equals("Usuario"))
            {
                txtUsuario.Text = "";
                txtUsuario.ForeColor = Color.DimGray;
            }
        }

        private void txtUsuario_Leave(object sender, EventArgs e)
        {
            if (txtUsuario.Text == "")
            {
                txtUsuario.Text = "Usuario";
                txtUsuario.ForeColor = Color.DimGray;
            }
        }

        private void textBox2_Enter(object sender, EventArgs e)
        {
            if (txtPassword.Text.Equals("Contraseña"))
            {
                txtPassword.Text = "";
                txtPassword.ForeColor = Color.DimGray;
                txtPassword.UseSystemPasswordChar = true;
            }
        }

        private void textBox2_Leave(object sender, EventArgs e)
        {
            if (txtPassword.Text == "")
            {
                txtPassword.Text = "Contraseña";
                txtPassword.ForeColor = Color.DimGray;
                txtPassword.UseSystemPasswordChar = false;
            }
        }

        private void txtUsuario_KeyDown(object sender, KeyEventArgs e)
        {
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
        }

        private void btnAcceder_Click(object sender, EventArgs e)
        {
           
          InitializationLoginSystem();
        }

        private void linkSetting_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            FrQuestion question = new FrQuestion();
            question.ShowDialog();
            Hide();
        }



        //=====================================methods==========================================================//

        private void mensajeDeError(String msge)
        {
            Timer temporizador = new Timer();
            temporizador.Interval = 5000;

            lblErrorMsg.Text = "   " + msge;
            lblErrorMsg.ForeColor = Color.OrangeRed;
            lblErrorMsg.Visible = true;

            temporizador.Tick += (sender, e) =>
            {
                // Oculta el labelText cuando el temporizador alcance los 5 segundos
                lblErrorMsg.Visible = false;

                // Detiene el temporizador
                temporizador.Stop();
            };

            // Inicia el temporizador
            temporizador.Start();
        }

        private void limpiar()
        {
            txtUsuario.Clear();
            txtPassword.Clear();
            txtUsuario.Focus();
        }


        private void InitializationLoginSystem()
        {
            if (txtUsuario.Text != "" && txtPassword.Text != "")
            {
                if (objLogin.AutenticarUsuario(txtUsuario.Text, txtPassword.Text))
                {
                    configuraciones.Settings.Default.userName = txtUsuario.Text;
                    configuraciones.Settings.Default.PassTemp = txtPassword.Text;
                    configuraciones.Settings.Default.Save();
                    Welcome frWelcome = new Welcome();
                    frWelcome.ShowDialog();
                    MainRayX frMain = new MainRayX();
                    frMain.Show();
                    this.Hide();
                }
                else
                {
                    mensajeDeError("Error de Authentificación!");
                    limpiar();
                }
            }
            else
            {
                mensajeDeError("Ingrese Usuario o Contraseña, Campos Vacíos...");
                limpiar();
            }
        }
 //===================================================================================================================//
        private void Login_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
            {
                txtUsuario.Focus();
            }
            else if (e.KeyCode == Keys.Down)
            {
                txtPassword.Focus();
            }
            else if (e.KeyCode == Keys.Enter)
            {
                btnAcceder.PerformClick();
            }
        }

        private void Login_Load(object sender, EventArgs e)
        {

        }


    }
}
    

