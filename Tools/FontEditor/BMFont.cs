#region LGPL.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 
// Created: Sunday, August 13, 2006 2:31:22 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using Drawing = System.Drawing;
using SharpUtilities;
using SharpUtilities.Native.Win32;
using GorgonLibrary;

namespace GorgonLibrary.Graphics.Fonts
{
	/// <summary>
	/// Object representing an Angelcode BM font.
	/// </summary>
	class BMFont
		: Font
	{
		#region Methods.
		/// <summary>
		/// Function to read the font from the disk.
		/// </summary>
		/// <param name="fontImage">Image to use for the font.</param>
		/// <param name="imageBasePath">Path that holds the image.</param>
		private void ReadFont(Image fontImage, string imageBasePath)
		{
			string fontXMLName = string.Empty;			// XML path to the font.
			string fontPath = string.Empty;				// Path to the font.
			XmlTextReader xmlFont = null;				// XML font data.
			bool foundFont = false;						// Font header was found.

			try
			{
				// Get path to the font.
				fontPath = Path.GetDirectoryName(_fontPath) + @"\";
				_fontImage = null;

				fontXMLName = fontPath + Path.GetFileNameWithoutExtension(_fontPath) + ".fnt";

				// Create the XML reader.
				xmlFont = new XmlTextReader(fontXMLName);

				// Read the font data.
				while (xmlFont.Read())
				{
					switch (xmlFont.NodeType)
					{
						case XmlNodeType.Element:
							switch (xmlFont.Name.ToLower())
							{
								case "font":
									foundFont = true;
									break;
								case "common":
									if (!xmlFont.HasAttributes)
										throw new InvalidResourceException(Name, typeof(BMFont), null);

									// Get image size.
									_imageSize.X = Convert.ToSingle(xmlFont["scaleW"]);
									_imageSize.Y = Convert.ToSingle(xmlFont["scaleH"]);
									break;
								case "info":
									string[] padValues = null;				// Padding values.
									string[] spaceValues = null;			// Spacing values.

									if (!xmlFont.HasAttributes)
										throw new InvalidResourceException(Name, typeof(BMFont), null);

									// Get meta data.
									_sourceFont = xmlFont["face"];

									// BM font uses... pixels?  em units?  I have no idea, but this seems close enough.
									_sourceFontSize = Convert.ToSingle(xmlFont["size"]) * 0.5625f;

									_bold = Convert.ToBoolean(Convert.ToInt32(xmlFont["bold"]));
									_italics = Convert.ToBoolean(Convert.ToInt32(xmlFont["italic"]));
									_antiAliased = ((xmlFont["aa"] != "0") || (xmlFont["smooth"] != "0"));
									padValues = xmlFont["padding"].Split(',');
									spaceValues = xmlFont["spacing"].Split(',');
									_leftOffset = -Convert.ToInt32(padValues[0]);
									_topOffset = -Convert.ToInt32(padValues[1]);
									_rightOffset = Convert.ToInt32(padValues[2]) + Convert.ToInt32(spaceValues[0]);
									_bottomOffset = Convert.ToInt32(padValues[3]) + Convert.ToInt32(spaceValues[1]);
									break;
								case "char":
									FontCharacter character = default(FontCharacter);	// Font character.

									character.CharacterOrdinal = Convert.ToInt32(xmlFont["id"]);
									character.Character = Convert.ToChar(character.CharacterOrdinal);

									// Add to collection.
									_characters.Add(character.Character, character);
									break;
							}
							break;
					}
				}

				if ((!foundFont) || (!_characters.ContainsKey(' ')))
					throw new InvalidResourceException(Name, typeof(BMFont), null);

				// Update the coordinates for the characters.
				UpdateTextureCoordinates();
			}
			catch (SharpException sEx)
			{
				throw sEx;
			}
			catch (Exception ex)
			{
				throw new CannotLoadException(_fontPath, typeof(BMFont), ex);
			}
			finally
			{
				if (xmlFont != null)
					xmlFont.Close();

				xmlFont = null;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name">Name of the font.</param>
		/// <param name="fontPath">File path to the font.</param>
		internal BMFont(string name, string fontPath)
			: base(name)
		{
			_fontPath = fontPath;
			ReadFont(null, null);
		}
		#endregion
	}
}
