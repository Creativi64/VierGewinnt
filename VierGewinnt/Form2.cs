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
        Graphics g;

        public Form2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form1 frm = new Form1();
            
            frm.Show();
            this.Hide();
            this.Hide();
            g.DrawEllipse(new Pen(Color.FromName("SlateBlue")),10,10,10,10);

        }
    }
}
