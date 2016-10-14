using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Imaging;
using System.Drawing;

namespace DicomImageViewer
{
    public class convertToBitmap

    {
        // For Window Level
        public int winMin { get; set; }
        public int winMax { get; set; }
        int winCentre;
        int winWidth;
        byte[] lut8 = new byte[256];
        byte[] lut16 = new byte[65536];

        int maxPixelValue;    
        int minPixelValue;
        int sizeImg;
        int sizeImg3;

        Bitmap bmp;
        //int imgWidth;
        //int imgHeight;


        List<byte> pix8;
        List<short> pix16;
        //List<int> pix16Signed;
        List<byte> pix24;

        byte[] imagePixels8;
        byte[] imagePixels16;
        byte[] imagePixels16Signed;
        byte[] imagePixels24;

        public unsafe Bitmap convertToBitmap16Bit(short[] argPixels16, bool argSignedImage, int argImageWidth, int argImageHeight,short windowmin=0,short windowmax=0)
        {

           

            //winWidth = 1000;
            if (windowmax == 0 && windowmin == 0)
            {
                minPixelValue = argPixels16.Min();
                maxPixelValue = argPixels16.Max();
            }
            else
            {
                minPixelValue = windowmin;
                maxPixelValue = windowmax;
            }

            if (maxPixelValue == minPixelValue)
                maxPixelValue = minPixelValue + 1;

            winMin = minPixelValue;
            winMax = maxPixelValue;

            sizeImg = argImageWidth * argImageHeight;
            sizeImg3 = sizeImg * 3;
            double sizeImg3By4 = sizeImg3 / 4.0;


           // pix16 = argPixels16;
            imagePixels16 = new byte[sizeImg3];



            //ComputeLookUpTable16();
            //int range = winMax - winMin;
            //if (range < 1) range = 1;
            //double factor = 255.0 / range;
            //int i;

            //for (i = 0; i < 65536; ++i)
            //{
            //    if (i <= winMin)
            //        lut16[i] = 0;
            //    else if (i >= winMax)
            //        lut16[i] = 255;
            //    else
            //    {
            //        lut16[i] = (byte)((i - winMin) * factor);
            //    }
            //}

            // To here compute lookup table16

            bmp = new Bitmap(argImageWidth, argImageHeight, System.Drawing.Imaging.PixelFormat.Format24bppRgb);


            //CreateImage16();
            BitmapData bmd = bmp.LockBits(new Rectangle(0, 0, argImageWidth, argImageHeight),
               System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);

            unsafe
            {
                int pixelSize = 3;
                int i2, j, j1, i1;
                byte b;

                for (i2 = 0; i2 < bmd.Height; ++i2)
                {
                    byte* row = (byte*)bmd.Scan0 + (i2 * bmd.Stride);
                    i1 = i2 * bmd.Width;

                    for (j = 0; j < bmd.Width; ++j)
                    {
                        b = (byte)((255 * (argPixels16[i2 * bmd.Width + j] - minPixelValue)) / (maxPixelValue - minPixelValue));
                        j1 = j * pixelSize;
                        row[j1] = b;            // Red
                        row[j1 + 1] = b;        // Green
                        row[j1 + 2] = b;        // Blue
                    }
                }
            }
            bmp.UnlockBits(bmd);
            //To here create image 16


            return bmp;

        }

        public Bitmap convertToBitmap16Bit(List<short> argPixels16, bool argSignedImage, int argImageWidth, int argImageHeight)
        {

            winMin = 0;
            winMax = 65535;

            //winWidth = 1000;
            minPixelValue = argPixels16.Min();
            maxPixelValue = argPixels16.Max();

            if (maxPixelValue == minPixelValue)
                maxPixelValue = minPixelValue + 1;

            winMin = minPixelValue;
            winMax = maxPixelValue;

            sizeImg = argImageWidth * argImageHeight;
            sizeImg3 = sizeImg * 3;
            double sizeImg3By4 = sizeImg3 / 4.0;


            //pix16 = argPixels16;
            imagePixels16 = new byte[sizeImg3];

            // To here compute lookup table16

            bmp = new Bitmap(argImageWidth, argImageHeight, System.Drawing.Imaging.PixelFormat.Format24bppRgb);


            //CreateImage16();
            BitmapData bmd = bmp.LockBits(new Rectangle(0, 0, argImageWidth, argImageHeight),
               System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);

            unsafe
            {
                int pixelSize = 3;
                int i2, j, j1, i1;
                byte b;

                for (i2 = 0; i2 < bmd.Height; ++i2)
                {
                    byte* row = (byte*)bmd.Scan0 + (i2 * bmd.Stride);
                    i1 = i2 * bmd.Width;

                    for (j = 0; j < bmd.Width; ++j)
                    {
                      //  b = lut16[pix16[i2 * bmd.Width + j]];
                        b = (byte)((255 * (argPixels16[i2 * bmd.Width + j] - minPixelValue)) / (maxPixelValue - minPixelValue));
                        j1 = j * pixelSize;
                        row[j1] = b;            // Red
                        row[j1 + 1] = b;        // Green
                        row[j1 + 2] = b;        // Blue
                    }
                }
            }
            bmp.UnlockBits(bmd);
            //To here create image 16


            return bmp;

        }

