using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VierGewinnt
{
    public partial class Form2 : Form
    {

        private bool Fullscreen;
        private string currentcolor;
        private float kreisausgleich = 2.501f;
        private float dropspeed = 60; //geschwindigkeit beim runterfallen
        private int gewinnnummer;


        public struct Spielfeldtile
        {
            public int x, y, iwidth, iheight;
            public string farbe;
        }


        private DateTime VergangeneSekunden;

        private int iSpielfeldheightpx;
        private int iSpielfeldwidthpx;

        public static int iSpielfeldheight = 4;
        public static int iSpielfeldwidth = 4;

        private Graphics spielfeldgraphic;
        private PointF[,] Dreieckspunkte;
        private Graphics punkte;

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
            DoubleBuffered = true;
            Fullscreen = _Fullscreen;
            AllocConsole();

            dropspeed = dropspeed * iSpielfeldheight;
            VergangeneSekunden = new DateTime(1, 1, 1, 0, 0, 0);

            if (_Fullscreen == true)
            {
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;

                this.MaximizeBox = true;
            }
            else
            {
                //this.FormBorderStyle = FormBorderStyle.FixedSingle;
                this.WindowState = FormWindowState.Normal;
                this.MaximizeBox = true;
            }
            iSpielfeldheightpx = this.Height - 150;
            iSpielfeldwidthpx = this.Width - 150;

            spielfeldgraphic = this.CreateGraphics();

            punkte = this.CreateGraphics();

            switch (Form1.sgewinnZahl)
            {
                case ("Zwei"):
                    gewinnnummer = 2;
                    break;
                case ("Drei"):
                    gewinnnummer = 3;
                    break;
                case ("Vier"):
                    gewinnnummer = 4;
                    break;
                case ("Fünf"):
                    gewinnnummer = 5;
                    break;
                case ("Sechs"):
                    gewinnnummer = 6;
                    break;
                case ("Sieben"):
                    gewinnnummer = 7;
                    break;
                case ("Acht"):
                    gewinnnummer = 8;
                    break;
                case ("Neun"):
                    gewinnnummer = 9;
                    break;
                case ("Zehn"):
                    gewinnnummer = 10;
                    break;
                default:
                    gewinnnummer = 4;
                    break;
            }

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

                EckenBerechnen(0, 0, spielfelder[0, 0].iwidth, spielfelder[0, 0].iheight);
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


#pragma warning disable CS4014 // Da auf diesen Aufruf nicht gewartet wird, wird die Ausführung der aktuellen Methode vor Abschluss des Aufrufs fortgesetzt.
                UhrStarten();
#pragma warning restore CS4014 // Da auf diesen Aufruf nicht gewartet wird, wird die Ausführung der aktuellen Methode vor Abschluss des Aufrufs fortgesetzt.
             
            });
        }

#pragma warning disable CS1998 // Bei der asynchronen Methode fehlen "await"-Operatoren. Die Methode wird synchron ausgeführt.
        private async Task UhrStarten()
