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
            EThresholdModel thresholdModel = rdbModel2.Checked ? EThresholdModel.Model2 : EThresholdModel.Model1;
            ImageProcessor imgProc = new ImageProcessor(searchFolder, txtLog, thresholdModel, filters);
            imgProc.ProcessImages();
        }        
    }
}
