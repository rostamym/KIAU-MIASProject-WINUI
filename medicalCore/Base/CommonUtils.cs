using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomImageViewer.Base
{
    public class CommonUtils
    {

        public static TK[,,] ApplyFilterFunction<T,TM, TK>(T[,,] pixes, TM[,,] mask, Func<T,TM, TK> func)
        {
            var maxRowlength = pixes.GetLength(0);
            var maxColLength = pixes.GetLength(1);
            var maxDepthLength = pixes.GetLength(2);
            var result = new TK[maxRowlength, maxColLength, maxDepthLength];

            for (int rowIndex = 0; rowIndex < maxRowlength; rowIndex++)
                for (int colIndex = 0; colIndex < maxColLength; colIndex++)
                    for (int depthIndex = 0; depthIndex < maxDepthLength; depthIndex++)
                        result[rowIndex, colIndex, depthIndex] = func(pixes[rowIndex, colIndex, depthIndex], mask[rowIndex, colIndex, depthIndex]);

            return result;
        }

        public static TK[, ,] ApplyFilterFunction<T,TK>(T[, ,] pixes, Func<T, TK> func)
        {
            var maxRowlength = pixes.GetLength(0);
            var maxColLength = pixes.GetLength(1);
            var maxDepthLength = pixes.GetLength(2);
            var result = new TK[maxRowlength, maxColLength, maxDepthLength];

            for (int rowIndex = 0; rowIndex < maxRowlength; rowIndex++)
                for (int colIndex = 0; colIndex < maxColLength; colIndex++)
                    for (int depthIndex = 0; depthIndex < maxDepthLength; depthIndex++)
                        result[rowIndex, colIndex, depthIndex] = func(pixes[rowIndex, colIndex, depthIndex]);

            return result;
        }

        public static TK[,] ApplyFilterFunction<T, TK>(T[,] pixes, Func<T, TK> func)
        {
            var maxRowlength = pixes.GetLength(0);
            var maxColLength = pixes.GetLength(1);
            var result = new TK[maxRowlength, maxColLength];

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
