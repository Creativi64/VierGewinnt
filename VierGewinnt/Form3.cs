using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using static System.Console;
using System.Drawing;
using System.Threading.Tasks;

namespace VierGewinnt
{
    public partial class Form3 : Form
    {
        #region Console

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AllocConsole();

        #endregion Console

        private bool Fullscreen;

        private const int PORT = 42069;

        private IPEndPoint[] GefundenEndpoints;

        private int iLezztesGefundene = 0;

        private IPAddress AndererSpieler;

        private bool Block = true;

        private bool MeinZug = false;

        private bool SpielEnde = false;

        public static ManualResetEvent EmpfangenSignal = new ManualResetEvent(false);

        public static ManualResetEvent MeinZugSignal = new ManualResetEvent(false);

        #region GameParams

        private string currentcolor;

        private float kreisausgleich;

        private float dropspeed = 100; //geschwindigkeit beim runterfallen

        private int gewinnnummer;

        public struct Spielfeldtile
        {
            public int x, y, iwidth, iheight;

            public string farbe;
        }

        private DateTime VergangeneSekunden;

        private int iSpielfeldheightpx;

        private int iSpielfeldwidthpx;

        public static int iSpielfeldheight = 4;         //spielfeldhöhe in spielfeldern

        public static int iSpielfeldwidth = 4;          //spielfeldbreite in spielfeldern

        private Graphics spielfeldgraphic;

        private PointF[,] Dreieckspunkte;

        private Graphics punkte;

        private bool AimationFlag = true;

        public Spielfeldtile[,] spielfelder;

        #endregion GameParams

