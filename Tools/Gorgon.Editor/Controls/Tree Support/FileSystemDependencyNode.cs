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
// Created: Thursday, March 12, 2015 9:27:45 PM
// 
#endregion

using System.Drawing;

namespace Gorgon.Editor
{
	/// <summary>
	/// A treeview node for a file dependency.
	/// </summary>
	class FileSystemDependencyNode
		: FileSystemFileNode
	{
		#region Properties.
		/// <summary>
		/// Property to return the type of node.
		/// </summary>
		public override NodeType NodeType
		{
			get
			{
				return NodeType.Dependency;
			}
		}

		/// <summary>
		/// Gets or sets the foreground color of the tree node.
		/// </summary>
		/// <returns>The foreground <see cref="T:System.Drawing.Color" /> of the tree node.</returns>
		///   <PermissionSet>
		///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
		///   </PermissionSet>
		public override Color ForeColor
		{
			get
			{
				return !IsBroken ? base.ForeColor : Color.Red;
			}
			set
			{
				// Nothing.
			}
		}

		/// <summary>
		/// Property to return whether the link for this dependency is broken or not.
		/// </summary>
		public bool IsBroken
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/*/// <summary>
		/// Function to update the node as a broken file link.
		/// </summary>
		/// <param name="dependency">Path to the file.</param>
		public void UpdateBroken(Dependency dependency)
		{
			string fileName = Path.GetFileName(dependency.EditorFile.FilePath);

			Name = dependency.EditorFile.FilePath;
			PlugIn = null;
			CollapsedImage = ExpandedImage = APIResources.image_missing_16x16;

			if (string.IsNullOrWhiteSpace(fileName))
			{
				fileName = APIResources.GOREDIT_TEXT_UNKNOWN_FILE;
			}

			Text = string.Format("{0} ({1})", fileName, APIResources.GOREDIT_TEXT_BROKEN_LINK);
			
			IsBroken = true;
		}*/
		#endregion
	}
}
