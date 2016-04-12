using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.UI;
using Emgu.CV.Structure;
using System.IO;
using Emgu.CV.CvEnum;

namespace StemCrossSection
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            //imboxDisplay.Image = img1;
            imboxDisplay.FunctionalMode = ImageBox.FunctionalModeOption.PanAndZoom;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {            
            DialogResult result = fbdImages.ShowDialog();
            if (result == DialogResult.OK)
            {
                txtFolder.Text = fbdImages.SelectedPath;
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            String searchFolder = txtFolder.Text;
            var filters = new String[] { "jpg", "jpeg", "png", "gif", "tiff", "bmp" };
            Image<Bgr, Byte> img1 = new Image<Bgr, Byte>(GetFilesFrom(searchFolder, filters, false)[0]);
            IdentifyContours(img1);
        }

        public static String[] GetFilesFrom(String searchFolder, String[] filters, bool isRecursive)
        {
            List<String> filesFound = new List<String>();
            var searchOption = isRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            foreach (var filter in filters)
            {
                filesFound.AddRange(Directory.GetFiles(searchFolder, String.Format("*.{0}", filter), searchOption));
            }
            return filesFound.ToArray();
        }

        public void IdentifyContours(Emgu.CV.Image<Bgr, Byte> colorImage)
        {
            imboxOriginal.Image = colorImage;
            ImageProcessor.FindContours(colorImage, imboxDisplay);
            /*
            colorImage = colorImage.Resize(0.5, Inter.Linear);
            Image<Gray, Byte> Img_Source_Gray = colorImage.Convert<Gray, Byte>();
            Image<Gray, Byte> mask = Img_Source_Gray.ThresholdBinary(new Gray(90), new Gray(255));
            Image<Bgr, byte> Img_Result_Bgr = new Image<Bgr, byte>(Img_Source_Gray.Width, Img_Source_Gray.Height);
            Img_Source_Gray = Img_Source_Gray.And(mask);
            CvInvoke.CvtColor(Img_Source_Gray, Img_Result_Bgr, ColorConversion.Gray2Bgr);

            int largest_contour_index = 0;
            double largest_area = 0;

            using (Mat hierachy = new Mat())
            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
            {
                IOutputArray hirarchy;

                CvInvoke.FindContours(mask, contours, hierachy, RetrType.Tree, ChainApproxMethod.ChainApproxNone);

                for (int i = 0; i < contours.Size; i++)
                {
                    MCvScalar color = new MCvScalar(0, 0, 255);

                    double a = CvInvoke.ContourArea(contours[i], false);  //  Find the area of contour
                    if (a > largest_area)
                    {
                        largest_area = a;
                        largest_contour_index = i;                //Store the index of largest contour
                    }

                    CvInvoke.DrawContours(mask, contours, largest_contour_index, new MCvScalar(255, 0, 0), 10);
                }                
            }


            
            //mask = mask.Erode(1);
            mask = mask.Dilate(1);
            mask = mask.Erode(1);
            /*
            Gray cannyThreshold = new Gray(100);
            Gray circleAccumulatorThreshold = new Gray(120);
            double Resolution = 2;
            double MinDistance = 20;//colorImage.Width/30;
            int MinRadius = 60;//colorImage.Width/30;
            int MaxRadius = 200;//colorImage.Width/16;
            CircleF[] HoughCircles = mask.Clone().HoughCircles(
                                    cannyThreshold,
                                    circleAccumulatorThreshold,
                                    Resolution, //Resolution of the accumulator used to detect centers of the circles
                                    MinDistance, //min distance 
                                    MinRadius, //min radius
                                    MaxRadius //max radius
                                    )[0]; //Get the circles from the first channel
            #region draw circles
            foreach (CircleF circle in HoughCircles)
                Img_Result_Bgr.Draw(circle, new Bgr(Color.Red), 2);
            #endregion
            */
            //imboxDisplay.Image = Img_Result_Bgr;
        }
    }
}