        public Bitmap convertToBitmap16Bit(short[,] argPixels16, int argWindowCenter, int argWindowWidth)
        {

            winMin = argWindowCenter - argWindowWidth;
            winMax = argWindowCenter + argWindowWidth;


            minPixelValue = short.MaxValue;
            maxPixelValue = short.MinValue;

            for (int i = 0; i < argPixels16.GetLength(0); i++)
                for (int j = 0; j < argPixels16.GetLength(1); j++)
                {
                    if (argPixels16[ i, j] < minPixelValue)
                        minPixelValue = argPixels16[ i, j];
                    if (argPixels16[ i, j] > maxPixelValue)
                        maxPixelValue = argPixels16[ i, j];
                }


            if (maxPixelValue == minPixelValue)
                maxPixelValue = minPixelValue + 1;

            if (winMin > minPixelValue)
                winMin = minPixelValue;

            if (winMax < maxPixelValue)
                winMax = maxPixelValue;

            sizeImg = argPixels16.GetLength(0) * argPixels16.GetLength(1);
            sizeImg3 = sizeImg * 3;
            double sizeImg3By4 = sizeImg3 / 4.0;


            //pix16 = argPixels16;
            imagePixels16 = new byte[sizeImg3];

            bmp = new Bitmap(argPixels16.GetLength(0), argPixels16.GetLength(1), System.Drawing.Imaging.PixelFormat.Format24bppRgb);


            //CreateImage16();
            BitmapData bmd = bmp.LockBits(new Rectangle(0, 0, argPixels16.GetLength(0), argPixels16.GetLength(1)),
               System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);

            unsafe
            {
                int pixelSize = 3;
                int i2, j, j1;
                byte b;

                for (i2 = 0; i2 < bmd.Height; ++i2)
                {
                    byte* row = (byte*)bmd.Scan0 + (i2 * bmd.Stride);

                    for (j = 0; j < bmd.Width; ++j)
                    {
                        if (argPixels16[j, i2] < winMin)
                            b = 0;
                        else if
                           (argPixels16[ j, i2] > winMax)
                            b = 0;
                        else
                            b = (byte)((255 * (argPixels16[ j, i2] - winMin)) / (winMax - winMin));
                        j1 = j * pixelSize;
                        row[j1] = b;            // Red
                        row[j1 + 1] = b;        // Green
                        row[j1 + 2] = b;        // Blue
                    }
                }
            }
            bmp.UnlockBits(bmd);
            //To here create image 16
            return bmp;

        }

        public Bitmap convertToBitmap16Bit(short[,] argPixels16)
        {

            minPixelValue = short.MaxValue;
            maxPixelValue = short.MinValue;

            for (int i = 0; i < argPixels16.GetLength(0); i++)
                for (int j = 0; j < argPixels16.GetLength(1); j++)
                {
                    if (argPixels16[i, j] < minPixelValue)
                        minPixelValue = argPixels16[i, j];
                    if (argPixels16[i, j] > maxPixelValue)
                        maxPixelValue = argPixels16[i, j];
                }


            if (maxPixelValue == minPixelValue)
                maxPixelValue = minPixelValue + 1;

         //   if (winMin < minPixelValue)
                winMin = minPixelValue;

           // if (winMax > maxPixelValue)
                winMax = maxPixelValue;

            sizeImg = argPixels16.GetLength(0) * argPixels16.GetLength(1);
            sizeImg3 = sizeImg * 3;
            double sizeImg3By4 = sizeImg3 / 4.0;


            //pix16 = argPixels16;
            imagePixels16 = new byte[sizeImg3];

            bmp = new Bitmap(argPixels16.GetLength(0), argPixels16.GetLength(1), System.Drawing.Imaging.PixelFormat.Format24bppRgb);


            //CreateImage16();
            BitmapData bmd = bmp.LockBits(new Rectangle(0, 0, argPixels16.GetLength(0), argPixels16.GetLength(1)),
               System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);

            unsafe
            {
                int pixelSize = 3;
                int i2, j, j1;
                byte b;

                for (i2 = 0; i2 < bmd.Height; ++i2)
                {
                    byte* row = (byte*)bmd.Scan0 + (i2 * bmd.Stride);

                    for (j = 0; j < bmd.Width; ++j)
                    {
                        if (argPixels16[j, i2] < winMin)
                            b = 0;
                        else if
                           (argPixels16[j, i2] > winMax)
                            b = 0;
                        else
                            
                            b = (byte)((255 * (argPixels16[j, i2] - winMin)) / (winMax - winMin));
            
                        short ss = argPixels16[j, i2];
                        //if (ss > 0 && ss < 109)
                        //    ss = ss;
                        j1 = j * pixelSize;

                        //if (b > 0)
                        //    b = b;
                        row[j1] = b;            // Red
                        row[j1 + 1] = b;        // Green
                        row[j1 + 2] = b;        // Blue
                    }
                }
            }
            bmp.UnlockBits(bmd);
            //To here create image 16
            return bmp;

        }
        int n = 0;
        public Bitmap convertToBitmap8Bit(byte[,] argPixels8)
        {

            minPixelValue = short.MaxValue;
            maxPixelValue = short.MinValue;

            for (int i = 0; i < argPixels8.GetLength(0); i++)
                for (int j = 0; j < argPixels8.GetLength(1); j++)
                {
                    if (argPixels8[i, j] < minPixelValue)
                        minPixelValue = argPixels8[i, j];
                    if (argPixels8[i, j] > maxPixelValue)
                        maxPixelValue = argPixels8[i, j];
                }


            if (maxPixelValue == minPixelValue)
                maxPixelValue = minPixelValue + 1;

            //   if (winMin < minPixelValue)
            winMin = minPixelValue;

            // if (winMax > maxPixelValue)
            winMax = maxPixelValue;

            sizeImg = argPixels8.GetLength(0) * argPixels8.GetLength(1);
            sizeImg3 = sizeImg * 3;
            double sizeImg3By4 = sizeImg3 / 4.0;


            //pix16 = argPixels16;
            imagePixels16 = new byte[sizeImg3];

            bmp = new Bitmap(argPixels8.GetLength(0), argPixels8.GetLength(1), System.Drawing.Imaging.PixelFormat.Format24bppRgb);


            //CreateImage16();
            BitmapData bmd = bmp.LockBits(new Rectangle(0, 0, argPixels8.GetLength(0), argPixels8.GetLength(1)),
               System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);

            unsafe
            {
                int pixelSize = 3;
                int i2, j, j1;
                byte b;

                for (i2 = 0; i2 < bmd.Height; ++i2)
                {
                    byte* row = (byte*)bmd.Scan0 + (i2 * bmd.Stride);

                    for (j = 0; j < bmd.Width; ++j)
                    {
                        if (argPixels8[j, i2] < winMin)
                            b = 0;
                        else if
                           (argPixels8[j, i2] > winMax)
                            b = 0;
                        else
                            b = (byte)((255 * (argPixels8[j, i2] - winMin)) / (winMax - winMin));
                        j1 = j * pixelSize;
                        row[j1] = b;            // Red
                        row[j1 + 1] = b;        // Green
                        row[j1 + 2] = b;        // Blue
                    }
                }
            }
            bmp.UnlockBits(bmd);
            //To here create image 16
            return bmp;

        }

