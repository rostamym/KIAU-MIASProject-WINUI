using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace DicomImageViewer
{
    public class structs
    {

        public struct point2D
        {
            public int X { get; set; }
            public int Y { get; set; }
        }

        public struct Point3D
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Z { get; set; }
        }

        public struct roiPoints
        {
            public Point[] points { get; set; }
        }

        [Serializable]
        public class roisPointsStruct
        {
            public Point[] boundaryPoints { get; set; }
            

        }

        public struct slice16
        {
            public short[] data { get; set; }
        }

        public struct slice8
        {
            public byte[] data { get; set; }
        }

        public struct slice24
        {
            public byte[] data { get; set; }
        }
    }
}
