using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using DicomImageViewer;
using System.Threading;
using MedicalCore;


namespace MedicalCore
{
    public partial class frmMainMenu : Form
    {
        //short[,,] inputSlices3D16;
        //short[,] inputSlices2D16;


        Thread wait;
        Thread doing2;

        DicomImageViewer.dicomReader objDicomReader;
        DicomImageViewer.convertToBitmap objConvertToBitmap;
        DicomImageViewer.Thresholding objThresholding;
        
        DicomImageViewer.Annotate objAnnotation;


        short[, ,] inputSlices3D16;
        byte[, ,] inputSlices3D8;
        byte[, ,] inputSlices3D24;

        short[, ,] outputSlices3D16;
        byte[, ,] outputSlices3D24;
        byte[, ,] outputSlices3D8;

        short[,] inputSlices2D16;
        byte[,] inputSlices2D8;
        byte[,] inputSlices2D24;

        short[,] outputSlices2D16;
        byte[,] outputSlices2D8;
        byte[,] outputSlices2D24;


        double[,] ROI_Features;
        int numberOfSlices = 0;
        int sliceNumber = -1;
        int minValue2D,maxValue2D, minValue3D, maxValue3D;
        int speedOfMovement;               
        int zoomRate = 1;      
        bool inAnnotationModeFlag = false;  // if this flag is true, it means that we are in annotation mode.
        bool inpanModeFlag = false;  // if this flag is true, it means that we are in pan mode.        
        int oldx=0, oldy=0;       
        Point startPoint = Point.Empty;        
        int dist_x = 0, dist_y = 0;
        int first_x = 0, first_y = 0;
        int movex = 0, movey = 0;
        Bitmap bmpZoomedPaned, bmpOriginal, bmpZoomedPanedOutput, bmpOriginalOutput;
        Graphics tmpGraphic;
        int[] numberOfROIsInSlice;        
        bool dicomImageType = false;
        int imageType = 0; // 1=dicom 2= pgm 3=bitmap
        int numberOfBitsAllocated = 0;
        int numberOfSamplesPerPixel = 0;

       
        string path_data = "e:";
        string Database_Features = "DataBase_Features.txt";
        string Init_Weights = "Init_Weights.txt";
        int numberofFeature = 24;

        bool existInputSlice;
        bool existOutputSlice;

        //int Filetype = 3;  // 3==PGM2D   1==DICOM2D
        public int imageProcessingType = new int();

      

      
     

        public frmMainMenu()
        {
            InitializeComponent();
            
        }

        private void drawRectangle()
        {
            pictureBoxZoomPan.Image = bmpOriginal;
            Bitmap tempBmp = new Bitmap(pictureBoxZoomPan.Image);
            Graphics gr = Graphics.FromImage(tempBmp);
            int border = 20 / zoomRate;
            if (border <= 5)
                border = 5;
            Pen p = new Pen(Color.Red, border);
            int x1 = movex ;
            int y1 = movey ;
            int x2 = pictureBox1.Width/zoomRate ;
            int y2 = pictureBox1.Height/zoomRate ;
            gr.DrawRectangle(p, new Rectangle(x1, y1, x2, y2));
            pictureBoxZoomPan.Image = tempBmp;                          
        }
       
        private void resetZoomPanParameters()
        {
            //if(bmpOriginal!=null )
            //     bmpZoomedPaned = new Bitmap(bmpOriginal);
            zoomRate = 1;
            lblZoomRate.Text = "1";
            movex = 0;
            movey = 0;
            oldx = 0;
            oldy = 0;            
        }
        private void showInPicturebox1(int argSliceNumber,int argWinCenter, int argWinWidth)
        {            
            if (imageType == 1)
            {
                Cursor.Current = Cursors.WaitCursor;
                for (int i = 0; i < inputSlices2D16.GetLength(0); i++)
                    for (int j = 0; j < inputSlices2D16.GetLength(1); j++)
                        inputSlices2D16[i, j] = inputSlices3D16[argSliceNumber, i, j];                                 
                objConvertToBitmap = new DicomImageViewer.convertToBitmap();
                pictureBox1.Image = objConvertToBitmap.convertToBitmap16Bit(inputSlices2D16, argWinCenter, argWinWidth);
                bmpOriginal = new Bitmap(pictureBox1.Image);
                lblSliceNumber.Text = Convert.ToString(argSliceNumber);                              
                bmpZoomedPaned = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                tmpGraphic = Graphics.FromImage(bmpZoomedPaned);
                tmpGraphic.DrawImage(bmpOriginal, new Rectangle(0, 0, bmpZoomedPaned.Width, bmpZoomedPaned.Height),
                                       new Rectangle(movex , movey, pictureBox1.Width / zoomRate, pictureBox1.Height / zoomRate), GraphicsUnit.Pixel);

                pictureBox1.Height = 600;
                pictureBox1.Width = 600;
                if (bmpOriginal.Height < pictureBox1.Height)
                    pictureBox1.Height = bmpOriginal.Height;
                if (bmpOriginal.Width < pictureBox1.Width)
                    pictureBox1.Width = bmpOriginal.Width;

                pictureBox1.Image = bmpZoomedPaned;
                drawRectangle();
                drawROI();
                Cursor.Current = Cursors.Default;  
            }
            
        }
        private void showInPicturebox2(int argSliceNumber)
        {
            if (imageProcessingType== 2 ) //3D  image
            {
                Cursor.Current = Cursors.WaitCursor;

                if (outputSlices2D16 == null)
                    outputSlices2D16 = new short[inputSlices2D16.GetLength(0), inputSlices2D16.GetLength(1)];

                if (numberOfSlices > 1)
                {
                    for (int i = 0; i < inputSlices2D16.GetLength(0); i++)
                        for (int j = 0; j < inputSlices2D16.GetLength(1); j++)
                            outputSlices2D16[i, j] = outputSlices3D16[argSliceNumber, i, j];
                }
                objConvertToBitmap = new DicomImageViewer.convertToBitmap();
                pictureBox2.Image = objConvertToBitmap.convertToBitmap16Bit(outputSlices2D16);
                bmpOriginalOutput = new Bitmap(pictureBox2.Image);
                lblSliceNumber.Text = Convert.ToString(argSliceNumber);
                bmpZoomedPanedOutput = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                tmpGraphic = Graphics.FromImage(bmpZoomedPanedOutput);
                tmpGraphic.DrawImage(bmpOriginalOutput, new Rectangle(0, 0, bmpZoomedPanedOutput.Width, bmpZoomedPanedOutput.Height),
                                       new Rectangle(movex, movey, pictureBox1.Width / zoomRate, pictureBox1.Height / zoomRate), GraphicsUnit.Pixel);
                pictureBox2.Image = bmpZoomedPanedOutput;                
                Cursor.Current = Cursors.Default;
            }
            else
                if(imageProcessingType == 1)
                {
                Cursor.Current = Cursors.WaitCursor;
                objConvertToBitmap = new DicomImageViewer.convertToBitmap();
                pictureBox2.Image = objConvertToBitmap.convertToBitmap16Bit(outputSlices2D16);
                bmpOriginalOutput = new Bitmap(pictureBox2.Image);
                lblSliceNumber.Text = Convert.ToString(argSliceNumber);
                bmpZoomedPanedOutput = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                tmpGraphic = Graphics.FromImage(bmpZoomedPanedOutput);
                tmpGraphic.DrawImage(bmpOriginalOutput, new Rectangle(0, 0, bmpZoomedPanedOutput.Width, bmpZoomedPanedOutput.Height),
                                       new Rectangle(movex, movey, pictureBox1.Width / zoomRate, pictureBox1.Height / zoomRate), GraphicsUnit.Pixel);
                pictureBox2.Image = bmpZoomedPanedOutput;
                Cursor.Current = Cursors.Default;
             }
            
        }

