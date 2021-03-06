﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomImageViewer.VQ
{
    public class LocalIntenceVector
    {
        public List<short> LocalIntenceList { get; set; }
        public List<double> LocalIntenceListDoubles { get; set; }
        public structs.Point3D mainPoint { get; set; }

        public bool ValidValue {
            get { return MainValue == LocalIntenceList[LocalIntenceList.Count/2]; }
        }
        public short MainValue { get; set; }

        public int Lable { get; set; }
    }
}
