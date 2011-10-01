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

namespace LH.Apps.RajceDownloader
{
    public partial class MainForm : Form, IStatusSink
    {
        public MainForm()
        {
            InitializeComponent();
            SetStatusText(null);
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

        private void button1_Click(object sender, EventArgs e)
        {
            PageParser pp = new PageParser("http://magicontrol.rajce.idnes.cz/Vystavba_kanalizace_Vladislav_2/");
            pp.Finished += new EventHandler(pp_Finished);
            pp.BeginParse();
        }

        private void pp_Finished(object sender, EventArgs e)
        {
            PageParser pp = sender as PageParser;

            MethodInvoker sync = new MethodInvoker(delegate()
            {
                lock (pp.PhotosURLs)
                {
                    foreach (string url in pp.PhotosURLs)
                        listBox.Items.Add(url);
                }
            });
            if (InvokeRequired)
                Invoke(sync);
            else
                sync();
        }
    }
}
