using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomImageViewer.Base
{
    public class CommonUtils
    {

        public static T[, ,] ApplyFilterFunction<T>(T[, ,] pixes, Func<T, T> func)
        {
            var maxRowlength = pixes.GetLength(0);
            var maxColLength = pixes.GetLength(1);
            var maxDepthLength = pixes.GetLength(2);
            var result = new T[maxRowlength, maxColLength, maxDepthLength];

            for (int rowIndex = 0; rowIndex < maxRowlength; rowIndex++)
                for (int colIndex = 0; colIndex < maxColLength; colIndex++)
                    for (int depthIndex = 0; depthIndex < maxDepthLength; depthIndex++)
                        result[rowIndex, colIndex, depthIndex] = func(pixes[rowIndex, colIndex, depthIndex]);

            return result;
        }
        public static double[, ,] ApplyFilterFunction(double[, ,] pixes, Func<double, double> func)
        {
            var maxRowlength = pixes.GetLength(0);
            var maxColLength = pixes.GetLength(1);
            var maxDepthLength = pixes.GetLength(3);
            var result = new double[maxRowlength, maxColLength, maxDepthLength];

            for (int rowIndex = 0; rowIndex < maxRowlength; rowIndex++)
                for (int colIndex = 0; colIndex < maxColLength; colIndex++)
                    for (int DepthIndex = 0; DepthIndex < maxDepthLength; DepthIndex++)
                        result[rowIndex, colIndex, maxDepthLength] = func(pixes[rowIndex, colIndex, DepthIndex]);

            return result;
        }

        public static short[,] ApplyFilterFunction(short[,] pixes, Func<short, short> func)
        {
            var maxRowlength = pixes.GetLength(0);
            var maxColLength = pixes.GetLength(1);
            var result = new short[maxRowlength, maxColLength];

            for (int rowIndex = 0; rowIndex < maxRowlength; rowIndex++)
            {
                for (int colIndex = 0; colIndex < maxColLength; colIndex++)
                {
                    result[rowIndex, colIndex] = func(pixes[rowIndex, colIndex]);

                }
            }

            return result;
        }



        public static double ConvertToOneDimension(byte rate255)
        {
            if (rate255 > 255)
                return 1;
            if (rate255 < 0)
                return 0;

            return ((double)rate255 / (double)255);
        }
        public static byte ConvertTo255Dimension(double rateOne)
        {
            if (rateOne > 1)
                return 255;
            if (rateOne < 0)
                return 0;

            return (byte)Convert.ToInt32((rateOne * (double)255));
        }


    }
}