        private void showInPicturebox2()
        {
                if (imageProcessingType == 1 && existOutputSlice)
                {
                    Cursor.Current = Cursors.WaitCursor;
                    objConvertToBitmap = new DicomImageViewer.convertToBitmap();
                    pictureBox2.Image = objConvertToBitmap.convertToBitmap16Bit(outputSlices2D16);
                    bmpOriginalOutput = new Bitmap(pictureBox2.Image);
                   // lblSliceNumber.Text = Convert.ToString(argSliceNumber);
                    bmpZoomedPanedOutput = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                    tmpGraphic = Graphics.FromImage(bmpZoomedPanedOutput);
                    tmpGraphic.DrawImage(bmpOriginalOutput, new Rectangle(0, 0, bmpZoomedPanedOutput.Width, bmpZoomedPanedOutput.Height),
                                           new Rectangle(movex, movey, pictureBox1.Width / zoomRate, pictureBox1.Height / zoomRate), GraphicsUnit.Pixel);
                    pictureBox2.Image = bmpZoomedPanedOutput;
                    Cursor.Current = Cursors.Default;
                }

        }
        private void showInPicturebox1(int argWinCenter, int argWinWidth)
        {
            
            if (imageType == 1 || imageType == 2 || imageType == 3)
            {
                Cursor.Current = Cursors.WaitCursor;
                objConvertToBitmap = new DicomImageViewer.convertToBitmap();
                pictureBox1.Image = objConvertToBitmap.convertToBitmap16Bit(inputSlices2D16, argWinCenter, argWinWidth);
                bmpOriginal = new Bitmap(pictureBox1.Image);                                            
                bmpZoomedPaned = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                tmpGraphic = Graphics.FromImage(bmpZoomedPaned);
                tmpGraphic.DrawImage(bmpOriginal, new Rectangle(0, 0, bmpZoomedPaned.Width, bmpZoomedPaned.Height),
                                       new Rectangle(movex, movey, pictureBox1.Width / zoomRate, pictureBox1.Height / zoomRate), GraphicsUnit.Pixel);
                pictureBox1.Image = bmpZoomedPaned;
                drawRectangle();
                drawROI();
                pictureBox1.Height = 600;
                pictureBox1.Width = 600;
                if (bmpOriginal.Height < pictureBox1.Height)
                    pictureBox1.Height = bmpOriginal.Height;
                if (bmpOriginal.Width < pictureBox1.Width)
                    pictureBox1.Width = bmpOriginal.Width;
                Cursor.Current = Cursors.Default;
            }
            //else
            //{
            //    Cursor.Current = Cursors.WaitCursor;
            //    objConvertToBitmap = new DicomImageViewer.convertToBitmap();
            //    pictureBox1.Image = objConvertToBitmap.convertToBitmap16Bit(inputSlices2D16);
            //    bmpOriginal = new Bitmap(pictureBox1.Image);
            //    pictureBox1.Height = 600;
            //    pictureBox1.Width = 600;
            //    bmpZoomedPaned = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            //    tmpGraphic = Graphics.FromImage(bmpZoomedPaned);
            //    tmpGraphic.DrawImage(bmpOriginal, new Rectangle(0, 0, bmpZoomedPaned.Width, bmpZoomedPaned.Height),
            //                           new Rectangle(movex, movey, pictureBox1.Width / zoomRate, pictureBox1.Height / zoomRate), GraphicsUnit.Pixel);
            //    pictureBox1.Image = bmpZoomedPaned;
            //    drawRectangle();
            //    drawROI();                
            //    if (bmpOriginal.Height < pictureBox1.Height)
            //        pictureBox1.Height = bmpOriginal.Height;
            //    if (bmpOriginal.Width < pictureBox1.Width)
            //        pictureBox1.Width = bmpOriginal.Width;
            //    Cursor.Current = Cursors.Default;
            //}
           
        }
        private void drawROI()
        {
            if ((objAnnotation != null) && (numberOfROIsInSlice[sliceNumber] >= 0))
            {
                bmpZoomedPaned = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                tmpGraphic = Graphics.FromImage(bmpZoomedPaned);
                tmpGraphic.DrawImage(bmpOriginal, new Rectangle(0, 0, bmpZoomedPaned.Width, bmpZoomedPaned.Height), new Rectangle(movex, movey, pictureBox1.Width / zoomRate, pictureBox1.Height / zoomRate), GraphicsUnit.Pixel);              
                for (int t = 0; t <= numberOfROIsInSlice[sliceNumber]; t++)
                    for (int i = 0; i < objAnnotation.roisBoundryPointsList[sliceNumber, t].Count - 1; i++)
                    {
                        tmpGraphic.DrawLine(Pens.Red, ((objAnnotation.roisBoundryPointsList[sliceNumber, t][i].X - (movex)) * zoomRate), 
                            ((objAnnotation.roisBoundryPointsList[sliceNumber, t][i].Y - (movey)) * zoomRate),
                            ((objAnnotation.roisBoundryPointsList[sliceNumber, t][i + 1].X - (movex)) * zoomRate),
                            ((objAnnotation.roisBoundryPointsList[sliceNumber, t][i + 1].Y - (movey)) * zoomRate));
                    }
                pictureBox1.Image = bmpZoomedPaned;
            }
        }
        private int FindInstanceNumber(List<string> dic_info)
        {
            int instanceNumber = 0;
            foreach (string s in dic_info)
            {
                if (s.Contains("Instance Number"))
                {
                    instanceNumber = Convert.ToInt32(s.Substring(s.IndexOf(':') + 1, s.Length - s.IndexOf(':') - 1));
                    return instanceNumber;
                }
            }
            return -1;
        }
        private void openDICOM3DToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Dicom Images (*.dcm)|*.dcm";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                existInputSlice = true;
                existOutputSlice = false;
                Cursor.Current = Cursors.WaitCursor;               
                imageProcessingType = 2;
                imageType = 1;
                minValue3D = int.MaxValue;
                maxValue3D = int.MinValue;
                objDicomReader = new DicomImageViewer.dicomReader(openFileDialog.FileName);

                numberOfBitsAllocated = objDicomReader.bitsAllocated;
                numberOfSamplesPerPixel = objDicomReader.samplesPerPixel;


