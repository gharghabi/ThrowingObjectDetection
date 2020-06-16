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
    public partial class Form1 : Form
    {
        public Capture _capture;
        //struct PolRes // baghimandehaye marboot be har polynomial
        //{
        //    public List<double> Residuals;
        //};
        List<PolRes> Ri;//?
 

        public Form1()
        {
            InitializeComponent();
        }




        private void ThrownButton_Click(object sender, EventArgs e)
        {

            _capture = new Capture("C:/Users/shaghayegh/Documents/Visual Studio 2010/Projects/motionemgu/Thrown-Object-All.avi");
            Image<Bgr, Byte> prev = _capture.QueryFrame();
            Image<Bgr, Byte> curr = _capture.QueryFrame();
            Image<Gray, Single> DFT = new Image<Gray, Single>(prev.Size);
            Image<Bgr, Single> Background = new Image<Bgr, Single>(prev.Size);
            Image<Gray, Single> DFT_Prev = new Image<Gray, Single>(prev.Size);
            Image<Gray, Single> originalLeft = new Image<Gray, Single>(prev.Size);
            Image<Gray, Single> originalRight = new Image<Gray, Single>(prev.Size);
            Image<Gray, Single> sum = new Image<Gray, Single>(prev.Size);
            Image<Bgr, Byte> BGSImage = new Image<Bgr, byte>(prev.Size);
            ContourDetection _contour = new ContourDetection();
            Image<Bgr, byte> outpic;
            //Polynomial polynomial = new Polynomial();
            List<Polynomial> polynomials = new List<Polynomial>();
            double xj;//x jadid
            double yj;//y jadid
            int FrameNumber = 1;
            double DistanceThr = 50; //?
           // List<double> Residuals = new List<double>();
            double Rix = 0, Riy = 0;
            int FrameStep=20;
            int MinPointForDelete = 12;
            List<MyPoint> FramePoints = new List<MyPoint>();
            List<double> Wi = new List<double>();
            

            Action play = new Action(() =>
                {

                    while (curr != null)
                    {
                        Ri= new List<PolRes>();
                        FrameNumber++;
                        outpic = new Image<Bgr, byte>(curr.Size);
                        CvInvoke.cvAbsDiff(curr, prev, BGSImage);
                        pictureBox2.Image = BGSImage.ToBitmap();

                        Image<Bgr, byte> outpic1 = _contour.FindContourSperatly(BGSImage.Convert<Bgr, byte>());
                        
                        Wi.Clear();
                        
                        FramePoints.Clear();
                        FramePoints = new List<MyPoint>();

                        for (Contour<Point> contour = outpic1.Convert<Gray, byte>().FindContours(Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE,
                         Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_EXTERNAL); contour != null; contour = contour.HNext)
                        {
                            Point[] pts;
                            Point p = new Point(0, 0);
                            int sumX=0;
                            int sumY=0;
                            pts = contour.ToArray();
                        
                            
                            if ((contour.Area / contour.Perimeter) <= 50)
                            {
                                outpic.DrawPolyline(pts, true, new Bgr(255, 0, 255), 1);
                                
                                for (int i = 0; i < pts.Length; i++)
                                {
                                    sumX = pts[i].X + sumX;
                                    sumY = pts[i].Y + sumY;
                                }
                                xj = sumX / pts.Length;
                                yj = sumY / pts.Length;//markaze contour peida shode ast
                                MyPoint MP = new MyPoint();
                                MP.added = false;
                                MP.point = new Point(Convert.ToInt32(xj),Convert.ToInt32(yj));
                                FramePoints.Add(MP);
                            }//end if size contour
                        }//for each contour

                        if (FramePoints.Count != 0)
                        {
                            for (int PolInd = 0; PolInd < polynomials.Count; PolInd++)
                            {
                                if (polynomials[PolInd].LastFrameNum - polynomials[PolInd].FirstFrameNum >= FrameStep)
                                {
                                    if (polynomials[PolInd].PolPoints.Count < MinPointForDelete)
                                    {
                                        //polynomials.Remove(polynomials[PolInd]);
                                        //?ri[polind].remove
                                    }
                                    else
                                    {
                                        MessageBox.Show("Salam");
                                    }
                                }
                                int t = FrameNumber - polynomials[PolInd].LastFrameNum;
                                //if(PolInd > Ri.Count-1)
                                //{
                                Ri.Add(new PolRes());
                                //}?

                                for (int j = 0; j < FramePoints.Count(); ++j)
                                {
                                    Rix = FramePoints[j].point.X - polynomials[PolInd].X(t);
                                    Riy = FramePoints[j].point.Y - polynomials[PolInd].Y(t);
                                    //Ri[PolInd].Residuals.Clear();

                                    Ri[PolInd].Residuals.Add(Math.Sqrt(Math.Pow(Rix, 2) + Math.Pow(Riy, 2)));
                                }

                                double denominator = 0;
                                double Sigma = 1;//?
                                for (int i = 0; i < FramePoints.Count(); i++)
                                {
                                    denominator += Math.Exp(-1 * Math.Pow(Ri[PolInd].Residuals[i], 2) / Math.Pow(Sigma, 2));
                                }

                                Wi.Clear();
                                for (int i = 0; i < FramePoints.Count; i++)
                                {
                                    if (denominator == 0)
                                    {
                                        Wi.Add(0);
                                    }
                                    else
                                    {
                                        Wi.Add(Math.Exp(-1 * Math.Pow(Ri[PolInd].Residuals[i], 2) / Math.Pow(Sigma, 2)) / denominator);
                                    }
                                }
                                /// T(p)
                                /// 

                                double T2 = Tcalculate(2, Wi, t);
                                double T3 = Tcalculate(3, Wi, t);
                                double T4 = Tcalculate(4, Wi, t);

                                //calculate sigmas
                                double M0 = 0, M1 = 0, M2 = 0;
                                for (int i = 0; i < Wi.Count; i++)
                                {
                                    M0 += Wi[i] * t * (FramePoints[i].point.X - polynomials[PolInd].bx);
                                    M1 += Wi[i] * Math.Pow(t, 2) * (FramePoints[i].point.Y - polynomials[PolInd].cy);
                                    M2 += Wi[i] * t * (FramePoints[i].point.Y - polynomials[PolInd].cy);

                                }

                                // shift e zamanie Polynomialha
                                double[] newParams = CalcParams(T2, T3, T4, Wi, t, M0, M1, M2);
                                polynomials[PolInd].ax = newParams[0];
                                polynomials[PolInd].bx -= newParams[0] * t;
                                polynomials[PolInd].ay = newParams[1];
                                polynomials[PolInd].by -= 2 * newParams[1] * t;
                                polynomials[PolInd].cy += newParams[1] * Math.Pow(t, 2) - newParams[2] * t;
                                /////
                                /// ... Add Points to Polynomials
                                /// 
                                double MinDist = 1000;
                                int MinRiIndex = -1;
                                for (int PointNum = 0; PointNum < FramePoints.Count; ++PointNum)
                                {
                                    //int minRi = Ri[PolInd].Residuals.IndexOf(Ri[PolInd].Residuals.Min());//be jaye for
                                    if (!FramePoints[PointNum].added)
                                    {
                                        if (Ri[PolInd].Residuals[PointNum] < MinDist)
                                        {
                                            MinDist = Ri[PolInd].Residuals[PointNum];
                                            MinRiIndex = PointNum;
                                        }
                                    }

                                }
                                if (MinRiIndex != -1)
                                { // noghte E ke beshe add kard nayaftim :D
                                    if (Ri[PolInd].Residuals[MinRiIndex] < DistanceThr)
                                    {
                                        FramePoints[MinRiIndex].added = true;

                                        polynomials[PolInd].PolPoints.Add(new Point(FramePoints[MinRiIndex].point.X, FramePoints[MinRiIndex].point.Y));
                                        polynomials[PolInd].LastFrameNum = FrameNumber;//? age in noghte be sahmie digeE nazdiktar bood chi?

                                    }
                                }
                                /////
                            }
                        }


                        for (int PointNum = 0; PointNum < FramePoints.Count; ++PointNum)
                        {
                            if (!FramePoints[PointNum].added)
                            {
                                polynomials.Add(new Polynomial(FramePoints[PointNum].point.X, FramePoints[PointNum].point.Y));
                                polynomials.Last().FirstFrameNum = FrameNumber;
                                polynomials.Last().LastFrameNum = FrameNumber;
                            }
                        }

                        //// ezafe kardane noghat be Polynomialha
                        double MinDis = 1000;
                        //double ide
                        for (int pNum = 0; pNum < FramePoints.Count; pNum++)
                        {
                            for (int polNum = 0; polNum < Ri.Count; polNum++)
                            {
                                if (MinDis > Ri[polNum].Residuals[pNum])
                                {
                                    MinDis = Ri[polNum].Residuals[pNum];
                                    
                                }
                            }
                            
                            //Rix = FramePoints[j].X - polynomials[PolInd].X(t);
                            //Riy = FramePoints[j].Y - polynomials[PolInd].Y(t);
                            //Ri[PolInd].Residuals.Clear();
                            //Ri[PolInd].Residuals.Add(Math.Sqrt(Math.Pow(Rix, 2) + Math.Pow(Riy, 2)));
                        }

                        /////

                            //    this.Invoke(new Action(() =>
                            //{
                            //    textBox1.Text = contour.Area.ToString() + ", p " + contour.Perimeter.ToString();
                            //}));


                        

                        
                        //// mohasebe T(p)
                        ///////

                        pictureBox1.Image = outpic.ToBitmap();
                        prev = curr.Copy();
                        curr = _capture.QueryFrame();
                        Ri.Clear();
                    }

                });
            play.BeginInvoke(null, null);

        }





        public double Tcalculate(int p, List<double> wi, int t)
        {
            double T;
            T = 0;
            for (int j = 0; j < wi.Count(); ++j)
            {
                T=wi[j] * (Math.Pow(t, p)) + T;
            }
            return T;
        }
        public double[] CalcParams(double T2, double T3, double T4, List<double> Wi, int t, double M0, double M1, double M2)
        {
            Matrix<double> T = new Matrix<double>(2, 2);
            T[0, 0] = T4;
            T[0, 1] = T3;
            T[1, 0] = T3;
            T[1, 1] = T2;

            Matrix<double> TT = new Matrix<double>(2, 2);
            TT[0, 0] = T2/T.Det;
            TT[0, 1] = T3/T.Det;
            TT[1, 0] = T3/T.Det;
            TT[1, 1] = T4/T.Det;


            Matrix<double> M = new Matrix<double>(2, 1);
            M[0, 0] = M1;
            M[1, 0] = M2;

            Matrix<double> A = new Matrix<double>(2, 1);
            A = TT.Mul(M);

            double ax = M0 / T2;
            double[] paramsPoly = new double[3];
            paramsPoly[0] = ax;
            paramsPoly[1] = A[0, 0];
            paramsPoly[2] = A[1, 0];
            return paramsPoly;

        }
    }



}



