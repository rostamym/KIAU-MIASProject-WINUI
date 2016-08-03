using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomImageViewer.VQ
{
    public class LocalIntenceVector
    {
        public List<short> LocalIntenceList { get; set; }

        public structs.Point3D mainPoint { get; set; }

        public int Lable { get; set; }
    }
}
