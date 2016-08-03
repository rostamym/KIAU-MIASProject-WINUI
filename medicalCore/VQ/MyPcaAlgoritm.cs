using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Statistics.Analysis;
using DicomImageViewer.VQ;

namespace DicomImageViewer.PCA
{
    public class MyPcaAlgoritm : pca.pcaBase ,IPcaAlgorithm
    {
        public List<LocalIntenceVector> LocalLocalIntenceVectores { get; private set; }

        public List<int> VarianceKL { get; private set; }

        public MyPcaAlgoritm(List<LocalIntenceVector> localIntensityVectores)
            : base(convertToArray(localIntensityVectores))
        {


            LocalLocalIntenceVectores = localIntensityVectores;


        }

        private static double[,] convertToArray(List<LocalIntenceVector> localIntensityVectores)
        {
            int numberOfDimensions = localIntensityVectores[0].LocalIntenceList.Count;
            int numberOfElements = localIntensityVectores.Count; //The number of elements per each dimension

            double[,] array = new double[numberOfDimensions, numberOfElements];

            for (int i = 0; i < numberOfElements; i++)
            {
                LocalIntenceVector liv = localIntensityVectores[i];

                for (int j = 0; j < numberOfDimensions; j++)
                {
                    array[j, i] = liv.LocalIntenceList[j];
                }
            }

            return array;
        }

        private List<LocalIntenceVector> transformMatrixToVector(double[][] mat)
        {
            List<LocalIntenceVector> liv = new List<LocalIntenceVector>();


            var temp = new LocalIntenceVector();
            for (int i = 0; i < mat[0].Length; i++)
            {
                for (int j = 0; j < mat.Length; j++)
                {
                    temp.LocalIntenceList.Add((short)mat[j][i]);
                }
                liv.Add(temp);
            }

            return liv;
        }

        public List<LocalIntenceVector> DoAlgorithm(int percent)
        {
            var transformedImg = base.DoAlgorithm(percent);

            var TransformedLocalIntensityVectors = transformMatrixToVector(transformedImg);

            return TransformedLocalIntensityVectors;
        }
     
    }
}
