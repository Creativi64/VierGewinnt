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

        public static string sGewinnAnzahl;
        private static PointF[,] Dreieckspunkte;
        private static int iSpielfeldwidthmax = 20, iSpielfeldheightmax = 20, iXstartMin = 308, iYstartMin = 105, iXstart = 468, iYstart = 265, iSpielfeldformat = 10;
        private static Bitmap Spielfeldframe;
        
        private Graphics Bitmapgraphic;
        private Graphics spielfeldgraphic;
        #region Console
        /// <summary>
        /// Erlaubt Uns Zum Form eine console zu starten
        /// </summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AllocConsole();

        #endregion
        public Form1()
        {
            InitializeComponent();
            AllocConsole();

            spielfeldgraphic = this.CreateGraphics();
            Spielfeldframe = new Bitmap(this.Width, this.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            Bitmapgraphic = Graphics.FromImage(Spielfeldframe);

            EckenBerechnen(0, 0, Form2.iSpielfeldWidth, Form2.iSpielfeldHeight);
            SpielfeldZeichnen();
            textBox1.Text = "Vier";
            Form2.iSpielfeldHeight = 4;
            Form2.iSpielfeldWidth = 4;


        }

        protected override void OnClosed(EventArgs e)
        {
            MessageBox.Show("Spiel Beendet",
                "Close Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            base.OnClosed(e);
            Environment.Exit(0);
            Application.Exit();
        }

        private static void EckenBerechnen(int x, int y, int iwidth, int iheight)
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

        private void Btn_Network_Click(object sender, EventArgs e)
        {
            Form3 frm = new Form3();

            frm.Show();
            this.Hide();
        }

        private void Btn_Play_Click(object sender, EventArgs e)
        {
            Form2 frm = new Form2();
            frm.Show();
            this.Hide();

        }
        private void Btn_Quit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }



        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            SpielfeldZeichnen();
        }

        private void SpielfeldTileZeichnen(int x, int y, int iwidth, int iheight)
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
                    Bitmapgraphic.FillPolygon(new SolidBrush(Color.Blue), hilfsarray);
                }
                Bitmapgraphic.DrawEllipse(new Pen(Color.Blue, 5), x, y, iwidth, iheight);
                Bitmapgraphic.DrawRectangle(new Pen(Color.Blue, 5), x, y, iwidth, iheight);
            });
            Feld.RunSynchronously();
        }

        private void SpielfeldZeichnen()
        {
            Bitmapgraphic.FillRectangle(new SolidBrush(Color.White), 0, 0, this.Width, this.Height);
            for (int x = 0; x < Form2.iSpielfeldWidth; x++)
            {
                for (int y = 0; y < Form2.iSpielfeldHeight; y++)
                {
                    SpielfeldTileZeichnen(
                        iXstart + x * iSpielfeldformat,
                        iYstart + y * iSpielfeldformat,
                        iSpielfeldformat,
                        iSpielfeldformat);
                }
            }
            spielfeldgraphic.DrawImage(Spielfeldframe, 0, 0);
        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            sGewinnAnzahl = textBox1.Text;
        }
        private void TrackBarX_Scroll(object sender, EventArgs e)
        {
            //spielfeldgraphic.FillRectangle(new SolidBrush(Color.White), 0, 0, this.Width , this.Height);
            Form2.iSpielfeldWidth = trackBarX.Value + 4;
            Form3.iSpielfeldwidth = trackBarX.Value + 4;
            iXstart = iXstartMin + (iSpielfeldwidthmax - Form2.iSpielfeldWidth) * iSpielfeldformat;
            SpielfeldZeichnen();
        }
        private void TrackBarY_Scroll(object sender, EventArgs e)
        {
            //spielfeldgraphic.FillRectangle(new SolidBrush(Color.White), 0, 0, this.Width, this.Height);
            Form2.iSpielfeldHeight = trackBarY.Value + 4;
            Form3.iSpielfeldheight = trackBarY.Value + 4;
            iYstart = iYstartMin + (iSpielfeldheightmax - Form2.iSpielfeldHeight) * iSpielfeldformat;
            SpielfeldZeichnen();
        }
    }
}