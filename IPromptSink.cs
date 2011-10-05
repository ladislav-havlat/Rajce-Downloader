/*
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

        /// <summary>
        /// Displays a question box with specified buttons.
        /// </summary>
        /// <param name="message">Message to be displayed.</param>
        /// <param name="buttons">Buttons to be displayed.</param>
        /// <returns>Dialog result of the box.</returns>
        DialogResult Question(string message, MessageBoxButtons buttons);
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
            return DialogResult.OK;
        }

        public DialogResult Error(string message, MessageBoxButtons buttons)
        {
            return DialogResult.OK;
        }

        public DialogResult Question(string message, MessageBoxButtons buttons)
        {
            return DialogResult.None;
        }
    }
}
