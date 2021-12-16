using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace Display_Streamer
{
    public partial class Capture : Form
    {

        // Top rectangle point 
        int selectX;
        // Left rectangle point
        int selectY;
        // Width of the rectangle
        public int selectWidth;
        // Height of the rectangle
        public int selectHeight;
        // Selection start time
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

            // Create a bitmap for raw pixel storage
            Bitmap printscreen = new Bitmap(screen.HorizontalResolution,
                                     screen.VerticalResolution);

            // Create a graphics variable for graphic manipulation
            Graphics graphics = Graphics.FromImage(printscreen);

            // Copy the image from the screen into the graphics variable
            graphics.CopyFromScreen(screen.MonitorArea.X, screen.MonitorArea.Y, 0, 0, printscreen.Size);

            // Create a temporal memory stream for the image
            using (MemoryStream s = new MemoryStream())
            {
                // Save graphics variable into memory
                printscreen.Save(s, ImageFormat.Bmp);
                // Set the picture box with temporary stream
                pictureBox1.Image = Image.FromStream(s);
            }

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
            // Close current form
            this.Close();
            // Rectangle based on captured coordinates and monitor position
            captureRect = new Rectangle(selectX + screen.MonitorArea.X, selectY + screen.MonitorArea.Y, selectWidth, selectHeight);
            // Start the server with rectangle coordinates
            Server server = new Server(captureRect);
            // Show server form
            server.Show();
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            // If user has finished with the rectangle selection
            if (selectStart)
            {
                // Redraw the graphics on screen
                pictureBox1.Refresh();
                selectWidth = e.X - selectX;
                selectHeight = e.Y - selectY;
                // Draw a dotted rectangle on screen as a visual feedback
                pictureBox1.CreateGraphics().DrawRectangle(selectPen, selectX,
             selectY, selectWidth, selectHeight);
            }
        }
    }
}
