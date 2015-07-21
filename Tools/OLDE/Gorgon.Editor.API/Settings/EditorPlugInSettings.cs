#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Wednesday, May 02, 2012 10:30:29 PM
// 
#endregion

using System;
using Gorgon.Configuration;
using Gorgon.IO;

namespace Gorgon.Editor
{
	/// <summary>
	/// Configuration settings for the plug-in interfaces.
	/// </summary>
	public abstract class EditorPlugInSettings
		: GorgonApplicationSettings
	{
		#region Properties.
		/// <summary>
		/// Property to return the path to the configuration file.
		/// </summary>
		public override string Path
		{
			get
			{
				return base.Path;
			}
			set
			{
			}
		}

		/// <summary>
		/// Property to return the path to the scratch area for building the data.
		/// </summary>
		public string ScratchPath => ScratchArea.ScratchPath;

		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="EditorPlugInSettings" /> class.
		/// </summary>
		/// <param name="contentType">Type of the content.</param>
		/// <param name="version">The version of the settings file.</param>
		protected EditorPlugInSettings(string contentType, Version version)
			: base("Gorgon.Editor." + contentType, version)
		{
			// Set the path for the application settings.
			base.Path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).FormatDirectory(System.IO.Path.DirectorySeparatorChar) 
					  + "Tape_Worm".FormatDirectory(System.IO.Path.DirectorySeparatorChar)
					  + "Gorgon.Editor".FormatDirectory(System.IO.Path.DirectorySeparatorChar)					  
#if !DEBUG
                      + this.ApplicationName.FormatFileName() + ".config.xml";		
#else
                      + ApplicationName.FormatFileName() + ".DEBUG.config.xml";		
#endif
		}
		#endregion
	}
}
