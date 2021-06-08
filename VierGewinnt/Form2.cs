﻿using System;
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
        private struct Spielfeldtile
        {
            public int x, y, iwidth, iheight;
            public string farbe;
        }

        private int iSpielfeldheight = 6;
        private int iSpielfeldwidth = 7;

        private int X, Y;
        private Graphics spielfeldgraphic;

        public Form2(bool Fullscreen)
        {
            InitializeComponent();
            Console.WriteLine("test");

            spielfeldgraphic = this.CreateGraphics();

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

            this.BeginInvoke((MethodInvoker)delegate
            {
                SpielfeldZeichnen();
            });
        }

        private void SpielfeldZeichnen()
        {
            //erstellung des Spielfeldes

            Spielfeldtile[,] spielfelder = new Spielfeldtile[iSpielfeldwidth, iSpielfeldheight];

            int ispielfeldformat;
            if (spielfeldpanel.Height / iSpielfeldheight <= spielfeldpanel.Width / iSpielfeldwidth)
            {
                ispielfeldformat = spielfeldpanel.Height / iSpielfeldheight;
            }
            else
            {
                ispielfeldformat = spielfeldpanel.Width / iSpielfeldwidth;
            }

            for (int x = 0; x < iSpielfeldwidth; x++)
            {
                for (int y = 0; y < iSpielfeldheight; y++)
                {
                    spielfelder[x, y].x = spielfeldpanel.Location.X + x * ispielfeldformat;
                    spielfelder[x, y].y = spielfeldpanel.Location.Y + y * ispielfeldformat;
                    spielfelder[x, y].iwidth = ispielfeldformat;
                    spielfelder[x, y].iheight = ispielfeldformat;

                    spielfeldtilezeichnen(spielfelder[x, y].x, spielfelder[x, y].y, spielfelder[x, y].iwidth, spielfelder[x, y].iheight);
                }
            }
        }

        private void AfterLoading(object sender, EventArgs e)
        {
            Spielfeldtile[,] spielfelder = new Spielfeldtile[iSpielfeldwidth, iSpielfeldheight];

            int ispielfeldformat;
            if (spielfeldpanel.Height / iSpielfeldheight <= spielfeldpanel.Width / iSpielfeldwidth)
            {
                ispielfeldformat = spielfeldpanel.Height / iSpielfeldheight;
            }
            else
            {
                ispielfeldformat = spielfeldpanel.Width / iSpielfeldwidth;
            }

            for (int x = 0; x < iSpielfeldwidth; x++)
            {
                for (int y = 0; y < iSpielfeldheight; y++)
                {
                    spielfelder[x, y].x = spielfeldpanel.Location.X + x * ispielfeldformat;
                    spielfelder[x, y].y = spielfeldpanel.Location.Y + y * ispielfeldformat;
                    spielfelder[x, y].iwidth = ispielfeldformat;
                    spielfelder[x, y].iheight = ispielfeldformat;

                    spielfeldtilezeichnen(spielfelder[x, y].x, spielfelder[x, y].y, spielfelder[x, y].iwidth, spielfelder[x, y].iheight);
                }
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
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Console.WriteLine("asdasd");
            Console.WriteLine("asdasd");
            X = 100;
            Y = 100;
            Kreis(X, Y);
        }

        private void spielfeldtilezeichnen(int x, int y, int iwidth, int iheight)
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

            spielfeldgraphic.DrawRectangle(new Pen(Color.Blue, 5), x, y, iwidth, iheight);
            spielfeldgraphic.DrawEllipse(new Pen(Color.Blue, 5), x, y, iwidth, iheight);

            PointF[] hilfsarray = new PointF[3];

            for (int j = 0; j < 4; j++)
            {
                for (int i = 0; i < 3; i++)
                {
                    hilfsarray[i] = Dreieckspunkte[j, i];
                }
                spielfeldgraphic.FillPolygon(new SolidBrush(Color.Blue), hilfsarray);
            }
        }

        private void btn_Up_Click(object sender, EventArgs e)
        {
            Task animation1 = new Task(() =>
            {
                for (int i = 0; i < 10; i++)
                {
                    spielfeldgraphic.Clear(Form2.DefaultBackColor);
                    Y -= 10;
                    Kreis(X, Y);
                    Thread.Sleep(25);
                }
            }
            );
            animation1.Start();
        }

        private void btn_down_Click(object sender, EventArgs e)
        {
            Task animation = new Task(() =>
            {
                for (int i = 0; i < 10; i++)
                {
                    spielfeldgraphic.Clear(Form2.DefaultBackColor);
                    Y += 10;
                    Kreis(X, Y);
                    Thread.Sleep(25);
                }
            }
            );
            animation.Start();
        }

        private void btn_Test_Click(object sender, EventArgs e)
        {
        }

        private void Kreis(int X, int Y)
        {
            Color farbe = Color.Goldenrod;
            using (SolidBrush pinsel = new SolidBrush(farbe))
            {
                spielfeldgraphic.FillEllipse((pinsel), X, Y, 100, 100);
            }
        }
    }
}

//http://www.bildungsgueter.de/CSharp/Pages/Beisp002Seite025.htm