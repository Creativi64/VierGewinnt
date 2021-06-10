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
using System.Net.Sockets;
using static System.Console;
using System.Runtime.InteropServices;
namespace VierGewinnt
{
    public partial class Form3 : Form
    {
        #region Console

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AllocConsole();

        #endregion
        public bool Fullscreen { get; private set; }
        public Form3()
        {
            InitializeComponent();
        }

        private void btn_Test_Click(object sender, EventArgs e)
        {
            // Daten, die gesendet werden
            const string textToSend = "Hallo Welt";
            // Endpunkt, zu dem verbunden wird
            const string localhost = "127.0.0.1";
            const int port = 80;

            var data = Encoding.UTF8.GetBytes(textToSend);
            var ip = IPAddress.Parse(localhost);
            var ipEndPoint = new IPEndPoint(ip, port);

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

                // Verbindung wird geschlossen
            }
        }
    }
}
