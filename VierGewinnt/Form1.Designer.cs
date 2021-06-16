
namespace VierGewinnt
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.btn_Play = new System.Windows.Forms.Button();
            this.btn_Network = new System.Windows.Forms.Button();
            this.btn_Quit = new System.Windows.Forms.Button();
            this.trackBarX = new System.Windows.Forms.TrackBar();
            this.trackBarY = new System.Windows.Forms.TrackBar();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.textBox1 = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarY)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_Play
            // 
            this.btn_Play.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btn_Play.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btn_Play.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btn_Play.Location = new System.Drawing.Point(306, 360);
            this.btn_Play.Margin = new System.Windows.Forms.Padding(0);
            this.btn_Play.Name = "btn_Play";
            this.btn_Play.Size = new System.Drawing.Size(200, 40);
            this.btn_Play.TabIndex = 0;
            this.btn_Play.Text = "Play";
            this.btn_Play.UseVisualStyleBackColor = true;
            this.btn_Play.Click += new System.EventHandler(this.Btn_Play_Click);
            // 
            // btn_Network
            // 
            this.btn_Network.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btn_Network.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btn_Network.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btn_Network.Location = new System.Drawing.Point(513, 360);
            this.btn_Network.Name = "btn_Network";
            this.btn_Network.Size = new System.Drawing.Size(200, 40);
            this.btn_Network.TabIndex = 3;
            this.btn_Network.Text = "Übers Netzwerk Spielen";
            this.btn_Network.UseVisualStyleBackColor = true;
            this.btn_Network.Click += new System.EventHandler(this.Btn_Network_Click);
            // 
            // btn_Quit
            // 
            this.btn_Quit.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btn_Quit.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btn_Quit.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btn_Quit.Location = new System.Drawing.Point(99, 360);
            this.btn_Quit.Margin = new System.Windows.Forms.Padding(10);
            this.btn_Quit.Name = "btn_Quit";
            this.btn_Quit.Size = new System.Drawing.Size(200, 40);
            this.btn_Quit.TabIndex = 4;
            this.btn_Quit.Text = "Quit";
            this.btn_Quit.UseVisualStyleBackColor = true;
            this.btn_Quit.Click += new System.EventHandler(this.Btn_Quit_Click);
            // 
            // trackBarX
            // 
            this.trackBarX.BackColor = System.Drawing.Color.White;
            this.trackBarX.Location = new System.Drawing.Point(297, 309);
            this.trackBarX.Maximum = 16;
            this.trackBarX.Name = "trackBarX";
            this.trackBarX.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.trackBarX.Size = new System.Drawing.Size(176, 45);
            this.trackBarX.TabIndex = 7;
            this.trackBarX.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
            this.trackBarX.Scroll += new System.EventHandler(this.TrackBarX_Scroll);
            // 
            // trackBarY
            // 
            this.trackBarY.BackColor = System.Drawing.Color.White;
            this.trackBarY.Location = new System.Drawing.Point(512, 96);
            this.trackBarY.Maximum = 16;
            this.trackBarY.Name = "trackBarY";
            this.trackBarY.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.trackBarY.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.trackBarY.RightToLeftLayout = true;
            this.trackBarY.Size = new System.Drawing.Size(45, 176);
            this.trackBarY.TabIndex = 8;
            this.trackBarY.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
            this.trackBarY.Scroll += new System.EventHandler(this.TrackBarY_Scroll);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Location = new System.Drawing.Point(512, 267);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(27, 56);
            this.panel1.TabIndex = 9;
            // 
            // panel2
            // 
            this.panel2.Location = new System.Drawing.Point(-44, 30);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(64, 29);
            this.panel2.TabIndex = 10;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.panel4);
            this.panel3.Location = new System.Drawing.Point(468, 309);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(71, 29);
            this.panel3.TabIndex = 11;
            // 
            // panel4
            // 
            this.panel4.Location = new System.Drawing.Point(-44, 30);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(64, 29);
            this.panel4.TabIndex = 10;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("Segoe UI Semibold", 40F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label1.ForeColor = System.Drawing.Color.Blue;
            this.label1.ImageKey = "(keine)";
            this.label1.Location = new System.Drawing.Point(235, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(349, 72);
            this.label1.TabIndex = 12;
            this.label1.Text = "Vier Gewinnt";
            // 
            // textBox1
            // 
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox1.Font = new System.Drawing.Font("Segoe UI Semibold", 40F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.textBox1.ForeColor = System.Drawing.Color.Blue;
            this.textBox1.Location = new System.Drawing.Point(200, 28);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(150, 71);
            this.textBox1.TabIndex = 13;
            this.textBox1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBox1.TextChanged += new System.EventHandler(this.TextBox1_TextChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(794, 430);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.trackBarY);
            this.Controls.Add(this.trackBarX);
            this.Controls.Add(this.btn_Quit);
            this.Controls.Add(this.btn_Network);
            this.Controls.Add(this.btn_Play);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(804, 444);
            this.Name = "Form1";
            this.Text = "Main Menu             ";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.Form1_Paint);
            ((System.ComponentModel.ISupportInitialize)(this.trackBarX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarY)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_Play;
        private System.Windows.Forms.Button btn_Network;
        private System.Windows.Forms.Button btn_Quit;
        private System.Windows.Forms.TrackBar trackBarX;
        private System.Windows.Forms.TrackBar trackBarY;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Label label1;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.TextBox textBox1;
    }
}

