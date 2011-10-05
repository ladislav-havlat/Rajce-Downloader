namespace LH.Apps.RajceDownloader
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
            this.startDownloadButton = new System.Windows.Forms.Button();
            this.abortButton = new System.Windows.Forms.Button();
            this.pageURLTextBox = new System.Windows.Forms.TextBox();
            this.pageURLGroupBox = new System.Windows.Forms.GroupBox();
            this.rajceLogo = new System.Windows.Forms.PictureBox();
            this.targetDirGroupBox = new System.Windows.Forms.GroupBox();
            this.selectTargetDirButton = new System.Windows.Forms.Button();
            this.targetDirTextBox = new System.Windows.Forms.TextBox();
            this.targetDirDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.toolbarStrip.SuspendLayout();
            this.pageURLGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.rajceLogo)).BeginInit();
            this.targetDirGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolbarStrip
            // 
            this.toolbarStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel,
            this.progressBar});
            this.toolbarStrip.Location = new System.Drawing.Point(0, 196);
            this.toolbarStrip.Name = "toolbarStrip";
            this.toolbarStrip.Size = new System.Drawing.Size(484, 22);
            this.toolbarStrip.TabIndex = 0;
            this.toolbarStrip.Text = "statusStrip1";
            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(469, 17);
            this.statusLabel.Spring = true;
            this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // progressBar
            // 
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(100, 16);
            this.progressBar.Visible = false;
            // 
            // startDownloadButton
            // 
            this.startDownloadButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.startDownloadButton.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.startDownloadButton.Location = new System.Drawing.Point(246, 151);
            this.startDownloadButton.Margin = new System.Windows.Forms.Padding(8);
            this.startDownloadButton.Name = "startDownloadButton";
            this.startDownloadButton.Size = new System.Drawing.Size(105, 28);
            this.startDownloadButton.TabIndex = 1;
            this.startDownloadButton.Text = "Stáhnout";
            this.startDownloadButton.UseVisualStyleBackColor = true;
            this.startDownloadButton.Click += new System.EventHandler(this.startDownloadButton_Click);
            // 
            // abortButton
            // 
            this.abortButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.abortButton.Location = new System.Drawing.Point(367, 151);
            this.abortButton.Margin = new System.Windows.Forms.Padding(8);
            this.abortButton.Name = "abortButton";
            this.abortButton.Size = new System.Drawing.Size(105, 28);
            this.abortButton.TabIndex = 3;
            this.abortButton.Text = "Zastavit";
            this.abortButton.UseVisualStyleBackColor = true;
            this.abortButton.Click += new System.EventHandler(this.abortButton_Click);
            // 
            // pageURLTextBox
            // 
            this.pageURLTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pageURLTextBox.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.pageURLTextBox.ForeColor = System.Drawing.SystemColors.GrayText;
            this.pageURLTextBox.Location = new System.Drawing.Point(11, 24);
            this.pageURLTextBox.Name = "pageURLTextBox";
            this.pageURLTextBox.Size = new System.Drawing.Size(438, 20);
            this.pageURLTextBox.TabIndex = 0;
            this.pageURLTextBox.Text = "http://jmeno.rajce.idnes.cz/album/";
            this.pageURLTextBox.Enter += new System.EventHandler(this.pageURLTextBox_Enter);
            // 
            // pageURLGroupBox
            // 
            this.pageURLGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pageURLGroupBox.Controls.Add(this.pageURLTextBox);
            this.pageURLGroupBox.Location = new System.Drawing.Point(12, 12);
            this.pageURLGroupBox.Name = "pageURLGroupBox";
            this.pageURLGroupBox.Padding = new System.Windows.Forms.Padding(8);
            this.pageURLGroupBox.Size = new System.Drawing.Size(460, 58);
            this.pageURLGroupBox.TabIndex = 4;
            this.pageURLGroupBox.TabStop = false;
            this.pageURLGroupBox.Text = " Adresa webového alba ";
            // 
            // rajceLogo
            // 
            this.rajceLogo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.rajceLogo.Cursor = System.Windows.Forms.Cursors.Hand;
            this.rajceLogo.Image = global::LH.Apps.RajceDownloader.Properties.Resources.rajce_net_100;
            this.rajceLogo.Location = new System.Drawing.Point(12, 143);
            this.rajceLogo.Name = "rajceLogo";
            this.rajceLogo.Size = new System.Drawing.Size(100, 36);
            this.rajceLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.rajceLogo.TabIndex = 5;
            this.rajceLogo.TabStop = false;
            this.rajceLogo.Click += new System.EventHandler(this.rajceLogo_Click);
            // 
            // targetDirGroupBox
            // 
            this.targetDirGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.targetDirGroupBox.Controls.Add(this.selectTargetDirButton);
            this.targetDirGroupBox.Controls.Add(this.targetDirTextBox);
            this.targetDirGroupBox.Location = new System.Drawing.Point(12, 76);
            this.targetDirGroupBox.Name = "targetDirGroupBox";
            this.targetDirGroupBox.Padding = new System.Windows.Forms.Padding(8);
            this.targetDirGroupBox.Size = new System.Drawing.Size(460, 58);
            this.targetDirGroupBox.TabIndex = 6;
            this.targetDirGroupBox.TabStop = false;
            this.targetDirGroupBox.Text = " Cílová složka ";
            // 
            // selectTargetDirButton
            // 
            this.selectTargetDirButton.Location = new System.Drawing.Point(419, 24);
            this.selectTargetDirButton.Name = "selectTargetDirButton";
            this.selectTargetDirButton.Size = new System.Drawing.Size(28, 20);
            this.selectTargetDirButton.TabIndex = 1;
            this.selectTargetDirButton.Text = "...";
            this.selectTargetDirButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.selectTargetDirButton.UseVisualStyleBackColor = true;
            this.selectTargetDirButton.Click += new System.EventHandler(this.selectTargetDirButton_Click);
            // 
            // targetDirTextBox
            // 
            this.targetDirTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.targetDirTextBox.Location = new System.Drawing.Point(11, 24);
            this.targetDirTextBox.Name = "targetDirTextBox";
            this.targetDirTextBox.Size = new System.Drawing.Size(402, 20);
            this.targetDirTextBox.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 218);
            this.Controls.Add(this.targetDirGroupBox);
            this.Controls.Add(this.rajceLogo);
            this.Controls.Add(this.pageURLGroupBox);
            this.Controls.Add(this.abortButton);
            this.Controls.Add(this.startDownloadButton);
            this.Controls.Add(this.toolbarStrip);
            this.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(10000, 400);
            this.MinimumSize = new System.Drawing.Size(500, 194);
            this.Name = "MainForm";
            this.Text = "Rajče Downloader";
            this.toolbarStrip.ResumeLayout(false);
            this.toolbarStrip.PerformLayout();
            this.pageURLGroupBox.ResumeLayout(false);
            this.pageURLGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.rajceLogo)).EndInit();
            this.targetDirGroupBox.ResumeLayout(false);
            this.targetDirGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip toolbarStrip;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.ToolStripProgressBar progressBar;
        private System.Windows.Forms.Button startDownloadButton;
        private System.Windows.Forms.Button abortButton;
        private System.Windows.Forms.TextBox pageURLTextBox;
        private System.Windows.Forms.GroupBox pageURLGroupBox;
        private System.Windows.Forms.PictureBox rajceLogo;
        private System.Windows.Forms.GroupBox targetDirGroupBox;
        private System.Windows.Forms.Button selectTargetDirButton;
        private System.Windows.Forms.TextBox targetDirTextBox;
        private System.Windows.Forms.FolderBrowserDialog targetDirDialog;
    }
}

