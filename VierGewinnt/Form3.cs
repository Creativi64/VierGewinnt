using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using static System.Console;

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

        private IPEndPoint[] GefundenEndpoints;

        private int iLezztesGefundene = 0;

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

        private void btn_Test_Click(object sender, EventArgs e)
        {
            // Daten, die gesendet werden
            string textToSend = txB_1.Text;

            // Endpunkt, zu dem verbunden wird
            const string localhost = "127.0.0.1";
            const int port = 42069;

            var data = Encoding.UTF8.GetBytes(textToSend);
            var ip = IPAddress.Parse(localhost);
            var ipEndPoint = new IPEndPoint(ip, port);

            try
            {
                // Socket, das verwendet wird
                using (var socket = new Socket(AddressFamily.InterNetwork,
                                           SocketType.Stream,
                                           ProtocolType.Tcp))
                {
                    // Es wird zum Endpunkt verbunden
                    socket.Connect(ipEndPoint);

                    // Daten werden gesendet
                    var byteCount = socket.Send(data, SocketFlags.None);
                    WriteLine("Es wurden {0} bytes gesendet", byteCount);

                    // Puffer für die zu empfangenen Daten
                    var buffer = new byte[256];

                    // Daten werden empfangen
                    byteCount = socket.Receive(buffer, SocketFlags.None);

                    // Wenn eine Antwort erhalten wurde, diese ausgeben
                    if (byteCount > 0)
                    {
                        WriteLine("Es wurden {0} Bytes empfangen", byteCount);
                        var answer = Encoding.UTF8.GetString(buffer);
                        WriteLine("Empfangene Daten: {0}", answer);
                    }
                }

                //Verbindung wird geschlossen
            }
            catch (SocketException)
            {
                Console.WriteLine("Error");
            }
        }
        
        private void btn_Suchen_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Suchen");

            this.backgroundWorker1.RunWorkerAsync(1);
            progressBar1.Value = 0;
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
            this.BcWork_Server.RunWorkerAsync();
            while (this.BcWork_Server.IsBusy)
            {
                // Keep UI messages moving, so the form remains
                // responsive during the asynchronous operation.
                Application.DoEvents();
            }
        }

        private void btn_ConnectTo_Click(object sender, EventArgs e)
        {
            if (IPAddress.TryParse(txB_VerbindenIP.Text, out IPAddress _IP))
            {
                StartClientVerbindung(_IP);
            }
            else
            {
                Console.WriteLine("Keine Gültige IP Adresse");
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
                //MessageBox.Show("Operation was canceled");
            }
            else if (e.Error != null)
            {
                // There was an error during the operation.
                //string msg = String.Format("An error occurred: {0}", e.Error.Message);
                Console.WriteLine("An error occurred: {0}", e.Error.Message);
                progressBar1.Value = 0;
                progressBar1.Visible = false;
                //MessageBox.Show(msg);
            }
            else
            {
                Console.WriteLine("Suche Abgeschlossen");

                // The operation completed normally.
                //GefundenEndpoints[iLezztesGefundene] = e.Result;
                //iLezztesGefundene++;
                progressBar1.Value = 0;
                progressBar1.Visible = false;
                //string msg = String.Format("Result = {0}", e.Result);
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

                //MessageBox.Show(msg);
            }
        }

        private void ConnectionSuchen(BackgroundWorker bw)
        {
            //IPEndPoint[] GefundeneEndPoints = new IPEndPoint[10];
            //int iZaeler = 0;
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

                            //throw new ApplicationException("Failed to connect server.");
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

        public static ManualResetEvent allDone = new ManualResetEvent(false);

        private static ManualResetEvent connectDone = new ManualResetEvent(false);

        private static ManualResetEvent sendDone = new ManualResetEvent(false);

        private static ManualResetEvent receiveDone = new ManualResetEvent(false);

        public static bool bConectet = false;

        private static void StartClientVerbindung(IPAddress Ip)
        {
            Console.WriteLine("Start");

            //const string localhost2 = "127.0.0.1";
            //var ip2 = IPAddress.Parse(Ip);

            const int port = 42069;

            string strrinf = "TestTestTes23";

            var data = Encoding.UTF8.GetBytes(strrinf);

            IPEndPoint RemoteEp = new IPEndPoint(Ip, port);

            // Create a TCP/IP socket.
            Socket s = new Socket(Ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Connect to the remote endpoint.
            s.BeginConnect(RemoteEp, new AsyncCallback(ConnectCallback), s);

            connectDone.WaitOne();

            // Send test data to the remote device.
            SendServer(s,"TEST<EOF>");

            sendDone.WaitOne();

            // Receive the response from the remote device.
            Receive(s);
            receiveDone.WaitOne();

            Console.WriteLine("Response received : {0}", response);

            s.Shutdown(SocketShutdown.Both);
            s.Close();
          

            #region re

            //s.Bind(lep);

            //s.Listen(1000);

            //while (true)
            //{
            //    allDone.Reset();

            //    Console.WriteLine($"{iZaeler} Waiting for a connection...");
            //    int receivedDataSize = 265;

            //    s.BeginAccept(null, receivedDataSize, new AsyncCallback(AcceptReceiveDataCallback), s);

            //    Thread.Sleep(1000);

            //    iZaeler++;
            //}

            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e.ToString());
            //}

            #endregion re
        }
        private static void SendServer(Socket client, String data)
        {
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.UTF8.GetBytes(data);

            // Begin sending the data to the remote device.  
            client.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), client);
        }
        private static String response = String.Empty;

        private static void Receive(Socket client)
        {
            try
            {
                // Create the state object.
                StateObject state = new StateObject();
                state.workSocket = client;

                // Begin receiving the data from the remote device.
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Console.WriteLine("Try Connecting");

                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.
                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}",
                    client.RemoteEndPoint.ToString());

                // Signal that the connection has been made.
                connectDone.Set();
                Console.WriteLine("Connectet");

                bConectet = false;
            }
            catch (Exception e)
            {
                Console.WriteLine("Conection Failed");
                Console.WriteLine(e.ToString());
                Thread.Sleep(1000);
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

        private static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket
                // from the asynchronous state object.
                StateObject state = (StateObject)ar.AsyncState;
               
                Socket client = state.workSocket;

                // Read data from the remote device.
                int bytesRead = client.EndReceive(ar); //instance not set to  a ref wenn es daten empfngen soll

                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    // Get the rest of the data.
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    // All the data has arrived; put it in response.
                    if (state.sb.Length > 1)
                    {
                        response = state.sb.ToString();
                    }

                    // Signal that all bytes have been received.
                    receiveDone.Set();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
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
          
            }
            else if (e.Error != null)
            {
                // There was an error during the operation.
                Console.WriteLine("An error occurred: {0}", e.Error.Message);
             
            }
            else
            {
                // The operation completed normally.
                Console.WriteLine("Server Beendet");
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

        //public AsynchronousSocketListener()
        //{
        //}

        public static void StartListening(BackgroundWorker bw)
        {
            // Establish the local endpoint for the socket.
            // The DNS name of the computer
            const int port = 42069;
            //IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            //IPAddress ipAddress = ipHostInfo.AddressList[0];
            //const string localhost2 = "127.0.0.1";
            //Console.WriteLine("IP einggabe '127.0.X.X' ");
            //var ip = IPAddress.Parse(localhost2);

            if (!bw.CancellationPending)
            {

                //IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                //IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPAddress[] ipv4Addresses = Array.FindAll(Dns.GetHostEntry(string.Empty).AddressList, a => a.AddressFamily == AddressFamily.InterNetwork);

                IPEndPoint localEndPoint = new IPEndPoint(ipv4Addresses[0], port);
                Console.WriteLine($"Running On {localEndPoint}");

                // Create a TCP/IP socket.
                Socket listener = new Socket(ipv4Addresses[0].AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                // Bind the socket to the local endpoint and listen for incoming connections.
                try
                {
                    listener.Bind(localEndPoint);
                    listener.Listen(100);

                    while (true)
                    {
                        // Set the event to nonsignaled state.
                        allDoneServer.Reset();

                        // Start an asynchronous socket to listen for connections.
                        Console.WriteLine("Waiting for a connection...");
                        listener.BeginAccept(
                            new AsyncCallback(AcceptCallback),
                            listener);

                        // Wait until a connection is made before continuing.
                        allDoneServer.WaitOne();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            //Console.WriteLine("\nPress ENTER to continue...");
            //Console.Read();
        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            allDoneServer.Set();

            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            // Create the state object.
            StateObjectServer state = new StateObjectServer();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObjectServer.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObjectServer state = (StateObjectServer)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket.
            int bytesRead = handler.EndReceive(ar); // CHrasht wenn die verbindung gescannt wurde ohne das sie Wieder Ge schlossen wurde // Gefixt

            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.
                state.sb.Append(Encoding.ASCII.GetString(
                    state.buffer, 0, bytesRead));

                // Check for end-of-file tag. If it is not there, read
                // more data.
                content = state.sb.ToString();
                if (content.IndexOf("<EOF>") > -1)
                {
                    // All the data has been read from the
                    // client. Display it on the console.
                    Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                        content.Length, content);

                    // Echo the data back to the client.
                    Send(handler, content);
                }
                else
                {
                    // Not all data received. Get more.
                    handler.BeginReceive(state.buffer, 0, StateObjectServer.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
                }
            }
        }

        private static void Send(Socket handler, String data)
        {
            data = "Sendback " + data;

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

        #endregion ServerHosten
    }

    //https://stackoverflow.com/questions/28601678/calling-async-method-on-button-click
    //https://docs.microsoft.com/de-de/dotnet/desktop/winforms/controls/how-to-run-an-operation-in-the-background?view=netframeworkdesktop-4.8
    //https://docs.microsoft.com/de-de/dotnet/api/system.threading.manualresetevent?view=net-5.0
    //https://docs.microsoft.com/de-de/dotnet/framework/network-programming/asynchronous-client-socket-example
}