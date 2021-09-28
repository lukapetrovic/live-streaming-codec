using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Display_Streamer

{
    class Streamer
    {
        Bitmap frame_zero;
        Bitmap frame_one;
        bool working = false;
        int msgNum = 0;

        public MemoryStream capture(Rectangle captureArea)
        {

            if (!working)
            {
                working = true;
                if (frame_one != null)
                {
                    frame_zero = frame_one;
                }

                frame_one = new Bitmap(captureArea.Width, captureArea.Height, PixelFormat.Format32bppArgb);
                Graphics graphics = Graphics.FromImage(frame_one);

                graphics.CopyFromScreen(captureArea.X, captureArea.Y, 0, 0, new Size(captureArea.Width, captureArea.Height), CopyPixelOperation.SourceCopy);

                // Send first full frame
                if (msgNum % 5 == 0)
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
                byte[] errorMetadata = new byte[16];
                errorMetadata[0] = 0;
                return new MemoryStream(errorMetadata);
            }
        }

        private MemoryStream phaseOne(Bitmap new_frame)
        {
            MemoryStream frameMem = new MemoryStream();
            new_frame.Save(frameMem, System.Drawing.Imaging.ImageFormat.Png);

            byte[] arrayWithMetadata = insertMetadata(frameMem.ToArray(), 1, 0, 0, 0);

            MemoryStream byteFrameMem = new MemoryStream();
            byteFrameMem.Write(arrayWithMetadata, 0, arrayWithMetadata.Length);

            working = false;
            msgNum++;

            return byteFrameMem;
        }

        private MemoryStream phaseTwo(Rectangle captureArea, Bitmap old_frame, Bitmap new_frame)
        {

            int pixelNum = captureArea.Width * captureArea.Height;
            byte[] residualArrayRed = new byte[pixelNum];
            byte[] residualArrayGreen = new byte[pixelNum];
            byte[] residualArrayBlue = new byte[pixelNum];

            int counter = 0;
            // Residual array
            for (int i = 0; i < captureArea.Height; i++)
            {
                for (int j = 0; j < captureArea.Width; j++)
                {
                    Color pixel_old = old_frame.GetPixel(j, i);
                    Color pixel_new = new_frame.GetPixel(j, i);
                    residualArrayRed[counter] = (byte)(((pixel_old.R - pixel_new.R) / 2) + 127);
                    residualArrayGreen[counter] = (byte)(((pixel_old.G - pixel_new.G) / 2) + 127);
                    residualArrayBlue[counter] = (byte)(((pixel_old.B - pixel_new.B) / 2) + 127);
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
            array.CopyTo(extendedArray, 16);

            return extendedArray;
        }

        private byte[] joinArrays(byte[] redArray, byte[] greenArray, byte[] blueArray)
        {
            int totalSize = redArray.Length + greenArray.Length + blueArray.Length;
            byte[] joinedArray = new byte[totalSize];
            redArray.CopyTo(joinedArray, 0);
            greenArray.CopyTo(joinedArray, redArray.Length);
            blueArray.CopyTo(joinedArray, redArray.Length + greenArray.Length);

            return joinedArray;
        }

        private byte[] compressArray(byte[] array, int numPixel)
        {
            int numSame = 1, numCompressed = 0;
            int last;
            byte[] compressedArray = new byte[numPixel * 2];

            last = array[0];

            for (int i = 1; i < numPixel; i++)
            {
                if (last != array[i] || numSame > 254)
                {
                    compressedArray[numCompressed++] = (byte)numSame;
                    compressedArray[numCompressed++] = (byte)last;
                    last = array[i];
                    numSame = 1;
                }
                else
                {
                    numSame++;
                }
            }

            // Add last pair
            compressedArray[numCompressed++] = (byte)numSame;
            compressedArray[numCompressed++] = (byte)last;

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
