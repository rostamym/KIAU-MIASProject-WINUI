using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Accord.Statistics.Analysis;

namespace DicomImageViewer
{

    public class CommonUtils
    {

        public static T[, ,] ApplyFilterFunction<T>(T[, ,] pixes, Func<T, T> func)
        {
            var maxRowlength = pixes.GetLength(0);
            var maxColLength = pixes.GetLength(1);
            var maxDepthLength = pixes.GetLength(2);
            var result = new T[maxRowlength, maxColLength, maxDepthLength];

            for (int rowIndex = 0; rowIndex < maxRowlength; rowIndex++)
                for (int colIndex = 0; colIndex < maxColLength; colIndex++)
                    for (int depthIndex = 0; depthIndex < maxDepthLength; depthIndex++)
                        result[rowIndex, colIndex, depthIndex] = func(pixes[rowIndex, colIndex, depthIndex]);

            return result;
        }
        public static double[, ,] ApplyFilterFunction(double[, ,] pixes, Func<double, double> func)
        {
            var maxRowlength = pixes.GetLength(0);
            var maxColLength = pixes.GetLength(1);
            var maxDepthLength = pixes.GetLength(3);
            var result = new double[maxRowlength, maxColLength, maxDepthLength];

            for (int rowIndex = 0; rowIndex < maxRowlength; rowIndex++)
                for (int colIndex = 0; colIndex < maxColLength; colIndex++)
                    for (int DepthIndex = 0; DepthIndex < maxDepthLength; DepthIndex++)
                        result[rowIndex, colIndex, maxDepthLength] = func(pixes[rowIndex, colIndex, DepthIndex]);

            return result;
        }

        public static short[,] ApplyFilterFunction(short[,] pixes, Func<short, short> func)
        {
            var maxRowlength = pixes.GetLength(0);
            var maxColLength = pixes.GetLength(1);
            var result = new short[maxRowlength, maxColLength];

            for (int rowIndex = 0; rowIndex < maxRowlength; rowIndex++)
            {
                for (int colIndex = 0; colIndex < maxColLength; colIndex++)
                {
                    result[rowIndex, colIndex] = func(pixes[rowIndex, colIndex]);

                }
            }

            return result;
        }



        public static double ConvertToOneDimension(byte rate255)
        {
            if (rate255 > 255)
                return 1;
            if (rate255 < 0)
                return 0;

            return ((double)rate255 / (double)255);
        }
        public static byte ConvertTo255Dimension(double rateOne)
        {
            if (rateOne > 1)
                return 255;
            if (rateOne < 0)
                return 0;

            return (byte)Convert.ToInt32((rateOne * (double)255));
        }


    }

    public class PulmonaryNodulesDetection
    {
        public Boolean[, ,] LocalIntenceMask { get; set; }

        public void FindInc(short[, ,] imageBinery)
        {
            //Simple thresholding
            short threshold = -500;
            imageBinery = RemoveAirByThreshold(imageBinery, threshold);

            //High-Level VQ
            List<LocalIntenceVector> intenceVectores = MakeIntenceVectores(imageBinery);
            var pcaAlgoritm = new PCAAlgoritm(intenceVectores);
            var localIntenceVectores = pcaAlgoritm.DoAlgorithm(95);
            var highLevelVqAlgoritm = new VQAlgoritm(pcaAlgoritm.VarianceKL, 2, localIntenceVectores);
            highLevelVqAlgoritm.DoAlgoritm();

            // Connect Component Analysis
            var maskSize = new structs.Point3D()
            {
                X = imageBinery.GetLength(0),
                Y = imageBinery.GetLength(1),
                Z = imageBinery.GetLength(2)
            };
            var lungMask = MakMaskFromLocalIntenceVectore(highLevelVqAlgoritm.VectorLabeleDictionary[1], maskSize);
            //Morphological Closing

            var structElement3D = MakeClosingMask();
            new Morphology().closing3D(lungMask, structElement3D);

            // low level VQ
            var lowLevelVqAlgoritm = new VQAlgoritm(pcaAlgoritm.VarianceKL, 4, localIntenceVectores);
            lowLevelVqAlgoritm.DoAlgoritm();

            var incVector = lowLevelVqAlgoritm.VectorLabeleDictionary[3];

        }

