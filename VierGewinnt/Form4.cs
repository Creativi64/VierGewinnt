using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VierGewinnt
{
    internal struct Spielfeld
    {
        public string[,] spielstein;
        public int irating;
    }

    public partial class Form4 : Form
    {
        private string sBotColor = "red";
        public static int iSpielfeldHeight = 4;//spielfeldhöhe in spielfeldern
        public static int iSpielfeldWidth = 4; //spielfeldbreite in spielfeldern

        public SpielfeldTile[,] Spielfelder;
        private PointF[,] DreiecksPunkte;

        private string sCurrentcolor;//Farbe die am Zug ist

        private float dKreisausgleich;// zahl die die Kreisdicke ausgleichen soll
        private float fDroptime = 50;//wie langsam der stein fällt

        private int iGewinnAnzahl; // anzahl zum Gewinnen nötiger steine in einer reihe
        private int iOldSpalte = 0; //Für hover effekt letzte spalte, damit nicht redrawt wird wenn es die gleiche Zeile ist
        private int iSpielfeldHeightPx;//höhe des Spielfeldes in Pixel
        private int iSpielfeldWidthPx;//breite des Spielfeldes in Pixel

        private bool bAimationFlag = false;//Animationsflag wird angeschalten wenn eine Animation stattfindet, da in dieser zeit nicht Redrawt werden soll
        private bool bResizing = false;

        private Bitmap Spielfeldframe;

        private Graphics Spielfeldgraphic;
        private Graphics Bitmapgraphic;

        private DateTime VergangeneSekunden;

        public struct SpielfeldTile                     //representiert die einzelnen Felder mit position und Farbe
        {
            public string sFarbe;
            public int iX, iY, iWidth, iHeight;
        }

        #region Console

        /// <summary>
        /// Erlaubt Uns Zum Form eine console zu starten
        /// </summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AllocConsole();

        #endregion Console

        public Form4()
        {
            InitializeComponent();
            DoubleBuffered = true;

            AllocConsole();

            fDroptime = fDroptime / iSpielfeldHeight;   //die Fallgeschwindigkeit ist abhängik von der Spielfeldgröße
            VergangeneSekunden = new DateTime(1, 1, 1, 0, 0, 0);

            //this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.WindowState = FormWindowState.Normal;
            this.MaximizeBox = false;

            iSpielfeldHeightPx = this.Height - 150;
            iSpielfeldWidthPx = this.Width - 50;

            Spielfeldgraphic = this.CreateGraphics();

            switch (Form1.sGewinnAnzahl)
            {
                case ("Zwei"):
                    iGewinnAnzahl = 2;
                    break;

                case ("Drei"):
                    iGewinnAnzahl = 3;
                    break;

                case ("Vier"):
                    iGewinnAnzahl = 4;
                    break;

                case ("Fünf"):
                    iGewinnAnzahl = 5;
                    break;

                case ("Sechs"):
                    iGewinnAnzahl = 6;
                    break;

                case ("Sieben"):
                    iGewinnAnzahl = 7;
                    break;

                case ("Acht"):
                    iGewinnAnzahl = 8;
                    break;

                case ("Neun"):
                    iGewinnAnzahl = 9;
                    break;

                case ("Zehn"):
                    iGewinnAnzahl = 10;
                    break;

                default:
                    iGewinnAnzahl = 4;
                    break;
            }

            this.BeginInvoke((MethodInvoker)delegate
            {
                // wird Aufgerufen wenn Das From Geladen Wurde

                Spielfelder = new SpielfeldTile[iSpielfeldWidth, iSpielfeldHeight];
                Thread.Sleep(400);

                //Erste Ecken Berechnen
                int ispielfeldformat;
                if (iSpielfeldHeightPx / iSpielfeldHeight <= iSpielfeldWidthPx / iSpielfeldWidth)                               //spielfeldformat wird so gewählt, dass das Spielfeld immer in das Fenster passt
                {
                    ispielfeldformat = iSpielfeldHeightPx / iSpielfeldHeight;
                }
                else
                {
                    ispielfeldformat = iSpielfeldWidthPx / iSpielfeldWidth;
                }
                Spielfelder[0, 0].iX = (this.Width / 2) - (ispielfeldformat * iSpielfeldWidth / 2) + 0 * ispielfeldformat;      //Spielfelder werden erstmals erstellt
                Spielfelder[0, 0].iY = (this.Height / 2) - (ispielfeldformat * iSpielfeldHeight / 2) + 0 * ispielfeldformat;
                Spielfelder[0, 0].iWidth = ispielfeldformat;
                Spielfelder[0, 0].iHeight = ispielfeldformat;

                EckenBerechnen(Spielfelder[0, 0].iWidth, Spielfelder[0, 0].iHeight);
                SpielfeldErstellen();

                Spielfeldframe = new Bitmap(this.Width, this.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                Bitmapgraphic = Graphics.FromImage(Spielfeldframe);

                Bitmapgraphic.FillRectangle(new SolidBrush(this.BackColor), 0, 0, this.Width, this.Height);
                SpielfeldZeichnen(Bitmapgraphic);

                for (int x = 0; x < iSpielfeldWidth; x++)       // Im array alle Farben Auf Weiß setzen
                {
                    for (int y = 0; y < iSpielfeldHeight; y++)
                    {
                        Spielfelder[x, y].sFarbe = "white";
                    }
                }

                // zufällige start Fahrbe Wählen

                sCurrentcolor = "yellow";
                lab_Player.Text = "Player Yellow";

                UhrStarten();
            });
        }

        protected override void OnClosed(EventArgs e)
        {
            //wenn man mit X das Programm Schließet Schliest es sich Komlett mit einer Meldung
            bAimationFlag = true;
            this.Hide();

            MessageBox.Show("Spiel Beendet",
                "Close Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            base.OnClosed(e);
            Environment.Exit(0);
            Application.Exit();

            bAimationFlag = false;
        }

        private void Button1_Click(object sender, EventArgs e)  //Zurück zum Menü
        {
            Form1 frm = new();

            frm.Show();
            this.Hide();
        }

        /// <summary>
        /// Diese Funktion Berechnet die Eckpunkte für ein Quadrat und speichert sie in dem Array Dreieckspunkte
        /// </summary>
        /// <param name="iWidth">Die Breite des Spielfeldes </param>
        /// <param name="iHeight">Die Breite des Spielfeldes </param>
        private void EckenBerechnen(int iWidth, int iHeight)
        {
            int iX = 0;
            int iY = 0;
            //Die koordinaten für die Polygone werden im bezug auf das Jeweilige Feld Berechnet
            float dDreieckkprozent = 0.3F;
            DreiecksPunkte = new PointF[4, 3];
            DreiecksPunkte[0, 0] = new PointF(iX, iY);
            DreiecksPunkte[0, 1] = new PointF((float)(iX + (iWidth * dDreieckkprozent)), iY);
            DreiecksPunkte[0, 2] = new PointF((iX), (float)(iY + (iHeight * dDreieckkprozent)));

            DreiecksPunkte[1, 0] = new PointF(iX + iWidth, iY);
            DreiecksPunkte[1, 1] = new PointF((float)(iX + iWidth - (iWidth * dDreieckkprozent)), iY);
            DreiecksPunkte[1, 2] = new PointF((iX + iWidth), (float)(iY + (iHeight * dDreieckkprozent)));

            DreiecksPunkte[2, 0] = new PointF(iX, iY + iHeight);
            DreiecksPunkte[2, 1] = new PointF((float)(iX + (iWidth * dDreieckkprozent)), iY + iHeight);
            DreiecksPunkte[2, 2] = new PointF((iX), (float)(iY + iHeight - (iHeight * dDreieckkprozent)));

            DreiecksPunkte[3, 0] = new PointF(iX + iWidth, iY + iHeight);
            DreiecksPunkte[3, 1] = new PointF((float)(iX + iWidth - (iWidth * dDreieckkprozent)), iY + iHeight);
            DreiecksPunkte[3, 2] = new PointF((iX + iWidth), (float)(iY + iHeight - (iHeight * dDreieckkprozent)));
        }

        private void Botzug()
        {
            int zuegmoeglichkeiten = 0;

            Spielfeld[] allezuege = new Spielfeld[iSpielfeldWidth];
            for (int i = 0; i < iSpielfeldWidth; i++)
            {
                allezuege[i].spielstein = new string[iSpielfeldWidth, iSpielfeldHeight];
                for (int x = 0; x < iSpielfeldWidth; x++)
                {
                    for (int y = 0; y < iSpielfeldHeight; y++)
                    {
                        allezuege[i].spielstein[x, y] = Spielfelder[x, y].sFarbe;
                    }
                }
            }
            for (int i = 0; i < iSpielfeldWidth; i++)
            {
                allezuege[i].spielstein = moeglicherzug(allezuege[i].spielstein, i);
            }
        }

        private string[,] moeglicherzug(string[,] farben, int spalte)
        {
            int reihe = -1;
            bool gesetzt = false;
            for (int i = iSpielfeldHeight - 1; i >= 0 && !gesetzt; i--)                                                         //zählt von unten nach oben...
            {
                if (Spielfelder[spalte, i].sFarbe == "white")                                                        //...Wenn die Farbe weis ist...
                {
                    gesetzt = true;
                    farben[spalte, i] = sBotColor;
                    reihe = i;
                }
            }
            return farben;
        }

        //private int zugbewertung(string[,] farben)
        //{
           
        //}

        private void Form2_Click(object sender, EventArgs e)
        {
            if (this.PointToClient(Cursor.Position).X > Spielfelder[0, 0].iX && this.PointToClient(Cursor.Position).X < Spielfelder[iSpielfeldWidth - 1, iSpielfeldHeight - 1].iX + Spielfelder[iSpielfeldWidth - 1, iSpielfeldHeight - 1].iWidth && this.PointToClient(Cursor.Position).Y > Spielfelder[0, 0].iY && this.PointToClient(Cursor.Position).Y < Spielfelder[iSpielfeldWidth - 1, iSpielfeldHeight - 1].iY + Spielfelder[iSpielfeldWidth - 1, iSpielfeldHeight - 1].iHeight)    //abfrage ob klick auf Spielfeld ist
            {
                int spalte, reihe = -1;
                bool gesetzt = false;
                spalte = (this.PointToClient(Cursor.Position).X - Spielfelder[0, 0].iX) / Spielfelder[0, 0].iWidth;      //berechnung der Spalte
                for (int i = iSpielfeldHeight - 1; i >= 0; i--)                                                         //zählt von unten nach oben...
                {
                    if (Spielfelder[spalte, i].sFarbe == "white")                                                        //...Wenn die Farbe weis ist...
                    {
                        gesetzt = true;
                        Spielfelder[spalte, i].sFarbe = sCurrentcolor;
                        Hovereffekt(-1);                                                                                 //Hovereffekt wird während der Fallanimation entfernt
                        KreiszeichnenAnimation(spalte, i + 1, sCurrentcolor, Bitmapgraphic);                              //...wird die animation abgespielt un das Feld Farbig
                        reihe = i;
                        i = 0;
                    }
                }

                Point[] Gewinnerkoordinaten = new Point[iGewinnAnzahl];

                bool gewonnen = false;
                if (gesetzt && reihe != -1)                                                                             //wenn ein stein gesetzt wurde
                {
                    //überprüfung ob jemand gewonne hat
                    int infolge = 0;
                    for (int x = 0; x < iSpielfeldWidth && !gewonnen; x++)
                    {
                        if (Spielfelder[x, reihe].sFarbe == sCurrentcolor)
                        {
                            Gewinnerkoordinaten[infolge] = new Point(x, reihe);
                            infolge++;
                        }
                        else
                        {
                            infolge = 0;
                            for (int i = 0; i < iGewinnAnzahl; i++)
                            {
                                Gewinnerkoordinaten[i] = new Point(0, 0);
                            }
                        }
                        if (infolge == iGewinnAnzahl)
                        {
                            gewonnen = true;
                        }
                    }
                    infolge = 0;
                    for (int y = 0; y < iSpielfeldHeight && !gewonnen; y++)
                    {
                        if (Spielfelder[spalte, y].sFarbe == sCurrentcolor)
                        {
                            Gewinnerkoordinaten[infolge] = new Point(spalte, y);
                            Console.WriteLine("new Point[" + infolge + "]:" + Gewinnerkoordinaten[infolge]);

                            infolge++;
                        }
                        else
                        {
                            Console.WriteLine("alle punkte löschen");
                            infolge = 0;
                            for (int i = 0; i < iGewinnAnzahl; i++)
                            {
                                Gewinnerkoordinaten[i] = new Point(0, 0);
                            }
                        }
                        if (infolge == iGewinnAnzahl)
                        {
                            gewonnen = true;
                        }
                    }

                    int xabstand;
                    int maxformat;
                    int widthheightdif;
                    xabstand = spalte - reihe;
                    widthheightdif = iSpielfeldWidth - iSpielfeldHeight;
                    if (iSpielfeldHeight > iSpielfeldWidth)
                    {
                        maxformat = iSpielfeldHeight;
                    }
                    else
                    {
                        maxformat = iSpielfeldWidth;
                    }
                    infolge = 0;
                    for (int xy = 0; xy < maxformat && !gewonnen; xy++)
                    {
                        if (xy + xabstand < iSpielfeldWidth && xy + xabstand >= 0 && xy < iSpielfeldHeight && xy >= 0 && Spielfelder[xy + xabstand, xy].sFarbe == sCurrentcolor)
                        {
                            Gewinnerkoordinaten[infolge] = new Point(xy + xabstand, xy);
                            infolge++;
                        }
                        else
                        {
                            infolge = 0;
                        }
                        if (infolge == iGewinnAnzahl)
                        {
                            gewonnen = true;
                        }
                    }
                    xabstand = -1 + (spalte - reihe + (iSpielfeldHeight - (spalte * 2)));
                    infolge = 0;
                    for (int xy = 0; xy < maxformat + 1 && !gewonnen; xy++)
                    {
                        if (iSpielfeldWidth - xy - xabstand - widthheightdif < iSpielfeldWidth && iSpielfeldWidth - xy - xabstand - widthheightdif >= 0 && xy - 1 < iSpielfeldHeight && xy - 1 >= 0 && Spielfelder[iSpielfeldWidth - xy - xabstand - widthheightdif, xy - 1].sFarbe == sCurrentcolor)
                        {
                            Gewinnerkoordinaten[infolge] = new Point(iSpielfeldWidth - xy - xabstand - widthheightdif, xy - 1);
                            infolge++;
                        }
                        else
                        {
                            infolge = 0;
                        }
                        if (infolge == iGewinnAnzahl)
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
                        bAimationFlag = true;
                        for (int i = 0; i < this.Width; i += 10)
                        {
                            Bitmapgraphic.FillRectangle(new SolidBrush(this.BackColor), 0, 0, this.Width, this.Height);
                            SpielsteineZeichnen(Bitmapgraphic, i);
                            SpielfeldZeichnen(Bitmapgraphic);
                            Spielfeldgraphic.DrawImage(Spielfeldframe, 0, 0);
                        }
                        bAimationFlag = false;

                        if (sCurrentcolor == "red")
                        {
                            Gewonnen("Rot");
                        }
                        else
                        {
                            Gewonnen("Gelb");
                        }
                    }

                    if (sCurrentcolor == "red")
                    {
                        sCurrentcolor = "yellow";
                        lab_Player.Text = "Player Yellow";
                    }
                    else
                    {
                        sCurrentcolor = "red";
                        lab_Player.Text = "Player Red";
                    }

                    //überpfrüfung ob noch einzug möglich ist
                    bool zugmöglich = false;
                    for (int x = 0; x < iSpielfeldWidth; x++)
                    {
                        for (int y = 0; y < iSpielfeldHeight; y++)
                        {
                            if (Spielfelder[x, y].sFarbe == "white")             // wenn mindestens ein Feld weis ist, ist noch ein zug möglich
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

        private void Form2_MouseMove(object sender, MouseEventArgs e)
        {
            int spalte = -1;
            if (this.PointToClient(Cursor.Position).X > Spielfelder[0, 0].iX && this.PointToClient(Cursor.Position).X < Spielfelder[iSpielfeldWidth - 1, iSpielfeldHeight - 1].iX + Spielfelder[iSpielfeldWidth - 1, iSpielfeldHeight - 1].iWidth && this.PointToClient(Cursor.Position).Y > Spielfelder[0, 0].iY && this.PointToClient(Cursor.Position).Y < Spielfelder[iSpielfeldWidth - 1, iSpielfeldHeight - 1].iY + Spielfelder[iSpielfeldWidth - 1, iSpielfeldHeight - 1].iHeight)    //überprüfung ob die Maus auf dem Spielfeld hovert
            {
                if (iOldSpalte != (this.PointToClient(Cursor.Position).X - Spielfelder[0, 0].iX) / Spielfelder[0, 0].iWidth)      //überprüfung ob die maus über einem anderen fällt ist als bei der Letzten abfrage
                {
                    spalte = (this.PointToClient(Cursor.Position).X - Spielfelder[0, 0].iX) / Spielfelder[0, 0].iWidth;
                    Hovereffekt(spalte);
                }
            }
            else
            {
                Hovereffekt(-1);        //wenn die Maus nichtmehr auf dem Feld ist wird der Hovereffekt aufgehoben
            }
        }

        private void Form2_Paint(object sender, PaintEventArgs e)
        {
            if (!bAimationFlag && !bResizing)
            {
                Spielfeldgraphic.DrawImage(Spielfeldframe, 0, 0);
            }
        }

        private void Form2_Resize(object sender, EventArgs e)
        {
            if (Bitmapgraphic != null)
            {
                Spielfeldframe = new Bitmap(this.Width, this.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                Bitmapgraphic.FillRectangle(new SolidBrush(this.BackColor), 0, 0, this.Width, this.Height);  // wenn das Spielfeld neugezogen wird, wird es weis
            }
            bResizing = true;
        }

        private void Form2_ResizeEnd(object sender, EventArgs e)                                                // wenn das Spielfeld losgelassen wird, wird dass Spielfeld neu gezeichnet
        {
            if (bResizing)
            {
                bResizing = false;
                iSpielfeldHeightPx = this.Height - 200;
                iSpielfeldWidthPx = this.Width - 50;
                SpielfeldErstellen();
                EckenBerechnen(Spielfelder[0, 0].iWidth, Spielfelder[0, 0].iHeight);

                Spielfeldframe = new Bitmap(this.Width, this.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                Bitmapgraphic = Graphics.FromImage(Spielfeldframe);
                Spielfeldgraphic = this.CreateGraphics();

                Bitmapgraphic.FillRectangle(new SolidBrush(this.BackColor), 0, 0, this.Width, this.Height);
                SpielsteineZeichnen(Bitmapgraphic, 0);

                SpielfeldZeichnen(Bitmapgraphic);

                Spielfeldgraphic.DrawImage(Spielfeldframe, 0, 0);

                //Bitmapgraphic.FillRectangle(new SolidBrush(this.BackColor), 0, 0, this.Width, this.Height);
            }
        }

        private void Gewonnen(string sGewinner)
        {
            //this.Hide();
            var Result = MessageBox.Show($"{sGewinner} Hat gewonnen",
                $"{sGewinner} hat Gewonnen", MessageBoxButtons.RetryCancel, MessageBoxIcon.Information);

            if (Result == DialogResult.Retry)
            {
                Form2 frm = new();

                frm.Show();
                this.Hide();
            }
            if (Result == DialogResult.Cancel)
            {
                Form1 frm = new();

                frm.Show();
                this.Hide();
            }
        }

        /// <summary>
        /// Zeichnet einen Kreis über der Entsprechenden Zeile
        /// </summary>
        /// <param name="iSpalte">Die entsprechende spalte -1 heist, dass der Kreis auf keiner Zeile gedrawt werden soll</param>
        private void Hovereffekt(int iSpalte)
        {
            if (iSpalte >= 0)
            {
                if (iOldSpalte >= 0)
                {
                    // hoverkreis entfernen:
                    Bitmapgraphic.FillEllipse(new SolidBrush(Color.FromName("white")), Spielfelder[iOldSpalte, 0].iX + dKreisausgleich, Spielfelder[iOldSpalte, 0].iY - Spielfelder[0, 0].iHeight + dKreisausgleich - 2, Spielfelder[0, 0].iWidth - dKreisausgleich * 2, Spielfelder[0, 0].iHeight - dKreisausgleich * 2);
                }

                // hoverkreis zeichnen:
                Bitmapgraphic.FillEllipse(new SolidBrush(Color.FromName(sCurrentcolor)), Spielfelder[iSpalte, 0].iX + dKreisausgleich, Spielfelder[iSpalte, 0].iY - Spielfelder[0, 0].iHeight + dKreisausgleich - 2, Spielfelder[0, 0].iWidth - dKreisausgleich * 2, Spielfelder[0, 0].iHeight - dKreisausgleich * 2);
                iOldSpalte = iSpalte;
            }
            else        // wenn minus 1 übergeben wird
            {
                if (iOldSpalte >= 0)
                {
                    // hoverkreis entfernen:
                    Bitmapgraphic.FillEllipse(new SolidBrush(Color.FromName("white")), Spielfelder[iOldSpalte, 0].iX + dKreisausgleich, Spielfelder[iOldSpalte, 0].iY - Spielfelder[0, 0].iHeight + dKreisausgleich - 2, Spielfelder[0, 0].iWidth - dKreisausgleich * 2, Spielfelder[0, 0].iHeight - dKreisausgleich * 2);
                }
                iOldSpalte = iSpalte;
            }
            Spielfeldgraphic.DrawImage(Spielfeldframe, 0, 0);
        }

        /// <summary>
        /// lässt die Kreise mit den Entsprechenden Koordinaten eine bestimmte anzahl oft Drehen. Wird für die Gewinn Animation verwendet
        /// </summary>
        /// <param name="KreisKoordinaten">die Koordinaten der Kreise, die Gedreht werden sollen, auf dem Spielfeld</param>
        /// <param name="iDrehungen">Die Anzahl der Drehungen</param>
        private void KreisDrehen(Point[] KreisKoordinaten, int iDrehungen)
        {
            int Drehgeschwindigkeit = 5;
            for (int k = 0; k < iDrehungen; k++)                         //schleife pro Drehung
            {
                for (int j = Spielfelder[0, 0].iWidth; j >= 0; j -= Drehgeschwindigkeit)       //schleife für halbe umdrehung
                {
                    for (int i = 0; i < KreisKoordinaten.Length; i++)   //schleife für jeden Kreis
                    {
                        Bitmapgraphic.FillEllipse(new SolidBrush(this.BackColor),
                            Spielfelder[KreisKoordinaten[i].X, KreisKoordinaten[i].Y].iX + dKreisausgleich,
                            Spielfelder[KreisKoordinaten[i].X, KreisKoordinaten[i].Y].iY + dKreisausgleich,
                            Spielfelder[0, 0].iWidth - dKreisausgleich * 2,
                            Spielfelder[0, 0].iHeight - dKreisausgleich * 2);

                        Bitmapgraphic.FillEllipse(new SolidBrush(Color.FromName(Spielfelder[KreisKoordinaten[i].X, KreisKoordinaten[i].Y].sFarbe)),
                            Spielfelder[KreisKoordinaten[i].X, KreisKoordinaten[i].Y].iX + dKreisausgleich + ((Spielfelder[0, 0].iWidth - j) / 2),
                            Spielfelder[KreisKoordinaten[i].X, KreisKoordinaten[i].Y].iY + dKreisausgleich,
                            j,
                            Spielfelder[0, 0].iHeight - dKreisausgleich * 2);

                        SpielfeldTileZeichnen(
                            Spielfelder[KreisKoordinaten[i].X, KreisKoordinaten[i].Y].iX,
                            Spielfelder[KreisKoordinaten[i].X, KreisKoordinaten[i].Y].iY,
                            Spielfelder[0, 0].iWidth,
                            Spielfelder[0, 0].iHeight,
                            Bitmapgraphic);
                    }
                    Spielfeldgraphic.DrawImage(Spielfeldframe, 0, 0);
                }
                for (int j = 0; j <= Spielfelder[0, 0].iWidth; j += Drehgeschwindigkeit)      //schleife für andere hälfte
                {
                    for (int i = 0; i < KreisKoordinaten.Length; i++)   //schleife für jeden Kreis
                    {
                        Bitmapgraphic.FillEllipse(new SolidBrush(this.BackColor),
                            Spielfelder[KreisKoordinaten[i].X, KreisKoordinaten[i].Y].iX + dKreisausgleich,
                            Spielfelder[KreisKoordinaten[i].X, KreisKoordinaten[i].Y].iY + dKreisausgleich,
                            Spielfelder[0, 0].iWidth - dKreisausgleich * 2,
                            Spielfelder[0, 0].iHeight - dKreisausgleich * 2);

                        Bitmapgraphic.FillEllipse(new SolidBrush(Color.FromName(Spielfelder[KreisKoordinaten[i].X, KreisKoordinaten[i].Y].sFarbe)),
                            Spielfelder[KreisKoordinaten[i].X, KreisKoordinaten[i].Y].iX + dKreisausgleich + ((Spielfelder[0, 0].iWidth - j) / 2),
                            Spielfelder[KreisKoordinaten[i].X, KreisKoordinaten[i].Y].iY + dKreisausgleich,
                            j,
                            Spielfelder[0, 0].iHeight - dKreisausgleich * 2);

                        SpielfeldTileZeichnen(
                            Spielfelder[KreisKoordinaten[i].X, KreisKoordinaten[i].Y].iX,
                            Spielfelder[KreisKoordinaten[i].X, KreisKoordinaten[i].Y].iY,
                            Spielfelder[0, 0].iWidth,
                            Spielfelder[0, 0].iHeight,
                            Bitmapgraphic);
                    }
                    Spielfeldgraphic.DrawImage(Spielfeldframe, 0, 0);
                }
            }
        }

        private void Kreiszeichnen(int iX, int iY, string sFarbe, Graphics G, int iStartY)
        {
            G.FillEllipse(new SolidBrush(Color.FromName(sFarbe)),
             Spielfelder[0, 0].iX + iX * Spielfelder[0, 0].iWidth + dKreisausgleich,
             Spielfelder[0, 0].iY + iY * Spielfelder[0, 0].iHeight + dKreisausgleich + iStartY,
             Spielfelder[0, 0].iWidth - dKreisausgleich * 2, Spielfelder[0, 0].iHeight - dKreisausgleich * 2);
        }

        private void KreiszeichnenAnimation(int iX, int iY, string sFarbe, Graphics G)
        {
            int iHilfszahl = 0;
            int iHilfszahl1 = 0;
            int multiplyer = (int)fDroptime; // durch 2 teil bare Zahlen funktionieren am besten da Dann Weniger Komma stellen Entstehen die Ignoriert werden
            Task animation1 = new(() =>
            {
                bAimationFlag = true;

                for (int i = -multiplyer; i < iY * multiplyer; i++)
                {
                    if (i == -multiplyer || i + 1 == iY * multiplyer || Convert.ToInt32((i / multiplyer) + 1) >= iY)
                    {
                        G.FillEllipse(new SolidBrush(this.BackColor),
                        Spielfelder[0, 0].iX + iX * Spielfelder[0, 0].iWidth + dKreisausgleich,
                        Spielfelder[0, 0].iY + ((i - 1) / multiplyer) * Spielfelder[0, 0].iHeight + dKreisausgleich,
                        Spielfelder[0, 0].iWidth - dKreisausgleich * 2,
                        Spielfelder[0, 0].iHeight - dKreisausgleich * 2);

                        G.FillEllipse(new SolidBrush(Color.FromName(sFarbe)),
                         Spielfelder[0, 0].iX + iX * Spielfelder[0, 0].iWidth + dKreisausgleich,
                         Spielfelder[0, 0].iY + (i / multiplyer) * Spielfelder[0, 0].iHeight + dKreisausgleich,
                         Spielfelder[0, 0].iWidth - dKreisausgleich * 2,
                         Spielfelder[0, 0].iHeight - dKreisausgleich * 2);
                        if (i + 1 == iY * multiplyer && i / multiplyer > 0)
                        {
                            SpielfeldTileZeichnen(
                                Spielfelder[iX, iHilfszahl].iX,
                                Spielfelder[iX, i / multiplyer - 1].iY,
                                Spielfelder[iX, iHilfszahl].iWidth,
                                Spielfelder[iX, iHilfszahl].iHeight,
                                Bitmapgraphic);
                        }
                    }
                    else
                    {
                        Task draw = new(() =>
                        {
                            iHilfszahl = i / multiplyer;

                            if (i / multiplyer + 1 < iY)
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
                                 Spielfelder[0, 0].iX + iX * Spielfelder[0, 0].iWidth + dKreisausgleich,
                                 Spielfelder[0, 0].iY + (((i - 1) * Spielfelder[0, 0].iHeight) / multiplyer) + dKreisausgleich,
                                 Spielfelder[0, 0].iWidth - dKreisausgleich * 2, Spielfelder[0, 0].iHeight - dKreisausgleich * 2);
                            }
                            else
                            {
                                G.FillRectangle(new SolidBrush(this.BackColor),
                                 Spielfelder[0, 0].iX + iX * Spielfelder[0, 0].iWidth + dKreisausgleich,
                                 Spielfelder[0, 0].iY + ((i - 1) / multiplyer) * Spielfelder[0, 0].iHeight + dKreisausgleich,
                                 Spielfelder[0, 0].iWidth - dKreisausgleich * 2, Spielfelder[0, 0].iHeight - dKreisausgleich * 2);
                            }

                            G.FillEllipse(new SolidBrush(Color.FromName(sFarbe)),
                             Spielfelder[0, 0].iX + iX * Spielfelder[0, 0].iWidth + dKreisausgleich,
                             Spielfelder[0, 0].iY + ((i * Spielfelder[0, 0].iHeight) / multiplyer) + dKreisausgleich,
                             Spielfelder[0, 0].iWidth - dKreisausgleich * 2,
                             Spielfelder[0, 0].iHeight - dKreisausgleich * 2);

                            SpielfeldTileZeichnen(
                                Spielfelder[iX, iHilfszahl].iX,
                                Spielfelder[iX, iHilfszahl + 1].iY,
                                Spielfelder[iX, iHilfszahl].iWidth,
                                Spielfelder[iX, iHilfszahl].iHeight,
                                Bitmapgraphic);

                            SpielfeldTileZeichnen(
                              Spielfelder[iX, iHilfszahl].iX,
                              Spielfelder[iX, iHilfszahl].iY,
                              Spielfelder[iX, iHilfszahl].iWidth,
                              Spielfelder[iX, iHilfszahl].iHeight,
                              Bitmapgraphic);
                            if (iHilfszahl - 1 >= 0)
                            {
                                SpielfeldTileZeichnen(
                              Spielfelder[iX, iHilfszahl].iX,
                              Spielfelder[iX, iHilfszahl - 1].iY,
                              Spielfelder[iX, iHilfszahl].iWidth,
                              Spielfelder[iX, iHilfszahl].iHeight,
                              Bitmapgraphic);
                            }

                            Spielfeldgraphic.DrawImage(Spielfeldframe, 0, 0);
                        });
                        draw.RunSynchronously();
                    }

                    //Thread.Sleep((int)(100 / dropspeed));
                }
            }
            );
            animation1.RunSynchronously();

            SpielsteineZeichnen(Bitmapgraphic, 0);
            Spielfeldgraphic.DrawImage(Spielfeldframe, 0, 0);

            bAimationFlag = false;
        }

        /// <summary>
        /// erstellt das Spielfeld mit pixelkoordinaten anhand der Spielfeldgröße zeichent dieses jedoch noch nicht.
        /// </summary>
        private void SpielfeldErstellen()
        {
            int ispielfeldformat;
            if (iSpielfeldHeightPx / iSpielfeldHeight <= iSpielfeldWidthPx / iSpielfeldWidth)               //spielfeldformat wird so gewählt, dass das Spielfeld immer in das Fenster passt

            {
                ispielfeldformat = iSpielfeldHeightPx / iSpielfeldHeight;
            }
            else
            {
                ispielfeldformat = iSpielfeldWidthPx / iSpielfeldWidth;
            }
            for (int x = 0; x < iSpielfeldWidth; x++)
            {
                for (int y = 0; y < iSpielfeldHeight; y++)
                {
                    Spielfelder[x, y].iX = (this.Width / 2) - (ispielfeldformat * iSpielfeldWidth / 2) + x * ispielfeldformat;        //Struct wird angelegt
                    Spielfelder[x, y].iY = (this.Height / 2) - (ispielfeldformat * iSpielfeldHeight / 2) + y * ispielfeldformat;
                    Spielfelder[x, y].iWidth = ispielfeldformat;
                    Spielfelder[x, y].iHeight = ispielfeldformat;
                }
            }
        }

        /// <summary>
        /// Zeichnet eine Spielfeldkachel für die angegebenen Parameter
        /// </summary>
        /// <param name="iX">X koordiante am oberen Rand in Pixeln</param>
        /// <param name="iY">Y koordiante am linken Rand in Pixeln</param>
        /// <param name="iWidth">Die Breite der Kachel</param>
        /// <param name="iHeight">Die Breite der Kachel</param>
        /// <param name="G">Graphic object auf das Gezeichnet werden soll</param>
        private void SpielfeldTileZeichnen(int iX, int iY, int iWidth, int iHeight, Graphics G)   //Methode um ein einzelnes Spielfeld-feld zu zeichnen
        {
            PointF[] hilfsarray = new PointF[3];

            //int x = 50, y = 50, iwidth = 100, iheight = 100;
            Task Feld = new(() =>
            {
                for (int j = 0; j < 4; j++)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        hilfsarray[i] = new PointF(DreiecksPunkte[j, i].X + iX, DreiecksPunkte[j, i].Y + iY);
                    }
                    G.FillPolygon(new SolidBrush(Color.Blue), hilfsarray);
                }
                G.DrawEllipse(new Pen(Color.Blue, Spielfelder[0, 0].iWidth / 22 + 1.8f), iX, iY, iWidth, iHeight);
                G.DrawRectangle(new Pen(Color.Blue, Spielfelder[0, 0].iWidth / 22 + 1.8f), iX, iY, iWidth, iHeight);
                dKreisausgleich = Convert.ToSingle(Math.Pow(Spielfelder[0, 0].iWidth / 32f, 0.88f));
            });
            Feld.RunSynchronously();
        }

        /// <summary>
        /// Zeichnet das Spielfeld anhand des Structs Spielfelder
        /// </summary>
        /// <param name="G">Graphic object auf das Gezeichnet werden soll</param>
        private void SpielfeldZeichnen(Graphics G)
        {
            for (int x = 0; x < iSpielfeldWidth; x++)
            {
                for (int y = 0; y < iSpielfeldHeight; y++)
                {
                    SpielfeldTileZeichnen(Spielfelder[x, y].iX, Spielfelder[x, y].iY, Spielfelder[x, y].iWidth, Spielfelder[x, y].iHeight, G);       //Struct wird benutzt um Das Spielfeld zu Zeichnen
                }
            }

            //spielfeldgraphic.DrawImage(Spielfeldframe, 0, 0);
        }

        /// <summary>
        /// Zeichnet die Bereits gelegten Spielsteine anhand des Structs Spielfelder.
        /// </summary>
        /// <param name="G">Graphic object auf das Gezeichnet werden soll</param>
        /// <param name="iStartY"> die Obersten Y koordinaten. Normal 0 zum Steine gleichzeitig fallen lassen erhöhen.</param>
        private void SpielsteineZeichnen(Graphics G, int iStartY)
        {
            for (int x = 0; x < iSpielfeldWidth; x++)
            {
                for (int y = 0; y < iSpielfeldHeight; y++)
                {
                    if (Spielfelder[x, y].sFarbe != "white" && Spielfelder[x, y].sFarbe != null)                                                  //wenn die Farbe des Kreises Nicht weis ist wird er auch noch gezeichnet
                    {
                        Kreiszeichnen(x, y, Spielfelder[x, y].sFarbe, Bitmapgraphic, iStartY);
                    }
                }
            }

            //spielfeldgraphic.DrawImage(Spielfeldframe, 0, 0);
        }

        private void UhrStarten()
        {
            Task uhr = Task.Run(() =>
            {
                timer.Tick += new EventHandler(UhrUpdate);
                timer.Interval = 1000;
                timer.Start();
                timer.Enabled = true;
            });
        }

        private void UhrUpdate(Object Obj, EventArgs e)
        {
            Task uhr = Task.Run(() =>
            {
                VergangeneSekunden = VergangeneSekunden.AddSeconds(1);
                this.Invoke((MethodInvoker)delegate
                {
                    lab_Timer.Text = VergangeneSekunden.ToLongTimeString();
                });
            });
        }
    }
}

//http://www.bildungsgueter.de/CSharp/Pages/Beisp002Seite025.htm