                if (numberOfSamplesPerPixel == 1 && numberOfBitsAllocated == 16)
                {
                    numberOfSlices = objDicomReader.filesPaths.Length;
                    inputSlices3D16 = new short[numberOfSlices, objDicomReader.width, objDicomReader.height];
                    numberOfROIsInSlice = new int[numberOfSlices];//This var determines the number of ROI in each slice. every entry is the number of ROIs for the slice with that index
                    for (int i = 0; i < numberOfSlices; i++)
                        numberOfROIsInSlice[i] = -1;
                    objAnnotation = new Annotate(numberOfSlices);              
                    Parallel.ForEach(objDicomReader.filesPaths, currentfile =>
                           {
                               DicomImageViewer.dicomReader objDicomTemp = new DicomImageViewer.dicomReader(currentfile);
                               objDicomTemp.readPixels(currentfile);
                               short[,] tmpArray2D = objDicomTemp.pixels16Array2D;
                               if (minValue2D > objDicomTemp.minValue)
                                   minValue2D = objDicomTemp.minValue;
                               if (maxValue2D < objDicomTemp.maxValue)
                                   maxValue2D = objDicomTemp.maxValue;

                               if (minValue2D < minValue3D)
                                   minValue3D = minValue2D;
                               if (maxValue2D > maxValue3D)
                                   maxValue3D = maxValue2D;
                               int instanceNumber = FindInstanceNumber(objDicomTemp.Dic_Info) - 1;
                               int row = objDicomReader.height;
                                int col = objDicomReader.width;
                               for(int i=0; i<row ; i++)
                                   for(int j=0 ; j<col ; j++)
                                       inputSlices3D16[instanceNumber, i, j] = tmpArray2D[i, j];                               
                           }
                    );
                    inputSlices2D16 = new short[objDicomReader.width, objDicomReader.height];
                    sliceNumber = 0;

                    ////////////////////////////

                    int winMin = minValue3D;
                    int winMax = maxValue3D;

                    int winCenter = (winMin + ((winMax - winMin) / 2));
                    trackBarWinCenter.Invoke(new MethodInvoker(delegate { trackBarWinCenter.Maximum = winMax; trackBarWinCenter.Minimum = winMin; trackBarWinCenter.Value = winCenter; }), null);

                    int winWidth = (winMax - winMin) / 2 + 5;
                    trackBarWinWidth.Invoke(new MethodInvoker(delegate { trackBarWinWidth.Minimum = 1; ; trackBarWinWidth.Maximum = winMax - winMin + 10; trackBarWinWidth.Value = winWidth; }), null);
                    lblWindowCenter.Invoke(new MethodInvoker(delegate { lblWindowCenter.Text = trackBarWinCenter.Value.ToString(); }), null);
                    lblWindowWidth.Invoke(new MethodInvoker(delegate { lblWindowWidth.Text = trackBarWinWidth.Value.ToString(); }), null);
                    ///////////////////////////
                    resetZoomPanParameters();
                    showInPicturebox1(sliceNumber,winCenter,winWidth);

                }
                if (numberOfSamplesPerPixel == 1 && numberOfBitsAllocated == 8)
                {
                    numberOfSlices = objDicomReader.filesPaths.Length;
                    inputSlices3D16 = new short[numberOfSlices, objDicomReader.width, objDicomReader.height];
                    numberOfROIsInSlice = new int[numberOfSlices];
                    for (int i = 0; i < numberOfSlices; i++)
                        numberOfROIsInSlice[i] = -1;
                    objAnnotation = new Annotate(numberOfSlices);
                    Parallel.ForEach(objDicomReader.filesPaths, currentfile =>
                           {
                               DicomImageViewer.dicomReader obj_dicom_temp = new DicomImageViewer.dicomReader(currentfile);
                               obj_dicom_temp.readPixels(currentfile);                                                              
                               byte  [,] temp1 = obj_dicom_temp.pixels8Array2D;
                               minValue2D = obj_dicom_temp.minValue;
                               maxValue2D = obj_dicom_temp.maxValue;
                               if (minValue2D < minValue3D)
                                   minValue3D = minValue2D;
                               if (maxValue2D > maxValue3D)
                                   maxValue3D = maxValue2D;
                               int instanceNumber = FindInstanceNumber(obj_dicom_temp.Dic_Info) - 1;
                               int row = objDicomReader.height;
                               int col = objDicomReader.width;
                               for (int i = 0; i < row; i++)
                                   for (int j = 0; j < col; j++)
                                       inputSlices3D16[instanceNumber, i, j] = temp1[i, j];     
                           }
                           );
                    int winMin = minValue3D;
                    int winMax = maxValue3D;

                    int winCenter = (winMin + ((winMax - winMin) / 2));
                    trackBarWinCenter.Invoke(new MethodInvoker(delegate { trackBarWinCenter.Maximum = winMax; trackBarWinCenter.Minimum = winMin; trackBarWinCenter.Value = winCenter; }), null);

                    int winWidth = (winMax - winMin) / 2 + 5;
                    trackBarWinWidth.Invoke(new MethodInvoker(delegate { trackBarWinWidth.Minimum = 1; ; trackBarWinWidth.Maximum = winMax - winMin + 10; trackBarWinWidth.Value = winWidth; }), null);
                    lblWindowCenter.Invoke(new MethodInvoker(delegate { lblWindowCenter.Text = trackBarWinCenter.Value.ToString(); }), null);
                    lblWindowWidth.Invoke(new MethodInvoker(delegate { lblWindowWidth.Text = trackBarWinWidth.Value.ToString(); }), null);

                    inputSlices2D16 = new short[objDicomReader.width, objDicomReader.height];
                    sliceNumber = 0;
                    resetZoomPanParameters();
                    showInPicturebox1(sliceNumber,trackBarWinCenter.Value,trackBarWinWidth.Value);
              }              
              Cursor.Current = Cursors.Default;              
              speedOfMovement = 20;
              showROIToolStripMenuItem.Enabled = false;

              trackBarSliceNumber.Invoke(new MethodInvoker(delegate { trackBarSliceNumber.Value = 0; }), null);
              trackBarSliceNumber.Invoke(new MethodInvoker(delegate { trackBarSliceNumber.Maximum = objDicomReader.filesPaths.Length - 1; }), null);
              trackBarSliceNumber.Invoke(new MethodInvoker(delegate { trackBarSliceNumber.Enabled = true; }), null);                
            }

