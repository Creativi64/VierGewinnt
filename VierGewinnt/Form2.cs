using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace VierGewinnt
{
    public partial class Form2 : Form
    {
        string currentcolor;
        public struct Spielfeldtile
        {
            public int x, y, iwidth, iheight;
            public string farbe;
        }

        private int iSpielfeldheightpx;
        private int iSpielfeldwidthpx;

        private int iSpielfeldheight = 6;
        private int iSpielfeldwidth = 7;

        private int X, Y;

        private GraphicsContainer SpielfeldContainer;
        private Graphics spielfeldgraphic;

        private Graphics punkte;
        private Graphics spielfeldbitmapgraphic;


        private bool AimationFlag = false;

        public Spielfeldtile[,] spielfelder;

        #region Console

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AllocConsole();

        #endregion Console

        public Form2(bool Fullscreen)
        {
            InitializeComponent();

            AllocConsole();

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
            iSpielfeldheightpx = this.Height - 100;
            iSpielfeldwidthpx = this.Width - 100;

            spielfeldgraphic = this.CreateGraphics();
            punkte = this.CreateGraphics();

            this.BeginInvoke((MethodInvoker)delegate
            {
                // wird Aufgerufen wenn Das From Geladen Wurde
                spielfelder = new Spielfeldtile[iSpielfeldwidth, iSpielfeldheight];

                SpielfeldZeichnen();
                for (int x = 0; x < iSpielfeldwidth; x++)
                {
                    for (int y = 0; y < iSpielfeldheight; y++)
                    {
                        spielfelder[x, y].farbe = "white";
                    }
                }
                if((new Random()).Next(0, 2)==0)
                {
                    currentcolor = "red";
                    Console.WriteLine("easgtfvsgb");
                }
                else
                {
                    currentcolor = "yellow";
                }
            });
        }

        Bitmap spielfeld;
        private void SpielfeldZeichnen()
        {
            //erstellung des Spielfeldes
            if (spielfeld == null)
            {
                int ispielfeldformat;
                if (iSpielfeldheightpx / iSpielfeldheight <= iSpielfeldwidthpx / iSpielfeldwidth)
                {
                    ispielfeldformat = iSpielfeldheightpx / iSpielfeldheight;
                }
                else
                {
                    ispielfeldformat = iSpielfeldwidthpx / iSpielfeldwidth;
                }

                for (int x = 0; x < iSpielfeldwidth; x++)
                {
                    for (int y = 0; y < iSpielfeldheight; y++)
                    {
                        spielfelder[x, y].x = (this.Width / 2) - (ispielfeldformat * iSpielfeldwidth / 2) + x * ispielfeldformat;
                        spielfelder[x, y].y = (this.Height / 2) - (ispielfeldformat * iSpielfeldheight / 2) + y * ispielfeldformat;
                        spielfelder[x, y].iwidth = ispielfeldformat;
                        spielfelder[x, y].iheight = ispielfeldformat;
                        spielfeldtilezeichnen(spielfelder[x, y].x, spielfelder[x, y].y, spielfelder[x, y].iwidth, spielfelder[x, y].iheight);

                        //if (spielfelder[x, y].farbe != "white" && spielfelder[x, y].farbe != null)
                        //{
                        //    Kreiszeichnen(x, y, spielfelder[x, y].farbe);

                        //}

                    }
                }
                spielfeld = new Bitmap(spielfelder[0,0].iwidth*iSpielfeldwidth, spielfelder[0, 0].iheight * iSpielfeldheight);


            }

        }

        protected override void OnClosed(EventArgs e)
        {
            //wenn man mit X das Programm Schließet Schliest es sich Komlett mit einer Meldung

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
            X = 100;
            Y = 100;
        }

        bool tileerstellt = false;
        private void spielfeldtilezeichnen(int x, int y, int iwidth, int iheight)
        {

            //int x = 50, y = 50, iwidth = 100, iheight = 100;
            Task FeldZeichnen = new Task(() =>
            {
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
            );
            FeldZeichnen.RunSynchronously();
        }

        //private void btn_Up_Click(object sender, EventArgs e)
        //{
        //    // Task Um Die "Animation" des Herrauf "Flüssig" zumachen und nicht Sofortig
        //    Task animation1 = new Task(() =>
        //    {
        //        AimationFlag = true;
        //        for (int i = 0; i < 10; i++)
        //        {
        //            punkte.Clear(this.BackColor);
        //            Y -= 10;
        //            Thread.Sleep(25);
        //        }
        //    }
        //    );
        //    animation1.Start();
        //    if (animation1.IsCompleted == true)
        //    {
        //        AimationFlag = false;
        //    }
        //}

        //private void btn_down_Click(object sender, EventArgs e)
        //{
        //    // Task Um Die "Animation" des Herrunter "Flüssig" zumachen und nicht Sofortig
        //    Task animation = new Task(() =>
        //    {
        //        AimationFlag = true;
        //        for (int i = 0; i < 10; i++)
        //        {
        //            punkte.Clear(this.BackColor);
        //            Y += 10;
        //            Thread.Sleep(25);
        //        }
        //    }
        //    );
        //    Console.WriteLine(animation.Status);
        //    animation.RunSynchronously();
        //    Console.WriteLine(animation.Status);
        //    if (animation.IsCompleted == true)
        //    {
        //        AimationFlag = false;
        //    }
        //}

        private void Form2_Paint(object sender, PaintEventArgs e)
        {
            SpielfeldZeichnen();
        }

        private void button3_Click(object sender, PaintEventArgs e)
        {
            //spielfeldtilezeichnen(20, 20, 20, 20);
        }



        private void Form2_Click(object sender, EventArgs e)
        {
            if (this.PointToClient(Cursor.Position).X > spielfelder[0, 0].x && this.PointToClient(Cursor.Position).X < spielfelder[iSpielfeldwidth - 1, iSpielfeldheight - 1].x + spielfelder[iSpielfeldwidth - 1, iSpielfeldheight - 1].iwidth && this.PointToClient(Cursor.Position).Y > spielfelder[0, 0].y && this.PointToClient(Cursor.Position).Y < spielfelder[iSpielfeldwidth - 1, iSpielfeldheight - 1].y + spielfelder[iSpielfeldwidth - 1, iSpielfeldheight - 1].iheight)
            {
                int spalte, reihe;
                bool gesetzt = false;
                spalte = (this.PointToClient(Cursor.Position).X - spielfelder[0, 0].x) / spielfelder[0, 0].iwidth;
                for (int i = iSpielfeldheight - 1; i >= 0; i--)
                {
                    if (spielfelder[spalte, i].farbe == "white")
                    {
                        gesetzt = true;
                        spielfelder[spalte, i].farbe = currentcolor;
                        Kreiszeichnen(spalte, i, currentcolor);
                        reihe = i;
                        i = 0;
                    }
                }

                for (int x = 0; x < 5; x++)
                {
                    for (int y = 0; y < 5; y++)
                    {

                    }
                }





                if (gesetzt)
                {
                    if (currentcolor == "red")
                    {
                        currentcolor = "yellow";
                    }
                    else
                    {
                        currentcolor = "red";
                    }
                }
            }

            Console.WriteLine(this.PointToClient(new Point(X, Y)));
        }

        private void Kreiszeichnen(int X, int Y, string farbe)
        {
            {
                punkte.FillEllipse(new SolidBrush(Color.FromName(farbe)), spielfelder[0, 0].x + X* spielfelder[0, 0].iwidth+2, spielfelder[0, 0].y + Y * spielfelder[0, 0].iheight+2, spielfelder[0,0].iwidth-4, spielfelder[0, 0].iheight-4);
            }
        }
    }
}

//http://www.bildungsgueter.de/CSharp/Pages/Beisp002Seite025.htm