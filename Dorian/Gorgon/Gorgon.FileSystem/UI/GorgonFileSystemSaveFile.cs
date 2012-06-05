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
// Created: Thursday, May 31, 2012 7:27:14 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.FileSystem;

namespace GorgonLibrary.UI
{
	/// <summary>
	/// The save file dialog interface.
	/// </summary>
	public class GorgonFileSystemSaveFile
		: GorgonFileSystemDialog
	{
		#region Variables.
		private string _writePath = string.Empty;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return whether to prompt if an existing file is going to be overwritten.
		/// </summary>
		public bool PromptForOverwrite
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the writable path on the file system.
		/// </summary>
		public string WritePath
		{
			get
			{
				return _writePath;
			}
			set
			{
				if (string.IsNullOrEmpty(value))
					return;

				_writePath = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to initialize the window.
		/// </summary>
		protected override void InitWindow()
		{
			DialogWindow.WritePath = _writePath;
			DialogWindow.AllowMultiSelect = false;
			DialogWindow.CheckForExistingFile = PromptForOverwrite;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonFileSystemSaveFile"/> class.
		/// </summary>
		/// <param name="fileSystem">The file system.</param>
		/// <param name="writePath">Path on the file system that's writable.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="fileSystem"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the file system has had a directory or packed file mounted.
		/// <para>-or-</para>
		/// <para>Thrown when the file system does not have a <see cref="P:GorgonLibrary.FileSystem.GorgonFileSystem.WriteLocation">write location</see> specified or the <paramref name="writePath"/> is empty or NULL.</para>
		/// </exception>
		/// <remarks>The writePath must be a path accessible by the file system and -not- a physical file path:  e.g. c:\dir\ is NOT valid, /dir/ is.</remarks>
		public GorgonFileSystemSaveFile(GorgonFileSystem fileSystem, string writePath)
			: base(fileSystem, false)
		{
			if ((string.IsNullOrEmpty(fileSystem.WriteLocation)) || (string.IsNullOrEmpty(writePath)))
				throw new ArgumentException("Cannot save to a file system without a write location.", "fileSystem");

			WritePath = writePath;
			PromptForOverwrite = true;
			Text = "Save file...";
		}
		#endregion
	}
}
