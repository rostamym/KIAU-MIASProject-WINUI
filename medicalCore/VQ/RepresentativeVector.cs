using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomImageViewer.VQ
{
    public class RepresentativeVector
    {

        public RepresentativeVector(LocalIntenceVector localIntenceVector)
        {
            this.LocalIntenceList = new List<short>(localIntenceVector.LocalIntenceList);
            this.MainValue = localIntenceVector.MainValue;

            LernningNumber = 1;
        }

        public short MainValue { get;  set; }

        public bool ValidValue
        {
            get { return MainValue == LocalIntenceList[LocalIntenceList.Count / 2]; }
        }
        public List<short> LocalIntenceList { get; set; }

        //        public structs.Point3D mainPoint { get; set; }

        //        public int Lable { get; set; }

        public int LernningNumber { get; set; }
        public int Lable { get; set; }
    }
}
