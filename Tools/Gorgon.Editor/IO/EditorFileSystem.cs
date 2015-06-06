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
// Created: Wednesday, March 11, 2015 9:34:44 PM
// 
#endregion

using System.IO;
using Gorgon.Editor.Properties;

namespace Gorgon.Editor
{
	/// <summary>
	/// A representation of a packed editor file in the application.
	/// </summary>
	class EditorFileSystem
		: IEditorFileSystem
	{
		#region Variables.
		// Path to the file.
		private string _filePath;
		// The scratch file location.
		private IScratchArea _scratchArea;
		#endregion

		#region Methods.
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="EditorFileSystem"/> struct.
		/// </summary>
		/// <param name="filePath">The path to the file.</param>
		/// <param name="scratchArea">The location used for the scratch files.</param>
		/// <exception cref="System.IO.IOException">Thrown when the <paramref name="filePath"/> does not contain a file name.</exception>
		/// <remarks>Pass NULL (<i>Nothing</i> in VB.Net) or an empty string to the <paramref name="filePath"/> parameter to create an empty file.</remarks>
		public EditorFileSystem(string filePath, IScratchArea scratchArea)
		{
			_scratchArea = scratchArea;
			FullName = filePath;
		}
		#endregion

		#region IGorgonNamedObject Members
		/// <summary>
		/// Property to return the filename for the file.
		/// </summary>
		public string Name
		{
			get;
			private set;
		}
		#endregion

		#region IEditorFileSystem Implementation.
		#region Properties.
		/// <summary>
		/// Property to return whether the file has been changed or not.
		/// </summary>
		public bool HasChanged
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the full path to the file.
		/// </summary>
		public string FullName
		{
			get
			{
				return _filePath;
			}
			set
			{
				if (string.IsNullOrWhiteSpace(value))
				{
					_filePath = string.Empty;
					Name = Resources.GOREDIT_TEXT_UNTITLED;
					return;
				}

				string fileName = Path.GetFileName(Path.GetFullPath(value));

				if (string.IsNullOrWhiteSpace(fileName))
				{
					throw new IOException(string.Format(Resources.GOREDIT_ERR_FILENAME_NOT_FOUND_IN_PATH, value));
				}

				_filePath = value;
				Name = fileName;
			}
		}
		#endregion

		#region Methods.
		#endregion
		#endregion
	}
}
