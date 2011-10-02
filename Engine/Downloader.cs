using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

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
    }

    public class Downloader
    {
        /// <summary>
        /// Object used to pass async state between async calls.
        /// </summary>
        private class AsyncState
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

            try
            {
                state = DownloaderState.Downloading;
                if (photos.Count > 0)
                {
                    currentPhoto = 0;
                    BeginDownloadPhoto(photos[currentPhoto]);
                    Program.StatusSink.BeginOperation(
                        0, photos.Count,
                        string.Format(Properties.Resources.Status_DownloadingFile, string.Empty)
                        );
                }
                else
                    EndDownload();
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
                    if (BeginDownloadPhoto(photos[currentPhoto]))
                    {
                        Program.StatusSink.SetStatusText(string.Format(
                            Properties.Resources.Status_DownloadingFile,
                            Path.GetFileName(photos[currentPhoto].URL)
                            ));
                        Program.StatusSink.SetProgressBarPos(currentPhoto);
                    }
                }
                else
                    EndDownload();
            }
            else
                EndDownload();
        }

        /// <summary>
        /// Starts download of a single photo.
        /// </summary>
        /// <param name="photo">The Photo to be downloaded.</param>
        /// <returns>True if the operation has been started successfully, false otherwise.</returns>
        public bool BeginDownloadPhoto(Photo photo)
        {
            if (photo == null)
                return false;

            Uri uri = new Uri(photo.URL);
            try
            {
                AsyncState state = new AsyncState();
                state.Photo = photo;

                try
                {
                    state.FileStream = new FileStream(photo.Target, FileMode.Create, FileAccess.Write, FileShare.None);
                }
                catch (Exception ex)
                {
                    Program.PromptSink.Error(string.Format(
                        Properties.Resources.Error_CreateFile,
                        ex.Message
                        ));
                    return false;
                }

                state.Request = WebRequest.Create(uri);
                state.Request.BeginGetResponse(new AsyncCallback(GetResponseCallback), state);
                return true;
            }
            catch (Exception ex)
            {
                Program.PromptSink.Error(ex.Message);
                return false;
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
                if (state.ResponseStream != null)
                {
                    state.ResponseStream.Close();
                    state.ResponseStream = null;
                }
                Program.PromptSink.Error(ex.Message);
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
                        if (state.FileStream != null)
                            state.FileStream.Close();
                        state.ResponseStream.Close();
                        state.ResponseStream = null;
                        NextPhoto();
                    }
                }
                catch (Exception ex)
                {
                    if (state.ResponseStream != null)
                    {
                        state.ResponseStream.Close();
                        state.ResponseStream = null;
                    }
                    Program.PromptSink.Error(ex.Message);
                }
        }
    }
}
