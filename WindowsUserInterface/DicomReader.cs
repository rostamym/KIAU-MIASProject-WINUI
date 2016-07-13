using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Imaging;
using System.Drawing;

namespace DicomImageViewer
{
    public class dicomReader
    {
        private DicomImageViewer.DicomDecoder objDicomDecoder;
        //private List<ushort> _datarow_16 = new List<ushort>();
        //public List<ushort> datarow_16 { get { return _datarow_16; } }
        //private List<byte> _datarow = new List<byte>();
        //public List<byte> datarow { get { return _datarow; } }
        private string[] _filesPaths;
        public string[] filesPaths { get { return _filesPaths; } }

        //List<byte> pix8;
        //List<ushort> pix16;
        ////List<int> pix16Signed;
        //List<byte> pix24;


        //Bitmap bmp;
        int hOffset;
        int vOffset;
        int hMax;
        int vMax;
        //int imgWidth;
        //int imgHeight;
        int panWidth;
        int panHeight;
        bool newImage;

        //// For Window Level
        //int winMin;
        //int winMax;
        //int winCentre;
        //int winWidth;
        //byte[] lut8;

        //byte[] imagePixels8;
        //byte[] imagePixels16;
        //byte[] imagePixels16Signed;
        //byte[] imagePixels24;
        //int sizeImg;
        //int sizeImg3;
        //int maxPixelValue;    // Updated July 2012
        //int minPixelValue;

        List<byte> _pixels8 = new List<byte>();
        List<short> _pixels16 = new List<short>();
        List<byte> _pixels24 = new List<byte>();
        List<int> _pixels16Signed = new List<int>();

        short[,] _pixels16Array2D;
        byte[,] _pixels24Array2D;
        byte[,] _pixels8Array2D;

        public List<byte> pixels8 { get { return _pixels8; } }
        public List<short> pixels16 { get { return _pixels16; } }
        public List<int> pixels16Signed { get { return _pixels16Signed; } }

        public short[,] pixels16Array2D { get { return _pixels16Array2D; } }
        public byte[,] pixels8Array2D { get { return _pixels8Array2D; } }
        public byte[,] pixels24Array2D { get { return _pixels24Array2D; } }


        public List<byte> pixels24 { get { return _pixels24; } }

        public List<string> Dic_Info { get { if (objDicomDecoder != null && objDicomDecoder.dicomInfo != null)return objDicomDecoder.dicomInfo; else return null; } }
        public int width { get { if (objDicomDecoder != null)return objDicomDecoder.width; else return 0; } }
        public int height { get { if (objDicomDecoder != null)return objDicomDecoder.height; else return 0; } }
        
        private int _samplesPerPixel;
        public int samplesPerPixel{ get { return _samplesPerPixel; } }

        public int _bitsAllocated { get; set; }
        public int bitsAllocated { get { return _bitsAllocated; } }

        public int minValue;
        public int maxValue;
        

         private bool _isSignedImage;
         public bool isSignedImage { get { return _isSignedImage; } }

        
        
        
       // private int bit_per_pixel;
        //public int Bit_Per_Pixel { get { return bit_per_pixel; } }



        //byte[] lut16 = new byte[65536];

