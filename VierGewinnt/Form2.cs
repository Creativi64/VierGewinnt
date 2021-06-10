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
        private bool Fullscreen;
        private string currentcolor;

        public struct Spielfeldtile
        {
            public int x, y, iwidth, iheight;
            public string farbe;
        }

        private int iSpielfeldheightpx;
        private int iSpielfeldwidthpx;

        private int iSpielfeldheight = 6;
        private int iSpielfeldwidth = 7;

        //private int X, Y;

        private Graphics spielfeldgraphic;
        private PointF[,] Dreieckspunkte;
        private Graphics punkte;
        private Graphics fullspielfeldgraphic;

        private bool AimationFlag = false;

        public Spielfeldtile[,] spielfelder;

        #region Console

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AllocConsole();

        #endregion Console

        public Form2(bool _Fullscreen)
        {
            InitializeComponent();
            Fullscreen = _Fullscreen;
            AllocConsole();

            if (_Fullscreen == true)
            {
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;

                this.MaximizeBox = false;
            }
            else
            {
                this.FormBorderStyle = FormBorderStyle.FixedSingle;
                this.WindowState = FormWindowState.Normal;
                this.MaximizeBox = false;
            }
            iSpielfeldheightpx = this.Height - 100;
            iSpielfeldwidthpx = this.Width - 100;

            spielfeldgraphic = this.CreateGraphics();
            fullspielfeldgraphic = this.CreateGraphics();

            punkte = this.CreateGraphics();

            this.BeginInvoke((MethodInvoker)delegate
            {
                // wird Aufgerufen wenn Das From Geladen Wurde
                spielfelder = new Spielfeldtile[iSpielfeldwidth, iSpielfeldheight];
                Thread.Sleep(400);

                //Erste Ecken Berechnen
                int ispielfeldformat;
                if (iSpielfeldheightpx / iSpielfeldheight <= iSpielfeldwidthpx / iSpielfeldwidth)
                {
                    ispielfeldformat = iSpielfeldheightpx / iSpielfeldheight;
                }
                else
                {
                    ispielfeldformat = iSpielfeldwidthpx / iSpielfeldwidth;
                }
                spielfelder[0, 0].x = (this.Width / 2) - (ispielfeldformat * iSpielfeldwidth / 2) + 0 * ispielfeldformat;
                spielfelder[0, 0].y = (this.Height / 2) - (ispielfeldformat * iSpielfeldheight / 2) + 0 * ispielfeldformat;
                spielfelder[0, 0].iwidth = ispielfeldformat;
                spielfelder[0, 0].iheight = ispielfeldformat;

                EckenBerechnen(/*spielfelder[0, 0].x, spielfelder[0, 0].y*/0, 0, spielfelder[0, 0].iwidth, spielfelder[0, 0].iheight);
                //Spielfeldzeichen
                SpielfeldZeichnen();

                // Im array alle Farben Auf Weiß zu setzen
                for (int x = 0; x < iSpielfeldwidth; x++)
                {
                    for (int y = 0; y < iSpielfeldheight; y++)
                    {
                        spielfelder[x, y].farbe = "white";
                    }
                }

                // zufällige start Fahrbe Wählen
                if ((new Random()).Next(0, 2) == 0)
                {
                    currentcolor = "red";
                    lab_Player.Text = "Player Red";
                }
                else
                {
                    currentcolor = "yellow";
                    lab_Player.Text = "Player Yellow";
                }
            });
        }

        private void SpielfeldZeichnen()
        {
            //erstellung des Spielfeldes

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

                    if (spielfelder[x, y].farbe != "white" && spielfelder[x, y].farbe != null)
                    {
                        Kreiszeichnen(x, y, spielfelder[x, y].farbe);
                    }
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            //wenn man mit X das Programm Schließet Schliest es sich Komlett mit einer Meldung
            AimationFlag = true;
            this.Hide();

            MessageBox.Show("Spiel Beendet",
                "Close Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            base.OnClosed(e);
            Application.Exit();
            AimationFlag = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form1 frm = new Form1();

            frm.Show();
            this.Hide();
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
            });
            Feld.RunSynchronously();
        }

        private void Form2_Paint(object sender, PaintEventArgs e)
        {
            if (AimationFlag == true)
            {
                SpielfeldZeichnen();
            }
        }

        private int gewinnnummer = 4;

        private void Form2_Click(object sender, EventArgs e)
        {
            if (this.PointToClient(Cursor.Position).X > spielfelder[0, 0].x && this.PointToClient(Cursor.Position).X < spielfelder[iSpielfeldwidth - 1, iSpielfeldheight - 1].x + spielfelder[iSpielfeldwidth - 1, iSpielfeldheight - 1].iwidth && this.PointToClient(Cursor.Position).Y > spielfelder[0, 0].y && this.PointToClient(Cursor.Position).Y < spielfelder[iSpielfeldwidth - 1, iSpielfeldheight - 1].y + spielfelder[iSpielfeldwidth - 1, iSpielfeldheight - 1].iheight)
            {
                int spalte, reihe = -1;
                bool gesetzt = false;
                spalte = (this.PointToClient(Cursor.Position).X - spielfelder[0, 0].x) / spielfelder[0, 0].iwidth;
                for (int i = iSpielfeldheight - 1; i >= 0; i--)
                {
                    if (spielfelder[spalte, i].farbe == "white")
                    {
                        gesetzt = true;
                        spielfelder[spalte, i].farbe = currentcolor;
                        KreiszeichnenAnimation(spalte, i + 1, currentcolor);
                        reihe = i;
                        i = 0;
                    }
                }

                bool gewonnen = false;
                if (gesetzt && reihe != -1)
                {
                    int infolge = 0;
                    for (int x = 0; x < iSpielfeldwidth; x++)
                    {
                        if (spielfelder[x, reihe].farbe == currentcolor)
                        {
                            infolge++;
                        }
                        else
                        {
                            infolge = 0;
                        }
                        if (infolge == gewinnnummer)
                        {
                            gewonnen = true;
                        }
                    }
                    for (int y = 0; y < iSpielfeldheight; y++)
                    {
                        if (spielfelder[spalte, y].farbe == currentcolor)
                        {
                            infolge++;
                        }
                        else
                        {
                            infolge = 0;
                        }
                        if (infolge == gewinnnummer)
                        {
                            gewonnen = true;
                        }
                    }

                    int xabstand;
                    int maxformat;
                    xabstand = spalte - reihe;
                    if (iSpielfeldheight > iSpielfeldwidth)
                    {
                        maxformat = iSpielfeldwidth;
                    }
                    else
                    {
                        maxformat = iSpielfeldheight;
                    }
                    for (int xy = 0; xy < maxformat; xy++)
                    {
                        if (xy + xabstand < maxformat && xy + xabstand >= 0 && spielfelder[xy + xabstand, xy].farbe == currentcolor)
                        {
                            infolge++;
                        }
                        else
                        {
                            infolge = 0;
                        }
                        if (infolge == gewinnnummer)
                        {
                            gewonnen = true;
                        }
                    }

                    for (int xy = 0; xy < maxformat; xy++)
                    {
                        if (iSpielfeldwidth - xy + xabstand < maxformat && iSpielfeldwidth - xy + xabstand >= 0 && spielfelder[iSpielfeldwidth - xy + xabstand, xy].farbe == currentcolor)
                        {
                            infolge++;
                        }
                        else
                        {
                            infolge = 0;
                        }
                        if (infolge == gewinnnummer)
                        {
                            gewonnen = true;
                        }
                    }

                    if (gewonnen)
                    {
                        Gewonnen(currentcolor);
                    }

                    if (currentcolor == "red")
                    {
                        currentcolor = "yellow";
                        lab_Player.Text = "Player Yellow";
                    }
                    else
                    {
                        currentcolor = "red";
                        lab_Player.Text = "Player Red";
                    }
                }
            }

            Console.WriteLine(this.PointToClient(new Point((Cursor.Position).X, (Cursor.Position).Y)));
        }

        private void KreiszeichnenAnimation(int X, int Y, string farbe)
        {
            int iHilfszahl =0;
            int iHilfszahl1 =0;
            int multiplyer =4; // durch 2 teil bare Zahlen funktionieren am besten da Dann Weniger Komma stellen Entstehen die Ignoriert werden
            Task animation1 = new Task(() =>
            {
                AimationFlag = true;

                for (int i = 0; i < Y * multiplyer; i += 1)
                {
                    if (i == 0 || i + 1 == Y * multiplyer || Convert.ToInt32((i / multiplyer)+1) >= Y)
                    {
                        punkte.FillEllipse(new SolidBrush(this.BackColor),
                             spielfelder[0, 0].x + X * spielfelder[0, 0].iwidth + 2,
                             spielfelder[0, 0].y + ((i - 1) / multiplyer) * spielfelder[0, 0].iheight + 2,
                             spielfelder[0, 0].iwidth - 4, spielfelder[0, 0].iheight - 4);

                        punkte.FillEllipse(new SolidBrush(Color.FromName(farbe)),
                         spielfelder[0, 0].x + X * spielfelder[0, 0].iwidth + 2,
                         spielfelder[0, 0].y + (i / multiplyer) * spielfelder[0, 0].iheight + 2,
                         spielfelder[0, 0].iwidth - 4,
                         spielfelder[0, 0].iheight - 4);
                    }
                    else
                    {
                        Task draw = new Task(() =>
                        {
                            iHilfszahl = i / multiplyer;

                            if (i / multiplyer + 1 < Y)
                            {
                                iHilfszahl1 = i / multiplyer + 1;
                            }
                            else
                            {
                                iHilfszahl1 = i / multiplyer;
                            }
                        
                            punkte.FillEllipse(new SolidBrush(this.BackColor),
                             spielfelder[0, 0].x + X * spielfelder[0, 0].iwidth + 2,
                             spielfelder[0, 0].y + ((i - 1) / multiplyer) * spielfelder[0, 0].iheight + 2,
                             spielfelder[0, 0].iwidth - 4, spielfelder[0, 0].iheight - 4);

                            punkte.FillEllipse(new SolidBrush(Color.FromName(farbe)),
                             spielfelder[0, 0].x + X * spielfelder[0, 0].iwidth + 2,
                             spielfelder[0, 0].y + ((i * spielfelder[0, 0].iheight) / multiplyer) + 2,
                             spielfelder[0, 0].iwidth - 4,
                             spielfelder[0, 0].iheight - 4);

                            spielfeldtilezeichnen(
                             spielfelder[X, iHilfszahl].x,
                             spielfelder[X, iHilfszahl].y,
                             spielfelder[X, iHilfszahl].iwidth,
                             spielfelder[X, iHilfszahl].iheight);

                            spielfeldtilezeichnen(
                             spielfelder[X, iHilfszahl1].x,
                             spielfelder[X, iHilfszahl1].y,
                             spielfelder[X, iHilfszahl1].iwidth,
                             spielfelder[X, iHilfszahl1].iheight);

                            
                        });
                        draw.RunSynchronously();
                    }
                    Thread.Sleep(200);
                }
            }
            );
            animation1.RunSynchronously();

            AimationFlag = false;
        }

        private void Kreiszeichnen(int X, int Y, string farbe)
        {
            punkte.FillEllipse(new SolidBrush(Color.FromName(farbe)),
             spielfelder[0, 0].x + X * spielfelder[0, 0].iwidth + 2,
             spielfelder[0, 0].y + Y * spielfelder[0, 0].iheight + 2,
             spielfelder[0, 0].iwidth - 4, spielfelder[0, 0].iheight - 4);
        }

        private void Gewonnen(string Gewinner)
        {
            //this.Hide();
            var Result = MessageBox.Show($"{Gewinner} Hat gewonnen",
                $"{Gewinner} Hat Gewonnen", MessageBoxButtons.RetryCancel, MessageBoxIcon.Information);

            if (Result == DialogResult.Retry)
            {
                Form2 frm = new Form2(Fullscreen);

                frm.Show();
                this.Hide();
            }
            if (Result == DialogResult.Cancel)
            {
                Form1 frm = new Form1();

                frm.Show();
                this.Hide();
            }
        }

        private void btn_Test_Click(object sender, EventArgs e)
        {
        }
    }
}

//http://www.bildungsgueter.de/CSharp/Pages/Beisp002Seite025.htm