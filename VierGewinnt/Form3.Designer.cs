
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
            this.txB_1.Location = new System.Drawing.Point(148, 56);
            this.txB_1.Name = "txB_1";
            this.txB_1.Size = new System.Drawing.Size(100, 23);
            this.txB_1.TabIndex = 2;
            // 
            // btn_Suchen
            // 
            this.btn_Suchen.Location = new System.Drawing.Point(51, 138);
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
            this.progressBar1.Maximum = 100;
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(798, 23);
            this.progressBar1.Step = 1;
            this.progressBar1.TabIndex = 4;
            this.progressBar1.UseWaitCursor = true;
            // 
            // btn_cancel
            // 
            this.btn_cancel.Location = new System.Drawing.Point(80, 183);
            this.btn_cancel.Name = "btn_cancel";
            this.btn_cancel.Size = new System.Drawing.Size(75, 23);
            this.btn_cancel.TabIndex = 5;
            this.btn_cancel.Text = "Cancel";
            this.btn_cancel.UseVisualStyleBackColor = true;
            this.btn_cancel.Click += new System.EventHandler(this.btn_cancel_Click);
            // 
            // Form3
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btn_cancel);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.btn_Suchen);
            this.Controls.Add(this.txB_1);
            this.Controls.Add(this.btn_Test2);
            this.Controls.Add(this.btn_Test);
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
    }
}