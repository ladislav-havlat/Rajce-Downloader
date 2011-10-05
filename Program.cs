/*
Copyright (C) 2011 by Ladislav Havlat

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using System;
using System.Windows.Forms;

namespace LH.Apps.RajceDownloader
{
    static class Program
    {
        private static IPromptSink s_promptSink;
        private static IStatusSink s_statusSink;

        /// <summary>
        /// Initializes the static fields of this class.
        /// </summary>
        static Program()
        {
            s_promptSink = new DummyPromptSink();
            s_statusSink = new DummyStatusSink();
        }

        /// <summary>
        /// The prompt sink object.
        /// </summary>
        public static IPromptSink PromptSink
        {
            get { return s_promptSink; }
        }

        /// <summary>
        /// The status sink object.
        /// </summary>
        public static IStatusSink StatusSink
        {
            get { return s_statusSink; }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            MainForm mf = new MainForm();
            s_promptSink = mf as IPromptSink;
            s_statusSink = mf as IStatusSink;
            Application.Run(mf);
        }
    }
}
