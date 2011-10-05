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
using System.IO;

namespace LH.Apps.RajceDownloader
{
    /// <summary>
    /// Utility class.
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Places the system directory separator at the end of the specified path if it doesn't
        /// already end with one.
        /// </summary>
        /// <param name="path">The path to be processed.</param>
        /// <returns>Path with trailing system directory separator.</returns>
        public static string IncludeTrailingPathSeparator(string path)
        {
            return IncludeTrailingPathSeparator(path, Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Places s directory separator at the end of the specified path if it doesn't already
        /// end with one.
        /// </summary>
        /// <param name="path">The path to be processed.</param>
        /// <param name="separator">The directory separator to be used.</param>
        /// <returns>Path with trailing directory separator.</returns>
        public static string IncludeTrailingPathSeparator(string path, char separator)
        {
            if (!path.EndsWith(separator.ToString()))
                return path + separator;
            else
                return path;
        }

        /// <summary>
        /// Checks whether the file exists. If it does, it amends the name so that the new file
        /// can be placed into the directory. If it does not, it returns the desired name without
        /// any changes.
        /// </summary>
        /// <param name="desiredName">The desired name of the file.</param>
        /// <returns>A file name that is unique within the directory.</returns>
        public static string GetUniqueFileName(string desiredName)
        {
            //ensure the input path is absolute!!!
            desiredName = Path.GetFullPath(desiredName);

            string path = IncludeTrailingPathSeparator(Path.GetDirectoryName(desiredName));
            string nameWithoutExt = Path.GetFileNameWithoutExtension(desiredName);
            string pathAndName = path + nameWithoutExt;
            string ext = Path.GetExtension(desiredName);

            string amend = string.Empty;
            int amendInt = 0;
            Func<string> getNewName = () => string.Concat(pathAndName, amend, ext);
            while (File.Exists(getNewName()))
            {
                amendInt++;
                amend = string.Format("({0})", amendInt);
            }
            return getNewName();
        }
    }
}
