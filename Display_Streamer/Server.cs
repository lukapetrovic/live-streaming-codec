using Fleck;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Timers;
using System.Windows.Forms;

namespace Display_Streamer
{
    public partial class Server : Form
    {

        WebSocketServer server;
        MemoryStream image;

        public Server(MemoryStream stream, System.Drawing.Size size)
        {
            InitializeComponent();
            pictureBox1.Size = new System.Drawing.Size(size.Width, size.Height);
            pictureBox1.Image = Image.FromStream(stream);
            image = stream;
            start();

        }

        private void start()
        {

            System.Timers.Timer screenshotTimer = new System.Timers.Timer();

            server = new WebSocketServer("ws://0.0.0.0:3000");
            server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    Console.WriteLine("Open!");
                    screenshotTimer.Elapsed += (sender, arguments) => OnTimedEvent(arguments, socket);
                    screenshotTimer.Interval = 1000;
                    screenshotTimer.Enabled = true;
                };
                socket.OnClose = () => Console.WriteLine("Close!");
                socket.OnMessage = message => Console.WriteLine("Message!");
            });
        }

        // Specify what you want to happen when the Elapsed event is raised.
        private void OnTimedEvent(ElapsedEventArgs e, IWebSocketConnection socket)
        {
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
