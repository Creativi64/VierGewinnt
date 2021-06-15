
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
            this.btn_Suchen = new System.Windows.Forms.Button();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.btn_cancel = new System.Windows.Forms.Button();
            this.LiB_GefundenenEndPoints = new System.Windows.Forms.ListBox();
            this.SpielHosten = new System.Windows.Forms.Button();
            this.btn_ConnectTo = new System.Windows.Forms.Button();
            this.txB_VerbindenIP = new System.Windows.Forms.TextBox();
            this.lab_IPeingabeHier = new System.Windows.Forms.Label();
            this.btn_ZumMenue = new System.Windows.Forms.Button();
            this.lab_Timer = new System.Windows.Forms.Label();
            this.lab_Player = new System.Windows.Forms.Label();
            this.lab_VerbundenMit = new System.Windows.Forms.Label();
            this.lab_MeineIp = new System.Windows.Forms.Label();
            this.lab_NotResponding = new System.Windows.Forms.Label();
            this.lab_Info = new System.Windows.Forms.Label();
            this.backgroundWorker2 = new System.ComponentModel.BackgroundWorker();
            this.SuspendLayout();
            // 
            // btn_Suchen
            // 
            this.btn_Suchen.Location = new System.Drawing.Point(2, 36);
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
            this.progressBar1.Location = new System.Drawing.Point(2, 536);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(779, 23);
            this.progressBar1.Step = 1;
            this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar1.TabIndex = 4;
            this.progressBar1.UseWaitCursor = true;
            this.progressBar1.Visible = false;
            // 
            // btn_cancel
            // 
            this.btn_cancel.Location = new System.Drawing.Point(83, 36);
            this.btn_cancel.Name = "btn_cancel";
            this.btn_cancel.Size = new System.Drawing.Size(75, 23);
            this.btn_cancel.TabIndex = 5;
            this.btn_cancel.Text = "Cancel";
            this.btn_cancel.UseVisualStyleBackColor = true;
            this.btn_cancel.Visible = false;
            this.btn_cancel.Click += new System.EventHandler(this.btn_cancel_Click);
            // 
            // LiB_GefundenenEndPoints
            // 
            this.LiB_GefundenenEndPoints.FormattingEnabled = true;
            this.LiB_GefundenenEndPoints.ItemHeight = 15;
            this.LiB_GefundenenEndPoints.Location = new System.Drawing.Point(178, 10);
            this.LiB_GefundenenEndPoints.Margin = new System.Windows.Forms.Padding(1);
            this.LiB_GefundenenEndPoints.Name = "LiB_GefundenenEndPoints";
            this.LiB_GefundenenEndPoints.Size = new System.Drawing.Size(164, 79);
            this.LiB_GefundenenEndPoints.TabIndex = 6;
            this.LiB_GefundenenEndPoints.Visible = false;
            // 
            // SpielHosten
            // 
            this.SpielHosten.Location = new System.Drawing.Point(2, 68);
            this.SpielHosten.Margin = new System.Windows.Forms.Padding(1);
            this.SpielHosten.Name = "SpielHosten";
            this.SpielHosten.Size = new System.Drawing.Size(97, 21);
            this.SpielHosten.TabIndex = 7;
            this.SpielHosten.Text = "ServerHosten";
            this.SpielHosten.UseVisualStyleBackColor = true;
            this.SpielHosten.Click += new System.EventHandler(this.ServerHosten_Click);
            // 
            // btn_ConnectTo
            // 
            this.btn_ConnectTo.Location = new System.Drawing.Point(2, 141);
            this.btn_ConnectTo.Margin = new System.Windows.Forms.Padding(1);
            this.btn_ConnectTo.Name = "btn_ConnectTo";
            this.btn_ConnectTo.Size = new System.Drawing.Size(79, 21);
            this.btn_ConnectTo.TabIndex = 8;
            this.btn_ConnectTo.Text = "Connect";
            this.btn_ConnectTo.UseVisualStyleBackColor = true;
            this.btn_ConnectTo.Click += new System.EventHandler(this.btn_ConnectTo_Click);
            // 
            // txB_VerbindenIP
            // 
            this.txB_VerbindenIP.Location = new System.Drawing.Point(2, 116);
            this.txB_VerbindenIP.Margin = new System.Windows.Forms.Padding(1);
            this.txB_VerbindenIP.Name = "txB_VerbindenIP";
            this.txB_VerbindenIP.Size = new System.Drawing.Size(147, 23);
            this.txB_VerbindenIP.TabIndex = 9;
            // 
            // lab_IPeingabeHier
            // 
            this.lab_IPeingabeHier.AutoSize = true;
            this.lab_IPeingabeHier.Location = new System.Drawing.Point(2, 100);
            this.lab_IPeingabeHier.Name = "lab_IPeingabeHier";
            this.lab_IPeingabeHier.Size = new System.Drawing.Size(147, 15);
            this.lab_IPeingabeHier.TabIndex = 13;
            this.lab_IPeingabeHier.Text = "Zu Welecher Ip verbinden?";
            // 
            // btn_ZumMenue
            // 
            this.btn_ZumMenue.BackColor = System.Drawing.Color.Transparent;
            this.btn_ZumMenue.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_ZumMenue.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.btn_ZumMenue.Location = new System.Drawing.Point(2, 7);
            this.btn_ZumMenue.Name = "btn_ZumMenue";
            this.btn_ZumMenue.Size = new System.Drawing.Size(75, 23);
            this.btn_ZumMenue.TabIndex = 14;
            this.btn_ZumMenue.Text = "ZumMenue";
            this.btn_ZumMenue.UseVisualStyleBackColor = false;
            this.btn_ZumMenue.Click += new System.EventHandler(this.btn_ZumMenue_Click);
            // 
            // lab_Timer
            // 
            this.lab_Timer.AutoSize = true;
            this.lab_Timer.Location = new System.Drawing.Point(12, 294);
            this.lab_Timer.Name = "lab_Timer";
            this.lab_Timer.Size = new System.Drawing.Size(0, 15);
            this.lab_Timer.TabIndex = 17;
            // 
            // lab_Player
            // 
            this.lab_Player.AutoSize = true;
            this.lab_Player.Location = new System.Drawing.Point(12, 275);
            this.lab_Player.Name = "lab_Player";
            this.lab_Player.Size = new System.Drawing.Size(0, 15);
            this.lab_Player.TabIndex = 16;
            // 
            // lab_VerbundenMit
            // 
            this.lab_VerbundenMit.AutoSize = true;
            this.lab_VerbundenMit.Location = new System.Drawing.Point(346, 7);
            this.lab_VerbundenMit.Name = "lab_VerbundenMit";
            this.lab_VerbundenMit.Size = new System.Drawing.Size(88, 15);
            this.lab_VerbundenMit.TabIndex = 18;
            this.lab_VerbundenMit.Text = "Verbunden Mit:";
            // 
            // lab_MeineIp
            // 
            this.lab_MeineIp.AutoSize = true;
            this.lab_MeineIp.Location = new System.Drawing.Point(535, 6);
            this.lab_MeineIp.Name = "lab_MeineIp";
            this.lab_MeineIp.Size = new System.Drawing.Size(53, 15);
            this.lab_MeineIp.TabIndex = 19;
            this.lab_MeineIp.Text = "MeineIP:";
            // 
            // lab_NotResponding
            // 
            this.lab_NotResponding.AutoSize = true;
            this.lab_NotResponding.BackColor = System.Drawing.Color.DarkRed;
            this.lab_NotResponding.Font = new System.Drawing.Font("Cascadia Code", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lab_NotResponding.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.lab_NotResponding.Location = new System.Drawing.Point(256, 498);
            this.lab_NotResponding.Name = "lab_NotResponding";
            this.lab_NotResponding.Size = new System.Drawing.Size(223, 35);
            this.lab_NotResponding.TabIndex = 20;
            this.lab_NotResponding.Text = "It still runs";
            this.lab_NotResponding.Visible = false;
            // 
            // lab_Info
            // 
            this.lab_Info.AutoSize = true;
            this.lab_Info.Location = new System.Drawing.Point(2, 167);
            this.lab_Info.Name = "lab_Info";
            this.lab_Info.Size = new System.Drawing.Size(26, 15);
            this.lab_Info.TabIndex = 21;
            this.lab_Info.Text = "idel";
            // 
            // Form3
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.lab_Info);
            this.Controls.Add(this.lab_NotResponding);
            this.Controls.Add(this.lab_MeineIp);
            this.Controls.Add(this.lab_VerbundenMit);
            this.Controls.Add(this.lab_Timer);
            this.Controls.Add(this.lab_Player);
            this.Controls.Add(this.btn_ZumMenue);
            this.Controls.Add(this.lab_IPeingabeHier);
            this.Controls.Add(this.txB_VerbindenIP);
            this.Controls.Add(this.btn_ConnectTo);
            this.Controls.Add(this.SpielHosten);
            this.Controls.Add(this.LiB_GefundenenEndPoints);
            this.Controls.Add(this.btn_cancel);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.btn_Suchen);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "Form3";
            this.Text = "Play Over Network";
            this.ResizeEnd += new System.EventHandler(this.Form3_ResizeEnd);
            this.Click += new System.EventHandler(this.Form3_Click);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.Form3_Paint);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Form3_MouseMove);
            this.Resize += new System.EventHandler(this.Form3_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btn_Suchen;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button btn_cancel;
 
        private System.Windows.Forms.ListBox LiB_GefundenenEndPoints;
        private System.Windows.Forms.Button SpielHosten;
        private System.Windows.Forms.Button btn_ConnectTo;
        private System.Windows.Forms.TextBox txB_VerbindenIP;
        private System.Windows.Forms.Label lab_IPeingabeHier;
        private System.Windows.Forms.Button btn_ZumMenue;
        private System.Windows.Forms.Label lab_Timer;
        private System.Windows.Forms.Label lab_Player;
        private System.Windows.Forms.Label lab_VerbundenMit;
        private System.Windows.Forms.Label lab_MeineIp;
        private System.Windows.Forms.Label lab_NotResponding;
        private System.Windows.Forms.Label lab_Info;
        private System.ComponentModel.BackgroundWorker backgroundWorker2;
    }
}