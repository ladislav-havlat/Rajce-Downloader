﻿using System;
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
                    BeginDownloadPhoto(nextPhoto);
                    Program.StatusSink.SetStatusText(string.Format(
                        Properties.Resources.Status_DownloadingFile,
                        Path.GetFileName(nextPhoto.URL)
                        ));
                    Program.StatusSink.SetProgressBarPos(currentPhoto);
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
        public void BeginDownloadPhoto(Photo photo)
        {
            if (photo == null)
                return;

            AsyncState state = new AsyncState();
            state.Photo = photo;
            try
            {
                state.FileStream = new FileStream(state.Photo.Target, FileMode.Create, FileAccess.Write, FileShare.None);
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
        private void HandleDownloadException(Exception ex)
        {
            MethodInvoker async;
            switch (Program.PromptSink.Error(ex.Message, MessageBoxButtons.AbortRetryIgnore))
            {
                case DialogResult.Abort:
                    EndDownload();
                    break;

                case DialogResult.Retry:
                    async = () => BeginDownloadPhoto(photos[currentPhoto]);
                    async.BeginInvoke(null, null);
                    break;

                case DialogResult.Ignore:
                    async = () => NextPhoto();
                    async.BeginInvoke(null, null);
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
                    state.Dispose();
                    state = null;
                    HandleDownloadException(ex);
                }
        }
    }
}
