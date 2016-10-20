using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
            VectorLabeleDictionary= new Dictionary<int, List<LocalIntenceVector>>();
        }



        public void DoAlgoritm()
        {

//            LocalIntenceVectors.ForEach(x => Classifier(x, VarianceList[0]));
            
                        foreach (var klValue in VarianceList)
                        {
                            C = null;
                            LocalIntenceVectors.ForEach(x => Classifier(x, klValue));
            
                            if (C.Count == K) break;
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
                representativeVector.Lable = lable;

                localIntenceVector.Lable = lable;
                var localIntenceVectorsInAClass = new List<LocalIntenceVector>();
                localIntenceVectorsInAClass.Add(localIntenceVector);
                VectorLabeleDictionary[lable] = localIntenceVectorsInAClass;
            }
            else
            {
                Dictionary<RepresentativeVector, Int64> euclideanDistanceValues = C.ToDictionary(x => x, y => EuclideanDistance(localIntenceVector, y));
                var winnerRepresentativeVectorValue = euclideanDistanceValues.Min(x => x.Value);


                RepresentativeVector winnerRepresentativeVector = euclideanDistanceValues.FirstOrDefault(x => x.Value == winnerRepresentativeVectorValue).Key;

               

                if (winnerRepresentativeVectorValue < Math.Pow(klValue ,2) * 5 || C.Count == K)
                {

                    if (!localIntenceVector.ValidValue ||
                        //                    !winnerRepresentativeVector.ValidValue ||
                        localIntenceVector.MainValue != winnerRepresentativeVector.MainValue)
                        //MessageBox.Show("aaaaa");
                        ;


                    var lable = C.IndexOf(winnerRepresentativeVector);
                    localIntenceVector.Lable = lable;
                    winnerRepresentativeVector.Lable = lable;

                    VectorLabeleDictionary[lable].Add(localIntenceVector);
                    UpdateRepresentativeVector(winnerRepresentativeVector, localIntenceVector);
                }
                else
                {
                    var newRepresentativeVector= new RepresentativeVector(localIntenceVector);
                    C.Add(newRepresentativeVector);
                    var lable = C.IndexOf(newRepresentativeVector);
                    localIntenceVector.Lable = lable;
                    newRepresentativeVector.Lable = lable;
                    VectorLabeleDictionary[lable] = new List<LocalIntenceVector>();
                    VectorLabeleDictionary[lable].Add(localIntenceVector);
                }
            }
        }

        private void UpdateRepresentativeVector(RepresentativeVector winnerRepresentativeVector, LocalIntenceVector localIntenceVector)
        {
            return;
            //nothing 
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

        private Int64 EuclideanDistance(LocalIntenceVector localIntenceVector, RepresentativeVector representativeVector)
        {
            Int64 result = 0;

            for (var i = 0; i < localIntenceVector.LocalIntenceList.Count; i++)
            {
                var diff = localIntenceVector.LocalIntenceList[i] - representativeVector.LocalIntenceList[i];
                var euclideanDis = Math.Pow(diff, 2);
                result += Convert.ToInt64(euclideanDis);
            }
            return result;
        }






    }
}