            lblTotalNumberOfSlices.Text = Convert.ToString(numberOfSlices);                              

        }

    private void annotationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if ((inputSlices2D16 == null)) return;
            if (inAnnotationModeFlag)
            {
                inAnnotationModeFlag = false;
                annotationToolStripMenuItem.Checked = false;
                pictureBox1.Cursor = Cursors.Default;
            }
            else
            {
                pictureBox1.Cursor = Cursors.Cross;
                inAnnotationModeFlag = true;
                annotationToolStripMenuItem.Checked = true;
            }
        }
       
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (inputSlices2D16 == null) return;
            first_x = Convert.ToInt16(movex + e.Location.X / zoomRate);
            first_y = Convert.ToInt16(movey + e.Location.Y / zoomRate);
            oldx = e.X;
            oldy = e.Y;
            if ((!inAnnotationModeFlag) && (e.Button == MouseButtons.Left))   // pan mode active 
            {
                inpanModeFlag = true;
                startPoint = new Point(e.Location.X, e.Location.Y); // start point is first point for calculate distance.
                pictureBox1.Cursor = Cursors.Hand;
            }
            if ((inAnnotationModeFlag) && (e.Button == MouseButtons.Left)) // annotation mode active
            {
                numberOfROIsInSlice[sliceNumber]++;
                objAnnotation.make_new_ROI(sliceNumber, numberOfROIsInSlice[sliceNumber]);
                objAnnotation.AddboundryPoint(sliceNumber, numberOfROIsInSlice[sliceNumber], first_x, first_y);
                bmpZoomedPaned = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                tmpGraphic = Graphics.FromImage(bmpZoomedPaned);
                tmpGraphic.DrawImage(bmpOriginal, new Rectangle(0, 0, bmpZoomedPaned.Width, bmpZoomedPaned.Height),
                    new Rectangle(movex, movey, pictureBox1.Width / zoomRate, pictureBox1.Height / zoomRate), GraphicsUnit.Pixel);
                tmpGraphic.DrawLine(Pens.Red, oldx, oldy, e.X, e.Y);              
                pictureBox1.Image = bmpZoomedPaned;
                
            }  
            
               
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {            
            if ((inpanModeFlag) && (e.Button == MouseButtons.Left))
            {
                inpanModeFlag = false;
                drawRectangle();
                pictureBox1.Cursor = Cursors.Default;
                drawROI();

                if (existOutputSlice)
                    showInPicturebox2(sliceNumber);

            }
            if ((inAnnotationModeFlag) && (e.Button == MouseButtons.Left) && ((inputSlices2D16 != null)))
            {
                inAnnotationModeFlag = false;
                objAnnotation.CloseROI(sliceNumber, numberOfROIsInSlice[sliceNumber]);
                pictureBox1.Cursor = Cursors.Default;
                annotationToolStripMenuItem.Checked = false;
                deleteAnnotationToolStripMenuItem.Enabled = true;
                drawROI();
            }
        }
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {

            Point p2 = new Point(Convert.ToInt16(movex + e.Location.X / zoomRate), Convert.ToInt16(movey + e.Location.Y / zoomRate)); // p2 is real point in main image 
            if ((inputSlices2D16 != null) && (p2.X >= 0) && (p2.Y >= 0) && (p2.X < inputSlices2D16.GetLength(0)) && (p2.Y < inputSlices2D16.GetLength(1)))
            {

                toolTip1.ToolTipTitle = "(" + (p2.X).ToString() + "," + ((p2.Y)).ToString() + "), value= " + inputSlices2D16[p2.X, p2.Y].ToString();
                
            }
          if (inpanModeFlag)  //  pan mode
            {
                dist_x = (startPoint.X - e.Location.X) / speedOfMovement;  // speedOfMovement is threshold for speed move 
                dist_y = (startPoint.Y - e.Location.Y) / speedOfMovement;
                bmpZoomedPaned  = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                tmpGraphic = Graphics.FromImage(bmpZoomedPaned);
                if ((movex + dist_x >= 0) && (movey + dist_y >= 0) && (movex + dist_x < bmpOriginal.Width - pictureBox1.Width / zoomRate) && 
                    (movey + dist_y < bmpOriginal.Height - pictureBox1.Height / zoomRate))
                {                    
                    tmpGraphic.DrawImage(bmpOriginal, new Rectangle(0, 0, bmpZoomedPaned.Width, bmpZoomedPaned.Height),
                                       new Rectangle(movex + dist_x, movey + dist_y, pictureBox1.Width / zoomRate, pictureBox1.Height / zoomRate), GraphicsUnit.Pixel);
                    pictureBox1.Image = bmpZoomedPaned;
                    movex = movex + dist_x;
                    movey = movey + dist_y;
                 
                }
            }
            if ((inAnnotationModeFlag) && (e.Button == MouseButtons.Left) && (inputSlices2D16 != null))
            {
                tmpGraphic = Graphics.FromImage(bmpZoomedPaned);
                tmpGraphic.DrawLine(Pens.Red, oldx, oldy, e.X, e.Y);
                pictureBox1.Image = bmpZoomedPaned;
                int tx = Convert.ToInt16(e.X / zoomRate + movex);
                int ty = Convert.ToInt16(e.Y / zoomRate + movey);
                objAnnotation.AddboundryPoint(sliceNumber, numberOfROIsInSlice[sliceNumber], tx, ty);               
                oldx = e.X;
                oldy = e.Y;
            }     
       
        }

        private void getROIsRegionPointsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (objAnnotation != null)
            {
                Cursor.Current = Cursors.WaitCursor;
                for (int i = 0; i <= numberOfROIsInSlice[sliceNumber]; i++)
                    objAnnotation.makeListOfBoundaryAndRegionPoints(sliceNumber,i);
                showROIToolStripMenuItem.Enabled = true;
                
                Cursor.Current = Cursors.Default;
            }
        }

        private void showROIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (objAnnotation != null)
            {
                if (numberOfROIsInSlice[sliceNumber] >= 0)
                {
                    Bitmap tempbmp = new Bitmap(bmpZoomedPaned);                                
                    for (int tmpROINumber = 0; tmpROINumber <= numberOfROIsInSlice[sliceNumber]; tmpROINumber++)
                        for (int i = 0; i < objAnnotation.roisRegionPointsList[sliceNumber, tmpROINumber].Count - 1; i++)
                        {
                            int x=(objAnnotation.roisRegionPointsList[sliceNumber, tmpROINumber][i].X - (movex)) * zoomRate;
                            int y=(objAnnotation.roisRegionPointsList[sliceNumber, tmpROINumber][i].Y - (movey)) * zoomRate;
                            if (x < tempbmp.Width && y < tempbmp.Height && x >= 0 && y >= 0)
                                tempbmp.SetPixel(x, y, Color.Yellow);                 
                        }
                    pictureBox1.Image = tempbmp;
                }
            }
        }
        
      
     
        private void saveROIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (inputSlices3D16 == null)
                return;
            SaveFileDialog objSaveFileDialog = new SaveFileDialog();
            objSaveFileDialog.Filter = "ROI (*.roi)|*.roi"; int x, y;
            if (objSaveFileDialog.ShowDialog() == DialogResult.OK)
            {
                StreamWriter sw = new StreamWriter(objSaveFileDialog.FileName);
                for (int slc = 0; slc < numberOfSlices; slc++)
                {
                    for (int t = 0; t <= numberOfROIsInSlice[slc]; t++)
                    {
                        sw.Write('*');
                        sw.WriteLine();
                        for (int i = 0; i < objAnnotation.roisBoundryPointsList[slc, t].Count; i++)
                        {
                            x = objAnnotation.roisBoundryPointsList[slc, t][i].X;
                            y = objAnnotation.roisBoundryPointsList[slc, t][i].Y;
                            sw.Write(Convert.ToString(x) + ',' + Convert.ToString(y));
                            sw.WriteLine();
                        }                       
                    }
                    sw.Write('#');
                    sw.WriteLine();
                }                
                sw.Close();
            }
        }    
    
   private void deleteAnnotationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if ((objAnnotation != null) && (numberOfROIsInSlice[sliceNumber] >= 0))
            {
                objAnnotation.ClearAnnotate(sliceNumber);
                numberOfROIsInSlice[sliceNumber] = -1;
                
                
                
            }
            showInPicturebox1(sliceNumber,trackBarWinCenter.Value, trackBarWinWidth.Value);           
        }
   private void deleteAllAnnotationToolStripMenuItem_Click(object sender, EventArgs e)
   {
       if ((objAnnotation != null))
       {
           for (int slc = 0; slc < numberOfSlices; slc++)
           {
               objAnnotation.ClearAnnotate(slc);
               numberOfROIsInSlice[slc] = -1;
           }
            
       }
       showInPicturebox1(sliceNumber, trackBarWinCenter.Value, trackBarWinWidth.Value);      
   }
   private void loadBoundryROIToolStripMenuItem_Click(object sender, EventArgs e)  // Load Boubdry from File
   {
       if (inputSlices3D16 == null)
           return;
       OpenFileDialog openFiledialog = new OpenFileDialog();
       string LINE;
       openFiledialog.Filter = "ROI (*.roi)|*.roi";
       if (openFiledialog.ShowDialog() == DialogResult.OK)
       {
           deleteAllAnnotationToolStripMenuItem_Click(sender,e );                      
           StreamReader sr = new StreamReader(openFiledialog.FileName);
           int tmpSliceNumber = 0;
           LINE = sr.ReadLine();
           while (true)
           {
               if (LINE == "#")
                   tmpSliceNumber++;
               else
               if (LINE == "*")
               {
                   numberOfROIsInSlice[tmpSliceNumber]++;
                   objAnnotation.make_new_ROI(tmpSliceNumber, numberOfROIsInSlice[tmpSliceNumber]);
               }                               
               else
               {
                   string t1 = LINE.Substring(0, LINE.IndexOf(','));
                   string t2 = LINE.Substring(LINE.IndexOf(',') + 1);
                   objAnnotation.AddboundryPoint(tmpSliceNumber, numberOfROIsInSlice[tmpSliceNumber], Convert.ToInt16(t1), Convert.ToInt16(t2));
               }
               LINE = sr.ReadLine();
               if (LINE == null) break;
           }
           sr.Close();
           drawROI();
       }
   }

        private void openDICOM2DToolStripMenuItem_Click(object sender, EventArgs e)
   {

       existInputSlice = true;
       existOutputSlice = false;
       OpenFileDialog openFileDialog = new OpenFileDialog();
       openFileDialog.Filter = "Dicom Images (*.dcm)|*.dcm";
       if (openFileDialog.ShowDialog() == DialogResult.OK)
       {
           Cursor.Current = Cursors.WaitCursor;
           imageProcessingType = 1;
           imageType = 1; //dicom 
           numberOfSlices = 1;
           sliceNumber = 0;
           numberOfROIsInSlice = new int[1];
           numberOfROIsInSlice[0] = -1;
           objAnnotation = new Annotate(numberOfSlices);
           objDicomReader = new DicomImageViewer.dicomReader(openFileDialog.FileName);
           inputSlices2D16 = objDicomReader.pixels16Array2D;

           numberOfBitsAllocated = objDicomReader.bitsAllocated;
           numberOfSamplesPerPixel = objDicomReader.samplesPerPixel;

           minValue2D = objDicomReader.minValue;
           maxValue2D = objDicomReader.maxValue;


           //Some operations are common between 2D and 3D image(Forexample deleting annotation in current slice).
           //Because of this, we put the 2D slice in 3D array and in its first location.
           inputSlices3D16 = new short[1, objDicomReader.width, objDicomReader.height];
           int row = objDicomReader.height;
           int col = objDicomReader.width;
           for (int i = 0; i < col; i++)
               for (int j = 0; j < row; j++)
                   inputSlices3D16[0, i, j] = inputSlices2D16[i, j];
           // To here - Some op......


           int winMin = minValue2D;
           int winMax = maxValue2D;

           int winCenter = (winMin + ((winMax - winMin) / 2));
           trackBarWinCenter.Invoke(new MethodInvoker(delegate { trackBarWinCenter.Maximum = winMax; trackBarWinCenter.Minimum = winMin; trackBarWinCenter.Value = winCenter; }), null);

           int winWidth = (winMax - winMin) / 2 + 5;
           trackBarWinWidth.Invoke(new MethodInvoker(delegate { trackBarWinWidth.Minimum = 1; ; trackBarWinWidth.Maximum = winMax - winMin + 10; trackBarWinWidth.Value = winWidth; }), null);
           lblWindowCenter.Invoke(new MethodInvoker(delegate { lblWindowCenter.Text = trackBarWinCenter.Value.ToString(); }), null);
           lblWindowWidth.Invoke(new MethodInvoker(delegate { lblWindowWidth.Text = trackBarWinWidth.Value.ToString(); }), null);
           ///////////////////////////
                      
           objConvertToBitmap = new DicomImageViewer.convertToBitmap();
           zoomRate = 1;
           movex = 0;
           movey = 0;
           showInPicturebox1(winCenter, winWidth);
           resetZoomPanParameters();
           speedOfMovement = 20;                      
           drawRectangle();          
           Cursor.Current = Cursors.Default;
           showROIToolStripMenuItem.Enabled = false;
           
       }
   }

      private void globalThresholdToolStripMenuItem_Click(object sender, EventArgs e)
      {

          existOutputSlice = true;
          objThresholding = new DicomImageViewer.Thresholding();

          

          if (imageProcessingType == 2)
          {
              var prompLst = new List<string> { "Enter T1 : ", "Enter T2 :" };
              var prompt = DicomImageViewer.Dialog.ShowPromptDialog(prompLst, "Enter value");
              if (numberOfSamplesPerPixel == 1 && numberOfBitsAllocated == 16)
              {
                  outputSlices3D16 = objThresholding.threshold3D(inputSlices3D16, Convert.ToInt32(prompt[0]), Convert.ToInt32(prompt[1]));
                  pictureBox2.Image = objConvertToBitmap.convertToBitmap16Bit(outputSlices3D16, sliceNumber, trackBarWinCenter.Value, trackBarWinWidth.Value);
              }

              if (numberOfSamplesPerPixel == 1 && numberOfBitsAllocated == 8)
              {
                  outputSlices3D8 = objThresholding.threshold3D(inputSlices3D8, Convert.ToInt32(prompt[0]), Convert.ToInt32(prompt[1]));
                  pictureBox2.Image = objConvertToBitmap.convertToBitmap8Bit(outputSlices3D8, sliceNumber, trackBarWinCenter.Value, trackBarWinWidth.Value);
              }

              if (numberOfSamplesPerPixel == 3 && numberOfBitsAllocated == 8)
              {
                  outputSlices3D24 = objThresholding.threshold3D(inputSlices3D24, Convert.ToInt32(prompt[0]), Convert.ToInt32(prompt[1]));
                  pictureBox2.Image = objConvertToBitmap.convertToBitmap24Bit(outputSlices3D24, sliceNumber, trackBarWinCenter.Value, trackBarWinWidth.Value);
              }

          }
          if (imageProcessingType == 1)
          {
              var prompLst = new List<string> { "Enter T1 : ", "Enter T2 :" };
              var prompt = DicomImageViewer.Dialog.ShowPromptDialog(prompLst, "Enter value");
              if (numberOfSamplesPerPixel == 1 && numberOfBitsAllocated == 16)
              {

                  existOutputSlice = true;

                  if (numberOfSamplesPerPixel == 1 && numberOfBitsAllocated == 16)
                  {
                      if (outputSlices2D16 == null)
                          outputSlices2D16 = new short[inputSlices2D16.GetLength(0), inputSlices2D16.GetLength(1)];
                      outputSlices2D16 = objThresholding.threshold2D(inputSlices2D16, Convert.ToInt32(prompt[0]), Convert.ToInt32(prompt[1])); ;

                      showInPicturebox2();
                  }
                  
                  
                  
                  //outputSlices2D16 = objThresholding.threshold2D(inputSlices2D16, Convert.ToInt32(prompt[0]), Convert.ToInt32(prompt[1]));
                  //pictureBox2.Image = objConvertToBitmap.convertToBitmap16Bit(outputSlices2D16);
              }

              if (numberOfSamplesPerPixel == 3 && numberOfBitsAllocated == 8)
              {
                  outputSlices2D24 = objThresholding.threshold2D(inputSlices2D24, Convert.ToInt32(prompt[0]), Convert.ToInt32(prompt[1]));
                  pictureBox2.Image = objConvertToBitmap.convertToBitmap24Bit(outputSlices2D24, trackBarWinCenter.Value, trackBarWinWidth.Value);
              }

              if (numberOfSamplesPerPixel == 1 && numberOfBitsAllocated == 8)
              {
                  outputSlices2D8 = objThresholding.threshold2D(inputSlices2D8, Convert.ToInt32(prompt[0]), Convert.ToInt32(prompt[1]));
                  pictureBox2.Image = objConvertToBitmap.convertToBitmap8Bit(outputSlices2D8, trackBarWinCenter.Value, trackBarWinWidth.Value);
              }
          }
          if(imageType ==2 && imageProcessingType == 1)
          {
              var prompLst = new List<string> { "Enter T1 : "};
              var prompt = DicomImageViewer.Dialog.ShowPromptDialog(prompLst, "Enter value");
              existOutputSlice =true;
              outputSlices2D16 = objThresholding.threshold2D(inputSlices2D16, Convert.ToInt32(prompt[0]));
              showInPicturebox2(sliceNumber);
          }
      }

      private void trackBarWinCenter_Scroll(object sender, EventArgs e)
      {
         
      }

      private void trackBarWinWidth_Scroll(object sender, EventArgs e)
      {
       
      }

     

      private void computeThresholdToolStripMenuItem_Click(object sender, EventArgs e)
      {
          int average = 0, finalThreshold = 0;
          objThresholding = new DicomImageViewer.Thresholding();
          if (imageProcessingType == 2)
          {
              var prompLst = new List<string> { "Enter threshold differece Limit : " };
              var promp = DicomImageViewer.Dialog.ShowPromptDialog(prompLst, "Enter value");

              int limit = 0;
              int.TryParse(promp[0], out limit);

              average = objThresholding.ComputeAverageIntensity3D(inputSlices3D16);
              finalThreshold = objThresholding.computeSingleThreshold3D(inputSlices3D16, average, limit);

              MessageBox.Show(finalThreshold.ToString());
          }
          else if (imageProcessingType == 1)
          {
              var prompLst = new List<string> { "Enter threshold differece Limit : " };
              var promp = DicomImageViewer.Dialog.ShowPromptDialog(prompLst, "Enter value");

              int limit = 0;
              int.TryParse(promp[0], out limit);

              average = objThresholding.ComputeAverageIntensity2D(inputSlices2D16);
              finalThreshold = objThresholding.computeSingleThreshold2D(inputSlices2D16, average, limit);
              MessageBox.Show(finalThreshold.ToString());
          }
      }

      private void adaptiveThresholdToolStripMenuItem_Click(object sender, EventArgs e)
      {
          objThresholding = new Thresholding();
          if (imageProcessingType == 1)
          {
              existOutputSlice = true;

              if (imageType == 1)//dicom type
              {
                  if (numberOfSamplesPerPixel == 1 && numberOfBitsAllocated == 16)
                  {
                      if (outputSlices2D16 == null)
                          outputSlices2D16 = new short[inputSlices2D16.GetLength(0), inputSlices2D16.GetLength(1)];
                      outputSlices2D16 = objThresholding.AdaptiveThreshold2D(inputSlices2D16);

                      showInPicturebox2();
                  }
              }
              if (imageType==2)//pgm type
              {
                  if (outputSlices2D16 == null)
                      outputSlices2D16 = new short[inputSlices2D16.GetLength(0), inputSlices2D16.GetLength(1)];
                  outputSlices2D16 = objThresholding.AdaptiveThreshold2D(inputSlices2D16);

                  showInPicturebox2();
              }

          }
          else if (imageProcessingType == 2)
          {
              existOutputSlice = true;
              outputSlices3D16 = objThresholding.AdaptiveThreshold3D(inputSlices3D16);
              pictureBox2.Image = objConvertToBitmap.convertToBitmap16Bit(outputSlices3D16, sliceNumber);
          }
      }

     
      private void btnNext_Click(object sender, EventArgs e)
      {
          if (sliceNumber < numberOfSlices - 1 && existInputSlice == true)
          {
              sliceNumber++;
              showInPicturebox1(sliceNumber, trackBarWinCenter.Value, trackBarWinWidth.Value);
              trackBarSliceNumber.Value = sliceNumber;
          }
          if (sliceNumber < numberOfSlices - 1 && existOutputSlice == true)
          {
              showInPicturebox2(sliceNumber);
              trackBarSliceNumber.Value = sliceNumber;
          }

          
      }

      private void btnPrevious_Click(object sender, EventArgs e)
      {
          if (sliceNumber > 0 && existInputSlice == true)
          {
              sliceNumber--;
              showInPicturebox1(sliceNumber, trackBarWinCenter.Value, trackBarWinWidth.Value);
              trackBarSliceNumber.Value = sliceNumber;
          }
          if (sliceNumber > 0  && existOutputSlice == true)
          {
              showInPicturebox2(sliceNumber);
              trackBarSliceNumber.Value = sliceNumber;
          }
        


      }

      private void btnZoomIn_Click(object sender, EventArgs e)
      {
          if (inputSlices2D16 != null)
          {
              zoomRate++;
              lblZoomRate.Text = Convert.ToString(zoomRate);
              showInPicturebox1(trackBarWinCenter.Value, trackBarWinWidth.Value);
              if (existOutputSlice)
                  showInPicturebox2(sliceNumber);
          }
      }

      private void btnZoomOut_Click(object sender, EventArgs e)
      {
          if (zoomRate > 1)
              {
                  zoomRate--;
                  lblZoomRate.Text = Convert.ToString(zoomRate);
                  if (zoomRate == 1) 
                      resetZoomPanParameters();                  
                  showInPicturebox1(trackBarWinCenter.Value, trackBarWinWidth.Value);
                  if (existOutputSlice)
                      showInPicturebox2(sliceNumber);
              }
      }

      private void dFilteringToolStripMenuItem_Click(object sender, EventArgs e)
      {
          Filtering filter = new Filtering();

          if (numberOfSamplesPerPixel == 1 && numberOfBitsAllocated == 16)
          {
              if (existInputSlice == false) return;
              existOutputSlice = true;
              Cursor.Current = Cursors.WaitCursor;

              if (imageProcessingType == 1)
              {

                  outputSlices2D16 = filter.filtering2D(inputSlices2D16, filter.kernel2D(3, 0.1));
                  showInPicturebox2(sliceNumber);
                  
                  //pictureBox2.Image = objConvertToBitmap.convertToBitmap16Bit( );

              }
              Cursor.Current = Cursors.Default;
          }

          if (numberOfSamplesPerPixel == 1 && numberOfBitsAllocated == 8)
          {
              pictureBox2.Image = objConvertToBitmap.convertToBitmap8Bit(
              filter.filtering2D(inputSlices2D8, filter.kernel2D(3, 0.1)));
          }

          if (numberOfSamplesPerPixel == 3 && numberOfBitsAllocated == 8)
          {
              pictureBox2.Image = objConvertToBitmap.convertToBitmap24Bit(
              filter.filtering2D(inputSlices2D24, filter.kernel2D(3, 0.1)));
          }
      }

      private void dFilteringToolStripMenuItem1_Click(object sender, EventArgs e)
      {
          existOutputSlice = true;
          Filtering filter = new Filtering();

          wait = new Thread(() =>
          {
              pictureBox2.Invoke(new MethodInvoker(delegate { pictureBox2.Image = DicomImageViewer.Properties.Resources.wait; }), null);
          });
          wait.Start();

          Thread doing = new Thread(() =>
          {
              if (numberOfSamplesPerPixel == 1 && numberOfBitsAllocated == 16)
              {
                  outputSlices3D16 = filter.filtering3D(inputSlices3D16, filter.kernel3D(3, 0.1));
                  pictureBox2.Image = objConvertToBitmap.convertToBitmap16Bit(outputSlices3D16, sliceNumber);
              }

              if (numberOfSamplesPerPixel == 3 && numberOfBitsAllocated == 8)
              {
                  outputSlices3D24 = filter.filtering3D(inputSlices3D24, filter.kernel3D(3, 0.1));
                  pictureBox2.Image = objConvertToBitmap.convertToBitmap24Bit(outputSlices3D24, sliceNumber);
              }


              if (numberOfSamplesPerPixel == 1 && numberOfBitsAllocated == 8)
              {
                  outputSlices3D8 = filter.filtering3D(inputSlices3D8, filter.kernel3D(3, 0.1));
                  pictureBox2.Image = objConvertToBitmap.convertToBitmap8Bit(outputSlices3D8, sliceNumber);
              }
              wait.Abort();
          });
          doing.Start();
      }

      private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
      {
          
      }

     
    

   

      private void setParametersToolStripMenuItem_Click(object sender, EventArgs e)
      {
        
      }

      private void getRegionOnImageToolStripMenuItem_Click(object sender, EventArgs e)
      {
      }

      private void toolStripMenuItem3_Click(object sender, EventArgs e)
      {

      }

    
      private void regionLabelingToolStripMenuItem_Click(object sender, EventArgs e)
      {
          
      }

      private void regionLabeling3DToolStripMenuItem_Click(object sender, EventArgs e)
      {
         
      }

      private void settingsToolStripMenuItem1_Click(object sender, EventArgs e)
      {
          
      }





      
      private void gToolStripMenuItem_Click(object sender, EventArgs e)
      {
         
      }

      private void harmonicMeanToolStripMenuItem_Click(object sender, EventArgs e)
      {
          
      }

      private void cantraharmonicMeanToolStripMenuItem_Click(object sender, EventArgs e)
      {
         
      }

      private void maxFilterToolStripMenuItem_Click(object sender, EventArgs e)
      {
         
      }

      private void minFilterToolStripMenuItem_Click(object sender, EventArgs e)
      {
        
      }

      private void arithmeticMeanToolStripMenuItem_Click(object sender, EventArgs e)
      {
         
      }

      private void geometricMeanToolStripMenuItem1_Click(object sender, EventArgs e)
      {
        
      }

      private void harmonicMeanToolStripMenuItem1_Click(object sender, EventArgs e)
      {
         
      }

      private void cantraHarmonicMeanToolStripMenuItem1_Click(object sender, EventArgs e)
      {
         
      }

      private void maxFilterToolStripMenuItem1_Click(object sender, EventArgs e)
      {
         
      }

     
     

     
    
    

     

      private void openBMPToolStripMenuItem_Click(object sender, EventArgs e)
      {
          openFileDialog1.Filter = "BMP|*bmp|JPG|*jpg|Gif|*gif|PNG|*png";
          objConvertToBitmap = new DicomImageViewer.convertToBitmap();
          if (openFileDialog1.ShowDialog() == DialogResult.OK)
          {
              imageType = 3; // Bitmap type
              Image bmpImage = Bitmap.FromFile(openFileDialog1.FileName);
              DicomImageViewer.BitmapOperations objBitmap = new BitmapOperations();
              byte[, ,] bmpArrayRGB = objBitmap.loadBmp((Bitmap)bmpImage);
              byte[,] bmpArrayGray = objBitmap.grayScale(bmpArrayRGB);
              inputSlices2D8 = bmpArrayGray;
              pictureBox1.Image = objConvertToBitmap.convertToBitmap8Bit(inputSlices2D8);
          }
      }

      private void saveToolStripMenuItem_Click(object sender, EventArgs e)
      {
          pictureBox1.Image.Save("d:\\c1.jpg");
      }

      
      

      private void trackBarSliceNumber_Scroll(object sender, EventArgs e)
      {
          //resetZoomSetting();
          sliceNumber = trackBarSliceNumber.Value;
          lblSliceNumber.Text = trackBarSliceNumber.Value.ToString();

          if (numberOfSamplesPerPixel == 1 && numberOfBitsAllocated == 16)
          {
              pictureBox1.Image = objConvertToBitmap.convertToBitmap16Bit(inputSlices3D16, sliceNumber, trackBarWinCenter.Value, trackBarWinWidth.Value);
          }

          if (numberOfSamplesPerPixel == 1 && numberOfBitsAllocated == 8)
          {
              pictureBox1.Image = objConvertToBitmap.convertToBitmap8Bit(inputSlices3D8, sliceNumber, trackBarWinCenter.Value, trackBarWinWidth.Value);
          }

          if (numberOfSamplesPerPixel == 3 && numberOfBitsAllocated == 8)
          {
              pictureBox1.Image = objConvertToBitmap.convertToBitmap24Bit(inputSlices3D24, sliceNumber, trackBarWinCenter.Value, trackBarWinWidth.Value);
          }

          //..............................................................

          if (outputSlices3D8 != null && outputSlices3D8.GetLength(1) > 0)
          {
              pictureBox2.Image = objConvertToBitmap.convertToBitmap8Bit(outputSlices3D8, sliceNumber, trackBarWinCenter.Value, trackBarWinWidth.Value);
          }
          if (outputSlices3D16 != null && outputSlices3D16.GetLength(1) > 0)
          {
              pictureBox2.Image = objConvertToBitmap.convertToBitmap16Bit(outputSlices3D16, sliceNumber, trackBarWinCenter.Value, trackBarWinWidth.Value);
          }
          if (outputSlices3D24 != null && outputSlices3D24.GetLength(1) > 0)
          {
              pictureBox2.Image = objConvertToBitmap.convertToBitmap8Bit(outputSlices3D24, sliceNumber, trackBarWinCenter.Value, trackBarWinWidth.Value);
          }

          //sourceImage = new Bitmap(pictureBox1.Image);
          //bmpAnnotatedImageSameSizeAsPicturebox = new Bitmap(sourceImage);

          //if (showAnnotationFlag)
          //    drawROI();
      }

     private void openPGMToolStripMenuItem_Click(object sender, EventArgs e)
      {
           OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "PGM Images (*.pgm)|*.pgm";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                Cursor.Current = Cursors.WaitCursor;
                sliceNumber = 0;
                numberOfSlices = 1;
                imageProcessingType = 1; // 2D image
                imageType = 2; //PGM type
                numberOfBitsAllocated = 16; // Number of bits for each pixel value
                numberOfSamplesPerPixel = 1;
                

                string s = openFileDialog.FileName;
                Text = s;

                objAnnotation = new Annotate(numberOfSlices);
                existInputSlice = true;
                existOutputSlice = false;
                
                numberOfROIsInSlice = new int[1];
                numberOfROIsInSlice[0] = -1;

                PGM pgm = new PGM(s);
                inputSlices2D16 = pgm.Pixels;
                //outputSlices2D16 = inputSlices2D16;


                // From here computes the minvalue2D and maxValue2D
                minValue2D = short.MaxValue;
                maxValue2D = short.MinValue;

                for (int i = 0; i < inputSlices2D16.GetLength(0); i++)
                {
                    for (int j = 0; j < inputSlices2D16.GetLength(1); j++)
                    {
                        if (inputSlices2D16[i, j] < minValue2D)
                            minValue2D = inputSlices2D16[i, j];

                        if (inputSlices2D16[i, j] > maxValue2D)
                            maxValue2D = inputSlices2D16[i, j];

                    }
                }

                //To here  From here computes the minvalue2D and maxValue2D
                int winMin = minValue2D;
                int winMax = maxValue2D;

                int winCenter = (winMin + ((winMax - winMin) / 2));
                trackBarWinCenter.Invoke(new MethodInvoker(delegate { trackBarWinCenter.Maximum = winMax; trackBarWinCenter.Minimum = winMin; trackBarWinCenter.Value = winCenter; }), null);

                int winWidth = (winMax - winMin) / 2 + 5;
                trackBarWinWidth.Invoke(new MethodInvoker(delegate { trackBarWinWidth.Minimum = 1; ; trackBarWinWidth.Maximum = winMax - winMin + 10; trackBarWinWidth.Value = winWidth; }), null);
                lblWindowCenter.Invoke(new MethodInvoker(delegate { lblWindowCenter.Text = trackBarWinCenter.Value.ToString(); }), null);
                lblWindowWidth.Invoke(new MethodInvoker(delegate { lblWindowWidth.Text = trackBarWinWidth.Value.ToString(); }), null);
                ///////////////////////////
            
                


                resetZoomPanParameters();
               
                showInPicturebox1(winCenter,winWidth);        
              
                speedOfMovement  = 20;
                showROIToolStripMenuItem.Enabled = false ;
                
                
                
                Cursor.Current = Cursors.Default;                        
            }
      }

     private void savePGMToolStripMenuItem_Click(object sender, EventArgs e)
     {
         if (outputSlices2D16 != null && imageType==2)
         {
             SaveFileDialog saveFiledialog = new SaveFileDialog();
             saveFiledialog.Filter = "PGM Images (*.pgm)|*.pgm";
             if (saveFiledialog.ShowDialog() == DialogResult.OK)
             {
                 Cursor.Current = Cursors.WaitCursor;
                 PGM pgm = new PGM(outputSlices2D16.GetLength(0), outputSlices2D16.GetLength(1));
                 pgm.Pixels = outputSlices2D16;
                 pgm.WriteToPath(saveFiledialog.FileName);
                 Cursor.Current = Cursors.Default;
             }
         }
     }

     private void dicomInfoToolStripMenuItem_Click(object sender, EventArgs e)
     {

     }

    

    

     
    

    
   

   
     private void GaussainCheck_CheckedChanged(object sender, EventArgs e)
     {
         
     }

     private void gaussianFilterToolStripMenuItem_Click(object sender, EventArgs e)
     {
         
     }

     private void calculateThresholdOfBackgroundToolStripMenuItem_Click(object sender, EventArgs e)
     {
       
     }

    
     private void featureExtractionFromROIToolStripMenuItem_Click(object sender, EventArgs e)
     {
         
     }

     
    

     private void outputOfNeuralNetworkToolStripMenuItem_Click(object sender, EventArgs e)
     {
        
     }

     private void benignORMalignantToolStripMenuItem_Click(object sender, EventArgs e)
     {
        
     }

     private void trainROIToolStripMenuItem_Click(object sender, EventArgs e)
     {
        
     }

    
     private void dShowToolStripMenuItem_Click(object sender, EventArgs e)
     {

         //............................................................
         openFileDialog1.Filter = "bvd|*.bvd";
         if (openFileDialog1.ShowDialog() == DialogResult.OK)
         {
             if (System.IO.File.Exists(Environment.CurrentDirectory + "\\h2.bvd"))
                 System.IO.File.Delete(Environment.CurrentDirectory + "\\h2.bvd");

             System.IO.File.Copy(openFileDialog1.FileName, Environment.CurrentDirectory + "\\h2.bvd");
             //..............................................................
             System.Diagnostics.Process.Start(Environment.CurrentDirectory + "\\Sample3DViewer.exe");
         }
     }

     private void settingsToolStripMenuItem2_Click(object sender, EventArgs e)
     {
        
     }

     private void playToolStripMenuItem_Click(object sender, EventArgs e)
     {
         
     }

     private void pauseToolStripMenuItem_Click(object sender, EventArgs e)
     {
         
     }

     private void global1ThreshildToolStripMenuItem_Click(object sender, EventArgs e)
     {

         objThresholding = new DicomImageViewer.Thresholding();

         
         if ( imageProcessingType == 1)
         {
             var prompLst = new List<string> { "Enter T1 : " };
             var prompt = DicomImageViewer.Dialog.ShowPromptDialog(prompLst, "Enter value");
             existOutputSlice = true;
             outputSlices2D16 = objThresholding.threshold2D(inputSlices2D16, Convert.ToInt32(prompt[0]));
             showInPicturebox2();
         }
     }

     private void testArraySortToolStripMenuItem_Click(object sender, EventArgs e)
     {
         int[] a = new int[5];
         int[] index = new int[5];

         int i = new int();

         a[0] = 1;
         a[1] = 5; a[2] = 2; a[3] = 3; a[4] = 7;

         



         i=i;

     }

     private void deleteBackgroundToolStripMenuItem_Click(object sender, EventArgs e)
     {

     }

     private void convertToBVDToolStripMenuItem_Click(object sender, EventArgs e)
     {

     }

     private void skewnessOfHistogramToolStripMenuItem_Click(object sender, EventArgs e)
     {

     }

     private void compactnessOfROIToolStripMenuItem_Click(object sender, EventArgs e)
     {

     }

     private void meanToolStripMenuItem_Click(object sender, EventArgs e)
     {

     }

    
   

     
   
   
  
 }
}
