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

        /// <summary>
        /// Displays an error box with specified buttons.
        /// </summary>
        /// <param name="message">Error message to be displayed.</param>
        /// <param name="caption">Caption to be used for the box.</param>
        /// <param name="buttons">Buttons to be displayed on the surface of the box.</param>
        /// <returns>Dialog result of the box.</returns>
        DialogResult Error(string message, string caption, MessageBoxButtons buttons);

        /// <summary>
        /// Displays an error box with specified buttons.
        /// </summary>
        /// <param name="message">Error message to be displayed.</param>
        /// <param name="buttons">Buttons to be displayed on the surface of the box.</param>
        /// <returns>Dialog result of the box.</returns>
        DialogResult Error(string message, MessageBoxButtons buttons);
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

        public DialogResult Error(string message, string caption, MessageBoxButtons buttons)
        {
            throw new NotImplementedException();
        }

        public DialogResult Error(string message, MessageBoxButtons buttons)
        {
            throw new NotImplementedException();
        }
    }
}
