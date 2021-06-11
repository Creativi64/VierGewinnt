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
        private object[] GefundenEndpoints;
        private int iLezztesGefundene = 0;
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

            this.Hide();

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
            //GefundenEndpoints = await ConnectionSuchen();

            while (this.backgroundWorker1.IsBusy)
            {
                progressBar1.Increment(1);
                // Keep UI messages moving, so the form remains 
                // responsive during the asynchronous operation.
                //Application.DoEvents();
            }

        }
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            // Do not access the form's BackgroundWorker reference directly.
            // Instead, use the reference provided by the sender parameter.
            BackgroundWorker bw = sender as BackgroundWorker;

            // Extract the argument.
           int arg = (int)e.Argument;

            // Start the time-consuming operation.
            e.Result = ConnectionSuchen(bw, arg);
           
            // If the operation was canceled by the user,
            // set the DoWorkEventArgs.Cancel property to true.
            //if (bw.CancellationPending)
            //{
            //    e.Cancel = true;
            //}
        }

        private void backgroundWorker1_RunWorkerCompleted(
            object sender,
            RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                // The user canceled the operation.

                Console.WriteLine("Canceled");
                //MessageBox.Show("Operation was canceled");
            }
            else if (e.Error != null)
            {
                // There was an error during the operation.
                //string msg = String.Format("An error occurred: {0}", e.Error.Message);
                Console.WriteLine("An error occurred: {0}", e.Error.Message);
                //MessageBox.Show(msg);
            }
            else
            {
                Console.WriteLine("Suche Abgeschlossen");
                // The operation completed normally.
                GefundenEndpoints[iLezztesGefundene] = e.Result;
                iLezztesGefundene++;
                string msg = String.Format("Result = {0}", e.Result);
                Console.WriteLine("ergebnis");
                MessageBox.Show(msg);
            }
        }

        //private static async Task<IPEndPoint[]> ConnectionSuchen(BackgroundWorker bw, int sleepPeriod)
        //{
        //   IPEndPoint[] GefundeneEndPoints = new IPEndPoint[10];
        //    int iZaeler = 0;
        //    for (int i = 0; i < 255; i++)
        //    {
        //        for (int a = 0; a < 255; a++)
        //        {
        //            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //            IPAddress Ip = IPAddress.Parse($"192.168.{i}.{a}");
        //            IPEndPoint hostep = new IPEndPoint(Ip, 42069);

        //            IAsyncResult result = s.BeginConnect(Ip, 42069, null, null);

        //            bool success = result.AsyncWaitHandle.WaitOne(1, true);

        //            if (s.Connected)
        //            {

        //                Console.WriteLine($"gefunden auf {hostep}");
        //                Console.ReadLine();
        //                GefundeneEndPoints[iZaeler] = hostep;
        //                s.EndConnect(result);
        //                iZaeler++;
        //            }
        //            else
        //            {
        //                // NOTE, MUST CLOSE THE SOCKET

        //                s.Close();
        //                Console.WriteLine($"Nix Gefunden Bei auf {hostep}");

        //                //throw new ApplicationException("Failed to connect server.");
        //            }
        //        }
        //    }
        //    return GefundeneEndPoints;
        //}
        private IPEndPoint[] ConnectionSuchen(BackgroundWorker bw, int sleepPeriod)
        {
            IPEndPoint[] GefundeneEndPoints = new IPEndPoint[10];
            int iZaeler = 0;
            for (int i = 0; i < 255; i++)
            {
                for (int a = 0; a < 255; a++)
                {
                    Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    IPAddress Ip = IPAddress.Parse($"192.168.{i}.{a}");
                    IPEndPoint hostep = new IPEndPoint(Ip, 42069);

                    IAsyncResult result = s.BeginConnect(Ip, 42069, null, null);

                    bool success = result.AsyncWaitHandle.WaitOne(0, true);

                    if (s.Connected)
                    {

                        Console.WriteLine($"gefunden auf {hostep}");
                       
                        GefundeneEndPoints[iZaeler] = hostep;
                        s.EndConnect(result);
                        iZaeler++;
                    }
                    else
                    {
                        // NOTE, MUST CLOSE THE SOCKET

                        s.Close();
                        Console.WriteLine($"Nix Gefunden Bei auf {hostep}");

                        //throw new ApplicationException("Failed to connect server.");
                    }
                }
            }
            return GefundeneEndPoints;
        }

        private void btn_cancel_Click(object sender, EventArgs e)
        {
            this.backgroundWorker1.CancelAsync();
        }
    }

    //https://stackoverflow.com/questions/28601678/calling-async-method-on-button-click
    //https://docs.microsoft.com/de-de/dotnet/desktop/winforms/controls/how-to-run-an-operation-in-the-background?view=netframeworkdesktop-4.8
    //https://docs.microsoft.com/de-de/dotnet/api/system.threading.manualresetevent?view=net-5.0
    //https://docs.microsoft.com/de-de/dotnet/framework/network-programming/asynchronous-client-socket-example
}