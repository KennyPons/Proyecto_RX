using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RayPro.Vista
{
    public partial class QuestionBox : Form
    {

        private Color primaryColor = Color.CornflowerBlue;
        private int boderSize = 2;



        public Color PrimarioColor
        {
            get { return primaryColor; }
            set
            {
                primaryColor = value;
                this.BackColor = primaryColor;//Form Border Color
                this.panelSuperior.BackColor = primaryColor;// tittle bar color
            }
        }
        /*---------mover cuadro-------------------------*/

        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();


        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);

        //constructors
        public QuestionBox()
        {
            InitializeComponent();
            IniciandoItems();
        }

        public QuestionBox(String texto, String Caption, MessageBoxButtons bottons)
        {
            InitializeComponent();
            IniciandoItems();
            this.PrimarioColor = primaryColor;
            this.lblMessage.Text = texto;
            this.lblCaption.Text = Caption;
            //SetFormSize();
            SetButtoness(bottons, MessageBoxDefaultButton.Button1);
        }

        //-----------------methods--------------------

        private void IniciandoItems()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.Padding = new Padding(boderSize);
            this.lblMessage.MaximumSize = new Size(550, 0);
            //this.btnSalir.DialogResult = DialogResult.Cancel;
            this.btnOne.DialogResult = DialogResult.OK;
            this.btnOne.Visible = false;
            this.btnTwo.Visible = false;
            this.btnThree.Visible = false;

        }

        private void SetButtoness(MessageBoxButtons bottones, MessageBoxDefaultButton defaultBotton)
        {

            int xCenter = (this.panelInferior.Width - this.btnOne.Width) / 2;
            int yCenter = (this.panelInferior.Height - this.btnOne.Height) / 2;

            switch (bottones)
            {

                case MessageBoxButtons.YesNo:
                    //YES BUTTON
                    btnOne.Visible = true;
                    btnOne.Location = new Point(xCenter - (btnOne.Width / 2) - 5, yCenter);
                    btnOne.Text = "Yes";
                    btnOne.DialogResult = DialogResult.Yes;

                    //NO BUTTON
                    btnTwo.Visible = true;
                    btnTwo.BackColor = Color.IndianRed;
                    btnTwo.Location = new Point(xCenter + (btnTwo.Width / 2) + 5, yCenter);
                    btnTwo.Text = "No";
                    btnTwo.DialogResult = DialogResult.No;

                    //set defaul button
                    if (defaultBotton != MessageBoxDefaultButton.Button3)//there are only 2 buttons, so the default Button cannot be Button3
                    {
                        SetDeafultBottones(defaultBotton);
                    }
                    else SetDeafultBottones(MessageBoxDefaultButton.Button1);
                    break;
                case MessageBoxButtons.YesNoCancel:
                    //Yes Button
                    btnOne.Visible = true;
                    btnOne.Location = new Point(xCenter - btnOne.Width - 5, yCenter);
                    btnOne.Text = "Yes";
                    btnOne.DialogResult = DialogResult.Yes;

                    //NO BUTTON
                    btnTwo.Visible = true;
                    btnTwo.BackColor = Color.IndianRed;
                    btnTwo.Location = new Point(xCenter, yCenter);
                    btnTwo.Text = "No";
                    btnTwo.DialogResult = DialogResult.No;


                    //Cancel Button
                    btnThree.Visible = true;
                    btnThree.Location = new Point(xCenter + btnTwo.Width + 5, yCenter);
                    btnThree.Text = "Cancel";
                    btnThree.DialogResult = DialogResult.Cancel;
                    btnThree.BackColor = Color.DimGray;

                    //set default
                    SetDeafultBottones(defaultBotton);
                    break;
                default: break;

            }
        }


        private void SetDeafultBottones(MessageBoxDefaultButton defaultBotton)
        {
            switch (defaultBotton)
            {
                case MessageBoxDefaultButton.Button1://Focus boton One
                    btnOne.Select();
                    btnOne.ForeColor = Color.White;
                    btnOne.Font = new Font(btnOne.Font, FontStyle.Underline);
                    break;
                case MessageBoxDefaultButton.Button2://Focus boton Second
                    btnTwo.Select();
                    btnTwo.ForeColor = Color.White;
                    btnTwo.Font = new Font(btnTwo.Font, FontStyle.Underline);
                    break;
                case MessageBoxDefaultButton.Button3://Focus boton Third
                    btnThree.Select();
                    btnThree.ForeColor = Color.White;
                    btnThree.Font = new Font(btnThree.Font, FontStyle.Underline);
                    break;
            }
        }


        //*LLAMADO*//

        public static DialogResult Show(String texto, String Captiom, MessageBoxButtons btn)
        {
            DialogResult result;
            using (var msgForm = new QuestionBox(texto, Captiom, btn))
                result = msgForm.ShowDialog();
            return result;
        }

        //---------------------------------------------


        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void panelSuperior_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
