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
                    ResponseStream.Dispose();
                    ResponseStream = null;
                }
                if (FileStream != null)
                {
                    FileStream.Dispose();
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
            /// The Downloader is currently downloading a file.
            /// </summary>
            Downloading
        }

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

        /// <summary>
        /// Inserts a range of Photo objects into the internal queue.
        /// </summary>
        /// <param name="newPhotos"></param>
        public void AddPhotos(IEnumerable<Photo> newPhotos)
        {
            if (state == DownloaderState.Downloading)
                throw new InvalidOperationException("Downloader is busy.");
            photos.AddRange(newPhotos);
        }

        /// <summary>
        /// Clears the internal queue.
        /// </summary>
        public void ClearPhotos()
        {
            if (state == DownloaderState.Downloading)
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

            if (photos.Count == 0)
                OnFinished(); //nothing to be done here...
            else
                try
                {
                    state = DownloaderState.Downloading;
                    MethodInvoker async = () => BeginDownloadNextPhoto();
                    async.BeginInvoke(null, null);
                    Program.StatusSink.BeginOperation(0, photos.Count, string.Empty);
                }
                catch (Exception ex)
                {
                    state = DownloaderState.Idle;
                    currentPhoto = -1;
                    Program.PromptSink.Error(ex.Message);
                }
        }

        /// <summary>
        /// Method to be called when all the requested photos have been downloaded.
        /// </summary>
        /// <remarks>This method directly calls OnFinished event invoker, so it should be 
        /// always BeginInvoke'd.</remarks>
        private void Done()
        {
            try
            {
                state = DownloaderState.Idle;
                Program.StatusSink.EndOperation();
            }
            finally
            {
                OnFinished(); //this should get called _ALWAYS_...
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
        public void BeginDownloadPhoto(int photoIndex)
        {
            if (photoIndex < 0 || photoIndex > photos.Count - 1)
                return;

            AsyncState state = new AsyncState();
            state.Photo = photos[photoIndex];
            try
            {
                string fileName = Path.GetFullPath(state.Photo.Target);
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
                            state.Dispose();
                            state = null;

                            MethodInvoker async = () => BeginDownloadNextPhoto();
                            async.BeginInvoke(null, null);
                            return;
                    }
                }

                state.FileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
                state.Request = WebRequest.Create(state.Photo.URL);
                state.Request.BeginGetResponse(new AsyncCallback(GetResponseCallback), state);
            }
            catch (Exception ex)
            {
                state.Dispose();
                state = null;
                HandleDownloadException(ex);
            }
        }

        /// <summary>
        /// Displays an Abort-Retry-Ignore error box and handles user's response.
        /// </summary>
        /// <param name="ex">The exception object to be the message got from.</param>
        /// <remarks>Though this method contains a marshalled call to MessageBox, it should not be
        /// BeginInvoked. Exception should be handled immediately and in the offending thread.</remarks>
        private void HandleDownloadException(Exception ex)
        {
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
            AsyncState state = ar.AsyncState as AsyncState;
            if (state == null)
                return;

            try
            {
                state.Response = state.Request.EndGetResponse(ar);
                if (state.Response != null)
                {
                    state.ResponseStream = state.Response.GetResponseStream();
                    state.ResponseStream.BeginRead(state.Buffer, 0, state.Buffer.Length,
                        new AsyncCallback(ResponseReadCallback), state);
                }
            }
            catch (Exception ex)
            {
                state.Dispose();
                state = null;
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
            AsyncState state = ar.AsyncState as AsyncState;
            if (state == null)
                return;

            if (state.ResponseStream != null)
                try
                {
                    int read = state.ResponseStream.EndRead(ar);
                    if (read > 0)
                    {
                        if (state.FileStream != null)
                            state.FileStream.Write(state.Buffer, 0, read);
                        state.ResponseStream.BeginRead(state.Buffer, 0, state.Buffer.Length,
                            new AsyncCallback(ResponseReadCallback), state);
                    }
                    else
                    {
                        state.Dispose();
                        state = null;
                        MethodInvoker async = () => BeginDownloadNextPhoto();
                        async.BeginInvoke(null, null);
                    }
                }
                catch (Exception ex)
                {
                    if (state != null)
                    {
                        //state might have got already disposed
                        state.Dispose();
                        state = null;
                    }
                    HandleDownloadException(ex);
                }
        }
    }
}
