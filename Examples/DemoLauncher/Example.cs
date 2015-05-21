#region MIT.
// 
// Gorgon.
// Copyright (C) 2014 Michael Winsor
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
// Created: Tuesday, September 23, 2014 1:50:15 AM
// 
#endregion

using System;
using System.Drawing;
using System.IO;
using System.Xml.Linq;
using Gorgon.Core;
using Gorgon.Examples.Properties;
using Gorgon.IO;

namespace Gorgon.Examples
{
	/// <summary>
	/// An example item.
	/// </summary>
	class Example
		: IDisposable, INamedObject
	{
		#region Variables.
		// Flag to indicate that the object was disposed.
		private bool _disposed;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the icon for the example.
		/// </summary>
		public Image Icon
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the index of the example.
		/// </summary>
		public int Index
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the text associated with the example.
		/// </summary>
		public string Text
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the path to the executable to run.
		/// </summary>
		public string ExePath
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return that owns the example.
		/// </summary>
		public string Category
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to read an icon image file.
		/// </summary>
		/// <param name="category">The category for the example.</param>
		/// <param name="icon">Path to the icon.</param>
		/// <returns>An icon image object.</returns>
		private static Image ReadIcon(string category, string icon)
		{
			if (string.IsNullOrEmpty(icon))
			{
				return Resources.Default_128x128;
			}

			int iconIndexValue;

			// If we have a numeric value, then use the resource section.
			if (int.TryParse(icon, out iconIndexValue))
			{
				string directory = (GorgonApplication.ApplicationDirectory + "Images" + Path.DirectorySeparatorChar + category).FormatDirectory(Path.DirectorySeparatorChar);
				string fileName = "E" + icon + "_128x128.png".FormatFileName();
				string filePath = directory + fileName;

				return !File.Exists(filePath) ? Resources.Default_128x128 : Image.FromFile(filePath);
			}

			icon = Path.GetFullPath(icon);

			return !File.Exists(icon) ? Resources.Default_128x128 : Image.FromFile(icon);
		}

		/// <summary>
		/// Function to read the values from the example node and place them in the object.
		/// </summary>
		/// <param name="exampleIndex">Current index of the example.</param>
		/// <param name="category">The category for the example.</param>
		/// <param name="exampleNode">XML node containing the example data.</param>
		/// <returns>A new example object.</returns>
		public static Example Read(int exampleIndex, string category, XElement exampleNode)
		{
			XAttribute iconNameAttr = exampleNode.Attribute("icon");
			XAttribute captionAttr = exampleNode.Attribute("caption");
			XAttribute exePathAttr = exampleNode.Attribute("path");

			var result = new Example
			             {
				             Index = exampleIndex,
							 Category = category
			             };

			if (captionAttr != null)
			{
				result.Name = captionAttr.Value;
			}

			if (exePathAttr != null)
			{
				// Ensure that we can launch this executable.
				if (!string.IsNullOrEmpty(exePathAttr.Value))
				{
					string exePath = Path.GetFullPath(Settings.Default.ExePath).FormatDirectory(Path.DirectorySeparatorChar);
					string exeDirectory = Path.GetDirectoryName(exePathAttr.Value);
					string exeFilename = Path.GetFileName(exePathAttr.Value);

					if ((!string.IsNullOrWhiteSpace(exeDirectory))
					    && (!string.IsNullOrWhiteSpace(exeFilename))
						&& (exeFilename.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)))
					{
						exePath = exePath + exeDirectory.FormatDirectory(Path.DirectorySeparatorChar) + exeFilename;

						if (File.Exists(exePath))
						{
							result.ExePath = exePath;
						}
					}
				}
			}

			result.Text = exampleNode.Value;

			if (iconNameAttr != null)
			{
				result.Icon = ReadIcon(category, iconNameAttr.Value);
			}

			return result;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Prevents a default instance of the <see cref="Example"/> class from being created.
		/// </summary>
		private Example()
		{
			Text = string.Empty;
			Icon = Resources.Default_128x128;
			ExePath = string.Empty;
			Index = -1;
			Name = string.Empty;
		}
		#endregion

		#region IDisposable
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			if (disposing)
			{
				if ((Icon != null) && (Icon != Resources.Default_128x128))
				{
					Icon.Dispose();
					Icon = null;
				}
			}

			_disposed = true;
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

		#region INamedObject Members
		/// <summary>
		/// Property to return the name of the example.
		/// </summary>
		public string Name
		{
			get;
			private set;
		}
		#endregion
	}
}
