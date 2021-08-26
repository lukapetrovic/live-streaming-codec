using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace Display_Streamer
{
    public partial class Capture : Form
    {
        int selectX;
        int selectY;
        public int selectWidth;
        public int selectHeight;
        bool selectStart = false;
        public Pen selectPen;
        Rectangle captureRect;
        DeviceInfo screen;


        public Capture(DeviceInfo selectedScreen)
        {
            screen = selectedScreen;
            InitializeComponent();
            captureDisplay();
        }

        private void captureDisplay()
        {
            selectPen = new Pen(Color.Red, 3);

            //Create the Bitmap
            Bitmap printscreen = new Bitmap(screen.HorizontalResolution,
                                     screen.VerticalResolution);

            //Create the Graphic Variable with screen Dimensions
            Graphics graphics = Graphics.FromImage(printscreen);

            //Copy Image from the screen
            graphics.CopyFromScreen(screen.MonitorArea.X, screen.MonitorArea.Y, 0, 0, printscreen.Size);

            //Create a temporal memory stream for the image
            using (MemoryStream s = new MemoryStream())
            {
                //save graphic variable into memory
                printscreen.Save(s, ImageFormat.Bmp);
                //set the picture box with temporary stream
                pictureBox1.Image = Image.FromStream(s);
            }

            //Cross Cursor
            Cursor = Cursors.Cross;
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

            this.Close();

            captureRect = new Rectangle(selectX, selectY, selectWidth, selectHeight);
            Server server = new Server(captureRect);
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
