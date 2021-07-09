using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace Display_Streamer

{
    class Streamer
    {
        Bitmap last_frame;
        bool working = false;

        struct Pixel
        {
            public int row;
            public int col;
        }

        public MemoryStream capture(Rectangle captureArea)
        {
            if (!working)
            {
                working = true;
                var bmp = new Bitmap(captureArea.Width, captureArea.Height, PixelFormat.Format32bppArgb);
                Graphics graphics = Graphics.FromImage(bmp);

                graphics.CopyFromScreen(captureArea.X, captureArea.Y, 0, 0, new Size(captureArea.Width, captureArea.Height), CopyPixelOperation.SourceCopy);

                int pixelsChangedNum = 0;
                Pixel[] pixels = new Pixel[captureArea.Width * captureArea.Height];
                var stream = new MemoryStream();

                // Slanje prvog frejma
                if (last_frame == null)
                {
                    last_frame.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                }
                // Slanje razlike
                else
                {
                    // Koji pikseli su se promenili
                    for (int i = 0; i < captureArea.Width; i++)
                    {
                        for (int j = 0; j < captureArea.Height; j++)
                        {
                            Color pixel_old = last_frame.GetPixel(i, j);
                            Color pixel_new = bmp.GetPixel(i, j);
                            if (pixel_old == pixel_new)
                            {
                                Console.WriteLine("Same pixel " + i + j);
                                pixels[pixelsChangedNum].row = i;
                                pixels[pixelsChangedNum].col = j;
                                pixelsChangedNum++;
                            }
                        }
                        
                    }
                    //byte[] rowByte = new byte[rowNum];

                    //Buffer.BlockCopy(row, 0, rowByte, 0, rowNum);

                    //stream.Write(rowByte, 0, rowNum);
                    stream.Write(new byte[3], 0, 3);
                }
                // Novi frejm se uzima za sledeci krug
                last_frame = new Bitmap(bmp);
                working = false;
                return stream;
            }
            else
            {
                MemoryStream emptyStream = new MemoryStream(new byte[3]);
                return emptyStream;
            }
        }
    }
}
