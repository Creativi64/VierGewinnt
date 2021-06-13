
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form3));
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
            this.txB_Empfangen = new System.Windows.Forms.TextBox();
            this.txB_Senden = new System.Windows.Forms.TextBox();
            this.lab_Info = new System.Windows.Forms.Label();
            this.btn_Senden = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btn_Test
            // 
            this.btn_Test.Location = new System.Drawing.Point(12, 12);
            this.btn_Test.Name = "btn_Test";
            this.btn_Test.Size = new System.Drawing.Size(75, 23);
            this.btn_Test.TabIndex = 0;
            this.btn_Test.Text = "Test";
            this.btn_Test.UseVisualStyleBackColor = true;
            this.btn_Test.Click += new System.EventHandler(this.btn_Test_Click);
            // 
            // btn_Test2
            // 
            this.btn_Test2.Location = new System.Drawing.Point(12, 41);
            this.btn_Test2.Name = "btn_Test2";
            this.btn_Test2.Size = new System.Drawing.Size(75, 23);
            this.btn_Test2.TabIndex = 1;
            this.btn_Test2.Text = "Test2";
            this.btn_Test2.UseVisualStyleBackColor = true;
            // 
            // txB_1
            // 
            this.txB_1.Location = new System.Drawing.Point(100, 12);
            this.txB_1.Name = "txB_1";
            this.txB_1.Size = new System.Drawing.Size(100, 23);
            this.txB_1.TabIndex = 2;
            // 
            // btn_Suchen
            // 
            this.btn_Suchen.Location = new System.Drawing.Point(12, 84);
            this.btn_Suchen.Name = "btn_Suchen";
            this.btn_Suchen.Size = new System.Drawing.Size(75, 23);
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
            this.progressBar1.Location = new System.Drawing.Point(2, 426);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(798, 23);
            this.progressBar1.Step = 1;
            this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar1.TabIndex = 4;
            this.progressBar1.UseWaitCursor = true;
            this.progressBar1.Visible = false;
            // 
            // btn_cancel
            // 
            this.btn_cancel.Location = new System.Drawing.Point(93, 84);
            this.btn_cancel.Name = "btn_cancel";
            this.btn_cancel.Size = new System.Drawing.Size(75, 23);
            this.btn_cancel.TabIndex = 5;
            this.btn_cancel.Text = "Cancel";
            this.btn_cancel.UseVisualStyleBackColor = true;
            this.btn_cancel.Click += new System.EventHandler(this.btn_cancel_Click);
            // 
            // LiB_GefundenenEndPoints
            // 
            this.LiB_GefundenenEndPoints.FormattingEnabled = true;
            this.LiB_GefundenenEndPoints.ItemHeight = 15;
            this.LiB_GefundenenEndPoints.Location = new System.Drawing.Point(384, 131);
            this.LiB_GefundenenEndPoints.Margin = new System.Windows.Forms.Padding(1);
            this.LiB_GefundenenEndPoints.Name = "LiB_GefundenenEndPoints";
            this.LiB_GefundenenEndPoints.Size = new System.Drawing.Size(128, 79);
            this.LiB_GefundenenEndPoints.TabIndex = 6;
            // 
            // ServerHosten
            // 
            this.ServerHosten.Location = new System.Drawing.Point(12, 111);
            this.ServerHosten.Margin = new System.Windows.Forms.Padding(1);
            this.ServerHosten.Name = "ServerHosten";
            this.ServerHosten.Size = new System.Drawing.Size(97, 21);
            this.ServerHosten.TabIndex = 7;
            this.ServerHosten.Text = "ServerHosten";
            this.ServerHosten.UseVisualStyleBackColor = true;
            this.ServerHosten.Click += new System.EventHandler(this.ServerHosten_Click);
            // 
            // btn_ConnectTo
            // 
            this.btn_ConnectTo.Location = new System.Drawing.Point(12, 147);
            this.btn_ConnectTo.Margin = new System.Windows.Forms.Padding(1);
            this.btn_ConnectTo.Name = "btn_ConnectTo";
            this.btn_ConnectTo.Size = new System.Drawing.Size(79, 21);
            this.btn_ConnectTo.TabIndex = 8;
            this.btn_ConnectTo.Text = "ConnectTo:";
            this.btn_ConnectTo.UseVisualStyleBackColor = true;
            this.btn_ConnectTo.Click += new System.EventHandler(this.btn_ConnectTo_Click);
            // 
            // txB_VerbindenIP
            // 
            this.txB_VerbindenIP.Location = new System.Drawing.Point(100, 145);
            this.txB_VerbindenIP.Margin = new System.Windows.Forms.Padding(1);
            this.txB_VerbindenIP.Name = "txB_VerbindenIP";
            this.txB_VerbindenIP.Size = new System.Drawing.Size(107, 23);
            this.txB_VerbindenIP.TabIndex = 9;
            // 
            // BcWork_Server
            // 
            this.BcWork_Server.WorkerSupportsCancellation = true;
            this.BcWork_Server.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker2_DoWork);
            this.BcWork_Server.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker2_RunWorkerCompleted);
            // 
            // txB_Empfangen
            // 
            this.txB_Empfangen.Location = new System.Drawing.Point(107, 226);
            this.txB_Empfangen.Name = "txB_Empfangen";
            this.txB_Empfangen.Size = new System.Drawing.Size(100, 23);
            this.txB_Empfangen.TabIndex = 10;
            // 
            // txB_Senden
            // 
            this.txB_Senden.Location = new System.Drawing.Point(107, 197);
            this.txB_Senden.Name = "txB_Senden";
            this.txB_Senden.Size = new System.Drawing.Size(100, 23);
            this.txB_Senden.TabIndex = 11;
            // 
            // lab_Info
            // 
            this.lab_Info.AutoSize = true;
            this.lab_Info.Location = new System.Drawing.Point(12, 178);
            this.lab_Info.Name = "lab_Info";
            this.lab_Info.Size = new System.Drawing.Size(26, 15);
            this.lab_Info.TabIndex = 12;
            this.lab_Info.Text = "Idel";
            // 
            // btn_Senden
            // 
            this.btn_Senden.Location = new System.Drawing.Point(12, 196);
            this.btn_Senden.Name = "btn_Senden";
            this.btn_Senden.Size = new System.Drawing.Size(75, 23);
            this.btn_Senden.TabIndex = 13;
            this.btn_Senden.Text = "Senden ->";
            this.btn_Senden.UseVisualStyleBackColor = true;
            this.btn_Senden.Click += new System.EventHandler(this.btn_Senden_Click);
            // 
            // Form3
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btn_Senden);
            this.Controls.Add(this.lab_Info);
            this.Controls.Add(this.txB_Senden);
            this.Controls.Add(this.txB_Empfangen);
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
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
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
        private System.Windows.Forms.TextBox txB_Empfangen;
        private System.Windows.Forms.TextBox txB_Senden;
        private System.Windows.Forms.Label lab_Info;
        private System.Windows.Forms.Button btn_Senden;
    }
}