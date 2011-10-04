using System;
using System.Collections;
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
    public class PageParser : IEnumerable<Photo>
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
            /// <summary>
            /// String decoder for the downloaded data.
            /// </summary>
            public Decoder Decoder;
            /// <summary>
            /// Contains the received data.
            /// </summary>
            public StringBuilder Data;

            /// <summary>
            /// Initializes a new instance of DownloadState.
            /// </summary>
            public AsyncState()
            {
                Buffer = new byte[BufferSize];
                Data = new StringBuilder();
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

        /// <summary>
        /// Enumerator class for PageParser.
        /// </summary>
        private class PageParserEnumerator : IEnumerator<Photo>
        {
            private int currentIndex;
            private PageParser pageParser;

            /// <summary>
            /// Initializes a new instance of PageParserEnumerator.
            /// </summary>
            /// <param name="aPageParser"></param>
            public PageParserEnumerator(PageParser aPageParser)
            {
                currentIndex = 0;
                pageParser = aPageParser;
            }

            /// <summary>
            /// Gets the current object the enumerator points to.
            /// </summary>
            public Photo Current
            {
                get 
                {
                    if (pageParser.state == PageParserState.Idle)
                        return pageParser.photos[currentIndex];
                    else
                        throw new InvalidOperationException(Properties.Resources.Error_ParserIsBusy);
                }
            }

            /// <summary>
            /// Disposes the unmanaged resources used by the object.
            /// </summary>
            public void Dispose()
            {
            }

            /// <summary>
            /// Gets the current object the enumerator points to.
            /// </summary>
            object IEnumerator.Current
            {
                get 
                {
                    return Current;
                }
            }

            /// <summary>
            /// Moves to the next object in the list.
            /// </summary>
            /// <returns></returns>
            public bool MoveNext()
            {
                if (pageParser.State == PageParserState.Idle)
                {
                    if (currentIndex < pageParser.photos.Count - 1)
                    {
                        currentIndex++;
                        return true;
                    }
                    else
                        return false;
                }
                else
                    throw new InvalidOperationException(Properties.Resources.Error_ParserIsBusy);
            }

            /// <summary>
            /// Resets internal state of the enumerator to the first object.
            /// </summary>
            public void Reset()
            {
                currentIndex = 0;
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

        private AsyncState asyncState;
        private string pageURL;
        private List<Photo> photos;
        private PageParserState state;

        /// <summary>
        /// Initializes a new instance of PageParser.
        /// </summary>
        /// <param name="aPageURL">URL of the album page to be parsed.</param>
        public PageParser(string aPageURL)
        {
            state = PageParserState.Idle;
            pageURL = aPageURL;
            photos = new List<Photo>();
        }

        #region Public properties and events
        /// <summary>
        /// Fired when the parser has finished its work.
        /// </summary>
        public event EventHandler Finished;

        /// <summary>
        /// State of the downloader + parser.
        /// </summary>
        public PageParserState State
        {
            get { return state; }
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

        #region IEnumerable Members
        /// <summary>
        /// Returns the enumerator for this enumerable object.
        /// </summary>
        /// <returns>The enumerator for this enumerable object.</returns>
        public IEnumerator<Photo> GetEnumerator()
        {
            return new PageParserEnumerator(this);
        }

        /// <summary>
        /// Returns the enumerator for this enumerable object.
        /// </summary>
        /// <returns>The enumerator for this enumerable object.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new PageParserEnumerator(this);
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

        /// <summary>
        /// Disposes the async state object and sets the reference to null.
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
        /// Method to be called when the operation has ended, either successfully or unsuccessfully.
        /// </summary>
        /// <remarks>This method directly calls OnFinished event invoker, so it should be 
        /// always BeginInvoke'd.</remarks>
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
                    Program.StatusSink.BeginOperation(
                        0, 
                        length > 0 ? length : 0, 
                        Properties.Resources.Status_DownloadingPage
                        );

                    //try to get the encoding of the data or use UTF-8 by default
                    HttpWebResponse httpResponse = asyncState.Response as HttpWebResponse;
                    Encoding encoding = Encoding.UTF8;
                    if (httpResponse != null && !string.IsNullOrEmpty(httpResponse.ContentEncoding))
                        try
                        {
                            Encoding.GetEncoding(httpResponse.ContentEncoding);
                        }
                        catch (ArgumentException)
                        {
                            //use default
                        }
                    asyncState.Decoder = encoding.GetDecoder();

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

            try
            {
                if (asyncState.ResponseStream != null)
                {
                    int read = asyncState.ResponseStream.EndRead(ar);
                    if (read > 0)
                    {
                        int arraySize = asyncState.Decoder.GetCharCount(asyncState.Buffer, 0, read);
                        char[] chars = new char[arraySize];
                        int charCount = asyncState.Decoder.GetChars(asyncState.Buffer, 0, read, chars, 0, 
                            false);
                        asyncState.Data.Append(chars);
                        asyncState.ResponseStream.BeginRead(asyncState.Buffer, 0, asyncState.Buffer.Length,
                            new AsyncCallback(ReadPageCallback), null);
                    }
                    else
                    {
                        MethodInvoker async = new MethodInvoker(() => ParsePage());
                        async.BeginInvoke(null, null);
                    }
                }
            }
            catch (Exception ex)
            {
                HandleDownloadException(ex);
            }
        }

        /// <summary>
        /// Called when the page is ready for parsing in the memory buffer.
        /// </summary>
        private void ParsePage()
        {
            if (asyncState == null)
                return;
            if (asyncState.Data == null)
                return;

            try
            {
                Program.StatusSink.BeginOperation(0, 0, Properties.Resources.Status_ParsingPage);
                try
                {
                    string data = asyncState.Data.ToString();
                    asyncState.Data = null;

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
                                string sourceURL = string.Format(Properties.Resources.Parser_PhotoURL, storage, fileName);
                                photos.Add(new Photo(sourceURL));
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
