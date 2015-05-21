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
// Created: Thursday, September 26, 2013 1:01:40 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Gorgon.Editor.Properties;
using Gorgon.IO;

namespace Gorgon.Editor
{
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
		public GorgonFileExtensionCollection FileExtensions
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
				return PlugInType.Content;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to create a content object interface.
		/// </summary>
		/// <param name="settings">The initial settings for the content.</param>
		/// <returns>A new content object interface.</returns>
		protected abstract ContentObject OnCreateContentObject(ContentSettings settings);

		/// <summary>
		/// Function to return the content settings object.
		/// </summary>
		/// <returns>The content settings.</returns>
		protected abstract ContentSettings OnGetContentSettings();

		/// <summary>
		/// Function to create a content object interface.
		/// </summary>
		/// <param name="settings">The initial settings for the content.</param>
		/// <returns>A new content object interface.</returns>
		internal ContentObject CreateContentObject(ContentSettings settings)
		{
			return OnCreateContentObject(settings ?? GetContentSettings());
		}

		/// <summary>
		/// Function to populate file editor attributes for imported content.
		/// </summary>
		/// <param name="stream">Stream to the file.</param>
		/// <param name="attributes">Attributes to populate.</param>
		public abstract void GetEditorFileAttributes(Stream stream, IDictionary<string, string> attributes);

		/// <summary>
		/// Function to retrieve the settings for the content.
		/// </summary>
		/// <returns>The settings for the content.</returns>
		public ContentSettings GetContentSettings()
		{
			var result = OnGetContentSettings();
			
			result.PlugIn = this;

			return result;
		}

		/// <summary>
		/// Function to create a tool strip menu item.
		/// </summary>
		/// <param name="name">Name of the menu item.</param>
		/// <param name="text">Text to display on the menu item.</param>
		/// <param name="icon">Icon to show on the menu item.</param>
		/// <returns>A new tool strip menu item.</returns>
		protected ToolStripMenuItem CreateMenuItem(string name, string text, Image icon)
		{
			var result = new ToolStripMenuItem(text, icon)
			             {
			                 Name = name,
			                 Tag = this
			             };

		    return result;
		}

		/// <summary>
		/// Function to find and return the registered image editor plug-in.
		/// </summary>
		/// <returns>The image editor plug-in, if found.  FALSE if not.</returns>
		public IImageEditorPlugIn GetRegisteredImageEditor()
		{
			ContentPlugIn plugIn;

			// Use the first image editor if we haven't selected one.
			if (string.IsNullOrWhiteSpace(PlugIns.DefaultImageEditorPlugIn))
			{
				// Find the first image editor plug-in.
				plugIn = PlugIns.ContentPlugIns.FirstOrDefault(item => item.Value is IImageEditorPlugIn).Value;

				if (plugIn == null)
				{
					return null;
				}
			}
			else
			{
				if (!PlugIns.ContentPlugIns.TryGetValue(PlugIns.DefaultImageEditorPlugIn, out plugIn))
				{
					return null;
				}
			}

			return plugIn as IImageEditorPlugIn;
		}

		/// <summary>
		/// Function to determine if an image editor plug-in is loaded.
		/// </summary>
		/// <returns>TRUE if loaded, FALSE if not.</returns>
		public bool HasImageEditor()
		{
			return GetRegisteredImageEditor() != null;
		}

		/// <summary>
		/// Function to determine if a specific plug-in has been loaded already.
		/// </summary>
		/// <param name="plugInName">Name of the plug-in to look up.</param>
		/// <returns>TRUE if the plug-in is loaded, FALSE if not.</returns>
		public bool HasPlugIn(string plugInName)
		{
			if (string.IsNullOrWhiteSpace(plugInName))
			{
				return false;
			}

			// Check the plug-in lists to see if the plug-in is loaded.
			return PlugIns.ContentPlugIns.Any(item => string.Equals(item.Value.Name, plugInName))
			       || PlugIns.ReaderPlugIns.Any(item => string.Equals(item.Value.Name, plugInName))
			       || PlugIns.WriterPlugIns.Any(item => string.Equals(item.Value.Name, plugInName));
		}

		/// <summary>
		/// Function to return the icon for the content.
		/// </summary>
		/// <returns>The 16x16 image for the content.</returns>
		public virtual Image GetContentIcon()
		{
			return APIResources.unknown_document_16x16;
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
			FileExtensions = new GorgonFileExtensionCollection();
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