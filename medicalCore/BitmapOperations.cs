using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace DicomImageViewer
{
   public class BitmapOperations
    {
        public byte[, ,] loadBmp(Bitmap bitmap)
        {
            Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            BitmapData image = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            byte[, ,] output = new byte[image.Width, image.Height, 3];
            unsafe
            {
                byte* ptr = (byte*)image.Scan0.ToPointer();
                for (int y = 0; y < image.Height; y++)
                {
                    ptr = (byte*)image.Scan0.ToPointer();
                    ptr += (y * image.Stride);
                    for (int x = 0; x < (image.Width) * 1; x += 1, ptr += 3)
                    {
                        output[x, y, 0] = ptr[0];
                        output[x, y, 1] = ptr[1];
                        output[x, y, 2] = ptr[2];
                    }
                }
            }
            bitmap.UnlockBits(image);
            return output;
        }

        public Bitmap grayScale(Bitmap bitmap)
        {
            int i, j, c;
            Bitmap image = bitmap;
            BitmapData bitmapData1 = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                                     ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            unsafe
            {
                byte* imagePointer1 = (byte*)bitmapData1.Scan0;

                for (i = 0; i < bitmapData1.Height; i++)
                {
                    imagePointer1 = (byte*)bitmapData1.Scan0.ToPointer();
                    imagePointer1 += (i * bitmapData1.Stride);
                    for (j = 0; j < bitmapData1.Width; j++)
                    {
                        c = (int)((imagePointer1[0] + imagePointer1[1] + imagePointer1[2]) / 3.0);

                        imagePointer1[0] = (byte)c;
                        imagePointer1[1] = (byte)c;
                        imagePointer1[2] = (byte)c;
                        imagePointer1[3] = (byte)255;

                        imagePointer1 += 4;
                    }

                }
            }
            image.UnlockBits(bitmapData1);
            return image;
        }

        public byte[,] grayScale(byte[, ,] RgbArray)
        {
            int i, j, c;

            byte[,] output = new byte[RgbArray.GetLength(0), RgbArray.GetLength(1)];
            for (i = 0; i < RgbArray.GetLength(0); i++)
            {
                for (j = 0; j < RgbArray.GetLength(1); j++)
                {
                    c = (int)((RgbArray[i, j, 0] + RgbArray[i, j, 1] + RgbArray[i, j, 2]) / 3.0);
                    output[i, j] = (byte)c;
                }

            }
            return output;
        }
    }
}
