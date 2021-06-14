using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace VierGewinnt
{
    public partial class Form1 : Form
    {
        public bool Fullscreen { get; private set; }
        private int iSpielfeldwidthmax = 20, iSpielfeldheightmax = 20, xstartmin = 308, ystartmin = 105, xstart = 468, ystart = 265, ispielfeldformat = 10;
        public static string sgewinnZahl;

        #region Console

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AllocConsole();

        #endregion
        public Form1()
        {
            InitializeComponent();
            AllocConsole();
            spielfeldgraphic = this.CreateGraphics();
            EckenBerechnen(0, 0, Form2.iSpielfeldwidth, Form2.iSpielfeldheight);
            SpielfeldZeichnen();
            textBox1.Text = "Vier";
        }

        private void btn_Play_Click(object sender, EventArgs e)
        {
            Form2 frm = new Form2(Fullscreen);
            frm.Show();
            this.Hide();

        }

        protected override void OnClosed(EventArgs e)
        {
            MessageBox.Show("Spiel Beendet",
                "Close Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            base.OnClosed(e);
            Application.Exit();
        }

        private void btn_Network_Click(object sender, EventArgs e)
        {
            Form3 frm = new Form3(Fullscreen);

            frm.Show();
            this.Hide();
        }

        private void btn_Quit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void chBox_Fullscreen_CheckedChanged(object sender, EventArgs e)
        {
            if (FormBorderStyle == FormBorderStyle.None)
            {
                this.FormBorderStyle = FormBorderStyle.FixedSingle;
                this.WindowState = FormWindowState.Normal;
                Fullscreen = false;
            }
            else
            {
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
                Fullscreen = true;
            }
        }
        private Graphics spielfeldgraphic;

        private PointF[,] Dreieckspunkte;

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            sgewinnZahl = textBox1.Text;
        }

        private void EckenBerechnen(int x, int y, int iwidth, int iheight)
        {
            double dDreieckkprozent = 0.3;
            Dreieckspunkte = new PointF[4, 3];
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
        }

        private void SpielfeldZeichnen()
        {


            for (int x = 0; x < Form2.iSpielfeldwidth; x++)
            {
                for (int y = 0; y < Form2.iSpielfeldheight; y++)
                {
                    spielfeldtilezeichnen(
                        xstart + x * ispielfeldformat,
                        ystart + y * ispielfeldformat,
                        ispielfeldformat,
                        ispielfeldformat);
                }
            }
        }

        private void spielfeldtilezeichnen(int x, int y, int iwidth, int iheight)
        {
            PointF[] hilfsarray = new PointF[3];

            //int x = 50, y = 50, iwidth = 100, iheight = 100;
            Task Feld = new Task(() =>
            {
                for (int j = 0; j < 4; j++)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        hilfsarray[i] = new PointF(Dreieckspunkte[j, i].X + x, Dreieckspunkte[j, i].Y + y);
                    }
                    spielfeldgraphic.FillPolygon(new SolidBrush(Color.Blue), hilfsarray);
                }
                spielfeldgraphic.DrawEllipse(new Pen(Color.Blue, 5), x, y, iwidth, iheight);
                spielfeldgraphic.DrawRectangle(new Pen(Color.Blue, 5), x, y, iwidth, iheight);
                Console.WriteLine(x + " " + y + " " + iwidth + " " + iheight);
            });
            Feld.RunSynchronously();
        }

        private void trackBarX_Scroll(object sender, EventArgs e)
        {
            spielfeldgraphic.FillRectangle(new SolidBrush(Color.White), 0, 0, this.Width , this.Height);
            Form2.iSpielfeldwidth = trackBarX.Value + 4;
            xstart = xstartmin + (iSpielfeldwidthmax - Form2.iSpielfeldwidth) * ispielfeldformat;
            SpielfeldZeichnen();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            SpielfeldZeichnen();
        }

        private void trackBarY_Scroll(object sender, EventArgs e)
        {
            spielfeldgraphic.FillRectangle(new SolidBrush(Color.White), 0, 0, this.Width, this.Height);
            Form2.iSpielfeldheight = trackBarY.Value + 4;
            ystart = ystartmin + (iSpielfeldheightmax - Form2.iSpielfeldheight) * ispielfeldformat;
            SpielfeldZeichnen();
        }
    }
}