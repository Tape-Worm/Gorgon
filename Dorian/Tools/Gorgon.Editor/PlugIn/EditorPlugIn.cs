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
// Created: Saturday, March 2, 2013 12:47:25 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using GorgonLibrary.PlugIns;
using GorgonLibrary.Graphics;
using GorgonLibrary.Renderers;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// Type of plug-in.
	/// </summary>
	public enum PlugInType
	{
		/// <summary>
		/// Plug-in writes out content packed files in a specific format.
		/// </summary>
		/// <remarks>File readers are implemented using the GorgonFileSystem objects, and are not specific to the editor.</remarks>
		FileWriter = 0,
		/// <summary>
		/// Plug-in is used to create content.
		/// </summary>
		Content = 1
	}

	/// <summary>
	/// An interface for editor plug-ins.
	/// </summary>
	public abstract class EditorPlugIn
		: GorgonPlugIn
	{
		#region Properties.
		/// <summary>
		/// Property to return the type of plug-in.
		/// </summary>
		/// <remarks>Implementors must provide one of the PlugInType enumeration values.</remarks>
		public abstract PlugInType PlugInType
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to create a tool strip menu item.
		/// </summary>
		/// <param name="name">Name of the menu item.</param>
		/// <param name="text">Text to display on the menu item.</param>
		/// <param name="icon">Icon to show on the menu item.</param>
		/// <returns>A new tool strip menu item.</returns>
		protected ToolStripMenuItem CreateMenuItem(string name, string text, Image icon)
		{
			ToolStripMenuItem result = new ToolStripMenuItem(text, icon);
			result.Name = name;
			result.Tag = this;

			return result;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="EditorPlugIn"/> class.
		/// </summary>
		/// <param name="description">Optional description of the plug-in.</param>
		/// <remarks>
		/// Objects that implement this base class should pass in a hard coded description on the base constructor.
		/// </remarks>
		protected EditorPlugIn(string description)
			: base(description)
		{
		}
		#endregion
	}

    /// <summary>
    /// An interface for content plug-ins.
    /// </summary>
    public abstract class ContentPlugIn
        : EditorPlugIn, IDisposable
    {
        #region Variables.

        #endregion

        #region Properties.
		/// <summary>
		/// Property to return the file extensions (and descriptions) for this content type.
		/// </summary>
		/// <remarks>This dictionary contains the file extension including the leading period as the key (in lowercase), and a tuple containing the file extension, and a description of the file (for display).</remarks>
		public IDictionary<string, Tuple<string, string>> FileExtensions
		{
			get;
			private set;
		}

		/// <summary>
        /// Property to return the type of plug-in.
        /// </summary>
        /// <remarks>Implementors must provide one of the PlugInType enumeration values.</remarks>
        public override PlugInType PlugInType
        {
            get 
            {
                return Editor.PlugInType.Content;
            }
        }

		/// <summary>
		/// Property to return whether the plug-in supports importing of external data.
		/// </summary>
		public abstract bool SupportsImport
		{
			get;
		}

		/// <summary>
		/// Property to return whether the plug-in supports exporting of content data.
		/// </summary>
		public abstract bool SupportsExport
		{
			get;
		}
        #endregion

        #region Methods.
        /// <summary>
        /// Function to create a content object interface.
        /// </summary>
        /// <returns>A new content object interface.</returns>
        protected internal abstract ContentObject CreateContentObject();

		/// <summary>
		/// Function to return the icon for the content.
		/// </summary>
		/// <returns>The 16x16 image for the content.</returns>
		public virtual Image GetContentIcon()
		{
			return Properties.Resources.unknown_document_16x16;
		}
		
		/// <summary>
		/// Function to retrieve the create menu item for this content.
		/// </summary>
		/// <returns>The menu item for this </returns>
		public virtual ToolStripMenuItem GetCreateMenuItem()
		{
			return null;
		}
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="ContentPlugIn"/> class.
        /// </summary>
        /// <param name="description">Optional description of the plug-in.</param>
        /// <remarks>
        /// Objects that implement this base class should pass in a hard coded description on the base constructor.
        /// </remarks>
        protected ContentPlugIn(string description)
            : base(description)
        {
			FileExtensions = new Dictionary<string, Tuple<string, string>>();
        }
        #endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