        private short[, ,] MakeClosingMask()
        {
            var result = new short[3, 3, 3];
            CommonUtils.ApplyFilterFunction(result, x => 0);

            result[1, 1, 0] = 1;
            result[1, 1, 1] = 1;
            result[1, 1, 2] = 1;
            result[1, 0, 1] = 1;
            result[1, 2, 1] = 1;
            result[0, 1, 1] = 1;
            result[2, 1, 1] = 1;

            return result;
        }

        private short[, ,] MakMaskFromLocalIntenceVectore(List<LocalIntenceVector> localIntenceVectors, structs.Point3D maskSize)
        {
            var result = new short[maskSize.X, maskSize.Y, maskSize.Z];
            CommonUtils.ApplyFilterFunction(result, x => 0);
            localIntenceVectors.ForEach(x => result[x.mainPoint.X, x.mainPoint.Y, x.mainPoint.Z] = 0);

            return result;
        }

        public static short[, ,] RemoveAirByThreshold(short[, ,] imageBinery, short threshold)
        {
            return CommonUtils.ApplyFilterFunction(imageBinery, x => (short)(x < threshold ? 1 : x));
        }


        private List<LocalIntenceVector> MakeIntenceVectores(short[, ,] imageBinnery)
        {
            var resualt = new List<LocalIntenceVector>();


            for (int x = 0; x < imageBinnery.GetLength(0); x++)
            {
                for (int y = 0; y < imageBinnery.GetLength(1); y++)
                {
                    for (int z = 0; z < imageBinnery.GetLength(0); z++)
                    {
                        var point3D = new structs.Point3D() { X = x, Y = y, Z = z };
                        LocalIntenceVector vector = GetLocalIntenceVectorFromImageBinnery(imageBinnery, point3D, LocalIntenceMask);
                        resualt.Add(vector);
                    }
                }
            }

            return resualt;
        }


        private LocalIntenceVector GetLocalIntenceVectorFromImageBinnery(short[, ,] imageBinnery, structs.Point3D localPoint3D, bool[, ,] localIntenceMask)
        {
            LocalIntenceVector localIntenceVector = null;


            var radialPoint = new structs.Point3D()
            {
                X = localIntenceMask.GetLength(0) / 2,
                Y = localIntenceMask.GetLength(1) / 2,
                Z = localIntenceMask.GetLength(2) / 2
            };



            if (CheckBoundry(imageBinnery, localPoint3D, localIntenceMask, radialPoint))
            {

                localIntenceVector = new LocalIntenceVector()
                {
                    mainPoint = localPoint3D,
                    LocalIntenceList = new List<short>()
                };

                for (int x = 0; x < localIntenceMask.GetLength(0); x++)
                {
                    for (int y = 0; y < localIntenceMask.GetLength(1); y++)
                    {
                        for (int z = 0; z < localIntenceMask.GetLength(0); z++)
                        {
                            if (localIntenceMask[x, y, z])
                            {
                                int indexX = localPoint3D.X - radialPoint.X + x;
                                int indexY = localPoint3D.Y - radialPoint.Y + y;
                                int indexZ = localPoint3D.Z - radialPoint.Z + z;

                                short intence = imageBinnery[indexX, indexY, indexZ];

                                localIntenceVector.LocalIntenceList.Add(intence);
                            }
                        }
                    }
                }

            }

            return localIntenceVector;
        }

        private static bool CheckBoundry(short[, ,] imageBinnery, structs.Point3D localPoint3D, bool[, ,] localIntenceMask, structs.Point3D radialPoint)
        {
            return localPoint3D.X - radialPoint.X >= 0 &&
                   localPoint3D.Y - radialPoint.Y >= 0 &&
                   localPoint3D.Z - radialPoint.Z >= 0 &&
                   localPoint3D.X - radialPoint.X + localIntenceMask.GetLength(0) < imageBinnery.GetLength(0) &&
                   localPoint3D.X - radialPoint.X + localIntenceMask.GetLength(0) < imageBinnery.GetLength(0) &&
                   localPoint3D.X - radialPoint.X + localIntenceMask.GetLength(0) < imageBinnery.GetLength(0);
        }
    }

