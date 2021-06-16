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
        private string currentcolor;                    //Farbe die am Zug ist
        private float kreisausgleich;                   // zahl die die Kreisdicke ausgleichen soll
        private float droptime = 50;                    //wie langsam der stein fällt
        private int gewinnnummer;                       // anzahl zum Gewinnen nötiger steine in einer reihe

        public struct Spielfeldtile                     //representiert die einzelnen Felder mit position und Farbe
        {
            public int x, y, iwidth, iheight;
            public string farbe;
        }

        private DateTime VergangeneSekunden;

        private int iSpielfeldheightpx;                 //höhe des Spielfeldes in Pixel
        private int iSpielfeldwidthpx;                  //breite des Spielfeldes in Pixel

        public static int iSpielfeldheight = 4;         //spielfeldhöhe in spielfeldern
        public static int iSpielfeldwidth = 4;          //spielfeldbreite in spielfeldern

        private Bitmap Spielfeldframe;
        private Graphics Bitmapgraphic;

        private Graphics spielfeldgraphic;
        private PointF[,] Dreieckspunkte;
        private Graphics punkte;

        private bool AimationFlag = false;              //Animationsflag wird angeschalten wenn eine Animation stattfindet, da in dieser zeit nicht Redrawt werden soll

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

            droptime = droptime / iSpielfeldheight;   //die Fallgeschwindigkeit ist abhängik von der Spielfeldgröße
            VergangeneSekunden = new DateTime(1, 1, 1, 0, 0, 0);


            //this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.WindowState = FormWindowState.Normal;
            this.MaximizeBox = false;

            iSpielfeldheightpx = this.Height - 150;
            iSpielfeldwidthpx = this.Width - 50;

            spielfeldgraphic = this.CreateGraphics();

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
                if (iSpielfeldheightpx / iSpielfeldheight <= iSpielfeldwidthpx / iSpielfeldwidth)                               //spielfeldformat wird so gewählt, dass das Spielfeld immer in das Fenster passt
                {
                    ispielfeldformat = iSpielfeldheightpx / iSpielfeldheight;
                }
                else
                {
                    ispielfeldformat = iSpielfeldwidthpx / iSpielfeldwidth;
                }
                spielfelder[0, 0].x = (this.Width / 2) - (ispielfeldformat * iSpielfeldwidth / 2) + 0 * ispielfeldformat;      //Spielfelder werden erstmals erstellt
                spielfelder[0, 0].y = (this.Height / 2) - (ispielfeldformat * iSpielfeldheight / 2) + 0 * ispielfeldformat;
                spielfelder[0, 0].iwidth = ispielfeldformat;
                spielfelder[0, 0].iheight = ispielfeldformat;

                EckenBerechnen(0, 0, spielfelder[0, 0].iwidth, spielfelder[0, 0].iheight);
                SpielfeldErstellen();

                Spielfeldframe = new Bitmap(this.Width, this.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                Bitmapgraphic = Graphics.FromImage(Spielfeldframe);

                Bitmapgraphic.FillRectangle(new SolidBrush(this.BackColor), 0, 0, this.Width, this.Height);
                SpielfeldZeichnen(Bitmapgraphic);

                for (int x = 0; x < iSpielfeldwidth; x++)       // Im array alle Farben Auf Weiß setzen
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

        private void SpielfeldErstellen()
        {
            int ispielfeldformat;
            if (iSpielfeldheightpx / iSpielfeldheight <= iSpielfeldwidthpx / iSpielfeldwidth)               //spielfeldformat wird so gewählt, dass das Spielfeld immer in das Fenster passt

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
                    spielfelder[x, y].x = (this.Width / 2) - (ispielfeldformat * iSpielfeldwidth / 2) + x * ispielfeldformat;        //Struct wird angelegt
                    spielfelder[x, y].y = (this.Height / 2) - (ispielfeldformat * iSpielfeldheight / 2) + y * ispielfeldformat;
                    spielfelder[x, y].iwidth = ispielfeldformat;
                    spielfelder[x, y].iheight = ispielfeldformat;
                }
            }
        }

        private void SpielfeldZeichnen(Graphics G)
        {
            for (int x = 0; x < iSpielfeldwidth; x++)
            {
                for (int y = 0; y < iSpielfeldheight; y++)
                {
                    spielfeldtilezeichnen(spielfelder[x, y].x, spielfelder[x, y].y, spielfelder[x, y].iwidth, spielfelder[x, y].iheight, G);       //Struct wird benutzt um Das Spielfeld zu Zeichnen
                }
            }
            //spielfeldgraphic.DrawImage(Spielfeldframe, 0, 0);
        }

        private void SpielsteineZeichnen(Graphics G, int starty)
        {
            for (int x = 0; x < iSpielfeldwidth; x++)
            {
                for (int y = 0; y < iSpielfeldheight; y++)
                {
                    if (spielfelder[x, y].farbe != "white" && spielfelder[x, y].farbe != null)                                                  //wenn die Farbe des Kreises Nicht weis ist wird er auch noch gezeichnet
                    {
                        Kreiszeichnen(x, y, spielfelder[x, y].farbe, Bitmapgraphic, starty);
                    }
                }
            }
            //spielfeldgraphic.DrawImage(Spielfeldframe, 0, 0);
        }

        protected override void OnClosed(EventArgs e)
        {
            //wenn man mit X das Programm Schließet Schliest es sich Komlett mit einer Meldung
            AimationFlag = true;
            this.Hide();

            MessageBox.Show("Spiel Beendet",
                "Close Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            base.OnClosed(e);
            Environment.Exit(0);
            Application.Exit();

            AimationFlag = false;
        }

        private void button1_Click(object sender, EventArgs e)                  //Zurück zum Menü
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
        } //Die koordinaten für die Polygone werden im bezug auf das Jeweilige Feld Berechnet

        private void spielfeldtilezeichnen(int x, int y, int iwidth, int iheight, Graphics G)   //Methode um ein einzelnes Spielfeld-feld zu zeichnen
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
                    G.FillPolygon(new SolidBrush(Color.Blue), hilfsarray);
                }
                G.DrawEllipse(new Pen(Color.Blue, spielfelder[0, 0].iwidth / 22 + 1.8f), x, y, iwidth, iheight);
                G.DrawRectangle(new Pen(Color.Blue, spielfelder[0, 0].iwidth / 22 + 1.8f), x, y, iwidth, iheight);
                kreisausgleich = Convert.ToSingle(Math.Pow(spielfelder[0, 0].iwidth / 32f, 0.88f));
            });
            Feld.RunSynchronously();
        }

        //private void doublespielfeldtilezeichnen(int x, int y, int iwidth, int iheight, Graphics G)         //Methode um zwei Spielfelder auf einmal zu zeichnen (wird für die Fallanimation verwendet)
        //{
        //    PointF[] hilfsarray = new PointF[3];

        //    Task Feld = new Task(() =>
        //    {
        //        for (int j = 0; j < 2; j++)
        //        {
        //            hilfsarray[0] = new PointF(Dreieckspunkte[2, 1].X - 1 + x, Dreieckspunkte[2, 1].Y + y);
        //            hilfsarray[1] = new PointF(Dreieckspunkte[2, 2].X + x, Dreieckspunkte[2, 2].Y + y);
        //            hilfsarray[2] = new PointF(Dreieckspunkte[0, 2].X + x, Dreieckspunkte[0, 2].Y + y + iheight);
        //            spielfeldgraphic.FillPolygon(new SolidBrush(Color.Blue), hilfsarray);

        //            hilfsarray[0] = new PointF(Dreieckspunkte[3, 1].X + x, Dreieckspunkte[3, 1].Y + y);
        //            hilfsarray[1] = new PointF(Dreieckspunkte[3, 2].X + x, Dreieckspunkte[3, 2].Y + y);
        //            hilfsarray[2] = new PointF(Dreieckspunkte[1, 2].X + x, Dreieckspunkte[1, 2].Y + y + iheight);
        //            spielfeldgraphic.FillPolygon(new SolidBrush(Color.Blue), hilfsarray);
        //        }

        //        spielfeldgraphic.DrawEllipse(new Pen(Color.Blue, spielfelder[0, 0].iwidth / 20 + 1.5f), x, y, iwidth, iheight);
        //        spielfeldgraphic.DrawRectangle(new Pen(Color.Blue, spielfelder[0, 0].iwidth / 20 + 1.5f), x, y, iwidth, iheight);
        //        spielfeldgraphic.DrawEllipse(new Pen(Color.Blue, spielfelder[0, 0].iwidth / 20 + 1.5f), x, y + iheight, iwidth, iheight);
        //        spielfeldgraphic.DrawRectangle(new Pen(Color.Blue, spielfelder[0, 0].iwidth / 20 + 1.5f), x, y + iheight, iwidth, iheight);
        //        kreisausgleich = Convert.ToSingle(Math.Pow(spielfelder[0, 0].iwidth / 32f, 0.88f));

        //    });
        //    Feld.RunSynchronously();
        //}

        private void Form2_Paint(object sender, PaintEventArgs e)
        {
            if (!AimationFlag && !resizing)
            {
                spielfeldgraphic.DrawImage(Spielfeldframe, 0, 0);
            }
        }

        private void Form2_Click(object sender, EventArgs e)
        {
            if (this.PointToClient(Cursor.Position).X > spielfelder[0, 0].x && this.PointToClient(Cursor.Position).X < spielfelder[iSpielfeldwidth - 1, iSpielfeldheight - 1].x + spielfelder[iSpielfeldwidth - 1, iSpielfeldheight - 1].iwidth && this.PointToClient(Cursor.Position).Y > spielfelder[0, 0].y && this.PointToClient(Cursor.Position).Y < spielfelder[iSpielfeldwidth - 1, iSpielfeldheight - 1].y + spielfelder[iSpielfeldwidth - 1, iSpielfeldheight - 1].iheight)    //abfrage ob klick auf Spielfeld ist
            {
                int spalte, reihe = -1;
                bool gesetzt = false;
                spalte = (this.PointToClient(Cursor.Position).X - spielfelder[0, 0].x) / spielfelder[0, 0].iwidth;      //berechnung der Spalte
                for (int i = iSpielfeldheight - 1; i >= 0; i--)                                                         //zählt von unten nach oben...
                {
                    if (spielfelder[spalte, i].farbe == "white")                                                        //...Wenn die Farbe weis ist...
                    {
                        gesetzt = true;
                        spielfelder[spalte, i].farbe = currentcolor;
                        Hovereffekt(-1);                                                                                 //Hovereffekt wird während der Fallanimation entfernt
                        KreiszeichnenAnimation(spalte, i + 1, currentcolor, Bitmapgraphic);                              //...wird die animation abgespielt un das Feld Farbig
                        reihe = i;
                        i = 0;
                    }
                }

                Point[] Gewinnerkoordinaten = new Point[gewinnnummer];

                bool gewonnen = false;
                if (gesetzt && reihe != -1)                                                                             //wenn ein stein gesetzt wurde
                {
                    //überprüfung ob jemand gewonne hat
                    int infolge = 0;
                    for (int x = 0; x < iSpielfeldwidth && !gewonnen; x++)
                    {
                        if (spielfelder[x, reihe].farbe == currentcolor)
                        {
                            Gewinnerkoordinaten[infolge] = new Point(x, reihe);
                            infolge++;
                        }
                        else
                        {
                            infolge = 0;
                            for (int i = 0; i < gewinnnummer; i++)
                            {
                                Gewinnerkoordinaten[i] = new Point(0, 0);
                            }
                        }
                        if (infolge == gewinnnummer)
                        {
                            gewonnen = true;
                        }
                    }
                    for (int y = 0; y < iSpielfeldheight && !gewonnen; y++)
                    {
                        if (spielfelder[spalte, y].farbe == currentcolor)
                        {
                            Gewinnerkoordinaten[infolge] = new Point(spalte, y);
                            infolge++;
                        }
                        else
                        {
                            infolge = 0;
                            for (int i = 0; i < gewinnnummer; i++)
                            {
                                Gewinnerkoordinaten[i] = new Point(0, 0);
                            }
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
                    for (int xy = 0; xy < maxformat && !gewonnen; xy++)
                    {
                        if (xy + xabstand < iSpielfeldwidth && xy + xabstand >= 0 && xy < iSpielfeldheight && xy >= 0 && spielfelder[xy + xabstand, xy].farbe == currentcolor)
                        {
                            Gewinnerkoordinaten[infolge] = new Point(xy + xabstand, xy);
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

                    for (int xy = 0; xy < maxformat + 1 && !gewonnen; xy++)
                    {
                        if (iSpielfeldwidth - xy - xabstand - widthheightdif < iSpielfeldwidth && iSpielfeldwidth - xy - xabstand - widthheightdif >= 0 && xy - 1 < iSpielfeldheight && xy - 1 >= 0 && spielfelder[iSpielfeldwidth - xy - xabstand - widthheightdif, xy - 1].farbe == currentcolor)
                        {
                            Gewinnerkoordinaten[infolge] = new Point(iSpielfeldwidth - xy - xabstand - widthheightdif, xy - 1);
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
                        for (int i = 0; i < 4; i++)
                        {
                            Console.WriteLine(Gewinnerkoordinaten[i]);
                        }
                        KreisDrehen(Gewinnerkoordinaten, 5);
                        for (int i = 0; i < 4; i++)
                        {
                            Console.WriteLine(Gewinnerkoordinaten[i]);
                        }
                        AimationFlag = true;
                        for (int i = 0; i < this.Width; i += 10)
                        {
                            Bitmapgraphic.FillRectangle(new SolidBrush(this.BackColor), 0, 0, this.Width, this.Height);
                            SpielsteineZeichnen(Bitmapgraphic, i);
                            SpielfeldZeichnen(Bitmapgraphic);
                            spielfeldgraphic.DrawImage(Spielfeldframe, 0, 0);
                        }
                        AimationFlag = false;

                        
                        if(currentcolor == "red")
                        {
                            Gewonnen("Rot");
                        }
                        else
                        {
                            Gewonnen("Gelb");
                        }
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

                    //überpfrüfung ob noch einzug möglich ist
                    bool zugmöglich = false;
                    for (int x = 0; x < iSpielfeldwidth; x++)
                    {
                        for (int y = 0; y < iSpielfeldheight; y++)
                        {
                            if (spielfelder[x, y].farbe == "white")             // wenn mindestens ein Feld weis ist, ist noch ein zug möglich
                            {
                                zugmöglich = true;
                            }
                        }
                    }
                    if (!zugmöglich)
                    {
                        Gewonnen("niemand");
                    }
                }
            }
        }

        private void KreisDrehen(Point[] Kreiskoordinaten, int Drehungen)
        {
            int Drehgeschwindigkeit = 5;
            for (int k = 0; k < Drehungen; k++)                         //schleife pro Drehung
            {
                for (int j = spielfelder[0, 0].iwidth; j >= 0; j -= Drehgeschwindigkeit)       //schleife für halbe umdrehung
                {
                    for (int i = 0; i < Kreiskoordinaten.Length; i++)   //schleife für jeden Kreis
                    {
                        Bitmapgraphic.FillEllipse(new SolidBrush(this.BackColor),
                            spielfelder[Kreiskoordinaten[i].X, Kreiskoordinaten[i].Y].x + kreisausgleich,
                            spielfelder[Kreiskoordinaten[i].X, Kreiskoordinaten[i].Y].y + kreisausgleich,
                            spielfelder[0, 0].iwidth - kreisausgleich * 2,
                            spielfelder[0, 0].iheight - kreisausgleich * 2);

                        Bitmapgraphic.FillEllipse(new SolidBrush(Color.FromName(spielfelder[Kreiskoordinaten[i].X, Kreiskoordinaten[i].Y].farbe)),
                            spielfelder[Kreiskoordinaten[i].X, Kreiskoordinaten[i].Y].x + kreisausgleich + ((spielfelder[0, 0].iwidth - j) / 2),
                            spielfelder[Kreiskoordinaten[i].X, Kreiskoordinaten[i].Y].y + kreisausgleich,
                            j,
                            spielfelder[0, 0].iheight - kreisausgleich * 2);

                        spielfeldtilezeichnen(
                            spielfelder[Kreiskoordinaten[i].X, Kreiskoordinaten[i].Y].x,
                            spielfelder[Kreiskoordinaten[i].X, Kreiskoordinaten[i].Y].y,
                            spielfelder[0, 0].iwidth,
                            spielfelder[0, 0].iheight,
                            Bitmapgraphic);
                    }
                    spielfeldgraphic.DrawImage(Spielfeldframe, 0, 0);
                }
                for (int j = 0; j <= spielfelder[0, 0].iwidth; j += Drehgeschwindigkeit)      //schleife für andere hälfte
                {
                    for (int i = 0; i < Kreiskoordinaten.Length; i++)   //schleife für jeden Kreis
                    {
                        Bitmapgraphic.FillEllipse(new SolidBrush(this.BackColor),
                            spielfelder[Kreiskoordinaten[i].X, Kreiskoordinaten[i].Y].x + kreisausgleich,
                            spielfelder[Kreiskoordinaten[i].X, Kreiskoordinaten[i].Y].y + kreisausgleich,
                            spielfelder[0, 0].iwidth - kreisausgleich * 2,
                            spielfelder[0, 0].iheight - kreisausgleich * 2);

                        Bitmapgraphic.FillEllipse(new SolidBrush(Color.FromName(spielfelder[Kreiskoordinaten[i].X, Kreiskoordinaten[i].Y].farbe)),
                            spielfelder[Kreiskoordinaten[i].X, Kreiskoordinaten[i].Y].x + kreisausgleich + ((spielfelder[0, 0].iwidth - j) / 2),
                            spielfelder[Kreiskoordinaten[i].X, Kreiskoordinaten[i].Y].y + kreisausgleich,
                            j,
                            spielfelder[0, 0].iheight - kreisausgleich * 2);

                        spielfeldtilezeichnen(
                            spielfelder[Kreiskoordinaten[i].X, Kreiskoordinaten[i].Y].x,
                            spielfelder[Kreiskoordinaten[i].X, Kreiskoordinaten[i].Y].y,
                            spielfelder[0, 0].iwidth,
                            spielfelder[0, 0].iheight,
                            Bitmapgraphic);
                    }
                    spielfeldgraphic.DrawImage(Spielfeldframe, 0, 0);
                }
            }
        }

        private void KreiszeichnenAnimation(int X, int Y, string farbe, Graphics G)
        {
            int iHilfszahl = 0;
            int iHilfszahl1 = 0;
            int multiplyer = (int)droptime; // durch 2 teil bare Zahlen funktionieren am besten da Dann Weniger Komma stellen Entstehen die Ignoriert werden
            Task animation1 = new Task(() =>
            {
                AimationFlag = true;

                for (int i = -multiplyer; i < Y * multiplyer; i++)
                {
                    if (i == -multiplyer || i + 1 == Y * multiplyer || Convert.ToInt32((i / multiplyer) + 1) >= Y)
                    {
                        G.FillEllipse(new SolidBrush(this.BackColor),
                        spielfelder[0, 0].x + X * spielfelder[0, 0].iwidth + kreisausgleich,
                        spielfelder[0, 0].y + ((i - 1) / multiplyer) * spielfelder[0, 0].iheight + kreisausgleich,
                        spielfelder[0, 0].iwidth - kreisausgleich * 2,
                        spielfelder[0, 0].iheight - kreisausgleich * 2);

                        G.FillEllipse(new SolidBrush(Color.FromName(farbe)),
                         spielfelder[0, 0].x + X * spielfelder[0, 0].iwidth + kreisausgleich,
                         spielfelder[0, 0].y + (i / multiplyer) * spielfelder[0, 0].iheight + kreisausgleich,
                         spielfelder[0, 0].iwidth - kreisausgleich * 2,
                         spielfelder[0, 0].iheight - kreisausgleich * 2);
                        if (i + 1 == Y * multiplyer && i / multiplyer > 0)
                        {
                            spielfeldtilezeichnen(
                                spielfelder[X, iHilfszahl].x,
                                spielfelder[X, i / multiplyer - 1].y,
                                spielfelder[X, iHilfszahl].iwidth,
                                spielfelder[X, iHilfszahl].iheight,
                                Bitmapgraphic);
                        }
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
                                G.FillRectangle(new SolidBrush(this.BackColor),
                                 spielfelder[0, 0].x + X * spielfelder[0, 0].iwidth + kreisausgleich,
                                 spielfelder[0, 0].y + (((i - 1) * spielfelder[0, 0].iheight) / multiplyer) + kreisausgleich,
                                 spielfelder[0, 0].iwidth - kreisausgleich * 2, spielfelder[0, 0].iheight - kreisausgleich * 2);
                            }
                            else
                            {
                                G.FillRectangle(new SolidBrush(this.BackColor),
                                 spielfelder[0, 0].x + X * spielfelder[0, 0].iwidth + kreisausgleich,
                                 spielfelder[0, 0].y + ((i - 1) / multiplyer) * spielfelder[0, 0].iheight + kreisausgleich,
                                 spielfelder[0, 0].iwidth - kreisausgleich * 2, spielfelder[0, 0].iheight - kreisausgleich * 2);
                            }

                            G.FillEllipse(new SolidBrush(Color.FromName(farbe)),
                             spielfelder[0, 0].x + X * spielfelder[0, 0].iwidth + kreisausgleich,
                             spielfelder[0, 0].y + ((i * spielfelder[0, 0].iheight) / multiplyer) + kreisausgleich,
                             spielfelder[0, 0].iwidth - kreisausgleich * 2,
                             spielfelder[0, 0].iheight - kreisausgleich * 2);

                            spielfeldtilezeichnen(
                                spielfelder[X, iHilfszahl].x,
                                spielfelder[X, iHilfszahl + 1].y,
                                spielfelder[X, iHilfszahl].iwidth,
                                spielfelder[X, iHilfszahl].iheight,
                                Bitmapgraphic);

                            spielfeldtilezeichnen(
                              spielfelder[X, iHilfszahl].x,
                              spielfelder[X, iHilfszahl].y,
                              spielfelder[X, iHilfszahl].iwidth,
                              spielfelder[X, iHilfszahl].iheight,
                              Bitmapgraphic);
                            if (iHilfszahl - 1 >= 0)
                            {
                                spielfeldtilezeichnen(
                              spielfelder[X, iHilfszahl].x,
                              spielfelder[X, iHilfszahl - 1].y,
                              spielfelder[X, iHilfszahl].iwidth,
                              spielfelder[X, iHilfszahl].iheight,
                              Bitmapgraphic);
                            }

                            spielfeldgraphic.DrawImage(Spielfeldframe, 0, 0);
                        });
                        draw.RunSynchronously();
                    }
                    //Thread.Sleep((int)(100 / dropspeed));
                }
            }
            );
            animation1.RunSynchronously();

            SpielsteineZeichnen(Bitmapgraphic, 0);
            spielfeldgraphic.DrawImage(Spielfeldframe, 0, 0);

            AimationFlag = false;
        }

        private void Kreiszeichnen(int X, int Y, string farbe, Graphics G, int starty)
        {
            G.FillEllipse(new SolidBrush(Color.FromName(farbe)),
             spielfelder[0, 0].x + X * spielfelder[0, 0].iwidth + kreisausgleich,
             spielfelder[0, 0].y + Y * spielfelder[0, 0].iheight + kreisausgleich + starty,
             spielfelder[0, 0].iwidth - kreisausgleich * 2, spielfelder[0, 0].iheight - kreisausgleich * 2);
        }

        private void Gewonnen(string Gewinner)
        {
            //this.Hide();
            var Result = MessageBox.Show($"{Gewinner} Hat gewonnen",
                $"{Gewinner} hat Gewonnen", MessageBoxButtons.RetryCancel, MessageBoxIcon.Information);

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
            if (this.PointToClient(Cursor.Position).X > spielfelder[0, 0].x && this.PointToClient(Cursor.Position).X < spielfelder[iSpielfeldwidth - 1, iSpielfeldheight - 1].x + spielfelder[iSpielfeldwidth - 1, iSpielfeldheight - 1].iwidth && this.PointToClient(Cursor.Position).Y > spielfelder[0, 0].y && this.PointToClient(Cursor.Position).Y < spielfelder[iSpielfeldwidth - 1, iSpielfeldheight - 1].y + spielfelder[iSpielfeldwidth - 1, iSpielfeldheight - 1].iheight)    //überprüfung ob die Maus auf dem Spielfeld hovert
            {
                if (oldspalte != (this.PointToClient(Cursor.Position).X - spielfelder[0, 0].x) / spielfelder[0, 0].iwidth)      //überprüfung ob die maus über einem anderen fällt ist als bei der Letzten abfrage
                {
                    spalte = (this.PointToClient(Cursor.Position).X - spielfelder[0, 0].x) / spielfelder[0, 0].iwidth;
                    Hovereffekt(spalte);
                }
            }
            else
            {
                Hovereffekt(-1);        //wenn die Maus nichtmehr auf dem Feld ist wird der Hovereffekt aufgehoben
            }
        }

        private int oldspalte = 0;

        private void Hovereffekt(int spalte)
        {
            if (spalte >= 0)
            {
                if (oldspalte >= 0)
                {
                    // hoverkreis entfernen:
                    Bitmapgraphic.FillEllipse(new SolidBrush(Color.FromName("white")), spielfelder[oldspalte, 0].x + kreisausgleich, spielfelder[oldspalte, 0].y - spielfelder[0, 0].iheight + kreisausgleich - 2, spielfelder[0, 0].iwidth - kreisausgleich * 2, spielfelder[0, 0].iheight - kreisausgleich * 2);
                }
                // hoverkreis zeichnen:
                Bitmapgraphic.FillEllipse(new SolidBrush(Color.FromName(currentcolor)), spielfelder[spalte, 0].x + kreisausgleich, spielfelder[spalte, 0].y - spielfelder[0, 0].iheight + kreisausgleich - 2, spielfelder[0, 0].iwidth - kreisausgleich * 2, spielfelder[0, 0].iheight - kreisausgleich * 2);
                oldspalte = spalte;
            }
            else        // wenn minus 1 übergeben wird
            {
                if (oldspalte >= 0)
                {
                    // hoverkreis entfernen:
                    Bitmapgraphic.FillEllipse(new SolidBrush(Color.FromName("white")), spielfelder[oldspalte, 0].x + kreisausgleich, spielfelder[oldspalte, 0].y - spielfelder[0, 0].iheight + kreisausgleich - 2, spielfelder[0, 0].iwidth - kreisausgleich * 2, spielfelder[0, 0].iheight - kreisausgleich * 2);
                }
                oldspalte = spalte;
            }
            spielfeldgraphic.DrawImage(Spielfeldframe, 0, 0);

        }

        private bool resizing = false;

        private void Form2_Resize(object sender, EventArgs e)
        {
            if (Bitmapgraphic != null)
            {
                Spielfeldframe = new Bitmap(this.Width, this.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                Bitmapgraphic.FillRectangle(new SolidBrush(this.BackColor), 0, 0, this.Width, this.Height);  // wenn das Spielfeld neugezogen wird, wird es weis
            }
            resizing = true;
        }

        private void Form2_ResizeEnd(object sender, EventArgs e)                                                // wenn das Spielfeld losgelassen wird, wird dass Spielfeld neu gezeichnet
        {
            if (resizing)
            {

                resizing = false;
                iSpielfeldheightpx = this.Height - 200;
                iSpielfeldwidthpx = this.Width - 50;
                SpielfeldErstellen();
                EckenBerechnen(0, 0, spielfelder[0, 0].iwidth, spielfelder[0, 0].iheight);

                Spielfeldframe = new Bitmap(this.Width, this.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                Bitmapgraphic = Graphics.FromImage(Spielfeldframe);
                spielfeldgraphic = this.CreateGraphics();

                Bitmapgraphic.FillRectangle(new SolidBrush(this.BackColor), 0, 0, this.Width, this.Height);
                SpielsteineZeichnen(Bitmapgraphic, 0);

                SpielfeldZeichnen(Bitmapgraphic);

                spielfeldgraphic.DrawImage(Spielfeldframe, 0, 0);

                //Bitmapgraphic.FillRectangle(new SolidBrush(this.BackColor), 0, 0, this.Width, this.Height);
            }
        }
    }
}

//http://www.bildungsgueter.de/CSharp/Pages/Beisp002Seite025.htm