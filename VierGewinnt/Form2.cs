using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VierGewinnt
{
    public partial class Form2 : Form
    {
        private int X, Y;
        private Graphics g;

        public Form2(bool Fullscreen)
        {
            InitializeComponent();

            
            g = this.CreateGraphics();

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

        protected override void OnClosed(EventArgs e)
        {
            MessageBox.Show("Spiel Beendet",
                "Close Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            base.OnClosed(e);

            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form1 frm = new Form1();

            frm.Show();
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Console.WriteLine(  "asdasd");
            Console.WriteLine(  "asdasd");
            X = 100;
            Y = 100;
            Kreis(X, Y);
        }

        private void spielfeldtileerstellung(int x, int y, int iwidth, int iheight)
        {
            //int x = 50, y = 50, iwidth = 100, iheight = 100;
            double dDreieckkprozent = 0.3;
            PointF[,] Dreieckspunkte = new PointF[4, 3];
            Dreieckspunkte[0, 0] = new PointF(x, y);
            Dreieckspunkte[0, 1] = new PointF((float)(x + (iwidth * dDreieckkprozent)), y);
            Dreieckspunkte[0, 2] = new PointF((x), (float)(y + (iheight * dDreieckkprozent)));

            Dreieckspunkte[1, 0] = new PointF(x + iwidth, y);
            Dreieckspunkte[1, 1] = new PointF((float)(x + iwidth - (iwidth * dDreieckkprozent)), y);
            Dreieckspunkte[1, 2] = new PointF((x + iwidth), (float)(y + (iheight * dDreieckkprozent)));

            Dreieckspunkte[2, 0] = new PointF(x, y + iheight);
            Dreieckspunkte[2, 1] = new PointF((float)(x + (iwidth * dDreieckkprozent)), y + iheight);
            Dreieckspunkte[2, 2] = new PointF((x), (float)(y + iheight - (iheight * dDreieckkprozent)));

            Dreieckspunkte[3, 0] = new PointF(x + iwidth, y + iheight);
            Dreieckspunkte[3, 1] = new PointF((float)(x + iwidth - (iwidth * dDreieckkprozent)), y + iheight);
            Dreieckspunkte[3, 2] = new PointF((x + iwidth), (float)(y + iheight - (iheight * dDreieckkprozent)));

            g.DrawRectangle(new Pen(Color.Blue, 5), x, y, iwidth, iheight);
            g.DrawEllipse(new Pen(Color.Blue, 5), x, y, iwidth, iheight);

            PointF[] hilfsarray = new PointF[3];

            for (int j = 0; j < 4; j++)
            {
                for (int i = 0; i < 3; i++)
                {
                    hilfsarray[i] = Dreieckspunkte[j, i];
                }
                g.FillPolygon(new SolidBrush(Color.Blue), hilfsarray);
            }
        }

        private void btn_Up_Click(object sender, EventArgs e)
        {
            Task animation = new Task(() =>
            {
                for (int i = 0; i < 10; i++)
                {
                    g.Clear(Form2.DefaultBackColor);
                    Y -= 10;
                    Kreis(X, Y);
                    Thread.Sleep(25);
                }
            }
            );
            animation.Start();
        }

        private void btn_down_Click(object sender, EventArgs e)
        {
            Task animation = new Task(() =>
            {
                for (int i = 0; i < 10; i++)
                {
                    g.Clear(Form2.DefaultBackColor);
                    Y += 10;
                    Kreis(X, Y);
                    Thread.Sleep(25);
                }

            }
            );
            animation.Start();
        }

        private void Kreis(int X, int Y)
        {
            Color farbe = Color.Goldenrod;
            using (SolidBrush pinsel = new SolidBrush(farbe))
            {
                g.FillEllipse((pinsel), X, Y, 100, 100);
            }
        }
    }
}

//http://www.bildungsgueter.de/CSharp/Pages/Beisp002Seite025.htm