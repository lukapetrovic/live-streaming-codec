using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Display_Streamer

{
    class Streamer
    {
        Bitmap frame_zero;
        Bitmap frame_one;
        bool working = false;
        int msgNum = 1;

        struct Pixel
        {
            // row/line
            public int row;
            // column
            public int column;
            // red
            public int red;
            // green
            public int green;
            // blue
            public int blue;
        }

        public MemoryStream capture(Rectangle captureArea)
        {
            if (!working)
            {
                if (frame_one != null)
                {
                    frame_zero = frame_one;
                }

                working = true;
                frame_one = new Bitmap(captureArea.Width, captureArea.Height, PixelFormat.Format32bppArgb);
                Graphics graphics = Graphics.FromImage(frame_one);

                graphics.CopyFromScreen(captureArea.X, captureArea.Y, 0, 0, new Size(captureArea.Width, captureArea.Height), CopyPixelOperation.SourceCopy);

                // Slanje prvog frejma
                if (msgNum == 1)
                {
                    return phaseOne(frame_one);
                }
                // Slanje razlike
                else
                {
                    return phaseTwo(captureArea, frame_zero, frame_one);
                }
            }
            else
            {
                return new MemoryStream(new byte[5]);
            }
        }

        private MemoryStream phaseOne(Bitmap new_frame)
        {
            MemoryStream stream = new MemoryStream();
            new_frame.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
            working = false;
            msgNum++;
            return stream;
        }

        private MemoryStream phaseTwo(Rectangle captureArea, Bitmap old_frame, Bitmap new_frame)
        {

            int pixelsChangedNum = 0;
            Pixel[] pixels = new Pixel[captureArea.Width * captureArea.Height];

            // Koji pikseli su se promenili
            for (int i = 0; i < captureArea.Width; i++)
            {
                for (int j = 0; j < captureArea.Height; j++)
                {
                    Color pixel_old = old_frame.GetPixel(i, j);
                    Color pixel_new = new_frame.GetPixel(i, j);
                    if (pixel_old != pixel_new)
                    {
                        pixels[pixelsChangedNum].row = j;
                        pixels[pixelsChangedNum].column = i;
                        pixels[pixelsChangedNum].red = pixel_new.R;
                        pixels[pixelsChangedNum].green = pixel_new.G;
                        pixels[pixelsChangedNum].blue = pixel_new.B;
                        pixelsChangedNum++;
                    }
                }
            }

            int structSize = 20;
            byte[] arr = new byte[structSize * pixelsChangedNum];
            // Initialize unmanged memory to hold the struct.
            IntPtr pnt = Marshal.AllocHGlobal(structSize);

            for (int i = 0; i < pixelsChangedNum; i++)
            {
                // Copy the struct to unmanaged memory.
                Marshal.StructureToPtr(pixels[i], pnt, false);
                Marshal.Copy(pnt, arr, i * structSize, structSize);
            }

            Marshal.FreeHGlobal(pnt);

            MemoryStream ms = new MemoryStream();
            ms.Write(arr, 0, arr.Length);

            working = false;
            msgNum++;
            return ms;
        }
    }
}
