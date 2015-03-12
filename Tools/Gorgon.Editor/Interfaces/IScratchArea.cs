#region MIT.
//  
// Gorgon
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
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR 
// IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// Created: 03/01/2015 9:20 PM
// 
#endregion

using System;
using GorgonLibrary.IO;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// The interface used to control the scratch file area for the application.
	/// </summary>
	/// <remarks>
	/// This interface performs file manipulation for the application.  When the editor is run, a new scratch 
	/// area is created on the local disk drive to allow for manipulation of content.  The editor settings 
	/// will contain the path to the scratch area on the drive.
	/// <para>
	/// Files/folders added to the editor will be placed in this scratch area to allow for modification as 
	/// most (if not all) of the pack file provider types that Gorgon comes with are read-only.  
	/// </para>
	/// <para>
	/// Upon saving, the files are gathered up from the scratch area, and placed within the packed file being 
	/// saved by the editor (depending on the current writer plug-in).
	/// </para>
	/// <para>
	/// When a packed file is being loaded, its contents are copied into the scratch area to allow for modification. 
	/// Again, this is because most of the file system providers for Gorgon are read-only.
	/// </para>
	/// <para>
	/// For security, a scratch area cannot be created at the root of a drive, and must be created under sub 
	/// directory that is writable by the current user.  Also, the scratch area cannot be created under "system" 
	/// folders.  System folders are critical operating system folders such as System32, Program Files, etc...  
	/// This is necessary because the contents of the scratch area are very volatile and if something were to go 
	/// wrong, damage to those areas would cause major problems, so this will help mitigate potential issues.
	/// </para>
	/// </remarks>
	interface IScratchArea
	{
		#region Events.
		/// <summary>
		/// Event fired when a file or directory is updated in the scratch area.
		/// </summary>
		event EventHandler<ScratchUpdatedEventArgs> ScratchUpdated;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the directories that will hold the scratch files.
		/// </summary>
		string ScratchDirectory
		{
			get;
		}

		/// <summary>
		/// Property to return the GUID for the scratch area.
		/// </summary>
		Guid ID
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to determine if the scratch area is accessible.
		/// </summary>
		/// <param name="path">Path to the scratch area.</param>
		/// <returns>A value from <see cref="ScratchAccessibility"/> indicating the level of accessibility to the scratch area on the drive.</returns>
		ScratchAccessibility CanAccessScratch(string path);

		/// <summary>
		/// Function to clean up the scratch area data.
		/// </summary>
		/// <param name="cleanEverything">[Optional] TRUE to remove this scratch area and stale scratch area IDs from the folder, FALSE to clean up the current scratch area only.</param>
		/// <returns>TRUE if the call was successful and the data deleted.  FALSE if not.</returns>
		/// <remarks>This will delete the files and folders under the scratch area path, and will delete the scratch area folder (<see cref="ScratchDirectory"/>\<see cref="ID"/>).</remarks>
		bool CleanUp(bool cleanEverything = false);

		/// <summary>
		/// Function to set the path to the directory that will hold the scratch data.
		/// </summary>
		/// <param name="directoryPath">Path to the scratch directory.</param>
		/// <returns>A value from the <see cref="ScratchAccessibility"/> enumeration.</returns>
		/// <remarks>
		/// This method will create a directory under the path specified in <paramref name="directoryPath"/> that will contain 
		/// the files and folders for the editor content. The directory specified must be writable by the current user and cannot 
		/// be a system directory (e.g. C:\windows\system32\) and cannot be the root of the operating system drive.
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="directoryPath"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="directoryPath"/> parameter is empty.</exception>
		ScratchAccessibility SetScratchDirectory(string directoryPath);

		/// <summary>
		/// Function to copy a packed file system file to our scratch area.
		/// </summary>
		/// <param name="fileSystem">File system to copy.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="fileSystem"/> parameter is NULL (Nothing in VB.Net).</exception>
		void CopyFileSystem(GorgonFileSystem fileSystem);
		#endregion
	}
}