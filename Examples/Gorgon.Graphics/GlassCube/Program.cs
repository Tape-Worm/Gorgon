#region MIT
// 
// Gorgon.
// Copyright (C) 2017 Michael Winsor
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
// Created: March 4, 2017 12:47:54 PM
// 
#endregion

using System;
using System.IO;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Graphics.Example.Properties;
using Gorgon.IO;
using Gorgon.UI;

namespace GorgonLibrary.Example
{
	/// <summary>
	/// Glasss cube example program.
	/// </summary>
    static class Program
    {
		/// <summary>
		/// Property to return the path to the resources for the example.
		/// </summary>
		/// <param name="resourceItem">The directory or file to use as a resource.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="resourceItem"/> was NULL (<i>Nothing</i> in VB.Net) or empty.</exception>
		public static string GetResourcePath(string resourceItem)
		{
			string path = Settings.Default.ResourceLocation;

			if (string.IsNullOrEmpty(resourceItem))
			{
				throw new ArgumentException(@"The resource was not specified.", nameof(resourceItem));
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
				path += resourceItem.FormatPath(Path.DirectorySeparatorChar);
			}

			// Ensure that we have an absolute path.
			return Path.GetFullPath(path);
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
        static void Main()
        {
	        try
	        {
		        Application.EnableVisualStyles();
		        Application.SetCompatibleTextRenderingDefault(false);

				GorgonApplication.Run(new formMain());
	        }
	        catch (Exception ex)
	        {
				ex.Catch(_ => GorgonDialogs.ErrorBox(null, _), GorgonApplication.Log);
			}
		}
    }
}
