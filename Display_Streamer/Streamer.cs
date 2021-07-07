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
        Bitmap first_frame;
        bool working = false;

        public MemoryStream capture(Rectangle captureArea)
        {
            if (!working)
            {
                working = true;
                var bmp = new Bitmap(captureArea.Width, captureArea.Height, PixelFormat.Format32bppArgb);
                Graphics graphics = Graphics.FromImage(bmp);

                graphics.CopyFromScreen(captureArea.X, captureArea.Y, 0, 0, new Size(captureArea.Width, captureArea.Height), CopyPixelOperation.SourceCopy);

                int[] row = new int[captureArea.Width * captureArea.Height];
                int rowNum = 0;
                var stream = new MemoryStream();

                // Slanje prvog frejma
                if (first_frame == null)
                {
                    first_frame = new Bitmap(bmp);
                    first_frame.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                }
                // Slanje razlike
                else
                {
                    // Koji pikseli su se promenili
                    for (int i = 0; i < captureArea.Width; i++)
                    {
                        for (int j = 0; j < captureArea.Height; j++)
                        {
                            Color pixel_old = first_frame.GetPixel(i, j);
                            Color pixel_new = bmp.GetPixel(i, j);
                            if (pixel_old == pixel_new)
                            {
                                Console.WriteLine("Same pixel " + i + j);
                                row[rowNum] = i;
                            }
                        }
                        rowNum++;
                    }
                    byte[] rowByte = new byte[rowNum];

                    Buffer.BlockCopy(row, 0, rowByte, 0, rowNum);

                    stream.Write(rowByte, 0, rowNum);
                }
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
