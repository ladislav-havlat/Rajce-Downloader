﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace LH.Apps.RajceDownloader.Engine
{
    public class PageParser
    {
        /// <summary>
        /// Passes download state between asynchronous calls.
        /// </summary>
        private class AsyncState : IDisposable
        {
            /// <summary>
            /// Size of the buffer to be used while downloading the album page.
            /// </summary>
            const int BufferSize = 1024;

            /// <summary>
            /// The request used.
            /// </summary>
            public WebRequest Request;
            /// <summary>
            /// The response returned.
            /// </summary>
            public WebResponse Response;
            /// <summary>
            /// The stream to be data read from.
            /// </summary>
            public Stream ResponseStream;
            /// <summary>
            /// Temporary buffer used for data transfer.
            /// </summary>
            public byte[] Buffer;

            public MemoryStream pageData;
            public Encoding pageDataEncoding;

            /// <summary>
            /// Initializes a new instance of DownloadState.
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
            }
        }

        /// <summary>
        /// Determines the state of a PageParser.
        /// </summary>
        public enum PageParserState
        {
            /// <summary>
            /// The PageParser is idle and ready for commands.
            /// </summary>
            Idle,
            /// <summary>
            /// The PageParser is preparing a request to be sent.
            /// </summary>
            Started,
            /// <summary>
            /// The PageParser has sent the request and is waiting for a response.
            /// </summary>
            RequestSent,
            /// <summary>
            /// The PageParser is downloading the page.
            /// </summary>
            Downloading,
            /// <summary>
            /// The PageParser is parsing the page.
            /// </summary>
            Parsing
        }

        #region Static fields
        private static Regex s_fileRegex;
        private static Regex s_photosRegex;
        private static Regex s_storageRegex;

        /// <summary>
        /// Initializes the static fields of PageParser.
        /// </summary>
        static PageParser()
        {
            s_fileRegex = new Regex(Properties.Resources.Parser_FileRegex,
                RegexOptions.IgnoreCase | RegexOptions.Compiled); 
            //file name regex must not be single line, the records are separated by \n

            s_photosRegex = new Regex(Properties.Resources.Parser_PhotosRegex,
                RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);

            s_storageRegex = new Regex(Properties.Resources.Parser_StorageRegex, 
                RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }
        #endregion

        private AsyncState asyncState;
        private string pageURL;
        private List<string> photos;
        private PageParserState state;

        /// <summary>
        /// Initializes a new instance of PageParser.
        /// </summary>
        /// <param name="aPageURL">URL of the album page to be parsed.</param>
        public PageParser(string aPageURL)
        {
            state = PageParserState.Idle;
            pageURL = aPageURL;
            photos = new List<string>();
        }

        #region Public properties and events
        /// <summary>
        /// Fired when the parser has finished its work.
        /// </summary>
        public event EventHandler Finished;

        /// <summary>
        /// Access to the URL list.
        /// </summary>
        public List<string> PhotosURLs
        {
            get { return photos; }
        }
        #endregion

        #region Event invokers
        /// <summary>
        /// Invokes the Finished event.
        /// </summary>
        protected void OnFinished()
        {
            if (Finished != null)
                Finished(this, new EventArgs());
        }
        #endregion

        /// <summary>
        /// Asynchronously starts the download and parsing process.
        /// </summary>
        public void BeginDownloadAndParse()
        {
            state = PageParserState.Started;   
            MethodInvoker async = () => BeginDownloadPage();
            async.BeginInvoke(null, null);
        }

        /// <summary>
        /// Prepares the request and sends it to the server.
        /// </summary>
        private void BeginDownloadPage()
        {
            lock (photos)
                photos.Clear();

            asyncState = new AsyncState();
            asyncState.Request = HttpWebRequest.Create(pageURL);
            state = PageParserState.RequestSent;
            Program.StatusSink.SetStatusText(Properties.Resources.Status_DownloadingPage);
            asyncState.Request.BeginGetResponse(GetResponseCallback, null);
        }

        private void DisposeAsyncState()
        {
            if (asyncState != null)
            {
                asyncState.Dispose();
                asyncState = null;
            }
        }

        private void Done()
        {
            try
            {
                DisposeAsyncState();
                Program.StatusSink.EndOperation();
                state = PageParserState.Idle;
            }
            finally
            {
                OnFinished();
            }
        }

        /// <summary>
        /// Displays an Retry-Cancel error box and handles user's response.
        /// </summary>
        /// <param name="ex">The exception object to be the message got from.</param>
        private void HandleDownloadException(Exception ex)
        {
            DisposeAsyncState();

            DialogResult dr = Program.PromptSink.Error(
                string.Format(
                    Properties.Resources.Error_DownloadPage,
                    ex.Message
                    ),
                MessageBoxButtons.RetryCancel
                );

            MethodInvoker async;
            switch (dr)
            {
                case DialogResult.Retry:
                    async = () => BeginDownloadPage();
                    async.BeginInvoke(null, null);
                    break;

                case DialogResult.Cancel:
                    async = () => Done();
                    async.BeginInvoke(null, null);
                    break;
            }
        }

        /// <summary>
        /// Async callback for BeginGetResponse.
        /// </summary>
        /// <param name="ar">Async parameter object.</param>
        public void GetResponseCallback(IAsyncResult ar)
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

                    int length = (int)asyncState.Response.ContentLength;
                    if (length > 0)
                    {
                        asyncState.pageData = new MemoryStream(length);
                        Program.StatusSink.BeginOperation(0, length, Properties.Resources.Status_DownloadingPage);
                    }
                    else
                    {
                        asyncState.pageData = new MemoryStream();
                        Program.StatusSink.BeginOperation(0, 0, Properties.Resources.Status_DownloadingPage);
                    }

                    asyncState.ResponseStream.BeginRead(asyncState.Buffer, 0, asyncState.Buffer.Length,
                        new AsyncCallback(ReadPageCallback), null);
                }
                else
                    Program.StatusSink.EndOperation();
            }
            catch (Exception ex)
            {
                HandleDownloadException(ex);
            }
        }

        /// <summary>
        /// Async callback for ResponseStream.BeginRead.
        /// </summary>
        /// <param name="ar">Async parameter object.</param>
        private void ReadPageCallback(IAsyncResult ar)
        {
            if (ar == null)
                return;
            if (asyncState == null)
                return;

            bool finished = false;
            try
            {
                if (asyncState.ResponseStream != null)
                {
                    int read = asyncState.ResponseStream.EndRead(ar);
                    if (read > 0)
                    {
                        asyncState.pageData.Write(asyncState.Buffer, 0, read);
                        if (asyncState.Response.ContentLength > 0)
                            Program.StatusSink.StepProgressBar(read);
                        asyncState.ResponseStream.BeginRead(asyncState.Buffer, 0, asyncState.Buffer.Length,
                            new AsyncCallback(ReadPageCallback), null);
                    }
                    else
                    {
                        //try to get the encoding of the data or use UTF-8 by default
                        HttpWebResponse httpResponse = asyncState.Response as HttpWebResponse;
                        if (httpResponse != null && !string.IsNullOrEmpty(httpResponse.ContentEncoding))
                            asyncState.pageDataEncoding = Encoding.GetEncoding(httpResponse.ContentEncoding);
                        if (asyncState.pageDataEncoding == null)
                            asyncState.pageDataEncoding = Encoding.UTF8;

                        finished = true;
                    }
                }
            }
            catch (Exception ex)
            {
                HandleDownloadException(ex);
            }

            if (finished)
            {
                MethodInvoker async = new MethodInvoker(() => ParsePage());
                async.BeginInvoke(null, null);
            }
        }

        /// <summary>
        /// Called when the page is ready for parsing in the memory buffer.
        /// </summary>
        private void ParsePage()
        {
            if (asyncState == null)
                return;
            if (asyncState.pageData == null || asyncState.pageDataEncoding == null)
                return;

            try
            {
                Program.StatusSink.BeginOperation(0, 0, Properties.Resources.Status_ParsingPage);
                try
                {
                    byte[] pageDataArray = asyncState.pageData.ToArray();
                    asyncState.pageData = null;
                    string data = asyncState.pageDataEncoding.GetString(pageDataArray);

                    Match storageMatch = s_storageRegex.Match(data);
                    if (storageMatch.Success)
                    {
                        string storage = storageMatch.Groups[1].Value;
                        if (!storage.EndsWith("/"))
                            storage += "/";

                        Match photosMatch = s_photosRegex.Match(data);
                        if (photosMatch.Success)
                        {
                            string photosValue = photosMatch.Groups[1].Value;
                            Match fileMatch = s_fileRegex.Match(photosValue);
                            while (fileMatch.Success)
                            {
                                string fileName = fileMatch.Groups[1].Value;
                                lock (photos)
                                    photos.Add(string.Format(Properties.Resources.Parser_PhotoURL, storage, fileName));
                                fileMatch = fileMatch.NextMatch();
                            }
                        } //if (photosMatch.Success)
                        else
                            Program.PromptSink.Error(Properties.Resources.Error_PhotosParseError);
                    } //if (storageMatch.Success)
                    else
                        Program.PromptSink.Error(Properties.Resources.Error_StorageParseError);
                }
                catch (Exception ex)
                {
                    Program.PromptSink.Error(string.Format(
                        Properties.Resources.Error_GenericParseError,
                        ex.Message
                        ));
                }
            }
            finally
            {
                Done();
            }
        }
    }
}
