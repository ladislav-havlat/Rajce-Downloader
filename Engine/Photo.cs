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
