using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomImageViewer
{
    public class Morphology
    {     
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        public short[,,] structuringElement3D(string str, int argSizeOfStructuringElement)
        {
            int i, j,k;
            int center = (argSizeOfStructuringElement + 1)/2 -1;
            short[,,] temp = new short[argSizeOfStructuringElement, argSizeOfStructuringElement,argSizeOfStructuringElement];
            if (str == "Disk")
            {
                for (i = 0; i < argSizeOfStructuringElement; i++)
                    for (j = 0; j < argSizeOfStructuringElement; j++)
                        for(k=0; k < argSizeOfStructuringElement; k++)
                            if (Math.Sqrt((i - center) * (i - center) + (j - center) * (j - center) + (k - center) * (k - center)) <= (argSizeOfStructuringElement/2))
                            temp[i, j,k] = 1;
                          else
                            temp[i, j,k] = 0;
            }
            if (str == "Diamond")
            {
                for (i = 0; i < argSizeOfStructuringElement ; i++)
                    for (j = 0; j < argSizeOfStructuringElement ; j++)
                        for (k=0; k<argSizeOfStructuringElement  ; k++)
                        if (Math.Abs(i - center) + Math.Abs(j - center)+Math.Abs (k-center) <= (argSizeOfStructuringElement/2))
                            temp[i, j,k] = 1;
                        else
                            temp[i, j,k] = 0;
            }
            if (str == "Square")
            {
                for (i = 0; i < argSizeOfStructuringElement; i++)
                    for (j = 0; j < argSizeOfStructuringElement; j++)
                        for(k=0; k < argSizeOfStructuringElement; k++)
                          temp[i, j,k] = 1;
            }
            return (temp);
        } // end structelement3D
       //-----------------------------------------------------------------
        public short[,] structuringElement2D(string str, int argSizeOfStructuringElement)
        {
            int i, j;
            int center = (argSizeOfStructuringElement + 1) / 2 -1; 
            short[,] temp = new short[argSizeOfStructuringElement, argSizeOfStructuringElement];   
            if(str=="Disk")
            {                                       
                for(i=0; i<argSizeOfStructuringElement ; i++)
                  for(j=0; j<argSizeOfStructuringElement ; j++)
                      if (Math.Sqrt((i - center) * (i - center) + (j - center) * (j - center)) <= (argSizeOfStructuringElement/2))
                            temp[i,j]=1;
                        else 
                          temp[i,j]=0;                
            }
            if (str == "Diamond")
            {                          
              for( i=0;i<argSizeOfStructuringElement ; i++)
                for( j=0;j<argSizeOfStructuringElement ; j++)
                    if (Math.Abs(i - center) + Math.Abs(j - center) <= argSizeOfStructuringElement)
                      temp[i,j]=1;
                  else
                      temp[i, j] = 0;              
            }
         if (str=="Square")
           {
             for (i = 0; i < argSizeOfStructuringElement; i++)
               for (j = 0; j < argSizeOfStructuringElement; j++)              
                temp[i,j]=1;                              
            }  
       return (temp);
      } // end structelement2D
//%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
  
    public short[,] dilation2D(short[,] argInputSlice2D16, short[,] structElement2D)
    {
        short[,] outputSlice2D16 = new short[argInputSlice2D16.GetLength(0), argInputSlice2D16.GetLength(1)];
              
        int Limit = (structElement2D.GetLength(0)) / 2;
                
        for (int i = Limit; i < argInputSlice2D16.GetLength(0) - Limit; i++)
        {
            for (int j = Limit; j < argInputSlice2D16.GetLength(1) - Limit; j++)
            {
                if (argInputSlice2D16[i, j] == 1)
                {
                    for (int k = -Limit; k <= Limit; k++)
                    {
                        for (int l = -Limit; l <= Limit; l++)
                        {
                            if (structElement2D[Limit + k, Limit + l] == 1)
                                outputSlice2D16[i + k, j + l] = 1;
                            //else
                            //    outputSlice2D16[i + k, j + l] = 0;
                        }
                    }
                }
                
            }
        }

        return (outputSlice2D16);
    } // end dialtion2D
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public short[,] erosion2D(short[,] argInputSlice2D16, short[,] structElement2D)
    {
        short[,] outputSlice2D16 = new short[argInputSlice2D16.GetLength(0), argInputSlice2D16.GetLength(1)];
        bool fit = new bool();

        int Limit = (structElement2D.GetLength(0)) / 2;

        for (int i = Limit; i < argInputSlice2D16.GetLength(0) - Limit; i++)
        {
            for (int j = Limit; j < argInputSlice2D16.GetLength(1) - Limit; j++)
            {
                if (argInputSlice2D16[i, j] == 1)
                {
                    fit = true;
                    for (int k = -Limit; k <= Limit; k++)
                    {
                        for (int l = -Limit; l <= Limit; l++)
                        {
                            if (structElement2D[Limit + k, Limit + l] == 1)
                                if (argInputSlice2D16[i + k, j + l] != 1)
                                    fit = false;
                        }
                    }
                    if (fit==true)
                        outputSlice2D16[i, j] = 1;
                    else
                        outputSlice2D16[i, j] = 0;
                }

            }
        }

        return (outputSlice2D16);
    } // end Erosion2D

////////////////////////////////////////////////////////////////////////////////////////
    public short[,] opening2D(short[,] argInputSlice2D16, short[,] structElement2D)
    {
        short[,] outputSlice2D16 = new short[argInputSlice2D16.GetLength(0), argInputSlice2D16.GetLength(1)];
        
        outputSlice2D16 = erosion2D(argInputSlice2D16, structElement2D);
        outputSlice2D16 = dilation2D(outputSlice2D16, structElement2D);        
        return (outputSlice2D16);
    } // end opening2DNew
////////////////////////////////////////////////////////////////////////////////////////              

    public short[,] closing2D(short[,] argInputSlice2D16, short[,] structElement2D)
    {
        short[,] outputSlice2D16 = new short[argInputSlice2D16.GetLength(0), argInputSlice2D16.GetLength(1)];
        
        outputSlice2D16 = dilation2D(argInputSlice2D16, structElement2D);
        outputSlice2D16 = erosion2D(outputSlice2D16, structElement2D);
        return (outputSlice2D16);
    } // end opening2DNew

///////////////////////////////////////////////////////////////////////////////////////
    public short[, ,] erosion3D(short[, ,] argInputSlice3D16, short[, ,] structElement3D)
    {
        short[, ,] outputSlice3D16 = new short[argInputSlice3D16.GetLength(0), argInputSlice3D16.GetLength(1), argInputSlice3D16.GetLength(2)];
        bool fit = new bool();

        int Limit = (structElement3D.GetLength(0)) / 2;
       // for (int q = Limit; q < argInputSlice3D16.GetLength(0) - Limit; q++)
        Parallel.For(Limit, argInputSlice3D16.GetLength(0) - Limit, q =>
     {
         for (int i = Limit; i < argInputSlice3D16.GetLength(1) - Limit; i++)
         {
             for (int j = Limit; j < argInputSlice3D16.GetLength(2) - Limit; j++)
             {
                 if (argInputSlice3D16[q, i, j] == 1)
                 {
                     fit = true;
                     for (int w = -Limit; w <= Limit; w++)
                     {
                         for (int k = -Limit; k <= Limit; k++)
                         {
                             for (int l = -Limit; l <= Limit; l++)
                             {
                                 if (structElement3D[Limit + w, Limit + k, Limit + l] == 1)
                                     if (argInputSlice3D16[q + w, i + k, j + l] != 1)
                                         fit = false;

                             }
                         }
                     }
                     if (fit == true)
                         outputSlice3D16[q, i, j] = 1;
                     else
                         outputSlice3D16[q, i, j] = 0;
                 }

             }

         }
     }
     );

        return (outputSlice3D16);
    } // end erosion3D
        ////////////////////////////////////


    public short[,,] dilation3D(short[,,] argInputSlice3D16, short[,,] structElement3D)
    {
        short[,,] outputSlice3D16 = new short[argInputSlice3D16.GetLength(0), argInputSlice3D16.GetLength(1), argInputSlice3D16.GetLength(2)];

        int Limit = (structElement3D.GetLength(0)) / 2;
       // for (int q = Limit; q < argInputSlice3D16.GetLength(0) - Limit; q++)
        Parallel.For(Limit, argInputSlice3D16.GetLength(0) - Limit, q =>
    {
        for (int i = Limit; i < argInputSlice3D16.GetLength(1) - Limit; i++)
        {
            for (int j = Limit; j < argInputSlice3D16.GetLength(2) - Limit; j++)
            {
                if (argInputSlice3D16[q, i, j] == 1)
                {
                    for (int w = -Limit; w <= Limit; w++)
                    {
                        for (int k = -Limit; k <= Limit; k++)
                        {
                            for (int l = -Limit; l <= Limit; l++)
                            {
                                if (structElement3D[Limit + w, Limit + k, Limit + l] == 1)
                                    outputSlice3D16[q + w, i + k, j + l] = 1;
                                //else
                                //    outputSlice3D16[q+w, i + k, j + l] = 0;
                            }
                        }
                    }
                }

            }

        }
    }
);

        return (outputSlice3D16);
    } // end dialtion3D


    public short[,,] opening3D(short[,,] argInputSlice3D16, short[,,] structElement3D)
    {
        short[, ,] outputSlice3D16 = new short[argInputSlice3D16.GetLength(0), argInputSlice3D16.GetLength(1), argInputSlice3D16.GetLength(2)];

        outputSlice3D16 = erosion3D(argInputSlice3D16, structElement3D);
        outputSlice3D16 = dilation3D(outputSlice3D16, structElement3D);
        return (outputSlice3D16);
    } // end opening3D

    public short[, ,] closing3D(short[, ,] argInputSlice3D16, short[, ,] structElement3D)
    {
        short[, ,] outputSlice3D16 = new short[argInputSlice3D16.GetLength(0), argInputSlice3D16.GetLength(1), argInputSlice3D16.GetLength(2)];


        outputSlice3D16 = dilation3D(argInputSlice3D16, structElement3D);
        outputSlice3D16 = erosion3D(outputSlice3D16, structElement3D);
        return (outputSlice3D16);
    } 

  } // end class
}
