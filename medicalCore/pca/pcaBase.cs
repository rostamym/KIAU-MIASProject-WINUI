using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomImageViewer.pca
{
    public class pcaBase
    {

        private double[,] multiDimensionalImage;//Each row is one dimension

        private double[][] arraysOfImageDimension;

        private int dimension;
        public List<int> VarianceKL { get; private set; }

        #region constructor
        public pcaBase(double[,] mdi)
        {
            multiDimensionalImage = mdi;

            dimension = mdi.GetLength(0);//keep the number of dimensions

            //Initialize the arrayOfImageDimension
            arraysOfImageDimension = new double[mdi.GetLength(0)][];
            separateDimension(multiDimensionalImage,arraysOfImageDimension);
        }


        #endregion

        //-------------------------------------------------------------------------

        #region private methods

        private void separateDimension(double[,] mdi,double[][] aid)//convert double[,] array to double[][] array 
        {
            for(int i =0; i < mdi.GetLength(0); i++)
            {
                aid[i] = new double[mdi.GetLength(1)];
                for(int j=0; j < mdi.GetLength(1); j++)
                {
                    aid[i][j] = mdi[i, j];
                }
            }
        }

        private List<double> meanVector(double[,] mdi)//Calculate the mean vector of matrix
        {
            List<double> expectedValues = new List<double>();//Keep the list of expected  values of each localIntenceVectors class intances

            //Calculate the expected vlaues of each localIntanceValues class intances and add it the
            //expectedValues list
            for(int row =0; row < mdi.GetLength(0); row++)
            {
                double sum = 0; //Keep the sum of all elements in one dimension

                //iterate in one dimension of array
                for(int col =0; col < mdi.GetLength(1); col++)
                {
                    sum += mdi[row, col];
                }

                double expectedValue = sum / mdi.GetLength(1);
                expectedValues.Add(expectedValue);
            }

            return expectedValues;
        }

        private double[,] computeAutoCovariance(double[][] aoid, List<double> expectedValues)//Calculate all spatial autocorrelations of list of vectors
        {
            double[,] Covariance = new double[dimension,dimension];//auto covarince matrix of mdi matrix
            
            for(int i =0; i <dimension; i++)
            {
                for(int j=0; j< dimension; j++)
                {
                    Covariance[i, j] = pca.matrixMath.covariance(aoid[i], expectedValues[i], aoid[i], expectedValues[i]);
                }
            }

            
            return Covariance;
        }

        private List<double[,]> computeEigenVectors(List<double[,]> ListOfMatrix)
        {
            List<double[,]> listOfEigenVectors = new List<double[,]>();

            foreach (var matrix in ListOfMatrix)
            {
                var eigenVector = pca.matrixMath.eigenVecotrs(matrix);
                listOfEigenVectors.Add(eigenVector);
            }

            return listOfEigenVectors;
        }

        private List<double[,]> tranformMatrix(List<LocalIntenceVector> listOf_liv, List<double[,]> listOfEigenVectors)
        {
            var listOfTransformedMatrix = new List<double[,]>();

            for (int i = 0; i < listOf_liv.Count; i++)
            {
                var livMat = listOf_liv[i].LocalIntenceList.ToArray();
                var result = pca.matrixMath.multipleMatrixoperator(listOfEigenVectors[i], livMat);

                listOfTransformedMatrix.Add(result);
            }

            return listOfTransformedMatrix;
        }

        private double[][] reduceDimension(double[,] covariance,double[][] matrix, int percent)
        {
            double sumOfCovarianceElements = 0;
            HashSet<int> rowsToRemove = new HashSet<int>();

            foreach (var item in covariance)
                sumOfCovarianceElements += item;

            for(int i =0; i < dimension; i++)
            {
                double sumOfOneRowElements = 0;
                for(int j=0; j<dimension; j++)
                {
                    sumOfOneRowElements += covariance[i, j];
                }
                double rowPercentage = (sumOfOneRowElements / sumOfCovarianceElements) * 100;

                if(rowPercentage < percent)
                {
                    rowsToRemove.Add(i);
                }
            }

            double[][] reducedDimensionImg = matrix.Where((arr, index) => !rowsToRemove.Contains(index)).ToArray();

            return reducedDimensionImg;
        }

        private List<double[,]> reverseMatrix(List<double[,]> listOfEigenVectors)
        {
            List<double[,]> reverse = new List<double[,]>();

            foreach (var mat in listOfEigenVectors)
            {
                double[,] temp = pca.matrixMath.reverseMatrix(mat);
                reverse.Add(temp);
            }

            return reverse;
        }

        private List<double[,]> tranformMatrix(List<double[,]> listOfCV, List<double[,]> listOfRevesedEigenVectors)
        {
            var listOfTransformedMatrix = new List<double[,]>();

            for (int i = 0; i < listOfCV.Count; i++)
            {
                var result = pca.matrixMath.multipleMatrixoperator(listOfRevesedEigenVectors[i], listOfCV[i]);

                listOfTransformedMatrix.Add(result);
            }

            return listOfTransformedMatrix;
        }

        private List<LocalIntenceVector> transformMatrixToVector(List<double[,]> ListOfMat)
        {
            List<LocalIntenceVector> liv = new List<LocalIntenceVector>();
            foreach (var mat in ListOfMat)
            {
                var temp = new LocalIntenceVector();
                for (int i = 0; i < mat.GetLength(0); i++)
                {
                    for (int j = 0; j < mat.GetLength(1); j++)
                    {
                        temp.LocalIntenceList.Add((short)mat[i, j]);
                    }
                    Console.WriteLine("");
                }
                liv.Add(temp);
            }

            return liv;
        }
        #endregion

        //------------------------------------------------------------------------------

        #region public methods
        public double[][] DoAlgorithm(int percent)
        {
            var expectedValues = meanVector(multiDimensionalImage);

            var autoCovariance = computeAutoCovariance(arraysOfImageDimension, expectedValues);

            var EigenVectors = matrixMath.eigenVecotrs(autoCovariance);

            var TransformedImg = matrixMath.multipleMatrixoperator(EigenVectors,multiDimensionalImage);


            //Correlation process
            double[][] arrayOfTransformedImgDimensions = new double[TransformedImg.GetLength(0)][];
            separateDimension(TransformedImg, arrayOfTransformedImgDimensions);

            var ExpectedValuesOfTransformedImg = meanVector(TransformedImg);

            var autoCovarianceOfTransformedImg = computeAutoCovariance(arrayOfTransformedImgDimensions, ExpectedValuesOfTransformedImg);

            var correlatedImg = reduceDimension(autoCovariance, arrayOfTransformedImgDimensions,percent);

            //var ExpectedValuesOfcorrelatedImg = meanVector(correlatedImg);

            return correlatedImg;
        }
        #endregion
    }

}
