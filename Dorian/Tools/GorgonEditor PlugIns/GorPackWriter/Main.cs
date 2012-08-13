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
// Created: Tuesday, June 12, 2012 12:34:20 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Windows.Forms;

namespace GorgonLibrary.GorgonEditor.PlugIns
{
	public class GorPack
		: ProjectWriterPlugIn
	{
		#region Variables.

		#endregion

		#region Properties.

		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve a list of project folder paths.
		/// </summary>
		/// <param name="folders">Folders to enumerate.</param>
		/// <param name="paths">Paths to populate.</param>
		private void FolderPaths(ProjectFolderCollection folders, List<string> paths)
		{
			foreach (var folder in folders)
			{
				if (folder.Folders.Count > 0)
					FolderPaths(folder.Folders, paths);
				paths.Add(folder.Path);
			}
		}

		/// <summary>
		/// Function to retrieve the path for the project file.
		/// </summary>
		/// <param name="owner">Owner control for a dialog.</param>
		/// <returns>
		/// The path to the project file.
		/// </returns>
		public override string GetSavePath(System.Windows.Forms.IWin32Window owner)
		{
			SaveFileDialog dialog = new SaveFileDialog();
			dialog.AddExtension = true;
			dialog.AutoUpgradeEnabled = true;
			dialog.CheckFileExists = true;
			dialog.CheckPathExists = true;
			dialog.CreatePrompt = false;
			dialog.DefaultExt = "gorPack";
			dialog.InitialDirectory = @".\";
			dialog.Filter = "Gorgon packed files (*.gorPack)|*.gorPack";
			dialog.SupportMultiDottedExtensions = true;
			dialog.ValidateNames = true;
			dialog.Title = "Save project file...";
			if (dialog.ShowDialog(owner) == DialogResult.OK)
				return dialog.FileName;
			else
				return null;
		}

		/// <summary>
		/// Function to save the project to a file.
		/// </summary>
		/// <param name="project">Project to save.</param>
		/// <param name="path">Path to the project file.</param>
		public override void SaveProject(Project project, string path)
		{
			List<string> folderPaths = new List<string>();
			XDocument fat = new XDocument();

			FolderPaths(project.Folders, folderPaths);

			folderPaths.Add("/");
			// Get folder paths.
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorPack"/> class.
		/// </summary>
		public GorPack()
			: base("Gorgon packed file writer.", new string[] { "*.gorPack" })
		{
		}
		#endregion
	}
}
