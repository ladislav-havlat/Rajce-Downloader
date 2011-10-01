using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LH.Apps.RajceDownloader
{
    /// <summary>
    /// Represents an object capable of displaying the status of the program.
    /// </summary>
    public interface IStatusSink
    {
        /// <summary>
        /// Displays the progress bar, sets its bounds, its position to the beginning and sets the status bar
        /// text. Combines SetProgressBarBounds, SetProgressBarPos and SetStatusText into one operation.
        /// </summary>
        /// <param name="Min">Minimal progress bar position.</param>
        /// <param name="Max">Maximal progress bar position.</param>
        /// <param name="StatusText">The text to be displayed. If null, "Ready" status shall be displayed.</param>
        void BeginOperation(int Min, int Max, string StatusText);

        /// <summary>
        /// Hides the progress bar and sets the status bar's text to "Ready".
        /// </summary>
        void EndOperation();

        /// <summary>
        /// Sets the progress bar's bounds.
        /// </summary>
        /// <param name="Min">Minimal progress bar position.</param>
        /// <param name="Max">Maximal progress bar position.</param>
        void SetProgressBarBounds(int Min, int Max);

        /// <summary>
        /// Sets the progress bar's position.
        /// </summary>
        /// <param name="Pos">Desired position of the progress bar.</param>
        void SetProgressBarPos(int Pos);
        
        /// <summary>
        /// Sets the text on a generic status bar.
        /// </summary>
        /// <param name="StatusText">The text to be displayed. If null, "Ready" status shall be displayed.</param>
        void SetStatusText(string StatusText);

        /// <summary>
        /// Shows or hides the progress bar.
        /// </summary>
        /// <param name="Show">True if the progress bar is to be shown, false otherwise.</param>
        void ShowProgressBar(bool Show);

        /// <summary>
        /// Increases the progress bar's position by Delta.
        /// </summary>
        /// <param name="Delta">Amount of progress to be increased by.</param>
        void StepProgressBar(int Delta);
    }

    /// <summary>
    /// Dummy status sink object. Does nothing, just implements the IStatusSink interface.
    /// </summary>
    public class DummyStatusSink : IStatusSink
    {
        public void BeginOperation(int Min, int Max, string StatusText)
        {
        }

        public void EndOperation()
        {
        }

        public void SetProgressBarBounds(int Min, int Max)
        {
        }

        public void SetProgressBarPos(int Pos)
        {
        }

        public void SetStatusText(string StatusText)
        {
        }

        public void ShowProgressBar(bool Show)
        {
        }

        public void StepProgressBar(int Delta)
        {
        }
    }
}