#pragma warning restore CS1998 // Bei der asynchronen Methode fehlen "await"-Operatoren. Die Methode wird synchron ausgeführt.
        {
            timer.Tick += new EventHandler(UhrUpdate);
            timer.Interval = 1000;
            timer.Start();
            timer.Enabled = true;
        }

        private void SpielfeldZeichnen()
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

        private void doublespielfeldtilezeichnen(int x, int y, int iwidth, int iheight)
        {
            PointF[] hilfsarray = new PointF[3];

            Task Feld = new Task(() =>
            {
                for (int j = 0; j < 2; j++)
                {
                    hilfsarray[0] = new PointF(Dreieckspunkte[2, 1].X - 1 + x, Dreieckspunkte[2, 1].Y + y);
                    hilfsarray[1] = new PointF(Dreieckspunkte[2, 2].X + x, Dreieckspunkte[2, 2].Y + y);
                    hilfsarray[2] = new PointF(Dreieckspunkte[0, 2].X + x, Dreieckspunkte[0, 2].Y + y + iheight);
                    spielfeldgraphic.FillPolygon(new SolidBrush(Color.Blue), hilfsarray);

                    hilfsarray[0] = new PointF(Dreieckspunkte[3, 1].X + x, Dreieckspunkte[3, 1].Y + y);
                    hilfsarray[1] = new PointF(Dreieckspunkte[3, 2].X + x, Dreieckspunkte[3, 2].Y + y);
                    hilfsarray[2] = new PointF(Dreieckspunkte[1, 2].X + x, Dreieckspunkte[1, 2].Y + y + iheight);
                    spielfeldgraphic.FillPolygon(new SolidBrush(Color.Blue), hilfsarray);
                }

                spielfeldgraphic.DrawEllipse(new Pen(Color.Blue, 5), x, y, iwidth, iheight);
                spielfeldgraphic.DrawRectangle(new Pen(Color.Blue, 5), x, y, iwidth, iheight);
                spielfeldgraphic.DrawEllipse(new Pen(Color.Blue, 5), x, y + iheight, iwidth, iheight);
                spielfeldgraphic.DrawRectangle(new Pen(Color.Blue, 5), x, y + iheight, iwidth, iheight);
            });
            Feld.RunSynchronously();
        }

        private void Form2_Paint(object sender, PaintEventArgs e)
        {
            if (AimationFlag == false)
            {
                Console.WriteLine("redraw");
                SpielfeldZeichnen();
            }
        }


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
                        Hovereffekt(-1);
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
                    int widthheightdif;
                    xabstand = spalte - reihe;
                    widthheightdif = iSpielfeldwidth - iSpielfeldheight;
                    if (iSpielfeldheight > iSpielfeldwidth)
                    {
                        maxformat = iSpielfeldheight;
                    }
                    else
                    {
                        maxformat = iSpielfeldwidth;
                    }
                    for (int xy = 0; xy < maxformat; xy++)
                    {
                        if (xy + xabstand < iSpielfeldwidth && xy + xabstand >= 0 && xy < iSpielfeldheight && xy >= 0 && spielfelder[xy + xabstand, xy].farbe == currentcolor)
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
                    xabstand = -1 + (spalte - reihe + (iSpielfeldheight - (spalte * 2)));

                    for (int xy = 0; xy < maxformat + 1; xy++)
                    {
                        if (iSpielfeldwidth - xy - xabstand - widthheightdif < iSpielfeldwidth && iSpielfeldwidth - xy - xabstand - widthheightdif >= 0 && xy - 1 < iSpielfeldheight && xy - 1 >= 0 && spielfelder[iSpielfeldwidth - xy - xabstand - widthheightdif, xy - 1].farbe == currentcolor)
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
                    bool zugmöglich = false;
                    for (int x = 0; x < iSpielfeldwidth; x++)
                    {
                        for (int y = 0; y < iSpielfeldheight; y++)
                        {
                            if (spielfelder[x, y].farbe == "white")
                            {
                                zugmöglich = true;
                            }
                        }
                    }
                    if (!zugmöglich)
                    {
                        Console.WriteLine("Ende");
                    }
                }
            }
            Console.WriteLine(this.PointToClient(new Point((Cursor.Position).X, (Cursor.Position).Y)));
        }

        private void KreiszeichnenAnimation(int X, int Y, string farbe)
        {
            int iHilfszahl = 0;
            int iHilfszahl1 = 0;
            int multiplyer = 8; // durch 2 teil bare Zahlen funktionieren am besten da Dann Weniger Komma stellen Entstehen die Ignoriert werden
            Task animation1 = new Task(() =>
            {
                AimationFlag = true;

                for (int i = -multiplyer; i < Y * multiplyer; i += 1)
                {
                    if (i == -multiplyer || i + 1 == Y * multiplyer || Convert.ToInt32((i / multiplyer) + 1) >= Y)
                    {
                        punkte.FillEllipse(new SolidBrush(this.BackColor),
                        spielfelder[0, 0].x + X * spielfelder[0, 0].iwidth + kreisausgleich,
                        spielfelder[0, 0].y + ((i - 1) / multiplyer) * spielfelder[0, 0].iheight + kreisausgleich,
                        spielfelder[0, 0].iwidth - kreisausgleich * 2, spielfelder[0, 0].iheight - kreisausgleich * 2);

                        punkte.FillEllipse(new SolidBrush(Color.FromName(farbe)),
                         spielfelder[0, 0].x + X * spielfelder[0, 0].iwidth + kreisausgleich,
                         spielfelder[0, 0].y + (i / multiplyer) * spielfelder[0, 0].iheight + kreisausgleich,
                         spielfelder[0, 0].iwidth - kreisausgleich * 2,
                         spielfelder[0, 0].iheight - kreisausgleich * 2);
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

                            if (i <= 0)
                            {
                                punkte.FillEllipse(new SolidBrush(this.BackColor),
                                 spielfelder[0, 0].x + X * spielfelder[0, 0].iwidth + kreisausgleich,
                                 spielfelder[0, 0].y + (((i - 1) * spielfelder[0, 0].iheight) / multiplyer) + kreisausgleich,
                                 spielfelder[0, 0].iwidth - kreisausgleich * 2, spielfelder[0, 0].iheight - kreisausgleich * 2);
                            }
                            else
                            {
                                punkte.FillEllipse(new SolidBrush(this.BackColor),
                                 spielfelder[0, 0].x + X * spielfelder[0, 0].iwidth + kreisausgleich,
                                 spielfelder[0, 0].y + ((i - 1) / multiplyer) * spielfelder[0, 0].iheight + kreisausgleich,
                                 spielfelder[0, 0].iwidth - kreisausgleich * 2, spielfelder[0, 0].iheight - kreisausgleich * 2);
                            }

                            punkte.FillEllipse(new SolidBrush(Color.FromName(farbe)),
                         spielfelder[0, 0].x + X * spielfelder[0, 0].iwidth + kreisausgleich,
                         spielfelder[0, 0].y + ((i * spielfelder[0, 0].iheight) / multiplyer) + kreisausgleich,
                         spielfelder[0, 0].iwidth - kreisausgleich * 2,
                         spielfelder[0, 0].iheight - kreisausgleich * 2);
                            if (iHilfszahl >= 0)
                            {
                                doublespielfeldtilezeichnen(
                                  spielfelder[X, iHilfszahl].x,
                                  spielfelder[X, iHilfszahl].y,
                                  spielfelder[X, iHilfszahl].iwidth,
                                  spielfelder[X, iHilfszahl].iheight);
                            }
                            if(i<=0)
                            {
                                spielfeldtilezeichnen(
                                  spielfelder[X, iHilfszahl].x,
                                  spielfelder[X, iHilfszahl].y,
                                  spielfelder[X, iHilfszahl].iwidth,
                                  spielfelder[X, iHilfszahl].iheight);
                            }
                        });
                        draw.RunSynchronously();
                    }
                    Thread.Sleep((int)(1000 / dropspeed));
                }
            }
            );
            animation1.RunSynchronously();

            AimationFlag = false;
        }

        private void Kreiszeichnen(int X, int Y, string farbe)
        {
            punkte.FillEllipse(new SolidBrush(Color.FromName(farbe)),
             spielfelder[0, 0].x + X * spielfelder[0, 0].iwidth + kreisausgleich,
             spielfelder[0, 0].y + Y * spielfelder[0, 0].iheight + kreisausgleich,
             spielfelder[0, 0].iwidth - kreisausgleich * 2, spielfelder[0, 0].iheight - kreisausgleich * 2);
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

        private void UhrUpdate(Object Obj, EventArgs e)
        {
            VergangeneSekunden = VergangeneSekunden.AddSeconds(1);
            lab_Timer.Text = VergangeneSekunden.ToLongTimeString();
            //lab_Timer.Text = DateTime.Now.ToLongTimeString();
        }

        private void Form2_MouseMove(object sender, MouseEventArgs e)
        {
            int spalte = -1;
            if (this.PointToClient(Cursor.Position).X > spielfelder[0, 0].x && this.PointToClient(Cursor.Position).X < spielfelder[iSpielfeldwidth - 1, iSpielfeldheight - 1].x + spielfelder[iSpielfeldwidth - 1, iSpielfeldheight - 1].iwidth && this.PointToClient(Cursor.Position).Y > spielfelder[0, 0].y && this.PointToClient(Cursor.Position).Y < spielfelder[iSpielfeldwidth - 1, iSpielfeldheight - 1].y + spielfelder[iSpielfeldwidth - 1, iSpielfeldheight - 1].iheight)
            {
                if (oldspalte != (this.PointToClient(Cursor.Position).X - spielfelder[0, 0].x) / spielfelder[0, 0].iwidth)
                {
                    spalte = (this.PointToClient(Cursor.Position).X - spielfelder[0, 0].x) / spielfelder[0, 0].iwidth;
                    Hovereffekt(spalte);
                }
            }
            else
            {
                Hovereffekt(-1);
            }
        }

        private int oldspalte = 0;

        private void Hovereffekt(int spalte)
        {

            if (spalte >= 0)
            {
                if (oldspalte >= 0)
                {
                    punkte.FillEllipse(new SolidBrush(Color.FromName("white")), spielfelder[oldspalte, 0].x + kreisausgleich, spielfelder[oldspalte, 0].y - spielfelder[0, 0].iheight + kreisausgleich, spielfelder[0, 0].iwidth - kreisausgleich * 2, spielfelder[0, 0].iheight - kreisausgleich * 2);
                }
                punkte.FillEllipse(new SolidBrush(Color.FromName(currentcolor)), spielfelder[spalte, 0].x + kreisausgleich, spielfelder[spalte, 0].y - spielfelder[0, 0].iheight + kreisausgleich, spielfelder[0, 0].iwidth - kreisausgleich * 2, spielfelder[0, 0].iheight - kreisausgleich * 2);
                oldspalte = spalte;
            }
            else
            {
                if (oldspalte >= 0)
                {
                    punkte.FillEllipse(new SolidBrush(Color.FromName("white")), spielfelder[oldspalte, 0].x + kreisausgleich, spielfelder[oldspalte, 0].y - spielfelder[0, 0].iheight + kreisausgleich, spielfelder[0, 0].iwidth - kreisausgleich * 2, spielfelder[0, 0].iheight - kreisausgleich * 2);
                }
                oldspalte = spalte;
            }
        }
    }
}

//http://www.bildungsgueter.de/CSharp/Pages/Beisp002Seite025.htm