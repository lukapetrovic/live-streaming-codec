using Fleck;
using System;
using System.Drawing;
using System.IO;
using System.Timers;
using System.Windows.Forms;

namespace Display_Streamer
{
    public partial class Server : Form
    {

        WebSocketServer server;
        MemoryStream image;
        Rectangle captureArea;

        public Server(Rectangle captureRect)
        {
            InitializeComponent();

            captureArea = captureRect;
            pictureBox1.Size = new System.Drawing.Size(captureRect.Width, captureRect.Height);
            //pictureBox1.Image = Image.FromStream(capture.captureArea());

            start();

        }

        private void start()
        {
            server = new WebSocketServer("ws://0.0.0.0:3000");
            server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    Console.WriteLine("Open!");
                    Streamer streamer = new Streamer();
                    System.Timers.Timer screenshotTimer = new System.Timers.Timer();
                    screenshotTimer.Elapsed += (sender, arguments) => OnTimedEvent(arguments, socket, streamer);
                    screenshotTimer.Interval = 5000;
                    screenshotTimer.Enabled = true;
                };
                socket.OnClose = () => Console.WriteLine("Close!");
                socket.OnMessage = message => Console.WriteLine("Message!");
            });
        }

        // Specify what you want to happen when the Elapsed event is raised.
        private void OnTimedEvent(ElapsedEventArgs e, IWebSocketConnection socket, Streamer streamer)
        {
            image = streamer.capture(captureArea);
            socket.Send(image.ToArray());
        }

        

        private void button1_Click(object sender, EventArgs e)
        {
            server.Dispose();
            this.Close();
            Capture capture = new Capture();
            capture.Show();
        }
    }
}
