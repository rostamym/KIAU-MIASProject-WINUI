using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DicomImageViewer.Base;
using DicomImageViewer.VQ;

namespace DicomImageViewer
{
    public class PulmonaryNodulesDetection
    {
        public static readonly short Threshold = -500;
        public static readonly short ReplaceValue = short.MinValue;

        enum EnMaskType
        {
            ONE=1,TWO=2,THREE=3,
        }
        public PulmonaryNodulesDetection():this(1)
        {

        }

        public PulmonaryNodulesDetection(int type)
        {
            this.LocalIntenceMask= GetInstanceMask(type);
        }

        private bool[,,] GetInstanceMask(int type)
        {
            EnMaskType maskType = (EnMaskType) type;
            short[,,] resualt;

            switch (maskType)
            {
                case EnMaskType.ONE:
                    resualt = new short[,,]
                    {
                        {
                            {0, 0, 0},
                            {0, 1, 0},
                            {0, 0, 0}
                        },
                        {
                            {0, 1, 0},
                            {1, 1, 1},
                            {0, 1, 0}
                        },
                        {
                            {0, 0, 0},
                            {0, 1, 0},
                            {0, 0, 0}
                        },
                    };
                    break;
                case EnMaskType.TWO:
                    resualt = new short[,,]
                    {
                        {
                            {0, 0, 0},
                            {0, 1, 0},
                            {0, 0, 0}
                        },
                        {
                            {1, 1, 1},
                            {1, 1, 1},
                            {1, 1, 1}
                        },
                        {
                            {0, 0, 0},
                            {0, 1, 0},
                            {0, 0, 0}
                        },
                    };
                    break;
                case EnMaskType.THREE:
                    resualt = new short[,,]
                    {
                        {
                            {0, 0, 0, 0, 0},
                            {0, 0, 1, 0, 0},
                            {0, 1, 1, 1, 0},
                            {0, 0, 1, 0, 0},
                            {0, 0, 0, 0, 0},
                        },
                        {
                            {0, 0, 1, 0, 0},
                            {0, 1, 1, 1, 0},
                            {1, 1, 1, 1, 1},
                            {0, 1, 1, 1, 0},
                            {0, 0, 1, 0, 0},
                        },
                        {
                            {0, 0, 0, 0, 0},
                            {0, 0, 1, 0, 0},
                            {0, 1, 1, 1, 0},
                            {0, 0, 1, 0, 0},
                            {0, 0, 0, 0, 0},
                        },
                       
                    };
                    break;
                default:
                    resualt = new short[,,]
                    {
                        {
                            {0, 0, 0},
                            {0, 1, 0},
                            {0, 0, 0}
                        },
                        {
                            {0, 1, 0},
                            {1, 1, 1},
                            {0, 1, 0}
                        },
                        {
                            {0, 0, 0},
                            {0, 1, 0},
                            {0, 0, 0}
                        },
                    };
                    break;

            }

            return CommonUtils.ApplyFilterFunction(resualt, x => x == 1);
        }

        public Boolean[, ,] LocalIntenceMask { get; set; }

        public List<short[,,]> SegmentPulmonary(short[,,] imageBinery,bool isApplyClosing)
        {
            //Simple thresholding

//            imageBinery = RemoveAirByThreshold(imageBinery, Threshold, ReplaceValue);

            //High-Level VQ
            List<LocalIntenceVector> intenceVectores = MakeIntenceVectores(imageBinery);
      //      IPcaAlgorithm pcaAlgoritm = new AccordPcaAlgorithm(intenceVectores);
           // var localIntenceVectores = pcaAlgoritm.DoAlgorithm(95);
            //            var highLevelVqAlgoritm = new VQAlgoritm(pcaAlgoritm.VarianceKL, 2, localIntenceVectores);


            var varianceList = makeVarianceList(intenceVectores);
            var highLevelVqAlgoritm = new VQAlgoritm(varianceList, 2, intenceVectores);
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
            if (isApplyClosing)
            {
                var structElement3D = MakeClosingMask();
                lungMask = new Morphology().closing3D(lungMask, structElement3D);
            }

            var resualt = new List<short[,,]>();
               resualt.Add(  CommonUtils.ApplyFilterFunction(imageBinery, lungMask, (x, m) => m == 1 ? x : ReplaceValue));
            var lungMask2 = MakMaskFromLocalIntenceVectore(highLevelVqAlgoritm.VectorLabeleDictionary[0], maskSize);

            //Morphological Closing
            if (isApplyClosing)
            {
                var structElement3D = MakeClosingMask();
                lungMask2 = new Morphology().closing3D(lungMask2, structElement3D);
            }

            resualt.Add(CommonUtils.ApplyFilterFunction(imageBinery, lungMask2, (x, m) => m == 1 ? x : ReplaceValue));
            return resualt;

        }

        private List<int> makeVarianceList(List<LocalIntenceVector> intenceVectores)
        {
            var varianceList = new List<int>();

            int minVal = Int32.MaxValue;
            int maxVal = Int32.MinValue;

            intenceVectores.ForEach(x =>
            {
                var localMin = x.LocalIntenceList.Min();
                var localMax = x.LocalIntenceList.Max();
                if (minVal > localMin) minVal = localMin;
                if (maxVal < localMax) maxVal = localMax;
            });

            var varianceVal = maxVal - minVal;

            for (int i = varianceVal; i > 10 ; i-=10)
            {
                varianceList.Add(i);
            }

            return varianceList;
        }

