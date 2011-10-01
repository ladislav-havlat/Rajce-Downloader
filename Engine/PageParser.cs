using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

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

        private MemoryStream pageData;
        private string pageURL;
        private List<string> photosURLs;

        /// <summary>
        /// Initializes a new instance of PageParser.
        /// </summary>
        /// <param name="aPageURL">URL of the album page to be parsed.</param>
        public PageParser(string aPageURL)
        {
            pageURL = aPageURL;
            photosURLs = new List<string>();
        }

        /// <summary>
        /// Asynchronously starts the download and parsing process.
        /// </summary>
        public void BeginParse()
        {
            photosURLs.Clear();
            pageData = null;

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
                        pageData = new MemoryStream();

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
                    {
                        state.Response.Close();
                        Program.StatusSink.EndOperation();

                        MethodInvoker pageReadyDelegate = new MethodInvoker(PageReady);
                        pageReadyDelegate.BeginInvoke(null, null);
                    }
                }
            }
            catch
            {
                Program.StatusSink.EndOperation();
            }
        }

        /// <summary>
        /// Called when the page is ready for parsing in the memory buffer.
        /// </summary>
        private void PageReady()
        {
            byte[] data = pageData.ToArray();
            using (FileStream fs = new FileStream("test", FileMode.Create, FileAccess.Write, FileShare.None))
            {
                fs.Write(data, 0, data.Length);
            }
        }
    }
}
