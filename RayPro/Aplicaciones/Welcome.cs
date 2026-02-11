using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RayPro.Vista
{
    public partial class Welcome : Form
    {
        public Welcome()
        {
            InitializeComponent();
        }

        private void timeInicio_Tick(object sender, EventArgs e)
        {
            if (this.Opacity < 1) this.Opacity += 0.05; //5% -> 0.05, 10% -> 0.1, 100% -> 1
            circularProgressBar1.Value++;
            circularProgressBar1.Text = circularProgressBar1.Value.ToString();
            if (circularProgressBar1.Value == 100)//ya trascurrio 3mil milisegundo
            {
                timeInicio.Stop();
                timeFin.Start();
            }
        }

        private void timeFin_Tick(object sender, EventArgs e)
        {
            this.Opacity -= 0.1;
            if (this.Opacity == 0)
            {
                timeFin.Stop();
                this.Close();
            }
        }

        private void Welcome_Load(object sender, EventArgs e)
        {
            lblNameUser.Text = configuraciones.Settings.Default.Usuarios;
            try
            {
                using (Stream stream = Properties.Resources.welcome)
                {
                    using (var sonido = new SoundPlayer(stream))
                    {
                        sonido.Play();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al reproducir el sonido: " + ex.Message);
            }

            this.Opacity = 0.0;
            circularProgressBar1.Value = 0;
            circularProgressBar1.Minimum = 0;
            circularProgressBar1.Maximum = 100;
            timeInicio.Start();
        }
    }
}
