using Fleck;
using System;
using System.Collections.Generic;
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

        //Websocket clients container
        List<IWebSocketConnection> clients = new List<IWebSocketConnection>();


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
                    clients.Add(socket);
                    Console.WriteLine("Open!");
                    Streamer streamer = new Streamer();
                    if (!label2.IsDisposed)
                    {
                        label2.Invoke(incrCount);
                    }
                    System.Timers.Timer screenshotTimer = new System.Timers.Timer();
                    screenshotTimer.Elapsed += (sender, arguments) => OnTimedEvent(arguments, socket, streamer);
                    screenshotTimer.Interval = Config.refresh_rate;
                    screenshotTimer.Enabled = true;
                };
                socket.OnClose = () => 
                {
                    try
                    {
                        Console.WriteLine("Close!");
                        if (!label2.IsDisposed)
                        {
                            label2.Invoke(descrCount);
                        }
                    }
                    catch (Exception)
                    {

                    }

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
            this.Close();
            Config.main_form.Show();
        }

        private void Server_FormClosed(object sender, FormClosedEventArgs e)
        {
            closeServer();
            Config.main_form.Show();
        }

        private void closeServer()
        {
            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].Close();
            }
            server.Dispose();
        }
    }
}
