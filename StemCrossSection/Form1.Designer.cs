namespace StemCrossSection
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblFolder = new System.Windows.Forms.Label();
            this.txtFolder = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.fbdImages = new System.Windows.Forms.FolderBrowserDialog();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.rdbModel2 = new System.Windows.Forms.RadioButton();
            this.grbThreshold = new System.Windows.Forms.GroupBox();
            this.rdbModel1 = new System.Windows.Forms.RadioButton();
            this.grbThreshold.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblFolder
            // 
            this.lblFolder.AutoSize = true;
            this.lblFolder.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFolder.Location = new System.Drawing.Point(40, 51);
            this.lblFolder.Name = "lblFolder";
            this.lblFolder.Size = new System.Drawing.Size(139, 26);
            this.lblFolder.TabIndex = 3;
            this.lblFolder.Text = "Select folder:";
            // 
            // txtFolder
            // 
            this.txtFolder.Location = new System.Drawing.Point(185, 57);
            this.txtFolder.Name = "txtFolder";
            this.txtFolder.Size = new System.Drawing.Size(312, 20);
            this.txtFolder.TabIndex = 4;
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(519, 57);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(148, 23);
            this.btnBrowse.TabIndex = 5;
            this.btnBrowse.Text = "Browse ...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(673, 57);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(85, 23);
            this.btnOk.TabIndex = 6;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // txtLog
            // 
            this.txtLog.Location = new System.Drawing.Point(45, 104);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.Size = new System.Drawing.Size(452, 173);
            this.txtLog.TabIndex = 7;
            // 
            // rdbModel2
            // 
            this.rdbModel2.AutoSize = true;
            this.rdbModel2.Location = new System.Drawing.Point(6, 54);
            this.rdbModel2.Name = "rdbModel2";
            this.rdbModel2.Size = new System.Drawing.Size(112, 17);
            this.rdbModel2.TabIndex = 8;
            this.rdbModel2.Text = "Threshold model 2";
            this.rdbModel2.UseVisualStyleBackColor = true;
            // 
            // grbThreshold
            // 
            this.grbThreshold.Controls.Add(this.rdbModel1);
            this.grbThreshold.Controls.Add(this.rdbModel2);
            this.grbThreshold.Location = new System.Drawing.Point(519, 104);
            this.grbThreshold.Name = "grbThreshold";
            this.grbThreshold.Size = new System.Drawing.Size(200, 83);
            this.grbThreshold.TabIndex = 10;
            this.grbThreshold.TabStop = false;
            this.grbThreshold.Text = "Select threshold model";
            // 
            // rdbModel1
            // 
            this.rdbModel1.AutoSize = true;
            this.rdbModel1.Checked = true;
            this.rdbModel1.Location = new System.Drawing.Point(6, 31);
            this.rdbModel1.Name = "rdbModel1";
            this.rdbModel1.Size = new System.Drawing.Size(112, 17);
            this.rdbModel1.TabIndex = 10;
            this.rdbModel1.TabStop = true;
            this.rdbModel1.Text = "Threshold model 1";
            this.rdbModel1.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(854, 349);
            this.Controls.Add(this.grbThreshold);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.txtFolder);
            this.Controls.Add(this.lblFolder);
            this.Name = "Form1";
            this.Text = "CrossSecCalculator";
            this.grbThreshold.ResumeLayout(false);
            this.grbThreshold.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label lblFolder;
        private System.Windows.Forms.TextBox txtFolder;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.FolderBrowserDialog fbdImages;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.RadioButton rdbModel2;
        private System.Windows.Forms.GroupBox grbThreshold;
        private System.Windows.Forms.RadioButton rdbModel1;
    }
}

