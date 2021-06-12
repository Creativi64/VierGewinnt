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

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            // Do not access the form's BackgroundWorker reference directly.
            // Instead, use the reference provided by the sender parameter.
            BackgroundWorker bw = sender as BackgroundWorker;

            // Extract the argument.
            int arg = (int)e.Argument;

            // Start the time-consuming operation.
            ConnectionSuchen(bw, arg);

            // If the operation was canceled by the user,
            // set the DoWorkEventArgs.Cancel property to true.
            if (bw.CancellationPending)
            {
                e.Cancel = true;
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender,RunWorkerCompletedEventArgs e)
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

        private void ConnectionSuchen(BackgroundWorker bw, int sleepPeriod)
        {
            //IPEndPoint[] GefundeneEndPoints = new IPEndPoint[10];
            //int iZaeler = 0;
            for (int i = 0; i < GefundenEndpoints.Length; i++)
            {
                GefundenEndpoints[i]= null;
            }
            int iHöchsteProzent = 0;

            int NetzverkBereich1 = 5, NetzverkBereich2 = 255;

            double iProgress;
            string NetzBereich = "127.0.";
            string NetzBereich1 = "192.168.";

            for (int i = 0; i <= NetzverkBereich1; i++)
            {
                for (int a = 0; a <= NetzverkBereich2; a++)
                {
                    if (!bw.CancellationPending)
                    {
                        Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        IPAddress Ip = IPAddress.Parse($"{NetzBereich}{i}.{a}");
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
                        iProgress = (a * i);
                        iProgress /= (NetzverkBereich1 *NetzverkBereich2 );
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
    }

    //https://stackoverflow.com/questions/28601678/calling-async-method-on-button-click
    //https://docs.microsoft.com/de-de/dotnet/desktop/winforms/controls/how-to-run-an-operation-in-the-background?view=netframeworkdesktop-4.8
    //https://docs.microsoft.com/de-de/dotnet/api/system.threading.manualresetevent?view=net-5.0
    //https://docs.microsoft.com/de-de/dotnet/framework/network-programming/asynchronous-client-socket-example
}