        public Bitmap convertToBitmap16Bit(short[,,] argPixels16,int sliceNumber,int argWindowCenter,int argWindowWidth)
        {

                winMin = argWindowCenter - argWindowWidth;
                winMax = argWindowCenter + argWindowWidth;


                minPixelValue = short.MaxValue;
                maxPixelValue = short.MinValue;

                for (int i = 0; i < argPixels16.GetLength(1); i++)
                    for (int j = 0; j < argPixels16.GetLength(2); j++)
                    {
                        if (argPixels16[sliceNumber, i, j] < minPixelValue)
                            minPixelValue = argPixels16[sliceNumber, i, j];
                        if (argPixels16[sliceNumber, i, j] > maxPixelValue)
                            maxPixelValue = argPixels16[sliceNumber, i, j];
                    }


                if (maxPixelValue == minPixelValue)
                    maxPixelValue = minPixelValue + 1;

                if (winMin > minPixelValue)
                    winMin = minPixelValue;

                if (winMax < maxPixelValue)
                    winMax = maxPixelValue;

            sizeImg = argPixels16.GetLength(1) * argPixels16.GetLength(2);
            sizeImg3 = sizeImg * 3;
            double sizeImg3By4 = sizeImg3 / 4.0;


            //pix16 = argPixels16;
            imagePixels16 = new byte[sizeImg3];

            bmp = new Bitmap(argPixels16.GetLength(1), argPixels16.GetLength(2), System.Drawing.Imaging.PixelFormat.Format24bppRgb);


            //CreateImage16();
            BitmapData bmd = bmp.LockBits(new Rectangle(0, 0, argPixels16.GetLength(1), argPixels16.GetLength(2)),
               System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);

            unsafe
            {
                int pixelSize = 3;
                int i2, j, j1;
                byte b;

                for (i2 = 0; i2 < bmd.Height; ++i2)
                {
                    byte* row = (byte*)bmd.Scan0 + (i2 * bmd.Stride);

                    for (j = 0; j < bmd.Width; ++j)
                    {
                        if (argPixels16[sliceNumber, j, i2] < winMin)
                            b = 0;
                        else if
                           (argPixels16[sliceNumber, j, i2] > winMax)
                             b=0;
                        else
                            b = (byte)((255 * (argPixels16[sliceNumber, j, i2] - winMin)) / (winMax - winMin));
                        j1 = j * pixelSize;
                        row[j1] = b;            // Red
                        row[j1 + 1] = b;        // Green
                        row[j1 + 2] = b;        // Blue
                    }
                }
            }
            bmp.UnlockBits(bmd);
            //To here create image 16
            return bmp;

        }

        public Bitmap convertToBitmap16Bit(short[,,] argPixels16,int sliceNumber)
        {

                minPixelValue = short.MaxValue;
                maxPixelValue = short.MinValue;

                for (int i = 0; i < argPixels16.GetLength(1); i++)
                    for (int j = 0; j < argPixels16.GetLength(2); j++)
                    {
                        if (argPixels16[sliceNumber,i, j] < minPixelValue)
                            minPixelValue = argPixels16[sliceNumber,i, j];
                        if (argPixels16[sliceNumber,i, j] > maxPixelValue)
                            maxPixelValue = argPixels16[sliceNumber,i, j];
                    }


                if (maxPixelValue == minPixelValue)
                    maxPixelValue = minPixelValue + 1;

                winMin = minPixelValue;
                winMax = maxPixelValue;
            

            sizeImg = argPixels16.GetLength(1) * argPixels16.GetLength(2);
            sizeImg3 = sizeImg * 3;
            double sizeImg3By4 = sizeImg3 / 4.0;


            //pix16 = argPixels16;
            imagePixels16 = new byte[sizeImg3];

            bmp = new Bitmap(argPixels16.GetLength(1), argPixels16.GetLength(2), System.Drawing.Imaging.PixelFormat.Format24bppRgb);


            //CreateImage16();
            BitmapData bmd = bmp.LockBits(new Rectangle(0, 0, argPixels16.GetLength(1), argPixels16.GetLength(2)),
               System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);

            unsafe
            {
                int pixelSize = 3;
                int i2, j, j1;
                byte b;

                for (i2 = 0; i2 < bmd.Height; ++i2)
                {
                    byte* row = (byte*)bmd.Scan0 + (i2 * bmd.Stride);

                    for (j = 0; j < bmd.Width; ++j)
                    {
                        b = (byte)((255 * (argPixels16[sliceNumber,j, i2] - minPixelValue)) / (maxPixelValue - minPixelValue));
                        j1 = j * pixelSize;
                        row[j1] = b;            // Red
                        row[j1 + 1] = b;        // Green
                        row[j1 + 2] = b;        // Blue
                    }
                }
            }
            bmp.UnlockBits(bmd);
            //To here create image 16
            return bmp;

        }


