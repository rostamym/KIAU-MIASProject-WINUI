//PGM.cs

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using System.Drawing.Imaging;

namespace DicomImageViewer
{
    public class StreamUtils
    {
        public const byte LF = 10;
        public const byte CR = 13;
        public const string CRLF = "\r\n";

        public static string ReadLine(FileStream stream)
        {
            string ans = string.Empty;
            int tbyte = stream.ReadByte();

            while (tbyte != LF)
            {
                ans += (char)tbyte;
                tbyte = stream.ReadByte();
            }
            return (ans);
        }

        public static void WriteLine(string line, FileStream stream)
        {
            line += CRLF;

            byte[] lineBytes = Encoding.ASCII.GetBytes(line);
            stream.Write(lineBytes, 0, lineBytes.Length);
        }
    }

    /// <summary>
    /// Indicates eight directions of a pixel.
    /// </summary>
    public enum Direction
    {
        NorthWest = 1,
        North = 2,
        NorthEast = 3,
        East = 4,
        SouthEast = 5,
        South = 6,
        SouthWest = 7,
        West = 8
    }

    /// <summary>
    /// PGM Image Class.
    /// </summary>
    public class PGM
    {
        #region Constants

        public const string MagicString = "P5";
        public const short MaxGray = 255;

        public const short Background = 0;
        public const short Foreground = 255;

        #endregion

        #region Fields

        private Size _size;

        #endregion

        #region Properties

        public string Path { get; set; }
        public string Comment { get; set; }
        public short[,] Pixels { get; set; }

        public Size Size
        {
            get
            {
                return (_size);
            }
            set
            {
                _size = value;
                Pixels = new short[_size.Width, _size.Height];
            }
        }

        #endregion

        #region Constructors

        public PGM()
        {
            this.Path = string.Empty;
            this.Comment = string.Empty;
            this.Size = new Size(0, 0);
        }

        public PGM(int width, int height)
        {
            this.Path = string.Empty;
            this.Comment = string.Empty;
            this.Size = new Size(width, height);
        }

        public PGM(string path)
        {
            this.Path = path;
            this.Read();
        }

        #endregion

        #region Methods

