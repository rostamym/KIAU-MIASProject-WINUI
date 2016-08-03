using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomImageViewer.VQ
{
    interface IPcaAlgorithm
    {
        List<LocalIntenceVector> LocalLocalIntenceVectores { get; }

        List<int> VarianceKL { get;  }

        List<LocalIntenceVector> DoAlgorithm(int percent);

    }

  }