        public void FindInc(short[, ,] imageBinery)
        {
            //Simple thresholding
           
            imageBinery = RemoveAirByThreshold(imageBinery, Threshold,ReplaceValue);

            //High-Level VQ
            List<LocalIntenceVector> intenceVectores = MakeIntenceVectores(imageBinery);
            IPcaAlgorithm pcaAlgoritm = new AccordPcaAlgorithm(intenceVectores);
            var localIntenceVectores = pcaAlgoritm.DoAlgorithm(95);
//            var highLevelVqAlgoritm = new VQAlgoritm(pcaAlgoritm.VarianceKL, 2, localIntenceVectores);
            var highLevelVqAlgoritm = new VQAlgoritm(pcaAlgoritm.VarianceKL, 2, intenceVectores);
            highLevelVqAlgoritm.DoAlgoritm();

            // Connect Component Analysis
            var maskSize = new structs.Point3D()
            {
                X = imageBinery.GetLength(0),
                Y = imageBinery.GetLength(1),
                Z = imageBinery.GetLength(2)
            };
            var lungMask = MakMaskFromLocalIntenceVectore(highLevelVqAlgoritm.VectorLabeleDictionary[0], maskSize);
            //Morphological Closing

            var structElement3D = MakeClosingMask();
            new Morphology().closing3D(lungMask, structElement3D);

            ApplyMaskAndRemoveUnusedLocalIntenceVectores(localIntenceVectores, lungMask);

            // low level VQ
            var lowLevelVqAlgoritm = new VQAlgoritm(pcaAlgoritm.VarianceKL, 4, localIntenceVectores);
            lowLevelVqAlgoritm.DoAlgoritm();

            var incVector = lowLevelVqAlgoritm.VectorLabeleDictionary[3];

            MessageBox.Show("incVector find :" + incVector.Count.ToString());


        }

        private static void ApplyMaskAndRemoveUnusedLocalIntenceVectores(List<LocalIntenceVector> localIntenceVectores, short[,,] lungMask)
        {
            localIntenceVectores.RemoveAll(x => lungMask[x.mainPoint.X, x.mainPoint.Y, x.mainPoint.Z] ==0  )  ;
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
            localIntenceVectors.ForEach(x => result[x.mainPoint.X, x.mainPoint.Y, x.mainPoint.Z] = 1);
            return result;
        }

        public static short[, ,] RemoveAirByThreshold(short[, ,] imageBinery, short threshold,short replaceValue)
        {
            return CommonUtils.ApplyFilterFunction(imageBinery, x => (short)(x < threshold ? replaceValue : x));
        }


        private List<LocalIntenceVector> MakeIntenceVectores(short[, ,] imageBinnery)
        {
            var resualt = new List<LocalIntenceVector>();


//            for (int x = 0; x <  imageBinnery.GetLength(0); x++)
            for (int x = 25; x <  35; x++)
            {
                for (int y = 0; y < imageBinnery.GetLength(1); y++)
                {
                    for (int z = 0; z < imageBinnery.GetLength(2); z++)
                    {
                         
                        var point3D = new structs.Point3D() { X = x, Y = y, Z = z };
                        LocalIntenceVector vector = GetLocalIntenceVectorFromImageBinnery(imageBinnery, point3D, LocalIntenceMask);
//                        LocalIntenceVector vector = new LocalIntenceVector()
//                        {
//                            LocalIntenceList = new List<short>() {imageBinnery[x, y, z]},
//                            mainPoint = point3D
//                        };

                        if(vector!= null)
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



            if (CheckBoundry(imageBinnery, localPoint3D, localIntenceMask, radialPoint) && 
                CheckThresoldValue(imageBinnery,localPoint3D))
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
                        for (int z = 0; z < localIntenceMask.GetLength(2); z++)
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

        private bool CheckThresoldValue(short[,,] imageBinnery, structs.Point3D localPoint3D)
        {
            return imageBinnery[localPoint3D.X, localPoint3D.Y, localPoint3D.Z]!= ReplaceValue;
        }

        private static bool CheckBoundry(short[, ,] imageBinnery, structs.Point3D localPoint3D, bool[, ,] localIntenceMask, structs.Point3D radialPoint)
        {
            return localPoint3D.X - radialPoint.X >= 0 &&
                   localPoint3D.Y - radialPoint.Y >= 0 &&
                   localPoint3D.Z - radialPoint.Z >= 0 &&
                   localPoint3D.X - radialPoint.X + localIntenceMask.GetLength(0) < imageBinnery.GetLength(0) &&
                   localPoint3D.Y - radialPoint.Y + localIntenceMask.GetLength(1) < imageBinnery.GetLength(1) &&
                   localPoint3D.Z - radialPoint.Z + localIntenceMask.GetLength(2) < imageBinnery.GetLength(2);
        }

        public short[,,] CreateParams3DGeometricImage(structs.Point3D sizeImage, short backGround, short foreground, Func<int ,int ,int ,bool > drawFunc  )
        {
            var result = new short[sizeImage.X, sizeImage.Y, sizeImage.Z];

            for (int x = 0; x < sizeImage.X; x++)
                for (int y = 0; y < sizeImage.Y; y++)
                    for (int z = 0; z < sizeImage.Z; z++)
                        if (drawFunc(x, y, z))
                            result[x, y, z] = foreground;
                        else
                            result[x, y, z] = backGround;

            return result;

        }
    }
}
