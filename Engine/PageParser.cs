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
        private class DownloadState
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

            /// <summary>
            /// Initializes a new instance of DownloadState.
            /// </summary>
            public DownloadState()
            {
                Buffer = new byte[BufferSize];
            }
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

        private MemoryStream pageData;
        private Encoding pageDataEncoding;
        private string pageURL;
        private List<string> photos;

        /// <summary>
        /// Initializes a new instance of PageParser.
        /// </summary>
        /// <param name="aPageURL">URL of the album page to be parsed.</param>
        public PageParser(string aPageURL)
        {
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
        public void BeginParse()
        {
            lock (photos)
                photos.Clear();
            pageData = null;
            pageDataEncoding = null;

            DownloadState state = new DownloadState();
            state.Request = HttpWebRequest.Create(pageURL);
            state.Request.BeginGetResponse(GetResponseCallback, state);
            Program.StatusSink.SetStatusText(Properties.Resources.Status_DownloadingPage);
        }

        /// <summary>
        /// Async callback for BeginGetResponse.
        /// </summary>
        /// <param name="ar">Async parameter object.</param>
        public void GetResponseCallback(IAsyncResult ar)
        {
            DownloadState state = ar.AsyncState as DownloadState;
            if (state == null)
                return;

            try
            {
                state.Response = state.Request.EndGetResponse(ar);
                if (state.Response != null)
                {
                    state.ResponseStream = state.Response.GetResponseStream();

                    int length = (int)state.Response.ContentLength;
                    if (length > 0)
                    {
                        pageData = new MemoryStream(length);
                        Program.StatusSink.BeginOperation(0, length, Properties.Resources.Status_DownloadingPage);
                    }
                    else
                    {
                        pageData = new MemoryStream();
                        Program.StatusSink.BeginOperation(0, 0, Properties.Resources.Status_DownloadingPage);
                    }

                    state.ResponseStream.BeginRead(state.Buffer, 0, state.Buffer.Length,
                        new AsyncCallback(ReadPageCallback), state);
                }
                else
                    Program.StatusSink.EndOperation();
            }
            catch
            {
                Program.StatusSink.EndOperation();
            }
        }

        /// <summary>
        /// Async callback for ResponseStream.BeginRead.
        /// </summary>
        /// <param name="ar">Async parameter object.</param>
        private void ReadPageCallback(IAsyncResult ar)
        {
            DownloadState state = ar.AsyncState as DownloadState;
            if (state == null)
                return;

            bool finished = false;
            try
            {
                if (state.ResponseStream != null)
                {
                    int read = state.ResponseStream.EndRead(ar);
                    if (read > 0)
                    {
                        pageData.Write(state.Buffer, 0, read);
                        if (state.Response.ContentLength > 0)
                            Program.StatusSink.StepProgressBar(read);
                        state.ResponseStream.BeginRead(state.Buffer, 0, state.Buffer.Length,
                            new AsyncCallback(ReadPageCallback), state);
                    }
                    else
                        try
                        {
                            //try to get the encoding of the data or use UTF-8 by default
                            HttpWebResponse httpResponse = state.Response as HttpWebResponse;
                            if (httpResponse != null && !string.IsNullOrEmpty(httpResponse.ContentEncoding))
                                pageDataEncoding = Encoding.GetEncoding(httpResponse.ContentEncoding);
                            if (pageDataEncoding == null)
                                pageDataEncoding = Encoding.UTF8;

                            finished = true;
                        }
                        finally
                        {
                            state.Response.Close();
                            Program.StatusSink.EndOperation();
                        }
                }
            }
            catch
            {
                Program.StatusSink.EndOperation();
            }

            if (finished)
                new MethodInvoker(ParsePage).BeginInvoke(null, null);
        }

        private void ParseError(string message)
        {
            //TODO: create a better error reporting system
            MessageBox.Show(message, "Parse error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Called when the page is ready for parsing in the memory buffer.
        /// </summary>
        private void ParsePage()
        {
            if (pageData == null || pageDataEncoding == null)
                return;

            try
            {
                Program.StatusSink.BeginOperation(0, 0, Properties.Resources.Status_ParsingPage);
                try
                {
                    byte[] pageDataArray = pageData.ToArray();
                    pageData = null;
                    string data = pageDataEncoding.GetString(pageDataArray);

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
                            ParseError(Properties.Resources.Error_PhotosParseError);
                    } //if (storageMatch.Success)
                    else
                        ParseError(Properties.Resources.Error_StorageParseError);
                }
                catch (Exception ex)
                {
                    ParseError(string.Format(
                        Properties.Resources.Error_GenericParseError,
                        ex.Message
                        ));
                }
            }
            finally
            {
                Program.StatusSink.EndOperation();
            }

            OnFinished();
        }
    }
}