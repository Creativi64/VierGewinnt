
namespace VierGewinnt
{
    partial class Form3
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
            this.btn_Test = new System.Windows.Forms.Button();
            this.btn_Test2 = new System.Windows.Forms.Button();
            this.txB_1 = new System.Windows.Forms.TextBox();
            this.btn_Suchen = new System.Windows.Forms.Button();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.btn_cancel = new System.Windows.Forms.Button();
            this.LiB_GefundenenEndPoints = new System.Windows.Forms.ListBox();
            this.ServerHosten = new System.Windows.Forms.Button();
            this.btn_ConnectTo = new System.Windows.Forms.Button();
            this.txB_VerbindenIP = new System.Windows.Forms.TextBox();
            this.BcWork_Server = new System.ComponentModel.BackgroundWorker();
            this.SuspendLayout();
            // 
            // btn_Test
            // 
            this.btn_Test.Location = new System.Drawing.Point(26, 30);
            this.btn_Test.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
            this.btn_Test.Name = "btn_Test";
            this.btn_Test.Size = new System.Drawing.Size(161, 57);
            this.btn_Test.TabIndex = 0;
            this.btn_Test.Text = "Test";
            this.btn_Test.UseVisualStyleBackColor = true;
            this.btn_Test.Click += new System.EventHandler(this.btn_Test_Click);
            // 
            // btn_Test2
            // 
            this.btn_Test2.Location = new System.Drawing.Point(26, 101);
            this.btn_Test2.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
            this.btn_Test2.Name = "btn_Test2";
            this.btn_Test2.Size = new System.Drawing.Size(161, 57);
            this.btn_Test2.TabIndex = 1;
            this.btn_Test2.Text = "Test2";
            this.btn_Test2.UseVisualStyleBackColor = true;
            // 
            // txB_1
            // 
            this.txB_1.Location = new System.Drawing.Point(317, 138);
            this.txB_1.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
            this.txB_1.Name = "txB_1";
            this.txB_1.Size = new System.Drawing.Size(210, 43);
            this.txB_1.TabIndex = 2;
            // 
            // btn_Suchen
            // 
            this.btn_Suchen.Location = new System.Drawing.Point(109, 340);
            this.btn_Suchen.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
            this.btn_Suchen.Name = "btn_Suchen";
            this.btn_Suchen.Size = new System.Drawing.Size(161, 57);
            this.btn_Suchen.TabIndex = 3;
            this.btn_Suchen.Text = "Serach";
            this.btn_Suchen.UseVisualStyleBackColor = true;
            this.btn_Suchen.Click += new System.EventHandler(this.btn_Suchen_Click);
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.WorkerReportsProgress = true;
            this.backgroundWorker1.WorkerSupportsCancellation = true;
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            this.backgroundWorker1.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker1_ProgressChanged);
            this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(4, 1051);
            this.progressBar1.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(1710, 57);
            this.progressBar1.Step = 1;
            this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar1.TabIndex = 4;
            this.progressBar1.UseWaitCursor = true;
            this.progressBar1.Visible = false;
            // 
            // btn_cancel
            // 
            this.btn_cancel.Location = new System.Drawing.Point(282, 340);
            this.btn_cancel.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
            this.btn_cancel.Name = "btn_cancel";
            this.btn_cancel.Size = new System.Drawing.Size(161, 57);
            this.btn_cancel.TabIndex = 5;
            this.btn_cancel.Text = "Cancel";
            this.btn_cancel.UseVisualStyleBackColor = true;
            this.btn_cancel.Click += new System.EventHandler(this.btn_cancel_Click);
            // 
            // LiB_GefundenenEndPoints
            // 
            this.LiB_GefundenenEndPoints.FormattingEnabled = true;
            this.LiB_GefundenenEndPoints.ItemHeight = 37;
            this.LiB_GefundenenEndPoints.Location = new System.Drawing.Point(823, 324);
            this.LiB_GefundenenEndPoints.Name = "LiB_GefundenenEndPoints";
            this.LiB_GefundenenEndPoints.Size = new System.Drawing.Size(270, 189);
            this.LiB_GefundenenEndPoints.TabIndex = 6;
            // 
            // ServerHosten
            // 
            this.ServerHosten.Location = new System.Drawing.Point(109, 461);
            this.ServerHosten.Name = "ServerHosten";
            this.ServerHosten.Size = new System.Drawing.Size(208, 52);
            this.ServerHosten.TabIndex = 7;
            this.ServerHosten.Text = "ServerHosten";
            this.ServerHosten.UseVisualStyleBackColor = true;
            this.ServerHosten.Click += new System.EventHandler(this.ServerHosten_Click);
            // 
            // btn_ConnectTo
            // 
            this.btn_ConnectTo.Location = new System.Drawing.Point(109, 561);
            this.btn_ConnectTo.Name = "btn_ConnectTo";
            this.btn_ConnectTo.Size = new System.Drawing.Size(169, 52);
            this.btn_ConnectTo.TabIndex = 8;
            this.btn_ConnectTo.Text = "ConnectTo:";
            this.btn_ConnectTo.UseVisualStyleBackColor = true;
            this.btn_ConnectTo.Click += new System.EventHandler(this.btn_ConnectTo_Click);
            // 
            // txB_VerbindenIP
            // 
            this.txB_VerbindenIP.Location = new System.Drawing.Point(302, 570);
            this.txB_VerbindenIP.Name = "txB_VerbindenIP";
            this.txB_VerbindenIP.Size = new System.Drawing.Size(225, 43);
            this.txB_VerbindenIP.TabIndex = 9;
            // 
            // BcWork_Server
            // 
            this.BcWork_Server.WorkerSupportsCancellation = true;
            this.BcWork_Server.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker2_DoWork);
            this.BcWork_Server.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker2_RunWorkerCompleted);
            // 
            // Form3
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(15F, 37F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1714, 1110);
            this.Controls.Add(this.txB_VerbindenIP);
            this.Controls.Add(this.btn_ConnectTo);
            this.Controls.Add(this.ServerHosten);
            this.Controls.Add(this.LiB_GefundenenEndPoints);
            this.Controls.Add(this.btn_cancel);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.btn_Suchen);
            this.Controls.Add(this.txB_1);
            this.Controls.Add(this.btn_Test2);
            this.Controls.Add(this.btn_Test);
            this.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
            this.Name = "Form3";
            this.Text = "Play Over Network";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_Test;
        private System.Windows.Forms.Button btn_Test2;
        private System.Windows.Forms.TextBox txB_1;
        private System.Windows.Forms.Button btn_Suchen;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button btn_cancel;
 
        private System.Windows.Forms.ListBox LiB_GefundenenEndPoints;
        private System.Windows.Forms.Button ServerHosten;
        private System.Windows.Forms.Button btn_ConnectTo;
        private System.Windows.Forms.TextBox txB_VerbindenIP;
        private System.ComponentModel.BackgroundWorker BcWork_Server;
    }
}