﻿namespace LH.Apps.RajceDownloader
{
    partial class MainForm
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
            this.toolbarStrip = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.progressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.button1 = new System.Windows.Forms.Button();
            this.listBox = new System.Windows.Forms.ListBox();
            this.button2 = new System.Windows.Forms.Button();
            this.toolbarStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolbarStrip
            // 
            this.toolbarStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel,
            this.progressBar});
            this.toolbarStrip.Location = new System.Drawing.Point(0, 240);
            this.toolbarStrip.Name = "toolbarStrip";
            this.toolbarStrip.Size = new System.Drawing.Size(490, 22);
            this.toolbarStrip.TabIndex = 0;
            this.toolbarStrip.Text = "statusStrip1";
            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(475, 17);
            this.statusLabel.Spring = true;
            this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // progressBar
            // 
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(100, 16);
            this.progressBar.Visible = false;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // listBox
            // 
            this.listBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listBox.FormattingEnabled = true;
            this.listBox.Location = new System.Drawing.Point(12, 41);
            this.listBox.Name = "listBox";
            this.listBox.Size = new System.Drawing.Size(466, 186);
            this.listBox.TabIndex = 2;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(93, 12);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 3;
            this.button2.Text = "button2";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(490, 262);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.listBox);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.toolbarStrip);
            this.Name = "MainForm";
            this.Text = "Form1";
            this.toolbarStrip.ResumeLayout(false);
            this.toolbarStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip toolbarStrip;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.ToolStripProgressBar progressBar;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ListBox listBox;
        private System.Windows.Forms.Button button2;
    }
}

