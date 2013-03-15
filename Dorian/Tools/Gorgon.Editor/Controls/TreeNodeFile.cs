#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Tuesday, March 12, 2013 9:44:49 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using GorgonLibrary.FileSystem;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// A treeview node for a file.
	/// </summary>
	class TreeNodeFile
		: EditorTreeNode
	{
		#region Variables.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the font for open files.
		/// </summary>
		public Font OpenFont
		{
			get;
			set;
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
                if ((IsSelected) && (PlugIn != null))
                {
					return DarkFormsRenderer.MenuHilightForeground;
                }

				// Draw the text as disabled.
				if (PlugIn == null)
				{
					return DarkFormsRenderer.DisabledColor;
				}

				if (Program.ChangedItems.ContainsKey(File.FullPath.ToLower()))
				{
					bool state = Program.ChangedItems[File.FullPath.ToLower()];

					return state ? Color.LightGreen : Color.FromArgb(94, 126, 255);
				}

				return Color.White;
			}
			set
			{
				// Nothing.
			}
		}

		/// <summary>
		/// Property to return the plug-in that is linked to the file attached to the node.
		/// </summary>
		public ContentPlugIn PlugIn
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the file associated with this node.
		/// </summary>
		public GorgonFileSystemFileEntry File
		{
			get
			{
				if (Program.ScratchFiles == null)
				{
					return null;
				}

				return Program.ScratchFiles.GetFile(Name);
			}			
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to get the file data.
		/// </summary>
		private void GetFileData()
		{
			ExpandedImage = Properties.Resources.unknown_document_16x16;
			CollapsedImage = ExpandedImage;
						
			if (!string.IsNullOrWhiteSpace(File.Extension))
			{
				if (PlugIn == null)
				{
					PlugIn = (from plugIn in Program.ContentPlugIns
							  where plugIn.Value.FileExtensions.ContainsKey(File.Extension.ToLower())
							  select plugIn.Value).FirstOrDefault();
				}

				if (PlugIn != null)
				{
					ExpandedImage = CollapsedImage = PlugIn.GetContentIcon();
				}
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="TreeNodeFile"/> class.
		/// </summary>
		/// <param name="file">File to associate with the node.</param>
		public TreeNodeFile(GorgonFileSystemFileEntry file)
		{
			this.Name = file.FullPath;
			this.Text = file.Name;
			GetFileData();
		}
		#endregion
	}
}
