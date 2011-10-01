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
            this.progessBar = new System.Windows.Forms.ToolStripProgressBar();
            this.toolbarStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolbarStrip
            // 
            this.toolbarStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel,
            this.progessBar});
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
            // progessBar
            // 
            this.progessBar.Name = "progessBar";
            this.progessBar.Size = new System.Drawing.Size(100, 16);
            this.progessBar.Visible = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(490, 262);
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
        private System.Windows.Forms.ToolStripProgressBar progessBar;
    }
}

