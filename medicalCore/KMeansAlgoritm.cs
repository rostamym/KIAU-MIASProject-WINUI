using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomImageViewer.VQ
{
    public class KMeansAlgoritm
    {
        #region Filed
//        public List<int> VarianceList { get; private set; }

        public int K { get; private set; }
        
        public List<RepresentativeVector> C { get; private set; }

        public Dictionary<int, List<LocalIntenceVector>> VectorLabeleDictionary { get; private set; }

        public List<LocalIntenceVector> LocalIntenceVectors { get; private set; }

        public List<LocalIntenceVector> normalizedVectors { get; private set; }

        private int dimension;

        private List<LocalIntenceVector> clusters;
        #endregion

        #region methods declaration
        public KMeansAlgoritm(List<int> varianceList, int k, List<LocalIntenceVector> localIntenceVectors)
        {
//            VarianceList = varianceList;
            K = k;
            LocalIntenceVectors = localIntenceVectors;

            dimension = localIntenceVectors[0].LocalIntenceListDoubles.Count;

            clusters = new List<LocalIntenceVector>();
            
            for (int i = 0; i < k; i++)
                clusters.Add(new LocalIntenceVector() { LocalIntenceListDoubles = new List<double> { 0,0,0,0,0,0,0 }, Lable = i }); 
        }

        public void DoAlgoritm()
        {
            normalizedVectors = normalizeVectors(LocalIntenceVectors);

            cluster();
        }

        private void cluster()
        {
            bool _changed = true;
            bool _success = true;
            InitializeCentroids();

            int maxIteration = normalizedVectors.Count * 10;

            int _threshold = 0;
            
            while (_success == true && _changed == true && _threshold < maxIteration)
            {
                 ++_threshold;
                _success = UpdateDataPointMeans();
                _changed = UpdateClusterMembership();
                
            }
        }

        private bool UpdateClusterMembership()
        {
            bool changed = false;

            double[] distances = new double[K];
            
            for (int i = 0; i < normalizedVectors.Count; ++i)
            {

                for (int k = 0; k < K; ++k)
                    distances[k] = ElucidanDistance(normalizedVectors[i], clusters[k]);

                int newClusterId = MinIndex(distances);

                if (newClusterId != normalizedVectors[i].Lable)
                {
                    changed = true;
                    normalizedVectors[i].Lable = LocalIntenceVectors[i].Lable  = newClusterId;
                }
                else
                {
                }

            }
            if (changed == false)
                return false;
            if (EmptyCluster(normalizedVectors)) return false;
            return true;
        }

        private int MinIndex(double[] distances)
        {
            int _indexOfMin = 0;
            double _smallDist = distances[0];
            for (int k = 0; k < distances.Length; ++k)
            {
                if (distances[k] < _smallDist)
                {
                    _smallDist = distances[k];
                    _indexOfMin = k;
                }
            }
            return _indexOfMin;
        }

        private double ElucidanDistance(LocalIntenceVector dataPoint, LocalIntenceVector mean)
        {
            double _diffs = 0.0;
            for (int i = 0; i < dimension; i++)
                _diffs += Math.Pow(dataPoint.LocalIntenceListDoubles[i] - mean.LocalIntenceListDoubles[i], 2);
            
            return Math.Sqrt(_diffs);
        }

        private bool UpdateDataPointMeans()
        {
            if (EmptyCluster(normalizedVectors)) return false;

            var groupToComputeMeans = normalizedVectors.GroupBy(s => s.Lable).OrderBy(s => s.Key);
            int clusterIndex = 0;
            double[] dimensionSum = new double[dimension];

            foreach (var item in groupToComputeMeans)
            {
                foreach (var value in item)
                {
                    for (int i = 0; i < dimension; i++)
                        dimensionSum[i] += value.LocalIntenceListDoubles[i];
                }

                for (int i = 0; i < dimension; i++)
                    clusters[clusterIndex].LocalIntenceListDoubles[i] = (dimensionSum[i] / item.Count());
                
                clusterIndex++;

                dimensionSum = new double[dimension];
            }
            return true;
        }

        private bool EmptyCluster(List<LocalIntenceVector> data)
        {
            var emptyCluster =
                data.GroupBy(s => s.Lable).OrderBy(s => s.Key).Select(g => new { Lable = g.Key, Count = g.Count() });

            foreach (var item in emptyCluster)
            {
                if (item.Count == 0)
                {
                    return true;
                }
            }
            return false;
        }

        private void InitializeCentroids()
        {
            Random random = new Random(K);
            for (int i = 0; i < K; ++i)
            {
                normalizedVectors[i].Lable = LocalIntenceVectors[i].Lable = i;
            }
            for (int i = K; i < normalizedVectors.Count; i++)
            {
                normalizedVectors[i].Lable = LocalIntenceVectors[i].Lable = random.Next(0, K);
            }
        }

        private List<LocalIntenceVector> normalizeVectors(List<LocalIntenceVector> localIntenceVectors)
        {
            
            double[] dimensionsSum = new double[dimension];

            List<LocalIntenceVector> normalizedVectors = new List<LocalIntenceVector>();

            foreach (LocalIntenceVector vector in localIntenceVectors)
            {
                for (int i = 0; i < dimension; i++)
                    dimensionsSum[i] += vector.LocalIntenceListDoubles[i];
                    
            }

            double[] dimensionsMean = new double[dimension];

            for (int i = 0; i < dimension; i++)
                dimensionsMean[i] = dimensionsSum[i] / localIntenceVectors.Count;

            double[] sumDimension = new double[dimension];

            foreach (LocalIntenceVector vector in localIntenceVectors)
            {
                for (int i = 0; i < dimension; i++)
                    sumDimension[i] += Math.Pow(vector.LocalIntenceListDoubles[i] - dimensionsMean[i], 2); 

            }
            
            double[] dimensionSD = new double[dimension];
            for (int i = 0; i < dimension; i++)
                dimensionSD[i] = sumDimension[i] / localIntenceVectors.Count;

            foreach (LocalIntenceVector vector in localIntenceVectors)
            {
                LocalIntenceVector tempVector = new LocalIntenceVector();

                tempVector.mainPoint = vector.mainPoint;
                tempVector.LocalIntenceListDoubles = new List<double>();

                for (int i = 0; i < dimension; i++)
                    tempVector.LocalIntenceListDoubles.Add(((vector.LocalIntenceListDoubles[i] - dimensionsMean[i]) / dimensionSD[i]));

                normalizedVectors.Add(tempVector);
            }
            
            return normalizedVectors;
        }

        
        
        #endregion

        #region The past written code
        /*
         public VQAlgoritm(List<int> varianceList, int k, List<LocalIntenceVector> localIntenceVectors)
        {
            VarianceList = varianceList;
            K = k;
            LocalIntenceVectors = localIntenceVectors;
            VectorLabeleDictionary= new Dictionary<int, List<LocalIntenceVector>>();
        }

        public void DoAlgoritm()
        {

            //foreach (var klValue in VarianceList)
            //{
                C = null;
            LocalIntenceVectors.ForEach(x => Classifier(x, VarianceList.Max()));
            
                //if (C.Count == K) break;
            //}

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
                Dictionary<RepresentativeVector, Int64> euclideanDistanceValues = C.ToDictionary(x => x, x => EuclideanDistance(localIntenceVector, x));
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
                    var newRepresentativeVector= new RepresentativeVector(localIntenceVector);
                    C.Add(newRepresentativeVector);
                    var lable = C.IndexOf(newRepresentativeVector);
                    localIntenceVector.Lable = lable;
                    VectorLabeleDictionary[lable] = new List<LocalIntenceVector>();
                    VectorLabeleDictionary[lable].Add(localIntenceVector);
                }
            }
        }

        private void UpdateRepresentativeVector(RepresentativeVector winnerRepresentativeVector, LocalIntenceVector localIntenceVector)
        {
            double lernningNumber = winnerRepresentativeVector.LernningNumber;

            for (int i = 0; i < winnerRepresentativeVector.LocalIntenceListDoubles.Count; i++)
            {
                var CMI = winnerRepresentativeVector.LocalIntenceListDoubles[i];
                var WI = localIntenceVector.LocalIntenceListDoubles[i];
                CMI = (short)((lernningNumber * CMI + WI) / lernningNumber + 1);
                winnerRepresentativeVector.LocalIntenceListDoubles[i] = CMI;
            }

            winnerRepresentativeVector.LernningNumber++;
        }

        private Int64 EuclideanDistance(LocalIntenceVector localIntenceVector, RepresentativeVector representativeVector)
        {
            Int64 result = 0;
            double euclideanDis = 0;
            var efectiveDimention = localIntenceVector.LocalIntenceListDoubles.Count;
            for (var i = 0; i < efectiveDimention; i++)
            {
                var diff = localIntenceVector.LocalIntenceListDoubles[i] - representativeVector.LocalIntenceListDoubles[i];
                euclideanDis =+ Math.Pow(diff, 2);
                
            }
            //euclideanDis = Math.Sqrt(euclideanDis);
            result = Convert.ToInt64(euclideanDis);
            return result;
        }
         */
        #endregion



    }
}
