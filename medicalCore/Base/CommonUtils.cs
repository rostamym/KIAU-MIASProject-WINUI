using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace DicomImageViewer.Base
{
    public class CommonUtils
    {

        public static void visitImageFunction<T>(T[,,] pixes,  Action<T, structs.Point3D > action)
        {
            var maxRowlength = pixes.GetLength(0);
            var maxColLength = pixes.GetLength(1);
            var maxDepthLength = pixes.GetLength(2);

            for (int rowIndex = 0; rowIndex < maxRowlength; rowIndex++)
                for (int colIndex = 0; colIndex < maxColLength; colIndex++)
                    for (int depthIndex = 0; depthIndex < maxDepthLength; depthIndex++)
                        action(pixes[rowIndex, colIndex, depthIndex],
                            new structs.Point3D() {Z = rowIndex, X = colIndex, Y = depthIndex});

           
        }

        public static List<TK> ToListImageFunction<T, TK>(T[,,] pixes, Func<T, bool> select,
            Func<T, structs.Point3D, TK> func)
        {
            var maxRowlength = pixes.GetLength(0);
            var maxColLength = pixes.GetLength(1);
            var maxDepthLength = pixes.GetLength(2);
            var result = new List<TK>();

            for (int rowIndex = 0; rowIndex < maxRowlength; rowIndex++)
                for (int colIndex = 0; colIndex < maxColLength; colIndex++)
                    for (int depthIndex = 0; depthIndex < maxDepthLength; depthIndex++)
                    {
                        var pixe = pixes[rowIndex, colIndex, depthIndex];

                        if (select(pixe))
                        {
                            result.Add(func(pixe,new structs.Point3D() {X = rowIndex, Y = colIndex, Z = depthIndex}));
                        }
                    }
        
        return result;
        }

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
