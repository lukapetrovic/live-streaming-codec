using Fleck;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

using System.Windows.Forms;

namespace Display_Streamer
{
    public partial class Capture : Form
    {
        int selectX;
        int selectY;
        int selectWidth;
        int selectHeight;
        bool selectStart = false;
        public Pen selectPen;

        Bitmap first_frame;

        public Capture()
        {
            InitializeComponent();
            captureDisplay();

        }

        private void captureDisplay()
        {
            selectPen = new Pen(Color.Red, 3);

            //Create the Bitmap
            Bitmap printscreen = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                                     Screen.PrimaryScreen.Bounds.Height);
            //Create the Graphic Variable with screen Dimensions
            Graphics graphics = Graphics.FromImage(printscreen);

            //Copy Image from the screen
            graphics.CopyFromScreen(0, 0, 0, 0, printscreen.Size);

            //Create a temporal memory stream for the image
            using (MemoryStream s = new MemoryStream())
            {
                //save graphic variable into memory
                printscreen.Save(s, ImageFormat.Bmp);
                pictureBox1.Size = new System.Drawing.Size(this.Width, this.Height);
                //set the picture box with temporary stream
                pictureBox1.Image = Image.FromStream(s);
            }

            //Cross Cursor
            Cursor = Cursors.Cross;
        }

        private MemoryStream captureArea()
        {
            var bmp = new Bitmap(selectWidth, selectHeight, PixelFormat.Format32bppArgb);
            Graphics graphics = Graphics.FromImage(bmp);

            graphics.CopyFromScreen(selectX, selectY, 0, 0, new Size(selectWidth, selectHeight), CopyPixelOperation.SourceCopy);


            if(first_frame == null)
            {
                first_frame = new Bitmap(bmp);
            }
            else
            {
                for (int i = 0; i < selectWidth; i++)
                {
                    for (int j = 0; j < selectHeight; j++)
                    {
                        Color pixel_old = first_frame.GetPixel(i, j);
                        Color pixel_new = bmp.GetPixel(i, j);
                        if(pixel_old == pixel_new)
                        {
                            Console.WriteLine("Same pixel " + i + j);
                        }
                    }
                }
            }

            var stream = new MemoryStream();
            bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);

            return stream;

        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            selectX = e.X;
            selectY = e.Y;
            selectStart = true;
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            selectStart = false;

            this.Hide();

            Server server = new Server(captureArea(), new Size(selectWidth, selectHeight));
            server.Show();

        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (selectStart)
            {
                pictureBox1.Refresh();
                selectWidth = e.X - selectX;
                selectHeight = e.Y - selectY;
                pictureBox1.CreateGraphics().DrawRectangle(selectPen, selectX,
             selectY, selectWidth, selectHeight);
            }
        }
    }
}
