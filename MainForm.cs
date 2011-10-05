using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LH.Apps.RajceDownloader.Engine;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;

namespace LH.Apps.RajceDownloader
{
    public partial class MainForm : Form, IStatusSink, IPromptSink
    {
        private IAbortible abortible;
        private Downloader downloader;
        private PageParser pageParser;

        public MainForm()
        {
            InitializeComponent();
            SetStatusText(null);
            EnableUI(false);
        }

        #region IStatusSink Members
        /// <summary>
        /// Displays the progress bar, sets its bounds, its position to the beginning and sets the status bar
        /// text. Combines SetProgressBarBounds, SetProgressBarPos and SetStatusText into one operation.
        /// </summary>
        /// <param name="Min">Minimal progress bar position.</param>
        /// <param name="Max">Maximal progress bar position.</param>
        /// <param name="StatusText">The text to be displayed. If null, "Ready" status shall be displayed.</param>
        public void BeginOperation(int Min, int Max, string StatusText)
        {
            MethodInvoker sync = new MethodInvoker(delegate()
            {
                //optimization: the calls are already internally synchronized, but this performs all 
                //the operations doing one sync call only
                SetStatusText(StatusText);
                SetProgressBarBounds(Min, Max);
                SetProgressBarPos(Min);
                ShowProgressBar(Min != Max);
            });

            if (InvokeRequired)
                Invoke(sync);
            else
                sync();
        }

        /// <summary>
        /// Hides the progress bar and sets the status bar's text to "Ready".
        /// </summary>
        public void EndOperation()
        {
            MethodInvoker sync = new MethodInvoker(delegate()
            {
                //optimization: the calls are already internally synchronized, but this performs all 
                //the operations doing one sync call only
                ShowProgressBar(false);
                SetStatusText(null);
            });

            if (InvokeRequired)
                Invoke(sync);
            else
                sync();
        }

        /// <summary>
        /// Sets the progress bar's bounds.
        /// </summary>
        /// <param name="Min">Minimal progress bar position.</param>
        /// <param name="Max">Maximal progress bar position.</param>
        public void SetProgressBarBounds(int Min, int Max)
        {
            MethodInvoker sync = new MethodInvoker(delegate()
            {
                progressBar.Minimum = Min;
                progressBar.Maximum = Max;
            });

            if (InvokeRequired)
                Invoke(sync);
            else
                sync();
        }

        /// <summary>
        /// Sets the progress bar's position.
        /// </summary>
        /// <param name="Pos">Desired position of the progress bar.</param>
        public void SetProgressBarPos(int Pos)
        {
            MethodInvoker sync = new MethodInvoker(delegate()
            {
                progressBar.Value = Pos;
            });

            if (InvokeRequired)
                Invoke(sync);
            else
                sync();
        }

        /// <summary>
        /// Sets the text on a generic status bar.
        /// </summary>
        /// <param name="StatusText">The text to be displayed. If null, "Ready" status shall be displayed.</param>
        public void SetStatusText(string StatusText)
        {
            MethodInvoker sync = new MethodInvoker(delegate()
            {
                if (StatusText != null)
                    statusLabel.Text = StatusText;
                else
                    statusLabel.Text = Properties.Resources.Status_Ready;
            });

            if (InvokeRequired)
                Invoke(sync);
            else
                sync();
        }

        /// <summary>
        /// Shows or hides the progress bar.
        /// </summary>
        /// <param name="Show">True if the progress bar is to be shown, false otherwise.</param>
        public void ShowProgressBar(bool Show)
        {
            MethodInvoker sync = new MethodInvoker(delegate()
            {
                progressBar.Visible = Show;
            });

            if (InvokeRequired)
                Invoke(sync);
            else
                sync();
        }

        /// <summary>
        /// Increases the progress bar's position by Delta.
        /// </summary>
        /// <param name="Delta">Amount of progress to be increased by.</param>
        public void StepProgressBar(int Delta)
        {
            MethodInvoker sync = new MethodInvoker(delegate()
            {
                progressBar.Step = Delta;
                progressBar.PerformStep();
            });

            if (InvokeRequired)
                Invoke(sync);
            else
                sync();
        }
        #endregion

        #region IPromptSink Members

        public void Error(string message)
        {
            Error(message, Properties.Resources.Caption_Generic);
        }

        public void Error(string message, string caption)
        {
            Error(message, caption, MessageBoxButtons.OK);
        }

        public DialogResult Error(string message, MessageBoxButtons buttons)
        {
            return Error(message, Properties.Resources.Caption_Generic, buttons);
        }

        public DialogResult Error(string message, string caption, MessageBoxButtons buttons)
        {
            DialogResult result = DialogResult.None;

            MethodInvoker sync = new MethodInvoker(
                () => result = MessageBox.Show(message, caption, buttons, MessageBoxIcon.Error)
            );
            if (InvokeRequired)
                Invoke(sync);
            else
                sync();

            return result;
        }

        public DialogResult Question(string message, MessageBoxButtons buttons)
        {
            DialogResult result = DialogResult.None;

            MethodInvoker sync = new MethodInvoker(delegate()
                {
                    result = MessageBox.Show(
                        message, 
                        Properties.Resources.Caption_Generic, 
                        buttons, 
                        MessageBoxIcon.Question);
                }
            );
            if (InvokeRequired)
                Invoke(sync);
            else
                sync();

            return result;
        }

        #endregion

        private void EnableUI(bool busy)
        {
            button1.Enabled = !busy;
            button2.Enabled = busy;
            textBox1.Enabled = !busy;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            pageParser = new PageParser("http://magicontrol.rajce.idnes.cz/Vystavba_kanalizace_Vladislav_1/");
            //pageParser = new PageParser("file:///C:/Temp/album.html");
            pageParser.Finished += new EventHandler(pageParser_Finished);
            pageParser.BeginDownloadAndParse();
            abortible = pageParser;
            button1.Enabled = false;
        }

        private void pageParser_Finished(object sender, EventArgs e)
        {
            if (pageParser == null)
                return;

            var photos = pageParser.Select(p =>
                {
                    p.TargetPath = Path.GetFileName(p.SourceURL);
                    return p;
                }).ToArray();

            if (photos.Length > 0)
            {
                downloader = new Downloader();
                abortible = downloader;
                downloader.AddPhotos(photos);
                downloader.Finished += new EventHandler(downloader_Finished);
                downloader.BeginDownload();
            }
            else
                Invoke(new MethodInvoker(() => button1.Enabled = true));
        }

        private void downloader_Finished(object sender, EventArgs e)
        {
            abortible = null;
            if (downloader == null)
                return;
            Invoke(new MethodInvoker(() => button1.Enabled = true));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (abortible != null)
                abortible.Abort();
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            if (textBox1.Tag == null)
            {
                textBox1.ForeColor = SystemColors.WindowText;
                textBox1.Font = Font;
                textBox1.Text = string.Empty;
                textBox1.Tag = new object();
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void rajceLogo_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://rajce.idnes.cz/");
        }
    }
}
