#region MIT.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
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
// Created: Monday, November 12, 2007 8:10:57 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Graphics.Tools
{
	/// <summary>
	/// Interface for nVidia texture atlas tool import.
	/// </summary>
	public class NVidiaAtlasImport
	{
		#region Value Types.
		/// <summary>
		/// Value type containing sprite data.
		/// </summary>
		public struct SpriteData
		{
			/// <summary>
			/// Name of the sprite.
			/// </summary>
			public string SpriteName;
			/// <summary>
			/// The sprite image.
			/// </summary>
			public Image Image;
			/// <summary>
			/// Sprite region.
			/// </summary>
			public RectangleF SpriteRect;
		}
		#endregion

		#region Variables.
		private List<string> _lines = null;							// List of lines in the TAI file.
		private SortedList<string, SpriteData> _sprites = null;		// List of sprites that are imported.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the list of sprites.
		/// </summary>
		public SortedList<string, SpriteData> Sprites
		{
			get
			{
				return _sprites;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to strip the source image names.
		/// </summary>
		/// <param name="line">Line containing data to parse.</param>
		/// <returns>Array containing line data.</returns>
		private string[] StripNames(string line)
		{
			string tempLine = line;						// Temporary worker line.

			if (line.LastIndexOf('\t') != -1)
			{
				tempLine = line.Substring(line.LastIndexOf('\t') + 1);
				if (tempLine == string.Empty)
					throw new InvalidDataException("Invalid format in TAI file.");

				return tempLine.Split(',');
			}

			throw new InvalidDataException("Invalid format in TAI file.");
		}

		/// <summary>
		/// Function to extract the sprite name.
		/// </summary>
		/// <param name="line">Line containing the data to parse.</param>
		/// <returns>Name of the sprite.</returns>
		private string GetName(string line)
		{
			string tempLine = line;						// Temporary worker line.

			if (line.IndexOf('\t') != -1)
			{
				tempLine = line.Substring(0, line.IndexOf('\t'));
				if (tempLine == string.Empty)
					throw new InvalidDataException("Invalid format in TAI file.");

				return Path.GetFileNameWithoutExtension(tempLine);
			}

			throw new InvalidDataException("Invalid format in TAI file.");
		}

		/// <summary>
		/// Function to get the sprite data from the atlas.
		/// </summary>
		/// <param name="path">Path to the sprite file.</param>
		/// <param name="line">Line containing data to parse.</param>
		private void GetSpriteData(string path, string line)
		{
			SpriteData sprite;				// Sprite.
			string[] items = null;			// List of item data.
			string imagePath = string.Empty;// Image path.

			sprite = new SpriteData();
			sprite.SpriteName = GetName(line);

			if (_sprites.ContainsKey(sprite.SpriteName))
				throw new InvalidDataException("The texture atlas item: '" + sprite.SpriteName + "' is already loaded.");

			items = StripNames(line);
			imagePath = path + @"\" + items[0];

			for (int i = 0; i < items.Length; i++)
				items[i] = items[i].Trim();

			if (string.Compare(items[2], "2D", true) == 0)
			{
				// Load the image if it doesn't already exist.
				if (!ImageCache.Images.Contains(Path.GetFileNameWithoutExtension(imagePath)))
					sprite.Image = Image.FromFile(imagePath);
				else
					sprite.Image = ImageCache.Images[Path.GetFileNameWithoutExtension(imagePath)];

				// Get sprite rectangle data.
				sprite.SpriteRect = new RectangleF(Convert.ToSingle(items[3]) * sprite.Image.Width, Convert.ToSingle(items[4]) * sprite.Image.Height,
								Convert.ToSingle(items[6]) * sprite.Image.Width, Convert.ToSingle(items[7]) * sprite.Image.Height);
			}

			_sprites.Add(sprite.SpriteName, sprite);
		}

		/// <summary>
		/// Function to import the texture atlas.
		/// </summary>
		/// <param name="atlasPath">Path to the atlas.</param>
		public void Import(string atlasPath)
		{
			string path;					// Path to the atlas.
			string oldPath = string.Empty;	// Old path.
			string taiFile;					// Texture indexing file.
			string[] fileLines;				// List of lines in the file.
			
			try
			{
				// Get the atlas path.
				path = Path.GetDirectoryName(atlasPath);

				// Go into the directory.
				oldPath = Directory.GetCurrentDirectory();
				Directory.SetCurrentDirectory(path);

				// Get the .tai file.
				taiFile = File.ReadAllText(atlasPath);

				// Split on character returns.
				_lines = new List<string>();
				fileLines = taiFile.Split('\n');

				// Remove carriage returns and linefeeds and any unnecessary cruft.
				for (int i = 0; i < fileLines.Length;i++)
				{
					fileLines[i] = fileLines[i].Replace("\n", string.Empty);
					fileLines[i] = fileLines[i].Replace("\r", string.Empty);

					// Drop comments.
					if ((!fileLines[i].StartsWith("#")) && (!string.IsNullOrEmpty(fileLines[i])))
						_lines.Add(fileLines[i]);						
				}

				// Parse each line.
				foreach (string line in _lines)
					GetSpriteData(path, line);
			}
			finally
			{
				Directory.SetCurrentDirectory(oldPath);
			}
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="NVidiaAtlasImport"/> class.
		/// </summary>
		public NVidiaAtlasImport()
		{
			_sprites = new SortedList<string, SpriteData>();
		}
		#endregion
	}
}
