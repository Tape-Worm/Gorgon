﻿#region MIT.
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
// Created: Tuesday, March 12, 2013 9:22:29 PM
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
	/// A treeview node for a directory.
	/// </summary>
	class TreeNodeDirectory
		: EditorTreeNode
	{
		#region Variables.
        private bool _isUpdated = false;
		#endregion

		#region Properties.
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
                if (IsSelected)
                {
                    return DarkFormsRenderer.MenuHilightForeground;
                }

				if (Directory == null)
				{
					return DarkFormsRenderer.DisabledColor;
				}

				var nodeName = Name.ToLower();

				if (Program.ChangedItems.ContainsKey(nodeName))
				{
					if (Program.ChangedItems[nodeName])
					{
						return Color.LightGreen;
					}
					else
					{
						return Color.FromArgb(94, 126, 255);
					}
				}

                return Color.White;
            }
            set
            {
                
            }
        }

		/// <summary>
		/// Property to return the directory associated with this node.
		/// </summary>
		public virtual GorgonFileSystemDirectory Directory
		{
			get
			{
				if (Program.ScratchFiles == null)
				{
					return null;
				}

				return Program.ScratchFiles.GetDirectory(Name);
			}			
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the node with new directory information.
		/// </summary>
		/// <param name="newDir">New directory info.</param>
		public void UpdateNode(GorgonFileSystemDirectory newDir)
		{
			this.Name = newDir.FullPath;
			this.Text = newDir.Name;

			if ((Nodes.Count > 0) && (IsExpanded))
			{
				Collapse();
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="TreeNodeDirectory"/> class.
		/// </summary>
		/// <param name="directory">The directory to associate with this node.</param>
		public TreeNodeDirectory(GorgonFileSystemDirectory directory)
		{
			ForeColor = Color.White;
			this.Name = directory.FullPath;
			this.Text = directory.Name;
			ExpandedImage = Properties.Resources.folder_open_16x16;
			CollapsedImage = Properties.Resources.folder_16x16;
		}
		#endregion
	}

	/// <summary>
	/// A treeview node for the root of the file system.
	/// </summary>
	class RootNodeDirectory
		: TreeNodeDirectory
	{
		#region Variables.

		#endregion

		#region Properties.
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
                if (IsSelected)
                {
                    return DarkFormsRenderer.MenuHilightForeground;
                }

                if (Program.ChangedItems.ContainsKey("/"))
                {
                    return Color.FromArgb(94, 126, 255);
                }

                return Color.White;
            }
            set
            {                
            }
        }

		/// <summary>
		/// Property to return the directory associated with this node.
		/// </summary>
		public override GorgonFileSystemDirectory Directory
		{
			get
			{
				if (Program.ScratchFiles == null)
				{
					return null;
				}

				return Program.ScratchFiles.RootDirectory;
			}
		}

		/// <summary>
		/// Gets or sets the name of the tree node.
		/// </summary>
		/// <returns>A <see cref="T:System.String" /> that represents the name of the tree node.</returns>
		public override string Text
		{
			get
			{
				return Program.EditorFile;
			}
			set
			{
				// Nothing.
			}
		}
		#endregion

		#region Methods.

		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="RootNodeDirectory"/> class.
		/// </summary>
		public RootNodeDirectory()
			: base(Program.ScratchFiles.RootDirectory)
		{
			ExpandedImage = CollapsedImage = Properties.Resources.project_node_16x16;
		}
		#endregion
	}
}