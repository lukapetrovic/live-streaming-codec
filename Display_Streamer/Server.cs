using Fleck;
using System;
using System.Drawing;
using System.Drawing.Imaging;
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
        int connectedDevices = 0;
        delegate void ChangingDeviceCount();

        public Server(Rectangle captureRect)
        {
            InitializeComponent();

            captureArea = captureRect;

            start();

        }

        private void start()
        {
            ChangingDeviceCount incrCount = new ChangingDeviceCount(increaseDeviceCount);
            ChangingDeviceCount descrCount = new ChangingDeviceCount(decreaseDeviceCount);

            server = new WebSocketServer("ws://0.0.0.0:3000");
            server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    Console.WriteLine("Open!");
                    Streamer streamer = new Streamer();
                    //label2.Invoke(incrCount);
                    System.Timers.Timer screenshotTimer = new System.Timers.Timer();
                    screenshotTimer.Elapsed += (sender, arguments) => OnTimedEvent(arguments, socket, streamer);
                    screenshotTimer.Interval = Config.refresh_rate;
                    screenshotTimer.Enabled = true;
                };
                socket.OnClose = () => 
                {
                    Console.WriteLine("Close!");
                    //label2.Invoke(descrCount);
                };
                socket.OnMessage = message => Console.WriteLine("Message!");
            });
        }

        // Specify what you want to happen when the Elapsed event is raised.
        private void OnTimedEvent(ElapsedEventArgs e, IWebSocketConnection socket, Streamer streamer)
        {
            image = streamer.capture(captureArea);
            socket.Send(image.ToArray());
        }

        public void increaseDeviceCount()
        {
            connectedDevices++;
            label2.Text = connectedDevices.ToString();
        }

        public void decreaseDeviceCount()
        {
            connectedDevices--;
            label2.Text = connectedDevices.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            server.Dispose();
            this.Close();
            Config.main_form.Show();
        }

        private void Server_FormClosed(object sender, FormClosedEventArgs e)
        {
            server.Dispose();
            Config.main_form.Show();
        }
    }
}
