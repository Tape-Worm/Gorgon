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
// Created: Monday, March 9, 2015 1:19:20 AM
// 
#endregion

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// The file system service used to load/save packed files.
	/// </summary>
	/// <remarks>
	/// This interface is meant to create, load and save a packed file containing content files. These packed files are copied to and from the 
	/// scratch area. This means that the file is only ever modified on save, and never during content creation/editing.
	/// <para>
	/// When opening a packed file, the system will locate the appropriate file system plug-in provider to allow reading of that file.  The 
	/// location is performed by first checking the extension of the file name against a list of known provider extensions. If the extension is 
	/// found, then the provider will then mount the file system and copy it into the scratch area. 
	/// </para>
	/// <para>
	/// If the extension could not be matched to a known provider, then the system will fall back to testing each known provider with the file. 
	/// Obviously, this is a more expensive operation.
	/// </para>
	/// <para>
	/// When saving the file, the system will attempt to locate the appropriate writer plug-in to allow it to persist the file system back to 
	/// the physical disk. The user will select this plug-in by providing an appropriate extension to the file name.  When this file is saved 
	/// a meta data file within the file system will record the name of the plug-in used to save the file. This is to facilitate a "Save" 
	/// operation (note: not a "Save as..." operation).  By using this plug-in name, the system can just persist the file back in the proper 
	/// format.
	/// </para>
	/// <para>
	/// If the writer plug-in does not exist, then the system will notify the user and will prompt them to provide a new file name/extension 
	/// registered by a known writer plug-in.
	/// </para>
	/// </remarks>
	interface IFileSystemService
	{
		/// <summary>
		/// Property to return a string of file types supported for reading.
		/// </summary>
		/// <remarks>This property will return a string formatted for the open file dialog extension property.</remarks>
		string ReadFileTypes
		{
			get;
		}

		/// <summary>
		/// Function to load a file from the physical file system.
		/// </summary>
		/// <param name="path">Path to the file to load.</param>
		void LoadFile(string path);

		/// <summary>
		/// Function to build the file system provider list.
		/// </summary>
		void GetFileSystemProviders();
	}
}