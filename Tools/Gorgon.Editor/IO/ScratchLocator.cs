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
// Created: Monday, March 2, 2015 12:10:38 AM
// 
#endregion

using System.IO;
using System.Windows.Forms;
using Gorgon.Editor.Properties;

namespace Gorgon.Editor
{
	/// <summary>
	/// A UI to allow a user to locate the scratch path.
	/// </summary>
	class ScratchLocator 
		: IScratchLocator
	{
		#region Variables.
		// The scratch file area that we're updating.
		private readonly IScratchArea _scratchArea;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to change the location of the scratch area.
		/// </summary>
		/// <returns>A value from the <see cref="ScratchAccessibility"/> enumeration to indicate whether we can use the area or not.</returns>
		public ScratchAccessibility ChangeLocation()
		{
			using (var folderDialog = new FolderBrowserDialog
			                          {
				                          Description = Resources.GOREDIT_DLG_SCRATCH_PATH,
										  ShowNewFolderButton = true
			                          })
			{
				if (Directory.Exists(_scratchArea.ScratchDirectory))
				{
					folderDialog.SelectedPath = _scratchArea.ScratchDirectory;
				}

				return folderDialog.ShowDialog() != DialogResult.OK ? ScratchAccessibility.Canceled : _scratchArea.SetScratchDirectory(folderDialog.SelectedPath);
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="ScratchLocator"/> class.
		/// </summary>
		/// <param name="scratchArea">The scratch area interface.</param>
		public ScratchLocator(IScratchArea scratchArea)
		{
			_scratchArea = scratchArea;
		}
		#endregion
	}
}
