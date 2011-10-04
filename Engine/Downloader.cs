using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Windows.Forms;
using System.ComponentModel;

namespace LH.Apps.RajceDownloader.Engine
{
    /// <summary>
    /// Represents a photo to be downloaded.
    /// </summary>
    public class Photo
    {
        /// <summary>
        /// The URL of the original file.
        /// </summary>
        public readonly string URL;
        /// <summary>
        /// The file to be the photo saved into.
        /// </summary>
        public readonly string Target;

        /// <summary>
        /// Initializes a new instance of Photo.
        /// </summary>
        /// <param name="aURL">The URL of the original file.</param>
        /// <param name="aTarget">The file to be the photo saved into.</param>
        public Photo(string aURL, string aTarget)
        {
            URL = aURL;
            Target = aTarget;
        }

        /// <summary>
        /// Returns a string representation of this object.
        /// </summary>
        /// <returns>A string representation of this object.</returns>
        public override string ToString()
        {
            return Path.GetFileName(URL);
        }
    }

    /// <summary>
    /// Provides a batch photo downloader with error handling.
    /// </summary>
    public class Downloader
    {
        /// <summary>
        /// Object used to pass async state between async calls.
        /// </summary>
        private class AsyncState : IDisposable
        {
            const int BufferSize = 1024;

            /// <summary>
            /// The original Photo object.
            /// </summary>
            public Photo Photo;
            /// <summary>
            /// The original web request.
            /// </summary>
            public WebRequest Request;
            /// <summary>
            /// The received web response.
            /// </summary>
            public WebResponse Response;
            /// <summary>
            /// The stream to be read from.
            /// </summary>
            public Stream ResponseStream;
            /// <summary>
            /// Transfer buffer.
            /// </summary>
            public byte[] Buffer;
            /// <summary>
            /// The stream to be written to.
            /// </summary>
            public Stream FileStream;

            /// <summary>
            /// Initializes a new instance of AsyncState.
            /// </summary>
            public AsyncState()
            {
                Buffer = new byte[BufferSize];
            }

            public void Dispose()
            {
                if (ResponseStream != null)
                {
                    ResponseStream.Close();
                    ResponseStream = null;
                }
                if (FileStream != null)
                {
                    FileStream.Close();
                    FileStream = null;
                }
            }
        }

        /// <summary>
        /// Determines state of a Downloader object.
        /// </summary>
        public enum DownloaderState
        {
            /// <summary>
            /// The Downloader is idle and ready for commands.
            /// </summary>
            Idle,
            /// <summary>
            /// The Downloader has started its work, access to the list is disabled.
            /// </summary>
            Started,
            /// <summary>
            /// The Downloader is preparing a request to be sent.
            /// </summary>
            PreparingRequest,
            /// <summary>
            /// The Downloader has sent the request and waits for a response.
            /// </summary>
            RequestSent,
            /// <summary>
            /// The Downloader is downloading a file.
            /// </summary>
            Downloading
        }

        private bool abort;
        private AsyncState asyncState;
        private int currentPhoto;
        private List<Photo> photos;
        private DownloaderState state;

        /// <summary>
        /// Initializes a new instance of Downloader.
        /// </summary>
        public Downloader()
        {
            currentPhoto = -1;
            photos = new List<Photo>();
            state = DownloaderState.Idle;
        }

        #region Public properties and events
        /// <summary>
        /// Current state of this Downloader instance.
        /// </summary>
        public DownloaderState State
        {
            get { return state; }
        }

        /// <summary>
        /// Fired when all the files have been downloaded.
        /// </summary>
        public event EventHandler Finished;
        #endregion

