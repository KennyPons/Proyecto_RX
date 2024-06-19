using RayPro.Aplicaciones;
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

namespace RayPro.Vista
{
    public partial class FrQuestion : Form
    {
        private loginController objLogin;
        public FrQuestion()
        {
            InitializeComponent();
            objLogin = new loginController();
            txtPassDev.Text = "Password...";
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Login frLogin = new Login();
            frLogin.Show();
            this.Close();
        }

        private void btnAccess_Click(object sender, EventArgs e)
        {
            if (txtPassDev.Text != "")
            {
                if (objLogin.AutenticarAdmin("", txtPassDev.Text))
                {
                    SettingDev frDev = new SettingDev();
                    frDev.ShowDialog();
                    this.Close();
                }
                else
                {
                    mensajeDeError("You don't have access to the Advanced Settings");
                    limpiar();
                }
            }
            else
            {
                mensajeDeError("Enter Username or Password, Empty Fields...");
                limpiar();
            }
        }


        //methods================================================================================/

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
            txtPassDev.Clear();
            txtPassDev.Focus();
        }

        private void txtPassDev_Enter(object sender, EventArgs e)
        {
            if (txtPassDev.Text.Equals("Password..."))
            {
                txtPassDev.Text = "";
                txtPassDev.ForeColor = Color.DimGray;
                txtPassDev.UseSystemPasswordChar = true;
            }
        }

        private void txtPassDev_Leave(object sender, EventArgs e)
        {
            if (txtPassDev.Text == "")
            {
                txtPassDev.Text = "Password...";
                txtPassDev.ForeColor = Color.DimGray;
                txtPassDev.UseSystemPasswordChar = true;
            }
        }



        //======================================================================================/
    }
}