        public Bitmap convertToBitmap8Bit(List<byte> argPixels8, bool argSignedImage, int argImageWidth, int argImageHeight)
        {
            winMin = 0;
            winMax = 255;


            minPixelValue = argPixels8.Min();
            maxPixelValue = argPixels8.Max();

            if (maxPixelValue == minPixelValue)
                maxPixelValue = minPixelValue + 1;


            winMin = minPixelValue;
            winMax = maxPixelValue;


            //imgWidth = argImageWidth;
            //imgHeight = argImageHeight;

            sizeImg = argImageWidth * argImageHeight;
            sizeImg3 = sizeImg * 3;

            //pix8 = argPixels8;
            //imagePixels8 = new byte[sizeImg3];

            // ResetValues();

            bmp = new Bitmap(argImageWidth, argImageHeight, System.Drawing.Imaging.PixelFormat.Format24bppRgb);


            //CreateImage8();
            BitmapData bmd = bmp.LockBits(new Rectangle(0, 0, argImageWidth, argImageHeight),
            System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);

            unsafe
            {
                int pixelSize = 3;
                int i3, j, j1, i1;
                byte b;

                for (i3 = 0; i3 < bmd.Height; ++i3)
                {
                    byte* row = (byte*)bmd.Scan0 + (i3 * bmd.Stride);
                    i1 = i3 * bmd.Width;

                    for (j = 0; j < bmd.Width; ++j)
                    {
                        b = (byte)((255 * (argPixels8[i3 * bmd.Width + j] - minPixelValue)) / (maxPixelValue - minPixelValue));
                        j1 = j * pixelSize;
                        row[j1] = b;            // Red
                        row[j1 + 1] = b;        // Green
                        row[j1 + 2] = b;        // Blue
                    }
                }
            }
            bmp.UnlockBits(bmd);
            return bmp;

        }

        public Bitmap convertToBitmap8Bit(byte[,,] argPixels8,int sliceNumber)
        {
            minPixelValue = byte.MaxValue;
            maxPixelValue = byte.MinValue;

            for (int i = 0; i < argPixels8.GetLength(1); i++)
                for (int j = 0; j < argPixels8.GetLength(2); j++)
                {
                    if (argPixels8[sliceNumber, i, j] < minPixelValue)
                        minPixelValue = argPixels8[sliceNumber, i, j];
                    if (argPixels8[sliceNumber, i, j] > maxPixelValue)
                        maxPixelValue = argPixels8[sliceNumber, i, j];
                }

            if (maxPixelValue == minPixelValue)
                maxPixelValue = minPixelValue + 1;

            winMin = minPixelValue;
            winMax = maxPixelValue;

            sizeImg = argPixels8.GetLength(1) * argPixels8.GetLength(2);
            sizeImg3 = sizeImg * 3;

            bmp = new Bitmap(argPixels8.GetLength(1), argPixels8.GetLength(2), System.Drawing.Imaging.PixelFormat.Format24bppRgb);


            //CreateImage8();
            BitmapData bmd = bmp.LockBits(new Rectangle(0, 0, argPixels8.GetLength(1), argPixels8.GetLength(2)),
            System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);

            unsafe
            {
                int pixelSize = 3;
                int i3, j, j1, i1;
                byte b;

                for (i3 = 0; i3 < bmd.Height; ++i3)
                {
                    byte* row = (byte*)bmd.Scan0 + (i3 * bmd.Stride);
                    i1 = i3 * bmd.Width;

                    for (j = 0; j < bmd.Width; ++j)
                    {
                        b = (byte)((255 * (argPixels8[sliceNumber,j,i3] - minPixelValue)) / (maxPixelValue - minPixelValue));
                        j1 = j * pixelSize;
                        row[j1] = b;            // Red
                        row[j1 + 1] = b;        // Green
                        row[j1 + 2] = b;        // Blue
                    }
                }
            }
            bmp.UnlockBits(bmd);
            return bmp;

        }

