using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//using MedicalCore;

namespace DicomImageViewer
{
   public class Thresholding
    {
       public short[,] AdaptiveThreshold2D(short[,] argInputImage)
       {
           //argInputImage = new short[6, 9] { { 200, 180, 210, 1, 1, 1, 1, 1, 1 }, { 80, 110, 200, 1, 1, 1, 1, 1, 1 }, { 90, 100, 120, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 50, 120, 5 }, { 1, 1, 1, 1, 1, 1, 60, 10, 10 }, { 1, 1, 1, 1, 1, 1, 30, 90, 1 } };
           short[,] outputSlice2D16 = new short[argInputImage.GetLength(0), argInputImage.GetLength(1)];

           var prompLst = new List<string> { "horizontal slice number : ", "vertical slice number : ", "limit : " };
           var promp = DicomImageViewer.Dialog.ShowPromptDialog(prompLst, "Enter values");
           int xSlice, ySlice, limit = 0;
           if (int.TryParse(promp[0], out xSlice) && int.TryParse(promp[1], out ySlice) && int.TryParse(promp[2], out limit))
           {
               if (argInputImage.GetLength(0) % xSlice != 0)
                   MessageBox.Show("Incorrect X slice number . picture width is " + argInputImage.GetLength(0));
               else if (argInputImage.GetLength(1) % ySlice != 0)
                   MessageBox.Show("Incorrect Y slice number. picture height is " + argInputImage.GetLength(1));
               else
               {
                   int sliceWidth = argInputImage.GetLength(0) / xSlice;
                   int sliceHeight = argInputImage.GetLength(1) / ySlice;
                   for (int x = 0; x < xSlice; x++)
                   {
                       for (int y = 0; y < ySlice; y++)
                       {
                           var subImage = new short[sliceWidth, sliceHeight];

                           for (int i = 0; i < sliceWidth; i++)
                           {
                               for (int j = 0; j < sliceHeight; j++)
                               {
                                   {
                                       subImage[i, j] = argInputImage[x * sliceWidth + i, y * sliceHeight + j];

                                   }
                               }
                           }
                           var T = ComputeAverageIntensity2D(subImage);

                           //threshold value
                           var thresholValue = computeSingleThreshold2D(subImage, T, limit);
                           var subOutput = threshold2D(subImage, thresholValue);

                           //output
                           for (int i = 0; i < sliceWidth; i++)
                           {
                               for (int j = 0; j < sliceHeight; j++)
                               {
                                   {
                                       outputSlice2D16[x * sliceWidth + i, y * sliceHeight + j] = subOutput[i, j];
                                   }
                               }
                           }

                       }
                   }
               }
           }
           return outputSlice2D16;
       }

       public short[,] threshold2D(short[,] argInputImage, int T)
       {
           var output = new short[argInputImage.GetLength(0), argInputImage.GetLength(1)];
           for (int i = 0; i < argInputImage.GetLength(0); i++)
           {
               for (int j = 0; j < argInputImage.GetLength(1); j++)
               {
                   if (argInputImage[i, j] > T)

                       output[i, j] = (1);

                   else
                       output[i, j] = (0);
               }
           }
           return output;
       }

       public int computeSingleThreshold3D(short[, ,] argInputImage, int initialThreshold, int limit)
       {
           int prevT = -10000;
           int T = initialThreshold;
           var LstLow = new List<int>();
           var LstHigh = new List<int>();

           while (!checkLimit(T, prevT, limit))
           {
               LstLow = new List<int>();
               LstHigh = new List<int>();

               for (int i = 0; i < argInputImage.GetLength(0); i++)
               {
                   for (int j = 0; j < argInputImage.GetLength(1); j++)
                   {
                       for (int k = 0; k < argInputImage.GetLength(2); k++)
                       {
                           if (argInputImage[i, j, k] > T)
                               LstHigh.Add((int)argInputImage[i, j, k]);
                           else
                               LstLow.Add((int)argInputImage[i, j, k]);
                       }
                   }
               }
               prevT = T;
               var u1 = LstLow.Any() ? LstLow.Average() : 0;
               var u2 = LstHigh.Any() ? LstHigh.Average() : 0;
               T = (int)(u1 + u2) / 2;

           }
           return T;
       }