        // This function is the constructor of the dicomReader class. It takes the .dcm file path as input and all .dcm files 
        // in that directoery is saved into an array called FilesPaths
        public dicomReader(string pathOfFile)
        {
            objDicomDecoder = new DicomImageViewer.DicomDecoder();
            //When the DicomFileName propert of the dicomDecoder is aggigned a value, the set method of its class is 
            // executed and the pixels values are read.
            objDicomDecoder.DicomFileName = pathOfFile;

            // Therefor, to here the dicom file is read into the dicom decoder object.

            //From here, others files paths are read into an array.

            System.IO.FileInfo fileInfo = new System.IO.FileInfo(pathOfFile);
            System.IO.DirectoryInfo directoryInfo = fileInfo.Directory;
            System.IO.FileInfo[] allFilesInDirectory = directoryInfo.GetFiles();
            //filesaddress = new string[allFilesInDirectory.Length];
            Queue<string> allFilesQueue = new Queue<string>();
            for (int i = 0; i < allFilesInDirectory.Length; i++)
            {
                if (allFilesInDirectory[i].Extension == ".dcm")
                    allFilesQueue.Enqueue(allFilesInDirectory[i].FullName);
            }
            //Creates an array of string with the number of files
            _filesPaths = new string[allFilesQueue.Count];
            _filesPaths = allFilesQueue.ToArray();

            //switch (objDicomDecoder.bitsAllocated)
            //{
            //    case 16: objDicomDecoder.GetPixels16(ref _datarow_16); break;
            //    case 8: objDicomDecoder.GetPixels8(ref _datarow); break;
            //    case 24: objDicomDecoder.GetPixels24(ref _datarow); break;
            //}


            _samplesPerPixel = objDicomDecoder.samplesPerPixel;
            _bitsAllocated = objDicomDecoder.bitsAllocated;

            if (objDicomDecoder.signedImage)
                _isSignedImage = true;
            else
                _isSignedImage = false;

                
            //_pixels16.Clear();
            //_pixels8.Clear();
            //_pixels24.Clear();
            //_pixels16Signed.Clear();

            if (objDicomDecoder.samplesPerPixel == 1 && objDicomDecoder.bitsAllocated == 16)
            {
                _pixels16Array2D = objDicomDecoder.pixels16Array2D;
                _bitsAllocated = 16;
                objDicomDecoder.GetPixels16(ref _pixels16);
             //   objDicomDecoder.GetPixels16Signed(ref _pixels16Signed); // Pixels16Signed is used to access to raw data
            }
            if (objDicomDecoder.samplesPerPixel == 1 && objDicomDecoder.bitsAllocated == 8)
            {
                _bitsAllocated = 8;
                objDicomDecoder.GetPixels8(ref _pixels8);
            }
            if (objDicomDecoder.samplesPerPixel == 3 && objDicomDecoder.bitsAllocated == 8)
            {
                _bitsAllocated = 24;
                objDicomDecoder.GetPixels24(ref _pixels24);
            }

            maxValue = objDicomDecoder.maxValue;
            minValue = objDicomDecoder.minValue;

        }

        public void readPixels(string argPathOfFile)
        {
            objDicomDecoder.DicomFileName = argPathOfFile;

            if (objDicomDecoder.samplesPerPixel == 1 && objDicomDecoder.bitsAllocated == 16)
            {
                _bitsAllocated = 16;

                _pixels16Array2D = objDicomDecoder.pixels16Array2D;

                objDicomDecoder.GetPixels16(ref _pixels16);

                minValue = objDicomDecoder.minValue;
                maxValue = objDicomDecoder.maxValue;

               // objDicomDecoder.GetPixels16Signed(ref _pixels16Signed); // Pixels16Signed is used to access to raw data
            }
            if (objDicomDecoder.samplesPerPixel == 1 && objDicomDecoder.bitsAllocated == 8)
            {
                _bitsAllocated = 8;

                _pixels8Array2D = objDicomDecoder.pixels8Array2D;

                objDicomDecoder.GetPixels8(ref _pixels8);

                minValue = objDicomDecoder.minValue;
                maxValue = objDicomDecoder.maxValue;
            }
            if (objDicomDecoder.samplesPerPixel == 3 && objDicomDecoder.bitsAllocated == 8)
            {
                _bitsAllocated = 24;

                _pixels24Array2D = objDicomDecoder.pixels24Array2D;

                objDicomDecoder.GetPixels24(ref _pixels24);

                minValue = objDicomDecoder.minValue;
                maxValue = objDicomDecoder.maxValue;
            }
           

        }



    }
}
