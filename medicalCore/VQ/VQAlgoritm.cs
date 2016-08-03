using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomImageViewer.VQ
{
    public class VQAlgoritm
    {
        public List<int> VarianceList { get; private set; }

        public int K { get; private set; }



        public List<RepresentativeVector> C { get; private set; }

        public Dictionary<int, List<LocalIntenceVector>> VectorLabeleDictionary { get; private set; }

        public List<LocalIntenceVector> LocalIntenceVectors { get; private set; }

        //        public double[][][] ImageBinery { get; private set; }


        public VQAlgoritm(List<int> varianceList, int k, List<LocalIntenceVector> localIntenceVectors)
        {
            VarianceList = varianceList;
            K = k;
            LocalIntenceVectors = localIntenceVectors;
        }



        public void DoAlgoritm()
        {

            foreach (var klValue in VarianceList)
            {
                C = null;
                LocalIntenceVectors.ForEach(x => Classifier(x, klValue));

                if (C.Count != K) break;
            }

        }

        private void Classifier(LocalIntenceVector localIntenceVector, int klValue)
        {
            if (C == null)
            {
                C = new List<RepresentativeVector>();
                var representativeVector = new RepresentativeVector(localIntenceVector);
                C.Add(representativeVector);
                int lable = C.IndexOf(representativeVector);
                localIntenceVector.Lable = lable;
                var localIntenceVectorsInAClass = new List<LocalIntenceVector>();
                localIntenceVectorsInAClass.Add(localIntenceVector);
                VectorLabeleDictionary[lable] = localIntenceVectorsInAClass;
            }
            else
            {
                Dictionary<RepresentativeVector, int> euclideanDistanceValues = C.ToDictionary(x => x, y => EuclideanDistance(localIntenceVector, y));
                var winnerRepresentativeVectorValue = euclideanDistanceValues.Min(x => x.Value);


                RepresentativeVector winnerRepresentativeVector = euclideanDistanceValues.FirstOrDefault(x => x.Value == winnerRepresentativeVectorValue).Key;

                if (winnerRepresentativeVectorValue < klValue || C.Count == K)
                {
                    var lable = C.IndexOf(winnerRepresentativeVector);
                    localIntenceVector.Lable = lable;
                    VectorLabeleDictionary[lable].Add(localIntenceVector);
                    UpdateRepresentativeVector(winnerRepresentativeVector, localIntenceVector);
                }
                else
                {
                    C.Add(winnerRepresentativeVector);
                    var lable = C.IndexOf(winnerRepresentativeVector);
                    localIntenceVector.Lable = lable;
                    VectorLabeleDictionary[lable].Add(localIntenceVector);
                }
            }
        }

        private void UpdateRepresentativeVector(RepresentativeVector winnerRepresentativeVector, LocalIntenceVector localIntenceVector)
        {
            int lernningNumber = winnerRepresentativeVector.LernningNumber;

            for (int i = 0; i < winnerRepresentativeVector.LocalIntenceList.Count; i++)
            {
                var CMI = winnerRepresentativeVector.LocalIntenceList[i];
                var WI = localIntenceVector.LocalIntenceList[i];
                CMI = (short)((lernningNumber * CMI + WI) / lernningNumber + 1);
                winnerRepresentativeVector.LocalIntenceList[i] = CMI;
            }

            winnerRepresentativeVector.LernningNumber++;
        }

        private int EuclideanDistance(LocalIntenceVector localIntenceVector, RepresentativeVector representativeVector)
        {
            var result = 0;

            var efectiveDimention = localIntenceVector.LocalIntenceList.Count;
            for (var i = 0; i < efectiveDimention; i++)
            {
                var diff = localIntenceVector.LocalIntenceList[i] - representativeVector.LocalIntenceList[i];
                var euclideanDis = Math.Pow(diff, 2);
                result += Convert.ToInt32(euclideanDis);
            }
            return result;
        }






    }
}
