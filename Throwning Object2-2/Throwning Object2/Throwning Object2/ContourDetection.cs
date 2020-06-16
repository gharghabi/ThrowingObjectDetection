using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.VideoSurveillance;
using Emgu.Util;
using Emgu.CV.UI;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using System.Diagnostics;
namespace Throwning_Object2
{
    class ContourDetection
    {
        public Image<Bgr, Byte> FindContourSperatly(Image<Bgr, Byte> image)//tasviri ke mikhahid contour ha dar an peyda shavand be onvane vorudi be aan dade mishavad
        {
            Gray cannyThreshold = new Gray(180);
            Gray cannyThresholdLinking = new Gray(120);
            Image<Gray, Byte> grayImage = image.Convert<Gray, Byte>();
            Image<Gray, Byte> cannyImage = new Image<Gray, Byte>(grayImage.Size);
            CvInvoke.cvCanny(grayImage, cannyImage, 100, 360, 3);//threshold baraye canny 

            Image<Bgr, Byte> BoundryImage = image.CopyBlank();

            StructuringElementEx kernel = new StructuringElementEx(3, 3, 1, 1, Emgu.CV.CvEnum.CV_ELEMENT_SHAPE.CV_SHAPE_ELLIPSE);
            CvInvoke.cvDilate(cannyImage, cannyImage, kernel, 1);

            IntPtr cont = IntPtr.Zero;

            Point[] pts;
            Point p = new Point(0, 0);

            using (MemStorage storage = new MemStorage()) //allocate storage for contour approximation
                for (Contour<Point> contours = cannyImage.FindContours(Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE,
                  Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_EXTERNAL); contours != null; contours = contours.HNext)
                {
                    pts = contours.ToArray();

                    //********keshidan khat dor ta dore contour******************/
                    BoundryImage.DrawPolyline(pts, true, new Bgr(255, 0, 255), 3);

                    /***************joda kardane ghesmate contoure tasvir****************/

                }


            return BoundryImage;
        }

    }
}
