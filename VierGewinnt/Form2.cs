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
        public bool Fullscreen { get; private set; }

        Bitmap image;
        Graphics g;


        public Form2()
        {
            InitializeComponent();

            this.Load += Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //this.TopMost = true;
            
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form1 frm = new Form1();

            frm.Show();
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            g = this.CreateGraphics();
            g.DrawLine(new Pen(new SolidBrush(Color.Black)),10, 10,10,10);
            g.DrawEllipse(new Pen(new SolidBrush(Color.Black)), 1000, 1000, 30, 30);

        }
    }
}