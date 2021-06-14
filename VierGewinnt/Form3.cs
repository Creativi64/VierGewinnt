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

#pragma warning disable CS4014 // Da auf diesen Aufruf nicht gewartet wird, wird die Ausführung der aktuellen Methode vor Abschluss des Aufrufs fortgesetzt.
#pragma warning disable CS1998 // Bei der asynchronen Methode fehlen "await"-Operatoren. Die Methode wird synchron ausgeführt.
#pragma warning restore CS1998 // Bei der asynchronen Methode fehlen "await"-Operatoren. Die Methode wird synchron ausgeführt.
#pragma warning restore CS4014 // Da auf diesen Aufruf nicht gewartet wird, wird die Ausführung der aktuellen Methode vor Abschluss des Aufrufs fortgesetzt.

namespace VierGewinnt
{
    public partial class Form3 : Form
    {
        #region Console

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AllocConsole();

        #endregion Console

        private string sEmpfangenerZug;

        private string sMeinZug;

        private bool Fullscreen;
        private const int port = 42069;
        private IPEndPoint[] GefundenEndpoints;

        private int iLezztesGefundene = 0;

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

        public static int iSpielfeldheight = 4;
        public static int iSpielfeldwidth = 4;

        private Graphics spielfeldgraphic;
        private PointF[,] Dreieckspunkte;
        private Graphics punkte;

        private bool AimationFlag = false;

        public Spielfeldtile[,] spielfelder;

        #endregion GameParams

