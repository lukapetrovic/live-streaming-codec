using Fleck;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Timers;
using System.Windows.Forms;
using System.Diagnostics;

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

        // Server send rate
        System.Timers.Timer screenshotTimer = new System.Timers.Timer();


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

            Streamer streamer = new Streamer();

            screenshotTimer.Elapsed += (sender, arguments) => OnTimedEvent(arguments, clients, streamer);
            screenshotTimer.Interval = Config.refresh_rate;
            screenshotTimer.Enabled = true;

            server.Start(socket =>
            {
                
                socket.OnOpen = () =>
                {
                    clients.Add(socket);
                    Console.WriteLine("Open!");
                    if (!label2.IsDisposed)
                    {
                        label2.Invoke(incrCount);
                    }

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
        private void OnTimedEvent(ElapsedEventArgs e, List<IWebSocketConnection> clients, Streamer streamer)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            image = streamer.capture(captureArea);
            byte[] sendArray = image.ToArray();

            sw.Stop();
            Debug.WriteLine("Elapsed={0}", sw.Elapsed);


            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].Send(sendArray);
            }
        }

        public void increaseDeviceCount()
        {
            connectedDevices++;
            label2.Text = connectedDevices.ToString();
        }

        public void decreaseDeviceCount()
        {
            connectedDevices--;
            if(connectedDevices < 0)
            {
                connectedDevices = 0;
            }
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
            screenshotTimer.Enabled = false;
            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].Close();
            }
            server.Dispose();
        }
    }
}
