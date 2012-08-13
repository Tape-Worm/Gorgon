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
// Created: Tuesday, June 12, 2012 12:34:51 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GorgonLibrary.PlugIns;

namespace GorgonLibrary.GorgonEditor.PlugIns
{
	/// <summary>
	/// Plug-in for a project file writer.
	/// </summary>
	public abstract class ProjectWriterPlugIn
		: GorgonPlugIn
	{
		#region Properties.
		/// <summary>
		/// Property to return the list of file extensions supported by this plug-in.
		/// </summary>
		public IEnumerable<string> FileExtensions
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the path for the project file.
		/// </summary>
		/// <param name="owner">Owner control for a dialog.</param>
		/// <returns>The path to the project file.</returns>
		public abstract string GetSavePath(System.Windows.Forms.IWin32Window owner);

		/// <summary>
		/// Function to save the project to a file.
		/// </summary>
		/// <param name="project">Project to save.</param>
		/// <param name="path">Path to the project file.</param>
		public abstract void SaveProject(Project project, string path);
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="ProjectWriterPlugIn"/> class.
		/// </summary>
		/// <param name="description">Optional description of the plug-in.</param>
		protected ProjectWriterPlugIn(string description, IList<string> fileExtensions)			
			: base(description)
		{
			if (fileExtensions != null)
				FileExtensions = new System.Collections.ObjectModel.ReadOnlyCollection<string>(fileExtensions);
			else
				FileExtensions = new string[] { };
		}
		#endregion
	}
}
