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
    public class PulmonaryNodulesDetectionWithKMeans
    {
        public static readonly short Threshold = -500;
        public static readonly short ReplaceValue = short.MinValue;

        enum EnMaskType
        {
            ONE=1,TWO=2,THREE=3,
        }
        public PulmonaryNodulesDetectionWithKMeans():this(1)
        {

        }

        public PulmonaryNodulesDetectionWithKMeans(int type)
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

        public short[,,] SegmentPulmonary(short[,,] imageBinery,bool isApplyClosing)
        {
            //Simple thresholding

            imageBinery = RemoveAirByThreshold(imageBinery, Threshold, ReplaceValue);

            //High-Level VQ
            List<LocalIntenceVector> intenceVectores = MakeIntenceVectores(imageBinery);
            IPcaAlgorithm pcaAlgoritm = new AccordPcaAlgorithm(intenceVectores);
            var localIntenceVectores = pcaAlgoritm.DoAlgorithm(95);
            var highLevelVqAlgoritm = new VQAlgoritm(pcaAlgoritm.VarianceKL, 2, intenceVectores);
            highLevelVqAlgoritm.DoAlgoritm();
             
            // Connect Component Analysis
            var maskSize = new structs.Point3D()
            {
                X = imageBinery.GetLength(0),
                Y = imageBinery.GetLength(1),
                Z = imageBinery.GetLength(2)
            };
            //highLevelVqAlgoritm = new VQAlgoritm(pcaAlgoritm.VarianceKL, 2, highLevelVqAlgoritm.VectorLabeleDictionary[0]);
            //highLevelVqAlgoritm.DoAlgoritm();
            var lungMask = MakMaskFromLocalIntenceVectore(highLevelVqAlgoritm.LocalIntenceVectors, maskSize);
            
            //Morphological Closing
            if (isApplyClosing)
            {
                var structElement3D = MakeClosingMask2();
                lungMask = new Morphology().closing3D(lungMask, structElement3D);
            }

            return lungMask;//CommonUtils.ApplyFilterFunction(imageBinery, lungMask, (x, m) => m == 1 ? x : ReplaceValue);//

        }

        public void FindInc(short[, ,] imageBinery)
        {
            //Simple thresholding
           
            imageBinery = RemoveAirByThreshold(imageBinery, Threshold,ReplaceValue);

            //High-Level VQ
            List<LocalIntenceVector> intenceVectores = MakeIntenceVectores(imageBinery);
            IPcaAlgorithm pcaAlgoritm = new AccordPcaAlgorithm(intenceVectores);
            var localIntenceVectores = pcaAlgoritm.DoAlgorithm(95);
            var highLevelVqAlgoritm = new VQAlgoritm(pcaAlgoritm.VarianceKL, 3, localIntenceVectores);
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

        private short[,,] MakeClosingMask2()
        {
            var result = new short[3, 3, 3];
            CommonUtils.ApplyFilterFunction(result, x => 0);

            result[1, 1, 0] = 0;
            result[1, 1, 1] = 0;
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
            foreach (var item in localIntenceVectors)
                if (item.Lable == 0)
                    result[item.mainPoint.X, item.mainPoint.Y, item.mainPoint.Z] = 1;
            //localIntenceVectors.ForEach(x => result[x.mainPoint.X, x.mainPoint.Y, x.mainPoint.Z] = 1);
            return result;
        }

        public static short[, ,] RemoveAirByThreshold(short[, ,] imageBinery, short threshold,short replaceValue)
        {
            return CommonUtils.ApplyFilterFunction(imageBinery, x => (short)(x < threshold ? replaceValue : x));
        }


        private List<LocalIntenceVector> MakeIntenceVectores(short[,,] imageBinnery)
        {
            var resualt = new List<LocalIntenceVector>();


            for (int x = 63; x < 67; x++)
            {
                for (int y = 0; y < imageBinnery.GetLength(1); y++)
                {
                    for (int z = 0; z < imageBinnery.GetLength(2); z++)
                    {
                        var point3D = new structs.Point3D() { X = x, Y = y, Z = z };
                        LocalIntenceVector vector = null;
                        //if (imageBinnery[x,y,z] > 0)
                            vector = GetLocalIntenceVectorFromImageBinnery(imageBinnery, point3D, LocalIntenceMask);
                        if (vector != null)
                            resualt.Add(vector);
                    }
                }
            }

            return resualt;
        }


        private LocalIntenceVector GetLocalIntenceVectorFromImageBinnery(short[,,] imageBinnery, structs.Point3D localPoint3D, bool[,,] localIntenceMask)
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
                    LocalIntenceListDoubles = new List<double>()
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
                                
                                localIntenceVector.LocalIntenceListDoubles.Add(intence);
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
    }
}
