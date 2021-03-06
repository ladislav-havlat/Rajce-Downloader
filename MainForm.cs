﻿/*
Copyright (C) 2011 by Ladislav Havlat

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using LH.Apps.RajceDownloader.Engine;

namespace LH.Apps.RajceDownloader
{
    /// <summary>
    /// The main form class.
    /// </summary>
    public partial class MainForm : Form, IStatusSink, IPromptSink
    {
        private IAbortible abortible;
        private Downloader downloader;
        private PageParser pageParser;
        private string targetDir;

        /// <summary>
        /// Initializes a new instance of MainForm.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
            SetStatusText(null);
            UpdateUIEnabledState();

            Application.Idle += new EventHandler(Application_Idle);
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

        /// <summary>
        /// Application.Idle event handler. Updates the UI state.
        /// </summary>
        private void Application_Idle(object sender, EventArgs e)
        {
            UpdateUIEnabledState();
        }

        /// <summary>
        /// Updates the Enabled properties of the controls to reflect the current state of the application.
        /// </summary>
        /// <remarks>Can be called from any thread, contains internal synchronization.</remarks>
        private void UpdateUIEnabledState()
        {
            MethodInvoker sync = delegate()
            {
                bool busy = (abortible != null);
                startDownloadButton.Enabled = !busy;
                pageURLTextBox.Enabled = !busy;
                targetDirTextBox.Enabled = !busy;
                selectTargetDirButton.Enabled = !busy;
                abortButton.Enabled = busy;
            };

            if (InvokeRequired)
                Invoke(sync);
            else
                sync();
        }

        /// <summary>
        /// Verifies the input parameters entered by the user.
        /// </summary>
        /// <returns>True if the download can be started, false otherwise.</returns>
        private bool VerifyInputs()
        {
            if (pageURLTextBox.Tag != null)
            {
                targetDir = targetDirTextBox.Text;
                if (string.IsNullOrEmpty(targetDir))
                {
                    MessageBox.Show(
                        Properties.Resources.MainForm_NoTargetDirSpecified,
                        Properties.Resources.Caption_Generic,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                        );
                    return false;
                }

                if (!Directory.Exists(targetDir))
                {
                    DialogResult dr = MessageBox.Show(
                        Properties.Resources.MainForm_TargetDirDoesntExist,
                        Properties.Resources.Caption_Generic,
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question
                        );
                    switch (dr)
                    {
                        case DialogResult.Yes:
                            try
                            {
                                Directory.CreateDirectory(targetDir);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(
                                    string.Format(
                                        Properties.Resources.Error_CannotCreateDir,
                                        ex.Message
                                        ),
                                    Properties.Resources.Caption_Generic,
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error
                                    );
                                return false;
                            }
                            break;

                        case DialogResult.No:
                            return false;
                    }
                }

                return true;
            }
            else
            {
                MessageBox.Show(
                    Properties.Resources.Error_NoURLSpecified,
                    Properties.Resources.Caption_Generic,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                    );
                return false;
            }
        }

        /// <summary>
        /// Aborts the operation.
        /// </summary>
        private void abortButton_Click(object sender, EventArgs e)
        {
            if (abortible != null)
                abortible.Abort();
        }

        /// <summary>
        /// Verifies the input parameters and starts the download.
        /// </summary>
        private void startDownloadButton_Click(object sender, EventArgs e)
        {
            if (VerifyInputs())
            {
                pageParser = new PageParser(pageURLTextBox.Text);
                pageParser.Finished += new EventHandler(pageParser_Finished);
                pageParser.BeginDownloadAndParse();
                abortible = pageParser;
                UpdateUIEnabledState();
            }
        }

        /// <summary>
        /// Displays targetDirDialog to let the user choose the target dialog.
        /// </summary>
        private void selectTargetDirButton_Click(object sender, EventArgs e)
        {
            if (targetDirDialog.ShowDialog() == DialogResult.OK)
                targetDirTextBox.Text = targetDirDialog.SelectedPath;
        }

        /// <summary>
        /// Clears pageURLTextBox and sets its font to normal on first enter.
        /// </summary>
        private void pageURLTextBox_Enter(object sender, EventArgs e)
        {
            if (pageURLTextBox.Tag == null)
            {
                pageURLTextBox.ForeColor = SystemColors.WindowText;
                pageURLTextBox.Font = Font;
                pageURLTextBox.Text = string.Empty;
                pageURLTextBox.Tag = new object();
            }
        }

        /// <summary>
        /// Opens the website on click on the logo.
        /// </summary>
        private void rajceLogo_Click(object sender, EventArgs e)
        {
            Process.Start("http://rajce.idnes.cz/");
        }

        /// <summary>
        /// Called when the photos has been downloaded.
        /// </summary>
        private void downloader_Finished(object sender, EventArgs e)
        {
            abortible = null;
            UpdateUIEnabledState();
        }

        /// <summary>
        /// Called when the album page has been downloaded and parsed.
        /// </summary>
        private void pageParser_Finished(object sender, EventArgs e)
        {
            abortible = null;
            if (pageParser == null)
                return;

            Photo[] photos = pageParser.ToArray();
            foreach (Photo p in photos)
                p.TargetPath = Path.Combine(targetDir, Path.GetFileName(p.SourceURL));
            if (photos.Length > 0)
            {
                downloader = new Downloader();
                abortible = downloader;
                downloader.AddPhotos(photos);
                downloader.Finished += new EventHandler(downloader_Finished);
                downloader.BeginDownload();
            }
            UpdateUIEnabledState();
        }
    }
}
