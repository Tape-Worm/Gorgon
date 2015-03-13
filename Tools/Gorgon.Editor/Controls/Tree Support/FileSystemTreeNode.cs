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
// Created: Thursday, March 12, 2015 9:28:24 PM
// 
#endregion

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace GorgonLibrary.Editor
{
	[Flags]
	enum NodeType
	{
		/// <summary>
		/// Unknown node type.
		/// </summary>
		Unknown = 0,
		/// <summary>
		/// Root node.
		/// </summary>
		Root = 1,
		/// <summary>
		/// Directory node.
		/// </summary>
		Directory = 2,
		/// <summary>
		/// File node.
		/// </summary>
		File = 4,
		/// <summary>
		/// Dependency node.
		/// </summary>
		Dependency = 8
	}

	/// <summary>
	/// A base tree node for the <see cref="FileSystemTreeView"/>.
	/// </summary>
	/// <remarks>
	/// All node types must inherit from this class, or they will not be shown in the editor.
	/// </remarks>
	abstract class FileSystemTreeNode
		: TreeNode
	{
		#region Properties.
		/// <summary>
		/// Property to set or return whether this node is active in a cut operation.
		/// </summary>
		public bool IsCut
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the node editing state.
		/// </summary>
		public NodeEditState EditState
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the type of node.
		/// </summary>
		public abstract NodeType NodeType
		{
			get;
		}

        /// <summary>
        /// Gets or sets the background color of the tree node.
        /// </summary>
        /// <returns>The background <see cref="T:System.Drawing.Color" /> of the tree node. The default is <see cref="F:System.Drawing.Color.Empty" />.</returns>
        ///   <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   </PermissionSet>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual new Color BackColor
        {
            get
            {
	            return Color.Empty;
            }
	        // ReSharper disable once ValueParameterNotUsed
	        set
            {
                // Do nothing.
            }
        }

		/// <summary>
		/// Gets or sets the foreground color of the tree node.
		/// </summary>
		/// <returns>The foreground <see cref="T:System.Drawing.Color" /> of the tree node.</returns>
		///   <PermissionSet>
		///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
		///   </PermissionSet>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual new Color ForeColor
		{
			get
			{
				return Color.Empty;
			}
			// ReSharper disable once ValueParameterNotUsed
			set
			{
				// Do nothing.
			}
		}

		/// <summary>
		/// Gets or sets the text displayed in the label of the tree node.
		/// </summary>
		/// <returns>The text displayed in the label of the tree node.</returns>
		///   <PermissionSet>
		///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
		///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
		///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
		///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
		///   </PermissionSet>
		public virtual new string Text
		{
			get
			{
				return base.Text;
			}
			set
			{
				base.Text = value;
			}
		}

		/// <summary>
		/// Property to return the collapsed image for the node.
		/// </summary>
		public Image CollapsedImage
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the expanded image for the node.
		/// </summary>
		public Image ExpandedImage
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the bounds of the tree node.
		/// </summary>
		/// <returns>The <see cref="T:System.Drawing.Rectangle" /> that represents the bounds of the tree node.</returns>
		///   <PermissionSet>
		///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
		///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
		///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
		///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
		///   </PermissionSet>
		public new Rectangle Bounds
		{
			get
			{
				return TreeView != null ? new Rectangle(0, base.Bounds.Top, TreeView.ClientSize.Width, base.Bounds.Height) : base.Bounds;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to determine if this node is an ancestor of the parent node.
		/// </summary>
		/// <param name="parentNode">Parent node to evaluate for ancestry.</param>
		/// <returns>TRUE if the child is ancestor of parent, FALSE if not.</returns>
		public bool IsAncestorOf(FileSystemTreeNode parentNode)
		{
			TreeNode node = Parent;

			// Walk up the chain.
			while (node != null)
			{
				if (node == parentNode)
				{
					return true;
				}

				node = node.Parent;
			}

			return false;
		}

		/// <summary>
		/// Function to begin the edit process.
		/// </summary>
		public new void BeginEdit()
		{
			((FileSystemTreeView)TreeView).ShowRenameBox(this);
		}

		/// <summary>
		/// Function to end the edit process.
		/// </summary>
		/// <param name="cancel">TRUE to cancel the edit, FALSE to commit it.</param>
		public new void EndEdit(bool cancel)
		{
			((FileSystemTreeView)TreeView).HideRenameBox(!cancel);
			EditState = NodeEditState.None;
		}

		/// <summary>
		/// Function to refresh this node.
		/// </summary>
		public void Redraw()
		{
			if (TreeView != null)
			{
				TreeView.Invalidate(new Rectangle(0, base.Bounds.Top, TreeView.ClientSize.Width, base.Bounds.Height), true);
			}			
		}
		#endregion
	}
}
