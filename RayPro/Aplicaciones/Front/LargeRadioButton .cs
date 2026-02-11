using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace MamografiaGP.Mamorx.Front
{
    public class LargeRadioButton : Control
    {
        public int CircleSize { get; set; } = 32;
        public int CircleThickness { get; set; } = 5;
        public Color BorderColor { get; set; } = Color.Silver;
        public Color CheckedColor { get; set; } = Color.DodgerBlue;

        private bool _checked;
        public bool Checked
        {
            get => _checked;
            set
            {
                if (_checked == value) return;
                _checked = value;
                if (_checked) UncheckSiblings();
                Invalidate();
                CheckedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler CheckedChanged;

        public LargeRadioButton()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);

            Size = new Size(200, 24);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.Clear(BackColor);

            int circleX = 0;
            int circleY = (Height - CircleSize) / 2;

            using (var pen = new Pen(BorderColor, CircleThickness))
            {
                e.Graphics.DrawEllipse(pen, circleX, circleY, CircleSize, CircleSize);
            }

            if (Checked)
            {
                int innerSize = CircleSize / 2;
                int innerX = circleX + (CircleSize - innerSize) / 2;
                int innerY = circleY + (CircleSize - innerSize) / 2;

                using (var brush = new SolidBrush(CheckedColor))
                {
                    e.Graphics.FillEllipse(brush, innerX, innerY, innerSize, innerSize);
                }
            }

            using (var brush = new SolidBrush(ForeColor))
            {
                float textX = CircleSize + 8;
                float textY = (Height - Font.Height) / 2f;
                e.Graphics.DrawString(Text, Font, brush, textX, textY);
            }
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            Checked = true;
        }

        private void UncheckSiblings()
        {
            if (Parent == null) return;
            foreach (Control c in Parent.Controls)
            {
                if (c is LargeRadioButton rb && rb != this)
                    rb.Checked = false;
            }
        }
    }
}
