﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.UI;
using Emgu.CV.Structure;
using System.IO;
using Emgu.CV.CvEnum;
using System.Drawing;

namespace StemCrossSection
{
    public class ImageProcessor
    {
        public static void FindContours(Emgu.CV.Image<Bgr, Byte> colorImage, ImageBox imbox)
        {
            int circles = 0;
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            Mat hierarchy = new Mat();

            /// Detect edges using Threshold
            Image<Gray, Byte> Img_Source_Gray = colorImage.Convert<Gray, Byte>();
            Img_Source_Gray = Img_Source_Gray.SmoothBlur(3, 3);
            Image<Gray, Byte> threshold_output = Img_Source_Gray.ThresholdBinary(new Gray(90), new Gray(255));

            /// Find contours
            CvInvoke.FindContours(threshold_output, contours, hierarchy, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);
            /// Approximate contours to polygons + get bounding rects and circles
            VectorOfVectorOfPoint contours_poly = new VectorOfVectorOfPoint(contours.Size);
            List<Rectangle> boundRect = new List<Rectangle>(contours.Size);
            List<CircleF> circle = new List<CircleF>(contours.Size);

            Point[][] con = contours.ToArrayOfArray();
            PointF[][] conf = new PointF[con.GetLength(0)][];
            for (int i = 0; i < con.GetLength(0); i++)
            {
                conf[i] = new PointF[con[i].Length];
                for (int j = 0; j < con[i].Length; j++)
                {
                    conf[i][j] = new PointF(con[i][j].X, con[i][j].Y);
                }
            }


            for (int i = 0; i < contours.Size; i++)
            {
                double a = CvInvoke.ContourArea(contours[i], false);
                if (a > 80000)
                {
                    CvInvoke.ApproxPolyDP(contours[i], contours_poly[circles], 3, true);
                    
                    circles++;
                }
                //boundRect[i] = CvInvoke.BoundingRectangle(contours[i]);
                //circle[i] = CvInvoke.MinEnclosingCircle(contours_poly[circles]);

            }


            /// Draw polygonal contour + bonding rects + circles

            Image<Bgr, byte> Img_Result_Bgr = new Image<Bgr, byte>(Img_Source_Gray.Width, Img_Source_Gray.Height);
            for (int i = 0; i < circles; i++)
            {
                CvInvoke.DrawContours(Img_Result_Bgr, contours_poly, i, new MCvScalar(255, 255, 255), -1);
                //MCvScalar color = new MCvScalar(0, 0, 255);
                //CvInvoke.Rectangle(threshold_output, boundRect[i], color);
                //CvInvoke.Circle(threshold_output, new Point((int)circle[i].Center.X, (int)circle[i].Center.Y), (int)circle[i].Radius, color);
            }


            Img_Result_Bgr = colorImage.And(Img_Result_Bgr);
            Image<Gray, Byte> whiteAreas = Img_Result_Bgr.Convert<Gray, Byte>().ThresholdBinary(new Gray(185), new Gray(255));
            for (int i = 0; i < circles; i++)
            {
                CvInvoke.DrawContours(whiteAreas, contours_poly, i, new MCvScalar(255, 255, 255), 3);
                //MCvScalar color = new MCvScalar(0, 0, 255);
                //CvInvoke.Rectangle(threshold_output, boundRect[i], color);
                //CvInvoke.Circle(threshold_output, new Point((int)circle[i].Center.X, (int)circle[i].Center.Y), (int)circle[i].Radius, color);
            }
            /// Show in a window
            imbox.Image = whiteAreas;
        }
    }
}