       public short[, ,] AdaptiveThreshold3D(short[, ,] argInputImage)
       {
           //argInputImage = new short[6, 9] { { 200, 180, 210, 1, 1, 1, 1, 1, 1 }, { 80, 110, 200, 1, 1, 1, 1, 1, 1 }, { 90, 100, 120, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 50, 120, 5 }, { 1, 1, 1, 1, 1, 1, 60, 10, 10 }, { 1, 1, 1, 1, 1, 1, 30, 90, 1 } };

           short[, ,] outputSlice3D16 = new short[argInputImage.GetLength(0), argInputImage.GetLength(1), argInputImage.GetLength(2)];

           var prompLst = new List<string> { "x slice number : ", "y slice number : ", "z slice number : ", "limit : " };
           var promp = DicomImageViewer.Dialog.ShowPromptDialog(prompLst, "Enter values");
           int xSlice, ySlice, zSlice, limit = 0;
           if (int.TryParse(promp[0], out xSlice) && int.TryParse(promp[1], out ySlice) && int.TryParse(promp[2], out zSlice) && int.TryParse(promp[3], out limit))
           {
               if (argInputImage.GetLength(0) % xSlice != 0)
                   MessageBox.Show("Incorrect X slice number . picture width is " + argInputImage.GetLength(0));
               else if (argInputImage.GetLength(1) % ySlice != 0)
                   MessageBox.Show("Incorrect Y slice number. picture height is " + argInputImage.GetLength(1));
               else if (argInputImage.GetLength(2) % zSlice != 0)
                   MessageBox.Show("Incorrect Z slice number. picture depth is " + argInputImage.GetLength(2));
               else
               {
                   int sliceWidth = argInputImage.GetLength(0) / xSlice;
                   int sliceHeight = argInputImage.GetLength(1) / ySlice;
                   int sliceDepth = argInputImage.GetLength(2) / zSlice;

                   for (int x = 0; x < xSlice; x++)
                   {
                       for (int y = 0; y < ySlice; y++)
                       {
                           for (int z = 0; z < zSlice; z++)
                           {
                               var subImage = new short[sliceWidth, sliceHeight, sliceDepth];

                               for (int i = 0; i < sliceWidth; i++)
                               {
                                   for (int j = 0; j < sliceHeight; j++)
                                   {
                                       {
                                           for (int k = 0; k < sliceDepth; k++)
                                           {
                                               subImage[i, j, k] = argInputImage[x * sliceWidth + i, y * sliceHeight + j, z * sliceDepth + k];
                                           }
                                       }
                                   }
                               }
                               var T = ComputeAverageIntensity3D(subImage);

                               //threshold value
                               var thresholValue = computeSingleThreshold3D(subImage, T, limit);
                               var subOutput = threshold3D(subImage, thresholValue);

                               //output
                               for (int i = 0; i < sliceWidth; i++)
                               {
                                   for (int j = 0; j < sliceHeight; j++)
                                   {
                                       for (int k = 0; k < sliceDepth; k++)
                                       {
                                           outputSlice3D16[x * sliceWidth + i, y * sliceHeight + j, z * sliceDepth + k] = subOutput[i, j, k];
                                       }
                                   }
                               }
                           }
                       }
                   }
               }
           }
           return outputSlice3D16;
       }

       //Private 3D Methods

       public short[, ,] threshold3D(short[, ,] argInputImage, int T)
       {
           var output = new short[argInputImage.GetLength(0), argInputImage.GetLength(1), argInputImage.GetLength(2)];
           for (int i = 0; i < argInputImage.GetLength(0); i++)
           {
               for (int j = 0; j < argInputImage.GetLength(1); j++)
               {
                   for (int k = 0; k < argInputImage.GetLength(2); k++)
                   {
                       if (argInputImage[i, j, k] > T)

                           output[i, j, k] = (1);

                       else
                           output[i, j, k] = (0);
                   }
               }
           }
           return output;
       }

       private bool checkLimit(int T, int prevT, int limit)
       {
           if (T < 0)
               T *= -1;
           if (prevT < 0)
               prevT *= -1;
           if (T - prevT > 10 || T - prevT < -10)
               return false;
           return true;
       }

       public int computeSingleThreshold2D(short[,] argInputImage, int startValue, int limit)
       {
           int prevT = -10000;
           int T = startValue;
           var LstLow = new List<int>();
           var LstHigh = new List<int>();



           while (!checkLimit(T, prevT, limit))
           {
               LstLow = new List<int>();
               LstHigh = new List<int>();

               for (int i = 0; i < argInputImage.GetLength(0); i++)
               {
                   for (int j = 0; j < argInputImage.GetLength(1); j++)
                   {
                       if (argInputImage[i, j] > T)
                           LstHigh.Add((int)argInputImage[i, j]);
                       else
                           LstLow.Add((int)argInputImage[i, j]);

                   }
               }
               prevT = T;
               var u1 = LstLow.Any() ? LstLow.Average() : 0;
               var u2 = LstHigh.Any() ? LstHigh.Average() : 0;
               T = (int)(u1 + u2) / 2;

           }
           return T;
       }

