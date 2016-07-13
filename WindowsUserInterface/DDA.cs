using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace DicomImageViewer
{
    public class DDA
    {
        public static IEnumerable<Point> dda(int xa, int ya, int xb, int yb)
        {
            int dx = xb - xa;
            int dy = yb - ya;
            int step = 0;
            float xinc, yinc, x = xa, y = ya;
            if (Math.Abs(dx) > Math.Abs(dy))
                step = Math.Abs(dx);
            else
                step = Math.Abs(dy);

            int[,] pixle = new int[step, 2];
            xinc = dx / (float)step;
            yinc = dy / (float)step;

            Point nextPoint = new Point();

            nextPoint .X= round(x);
            nextPoint.Y = round(y);
            yield return nextPoint;
            for (int i = 1; i < step; i++)
            {
                x += xinc;
                y += yinc;
                nextPoint.X = round(x);
                nextPoint .Y= round(y);
                yield return nextPoint;
            }
        }

        private static int round(float a)
        {
            return (int)(a + 0.5);
        }
       
    }

}
