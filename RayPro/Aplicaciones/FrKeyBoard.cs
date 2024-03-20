using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace RayPro.Aplicaciones
{
    public partial class FrKeyBoard : Form
    {
        public FrKeyBoard()
        {
            InitializeComponent();
            iniPropiedadesTecla();
            
        }

        // Sobrescribir el método OnPaint para dibujar los bordes redondeados
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Dibujar el borde redondeado
            int radio = 15; // Radio de los bordes redondeados
            int anchoBorde = 2; // Ancho del borde
            Rectangle rectangulo = new Rectangle(anchoBorde, anchoBorde, this.Width - 2 * anchoBorde, this.Height - 2 * anchoBorde);
            GraphicsPath bordeRedondeado = GetRoundedRect(rectangulo, radio);
            this.Region = new Region(bordeRedondeado);

            // Opcional: Puedes dibujar el contenido del formulario aquí
        }

        // Método para obtener un borde redondeado
        private GraphicsPath GetRoundedRect(Rectangle rect, int radio)
        {
            GraphicsPath bordeRedondeado = new GraphicsPath();
            bordeRedondeado.AddArc(rect.X, rect.Y, radio * 2, radio * 2, 180, 90);
            bordeRedondeado.AddArc(rect.Right - radio * 2, rect.Y, radio * 2, radio * 2, 270, 90);
            bordeRedondeado.AddArc(rect.Right - radio * 2, rect.Bottom - radio * 2, radio * 2, radio * 2, 0, 90);
            bordeRedondeado.AddArc(rect.X, rect.Bottom - radio * 2, radio * 2, radio * 2, 90, 90);
            bordeRedondeado.CloseFigure();
            return bordeRedondeado;
        }


        private void iniPropiedadesTecla()
        {
            this.KeyPreview = true;
            this.FormBorderStyle = FormBorderStyle.None; // Ocultar borde estándar
            this.StartPosition = FormStartPosition.CenterScreen; // Centrar en pantalla
            this.Size = new Size(200, 285); // Tamaño del formulario
        }

        ///FUNCTIONS 
        private void AppendNumber(string number)
        {
            txtShowNum.Text += number;
        }

        private void eliminandoNumxNum()
        {
            if (txtShowNum.Text.Length > 0)
            {
                txtShowNum.Text = txtShowNum.Text.Substring(0, txtShowNum.Text.Length - 1);
            }
        }

        ///BTUNES


        private void btn_cerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btn_cerrar_MouseHover(object sender, EventArgs e)
        {
            
        }

        private void btnEnter_Click(object sender, EventArgs e)
        {
           
        }

        private void btn1_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Button button = sender as System.Windows.Forms.Button;
            AppendNumber(button.Text);
        }

        private void btn2_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Button button = sender as System.Windows.Forms.Button;
            AppendNumber(button.Text);
        }

        private void btn3_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Button button = sender as System.Windows.Forms.Button;
            AppendNumber(button.Text);
        }

        private void btn4_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Button button = sender as System.Windows.Forms.Button;
            AppendNumber(button.Text);
        }

        private void btn5_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Button button = sender as System.Windows.Forms.Button;
            AppendNumber(button.Text);
        }

        private void btn6_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Button button = sender as System.Windows.Forms.Button;
            AppendNumber(button.Text);
        }

        private void btn7_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Button button = sender as System.Windows.Forms.Button;
            AppendNumber(button.Text);
        }

        private void btn8_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Button button = sender as System.Windows.Forms.Button;
            AppendNumber(button.Text);
        }

        private void btn9_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Button button = sender as System.Windows.Forms.Button;
            AppendNumber(button.Text);
        }

        private void btn0_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Button button = sender as System.Windows.Forms.Button;
            AppendNumber(button.Text);
        }

        private void btnEliminando_Click(object sender, EventArgs e)
        {
            eliminandoNumxNum();
        }

        private void btnBorrar_Click(object sender, EventArgs e)
        {
            txtShowNum.Clear();
        }

        private void FrKeyBoard_KeyDown(object sender, KeyEventArgs e)
        {
            // Manejar pulsaciones de teclado
            switch (e.KeyCode)
            {
                case Keys.Back:
                    eliminandoNumxNum();
                    break;
                case Keys.D0:
                case Keys.NumPad0:
                    AppendNumber("0");
                    break;
                case Keys.D1:
                case Keys.NumPad1:
                    AppendNumber("1");
                    break;
                case Keys.D2:
                case Keys.NumPad2:
                    AppendNumber("2");
                    break;
                case Keys.D3:
                case Keys.NumPad3:
                    AppendNumber("3");
                    break;
                case Keys.D4:
                case Keys.NumPad4:
                    AppendNumber("4");
                    break;
                case Keys.D5:
                case Keys.NumPad5:
                    AppendNumber("5");
                    break;
                case Keys.D6:
                case Keys.NumPad6:
                    AppendNumber("6");
                    break;
                case Keys.D7:
                case Keys.NumPad7:
                    AppendNumber("7");
                    break;
                case Keys.D8:
                case Keys.NumPad8:
                    AppendNumber("8");
                    break;
                case Keys.D9:
                case Keys.NumPad9:
                    AppendNumber("9");
                    break;
            }
        }
    }
}
