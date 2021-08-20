using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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
            public int l;
            // column
            public int c;
            // red
            public int r;
            // green
            public int g;
            // blue
            public int b;
        }

        public MemoryStream capture(Rectangle captureArea)
        {
            if (!working)
            {
                if(frame_one != null)
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
            new_frame.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
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
                        pixels[pixelsChangedNum].l = j;
                        pixels[pixelsChangedNum].c = i;
                        pixels[pixelsChangedNum].r = pixel_new.R;
                        pixels[pixelsChangedNum].g = pixel_new.G;
                        pixels[pixelsChangedNum].b = pixel_new.B;
                        pixelsChangedNum++;
                    }
                }
            }

            Pixel[] pixelsResized = new Pixel[pixelsChangedNum];
            for (int i = 0; i < pixelsChangedNum; i++)
            {
                pixelsResized[i] = pixels[i];
            }

            MemoryStream ms = new MemoryStream();
            string json = JsonConvert.SerializeObject(pixelsResized);
            byte[] jsonByte = Encoding.UTF8.GetBytes(json);
            ms.Write(jsonByte, 0, jsonByte.Length);

            working = false;
            msgNum++;
            return ms;
        }
    }

}