        public void Read()
        {
            try
            {
                FileStream stream = new FileStream(this.Path, FileMode.Open);

                #region Read Image Header

                string magicString = StreamUtils.ReadLine(stream);

                //read comment-lines
                this.Comment = string.Empty;
                string str = string.Empty;
                while (true)
                {
                    str = StreamUtils.ReadLine(stream);
                    if (str.Length > 0)
                    {
                        if (str.StartsWith("#"))
                        {
                            this.Comment += str;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                //parse dimension
                string[] dimensionArray = str.Split(' ');
                int width = Int32.Parse(dimensionArray[0]);
                int height = Int32.Parse(dimensionArray[1]);
                this.Size = new Size(width, height);

                int maxGray = short.Parse(StreamUtils.ReadLine(stream));

                #endregion

                #region Read Image Pixels

                for (int y = 0; y < this.Size.Height; y++)
                {
                    for (int x = 0; x < this.Size.Width; x++)
                    {
                        int gray = stream.ReadByte();
                        this.Pixels[x, y] = (short)gray;
                    }
                }

                #endregion

                stream.Close();
            }
            catch (Exception exception)
            {
                IOException imageReadException = new IOException("Error in reading image [" + this.Path + "].", exception);
                throw imageReadException;
            }
        }

        public void Write()
        {
            this.WriteToPath(this.Path);
        }

        public void WriteToPath(string path)
        {
            FileStream stream = new FileStream(path, FileMode.Create);

            //write imageheader
            StreamUtils.WriteLine(MagicString, stream);
            if (this.Comment.Length > 0)
            {
                StreamUtils.WriteLine("#" + this.Comment, stream);
            }
            StreamUtils.WriteLine(this.Size.Width + " " + this.Size.Height, stream);
            StreamUtils.WriteLine(MaxGray.ToString(), stream);

            //write imagedata
            for (int y = 0; y < this.Size.Height; y++)
            {
                for (int x = 0; x < this.Size.Width; x++)
                {
                    stream.WriteByte((byte)this.Pixels[x, y]);
                }
            }

            stream.Close();
        }

        public PGM Clone()
        {
            PGM newImage = new PGM(this.Size.Width, this.Size.Height);

            //copy image-data
            for (int y = 0; y < this.Size.Height; y++)
            {
                for (int x = 0; x < this.Size.Width; x++)
                {
                    newImage.Pixels[x, y] = this.Pixels[x, y];
                }
            }

            return (newImage);
        }

        public short GetNeighbor(int x, int y, Direction direction)
        {
            int neighborX = -1, neighborY = -1;

            switch (direction)
            {
                case Direction.NorthWest:
                    neighborX = x - 1;
                    neighborY = y - 1;
                    break;
                case Direction.North:
                    neighborX = x;
                    neighborY = y - 1;
                    break;
                case Direction.NorthEast:
                    neighborX = x + 1;
                    neighborY = y - 1;
                    break;
                case Direction.East:
                    neighborX = x + 1;
                    neighborY = y;
                    break;
                case Direction.SouthEast:
                    neighborX = x + 1;
                    neighborY = y + 1;
                    break;
                case Direction.South:
                    neighborX = x;
                    neighborY = y + 1;
                    break;
                case Direction.SouthWest:
                    neighborX = x - 1;
                    neighborY = y + 1;
                    break;
                case Direction.West:
                    neighborX = x - 1;
                    neighborY = y;
                    break;
            }

            if (neighborX >= 0 && neighborX <= this.Size.Width - 1)
            {
                if (neighborY >= 0 && neighborY <= this.Size.Height - 1)
                {
                    return (this.Pixels[neighborX, neighborY]);
                }
            }

            return (-1);
        }

        public List<int> GetAllNeighbors(int x, int y)
        {
            //find all neighbors
            List<int> allNeighbors = new List<int>();
            allNeighbors.Add(this.Pixels[x, y]);
            allNeighbors.Add(GetNeighbor(x, y, Direction.NorthWest));
            allNeighbors.Add(GetNeighbor(x, y, Direction.North));
            allNeighbors.Add(GetNeighbor(x, y, Direction.NorthEast));
            allNeighbors.Add(GetNeighbor(x, y, Direction.East));
            allNeighbors.Add(GetNeighbor(x, y, Direction.SouthEast));
            allNeighbors.Add(GetNeighbor(x, y, Direction.South));
            allNeighbors.Add(GetNeighbor(x, y, Direction.SouthWest));
            allNeighbors.Add(GetNeighbor(x, y, Direction.West));

            //return valid-neighbors
            List<int> validNeighbors = new List<int>();
            validNeighbors.AddRange(from neighborPixel in allNeighbors
                                    where neighborPixel != -1
                                    select neighborPixel);
            return (validNeighbors);
        }

        #endregion

        #region Static-Methods

        public static PGM ReadFromBitmap(Bitmap bmp)
        {
            PGM pgm = new PGM(bmp.Width, bmp.Height);

            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    int rgb = bmp.GetPixel(x, y).ToArgb();
                    int red = rgb & 255; //get 4th byte
                    int green = (rgb & 65280) / 256; //get 3rd byte
                    int blue = (rgb & 16711680) / 65536; //get 2nd byte, 1st byte is alpha
                    int gray = (red + green + blue) / 3; //IBA - inter-band-average
                    pgm.Pixels[x, y] = (short)gray;
                }
            }

            return (pgm);
        }

        public static void DrawToGraphics(PGM pgm, Graphics g)
        {
          for (int x = 0; x < pgm.Size.Width; x++) 
            {
                for (int y = 0; y < pgm.Size.Height; y++)
                {
                    int gray = pgm.Pixels[x, y];
                    Pen pen = new Pen(Color.FromArgb(gray, gray, gray), 1);
                    g.DrawRectangle(pen, x, y, 1, 1);
                }
            }
           
        }

        public static Bitmap CreateBitmap(PGM pgm)
        {
            Bitmap bmp = new Bitmap(pgm.Size.Width, pgm.Size.Height, PixelFormat.Format24bppRgb);
            Graphics g = Graphics.FromImage(bmp);
            DrawToGraphics(pgm, g);
            return (bmp);
        }         
        #endregion
    }
}
