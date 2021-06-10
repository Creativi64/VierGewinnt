using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.WebSockets;
using System.Net.Sockets;
using static System.Console;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections.Specialized;
using System.Collections;


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

        public Form3(bool _Fullscreen)
        {
            InitializeComponent();

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
            AimationFlag = true;
            this.Hide();

            MessageBox.Show("Spiel Beendet",
                "Close Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            base.OnClosed(e);
            Application.Exit();
            AimationFlag = false;
        }
        private void btn_Test_Click(object sender, EventArgs e)
        {
            // Daten, die gesendet werden
            string textToSend = txB_1.Text;
            // Endpunkt, zu dem verbunden wird
            const string localhost = "127.0.0.1";
            const int port = 13000;

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
        #region Empänger
        public static ManualResetEvent allDone = new ManualResetEvent(false);

        

        //private void btn_Test2_Click(object sender, EventArgs e)
        //{

        //    const string localhost2 = "127.0.0.1";
        //    var ip2 = IPAddress.Parse(localhost2);
        //    const int port2 = 13000;

        //    IPEndPoint lep = new IPEndPoint(ip2, port2);

        //    Socket s = new Socket(lep.AddressFamily,
        //                       SocketType.Stream,
        //                             ProtocolType.Tcp);
        //    try
        //    {
        //        s.Bind(lep);
        //        s.Listen(1000);

        //        while (true)
        //        {
        //            allDone.Reset();

        //            Console.WriteLine("Waiting for a connection...");
        //            int receivedDataSize = 10;
        //            s.BeginAccept(null, receivedDataSize, new AsyncCallback(AcceptReceiveDataCallback), s);

        //            allDone.WaitOne();
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        Console.WriteLine(e.ToString());
        //    }
        //}
        //public static void AcceptReceiveDataCallback(IAsyncResult ar)
        //{
        //    // Get the socket that handles the client request.
        //    Socket listener = (Socket)ar.AsyncState;

        //    // End the operation and display the received data on the console.
        //    byte[] Buffer;
        //    int bytesTransferred;
        //    Socket handler = listener.EndAccept(out Buffer, out bytesTransferred, ar);
        //    string stringTransferred = Encoding.ASCII.GetString(Buffer, 0, bytesTransferred);

        //    Console.WriteLine(stringTransferred);
        //    Console.WriteLine("Size of data transferred is {0}", bytesTransferred);

        //    // Create the state object for the asynchronous receive.
        //    //StateObject state = new StateObject();
        //    //state.workSocket = handler;
        //    //handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
        //    //new AsyncCallback(ReadCallback), state);
        //}
        #endregion
    }
    //https://docs.microsoft.com/de-de/dotnet/api/system.threading.manualresetevent?view=net-5.0
    //https://docs.microsoft.com/de-de/dotnet/framework/network-programming/asynchronous-client-socket-example
}