        public Bitmap convertToBitmap8Bit(byte[, ,] argPixels8, int sliceNumber, int argWindowCenter, int argWindowWidth)
        {
            winMin = argWindowCenter - argWindowWidth;
            winMax = argWindowCenter + argWindowWidth;


            minPixelValue = short.MaxValue;
            maxPixelValue = short.MinValue;

            for (int i = 0; i < argPixels8.GetLength(1); i++)
                for (int j = 0; j < argPixels8.GetLength(2); j++)
                {
                    if (argPixels8[sliceNumber, i, j] < minPixelValue)
                        minPixelValue = argPixels8[sliceNumber, i, j];
                    if (argPixels8[sliceNumber, i, j] > maxPixelValue)
                        maxPixelValue = argPixels8[sliceNumber, i, j];
                }


            if (maxPixelValue == minPixelValue)
                maxPixelValue = minPixelValue + 1;

            if (winMin < minPixelValue)
                winMin = minPixelValue;

            if (winMax > maxPixelValue)
                winMax = maxPixelValue;

            sizeImg = argPixels8.GetLength(1) * argPixels8.GetLength(2);
            sizeImg3 = sizeImg * 3;

            bmp = new Bitmap(argPixels8.GetLength(1), argPixels8.GetLength(2), System.Drawing.Imaging.PixelFormat.Format24bppRgb);


            //CreateImage8();
            BitmapData bmd = bmp.LockBits(new Rectangle(0, 0, argPixels8.GetLength(1), argPixels8.GetLength(2)),
            System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);

            unsafe
            {
                int pixelSize = 3;
                int i3, j, j1, i1;
                byte b;

                for (i3 = 0; i3 < bmd.Height; ++i3)
                {
                    byte* row = (byte*)bmd.Scan0 + (i3 * bmd.Stride);
                    i1 = i3 * bmd.Width;

                    for (j = 0; j < bmd.Width; ++j)
                    {
                        b = (byte)((255 * (argPixels8[sliceNumber, j, i3] - winMin)) / (winMax - winMin));
                        j1 = j * pixelSize;
                        row[j1] = b;            // Red
                        row[j1 + 1] = b;        // Green
                        row[j1 + 2] = b;        // Blue
                    }
                }
            }
            bmp.UnlockBits(bmd);
            return bmp;

        }

        public Bitmap convertToBitmap8Bit(byte[,] argPixels8, int argWindowCenter, int argWindowWidth)
        {
            winMin = argWindowCenter - argWindowWidth;
            winMax = argWindowCenter + argWindowWidth;


            minPixelValue = short.MaxValue;
            maxPixelValue = short.MinValue;

            for (int i = 0; i < argPixels8.GetLength(0); i++)
                for (int j = 0; j < argPixels8.GetLength(1); j++)
                {
                    if (argPixels8[ i, j] < minPixelValue)
                        minPixelValue = argPixels8[ i, j];
                    if (argPixels8[ i, j] > maxPixelValue)
                        maxPixelValue = argPixels8[ i, j];
                }


            if (maxPixelValue == minPixelValue)
                maxPixelValue = minPixelValue + 1;

            if (winMin < minPixelValue)
                winMin = minPixelValue;

            if (winMax > maxPixelValue)
                winMax = maxPixelValue;

            sizeImg = argPixels8.GetLength(0) * argPixels8.GetLength(1);
            sizeImg3 = sizeImg * 3;

            bmp = new Bitmap(argPixels8.GetLength(0), argPixels8.GetLength(1), System.Drawing.Imaging.PixelFormat.Format24bppRgb);


            //CreateImage8();
            BitmapData bmd = bmp.LockBits(new Rectangle(0, 0, argPixels8.GetLength(0), argPixels8.GetLength(1)),
            System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);

            unsafe
            {
                int pixelSize = 3;
                int i3, j, j1, i1;
                byte b;

                for (i3 = 0; i3 < bmd.Height; ++i3)
                {
                    byte* row = (byte*)bmd.Scan0 + (i3 * bmd.Stride);
                    i1 = i3 * bmd.Width;

                    for (j = 0; j < bmd.Width; ++j)
                    {
                        b = (byte)((255 * (argPixels8[ j, i3] - winMin)) / (winMax - winMin));
                        j1 = j * pixelSize;
                        row[j1] = b;            // Red
                        row[j1 + 1] = b;        // Green
                        row[j1 + 2] = b;        // Blue
                    }
                }
            }
            bmp.UnlockBits(bmd);
            return bmp;

        }

