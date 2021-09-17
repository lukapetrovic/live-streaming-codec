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

                // Send first full frame
                if (msgNum == 1)
                {
                    return phaseOne(frame_one);
                }
                // Send frame difference
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

            byte[] arrayWithMetadata = insertMetadata(stream.ToArray(), 1, 0, 0, 0);
            stream.Write(arrayWithMetadata, 0, arrayWithMetadata.Length);
            working = false;
            msgNum++;
            return stream;
        }

        private MemoryStream phaseTwo(Rectangle captureArea, Bitmap old_frame, Bitmap new_frame)
        {

            int pixelNum = captureArea.Width * captureArea.Height;
            byte[] residualArrayRed = new byte[pixelNum];
            byte[] residualArrayGreen = new byte[pixelNum];
            byte[] residualArrayBlue = new byte[pixelNum];

            int counter = 0;
            // Residual array
            for (int i = 0; i < captureArea.Width; i++)
            {
                for (int j = 0; j < captureArea.Height; j++)
                {
                    Color pixel_old = old_frame.GetPixel(i, j);
                    Color pixel_new = new_frame.GetPixel(i, j);
                    residualArrayRed[counter] = (byte)(((pixel_old.R - pixel_new.R) / 2) + 127);
                    residualArrayGreen[counter] = (byte)((pixel_old.G - pixel_new.G + 127) / 2);
                    residualArrayBlue[counter] = (byte)((pixel_old.B - pixel_new.B + 127) / 2);
                    counter++;
                }
            }

            byte[] compressedRed = compressArray(residualArrayRed, pixelNum);
            byte[] compressedGreen = compressArray(residualArrayGreen, pixelNum);
            byte[] compressedBlue = compressArray(residualArrayBlue, pixelNum);


            byte[] joinedArray = joinArrays(compressedRed, compressedGreen, compressedBlue);

            byte[] arrayWithMetadata = insertMetadata(joinedArray, 2, compressedRed.Length, compressedGreen.Length, compressedBlue.Length);

            MemoryStream ms = new MemoryStream();
            ms.Write(arrayWithMetadata, 0, arrayWithMetadata.Length);

            working = false;
            msgNum++;
            return ms;
        }

        private byte[] insertMetadata(byte[] array, int frameType, int redCount, int greenCount, int blueCount)
        {
            byte[] metadata = new byte[16];

            byte[] frameTypeBytes = BitConverter.GetBytes(frameType);
            frameTypeBytes.CopyTo(metadata, 0);
            
            byte[] redCountBytes = BitConverter.GetBytes(redCount);
            redCountBytes.CopyTo(metadata, 4);

            byte[] greenCountBytes = BitConverter.GetBytes(greenCount);
            greenCountBytes.CopyTo(metadata, 8);

            byte[] blueCountBytes = BitConverter.GetBytes(blueCount);
            blueCountBytes.CopyTo(metadata, 12);

            byte[] extendedArray = new byte[array.Length + 16];
            metadata.CopyTo(extendedArray, 0);
            array.CopyTo(extendedArray, 15);

            return extendedArray;
        }

        private byte[] joinArrays(byte[] redArray, byte[] greenArray, byte[] blueArray)
        {
            int totalSize = redArray.Length + greenArray.Length + blueArray.Length;
            byte[] joinedArray = new byte[totalSize];
            return joinedArray;
        }

        private byte[] compressArray(byte[] array, int numPixel)
        {
            int numSame = 0, numCompressed = 0;
            int last, next;
            byte[] compressedArray = new byte[numPixel];

            last = array[0];

            for (int i = 1; i < numPixel; i++)
            {
                next = array[i];
                if (last != next && numSame > 254)
                {
                    compressedArray[numCompressed++] = (byte)numSame;
                    compressedArray[numCompressed++] = (byte)last;
                    last = next;
                    numSame = 0;
                }
                else
                {
                    numSame++;
                }
            }
            byte[] compressedArrayResized = resizeArray(compressedArray, numCompressed);
            return compressedArrayResized;
        }

        private byte[] resizeArray(byte[] array, int compressNumPixel)
        {
            // Resize compressed array
            byte[] compressedArrayResized = new byte[compressNumPixel];
            for (int i = 0; i < compressNumPixel; i++)
            {
                compressedArrayResized[i] = array[i];
            }

            return compressedArrayResized;
        }

    }
}
