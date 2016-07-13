using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using DIC;

namespace DicomImageViewer
{
    public class Filtering
    {
        //int kernelsize = 0;

        public double[,] CreateKernelGaussain2D(double sigma, int length)
        {
            double[,] Kernel = new double[length, length];
            double sumTotal = 0;

            int kernelRadius = length / 2;
            double distance = 0;

            double calculatedEuler = 1.0 / (2.0 * Math.PI * Math.Pow(sigma, 2));

            for (int filterY = -kernelRadius; filterY <= kernelRadius; filterY++)
            {
                for (int filterX = -kernelRadius; filterX <= kernelRadius; filterX++)
                {
                    distance = ((filterX * filterX) + (filterY * filterY)) / (2 * (sigma * sigma));
                    Kernel[filterY + kernelRadius, filterX + kernelRadius] = calculatedEuler * Math.Exp(-distance);
                    sumTotal += Kernel[filterY + kernelRadius, filterX + kernelRadius];
                }
            }

            for (int y = 0; y < length; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    Kernel[y, x] = Kernel[y, x] * (1.0 / sumTotal);
                }
            }
            return Kernel;
        }
        // ----------------------------------------------------------------------------------
        public double[,] CreateKernelGaussain2D(double sigma)
        {
            int length = Convert.ToInt16(4 * sigma + 1);
            double[,] Kernel = new double[length, length];
            double sumTotal = 0;

            int kernelRadius = length / 2;
            double distance = 0;

            double calculatedEuler = 1.0 / (2.0 * Math.PI * Math.Pow(sigma, 2));

            for (int filterY = -kernelRadius; filterY <= kernelRadius; filterY++)
            {
                for (int filterX = -kernelRadius; filterX <= kernelRadius; filterX++)
                {
                    distance = ((filterX * filterX) + (filterY * filterY)) / (2 * (sigma * sigma));
                    Kernel[filterY + kernelRadius, filterX + kernelRadius] = calculatedEuler * Math.Exp(-distance);
                    sumTotal += Kernel[filterY + kernelRadius, filterX + kernelRadius];
                }
            }

            for (int y = 0; y < length; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    Kernel[y, x] = Kernel[y, x] * (1.0 / sumTotal);
                }
            }
            return Kernel;
        }

        public double[,] kernel2D(int size, double value)
        {
            //kernelsize = size;
            double[,] array;

            array = new double[size, size];
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    array[i, j] = value;
            
            return array;
        }

        public double[, ,] kernel3D(int size, double value)
        {
            //kernelsize = size;
            double[, ,] array;

            array = new double[size, size, size];
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    for (int k = 0; k < size; k++)
                        array[i, j, k] = value;
            return array;
        }

        private short[,] convertTo2d(short[] input, int width, int height)
        {
            short[,] output = new short[width, height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    output[i, j] = input[i * width + j];
                }
            }
            return output;
        }

        private byte[,] convertTo2d(byte[] input, int width, int height)
        {
            byte[,] output = new byte[width, height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    output[i, j] = input[i * width + j];
                }
            }
            return output;
        }

        private short[] convert2DTo1D(short[,] input)
        {
            short[] output = new short[input.GetLength(0) * input.GetLength(1)];
            for (int i = 0; i < input.GetLength(0); i++)
            {
                for (int j = 0; j < input.GetLength(1); j++)
                {
                    output[i * input.GetLength(0) + j] = input[i, j];
                }
            }
            return output;
        }

        private byte[] convert2DTo1D(byte[,] input)
        {
            byte[] output = new byte[input.GetLength(0) * input.GetLength(1)];
            for (int i = 0; i < input.GetLength(0); i++)
            {
                for (int j = 0; j < input.GetLength(1); j++)
                {
                    output[i * input.GetLength(0) + j] = input[i, j];
                }
            }
            return output;
        }

        public short[,] filtering2D(short[,] argInputImage, double[,] argKerenel2D)
        {
            int Limit = argKerenel2D.GetLength(0) / 2;
            short[,] Output =
                new short[argInputImage.GetLength(0), argInputImage.GetLength(1)];
            for (int i = Limit; i <= ((argInputImage.GetLength(0) - 1) - Limit); i++)
            {
                for (int j = Limit; j <= ((argInputImage.GetLength(1) - 1) - Limit); j++)
                {
                    double Sum = 0;
                    for (int k = -Limit; k <= Limit; k++)
                    {

                        for (int l = -Limit; l <= Limit; l++)
                        {
                            Sum = Sum +
                                ((double)argInputImage[i + k, j + l] *
                                argKerenel2D[Limit + k, Limit + l]);
                        }
                    }
                    Output[i, j] = (short)(Math.Round(Sum));
                }

            }
            return Output;           

        }

        public byte[,] filtering2D(byte[,] argInputImage, double[,] argKerenel2D)
        {
            int Limit = argKerenel2D.GetLength(0) / 2;
            byte[,] Output = new byte[argInputImage.GetLength(0), argInputImage.GetLength(1)];
            for (int i = Limit; i <= ((argInputImage.GetLength(0) - 1) - Limit); i++)
            {
                for (int j = Limit; j <= ((argInputImage.GetLength(1) - 1) - Limit); j++)
                {
                    double Sum = 0;
                    for (int k = -Limit; k <= Limit; k++)
                    {

                        for (int l = -Limit; l <= Limit; l++)
                        {
                            Sum = Sum + ((double)argInputImage[i + k, j + l] * argKerenel2D[Limit + k, Limit + l]);//GaussianKernel[Limit + k, Limit + l]

                        }
                    }
                    Output[i, j] = (byte)(Math.Round(Sum));
                }

            }
            return Output;
        }
        //...............................................................................
        public short[, ,] filtering3D(short[, ,] argInputSlice, double[, ,] argKerenel3D)
        {
            int Limit = argKerenel3D.GetLength(0) / 2;

            short[, ,] Output = new short[argInputSlice.GetLength(0), argInputSlice.GetLength(1), argInputSlice.GetLength(2)];

            DateTime t1 = DateTime.Now;
        //     for (int q = Limit; q <= ((argInputSlice.GetLength(0) - 1) - Limit); q++)

            Parallel.For(Limit,((argInputSlice.GetLength(0) - 1) - Limit),q =>
      {
          for (int i = Limit; i <= ((argInputSlice.GetLength(1) - 1) - Limit); i++)
          {
              for (int j = Limit; j <= ((argInputSlice.GetLength(2) - 1) - Limit); j++)
              {
                  double Sum = 0;
                  for (int w = -Limit; w <= Limit; w++)
                      for (int k = -Limit; k <= Limit; k++)
                      {

                          for (int l = -Limit; l <= Limit; l++)
                          {
                              Sum = Sum + ((double)argInputSlice[q + w, i + k, j + l] * argKerenel3D[Limit + w, Limit + k, Limit + l]);

                          }
                      }
                  Output[q, i, j] = (short)(Math.Round(Sum));
              }

          }
      }
      );
            TimeSpan ts = DateTime.Now - t1;

            return Output;
        }

        public byte[, ,] filtering3D(byte[, ,] argInputSlice, double[, ,] argKerenel3D)
        {
            int Limit = argKerenel3D.GetLength(0) / 2;

            byte[, ,] Output = new byte[argInputSlice.GetLength(0), argInputSlice.GetLength(1), argInputSlice.GetLength(2)];
        //    for (int q = Limit; q <= ((argInputSlice.GetLength(0) - 1) - Limit); q++)
            Parallel.For(Limit, ((argInputSlice.GetLength(0) - 1) - Limit), q =>
{
    for (int i = Limit; i <= ((argInputSlice.GetLength(1) - 1) - Limit); i++)
    {
        for (int j = Limit; j <= ((argInputSlice.GetLength(2) - 1) - Limit); j++)
        {
            double Sum = 0;
            for (int w = -Limit; w <= Limit; w++)
                for (int k = -Limit; k <= Limit; k++)
                {

                    for (int l = -Limit; l <= Limit; l++)
                    {
                        Sum = Sum + ((double)argInputSlice[q + w, i + k, j + l] * argKerenel3D[Limit + w, Limit + k, Limit + l]);

                    }
                }
            Output[q, i, j] = (byte)(Math.Round(Sum));
        }

    }
}
);
            return Output;
        }

        public structs.slice8[] filtering3D(structs.slice8[] argInputSlice, double[,] argKerenel2D, int Width, int Height)
        {
            int Limit = argKerenel2D.GetLength(0) / 2;
            structs.slice8[] temp = new structs.slice8[argInputSlice.Length];
            for (int z = 0; z < argInputSlice.Length; z++)
            {
                byte[,] inputImage = convertTo2d(argInputSlice[z].data, Width, Height);
                byte[,] Output = new byte[Width, Height];
                for (int i = Limit; i <= ((Width - 1) - Limit); i++)
                {
                    for (int j = Limit; j <= ((Height - 1) - Limit); j++)
                    {
                        double Sum = 0;
                        for (int k = -Limit; k <= Limit; k++)
                        {

                            for (int l = -Limit; l <= Limit; l++)
                            {
                                Sum = Sum + ((double)inputImage[i + k, j + l] * argKerenel2D[Limit + k, Limit + l]);

                            }
                        }
                        Output[i, j] = (byte)(Math.Round(Sum));
                    }

                }
                temp[z].data = convert2DTo1D(Output);
            }
            return temp;
        }

        public structs.slice24[] filtering3D(structs.slice24[] argInputSlice, double[,] argKerenel2D, int Width, int Height)
        {
            int Limit = argKerenel2D.GetLength(0) / 2;
            structs.slice24[] temp = new structs.slice24[argInputSlice.Length];
            for (int z = 0; z < argInputSlice.Length; z++)
            {
                byte[,] inputImage = convertTo2d(argInputSlice[z].data, Width, Height);
                byte[,] Output = new byte[Width, Height];
                for (int i = Limit; i <= ((Width - 1) - Limit); i++)
                {
                    for (int j = Limit; j <= ((Height - 1) - Limit); j++)
                    {
                        double Sum = 0;
                        for (int k = -Limit; k <= Limit; k++)
                        {

                            for (int l = -Limit; l <= Limit; l++)
                            {
                                Sum = Sum + ((double)inputImage[i + k, j + l] * argKerenel2D[Limit + k, Limit + l]);

                            }
                        }
                        Output[i, j] = (byte)(Math.Round(Sum));
                    }

                }
                temp[z].data = convert2DTo1D(Output);
            }
            return temp;
        }
        //...............................................................................
        public structs.slice16[] filtering3D(structs.slice16[] argInputSlice, double[,,] argKerenel3D, int Width, int Height)
        {
            int Limit = argKerenel3D.GetLength(0) / 2;
            structs.slice16[] temp = new structs.slice16[argInputSlice.Length];
            for (int z = 0; z < argInputSlice.Length; z++)
            {
                short[,] inputImage = convertTo2d(argInputSlice[z].data, Width, Height);

                short[,] Output = new short[Width, Height];
                for (int i = Limit; i <= ((Width - 1) - Limit); i++)
                {
                    for (int j = Limit; j <= ((Height - 1) - Limit); j++)
                    {
                        double Sum = 0;
                        for (int k = -Limit; k <= Limit; k++)
                        {

                            for (int l = -Limit; l <= Limit; l++)
                            {
                                for (int c = -Limit; c <= Limit; c++)
                                    Sum = Sum + ((double)inputImage[i + k, j + l] * argKerenel3D[Limit + k, Limit + l, Limit + c]);

                            }
                        }
                        Output[i, j] = (short)(Math.Round(Sum));
                    }

                }
                temp[z].data = convert2DTo1D(Output);
            }
            return temp;
        }
    }
}