        public Bitmap convertToBitmap8Bit(byte[] argPixels8, bool argSignedImage, int argImageWidth, int argImageHeight)
        {
            winMin = 0;
            winMax = 255;


            minPixelValue = argPixels8.Min();
            maxPixelValue = argPixels8.Max();

            if (maxPixelValue == minPixelValue)
                maxPixelValue = minPixelValue + 1;

            winMin = minPixelValue;
            winMax = maxPixelValue;

            sizeImg = argImageWidth * argImageHeight;
            sizeImg3 = sizeImg * 3;

            bmp = new Bitmap(argImageWidth, argImageHeight, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            if (winMax == 0)
                winMax = 255;

            //int range = winMax - winMin;
            //if (range < 1) range = 1;
            //double factor = 255.0 / range;

            //for (int i = 0; i < 256; ++i)
            //{
            //    if (i <= winMin)
            //        lut8[i] = 0;
            //    else if (i >= winMax)
            //        lut8[i] = 255;
            //    else
            //    {
            //        lut8[i] = (byte)((i - winMin) * factor);
            //    }
            //}

            BitmapData bmd = bmp.LockBits(new Rectangle(0, 0, argImageWidth, argImageHeight),
            System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);

            unsafe
            {
                int pixelSize = 3;
                int i3, j, j1, i1;
                byte b;

                for (i3 = 0; i3 < bmd.Height; ++i3)
                {
                    byte* row = (byte*)bmd.Scan0 + (i3 * bmd.Stride);
                    i1 = i3 * bmd.Width;

                    for (j = 0; j < bmd.Width; ++j)
                    {
                        b = (byte)((255 * (argPixels8[i3 * bmd.Width + j] - minPixelValue)) / (maxPixelValue - minPixelValue));
                        j1 = j * pixelSize;
                        row[j1] = b;            // Red
                        row[j1 + 1] = b;        // Green
                        row[j1 + 2] = b;        // Blue
                    }
                }
            }
            bmp.UnlockBits(bmd);
            return bmp;

        }

        public Bitmap convertToBitmap24Bit(List<byte> argPixels24, bool argSignedImage, int argImageWidth, int argImageHeight)
        {

            winMin = 0;
            winMax = 255;


            minPixelValue = argPixels24.Min();
            maxPixelValue = argPixels24.Max();

            if (maxPixelValue == minPixelValue)
                maxPixelValue = minPixelValue + 1;

            winMin = minPixelValue;
            winMax = maxPixelValue;

            //imgWidth = argImageWidth;
            //imgHeight = argImageHeight;

            //winWidth = Convert.ToInt32(winWidth);
            //winCentre = Convert.ToInt32(winCentre);

            //winMax = Convert.ToInt32(winCentre + 0.5 * winWidth);
            //winMin = winMax - winWidth;



            sizeImg = argImageWidth * argImageHeight;
            sizeImg3 = sizeImg * 3;

          //  pix24 = argPixels24;
            imagePixels24 = new byte[sizeImg3];



            bmp = new Bitmap(argImageWidth, argImageHeight, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            //ComputeLookUpTable8();

            if (winMax == 0)
                winMax = 255;

            //int range = winMax - winMin;
            //if (range < 1) range = 1;
            //double factor = 255.0 / range;

            //for (int i = 0; i < 256; ++i)
            //{
            //    if (i <= winMin)
            //        lut8[i] = 0;
            //    else if (i >= winMax)
            //        lut8[i] = 255;
            //    else
            //    {
            //        lut8[i] = (byte)((i - winMin) * factor);
            //    }
            //}


            //CreateImage24();
            int numBytes = argImageWidth * argImageHeight * 3;
            int j;
            int i2, i1;

            BitmapData bmd = bmp.LockBits(new Rectangle(0, 0, bmp.Width,
                bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);

            int width3 = bmd.Width * 3;

            unsafe
            {
                for (i2 = 0; i2 < bmd.Height; ++i2)
                {
                    byte* row = (byte*)bmd.Scan0 + (i2 * bmd.Stride);
                    i1 = i2 * bmd.Width * 3;

                    for (j = 0; j < width3; j += 3)
                    {
                        // Windows uses little-endian, so the RGB data is 
                        //  actually stored as BGR
                        //row[j + 2] = lut8[pix24[i1 + j]];     // Blue
                        //row[j + 1] = lut8[pix24[i1 + j + 1]]; // Green
                        //row[j] = lut8[pix24[i1 + j + 2]];     // Red

                       // b = (byte)((255 * (pix16[i2 * bmd.Width + j] - minPixelValue)) / (maxPixelValue - minPixelValue));
                       // j1 = j * pixelSize;
                        row[j + 2] = (byte)((255 * (argPixels24[i1 * bmd.Width + j] - minPixelValue)) / (maxPixelValue - minPixelValue)); ;            // Red
                        row[j + 1] = (byte)((255 * (argPixels24[i1 * bmd.Width + j + 1] - minPixelValue)) / (maxPixelValue - minPixelValue)); ;        // Green
                        row[j] = (byte)((255 * (argPixels24[i1 * bmd.Width + j + 2] - minPixelValue)) / (maxPixelValue - minPixelValue)); ;        // Blue
                    }
                }
            }
            bmp.UnlockBits(bmd);
            return bmp;




        }

        public Bitmap convertToBitmap24Bit(byte[,,] argPixels24, int sliceNumber)
        {
            minPixelValue = byte.MaxValue;
            maxPixelValue = byte.MinValue;

            for (int i = 0; i < argPixels24.GetLength(1); i++)
                for (int j = 0; j < argPixels24.GetLength(2); j++)
                {
                    if (argPixels24[sliceNumber, i, j] < minPixelValue)
                        minPixelValue = argPixels24[sliceNumber, i, j];
                    if (argPixels24[sliceNumber, i, j] > maxPixelValue)
                        maxPixelValue = argPixels24[sliceNumber, i, j];
                }

            if (maxPixelValue == minPixelValue)
                maxPixelValue = minPixelValue + 1;

            winMin = minPixelValue;
            winMax = maxPixelValue;

            sizeImg = argPixels24.GetLength(1) * argPixels24.GetLength(2);
            sizeImg3 = sizeImg * 3;

            //  pix24 = argPixels24;
            imagePixels24 = new byte[sizeImg3];



            bmp = new Bitmap(argPixels24.GetLength(1), argPixels24.GetLength(2), System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            if (winMax == 0)
                winMax = 255;

            int numBytes = argPixels24.GetLength(1) * argPixels24.GetLength(2) * 3;
            int i2, i1;

            BitmapData bmd = bmp.LockBits(new Rectangle(0, 0, bmp.Width,
                bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);

            int width3 = bmd.Width * 3;

            unsafe
            {
                for (i2 = 0; i2 < bmd.Height; ++i2)
                {
                    byte* row = (byte*)bmd.Scan0 + (i2 * bmd.Stride);
                    i1 = i2 * bmd.Width * 3;

                    for (int j = 0; j < width3; j += 3)
                    {
                        row[j + 2] = (byte)((255 * (argPixels24[sliceNumber,j,i1] - minPixelValue)) / (maxPixelValue - minPixelValue)); ;            // Red
                        row[j + 1] = (byte)((255 * (argPixels24[sliceNumber, j+1, i1] - minPixelValue)) / (maxPixelValue - minPixelValue)); ;        // Green
                        row[j] = (byte)((255 * (argPixels24[sliceNumber, j+3, i1] - minPixelValue)) / (maxPixelValue - minPixelValue)); ;        // Blue
                    }
                }
            }
            bmp.UnlockBits(bmd);
            return bmp;
        }



        public Bitmap convertToBitmap24Bit(byte[, ,] argPixels24, int sliceNumber, int argWindowCenter, int argWindowWidth)
        {
            winMin = argWindowCenter - argWindowWidth;
            winMax = argWindowCenter + argWindowWidth;


            minPixelValue = short.MaxValue;
            maxPixelValue = short.MinValue;

            for (int i = 0; i < argPixels24.GetLength(1); i++)
                for (int j = 0; j < argPixels24.GetLength(2); j++)
                {
                    if (argPixels24[sliceNumber, i, j] < minPixelValue)
                        minPixelValue = argPixels24[sliceNumber, i, j];
                    if (argPixels24[sliceNumber, i, j] > maxPixelValue)
                        maxPixelValue = argPixels24[sliceNumber, i, j];
                }


            if (maxPixelValue == minPixelValue)
                maxPixelValue = minPixelValue + 1;

            if (winMin < minPixelValue)
                winMin = minPixelValue;

            if (winMax > maxPixelValue)
                winMax = maxPixelValue;

            sizeImg = argPixels24.GetLength(1) * argPixels24.GetLength(2);
            sizeImg3 = sizeImg * 3;

            //  pix24 = argPixels24;
            imagePixels24 = new byte[sizeImg3];



            bmp = new Bitmap(argPixels24.GetLength(1), argPixels24.GetLength(2), System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            if (winMax == 0)
                winMax = 255;

            int numBytes = argPixels24.GetLength(1) * argPixels24.GetLength(2) * 3;
            int i2, i1;

            BitmapData bmd = bmp.LockBits(new Rectangle(0, 0, bmp.Width,
                bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);

            int width3 = bmd.Width * 3;

            unsafe
            {
                for (i2 = 0; i2 < bmd.Height; ++i2)
                {
                    byte* row = (byte*)bmd.Scan0 + (i2 * bmd.Stride);
                    i1 = i2 * bmd.Width * 3;

                    for (int j = 0; j < width3; j += 3)
                    {
                        row[j + 2] = (byte)((255 * (argPixels24[sliceNumber, j, i1] - winMin)) / (winMax - winMin)); ;            // Red
                        row[j + 1] = (byte)((255 * (argPixels24[sliceNumber, j + 1, i1] - winMin)) / (winMax - winMin)); ;        // Green
                        row[j] = (byte)((255 * (argPixels24[sliceNumber, j + 3, i1] - winMin)) / (winMax - winMin)); ;        // Blue
                    }
                }
            }
            bmp.UnlockBits(bmd);
            return bmp;
        }

      

        public Bitmap convertToBitmap24Bit(byte[,] argPixels24, int argWindowCenter, int argWindowWidth)
        {
            winMin = argWindowCenter - argWindowWidth;
            winMax = argWindowCenter + argWindowWidth;


            minPixelValue = short.MaxValue;
            maxPixelValue = short.MinValue;

            for (int i = 0; i < argPixels24.GetLength(1); i++)
                for (int j = 0; j < argPixels24.GetLength(2); j++)
                {
                    if (argPixels24[i, j] < minPixelValue)
                        minPixelValue = argPixels24[i, j];
                    if (argPixels24[i, j] > maxPixelValue)
                        maxPixelValue = argPixels24[i, j];
                }


            if (maxPixelValue == minPixelValue)
                maxPixelValue = minPixelValue + 1;

            if (winMin < minPixelValue)
                winMin = minPixelValue;

            if (winMax > maxPixelValue)
                winMax = maxPixelValue;

            sizeImg = argPixels24.GetLength(1) * argPixels24.GetLength(2);
            sizeImg3 = sizeImg * 3;

            //  pix24 = argPixels24;
            imagePixels24 = new byte[sizeImg3];



            bmp = new Bitmap(argPixels24.GetLength(1), argPixels24.GetLength(2), System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            if (winMax == 0)
                winMax = 255;

            int numBytes = argPixels24.GetLength(1) * argPixels24.GetLength(2) * 3;
            int i2, i1;

            BitmapData bmd = bmp.LockBits(new Rectangle(0, 0, bmp.Width,
                bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);

            int width3 = bmd.Width * 3;

            unsafe
            {
                for (i2 = 0; i2 < bmd.Height; ++i2)
                {
                    byte* row = (byte*)bmd.Scan0 + (i2 * bmd.Stride);
                    i1 = i2 * bmd.Width * 3;

                    for (int j = 0; j < width3; j += 3)
                    {
                        row[j + 2] = (byte)((255 * (argPixels24[j, i1] - winMin)) / (winMax - winMin)); ;            // Red
                        row[j + 1] = (byte)((255 * (argPixels24[j + 1, i1] - winMin)) / (winMax - winMin)); ;        // Green
                        row[j] = (byte)((255 * (argPixels24[j + 3, i1] - winMin)) / (winMax - winMin)); ;        // Blue
                    }
                }
            }
            bmp.UnlockBits(bmd);
            return bmp;
        }

        public Bitmap convertToBitmap24Bit(byte[,] argPixels24)
        {
 
            minPixelValue = short.MaxValue;
            maxPixelValue = short.MinValue;

            for (int i = 0; i < argPixels24.GetLength(1); i++)
                for (int j = 0; j < argPixels24.GetLength(2); j++)
                {
                    if (argPixels24[i, j] < minPixelValue)
                        minPixelValue = argPixels24[i, j];
                    if (argPixels24[i, j] > maxPixelValue)
                        maxPixelValue = argPixels24[i, j];
                }


            if (maxPixelValue == minPixelValue)
                maxPixelValue = minPixelValue + 1;

           // if (winMin < minPixelValue)
                winMin = minPixelValue;

            //if (winMax > maxPixelValue)
                winMax = maxPixelValue;

            sizeImg = argPixels24.GetLength(1) * argPixels24.GetLength(2);
            sizeImg3 = sizeImg * 3;

            //  pix24 = argPixels24;
            imagePixels24 = new byte[sizeImg3];



            bmp = new Bitmap(argPixels24.GetLength(1), argPixels24.GetLength(2), System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            if (winMax == 0)
                winMax = 255;

            int numBytes = argPixels24.GetLength(1) * argPixels24.GetLength(2) * 3;
            int i2, i1;

            BitmapData bmd = bmp.LockBits(new Rectangle(0, 0, bmp.Width,
                bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);

            int width3 = bmd.Width * 3;

            unsafe
            {
                for (i2 = 0; i2 < bmd.Height; ++i2)
                {
                    byte* row = (byte*)bmd.Scan0 + (i2 * bmd.Stride);
                    i1 = i2 * bmd.Width * 3;

                    for (int j = 0; j < width3; j += 3)
                    {
                        row[j + 2] = (byte)((255 * (argPixels24[j, i1] - winMin)) / (winMax - winMin)); ;            // Red
                        row[j + 1] = (byte)((255 * (argPixels24[j + 1, i1] - winMin)) / (winMax - winMin)); ;        // Green
                        row[j] = (byte)((255 * (argPixels24[j + 3, i1] - winMin)) / (winMax - winMin)); ;        // Blue
                    }
                }
            }
            bmp.UnlockBits(bmd);
            return bmp;
        }

     


        public Bitmap convertToBitmap24Bit(byte[] argPixels24, bool argSignedImage, int argImageWidth, int argImageHeight)
        {

            winMin = 0;
            winMax = 255;


            minPixelValue = argPixels24.Min();
            maxPixelValue = argPixels24.Max();

            if (maxPixelValue == minPixelValue)
                maxPixelValue = minPixelValue + 1;

            winMin = minPixelValue;
            winMax = maxPixelValue;

            sizeImg = argImageWidth * argImageHeight;
            sizeImg3 = sizeImg * 3;

            //pix24 = argPixels24;
            imagePixels24 = new byte[sizeImg3];



            bmp = new Bitmap(argImageWidth, argImageHeight, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            //ComputeLookUpTable8();

            if (winMax == 0)
                winMax = 255;

            //int range = winMax - winMin;
            //if (range < 1) range = 1;
            //double factor = 255.0 / range;

            //for (int i = 0; i < 256; ++i)
            //{
            //    if (i <= winMin)
            //        lut8[i] = 0;
            //    else if (i >= winMax)
            //        lut8[i] = 255;
            //    else
            //    {
            //        lut8[i] = (byte)((i - winMin) * factor);
            //    }
            //}


            //CreateImage24();
            int numBytes = argImageWidth * argImageHeight * 3;
            int j;
            int i2, i1;

            BitmapData bmd = bmp.LockBits(new Rectangle(0, 0, bmp.Width,
                bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);

            int width3 = bmd.Width * 3;

            unsafe
            {
                for (i2 = 0; i2 < bmd.Height; ++i2)
                {
                    byte* row = (byte*)bmd.Scan0 + (i2 * bmd.Stride);
                    i1 = i2 * bmd.Width * 3;

                    for (j = 0; j < width3; j += 3)
                    {
                        // Windows uses little-endian, so the RGB data is 
                        //  actually stored as BGR
                        //row[j + 2] = lut8[argPixels24[i1 + j]];     // Blue
                        //row[j + 1] = lut8[argPixels24[i1 + j + 1]]; // Green
                        //row[j] = lut8[argPixels24[i1 + j + 2]];     // Red

                        row[j + 2] = (byte)((255 * (argPixels24[i1 * bmd.Width + j] - minPixelValue)) / (maxPixelValue - minPixelValue)); ;            // Red
                        row[j + 1] = (byte)((255 * (argPixels24[i1 * bmd.Width + j + 1] - minPixelValue)) / (maxPixelValue - minPixelValue)); ;        // Green
                        row[j] = (byte)((255 * (argPixels24[i1 * bmd.Width + j + 2] - minPixelValue)) / (maxPixelValue - minPixelValue)); ;        // Blue
                    }
                }
            }
            bmp.UnlockBits(bmd);
            return bmp;

        }

    }
}
