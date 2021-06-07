using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VierGewinnt
{
    public partial class Form2 : Form
    {
        private Graphics g;

        public Form2(bool Fullscreen)
        {
            InitializeComponent();

            if (Fullscreen == true)
            {
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
            }
            else
            {
                this.FormBorderStyle = FormBorderStyle.FixedSingle;
                this.WindowState = FormWindowState.Normal;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form1 frm = new Form1();

            frm.Show();
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
            

            g.DrawEllipse(new Pen(Color.FromName("SlateBlue")), 10, 10, 10, 10);
        }
    }
}