        public Form3(bool _Fullscreen)
        {
            InitializeComponent();
            AllocConsole();

            GefundenEndpoints = new IPEndPoint[10]; // maximal 10  Vill. Variable, Je anch Anzahl, Aber Es Sollten Momentan Nicht mehr als 10 Geben

            Fullscreen = _Fullscreen;
            if (_Fullscreen == true)
            {
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
                this.MaximizeBox = true;
            }
            else
            {
                this.WindowState = FormWindowState.Normal;
                this.MaximizeBox = true;
            }

            VergangeneSekunden = new DateTime(1, 1, 1, 0, 0, 0);

            DoubleBuffered = true;

            dropspeed = dropspeed * iSpielfeldheight;

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
                MeineIP();
            });
        }

        private void MeineIP()
        {
            IPAddress[] ipv4Addresses = Array.FindAll(Dns.GetHostEntry(string.Empty).AddressList, a => a.AddressFamily == AddressFamily.InterNetwork);
            lab_MeineIp.Text = $"MeineIP: {ipv4Addresses[0]}";
        }

        private void SpielerWählen()
        {
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
        }

        private void SpielFelInizialisieren()
        {
            Console.WriteLine("Spielfeld");
            Block = false;
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
            SpielfeldErstellen();
            SpielfeldZeichnen();

            // Im array alle Farben Auf Weiß zu setzen
            for (int x = 0; x < iSpielfeldwidth; x++)
            {
                for (int y = 0; y < iSpielfeldheight; y++)
                {
                    spielfelder[x, y].farbe = "white";
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            //wenn man mit X das Programm Schließet Schliest es sich Komlett mit einer Meldung
            this.Hide();
            this.backgroundWorker1.CancelAsync();
            MessageBox.Show("Spiel Beendet",
                "Close Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            base.OnClosed(e);

            //backgroundworkerBeeneden

            Application.Exit();
        }

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

        private void ServerHosten_Click(object sender, EventArgs e)
        {
            // HOST IST IMMER RED

            btn_Suchen.Visible = false;
            btn_cancel.Visible = false;
            LiB_GefundenenEndPoints.Visible = false;
            lab_IPeingabeHier.Visible = false;
            txB_VerbindenIP.Visible = false;
            btn_ConnectTo.Visible = false;
            SpielHosten.Enabled = false;

            // Empfängt die Ip Des Zu verbindenden
            string EmpfangeneIp = StartListening("Empfangen");
            AndererSpieler = IPAddress.Parse(EmpfangeneIp);
            lab_VerbundenMit.Text = $"Verbunden Mit: {AndererSpieler}";

            // Feld Inizialisieren und Spieler Wählen
            SpielFelInizialisieren();
            this.Refresh();
            Thread.Sleep(1000);
            SpielerWählen();

            // Feld, gewinnzahl und Farb Daten Schicken
            EmpfangenSignal.Reset();
            StartClient(iSpielfeldheight.ToString());
            EmpfangenSignal.WaitOne();

            EmpfangenSignal.Reset();
            StartClient(iSpielfeldwidth.ToString());
            EmpfangenSignal.WaitOne();

            EmpfangenSignal.Reset();
            StartClient(currentcolor.ToString());
            EmpfangenSignal.WaitOne();

            EmpfangenSignal.Reset();
            StartClient(gewinnnummer.ToString());
            EmpfangenSignal.WaitOne();

            Console.WriteLine("Spieldaten ausgetauischt");

            if (currentcolor == "red")
            {
                MeinZug = true;

                Task ts = new Task(() =>
                {
                    do
                    {
                        while (!MeinZugSignal.WaitOne())
                        {
                            Application.DoEvents();
                        }

                        string sGegnerZug = StartListening("");
                        GegnerZug(Convert.ToInt32(sGegnerZug));
                        MeinZugSignal.Reset();
                        MeinZug = true;
                    } while (SpielEnde == false);
                });
                ts.Start();
            }
            else
            {
                string sGegnerZugErster = StartListening("Empfngen");
                GegnerZug(Convert.ToInt32(sGegnerZugErster));
                Task ts = new Task(() =>
                {
                    do
                    {
                        MeinZug = true;
                        Console.WriteLine("warte auf Zug");
                        while (!MeinZugSignal.WaitOne())
                        {
                            Application.DoEvents();
                            Console.WriteLine("warten");
                        }
                        string sGegnerZug = StartListening("");
                        GegnerZug(Convert.ToInt32(sGegnerZug));
                        MeinZugSignal.Reset();
                    } while (SpielEnde == false);
                });
                ts.Start();
            }

            Console.WriteLine("Ende");
        }

        

        private void btn_ConnectTo_Click(object sender, EventArgs e)
        {
            // CLIENT IST IMMER YELLOW
            if (IPAddress.TryParse(txB_VerbindenIP.Text, out IPAddress _IP))
            {
                btn_Suchen.Visible = false;
                btn_cancel.Visible = false;
                LiB_GefundenenEndPoints.Visible = false;
                SpielHosten.Visible = false;
                txB_VerbindenIP.ReadOnly = true;
                btn_ConnectTo.Visible = false;

                AndererSpieler = IPAddress.Parse(txB_VerbindenIP.Text);
                lab_VerbundenMit.Text = $"Verbunden mit {AndererSpieler}";

                // sendet Seine Ip An Den server
                IPAddress[] ipv4Addresses = Array.FindAll(Dns.GetHostEntry(string.Empty).AddressList, a => a.AddressFamily == AddressFamily.InterNetwork);
                StartClient(ipv4Addresses[0].ToString());

                //empängt Spielfeld Höhe, breite, gewinnummer und Die farbe
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

                iSpielfeldheight = Convert.ToInt32(sSpielfeldHöhe);
                gewinnnummer = Convert.ToInt32(sGewinnummer);
                iSpielfeldwidth = Convert.ToInt32(sSpielfeldBreite);
                currentcolor = sFarbe;

                Console.WriteLine("Spieldaten ausgetauischt");

                SpielFelInizialisieren();

                if (currentcolor == "red")
                {
                    //warte auf spielzug
                    string sGegnerZugStart = StartListening("");
                    GegnerZug(Convert.ToInt32(sGegnerZugStart));
                    Task ts = new Task(() =>
                    {
                        do
                        {
                            MeinZug = true;
                            Console.WriteLine("warte auf Zug");
                            while (!MeinZugSignal.WaitOne())
                            {
                                Application.DoEvents();
                                Console.WriteLine("warten");
                            }
                            string sGegnerZug = StartListening("");
                            GegnerZug(Convert.ToInt32(sGegnerZug));
                            MeinZugSignal.Reset();
                        } while (SpielEnde == false);
                    });
                    ts.Start();
                }
                else
                {
                    //mache Spiezug
                    MeinZug = true;
                    Task ts = new Task(() =>
                    {
                        Console.WriteLine("warte auf Zug");
                        do
                        {
                            while (!MeinZugSignal.WaitOne())
                            {
                                Application.DoEvents();
                                Console.WriteLine("warten");
                            }
                            string sGegnerZug = StartListening("");
                            GegnerZug(Convert.ToInt32(sGegnerZug));
                            MeinZugSignal.Reset();
                            MeinZug = true;
                        } while (SpielEnde == false);
                    });
                    ts.Start();
                }

                Console.WriteLine("Ende");
            }
            else
            {
                Console.WriteLine("Keine Gültige IP Adresse");
                lab_Info.Text = "Keine Gültige IP";
            }
        }

        private void btn_ZumMenue_Click(object sender, EventArgs e)
        {
            Form1 frm = new Form1();

            frm.Show();
            this.Hide();
        }

        #region NachConnectionsSuchen

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
                        Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        IPAddress Ip = IPAddress.Parse($"{NetzBereich1}{i}.{a}");
                        IPEndPoint hostep = new IPEndPoint(Ip, 42069);

                        IAsyncResult result = s.BeginConnect(Ip, 42069, null, null);

                        bool success = result.AsyncWaitHandle.WaitOne(1, true);

                        if (s.Connected)
                        {
                            s.EndConnect(result);
                            s.Close();
                            Console.WriteLine($"gefunden auf {hostep}");

                            GefundenEndpoints[iLezztesGefundene] = hostep;
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

        public string StartListening(string Senden)
        {
            this.Invoke((MethodInvoker)delegate
           {
               lab_NotResponding.Visible = true;
               this.Refresh();
           });

            string sEmpfangen = null;

            // Data buffer for incoming data.
            byte[] bytes = new Byte[1024];

            // Establish the local endpoint for the socket.

            IPAddress[] ipv4Addresses = Array.FindAll(Dns.GetHostEntry(string.Empty).AddressList, a => a.AddressFamily == AddressFamily.InterNetwork);

            IPEndPoint localEndPoint = new IPEndPoint(ipv4Addresses[0], PORT);

            // Create a TCP/IP socket.
            Socket listener = new Socket(ipv4Addresses[0].AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and
            // listen for incoming connections.
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);

                // Start listening for connections.
                Console.WriteLine($"Running On {localEndPoint}");

                Console.WriteLine("Waiting for a connection...");

                // Program is suspended while waiting for an incoming connection.
                Socket handler = listener.Accept();

                // An incoming connection needs to be processed.
                while (true)
                {
                    int bytesRec = handler.Receive(bytes);
                    sEmpfangen += Encoding.ASCII.GetString(bytes, 0, bytesRec);

                    EmpfangenSignal.Set();
                    break;
                }

                // Show the data on the console.
                Console.WriteLine("Text received : {0}", sEmpfangen);

                // Echo the data back to the client.
                byte[] msg = Encoding.ASCII.GetBytes(Senden);

                handler.Send(msg);
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            listener.Close();
            this.Invoke((MethodInvoker)delegate
            {
                this.lab_NotResponding.Visible = false;
                this.Refresh();
            });
            return sEmpfangen;
        }

        #endregion Einfacher Server

        #region EinfacherClient

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
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, PORT);

                // Create a TCP/IP  socket.
                Socket sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // Connect the socket to the remote endpoint. Catch any errors.
                try
                {
                    sender.Connect(remoteEP);

                    Console.WriteLine("Socket connected to {0}", sender.RemoteEndPoint.ToString());

                    // Encode the data string into a byte array.
                    byte[] msg = Encoding.ASCII.GetBytes(Senden);

                    // Send the data through the socket.
                    int bytesSent = sender.Send(msg);

                    // Receive the response from the remote device.
                    int bytesRec = sender.Receive(bytes);
                    sEmpfangen = Encoding.ASCII.GetString(bytes, 0, bytesRec);
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

        #region Game

        private void SpielfeldErstellen()
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
                }
            }
        }

        private void SpielfeldZeichnen()
        {
            for (int x = 0; x < iSpielfeldwidth; x++)
            {
                for (int y = 0; y < iSpielfeldheight; y++)
                {
                    spielfeldtilezeichnen(spielfelder[x, y].x, spielfelder[x, y].y, spielfelder[x, y].iwidth, spielfelder[x, y].iheight);
                    if (spielfelder[x, y].farbe != "white" && spielfelder[x, y].farbe != null)
                    {
                        Kreiszeichnen(x, y, spielfelder[x, y].farbe);
                    }
                }
            }
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
                spielfeldgraphic.DrawEllipse(new Pen(Color.Blue, spielfelder[0, 0].iwidth / 22 + 1.8f), x, y, iwidth, iheight);
                spielfeldgraphic.DrawRectangle(new Pen(Color.Blue, spielfelder[0, 0].iwidth / 22 + 1.8f), x, y, iwidth, iheight);
                kreisausgleich = Convert.ToSingle(Math.Pow(spielfelder[0, 0].iwidth / 32f, 0.88f));
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

                spielfeldgraphic.DrawEllipse(new Pen(Color.Blue, spielfelder[0, 0].iwidth / 20 + 1.5f), x, y, iwidth, iheight);
                spielfeldgraphic.DrawRectangle(new Pen(Color.Blue, spielfelder[0, 0].iwidth / 20 + 1.5f), x, y, iwidth, iheight);
                spielfeldgraphic.DrawEllipse(new Pen(Color.Blue, spielfelder[0, 0].iwidth / 20 + 1.5f), x, y + iheight, iwidth, iheight);
                spielfeldgraphic.DrawRectangle(new Pen(Color.Blue, spielfelder[0, 0].iwidth / 20 + 1.5f), x, y + iheight, iwidth, iheight);
                kreisausgleich = Convert.ToSingle(Math.Pow(spielfelder[0, 0].iwidth / 32f, 0.88f));
            });
            Feld.RunSynchronously();
        }

        private void Form3_Paint(object sender, PaintEventArgs e)
        {
            if ((!AimationFlag && !resizing) || Block != true)
            {
                Console.WriteLine("redraw");
                SpielfeldZeichnen();
            }
        }

        private void GegnerZug(int Spalte)
        {
            int spalte = -1, reihe = -1;
            bool gesetzt = false;
            spalte = Spalte;

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

                    SpielEnde = true;
                }

                if (currentcolor == "red")
                {
                    currentcolor = "yellow";
                    this.Invoke((MethodInvoker)delegate
                    {
                        lab_Player.Text = "Player Yellow";
                    });
                }
                else
                {
                    currentcolor = "red";
                    this.Invoke((MethodInvoker)delegate
                    {
                        lab_Player.Text = "Player Red";
                    });
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

        private void Form3_Click(object sender, EventArgs e)
        {
            if (!Block && MeinZug == true)
            {
                if (this.PointToClient(Cursor.Position).X > spielfelder[0, 0].x && this.PointToClient(Cursor.Position).X < spielfelder[iSpielfeldwidth - 1, iSpielfeldheight - 1].x + spielfelder[iSpielfeldwidth - 1, iSpielfeldheight - 1].iwidth && this.PointToClient(Cursor.Position).Y > spielfelder[0, 0].y && this.PointToClient(Cursor.Position).Y < spielfelder[iSpielfeldwidth - 1, iSpielfeldheight - 1].y + spielfelder[iSpielfeldwidth - 1, iSpielfeldheight - 1].iheight)
                {
                    int spalte = -1, reihe = -1;
                    bool gesetzt = false;
                    spalte = (this.PointToClient(Cursor.Position).X - spielfelder[0, 0].x) / spielfelder[0, 0].iwidth;

                    //sMeinZug = spalte.ToString();  // die Angecklikte spalte Wird Alszug Gesetzt Um sie Dann zu versenden

                    StartClient(spalte.ToString());
                    MeinZugSignal.Set();
                    MeinZug = false;

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
                            SpielEnde = true;
                        }

                        if (currentcolor == "red")
                        {
                            currentcolor = "yellow";
                            this.Invoke((MethodInvoker)delegate
                            {
                                lab_Player.Text = "Player Yellow";
                            });
                        }
                        else
                        {
                            currentcolor = "red";
                            this.Invoke((MethodInvoker)delegate
                            {
                                lab_Player.Text = "Player Red";
                            });
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
                            if (i <= 0)
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
            var Result = MessageBox.Show($"{Gewinner} Hat gewonnen",
                $"{Gewinner} Hat Gewonnen", MessageBoxButtons.RetryCancel, MessageBoxIcon.Information);

            if (Result == DialogResult.Retry)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    Form3 frm = new Form3(Fullscreen);
                    frm.Show();

                    this.Hide();
                });
            }
            if (Result == DialogResult.Cancel)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    Form1 frm = new Form1();
                    frm.Show();
                    this.Hide();
                });
            }
        }

        private void Form3_MouseMove(object sender, MouseEventArgs e)
        {
            if (!Block && MeinZug == true)
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
        }

        private int oldspalte = 0;

        private void Hovereffekt(int spalte)
        {
            if (spalte >= 0)
            {
                if (oldspalte >= 0)
                {
                    punkte.FillEllipse(new SolidBrush(Color.FromName("white")), spielfelder[oldspalte, 0].x + kreisausgleich, spielfelder[oldspalte, 0].y - spielfelder[0, 0].iheight + kreisausgleich - 2, spielfelder[0, 0].iwidth - kreisausgleich * 2, spielfelder[0, 0].iheight - kreisausgleich * 2);
                }
                punkte.FillEllipse(new SolidBrush(Color.FromName(currentcolor)), spielfelder[spalte, 0].x + kreisausgleich, spielfelder[spalte, 0].y - spielfelder[0, 0].iheight + kreisausgleich - 2, spielfelder[0, 0].iwidth - kreisausgleich * 2, spielfelder[0, 0].iheight - kreisausgleich * 2);
                oldspalte = spalte;
            }
            else
            {
                if (oldspalte >= 0)
                {
                    punkte.FillEllipse(new SolidBrush(Color.FromName("white")), spielfelder[oldspalte, 0].x + kreisausgleich, spielfelder[oldspalte, 0].y - spielfelder[0, 0].iheight + kreisausgleich - 2, spielfelder[0, 0].iwidth - kreisausgleich * 2, spielfelder[0, 0].iheight - kreisausgleich * 2);
                }
                oldspalte = spalte;
            }
        }

        private bool resizing = false;

        private void Form3_Resize(object sender, EventArgs e)
        {
            if (Block == false)
            {
                Console.WriteLine("resize");
                if (spielfeldgraphic != null)
                {
                    spielfeldgraphic.FillRectangle(new SolidBrush(this.BackColor), 0, 0, this.Width, this.Height);
                }
                resizing = true;
            }
        }

        private void Form3_ResizeEnd(object sender, EventArgs e)
        {
            if (Block == false)
            {
                if (resizing)
                {
                    Console.WriteLine("resizeend");

                    resizing = false;
                    iSpielfeldheightpx = this.Height - 150;
                    iSpielfeldwidthpx = this.Width - 50;
                    SpielfeldErstellen();
                    EckenBerechnen(0, 0, spielfelder[0, 0].iwidth, spielfelder[0, 0].iheight);

                    spielfeldgraphic = this.CreateGraphics();
                    punkte = this.CreateGraphics();
                    Console.WriteLine("redraw");
                    spielfeldgraphic.FillRectangle(new SolidBrush(this.BackColor), 0, 0, this.Width, this.Height);
                    SpielfeldZeichnen();
                }
            }
        }

        #endregion Game
    }
}

//https://stackoverflow.com/questions/28601678/calling-async-method-on-button-click
//https://docs.microsoft.com/de-de/dotnet/desktop/winforms/controls/how-to-run-an-operation-in-the-background?view=netframeworkdesktop-4.8
//https://docs.microsoft.com/de-de/dotnet/api/system.threading.manualresetevent?view=net-5.0
//https://docs.microsoft.com/de-de/dotnet/framework/network-programming/asynchronous-client-socket-example