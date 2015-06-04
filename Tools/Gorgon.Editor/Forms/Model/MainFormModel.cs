#region MIT.
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
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
// Created: Tuesday, March 17, 2015 12:14:01 AM
// 
#endregion

using Gorgon.Diagnostics;
using Gorgon.Editor.Properties;

namespace Gorgon.Editor
{
	/// <summary>
	/// Model for the main form.
	/// </summary>
	class MainFormModel 
		: IMainFormModel
	{
		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="MainFormModel"/> class.
		/// </summary>
		/// <param name="logFile">The log file for the application.</param>
		/// <param name="settings">The settings for the application.</param>
		/// <param name="defaultFileSystem">The default file system object.</param>
		public MainFormModel(GorgonLogFile logFile, IEditorSettings settings, IEditorFileSystem defaultFileSystem)
		{
			LogFile = logFile;
			EditorSettings = settings;
			WindowText = Resources.GOREDIT_CAPTION;
			CurrentFile = defaultFileSystem;
		}
		#endregion

		#region IMainFormModel Implementation.
		/// <summary>
		/// Property to return the application log file.
		/// </summary>
		public GorgonLogFile LogFile
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the application settings.
		/// </summary>
		public IEditorSettings EditorSettings
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the base window text.
		/// </summary>
		public string WindowText
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the current file.
		/// </summary>
		public IEditorFileSystem CurrentFile
		{
			get;
			set;
		}
		#endregion
	}
}
