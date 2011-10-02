using System;
using System.Collections.Generic;
using System.Linq;
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
