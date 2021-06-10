
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
            this.btn_Play = new System.Windows.Forms.Button();
            this.lab1 = new System.Windows.Forms.Label();
            this.btn_Network = new System.Windows.Forms.Button();
            this.btn_Quit = new System.Windows.Forms.Button();
            this.chBox_Fullscreen = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // btn_Play
            // 
            this.btn_Play.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btn_Play.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btn_Play.Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btn_Play.Location = new System.Drawing.Point(216, 181);
            this.btn_Play.MinimumSize = new System.Drawing.Size(370, 60);
            this.btn_Play.Name = "btn_Play";
            this.btn_Play.Size = new System.Drawing.Size(370, 60);
            this.btn_Play.TabIndex = 0;
            this.btn_Play.Text = "Play";
            this.btn_Play.UseVisualStyleBackColor = true;
            this.btn_Play.Click += new System.EventHandler(this.btn_Play_Click);
            // 
            // lab1
            // 
            this.lab1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.lab1.Font = new System.Drawing.Font("Snap ITC", 48F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lab1.Location = new System.Drawing.Point(8, 0);
            this.lab1.Name = "lab1";
            this.lab1.Size = new System.Drawing.Size(794, 164);
            this.lab1.TabIndex = 1;
            this.lab1.Text = "Menue \r\nVier Gewinnt";
            this.lab1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btn_Network
            // 
            this.btn_Network.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btn_Network.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btn_Network.Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btn_Network.Location = new System.Drawing.Point(216, 242);
            this.btn_Network.MinimumSize = new System.Drawing.Size(370, 60);
            this.btn_Network.Name = "btn_Network";
            this.btn_Network.Size = new System.Drawing.Size(370, 60);
            this.btn_Network.TabIndex = 3;
            this.btn_Network.Text = "Übers Netzwerk Spielen";
            this.btn_Network.UseVisualStyleBackColor = true;
            this.btn_Network.Click += new System.EventHandler(this.btn_Network_Click);
            // 
            // btn_Quit
            // 
            this.btn_Quit.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btn_Quit.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btn_Quit.Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btn_Quit.Location = new System.Drawing.Point(216, 303);
            this.btn_Quit.Margin = new System.Windows.Forms.Padding(10);
            this.btn_Quit.MinimumSize = new System.Drawing.Size(370, 60);
            this.btn_Quit.Name = "btn_Quit";
            this.btn_Quit.Size = new System.Drawing.Size(370, 60);
            this.btn_Quit.TabIndex = 4;
            this.btn_Quit.Text = "Quit";
            this.btn_Quit.UseVisualStyleBackColor = true;
            this.btn_Quit.Click += new System.EventHandler(this.btn_Quit_Click);
            // 
            // chBox_Fullscreen
            // 
            this.chBox_Fullscreen.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.chBox_Fullscreen.AutoSize = true;
            this.chBox_Fullscreen.Location = new System.Drawing.Point(355, 376);
            this.chBox_Fullscreen.Name = "chBox_Fullscreen";
            this.chBox_Fullscreen.Size = new System.Drawing.Size(84, 19);
            this.chBox_Fullscreen.TabIndex = 6;
            this.chBox_Fullscreen.Text = "Fullscreen?";
            this.chBox_Fullscreen.UseVisualStyleBackColor = true;
            this.chBox_Fullscreen.CheckedChanged += new System.EventHandler(this.chBox_Fullscreen_CheckedChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(794, 451);
            this.Controls.Add(this.chBox_Fullscreen);
            this.Controls.Add(this.btn_Quit);
            this.Controls.Add(this.btn_Network);
            this.Controls.Add(this.lab1);
            this.Controls.Add(this.btn_Play);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(810, 490);
            this.Name = "Form1";
            this.Text = "Main Menu             ";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_Play;
        private System.Windows.Forms.Label lab1;
        private System.Windows.Forms.Button btn_Network;
        private System.Windows.Forms.Button btn_Quit;
        private System.Windows.Forms.CheckBox chBox_Fullscreen;
    }
}