/***********************************************************************************
_capture = new Capture("C:/Users/shaghayegh/Documents/Visual Studio 2010/Projects/motionemgu/s101.avi");
            Image<Bgr, Byte> prev = _capture.QueryFrame();
            Image<Bgr, Byte> curr = _capture.QueryFrame();
            Image<Gray, Single> DFT = new Image<Gray, Single>(prev.Size);
            Image<Bgr, Single> Background = new Image<Bgr,Single>(prev.Size);
            Image<Gray, Single> DFT_Prev = new Image<Gray, Single>(prev.Size);
            Image<Gray, Single> originalLeft = new Image<Gray, Single>(prev.Size);
            Image<Gray, Single> originalRight = new Image<Gray, Single>(prev.Size);
            Image<Gray, Single> sum = new Image<Gray, Single>(prev.Size);
            Image<Bgr, Byte> BGSImage = new Image<Bgr, byte>(prev.Size);
            ContourDetection _contour = new ContourDetection();
            Image<Bgr,byte> outpic;
            Action play = new Action(() =>
                {
                    while (curr != null)
                    {
                        outpic = new Image<Bgr, byte>(curr.Size);

                        CvInvoke.cvDFT(curr.Convert<Gray, Single>(), DFT, Emgu.CV.CvEnum.CV_DXT.CV_DXT_FORWARD, -1);
                        CvInvoke.cvDFT(prev.Convert<Gray, Single>().Ptr, DFT_Prev.Ptr, Emgu.CV.CvEnum.CV_DXT.CV_DXT_FORWARD, -1);

                        CvInvoke.cvDFT((DFT_Prev - DFT).Convert<Gray, Single>().Ptr, originalLeft.Ptr, Emgu.CV.CvEnum.CV_DXT.CV_DXT_INVERSE, -1);
                        CvInvoke.cvDFT((DFT - DFT_Prev).Ptr, originalRight.Ptr, Emgu.CV.CvEnum.CV_DXT.CV_DXT_INVERSE, -1);
                        sum = originalLeft - originalRight;
                        pictureBox2.Image = sum.ToBitmap();
                        //CvInvoke.cvAbsDiff(curr, prev, BGSImage);
                        //pictureBox2.Image = BGSImage.ToBitmap();

                        Image<Bgr, byte> outpic1 = _contour.FindContourSperatly(sum.Convert<Bgr, byte>());
                        //pictureBox1.Image = outpic.ToBitmap();
                        
                        //BGSImage=_contour.FindContourSperatly(BGSImage);
                        for (Contour<Point> contour = outpic1.Convert<Gray, byte>().FindContours(Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE,
                         Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_EXTERNAL); contour != null; contour = contour.HNext)
                        {
                            Point[] pts;
                            Point p = new Point(0, 0);
                            pts = contour.ToArray();
                           // if ((contour.Area / contour.Perimeter) <= 100)
                                outpic.DrawPolyline(pts, true, new Bgr(255, 0, 255), 1);
                            this.Invoke(new Action(()=>{textBox1.Text=contour.Area.ToString()+", p "+contour.Perimeter.ToString();
                            }));

                        }
                        
                        pictureBox1.Image = outpic.ToBitmap();
                        prev = curr.Copy();
                        curr = _capture.QueryFrame();
                    }

                });
            play.BeginInvoke(null, null);

        }
*****************************************************************/