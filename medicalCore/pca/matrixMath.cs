using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomImageViewer.pca
{
    class matrixMath
    {
        public static double ExpectedValueOfMatrix(LocalIntenceVector liv)//Calcualte the mathematical expectation of matrix
        {
            double average = 0;
            double sum = 0; //sum of all elements in matrix
            int length;//the number of all elements in matrix
            try
            {
                length = liv.LocalIntenceList.Count;
            }
            catch(Exception)
            {
                length = 1;
            }
            
            //Evalute the sum of all elements in matrix
            foreach(double element in liv.LocalIntenceList)
                    sum += element;
             

            //Evalute the average of all elements in matrix
            average = sum / length;
            return average;
        }
   
        public static double[,] multipleMatrixoperator(double[,] matrix1, short[] matrix2)//Do matrix multiplication operation
        {
            if (matrix1.GetLength(0) != matrix2.Length)
                throw new invalidMatrixSizeForOperation();

            double[,] resutl = new double[matrix1.GetLength(0), matrix2.GetLength(1)];

            for(int row = 0; row < matrix1.GetLength(1); row++)
            {
               double temp = 0;

               for (int associate = 0; associate < matrix1.GetLength(0); associate++)
                    temp += matrix1[row, associate] * matrix2[associate];

               resutl[row, 0] = temp;
                
            }
            return resutl;
        }

        public static double[,] covariance(LocalIntenceVector liv, double e)//computing the special vector covariance
        {
            int count = liv.LocalIntenceList.Count;//number of elements in vector
            double[,] covarianceMatrix = new double[count, count];

            for (int i = 0; i < count; i++)
            {

                for (int j = 0; j < count; j++)
                {
                    int index = j + i;
                    if (index > count)
                        index %= count;
                    covarianceMatrix[i, j] = (liv.LocalIntenceList[j] - e) * (liv.LocalIntenceList[index] - e);

                }
            }

            return covarianceMatrix;
        }

        public static double[,] eigenValues(double[,] matrix)
        {
            if (matrix.GetLength(0) != matrix.GetLength(1))
                throw new invalidMatrixSizeForOperation();

            MLApp.MLApp matlab = new MLApp.MLApp();//Initialize matlab object which connect the matlab program

            object result = null;//result which get the raw data from matlab program

            matlab.Feval("eig", 1,out result, matrix);//executing the eig method from matlab which take a matrix and return 1 result

            object[] res = result as object[];

            double[,] eigenValuesMatrix = res[0] as double[,];//cast the data from matlab to double array

            return eigenValuesMatrix;
        }

        public static double[,] eigenVecotrs(double[,] matrix)
        {
            if (matrix.GetLength(0) != matrix.GetLength(1))
                throw new invalidMatrixSizeForOperation();

            MLApp.MLApp matlab = new MLApp.MLApp();//Initialize matlab object which connect the matlab program

            object result = null;//result which get the raw data from matlab program

            matlab.Feval("eig", 2, out result, matrix);//executing the eig method from matlab which take a matrix and return 1 result

            object[] res = result as object[];

            double[,] eigenVectorsMatrix = res[0] as double[,];//cast the data from matlab to double array

            return eigenVectorsMatrix;
        }

        public static double[,] reverseMatrix(double[,] mat)
        {
            int cols = mat.GetLength(0);
            int rows = mat.GetLength(1);
            double[,] reversedMat = new double[cols,rows];

            for(int i =0; i < mat.GetLength(0); i++)
                for(int j =0; j < mat.GetLength(1); j++)
                {
                    reversedMat[j, i] = mat[i, j];
                }
            return reversedMat;
        }

        public static double[,] multipleMatrixoperator(double[,] matrix1, double[,] matrix2)
        {
            if(matrix1.GetLength(0) != matrix2.GetLength(1))
                throw new invalidMatrixSizeForOperation();

            double[,] resutl = new double[matrix1.GetLength(0), matrix2.GetLength(1)];

            for (int row = 0; row < matrix1.GetLength(1); row++)
            {
                for(int col =0; col < matrix2.GetLength(0);col++)
                {
                    double temp = 0;

                    for (int associate = 0; associate < matrix1.GetLength(0); associate++)
                        temp += matrix1[row, associate] * matrix2[associate,col];

                    resutl[row, col] = temp;
                }
                

            }
            return resutl;
        }
    }
}