        public Form3(bool _Fullscreen)
        {
            InitializeComponent();

            GefundenEndpoints = new IPEndPoint[10];

            Fullscreen = _Fullscreen;
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

            DoubleBuffered = true;
            Fullscreen = _Fullscreen;
            AllocConsole();

            dropspeed = dropspeed * iSpielfeldheight;
            VergangeneSekunden = new DateTime(1, 1, 1, 0, 0, 0);

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

        protected override void OnClosed(EventArgs e)
        {
            //wenn man mit X das Programm Schließet Schliest es sich Komlett mit einer Meldung
            this.Hide();
            this.backgroundWorker1.CancelAsync();
            MessageBox.Show("Spiel Beendet",
                "Close Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            base.OnClosed(e);
            Application.Exit();
        }

        private void btn_Suchen_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Suchen");

            this.backgroundWorker1.RunWorkerAsync(1);
            progressBar1.Value = 0;
            btn_Suchen.Enabled = false;
            progressBar1.Visible = true;

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
            btn_Suchen.Visible = false;
            btn_cancel.Visible = false;
            LiB_GefundenenEndPoints.Visible = false;

            ServerHosten.Enabled = false;

            StartListening();
            //this.BcWork_Server.RunWorkerAsync();
            //while (this.BcWork_Server.IsBusy)
            //{
            //    // Keep UI messages moving, so the form remains
            //    // responsive during the asynchronous operation.
            //    Application.DoEvents();
            //}
        }

        private void btn_ConnectTo_Click(object sender, EventArgs e)
        {
            lab_Info.Text = ("");
            //StartClientVerbindung(IPAddress.Parse(txB_VerbindenIP.Text));

            StartClient();
        }

        private void btn_ZumMenue_Click(object sender, EventArgs e)
        {
            Form1 frm = new Form1();

            frm.Show();
            this.Hide();
        }

        private void btn_Bestätigen_Click(object sender, EventArgs e)
        {
            if (IPAddress.TryParse(txB_VerbindenIP.Text, out IPAddress _IP))
            {
                btn_Suchen.Visible = false;
                btn_cancel.Visible = false;
                LiB_GefundenenEndPoints.Visible = false;
                ServerHosten.Visible = false;
                txB_VerbindenIP.ReadOnly = true;
            }
            else
            {
                Console.WriteLine("Keine Gültige IP Adresse");
                lab_Info.Text = ("Keine Gültige IP");
            }
        }

        #region NachConnectionsSuchen

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            // Do not access the form's BackgroundWorker reference directly.
            // Instead, use the reference provided by the sender parameter.
            BackgroundWorker bw = sender as BackgroundWorker;

            // Extract the argument.
            //int arg = (int)e.Argument;

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
                //MessageBox.Show(msg);
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

            //string NetzBereich = "127.0.";
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

        #region Client_VerbindingMitServerHerstellen

        private static ManualResetEvent connectDone = new ManualResetEvent(false);

        private static ManualResetEvent sendDone = new ManualResetEvent(false);

        private static ManualResetEvent receiveDone = new ManualResetEvent(false);

        private static ManualResetEvent disconnectDone = new ManualResetEvent(false);

        public static bool bConectet = false;

        public class StateObject
        {
            // Client socket.
            public Socket workSocket = null;

            // Size of receive buffer.
            public const int BufferSize = 256;

            // Receive buffer.
            public byte[] buffer = new byte[BufferSize];

            // Received data string.
            public StringBuilder sb = new StringBuilder();
        }

        private static String response = String.Empty;

        private void StartClientVerbindung(IPAddress Ip)
        {
            Console.WriteLine("--------------------------");
            Console.WriteLine("Start");

            lab_Info.Text = ("Connect...");
            txB_Empfangen.Clear();

            sendDone.Reset();
            connectDone.Reset();
            receiveDone.Reset();
            disconnectDone.Reset();

            const int port = 42069;

            IPEndPoint RemoteEp = new IPEndPoint(Ip, port);

            // Create a TCP/IP socket.
            Socket s = new Socket(Ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Connect to the remote endpoint.
            s.BeginConnect(RemoteEp, new AsyncCallback(ConnectCallback), s);
            connectDone.WaitOne(100);

            if (s.Connected)
            {
                Console.WriteLine($"Kann Connecten {RemoteEp}");
                lab_Info.Text = ("Connected");

                // Send test data to the remote device.

                // Hier Werden Die Daten Eingegeben Die versedet werden
                lab_Info.Text = ("Warte auf Zug");
                //sZuSendenerZug = txB_Senden.Text;
                lab_Info.Text = ("Sending");
                SendClient(s, "ase");

                sendDone.WaitOne();

                // Receive the response from the remote device.
                lab_Info.Text = ("Receiving");
                Receive(s);
                receiveDone.WaitOne();

                Console.WriteLine("Response received : {0}", sEmpfangenerZug);
                txB_Empfangen.Text = sEmpfangenerZug;
                lab_Info.Text = ("Done");
            }
            else
            {
                Console.WriteLine($"Nix Gefunden Bei auf {RemoteEp}");
                lab_Info.Text = $"Konnte Zu {Ip} nicht Verbinden";
            }
        }

        private static void SendClient(Socket client, string data)
        {
            // Convert the string data to byte data using ASCII encoding.
            Console.WriteLine($"Sending '{data}'");
            byte[] byteData = Encoding.UTF8.GetBytes(data);

            // Begin sending the data to the remote device.
            client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), client);
        }

        private void Receive(Socket client)
        {
            try
            {
                // Create the state object.
                StateObject state = new StateObject();
                state.workSocket = client;

                // Begin receiving the data from the remote device.
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Console.WriteLine("Try Connecting");

                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.
                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}", client.RemoteEndPoint.ToString());

                // Signal that the connection has been made.
                connectDone.Set();
                Console.WriteLine("Connectet");
            }
            catch (Exception e)
            {
                Console.WriteLine("Conection Failed");
                Console.WriteLine(e.ToString());
            }
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket
                // from the asynchronous state object.
                StateObject state = (StateObject)ar.AsyncState;

                Socket client = state.workSocket;

                // Read data from the remote device.
                int bytesRead = client.EndReceive(ar);
                Console.WriteLine($"read {bytesRead} Bytes From Server");

                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    // Get the rest of the data.
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    // All the data has arrived; put it in response.
                    if (state.sb.Length > 1)
                    {
                        response = state.sb.ToString();
                        sEmpfangenerZug = state.sb.ToString();
                    }

