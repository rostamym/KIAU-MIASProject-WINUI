using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Statistics.Analysis;

namespace DicomImageViewer.VQ
{
    public class AccordPcaAlgorithm : IPcaAlgorithm
    {
        public List<LocalIntenceVector> LocalLocalIntenceVectores { get; private set; }
        public List<int> VarianceKL { get; private set; }


        public AccordPcaAlgorithm(List<LocalIntenceVector> localIntensityVectores)
        {
            LocalLocalIntenceVectores = localIntensityVectores;
        }

        public List<LocalIntenceVector> DoAlgorithm(int percent)
        {
            var resualt = new List<LocalIntenceVector>();
            var rowCount = this.LocalLocalIntenceVectores.Count;
            var colCount = this.LocalLocalIntenceVectores[0].LocalIntenceList.Count;
            var arrayData = new double[rowCount, colCount];

            for (int row = 0; row < rowCount; row++)
            {
                for (int col = 0; col < colCount; col++)
                {
                    arrayData[row, col] = LocalLocalIntenceVectores[row].LocalIntenceList[col];
                }
            }

            var pca = new PrincipalComponentAnalysis(arrayData, PrincipalComponentAnalysis.AnalysisMethod.Correlation);
            pca.Compute();
            var threshold = percent/100f;
            var numberOfComponents = pca.GetNumberOfComponents(threshold);
            var transform = pca.Transform(arrayData, numberOfComponents);
            this.VarianceKL = pca.EigenValues.ToList().ConvertAll(x => (int)x);

            for (int row = 0; row < transform.GetLength(0); row++)
            {
                var localIntenceVector = new LocalIntenceVector();
                var oldLocalVector = this.LocalLocalIntenceVectores[row];
                localIntenceVector.mainPoint = oldLocalVector.mainPoint;
                localIntenceVector.Lable = oldLocalVector.Lable;
                localIntenceVector.LocalIntenceList = new List<short>();

                for (int col = 0; col < transform.GetLength(1); col++)
                {
                    localIntenceVector.LocalIntenceList.Add((short)transform[row, col]);
                }
                resualt.Add(localIntenceVector);

            }





            return resualt;
        }

    }
}
