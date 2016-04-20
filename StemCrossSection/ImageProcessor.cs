using System;
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
using System.Windows.Forms;

namespace StemCrossSection
{
    public class DetectedCircle
    {
        public VectorOfPoint contour { get; set; }
        public Point center { get; set; }
        public Rectangle boundingRect { get; set; }
    }
    public class ImageProcessor
    {
        public String ImagesFolder { get; set; }
        public String[] Extensions { get; set; }
        public String OutputFile { get { return string.Concat(ImagesFolder, "\\", "output.csv"); } }
        public TextBox TxtLog { get; set; }

        public ImageProcessor(String folder, System.Windows.Forms.TextBox txtLog, params String[] extensions)
        {
            this.TxtLog = txtLog;
            this.ImagesFolder = folder;
            if (extensions.Length > 0)
                this.Extensions = extensions;
        }
        private String[] _GetFilesFromFolder(bool isRecursive = false)
        {
            List<String> filesFound = new List<String>();
            var searchOption = isRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            foreach (var filter in Extensions)
            {
                filesFound.AddRange(Directory.GetFiles(ImagesFolder, String.Format("*.{0}", filter), searchOption));
            }
            return filesFound.ToArray();
        }

        public void ProcessImages()
        {
            String[] imagesPaths = _GetFilesFromFolder();
            foreach (String imgPath in imagesPaths)
            {
                _LogProgress(imgPath);
                Image<Bgr, Byte> img = new Image<Bgr, Byte>(imgPath);
                Image<Gray, Byte> procImg;                
                decimal[] results = _FindContoursAndCalculate(img, out procImg);
                _WriteToCSV(imgPath, results);
                _WriteProcImage(imgPath, procImg);                
            }
            _LogProgress();
        }

        private void _LogProgress(String currentImage = null)
        {
            if (currentImage != null)
            {
                TxtLog.AppendText("Processing Image ");
                TxtLog.AppendText(currentImage.Substring(ImagesFolder.Length + 1));
                TxtLog.AppendText(" ....");
                TxtLog.AppendText(Environment.NewLine);
            }
            else
                TxtLog.AppendText("DONE!");
        }

        private decimal[] _FindContoursAndCalculate(Emgu.CV.Image<Bgr, Byte> colorImage, out Emgu.CV.Image<Gray, Byte> processedImage)
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
                if (a > 30000)
                {
                    CvInvoke.ApproxPolyDP(contours[i], contours_poly[circles], 3, true);

                    circles++;
                }

            }

            /// Draw polygonal contour + bonding rects + circles
            Image<Gray, byte> filteredImage = new Image<Gray, byte>(Img_Source_Gray.Width, Img_Source_Gray.Height);
            for (int i = 0; i < circles; i++)
            {
                CvInvoke.DrawContours(filteredImage, contours_poly, i, new MCvScalar(255, 255, 255), -1);
            }


            filteredImage = colorImage.Convert<Gray, Byte>().And(filteredImage);
            Image<Gray, Byte> whiteAreas = new Image<Gray, byte>(filteredImage.Width, filteredImage.Height);
            List<DetectedCircle> detectedCircles = new List<DetectedCircle>();
            for (int i = 0; i < circles; i++)
            {                
                PointF[] pointfs = Array.ConvertAll(contours_poly[i].ToArray(), input => new PointF(input.X, input.Y));
                Rectangle boundingRect = PointCollection.BoundingRectangle(pointfs);
                detectedCircles.Add(
                    new DetectedCircle()
                    { contour = contours_poly[i], boundingRect = boundingRect, center = new Point(boundingRect.X + (boundingRect.Width / 2), boundingRect.Y + (boundingRect.Height / 2)) });
                filteredImage.ROI = boundingRect;
                Gray avg = filteredImage.GetAverage();
                filteredImage.ROI = Rectangle.Empty;
                int threshold = _ComputeThreshold(avg);
                Image<Gray, Byte> mask = new Image<Gray, byte>(filteredImage.Width, filteredImage.Height);
                mask.Draw(boundingRect, new Gray(255), -1);
                mask = filteredImage.And(mask);
                mask = mask.ThresholdBinary(new Gray(threshold), new Gray(255));
                whiteAreas = whiteAreas.Or(mask);
                CvInvoke.DrawContours(whiteAreas, contours_poly, i, new MCvScalar(255, 255, 255), 3);
            }


            detectedCircles = detectedCircles.OrderBy(c => c.center.X).ToList();
            processedImage = whiteAreas;
            return _ComputeMetrics(whiteAreas, detectedCircles, circles);
        }

        private int _ComputeThreshold(Gray avgPixel)
        {

            //y = 1.0042x + 61.03
            return (int)(1.0042 * avgPixel.Intensity + 61.03);
            /*if (avgPixel.Intensity >= 115)
                return 185;
            else if (avgPixel.Intensity >= 105)
                return 180;
            else if (avgPixel.Intensity >= 95)
                return 175;
            else if (avgPixel.Intensity >= 85)
                return 170;
            else if (avgPixel.Intensity >= 75)
                return 165;
            else
                return 160;*/
        }

        private decimal[] _ComputeMetrics(Image<Gray, Byte> whiteAreas, List<DetectedCircle> detectedCircles, int circles)
        {
            decimal[] percentages = new decimal[circles];
            int[] whitePixelsPerCircle = new int[circles];
            int[] totalPixelsPerCircle = new int[circles];

            byte[,,] data = whiteAreas.Data;
            Parallel.For(0, whiteAreas.Rows, i =>
            {
                Parallel.For(0, whiteAreas.Cols, j =>
                {
                    for (int k = 0; k < circles; k++)
                    {
                        double inside = CvInvoke.PointPolygonTest(detectedCircles[k].contour, new PointF(j, i), false);
                        detectedCircles[k].center = detectedCircles[k].center;
                        if (inside >= 0)
                        {
                            totalPixelsPerCircle[k]++;
                            if (data[i, j, 0] != 0)
                                whitePixelsPerCircle[k]++;
                        }

                    }
                });
            });

            for (int i = 0; i < circles; i++)
            {
                percentages[i] = (decimal)whitePixelsPerCircle[i] / (decimal)totalPixelsPerCircle[i];
            }
            return percentages;
        }

        private void _WriteToCSV(string fileName, decimal[] results)
        {

            var csv = new StringBuilder();
            csv.Append(fileName);
            for (int i = 0; i < results.Length; i++)
            {
                csv.Append(string.Concat(",", results[i]));
            }
            csv.AppendLine();

            //after your loop
            File.AppendAllText(OutputFile, csv.ToString());
        }

        private void _WriteProcImage(string originaPath, Image<Gray, Byte> procImg)
        {
            procImg.Save(string.Concat(originaPath.Substring(0, originaPath.Length - 4), "(Processed).proc.jpg"));
        }

    }
}
