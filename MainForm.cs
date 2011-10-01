using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

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
        public void BeginOperation(int Min, int Max, string StatusText)
        {
            SetProgressBarBounds(Min, Max);
            SetStatusText(StatusText);
            ShowProgressBar(true);
        }

        public void EndOperation()
        {
            ShowProgressBar(false);
            SetStatusText(null);
        }

        public void SetProgressBarBounds(int Min, int Max)
        {
            progessBar.Minimum = Min;
            progessBar.Maximum = Max;
        }

        public void SetProgressBarPos(int Pos)
        {
            progessBar.Value = Pos;
        }

        public void SetStatusText(string StatusText)
        {
            if (StatusText != null)
                statusLabel.Text = StatusText;
            else
                statusLabel.Text = Properties.Resources.Status_Ready;
        }

        public void ShowProgressBar(bool Show)
        {
            progessBar.Visible = Show;
        }

        public void StepProgressBar(int Delta)
        {
            progessBar.Value += Delta;
        }
        #endregion
    }
}
