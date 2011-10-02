using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LH.Apps.RajceDownloader
{
    public interface IPromptSink
    {
        /// <summary>
        /// Displays an error box.
        /// </summary>
        /// <param name="message">Error message to be displayed.</param>
        /// <param name="caption">Caption to be used for the box.</param>
        void Error(string message, string caption);

        /// <summary>
        /// Displays an error box with generic error caption.
        /// </summary>
        /// <param name="message">Error message to be displayed.</param>
        void Error(string message);
    }

    /// <summary>
    /// Dummy prompt sink. Does nothing.
    /// </summary>
    public class DummyPromptSink : IPromptSink
    {
        public void Error(string message, string caption)
        {
        }

        public void Error(string message)
        {
        }
    }
}