                    // Signal that all bytes have been received.
                    receiveDone.Set();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine("ConnectionERROR");
            }
        }

        #endregion Client_VerbindingMitServerHerstellen

        #region ServerHosten

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            // Do not access the form's BackgroundWorker reference directly.
            // Instead, use the reference provided by the sender parameter.
            BackgroundWorker bw = sender as BackgroundWorker;

            // Extract the argument.
            //int arg = (int)e.Argument;

            // Start the time-consuming operation.
            StartListening(bw);

            // If the operation was canceled by the user,
            // set the DoWorkEventArgs.Cancel property to true.
            if (bw.CancellationPending)
            {
                e.Cancel = true;
            }
        }

        private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                // The user/programm canceled the operation.
                Console.WriteLine("Canceled");
                ServerHosten.Enabled = true;
            }
            else if (e.Error != null)
            {
                // There was an error during the operation.
                Console.WriteLine("An error occurred: {0}", e.Error.Message);
                ServerHosten.Enabled = true;
            }
            else
            {
                // The operation completed normally.
                Console.WriteLine("Server Beendet");
                ServerHosten.Enabled = true;
            }
        }

        public class StateObjectServer
        {
            // Size of receive buffer.
            public const int BufferSize = 1024;

            // Receive buffer.
            public byte[] buffer = new byte[BufferSize];

            // Received data string.
            public StringBuilder sb = new StringBuilder();

            // Client socket.
            public Socket workSocket = null;
        }

        // Thread signal.
        public static ManualResetEvent allDoneServer = new ManualResetEvent(false);

        public void StartListening(BackgroundWorker bw)
        {
            // Establish the local endpoint for the socket.
            // The DNS name of the computer
            const int port = 42069;

            if (!bw.CancellationPending)
            {
                IPAddress[] ipv4Addresses = Array.FindAll(Dns.GetHostEntry(string.Empty).AddressList, a => a.AddressFamily == AddressFamily.InterNetwork);

                IPEndPoint localEndPoint = new IPEndPoint(ipv4Addresses[0], port);
                Console.WriteLine($"Running On {localEndPoint}");

                // Create a TCP/IP socket.
                Socket listener = new Socket(ipv4Addresses[0].AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // Bind the socket to the local endpoint and listen for incoming connections.
                try
                {
                    listener.Bind(localEndPoint);
                    listener.Listen(1000);

                    while (true)
                    {
                        // Set the event to nonsignaled state.
                        allDoneServer.Reset();

                        // Start an asynchronous socket to listen for connections.
                        Console.WriteLine("Waiting for a connection...");
                        listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

                        // Wait until a connection is made before continuing.
                        allDoneServer.WaitOne();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }

        public void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            allDoneServer.Set();

            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            // Create the state object.
            StateObjectServer state = new StateObjectServer();
            state.workSocket = handler;

            handler.BeginReceive(state.buffer, 0, StateObjectServer.BufferSize, 0, new AsyncCallback(ReadCallback), state);
        }

        public void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObjectServer state = (StateObjectServer)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket.
            int bytesRead = handler.EndReceive(ar); // CHrasht wenn die verbindung plötzlich getrennt wird

            //store the data
            state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

            //Hier werden Die Daten Entgegen Genommen
            sEmpfangenerZug = state.sb.ToString();
            BcWork_Server.ReportProgress(1);
            // Display it on the console.
            Console.WriteLine("Read {0} bytes from socket. \n Data : {1}", content.Length, sEmpfangenerZug);

            // Echo the data back to the client.

            // sZuSendenerZug Await (ZusenderZug)

            // hier Spielzug Einfügen

            Send(handler, "awe");
        }

        private static void Send(Socket handler, String data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallbackServer), handler);
        }

        private static void SendCallbackServer(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void BcWork_Server_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.txB_Senden.Text = sEmpfangenerZug;
        }

        #endregion ServerHosten

        #region Einfacher Server

        public static string data = null;

        public static void StartListening()
        {
            // Data buffer for incoming data.
            byte[] bytes = new Byte[1024];

            // Establish the local endpoint for the socket.
            // Dns.GetHostName returns the name of the
            // host running the application

            IPAddress[] ipv4Addresses = Array.FindAll(Dns.GetHostEntry(string.Empty).AddressList, a => a.AddressFamily == AddressFamily.InterNetwork);

            IPEndPoint localEndPoint = new IPEndPoint(ipv4Addresses[0], port);

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
                while (true)
                {
                    Console.WriteLine("Waiting for a connection...");
                    // Program is suspended while waiting for an incoming connection.
                    Socket handler = listener.Accept();
                    data = null;

                    // An incoming connection needs to be processed.
                    while (true)
                    {
                        int bytesRec = handler.Receive(bytes);
                        data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                        //if (data.IndexOf("<EOF>") > -1)
                        //{
                        break;
                        //}
                    }

                    // Show the data on the console.
                    Console.WriteLine("Text received : {0}", data);

                    // Echo the data back to the client.
                    byte[] msg = Encoding.ASCII.GetBytes(data);

                    handler.Send(msg);
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        #endregion Einfacher Server

        #region EinfacherClient

        public static void StartClient()
        {
            // Data buffer for incoming data.
            byte[] bytes = new byte[1024];

            // Connect to a remote device.
            try
            {
                // Establish the remote endpoint for the socket.
                // This example uses port 11000 on the local computer.
                IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);

                // Create a TCP/IP  socket.
                Socket sender = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect the socket to the remote endpoint. Catch any errors.
                try
                {
                    sender.Connect(remoteEP);

                    Console.WriteLine("Socket connected to {0}", sender.RemoteEndPoint.ToString());

                    // Encode the data string into a byte array.
                    byte[] msg = Encoding.ASCII.GetBytes("This is a test<EOF>");

                    // Send the data through the socket.
                    int bytesSent = sender.Send(msg);

                    // Receive the response from the remote device.
                    int bytesRec = sender.Receive(bytes);
                    Console.WriteLine("Echoed test = {0}",
                        Encoding.ASCII.GetString(bytes, 0, bytesRec));

                    // Release the socket.
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
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
            if (!AimationFlag && !resizing)
            {
                Console.WriteLine("redraw");
                SpielfeldZeichnen();
            }
        }

        private void Form3_Click(object sender, EventArgs e)
        {
            if (this.PointToClient(Cursor.Position).X > spielfelder[0, 0].x && this.PointToClient(Cursor.Position).X < spielfelder[iSpielfeldwidth - 1, iSpielfeldheight - 1].x + spielfelder[iSpielfeldwidth - 1, iSpielfeldheight - 1].iwidth && this.PointToClient(Cursor.Position).Y > spielfelder[0, 0].y && this.PointToClient(Cursor.Position).Y < spielfelder[iSpielfeldwidth - 1, iSpielfeldheight - 1].y + spielfelder[iSpielfeldwidth - 1, iSpielfeldheight - 1].iheight)
            {
                int spalte = -1, reihe = -1;
                bool gesetzt = false;
                spalte = (this.PointToClient(Cursor.Position).X - spielfelder[0, 0].x) / spielfelder[0, 0].iwidth;
                sMeinZug = spalte.ToString();  // die Angecklikte spalte Wird Alszug Gesetzt Um sie Dann zu versenden
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
                        // Senden Einder END Flagg
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

        private void Form2_Resize(object sender, EventArgs e)
        {
            Console.WriteLine("resize");
            if (spielfeldgraphic != null)
            {
                spielfeldgraphic.FillRectangle(new SolidBrush(this.BackColor), 0, 0, this.Width, this.Height);
            }
            resizing = true;
        }

        private void Form2_ResizeEnd(object sender, EventArgs e)
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

//https://stackoverflow.com/questions/28601678/calling-async-method-on-button-click
//https://docs.microsoft.com/de-de/dotnet/desktop/winforms/controls/how-to-run-an-operation-in-the-background?view=netframeworkdesktop-4.8
//https://docs.microsoft.com/de-de/dotnet/api/system.threading.manualresetevent?view=net-5.0
//https://docs.microsoft.com/de-de/dotnet/framework/network-programming/asynchronous-client-socket-example