    public  class VQAlgoritm
    {
        public List<int> VarianceList { get; private set; }

        public int K { get; private set; }

        

        public List<RepresentativeVector> C { get;private set; }

        public Dictionary<int,List<LocalIntenceVector>> VectorLabeleDictionary { get; private set; }

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
                LocalIntenceVectors.ForEach(x => Classifier(x,klValue));

                if (C.Count != K) break;
            } 

        }

        private void Classifier(LocalIntenceVector localIntenceVector, int klValue)
        {
            if (C == null)
            {
                C=new List<RepresentativeVector>();
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
                    UpdateRepresentativeVector(winnerRepresentativeVector,localIntenceVector);
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

        private void UpdateRepresentativeVector(RepresentativeVector winnerRepresentativeVector,LocalIntenceVector  localIntenceVector)
        {
            int lernningNumber = winnerRepresentativeVector.LernningNumber;

            for (int i = 0; i < winnerRepresentativeVector.LocalIntenceList.Count; i++)
            {
                var CMI = winnerRepresentativeVector.LocalIntenceList[i];
                var WI = localIntenceVector.LocalIntenceList[i];
                CMI= (short) ((lernningNumber*CMI+WI)/lernningNumber+1);
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
                var euclideanDis = Math.Pow( diff ,2);
                result += Convert.ToInt32(euclideanDis);
            }
            return result;
        }


    
        

      
    }

    public class LocalIntenceVector
    {
        public List<short> LocalIntenceList { get; set; }

        public structs.Point3D mainPoint { get; set; }

        public int Lable { get; set; }
    }

    public class RepresentativeVector
    {

        public RepresentativeVector(LocalIntenceVector localIntenceVector)
        {
          this.LocalIntenceList = localIntenceVector.LocalIntenceList;
            LernningNumber = 1;
        }
        public List<short> LocalIntenceList { get; set; }

//        public structs.Point3D mainPoint { get; set; }

//        public int Lable { get; set; }

        public int LernningNumber { get; set; }
 


    }


    public class PCAAlgoritm : pca.pcaBase
    {
        public List<LocalIntenceVector> LocalLocalIntenceVectores { get; private set; }

        public List<int> VarianceKL { get; private set; }

        public PCAAlgoritm(List<LocalIntenceVector> localIntensityVectores):base(convertToArray(localIntensityVectores))
        {
           

            LocalLocalIntenceVectores = localIntensityVectores;

            
        }

        private static double[,] convertToArray(List<LocalIntenceVector> localIntensityVectores)
        {
            int numberOfDimensions = localIntensityVectores[0].LocalIntenceList.Count;
            int numberOfElements = localIntensityVectores.Count; //The number of elements per each dimension

            double[,] array = new double[numberOfDimensions,numberOfElements];

            for(int i =0; i < numberOfElements; i++)
            {
                LocalIntenceVector liv = localIntensityVectores[i];

                for(int j =0; j< numberOfDimensions; j++)
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
        
        public  List<LocalIntenceVector> DoAlgorithm(int percent)
        {
            var transformedImg = base.DoAlgorithm(percent);

            var TransformedLocalIntensityVectors = transformMatrixToVector(transformedImg);

            return TransformedLocalIntensityVectors;
        }
        public List<LocalIntenceVector> DoAlgoritm2(int percent)
        {
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

//            var pca = new PrincipalComponentAnalysis(arrayData, PrincipalComponentAnalysis.AnalysisMethod.Correlation);
//            pca.Compute();
//            pca.Transform(arrayData, 95);
//            pca.
//            this.VarianceKL = pca.EigenValues.ToList().ConvertAll(x => (int)x );
            
            
            return null;
        }

    }
}

