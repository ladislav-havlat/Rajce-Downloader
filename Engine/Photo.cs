using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LH.Apps.RajceDownloader.Engine
{
    public class Photo
    {
        #region Public properties
        /// <summary>
        /// Source URL to be the file downloaded from.
        /// </summary>
        public string SourceURL;
        /// <summary>
        /// Target path to be the file saved to.
        /// </summary>
        public string TargetPath;
        #endregion

        /// <summary>
        /// Initializes a new instance of Photo.
        /// </summary>
        public Photo()
        {
        }

        /// <summary>
        /// Initializes a new instance of Photo.
        /// </summary>
        /// <param name="aSourceURL">Source URL to be the file downloaded from.</param>
        public Photo(string aSourceURL)
        {
            SourceURL = aSourceURL;
        }

        /// <summary>
        /// Returns the string representation of this object.
        /// </summary>
        /// <returns>The string representation of this object.</returns>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(SourceURL))
                return Path.GetFileName(SourceURL);
            else
                return base.ToString();
        }
    }
}