       public int ComputeAverageIntensity2D(short[,] inputImage)
       {
           var sum = 0;
           for (int i = 0; i < inputImage.GetLength(0); i++)
           {
               for (int j = 0; j < inputImage.GetLength(1); j++)
               {
                   sum += inputImage[i, j];
               }
           }
           var w = inputImage.GetLength(0);
           var h = inputImage.GetLength(1);
           return (int)(sum / (w * h));
       }

       public int ComputeAverageIntensity3D(short[, ,] inputImage)
       {
           var width = inputImage.GetLength(0);
           var height = inputImage.GetLength(1);
           var depth = inputImage.GetLength(2);

           var sum = 0;
           for (int i = 0; i < width; i++)
           {
               for (int j = 0; j < height; j++)
               {
                   for (int k = 0; k < depth; k++)
                   {
                       sum += inputImage[i, j, k];
                   }
               }
           }

           return (int)(sum / (width * height * depth));
       }

       //.........................................
       public short[,] threshold2D(short[,] argInputImage, int T1, int T2)
           
       {           
               int count = 0;
               short[,] outputSlice2D8 = new short[argInputImage.GetLength(0), argInputImage.GetLength(1)];

               

                       for(int i=0;i<argInputImage.GetLength(0);i++)
                           for (int j = 0; j < argInputImage.GetLength(1); j++)
                               if (argInputImage[i, j] > T1 && argInputImage[i, j] < T2)

                           outputSlice2D8[i,j] = (1);

                       else
                           outputSlice2D8[i,j] = (0);
                       count++;
                   
               
               return outputSlice2D8;              
       }

       public byte[,] threshold2D(byte[,] argInputImage, int T1, int T2)
       {
           int count = 0;
           byte[,] outputSlice2D8 = new byte[argInputImage.GetLength(0), argInputImage.GetLength(1)];

          
               for (int i = 0; i < argInputImage.GetLength(0); i++)
                   for (int j = 0; j < argInputImage.GetLength(1); j++)
                       if (argInputImage[i, j] > T1 && argInputImage[i, j] < T2)

                           outputSlice2D8[i, j] = (1);

                       else
                           outputSlice2D8[i, j] = (0);
               count++;

           
           return outputSlice2D8;
       }

       public short[, ,] threshold3D(short[, ,] argInputImage3D, int T1, int T2)
       {

           short[, ,] outputSlices3D8 = new short[argInputImage3D.GetLength(0), argInputImage3D.GetLength(1), argInputImage3D.GetLength(2)];
          
          
               DateTime t1 = DateTime.Now;
              //for (int i = 0; i < argInputImage3D.GetLength(0); i++)

               Parallel.For(0, argInputImage3D.GetLength(0), i =>
                   {
                       for (int j = 0; j < argInputImage3D.GetLength(1); j++)
                           for (int k = 0; k < argInputImage3D.GetLength(2); k++)
                               if (argInputImage3D[i, j, k] > T1 && argInputImage3D[i, j, k] < T2)
                                   outputSlices3D8[i, j, k] = 1;
                               else
                                   outputSlices3D8[i, j, k] = 0;
                   }
               );
                   TimeSpan ys = DateTime.Now - t1;
             

           
           return outputSlices3D8;
       }

       public byte[, ,] threshold3D(byte[, ,] argInputImage3D, int T1, int T2)
       {

           byte[, ,] outputSlices3D8 = new byte[argInputImage3D.GetLength(0), argInputImage3D.GetLength(1), argInputImage3D.GetLength(2)];

          
               for (int i = 0; i < argInputImage3D.GetLength(0); i++)
                   for (int j = 0; j < argInputImage3D.GetLength(1); j++)
                       for (int k = 0; k < argInputImage3D.GetLength(2); k++)
                           if (argInputImage3D[i, j, k] > T1 && argInputImage3D[i, j, k] < T2)
                               outputSlices3D8[i, j, k] = 1;
                           else
                               outputSlices3D8[i, j, k] = 0;

           
           return outputSlices3D8;
       }

    



    }
}
