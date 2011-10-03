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
                return;

            try
            {
                state = DownloaderState.Downloading;
                currentPhoto = 0;

                Photo photo = photos[currentPhoto];
                BeginDownloadPhoto(photo);
                Program.StatusSink.BeginOperation(
                    0, photos.Count,
                    string.Format(
                        Properties.Resources.Status_DownloadingFile, 
                        Path.GetFileName(photo.URL)
                        )
                    );
            }
            catch
            {
                EndDownload();
            }
        }

        /// <summary>
        /// Call this to put the downloader into idle state.
        /// </summary>
        private void EndDownload()
        {
            state = DownloaderState.Idle;
            currentPhoto = -1;
            Program.StatusSink.EndOperation();
        }

        /// <summary>
        /// Starts downloading of the next photo int the queue.
        /// </summary>
        private void NextPhoto()
        {
            if (photos.Count > 0 && currentPhoto > -1)
            {
                currentPhoto++;
                if (currentPhoto <= photos.Count - 1)
                {
                    Photo nextPhoto = photos[currentPhoto];

                    MethodInvoker async = new MethodInvoker(() => BeginDownloadPhoto(nextPhoto));
                    async.BeginInvoke(null, null);

                    Program.StatusSink.SetStatusText(string.Format(
                        Properties.Resources.Status_DownloadingFile,
                        Path.GetFileName(nextPhoto.URL)
                        ));
                    Program.StatusSink.SetProgressBarPos(currentPhoto);
                }
                else
                {
                    EndDownload();
                    MethodInvoker async = new MethodInvoker(() => OnFinished());
                    async.BeginInvoke(null, null);
                }
            }
        }

        /// <summary>
        /// Starts download of a single photo.
        /// </summary>
        /// <param name="photo">The Photo to be downloaded.</param>
        /// <remarks>As this method might block on a MessageBox, it is advisable to allways BeginInvoke it.</remarks>
        public void BeginDownloadPhoto(Photo photo)
        {
            if (photo == null)
                return;

            AsyncState state = new AsyncState();
            state.Photo = photo;
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
                            NextPhoto();
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
                    EndDownload();
                    async = () => OnFinished();
                    async.BeginInvoke(null, null);
                    break;

                case DialogResult.Retry:
                    async = () => BeginDownloadPhoto(photos[currentPhoto]);
                    async.BeginInvoke(null, null);
                    break;

                case DialogResult.Ignore:
                    NextPhoto();
                    break;
            }
        }

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
                        NextPhoto();
                    }
                }
                catch (Exception ex)
                {
                    if (state != null)
                    {
                        //state might has got already disposed
                        state.Dispose();
                        state = null;
                    }
                    HandleDownloadException(ex);
                }
        }
    }
}
