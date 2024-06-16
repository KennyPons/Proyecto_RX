﻿using RayPro.Persistencia;
using RayPro.Persistencia.db;
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

        private loginController objLog;
        private BDExcell handerExcell;
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
            objLog = new loginController();   
            string[] puertos = SerialPort.GetPortNames();
            cboComp.Items.AddRange(puertos);

            string[] baudios = { "2400","4800","9600", "19200" , "115200" };
            cboBaud.Items.AddRange(baudios);

            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DbSerial.xlsx");
            handerExcell = new BDExcell(path);

            txtUsuario.Text = configuraciones.Settings.Default.userName;
            txtMaster.Focus();
            txtUsuario.Focus();
            lblErrorMsg.Visible = false;
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
            txtMaster.Clear();
            txtMaster.Focus();
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
            if(txtPassAnt.Text != configuraciones.Settings.Default.PassTemp)
            {
                mensajeDeError("No es la contraseña correcta, digite nuevamente");
                reset();
            }
            else
            {
                if(objLog.createNewUser(txtUsuario.Text, txtPassNew.Text))
                {

                    reset();
                }
                else
                {
                    mensajeDeError("No se pudo modificar correctamente,"+"\nintentelo nuecamente");
                }
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (objLog.AutenticarAdmin(txtMaster.Text.Trim(),""))
            {
                /*configuraciones.Settings.Default.Puerto = cboComp.SelectedItem.ToString();
                configuraciones.Settings.Default.Baudios = int.Parse(cboBaud.SelectedItem.ToString());
                configuraciones.Settings.Default.Save();*/

                string puerto = cboComp.SelectedItem.ToString();
                int baud = int.Parse(cboBaud.SelectedItem.ToString());
                handerExcell.SaveDataSerialExcell(puerto,baud);
                FrCuadro.Show("Save Sufecull!", "Warning", MessageBoxButtons.YesNo);
            }
            else
            {
                FrCuadro.Show("It is not the correct master's degree", "Warning", MessageBoxButtons.YesNo);
                reset();
            }
        }
    }
}
