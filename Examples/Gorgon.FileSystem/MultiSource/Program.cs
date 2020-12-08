#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: Thursday, January 17, 2013 11:06:54 PM
// 
#endregion

using System;
using System.IO;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Examples.Properties;
using Gorgon.IO;
using Gorgon.UI;

namespace Gorgon.Examples
{
    /// <summary>
    /// Example entry point.
    /// </summary>
    /// <remarks>To see a description of this example, look in formMain.cs</remarks>
    internal static class Program
    {
        #region Properties.
        /// <summary>
        /// Property to return the log used for debug log messages.
        /// </summary>
	    public static IGorgonLog Log
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the path to the plug ins.
        /// </summary>
        public static string PlugInPath
        {
            get
            {
                string path = Settings.Default.PlugInLocation;

                if (path.Contains("{0}"))
                {
#if DEBUG
                    path = string.Format(path, "Debug");
#else
					path = string.Format(path, "Release");					
#endif
                }

                if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()))
                {
                    path += Path.DirectorySeparatorChar.ToString();
                }

                return Path.GetFullPath(path);
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Property to return the path to the resources for the example.
        /// </summary>
        /// <param name="resourceItem">The directory or file to use as a resource.</param>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="resourceItem"/> parameter is <b>null</b> or empty.</exception>
        public static string GetResourcePath(string resourceItem)
        {
            string path = Settings.Default.ResourceLocation;

            if (string.IsNullOrEmpty(resourceItem))
            {
                throw new ArgumentException("The resource was not specified.", nameof(resourceItem));
            }

            path = path.FormatDirectory(Path.DirectorySeparatorChar);

            // If this is a directory, then sanitize it as such.
            if (resourceItem.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                path += resourceItem.FormatDirectory(Path.DirectorySeparatorChar);
            }
            else
            {
                // Otherwise, format the file name.
                path += resourceItem.FormatFileName();
            }

            // Ensure that we have an absolute path.
            return Path.GetFullPath(path);
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Log = new GorgonTextFileLog("MultiSource", "Tape_Worm");
            Log.LogStart();

            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new FormMain());
            }
            catch (Exception ex)
            {
                ex.Catch(_ => GorgonDialogs.ErrorBox(null, _), Log);
            }
            finally
            {
                Log.LogEnd();
            }
        }
        #endregion
    }
}
