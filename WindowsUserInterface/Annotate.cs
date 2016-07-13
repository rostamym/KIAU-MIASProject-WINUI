using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace DicomImageViewer
{
    
    public class Annotate
    {
        private const int count_ROI=100;
        public List<Point>[,] roisRegionPointsList;
        public List<Point>[,] roisBoundryPointsList;               
        private int number_slice;
        public Annotate(int slice)
        {
            number_slice = slice;
            roisRegionPointsList = new List<Point>[number_slice, count_ROI];
            roisBoundryPointsList = new List<Point>[number_slice, count_ROI];          
        }
        public void make_new_ROI(int slice,int n)
        {
            roisBoundryPointsList[slice,n] = new List<Point>();
            roisRegionPointsList[slice,n] = new List<Point>();
        }
           
        public void AddboundryPoint(int slice,int n,int x, int y)
        {           
            Point temp = new Point();
            temp.X = x;
            temp.Y = y;           
            roisBoundryPointsList[slice,n].Add(temp);            
        }
        public void ClearAnnotate(int slice)
        {
            for (int i = 0; i < count_ROI; i++)                
                {
                    if (roisBoundryPointsList[slice ,i] != null)
                        roisBoundryPointsList[slice ,i].Clear();
                    if (roisRegionPointsList[slice ,i] != null)
                        roisRegionPointsList[slice ,i].Clear();
                }

        }
        private bool IsInPolygon(List  <Point> poly, Point p)
        {
            Point p1, p2;
            bool inside = false;
            if (poly.Count  < 3)
            {
                return inside;
            }
            var oldPoint = new Point(poly[poly.Count - 1].X, poly[poly.Count - 1].Y);
            for (int i = 0; i < poly.Count; i++)
            {
                var newPoint = new Point(poly[i].X, poly[i].Y);
                if (newPoint.X > oldPoint.X)
                {
                    p1 = oldPoint;
                    p2 = newPoint;
                }
                else
                    {
                      p1 = newPoint;
                      p2 = oldPoint;
                     }
                if ((newPoint.X < p.X) == (p.X <= oldPoint.X)
                    && (p.Y - (long)p1.Y) * (p2.X - p1.X)
                    < (p2.Y - (long)p1.Y) * (p.X - p1.X))
                {
                    inside = !inside;
                }
                oldPoint = newPoint;
            }
            return inside;
        }

        public void makeListOfBoundaryAndRegionPoints(int argSliceNumber,int argTheNoOfROI)
        {
            roisRegionPointsList[argSliceNumber, argTheNoOfROI].Clear(); 
            Point tmpPoint = new Point();
            int min_x = roisBoundryPointsList[argSliceNumber, argTheNoOfROI][0].X;
            int max_x = roisBoundryPointsList[argSliceNumber, argTheNoOfROI][0].X;
            for (int i = 0; i < roisBoundryPointsList[argSliceNumber, argTheNoOfROI].Count - 1; i++)
            {
                if (roisBoundryPointsList[argSliceNumber, argTheNoOfROI][i].X < min_x)
                    min_x = roisBoundryPointsList[argSliceNumber, argTheNoOfROI][i].X;
                if (roisBoundryPointsList[argSliceNumber, argTheNoOfROI][i].X > max_x)
                    max_x = roisBoundryPointsList[argSliceNumber, argTheNoOfROI][i].X;
            }
            int min_y = roisBoundryPointsList[argSliceNumber, argTheNoOfROI][0].Y;
            int max_y = roisBoundryPointsList[argSliceNumber, argTheNoOfROI][0].Y;
            for (int i = 0; i < roisBoundryPointsList[argSliceNumber, argTheNoOfROI].Count - 1; i++)
            {
                if (roisBoundryPointsList[argSliceNumber, argTheNoOfROI][i].Y < min_y)
                    min_y = roisBoundryPointsList[argSliceNumber, argTheNoOfROI][i].Y;
                if (roisBoundryPointsList[argSliceNumber, argTheNoOfROI][i].Y > max_y)
                    max_y = roisBoundryPointsList[argSliceNumber, argTheNoOfROI][i].Y;
            }



                for (int i = min_x ; i <= max_x; i++)
                {
                    for (int j = min_y ; j <= max_y ; j++)
                    {
                        tmpPoint.X = i; tmpPoint.Y = j;
                        if (IsInPolygon(roisBoundryPointsList[argSliceNumber, argTheNoOfROI], tmpPoint))
                        {
                            roisRegionPointsList[argSliceNumber, argTheNoOfROI].Add(tmpPoint);

                        }
                    }
                }
        }
        private bool search(List<Point> ROI, Point p)
        {
            bool find = false;
            for (int i = 0; i < ROI.Count; i++)
                if ((ROI[i].X == p.X) && (ROI[i].Y == p.Y))
                {
                    find = true;
                    break;
                }
            return find;
        }
        private bool isborder(List<Point> ROI, Point p)
        {
            Point temp = new Point();
            temp.X = p.X - 1; temp.Y = p.Y - 1;
            if (search(ROI, temp) == false)
                return (true);
            temp.X = p.X - 1; temp.Y = p.Y + 1;
            if (search(ROI, temp) == false)
                return (true);
            temp.X = p.X - 1; temp.Y = p.Y;
            if (search(ROI, temp) == false)
                return (true);

            temp.X = p.X; temp.Y = p.Y - 1;
            if (search(ROI, temp) == false)
                return (true);
            temp.X = p.X; temp.Y = p.Y + 1;
            if (search(ROI, temp) == false)
                return (true);

            temp.X = p.X + 1; temp.Y = p.Y - 1;
            if (search(ROI, temp) == false)
                return (true);
            temp.X = p.X + 1; temp.Y = p.Y;
            if (search(ROI, temp) == false)
                return (true);
            temp.X = p.X + 1; temp.Y = p.Y + 1;
            if (search(ROI, temp) == false)
                return (true);

            return false;
        }
        public int Perimeter(List<Point> ROI)
        {
            int perim = 0;
            foreach (Point p in ROI )
              if (isborder(ROI,p)==true)
                  perim++;
            return (perim);
        }
        public void CloseROI(int slice,int n)
        {           
            Point[] linePoints;
            if (roisBoundryPointsList[slice,n].Count > 0)
            {
                int xa = roisBoundryPointsList[slice,n][0].X;
                int ya = roisBoundryPointsList[slice,n][0].Y;
                int xb = roisBoundryPointsList[slice, n][roisBoundryPointsList[slice,n].Count - 1].X;
                int yb = roisBoundryPointsList[slice, n][roisBoundryPointsList[slice,n].Count - 1].Y;
                linePoints = DDA.dda(xa, ya, xb, yb).ToArray();
                for (int i = 0; i < linePoints.GetLength(0); i++)
                    roisBoundryPointsList[slice,n].Add(linePoints[i]);
            }           
        }


    }
}
