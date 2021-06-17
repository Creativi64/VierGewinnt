using System;
using System.ComponentModel;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VierGewinnt
{
    /// <summary>
    ///
    /// Kann Nach Offen Servern Im Netzwerk Suchen (192.168)
    ///
    /// Man kann ein spiel auf machen auf das jemand conecten kann
    ///
    /// Mit einem offenen spiel connecten
    ///
    /// Der Code Fürs Game Ist der Gleiche Wie Im Lokalen (Form2)
    /// Unterschiedlich ist nur das Alle Label Änderungen Mit invoke von einem anderen thread aufgerufen werden da diese meist nicht im main thread Aufgreiden werden
    /// Block und MeinZug Blokieren am Anfang die spielfeld erstellung und dann immer wenn man nicht dran ist
    /// Von der Click Event Funktion Gibt es eine Kopie Die Nur Dien Empfangenen zug übergeben bekommt und die gleiche logig mit diesem aus führt wie als häte man es selbst gemacht
    /// Nach Dem man selbst Gecklickt hat wird der zug beendet und weggeschickt
    ///
    /// </summary>
    public partial class Form3 : Form
    {
        #region Console

        /// <summary>
        /// Erlaubt Uns Zum Form eine console zu starten
        /// </summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AllocConsole();

        #endregion Console

        private const int PORT = 42069;

        private IPEndPoint[] GefundenEndpoints;
        private int iLetzterGefundeneEndpoint = 0;

        private IPAddress AndererSpieler;

        private bool bBlock = true;
        private bool bMeinZug = false;
        private bool bSpielEnde = false;
        private bool bInizialisiertFlag = false;

        /// <summary>
        /// Signaliseirt Den Task/Threads ob was Empfangen wurde damit sie nicht mehr wartenüssen
        /// </summary>
        private static ManualResetEvent EmpfangenSignal = new(false);

        /// <summary>
        /// Mein Zug Signalisert wann Der Eigene Zug Zuendet ist undnicht mehr darauf gewartet werden muss Das man sein zug macht
        /// </summary>
        private static ManualResetEvent MeinZugSignal = new(false);

        #region GameParams

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

        public struct SpielfeldTile                     //representiert die einzelnen Felder mit position und Farbe
        {
            public string sFarbe;
            public int iX, iY, iWidth, iHeight;
        }

        #endregion GameParams

        /// <summary>
        ///  Beim Inizialisieren Von Form 3
        ///
        ///     Array Von Endpoinst Difiniert Die Bei Der Suche Nach Diesen Gefüllt wird Momentan wird davon ausgegangen das im gleichen Netztwerk nicht mehr als 10 sind
        ///
        ///     Window Pratmeter wie
        ///         Window state
        ///         maximise box -> ob man das Fenster vergrößern kann
        ///
        ///     die Größe Der Pixel Von den Federn X und Y
        ///
        ///     Die Bitmap Frame Wird Definiert
        ///
        ///     Die Bitmap Wird mit der Hintergrund farbe gefüllt
        ///
        ///     Das Switch Case Legt Die Gewinnummer Fest Auf Der Bsis Was man In Form1 Eingegeben hat
        ///     Der Default ist 4
        ///     Eher Ein Easter Egg da es nicht gesagt wird das Man Die Gewinnummer Änder kann
        ///
        ///     Am Ende Wird nach der Inizialisertung Noch die Ip Des Pc abgefragt und angezeigt
        ///     Es Wird immer Die erste IP genommen auch wenn er mehr findet nimmt er immer die erste
        ///
        /// </summary>
        public Form3()
        {
            InitializeComponent();
            //AllocConsole();

            GefundenEndpoints = new IPEndPoint[10]; // maximal 10  Vill. Variable, Je anch Anzahl, Aber Es Sollten Momentan Nicht mehr als 10 Geben

            this.WindowState = FormWindowState.Normal;
            this.MaximizeBox = true;

            iSpielfeldHeightPx = this.Height - 150;
            iSpielfeldWidthPx = this.Width - 50;

            Spielfeldgraphic = this.CreateGraphics();
            SpielfeldErstellen();
            Spielfeldframe = new Bitmap(this.Width, this.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            Bitmapgraphic = Graphics.FromImage(Spielfeldframe);

            Bitmapgraphic.FillRectangle(new SolidBrush(this.BackColor), 0, 0, this.Width, this.Height);

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
                MeineIP();
            });
        }

        /// <summary>
        /// Zeig Die Erste ip von einem selbst an
        /// </summary>
        private void MeineIP()
        {
            IPAddress[] ipv4Addresses = Array.FindAll(Dns.GetHostEntry(string.Empty).AddressList, a => a.AddressFamily == AddressFamily.InterNetwork);
            lab_MeineIp.Text = $"MeineIP: {ipv4Addresses[0]}";
        }

        /// <summary>
        /// zufällige farbewählen die dann als erste eine spielzug macht
        /// </summary>
        private void SpielerWählen()
        {
            if ((new Random()).Next(0, 2) == 0)
            {
                sCurrentcolor = "red";
                lab_Player.Text = "Player Red";
            }
            else
            {
                sCurrentcolor = "yellow";
                lab_Player.Text = "Player Yellow";
            }
        }

        /// <summary>
        /// Wird aufgerufen vom server bevor der die daten Sendet und vom Client nachtdem er alle Daten bekommen hat
        /// Es Fängt dann an das spielfeld zu zeichn
        /// </summary>
        private void SpielFelInizialisieren()
        {
            bInizialisiertFlag = true;
            Console.WriteLine("Spielfeld");
            fDroptime = fDroptime / iSpielfeldHeight;
            bBlock = false;
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

            EckenBerechnen(0, 0, Spielfelder[0, 0].iWidth, Spielfelder[0, 0].iHeight);
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

            Spielfeldgraphic.DrawImage(Spielfeldframe, 0, 0);
        }

        /// <summary>
        /// Wenn Man Das Fenster Schliest Sorgt dies Dafür das Alles geschlossen wird auch wenn noch die suche läuft oder ein Task/Thread hintergrund wartet und diese auch beendet werden
        /// Es wird eine message box Angezeigt die sagt das Das Programm beendet wurde
        ///
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            //wenn man mit X das Programm Schließet Schliest es sich Komlett mit einer Meldung
            this.Hide();
            this.backgroundWorker1.CancelAsync();
            MessageBox.Show("Spiel Beendet",
                "Close Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            base.OnClosed(e);

            //backgroundworkerBeeneden
            EmpfangenSignal.Set();

            MeinZugSignal.Set();
            Application.Exit();
            Environment.Exit(0);
        }

        /// <summary>
        /// Startet den Background worker der dann nach Offenen Ports Im Lokalen Netzwerk sucht
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">EventArgs</param>
        private void btn_Suchen_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Suchen");

            this.backgroundWorker1.RunWorkerAsync(1);
            progressBar1.Value = 0;
            btn_Suchen.Enabled = false;
            progressBar1.Visible = true;
            btn_cancel.Visible = true;
            LiB_GefundenenEndPoints.Visible = true;

            //GefundenEndpoints = await ConnectionSuchen();

            while (this.backgroundWorker1.IsBusy)
            {
                // Keep UI messages moving, so the form remains
                // responsive during the asynchronous operation.
                Application.DoEvents();
            }
        }

        /// <summary>
        /// Hostet ein spiel, Bezieht sich aber nur darauf wer die parameter am spiel anfang festlegt
        /// Startet bei Einem Den Listening prozess der Darauf wartet das sich jemand verbindet
        /// nach der Verbindung werden die spiel daten ausgetauscht
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">EventArgs</param>
        private void ServerHosten_Click(object sender, EventArgs e)
        {
            SpielHosten.Visible = false;
            lab_Info.Text = "Spiel wird gehostet";
            /// HOST IST IMMER RED

            btn_Suchen.Visible = false;
            btn_cancel.Visible = false;
            LiB_GefundenenEndPoints.Visible = false;
            lab_IPeingabeHier.Visible = false;
            txB_VerbindenIP.Visible = false;
            btn_ConnectTo.Visible = false;
            SpielHosten.Enabled = false;

            string EmpfangeneIp = null;

            /// Empfängt die Ip Des Zu verbindenden
            EmpfangenSignal.Reset();
            do
            {
                Task iPAustasuchen = Task.Run(() =>
                {
                    EmpfangeneIp = StartListening("Empfangen");
                    while (!EmpfangenSignal.WaitOne())
                    {
                        Application.DoEvents();
                    }
                    EmpfangenSignal.Reset();
                });
                while (iPAustasuchen.IsCompleted == false)
                {
                    Application.DoEvents();
                }
                iPAustasuchen.Wait();
            } while (EmpfangeneIp == null);

            AndererSpieler = IPAddress.Parse(EmpfangeneIp);
            lab_VerbundenMit.Text = $"Verbunden Mit: {AndererSpieler}";

            /// Feld Inizialisieren und Spieler Wählen
            SpielFelInizialisieren();

            Thread.Sleep(1000);
            SpielerWählen();

            /// Feld Höhe, Breite , Farbe und Gewinnnummer werden Übertragen
            EmpfangenSignal.Reset();
            StartClient(iSpielfeldHeight.ToString());
            EmpfangenSignal.WaitOne();

            EmpfangenSignal.Reset();
            StartClient(iSpielfeldWidth.ToString());
            EmpfangenSignal.WaitOne();

            EmpfangenSignal.Reset();
            StartClient(sCurrentcolor.ToString());
            EmpfangenSignal.WaitOne();

            EmpfangenSignal.Reset();
            StartClient(iGewinnAnzahl.ToString());
            EmpfangenSignal.WaitOne();

            Console.WriteLine("Spieldaten ausgetauischt");

            /// je nachdem welche Farbe Anfängt wird Ein Andere Spiel Verlauf Genutzt wenn die Farbe Rot ist wird 1. Genutzt
            /// wenn die farbe nicht so ist dann 2.
            /// 
            ///  L listen, S = Senden
            ///
            /// 1.
            /// Es wird zuerst Selbst Gesendet und dann Empfangen und wieder Gesendet
            /// 
            ///    S L
            ///
            ///    L S
            ///
            ///    S L
            ///
            /// ...
            /// 
            /// 2.
            /// es wird Zuerst Empfangen und dann gesenter und dann Empfangen
            ///
            ///    L S
            ///
            ///    S L
            ///
            ///    L S
            /// 
            /// ...
            /// 
            /// Dies Muss immer mit der Gegen seite Syncron laufen

            if (sCurrentcolor == "red")
            {
                // eigenen Spiezugmachen
                Task warten2 = Task.Run(() =>
                {
                    bMeinZug = true;
                    while (!MeinZugSignal.WaitOne())
                    {
                        Application.DoEvents();
                    }
                });

                while (warten2.IsCompleted == false)
                {
                    Application.DoEvents();
                }
                MeinZugSignal.Reset();

                do
                {
                    // auf zug warten
                    Task warten = Task.Run(() =>
                    {
                        string sGegnerZug = StartListening("");
                        GegnerZug(Convert.ToInt32(sGegnerZug));
                    });
                    while (warten.IsCompleted == false)
                    {
                        Application.DoEvents();
                    }

                    // eigenen Spiezugmachen
                    Task warten1 = Task.Run(() =>
                    {
                        bMeinZug = true;
                        while (!MeinZugSignal.WaitOne())
                        {
                            Application.DoEvents();
                        }
                    });
                    while (warten1.IsCompleted == false)
                    {
                        Application.DoEvents();
                    }
                    MeinZugSignal.Reset();
                } while (bSpielEnde == false);
            }
            else
            {
                // auf Zug warten
                Task warten1 = Task.Run(() =>
                {
                    string sGegnerZugErster = StartListening("Empfngen");
                    GegnerZug(Convert.ToInt32(sGegnerZugErster));
                });
                while (warten1.IsCompleted == false)
                {
                    Application.DoEvents();
                }

                do
                {
                    // eigenen Spiezugmachen
                    Task warten2 = Task.Run(() =>
                    {
                        bMeinZug = true;
                        while (!MeinZugSignal.WaitOne())
                        {
                            Application.DoEvents();
                        }
                    });
                    while (warten2.IsCompleted == false)
                    {
                        Application.DoEvents();
                    }
                    MeinZugSignal.Reset();

                    //Auf Zug warten
                    Task warten = Task.Run(() =>
                    {
                        string sGegnerZug = StartListening("");
                        GegnerZug(Convert.ToInt32(sGegnerZug));
                    });
                    while (warten.IsCompleted == false)
                    {
                        Application.DoEvents();
                    }
                } while (bSpielEnde == false);
            }

            Console.WriteLine("Ende");
        }

        /// <summary>
        /// Versicht sich zu der in txB_VerbindenIP Eingegeben ip zu verbinden
        /// wennn er dies nich kann bricht er ab und meldet über das label das es nicht geklappt hat
        /// Wenn er eine verbindung hat beginnt der daten austauscht und es wird angefangen zu lauschen um die parameter fürs spiel zu empfangen
        /// </summary>
        /// <param name="sender">object sender</param>
        /// <param name="e"> EventArgs</param>
        private void btn_ConnectTo_Click(object sender, EventArgs e)
        {
            /// CLIENT IST IMMER YELLOW

            /// die Ip adresse prüfen
            if (IPAddress.TryParse(txB_VerbindenIP.Text, out IPAddress _IP))
            {
                /// prüfen ob man sich auf die adresse verbidnen kann
                Socket s = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress Ip = IPAddress.Parse(txB_VerbindenIP.Text);
                IPEndPoint hostep = new(Ip, 42069);

                IAsyncResult result = s.BeginConnect(Ip, 42069, null, null);

                bool success = result.AsyncWaitHandle.WaitOne(1, true);
                if (s.Connected)
                {
                    /// wemm man sich mit dem Endpoint verbinden kann wird die verbindung wieder getrennt und danacht die Richtige verbindung erstellt
                    s.EndConnect(result);
                    s.Send(Encoding.UTF8.GetBytes("Ping"));
                    s.Close();

                    btn_Suchen.Visible = false;
                    btn_cancel.Visible = false;
                    LiB_GefundenenEndPoints.Visible = false;
                    SpielHosten.Visible = false;
                    txB_VerbindenIP.ReadOnly = true;
                    btn_ConnectTo.Visible = false;
                    txB_VerbindenIP.Visible = false;
                    lab_IPeingabeHier.Visible = false;

                    AndererSpieler = IPAddress.Parse(txB_VerbindenIP.Text);
                    lab_VerbundenMit.Text = $"Verbunden mit {AndererSpieler}";

                    /// sendet Seine Ip An Den server
                    IPAddress[] ipv4Addresses = Array.FindAll(Dns.GetHostEntry(string.Empty).AddressList, a => a.AddressFamily == AddressFamily.InterNetwork);
                    StartClient(ipv4Addresses[0].ToString());

                    ///empängt Spielfeld Höhe, breite, gewinnummer und Die farbe
                    EmpfangenSignal.Reset();
                    string sSpielfeldHöhe = StartListening("Empfangen");
                    EmpfangenSignal.WaitOne();

                    EmpfangenSignal.Reset();
                    string sSpielfeldBreite = StartListening("Empfangen");
                    EmpfangenSignal.WaitOne();

                    EmpfangenSignal.Reset();
                    string sFarbe = StartListening("Empfangen");
                    EmpfangenSignal.WaitOne();

                    EmpfangenSignal.Reset();
                    string sGewinnummer = StartListening("Empfangen");
                    EmpfangenSignal.WaitOne();

                    // parameter wqerden Gesetzt
                    iSpielfeldHeight = Convert.ToInt32(sSpielfeldHöhe);
                    iGewinnAnzahl = Convert.ToInt32(sGewinnummer);
                    iSpielfeldWidth = Convert.ToInt32(sSpielfeldBreite);
                    sCurrentcolor = sFarbe;

                    Console.WriteLine("Spieldaten ausgetauischt");

                    SpielFelInizialisieren();
                    //this.Refresh();
                    //spielfeldgraphic.DrawImage(Spielfeldframe, 0, 0);
                    Thread.Sleep(1000);
                    
                    /// je nachdem welche Farbe Anfängt wird Ein Andere Spiel Verlauf Genutzt wenn die Farbe Rot ist wird 1. Genutzt
                    /// wenn die farbe nicht so ist dann 2.
                    ///
                    /// L = listen, S = Senden
                    /// 
                    /// 1.
                    /// es wird Zuerst Empfangen und dann gesenter und dann Empfangen
                    /// 
                    ///    L S
                    ///
                    ///    S L
                    ///
                    ///    L S
                    /// 
                    /// ...
                    /// 
                    ///2.
                    /// Es wird zuerst Selbst Gesendet und dann Empangen und wieder Gesendet
                    ///
                    ///    S L
                    ///
                    ///    L S
                    ///
                    ///    S L
                    ///
                    /// ...
                    /// 
                    /// Dies Muss immer mit der Gegen seite Syncron laufen
                   
                    if (sCurrentcolor == "red")
                    {
                        //auf zug warten
                        Task warten = Task.Run(() =>
                        {
                            string sGegnerZugStart = StartListening("");
                            GegnerZug(Convert.ToInt32(sGegnerZugStart));
                        });
                        while (warten.IsCompleted == false)
                        {
                            Application.DoEvents();
                        }

                        do
                        {
                            // eigenen Spiezugmachen
                            Task warten2 = Task.Run(() =>
                            {
                                bMeinZug = true;
                                while (!MeinZugSignal.WaitOne())
                                {
                                    ;
                                    Application.DoEvents();
                                }
                            });
                            while (warten2.IsCompleted == false)
                            {
                                Application.DoEvents();
                            }
                            MeinZugSignal.Reset();
                            //auf zug warten
                            Task warten1 = Task.Run(() =>
                            {
                                string sGegnerZug = StartListening("");
                                GegnerZug(Convert.ToInt32(sGegnerZug));
                            });
                            while (warten1.IsCompleted == false)
                            {
                                Application.DoEvents();
                            }
                        } while (bSpielEnde == false);
                    }
                    else
                    {
                        //eigenen Spiezugmachen

                        Task warten1 = Task.Run(() =>
                        {
                            bMeinZug = true;
                            while (!MeinZugSignal.WaitOne())
                            {
                                Application.DoEvents();
                            }
                        });
                        while (warten1.IsCompleted == false)
                        {
                            Application.DoEvents();
                        }
                        MeinZugSignal.Reset();
                        do
                        {
                            // auf zug warten

                            Task warten = Task.Run(() =>
                            {
                                string sGegnerZug = StartListening("");
                                GegnerZug(Convert.ToInt32(sGegnerZug));
                            });
                            while (warten.IsCompleted == false)
                            {
                                Application.DoEvents();
                            }

                            // eigenen Spiezugmachen
                            Task warten2 = Task.Run(() =>
                            {
                                bMeinZug = true;
                                while (!MeinZugSignal.WaitOne())
                                {
                                    Application.DoEvents();
                                }
                            });
                            while (warten2.IsCompleted == false)
                            {
                                Application.DoEvents();
                            }
                            MeinZugSignal.Reset();
                        } while (bSpielEnde == false);
                    }

                    Console.WriteLine("Ende");
                }
                else
                {
                    s.Close();
                    Console.WriteLine("Keine Gültige Verbindug");
                    lab_Info.Text = "Keine Gültige Verbindung";
                }
            }
            else
            {
                Console.WriteLine("Keine Gültige IP Adresse");
                lab_Info.Text = "Keine Gültige IP";
            }
        }

        private void btn_ZumMenue_Click(object sender, EventArgs e)
        {
            Form1 frm = new();

            frm.Show();
            this.Hide();
        }

        #region NachConnectionsSuchen

        /// <summary>
        /// Startet den such prozess als BackgroundWorker im Hintergrund
        /// </summary>
        /// <param name="sender"> sender Objekt Vom aufrufer</param>
        /// <param name="e"> DoWorkEventArgs um die Events zu verarbeiten wird nur für Cancel Genutzt</param>
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            // Do not access the form's BackgroundWorker reference directly.
            // Instead, use the reference provided by the sender parameter.
            BackgroundWorker bw = sender as BackgroundWorker;

            // Extract the argument.

            // Start the time-consuming operation.
            ConnectionSuchen(bw);

            // If the operation was canceled by the user,
            // set the DoWorkEventArgs.Cancel property to true.
            if (bw.CancellationPending)
            {
                e.Cancel = true;
            }
        }

        /// <summary>
        /// Wird Vom BackgroundWorker nach beendigung aufgerufen
        /// sotiert nach Canceled
        /// Error wenn ein Fehler Passiert ist
        /// Normaler Durchlauf Hier nach werden die Ergebnise in der LiB_GefundenenEndPoints Aufgelistet
        /// </summary>
        /// <param name="sender">sender Objekt Vom aufrufer</param>
        /// <param name="e"> die RunWorkerCompletedEventArgs die dann den beendigungs grund mitliefern </param>
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                // The user canceled the operation.

                Console.WriteLine("Canceled");
                progressBar1.Value = 0;
                progressBar1.Visible = false;
                btn_Suchen.Enabled = true;
            }
            else if (e.Error != null)
            {
                // There was an error during the operation.
                Console.WriteLine("An error occurred: {0}", e.Error.Message);
                progressBar1.Value = 0;
                progressBar1.Visible = false;

                btn_Suchen.Enabled = true;
            }
            else
            {
                Console.WriteLine("Suche Abgeschlossen");

                // The operation completed normally.
                progressBar1.Value = 0;
                progressBar1.Visible = false;
                btn_Suchen.Enabled = true;
                Console.WriteLine("ergebnis");

                LiB_GefundenenEndPoints.Items.Clear();
                LiB_GefundenenEndPoints.Items.Add("Gefundene Spiele");
                LiB_GefundenenEndPoints.Items.Add("--------------------------------------");

                for (int i = 0; i < GefundenEndpoints.Length; i++)
                {
                    if (GefundenEndpoints[i] != null)
                    {
                        LiB_GefundenenEndPoints.Items.Add(GefundenEndpoints[i]);
                        LiB_GefundenenEndPoints.Items.Add("###");
                    }
                }
            }
        }

        /// <summary>
        /// Wird vom BackgroundWorker aufgerufen um nach den Endpoints zu sichen
        /// </summary>
        /// <param name="bw"> der BackgroundWorker wird übergeben um zu wissen welcher status er hat und Den prozess vortschritt zurück zu geben</param>
        private void ConnectionSuchen(BackgroundWorker bw)
        {
            for (int i = 0; i < GefundenEndpoints.Length; i++)
            {
                GefundenEndpoints[i] = null;
            }
            int iHöchsteProzent = 0;

            int NetzverkBereich1 = 255, NetzverkBereich2 = 255;

            double iProgress;

            string NetzBereich1 = "192.168.";

            for (int i = 0; i <= NetzverkBereich1; i++)
            {
                for (int a = 0; a <= NetzverkBereich2; a++)
                {
                    if (!bw.CancellationPending)
                    {
                        Socket s = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        IPAddress Ip = IPAddress.Parse($"{NetzBereich1}{i}.{a}");
                        IPEndPoint hostep = new(Ip, 42069);

                        IAsyncResult result = s.BeginConnect(Ip, 42069, null, null);

                        bool success = result.AsyncWaitHandle.WaitOne(1, true);

                        if (s.Connected)
                        {
                            s.EndConnect(result);
                            s.Send(Encoding.UTF8.GetBytes("Ping"));
                            s.Close();
                            Console.WriteLine($"gefunden auf {hostep}");

                            GefundenEndpoints[iLetzterGefundeneEndpoint] = hostep;
                        }
                        else
                        {
                            // NOTE, MUST CLOSE THE SOCKET

                            s.Close();
                            Console.WriteLine($"Nix Gefunden Bei auf {hostep}");
                        }

                        iProgress = (i * a);
                        iProgress /= (NetzverkBereich1 * NetzverkBereich2);
                        iProgress *= 100;
                        Console.WriteLine($"Progress {iProgress} - {Convert.ToInt32(iProgress)} %");

                        if (Convert.ToInt32(iProgress) > iHöchsteProzent)
                        {
                            iHöchsteProzent = Convert.ToInt32(iProgress);
                            bw.ReportProgress(iHöchsteProzent);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                if (bw.CancellationPending)
                {
                    break;
                }
            }
        }

        private void btn_cancel_Click(object sender, EventArgs e)
        {
            this.backgroundWorker1.CancelAsync();
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.progressBar1.Increment(1);
            Application.DoEvents();
        }

        #endregion NachConnectionsSuchen

        #region Einfacher Server

        /// <summary>
        /// Startet Ein Listening Prozzess um daten entegen zu nehmen
        /// Encodeing und Decoding Leuft gleich Über UTF8 es muss der zu sendende oder empfangende vorher nicht geändert werden
        /// </summary>
        /// <param name="Senden">Es wird Ein String Übergeben der nach den empfangen von etwas zurück gesendet wird</param>
        /// <returns>Gibt das empfangenen als string zurück</returns>
        public string StartListening(string Senden)
        {
            this.Invoke((MethodInvoker)delegate
           {
           });

            string sEmpfangen = null;

            // Data buffer for incoming data.
            byte[] bytes = new Byte[1024];

            // Establish the local endpoint for the socket.

            IPAddress[] ipv4Addresses = Array.FindAll(Dns.GetHostEntry(string.Empty).AddressList, a => a.AddressFamily == AddressFamily.InterNetwork);

            IPEndPoint localEndPoint = new(ipv4Addresses[0], PORT);

            // Create a TCP/IP socket.
            Socket listener = new(ipv4Addresses[0].AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and
            // listen for incoming connections.
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);

                // Start listening for connections.
                Console.WriteLine($"Running On {localEndPoint}");

                Console.WriteLine("Waiting for a connection...");
                do
                {
                    // Program is suspended while waiting for an incoming connection.
                    Socket handler = listener.Accept();

                    // An incoming connection needs to be processed.
                    while (true)
                    {
                        int bytesRec = handler.Receive(bytes);
                        sEmpfangen = Encoding.UTF8.GetString(bytes, 0, bytesRec);

                        EmpfangenSignal.Set();

                        break;
                    }

                    // Show the data on the console.
                    Console.WriteLine("Text received : {0}", sEmpfangen);
                    if (sEmpfangen != "Ping")
                    {
                        //data back to the client.
                        byte[] msg = Encoding.UTF8.GetBytes(Senden);
                        handler.Send(msg);
                    }

                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                } while (sEmpfangen == "Ping");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            listener.Close();
            this.Invoke((MethodInvoker)delegate
            {
            });
            return sEmpfangen;
        }

        #endregion Einfacher Server

        #region EinfacherClient

        /// <summary>
        /// Startet Einen Client der daten an einen Server sendet der am lauschen ist
        /// Encodeing und Decoding Leuft Über UTF8  es muss der zu sendende oder empfangende vorher nicht geändert werden
        /// </summary>
        /// <param name="Senden">der zu sendende string</param>
        /// <returns>Gibt zürück was es von server als antwort auf das erhaltenn von den daten bekommt</returns>
        public string StartClient(string Senden)
        {
            string sEmpfangen = null;

            // Data buffer for incoming data.
            byte[] bytes = new byte[1024];

            // Connect to a remote device.
            try
            {
                // Establish the remote endpoint for the socket.

                IPAddress ipAddress = AndererSpieler;
                IPEndPoint remoteEP = new(ipAddress, PORT);

                // Create a TCP/IP  socket.
                Socket sender = new(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // Connect the socket to the remote endpoint. Catch any errors.
                try
                {
                    sender.Connect(remoteEP);

                    Console.WriteLine("Socket connected to {0}", sender.RemoteEndPoint.ToString());

                    // Encode the data string into a byte array.
                    byte[] msg = Encoding.UTF8.GetBytes(Senden);

                    // Send the data through the socket.
                    int bytesSent = sender.Send(msg);

                    // Receive the response from the remote device.
                    int bytesRec = sender.Receive(bytes);
                    sEmpfangen = Encoding.UTF8.GetString(bytes, 0, bytesRec);
                    Console.WriteLine("Echoed test = {0}", sEmpfangen);

                    // Release the socket.
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
                    EmpfangenSignal.Set();
                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return sEmpfangen;
        }

        #endregion EinfacherClient

        /// <summary>
        /// Mehr Informationeen Zu dem Game In Form2
        /// </summary>

        #region Game

        private void SpielfeldErstellen()
        {
            if (bInizialisiertFlag)
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
        }

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

        private void EckenBerechnen(int iX, int iY, int iWidth, int iHeight)
        {
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

        private void Form3_Paint(object sender, PaintEventArgs e)
        {
            if (!bAimationFlag && !bResizing)
            {
                Spielfeldgraphic.DrawImage(Spielfeldframe, 0, 0);
            }
        }

        /// <summary>
        /// macht in eigene Spielfend den zug der Empfangen wurde
        /// </summary>
        /// <param name="Spalte">Die Spalte in der Der Zug gemacht werden soll</param>
        private void GegnerZug(int Spalte)
        {
            int spalte, reihe = -1;
            bool gesetzt = false;
            spalte = Spalte;   //berechnung der Spalte
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
                        bSpielEnde = true;
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
                        bSpielEnde = true;
                    }
                }

                if (gewonnen)
                {
                    bSpielEnde = true;
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
                }
                else
                {
                    sCurrentcolor = "red";
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
                    bSpielEnde = true;
                }
            }
        }

        /// <summary>
        /// nach dem man gecklickt hat wird der zug gesendet und er dann Von GegnerZug Auch bei gegner gemacht
        /// Die anzeige und Animation passiert Nahezu syncron bei beiden spielern
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form3_Click(object sender, EventArgs e)
        {
            if (!bBlock && bMeinZug == true)
            {
                if (this.PointToClient(Cursor.Position).X > Spielfelder[0, 0].iX && this.PointToClient(Cursor.Position).X < Spielfelder[iSpielfeldWidth - 1, iSpielfeldHeight - 1].iX + Spielfelder[iSpielfeldWidth - 1, iSpielfeldHeight - 1].iWidth && this.PointToClient(Cursor.Position).Y > Spielfelder[0, 0].iY && this.PointToClient(Cursor.Position).Y < Spielfelder[iSpielfeldWidth - 1, iSpielfeldHeight - 1].iY + Spielfelder[iSpielfeldWidth - 1, iSpielfeldHeight - 1].iHeight)    //abfrage ob klick auf Spielfeld ist
                {
                    int spalte, reihe = -1;
                    bool gesetzt = false;
                    spalte = (this.PointToClient(Cursor.Position).X - Spielfelder[0, 0].iX) / Spielfelder[0, 0].iWidth;      //berechnung der Spalte

                    // zug wird Gesendet
                    StartClient(spalte.ToString());
                    MeinZugSignal.Set();
                    bMeinZug = false;

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
                            bSpielEnde = true;
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
                            bSpielEnde = true;
                        }
                    }
                }
            }
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

        private void Kreiszeichnen(int iX, int iY, string sFarbe, Graphics G, int iStartY)
        {
            G.FillEllipse(new SolidBrush(Color.FromName(sFarbe)),
             Spielfelder[0, 0].iX + iX * Spielfelder[0, 0].iWidth + dKreisausgleich,
             Spielfelder[0, 0].iY + iY * Spielfelder[0, 0].iHeight + dKreisausgleich + iStartY,
             Spielfelder[0, 0].iWidth - dKreisausgleich * 2, Spielfelder[0, 0].iHeight - dKreisausgleich * 2);
        }

        private void Gewonnen(string Gewinner)
        {
            var Result = MessageBox.Show($"{Gewinner} Hat gewonnen",
                $"{Gewinner} Hat Gewonnen", MessageBoxButtons.RetryCancel, MessageBoxIcon.Information);

            if (Result == DialogResult.Retry)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    Form3 frm = new();
                    frm.Show();

                    this.Hide();
                });
            }
            if (Result == DialogResult.Cancel)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    Form1 frm = new();
                    frm.Show();
                    this.Hide();
                });
            }
        }

        private void Form3_MouseMove(object sender, MouseEventArgs e)
        {
            if (!bBlock && bMeinZug == true)
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
        }

        private void Hovereffekt(int iSpalte)
        {
            if (bMeinZug == true)
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
        }

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

        private void Form3_Resize(object sender, EventArgs e)
        {
            if (Bitmapgraphic != null && !bBlock)
            {
                Spielfeldframe = new Bitmap(this.Width, this.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                Bitmapgraphic.FillRectangle(new SolidBrush(this.BackColor), 0, 0, this.Width, this.Height);  // wenn das Spielfeld neugezogen wird, wird es weis
            }
            bResizing = true;
        }

        private void Form3_ResizeEnd(object sender, EventArgs e)// wenn das Spielfeld losgelassen wird, wird dass Spielfeld neu gezeichnet
        {
            if (bResizing && !bBlock)
            {
                bResizing = false;
                iSpielfeldHeightPx = this.Height - 200;
                iSpielfeldWidthPx = this.Width - 50;
                SpielfeldErstellen();
                EckenBerechnen(0, 0, Spielfelder[0, 0].iWidth, Spielfelder[0, 0].iHeight);

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

        #endregion Game

        private void progressBar1_Click(object sender, EventArgs e)
        {
        }

        private void lab_Info_Click(object sender, EventArgs e)
        {
        }
    }
}

//https://stackoverflow.com/questions/28601678/calling-async-method-on-button-click
//https://docs.microsoft.com/de-de/dotnet/desktop/winforms/controls/how-to-run-an-operation-in-the-background?view=netframeworkdesktop-4.8
//https://docs.microsoft.com/de-de/dotnet/api/system.threading.manualresetevent?view=net-5.0
//https://docs.microsoft.com/de-de/dotnet/framework/network-programming/asynchronous-client-socket-example