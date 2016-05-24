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

    public enum EThresholdModel
    {
        Model0,
        Model1,//Threshold = 5.7 + 0.6136*mean + 0.6083*max - 0.936*sd - 0.2385*min
        Model2, //Threshold = 0.6210*mean + 0.6431*max - 1.005*sd - 0.2508*min
        Model4, //Threshold = 48.74 + mean*0.4726 + max*0.3060
        Model5, //Threshold = 83.97 + 0.7667*mean
    }
    public class ImageProcessor
    {
        public EThresholdModel ThresholdModel { get; set; }
        public String ImagesFolder { get; set; }
        public String[] Extensions { get; set; }
        public String OutputFile { get { return string.Concat(ImagesFolder, "\\", "output.csv"); } }
        public TextBox TxtLog { get; set; }

        public ImageProcessor(String folder, System.Windows.Forms.TextBox txtLog, EThresholdModel thresholdModel, params String[] extensions)
        {
            this.TxtLog = txtLog;
            this.ImagesFolder = folder;
            this.ThresholdModel = thresholdModel;
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
            Image<Bgr, byte> filteredImage = new Image<Bgr, byte>(Img_Source_Gray.Width, Img_Source_Gray.Height);
            for (int i = 0; i < circles; i++)
            {
                CvInvoke.DrawContours(filteredImage, contours_poly, i, new MCvScalar(255, 255, 255), -1);
            }


            filteredImage = colorImage.And(filteredImage);
            Image<Gray, Byte> whiteAreas = new Image<Gray, byte>(filteredImage.Width, filteredImage.Height);
            List<DetectedCircle> detectedCircles = new List<DetectedCircle>();
            for (int i = 0; i < circles; i++)
            {                
                PointF[] pointfs = Array.ConvertAll(contours_poly[i].ToArray(), input => new PointF(input.X, input.Y));
                Rectangle boundingRect = PointCollection.BoundingRectangle(pointfs);
                DetectedCircle detectedCircle = new DetectedCircle()
                { contour = contours_poly[i], boundingRect = boundingRect, center = new Point(boundingRect.X + (boundingRect.Width / 2), boundingRect.Y + (boundingRect.Height / 2)) };
                detectedCircles.Add(detectedCircle);
                filteredImage.ROI = boundingRect;
                int threshold = _ComputeThreshold(filteredImage, detectedCircle);   
                filteredImage.ROI = Rectangle.Empty;
                Image<Bgr, Byte> mask = new Image<Bgr, byte>(filteredImage.Width, filteredImage.Height);
                mask.Draw(boundingRect, new Bgr(255, 255, 255), -1);
                mask = filteredImage.And(mask);
                /* Extract white are solely based on the Blue channel */
                mask = mask.ThresholdBinary(new Bgr(threshold, 0, 0), new Bgr(255, 0, 0));                
                whiteAreas = whiteAreas.Or(mask.Convert<Gray, Byte>().ThresholdBinary(new Gray(10), new Gray(255)));
                CvInvoke.DrawContours(whiteAreas, contours_poly, i, new MCvScalar(255, 255, 255), 3);
            }


            detectedCircles = detectedCircles.OrderBy(c => c.center.X).ToList();
            processedImage = whiteAreas;
            return _ComputeMetrics(whiteAreas, detectedCircles, circles);
        }

        private float[] _ComputeHistogram(Image<Bgr, Byte> filteredImage, DetectedCircle detectedCircle)
        {
            float[] hist = new float[256];

            byte[,,] data = filteredImage.Data;
            Parallel.For(filteredImage.ROI.Y, filteredImage.ROI.Y + filteredImage.Rows, i =>
            {
                Parallel.For(filteredImage.ROI.X, filteredImage.ROI.X + filteredImage.Cols, j =>
                {
                    double inside = CvInvoke.PointPolygonTest(detectedCircle.contour, new PointF(j, i), false);
                    if (inside >= 0)
                    {
                        hist[data[i, j, 0]]++;
                    }
                });
            });

            return hist;

        }

        private int _ComputeThreshold(Image<Bgr, Byte> filteredImage, DetectedCircle circle)
        {
            float min, max;
            //Gray avg;
            //MCvScalar sdv;
            //filteredImageWithROI.AvgSdv(out avg, out sdv);
            //DenseHistogram histo = new DenseHistogram(256, new RangeF(0, 256));

            //histo.Calculate(new Image<Gray, Byte>[] { filteredImageWithROI }, true, null);
            float[] grayHist = _ComputeHistogram(filteredImage, circle);//histo.GetBinValues();
            //grayHist = grayHist.Skip(20).ToArray(); //discard the black pixels of the borders

            double avg = 0;
            double numPixels = 0;
            for (int i = 0; i < grayHist.Length; i++)
            {
                numPixels += grayHist[i];
                avg += (i) * grayHist[i];
            }
            avg = avg / numPixels;
            double sumOfSquaresOfDifferences = grayHist.Select(val => (val - avg) * (val - avg)).Sum();
            double sd = Math.Sqrt(sumOfSquaresOfDifferences / numPixels);

            min = Array.FindIndex(grayHist, b => b > 0);
            max = 255 - Array.FindIndex(grayHist.Reverse().ToArray(), b => b > 0);

            if (ThresholdModel == EThresholdModel.Model0)
                return (int)(-30.2529255 + 0.4451667 * avg - 0.1759019 * min + 0.9173600 * max - 1.2452913 * sd);
            else if (ThresholdModel == EThresholdModel.Model1)
                return (int)(5.7 + 0.6136 * avg + 0.6083 * max - 0.936 * sd - 0.2385 * min);
            else if (ThresholdModel == EThresholdModel.Model2)
                return (int)(0.6210 * avg + 0.6431 * max - 1.005 * sd - 0.2508 * min);
            else if (ThresholdModel == EThresholdModel.Model4)
                return (int)(48.74 + avg * 0.4726 + max * 0.3060);
            else
                return (int)(83.97 + 0.7667 * avg);
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
