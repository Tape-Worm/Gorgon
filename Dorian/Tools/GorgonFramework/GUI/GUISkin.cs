#region MIT.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: Monday, June 09, 2008 8:53:19 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Text;
using GorgonLibrary.Graphics;
using GorgonLibrary.FileSystems;

namespace GorgonLibrary.GUI
{
	/// <summary>
	/// Object representing a custom GUI skin.
	/// </summary>
	public class GUISkin
		: IDisposable
	{
		#region Variables.
		private List<Image> _skinImages = null;					// List of skin images.
		private GUIElementCollection _elements = null;			// Elements for the skin.
		private bool _disposed = false;							// Flag to indicate that the object was disposed.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the list of elements for the skin.
		/// </summary>
		public GUIElementCollection Elements
		{
			get
			{
				return _elements;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the skin sprites animations (if any).
		/// </summary>
		/// <param name="frameTime">Frame time delta.</param>
		internal void Update(float frameTime)
		{
			foreach (GUIElement element in _elements)
				element.Update(frameTime);
		}

		/// <summary>
		/// Function to load a skin from a Gorgon packed file.
		/// </summary>
		/// <param name="skinFileSystem">File system that contains the GUI skin information.</param>
		/// <returns>A new GUI skin to use.</returns>
		public static GUISkin FromFile(FileSystem skinFileSystem)
		{
			GUISkin newSkin = new GUISkin();			// GUI skin.
			GUIElement newElement = null;				// GUI element.
			FileSystemPath images = null;				// Image path.
			FileSystemPath elements = null;				// Elements.

			if (skinFileSystem == null)
				throw new ArgumentNullException("skinFileSystem");

			try
			{
				images = skinFileSystem.Paths.ChildPaths["Images"];
				elements = skinFileSystem.Paths.ChildPaths["Elements"];

				// Load all related skin images.
				foreach (FileSystemFile file in images.Files)
					Image.FromFileSystem(skinFileSystem, file.FullPath);

				// Load in cursors.
				if (elements.ChildPaths.Contains("Cursors"))
				{
					foreach (FileSystemFile file in elements.ChildPaths["Cursors"].Files)
					{
						Sprite cursorSprite = Sprite.FromFileSystem(skinFileSystem, file.FullPath);
						newElement = new GUIElement("Cursor." + file.Filename, cursorSprite);
						newSkin.Elements.Add(newElement);
					}
				}

				// Load in window elements.
				if (elements.ChildPaths.Contains("Window"))
				{
					foreach (FileSystemFile file in elements.ChildPaths["Window"].Files)
					{
						Sprite windowElement = Sprite.FromFileSystem(skinFileSystem, file.FullPath);
						newElement = new GUIElement("Window." + file.Filename, windowElement);
						newSkin.Elements.Add(newElement);
					}
				}

				if (elements.ChildPaths.Contains("Controls"))
				{
					foreach (FileSystemFile file in elements.ChildPaths["Controls"].Files)
					{
						Sprite windowElement = Sprite.FromFileSystem(skinFileSystem, file.FullPath);
						newElement = new GUIElement("Controls." + file.Filename, windowElement);
						newSkin.Elements.Add(newElement);
					}
				}

				return newSkin;
			}
			catch
			{
				if (newSkin != null)
					newSkin.Dispose();
				throw;
			}			
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GUISkin"/> class.
		/// </summary>
		public GUISkin()
		{
			_skinImages = new List<Image>();
			_elements = new GUIElementCollection(this);
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					foreach (Image skinImage in _skinImages)
						skinImage.Dispose();

					_skinImages.Clear();
				}
				_disposed = true;
			}
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