        #region Event invokers
        /// <summary>
        /// Invokes the Finished event.
        /// </summary>
        protected virtual void OnFinished()
        {
            if (Finished != null)
                Finished(this, new EventArgs());
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Inserts a range of Photo objects into the internal queue.
        /// </summary>
        /// <param name="newPhotos"></param>
        public void AddPhotos(IEnumerable<Photo> newPhotos)
        {
            if (state != DownloaderState.Idle)
                throw new InvalidOperationException("Downloader is busy.");
            photos.AddRange(newPhotos);
        }

        /// <summary>
        /// Clears the internal queue.
        /// </summary>
        public void ClearPhotos()
        {
            if (state != DownloaderState.Idle)
                throw new InvalidOperationException("Downloader is busy.");
            photos.Clear();
        }

        /// <summary>
        /// Starts the downloading.
        /// </summary>
        public void BeginDownload()
        {
            if (state != DownloaderState.Idle)
                return;
            abort = false;

            if (photos.Count == 0)
            {
                //nothing to be done here and no work has started yet
                //as we haven't even started yet, there is no need to call Done() here
                //just invoke Finished event, it should get called always
                MethodInvoker async = () => OnFinished();
                async.BeginInvoke(null, null);
            }
            else
                try
                {
                    state = DownloaderState.Started;
                    Program.StatusSink.BeginOperation(0, photos.Count, string.Empty);
                    if (!abort)
                    {
                        MethodInvoker async = () => BeginDownloadNextPhoto();
                        async.BeginInvoke(null, null);
                    }
                    else
                    {
                        MethodInvoker async = () => Done();
                        async.BeginInvoke(null, null);
                    }
                }
                catch (Exception ex)
                {
                    state = DownloaderState.Idle;
                    currentPhoto = -1;
                    Program.PromptSink.Error(ex.Message);
                }
        }

        /// <summary>
        /// Aborts the pending operation.
        /// </summary>
        public void Abort()
        {
            abort = true;
            switch (state)
            {
                case DownloaderState.RequestSent:
                    if (asyncState != null)
                        if (asyncState.Request != null)
                            asyncState.Request.Abort();
                    break;

                case DownloaderState.Downloading:
                    if (asyncState != null)
                        if (asyncState.Response != null)
                            asyncState.Response.Close();
                    break;
            }
        }
        #endregion

        /// <summary>
        /// Method to be called when all the requested photos have been downloaded.
        /// </summary>
        /// <remarks>This method directly calls OnFinished event invoker, so it should be 
        /// always BeginInvoke'd.</remarks>
        private void Done()
        {
            try
            {
                DisposeAsyncState(); //it should be already disposed
                Program.StatusSink.EndOperation();
                state = DownloaderState.Idle;
            }
            finally
            {
                OnFinished(); //this should get called _ALWAYS_...
            }
        }

        /// <summary>
        /// Disposes the current async state and sets the reference to null.
        /// </summary>
        private void DisposeAsyncState()
        {
            if (asyncState != null)
            {
                asyncState.Dispose();
                asyncState = null;
            }
        }

        /// <summary>
        /// Starts downloading of the next photo int the queue.
        /// </summary>
        /// <remarks>This method should be treated as an extension to BeginDownloadPhoto()
        /// that just automatically fills in the photoIndex parameter and thus, it should be
        /// always called asynchronously.</remarks>
        private void BeginDownloadNextPhoto()
        {
            if (photos.Count == 0)
                return;

            if (currentPhoto == photos.Count - 1)
            {
                //this was the last photo, we're done
                currentPhoto = -1;
                MethodInvoker async = () => Done();
                async.BeginInvoke(null, null);
            }
            else if (currentPhoto >= -1 && currentPhoto <= photos.Count - 1)
            {
                //-1 is valid here as the photos must contain at least one item
                state = DownloaderState.PreparingRequest;
                currentPhoto++;
                Program.StatusSink.SetStatusText(string.Format(
                    Properties.Resources.Status_DownloadingFile,
                    Path.GetFileName(photos[currentPhoto].URL)
                    ));
                Program.StatusSink.SetProgressBarPos(currentPhoto);
                BeginDownloadPhoto(currentPhoto);
            }
            else
                throw new InvalidOperationException("Invalid currentPhoto value.");
        }

        /// <summary>
        /// Starts download of a single photo.
        /// </summary>
        /// <param name="photoIndex">Index of the photo to be downloaded.</param>
        /// <remarks>This method should be called always asynchronously, with the exception of 
        /// BeginDownloadNextPhoto().</remarks>
        /// <seealso cref="LH.Apps.RajceDownloader.Engine.Downloader"/>
        private void BeginDownloadPhoto(int photoIndex)
        {
            if (photoIndex < 0 || photoIndex > photos.Count - 1)
                return;

            if (asyncState != null)
                asyncState.Dispose();

            asyncState = new AsyncState();
            asyncState.Photo = photos[photoIndex];
            try
            {
                string fileName = Path.GetFullPath(asyncState.Photo.Target);
                if (File.Exists(fileName))
                {
                    //TODO: workaround for a directory with that name...
                    DialogResult dr = Program.PromptSink.Question(
                        string.Format(
                            Properties.Resources.Downloader_FileExists, 
                            Path.GetFileName(fileName)
                            ),
                        MessageBoxButtons.YesNoCancel
                        );
                    switch (dr)
                    {
                        case DialogResult.Yes:
                            //just continue
                            break;

                        case DialogResult.No:
                            fileName = Utils.GetUniqueFileName(fileName);
                            break;

                        case DialogResult.Cancel:
                            //cancel this round
                            asyncState.Dispose();
                            asyncState = null;

                            MethodInvoker async = () => BeginDownloadNextPhoto();
                            async.BeginInvoke(null, null);
                            return;
                    }
                }

                asyncState.FileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
                asyncState.Request = WebRequest.Create(asyncState.Photo.URL);
                if (!abort)
                {
                    asyncState.Request.BeginGetResponse(new AsyncCallback(GetResponseCallback), null);
                    state = DownloaderState.RequestSent;
                }
                else
                    HandleAsyncAbort();
            }
            catch (Exception ex)
            {
                HandleDownloadException(ex);
            }
        }

        /// <summary>
        /// Handles an abort condition encountered in the async phase. Cleans up async state and invokes Done().
        /// </summary>
        private void HandleAsyncAbort()
        {
            DisposeAsyncState();
            MethodInvoker async = () => Done();
            async.BeginInvoke(null, null);
        }

        /// <summary>
        /// Displays an Abort-Retry-Ignore error box and handles user's response.
        /// </summary>
        /// <param name="ex">The exception object to be the message got from.</param>
        /// <remarks>Though this method contains a marshalled call to MessageBox, it should not be
        /// BeginInvoked. Exception should be handled immediately and in the offending thread.</remarks>
        private void HandleDownloadException(Exception ex)
        {
            DisposeAsyncState();

            MethodInvoker async;
            switch (Program.PromptSink.Error(ex.Message, MessageBoxButtons.AbortRetryIgnore))
            {
                case DialogResult.Abort:
                    async = () => Done();
                    async.BeginInvoke(null, null);
                    break;

                case DialogResult.Retry:
                    async = () => BeginDownloadPhoto(currentPhoto);
                    async.BeginInvoke(null, null);
                    break;

                case DialogResult.Ignore:
                    async = () => BeginDownloadNextPhoto();
                    async.BeginInvoke(null, null);
                    break;
            }
        }

        /// <summary>
        /// Async callback for Request.BeginGetResponse().
        /// </summary>
        /// <param name="ar">Async result.</param>
        private void GetResponseCallback(IAsyncResult ar)
        {
            if (ar == null)
                return;
            if (asyncState == null)
                return;

            try
            {
                asyncState.Response = asyncState.Request.EndGetResponse(ar);
                if (asyncState.Response != null)
                {
                    asyncState.ResponseStream = asyncState.Response.GetResponseStream();
                    asyncState.ResponseStream.BeginRead(asyncState.Buffer, 0, asyncState.Buffer.Length,
                        new AsyncCallback(ResponseReadCallback), null);
                    state = DownloaderState.Downloading;
                }
            }
            catch (WebException ex)
            {
                if (abort)
                    HandleAsyncAbort();
                else
                    HandleDownloadException(ex);
            }
            catch (Exception ex)
            {
                HandleDownloadException(ex);
            }
        }

        /// <summary>
        /// Async callback for ResponseStream.BeginRead().
        /// </summary>
        /// <param name="ar">Async result.</param>
        private void ResponseReadCallback(IAsyncResult ar)
        {
            if (ar == null)
                return;
            if (asyncState == null)
                return;

            if (asyncState.ResponseStream != null)
                try
                {
                    int read = asyncState.ResponseStream.EndRead(ar);
                    if (read > 0)
                    {
                        if (asyncState.FileStream != null)
                            asyncState.FileStream.Write(asyncState.Buffer, 0, read);
                        asyncState.ResponseStream.BeginRead(asyncState.Buffer, 0, asyncState.Buffer.Length,
                            new AsyncCallback(ResponseReadCallback), null);
                    }
                    else
                        try
                        {
                            asyncState.Dispose();
                            asyncState = null;
                        }
                        finally
                        {
                            MethodInvoker async = () => BeginDownloadNextPhoto();
                            async.BeginInvoke(null, null);
                        }
                }
                catch (WebException ex)
                {
                    if (abort)
                        HandleAsyncAbort();
                    else
                        HandleDownloadException(ex);
                }
                catch (Exception ex)
                {
                    HandleDownloadException(ex);
                }
        }
    }
}
