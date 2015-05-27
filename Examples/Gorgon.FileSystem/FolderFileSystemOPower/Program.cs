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
// Created: Saturday, January 19, 2013 7:32:45 PM
// 
#endregion

using System;
using System.IO;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Examples.Properties;
using Gorgon.IO;
using Gorgon.UI;

namespace Gorgon.Examples
{
	/// <summary>
	/// Main application.
	/// </summary>
	static class Program
	{
		#region Methods.
		/// <summary>
		/// Property to return the path to the resources for the example.
		/// </summary>
		/// <param name="resourceItem">The directory or file to use as a resource.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="resourceItem"/> was NULL (Nothing in VB.Net) or empty.</exception>
		public static string GetResourcePath(string resourceItem)
		{
			string path = Settings.Default.ResourceLocation;

			if (string.IsNullOrEmpty(resourceItem))
			{
                throw new ArgumentException("The resource was not specified.", "resourceItem");
			}

			if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()))
			{
				path += Path.DirectorySeparatorChar.ToString();
			}

			path = path.RemoveIllegalPathChars();

			// If this is a directory, then sanitize it as such.
			if (resourceItem.EndsWith(Path.DirectorySeparatorChar.ToString()))
			{
				path += resourceItem.RemoveIllegalPathChars();
			}
			else
			{
				// Otherwise, sanitize the file name.
				path += resourceItem.RemoveIllegalFilenameChars();
			}

			// Ensure that 
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

				GorgonApplication.Run(new MainForm());
			}
			catch (Exception ex)
			{
				GorgonException.Catch(ex, _ => GorgonDialogs.ErrorBox(null, _), true);
			}
		}
		#endregion
	}
}