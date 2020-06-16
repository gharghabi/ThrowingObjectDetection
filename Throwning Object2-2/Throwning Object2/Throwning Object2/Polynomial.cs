using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Data;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.VideoSurveillance;
using Emgu.Util;
using Emgu.CV.UI;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;


namespace Throwning_Object2
{
    class Polynomial
    {
        public double ax;
        public double bx;
        public double ay;
        public double by;
        public double cy;
        public int LastFrameNum;
        public int FirstFrameNum;
        public List<Point> PolPoints;

        public Polynomial(int x,int y)
        {
            PolPoints = new List<Point>();
            PolPoints.Add(new Point(x, y));
            ax = 0;
            bx = x;
            ay = 0;
            by = 0;
            cy = y;
            LastFrameNum = 0;
        }
        
        public double X(int t)
        {
            double sum = ax * t + bx;
            return sum;
        }
        public double Y(int t)
        {
            double sum = ay * Math.Pow(t, 2) + by * t + cy;
            return sum;
        }
    }
}
