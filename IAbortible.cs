
namespace LH.Apps.RajceDownloader
{
    /// <summary>
    /// Represents an object performing some kind of action that can be aborted.
    /// </summary>
    interface IAbortible
    {
        /// <summary>
        /// Aborts the operation.
        /// </summary>
        void Abort();
    }
}
