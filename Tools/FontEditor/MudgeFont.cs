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
// Created: Tuesday, July 25, 2006 11:34:22 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using Drawing = System.Drawing;
using System.Xml;
using System.IO;
using SharpUtilities;
using GorgonLibrary.Graphics.Tools;

namespace GorgonLibrary.Graphics.Fonts
{
	/// <summary>
	/// Object representing a mudge font.
	/// </summary>
	public class MudgeFont
		: Font
	{
		#region Methods.
		/// <summary>
		/// Function to read the Mudge font.
		/// </summary>
		private void ReadMudge()
		{
			FileStream stream = null;					// File stream.
			BinaryReader reader = null;					// Binary file reader.
			int charCount = 0;							// Character count.			

			try
			{
				// Open the file for reading.
				stream = File.OpenRead(_fontPath);
				reader = new BinaryReader(stream);

				// Read past the version bytes.
				reader.ReadInt32();
				reader.ReadInt32();

				// Read texture size.
				_imageSize.X = reader.ReadInt32();
				_imageSize.Y = reader.ReadInt32();

				// Get border.
				_leftOffset = reader.ReadInt32();
				_topOffset = reader.ReadInt32();
				_rightOffset = reader.ReadInt32();
				_bottomOffset = reader.ReadInt32();

				// Get font size.
				_sourceFontSize = (float)reader.ReadInt32();

				// Get weight. 				
				_bold = (reader.ReadInt32() > 400);
				// Anti aliasing settings.  
				_antiAliased = (reader.ReadInt32() != 4);
				// Skip the sampling setting.
				reader.ReadInt32();
				// Get italics.
				_italics = (reader.ReadInt32() != 0);

				// Get the name of the font.
				charCount = reader.ReadInt32();
				if (charCount == 0)
					throw new InvalidResourceException(Name, typeof(MudgeFont), null);

				_sourceFont = Encoding.UTF8.GetString(reader.ReadBytes(charCount + 1));
				_sourceFont = _sourceFont.Substring(0, _sourceFont.Length - 1);

				// Get character count.
				charCount = reader.ReadInt32();

				if ((charCount == 0) || (charCount >= 256))
					throw new InvalidResourceException(Name, typeof(MudgeFont), null);

				// Read character data.
				for (int i = 0; i < charCount; i++)
				{
					FontCharacter character = new FontCharacter();		// Character.

					// Get the character ordinal.
					character.CharacterOrdinal = (int)reader.ReadByte();
					character.Character = Convert.ToChar(character.CharacterOrdinal);

					// Skip the borders for now.
					reader.ReadInt32();
					reader.ReadInt32();
					reader.ReadInt32();
					reader.ReadInt32();

					// Add to collection.
					_characters.Add(character.Character, character);
				}

				// Update the coordinates for the characters.
				UpdateTextureCoordinates();
			}
			catch (SharpException sEx)
			{
				throw sEx;
			}
			catch (Exception ex)
			{
				throw new CannotLoadException(_fontPath, typeof(MudgeFont), ex);
			}
			finally
			{
				if (stream != null)
					stream.Close();
				if (reader != null)
					reader.Close();

				stream = null;
				reader = null;
			}
		}

		/// <summary>
		/// Function to read the XML font.
		/// </summary>
		private void ReadXML()
		{
			string fontXMLName = string.Empty;			// XML path to the font.
			string fontPath = string.Empty;				// Path to the font.
			XmlTextReader xmlFont = null;				// XML font data.

			try
			{
				// Get path to the font.
				fontPath = Path.GetDirectoryName(_fontPath) + @"\";
				_fontImage = null;

				fontXMLName = fontPath + Path.GetFileNameWithoutExtension(_fontPath) + ".xml";

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
								case "spacing":
									char ID;		// Character ID.

									if (!xmlFont.HasAttributes)
										throw new InvalidResourceException(Name, typeof(MudgeFont), null);

									// Get ID.
									ID = Convert.ToChar(Convert.ToInt32(xmlFont["id"]));

									// Get spacings.
									if (_characters.ContainsKey(ID))
									{
										FontCharacter character;		// Font character.

										character = _characters[ID];
										character.A = Convert.ToInt32(xmlFont["a"]);
										character.B = Convert.ToUInt32(xmlFont["b"]);
										character.C = Convert.ToInt32(xmlFont["c"]);

										// Copy back.
										_characters[ID] = character;
									}
									break;
								case "char":
									FontCharacter newCharacter = default(FontCharacter);	// New font character.
									string attribName = string.Empty;						// Attribute name.

									// Confirm that the font is valid.
									if (!xmlFont.HasAttributes)
										throw new InvalidResourceException(Name, typeof(MudgeFont), null);

									// Check for valid attributes.
									attribName = xmlFont["id"];
									if (attribName == null)
										throw new InvalidResourceException(Name, typeof(MudgeFont), null);
									attribName = xmlFont["x"];
									if (attribName == null)
										throw new InvalidResourceException(Name, typeof(MudgeFont), null);
									attribName = xmlFont["y"];
									if (attribName == null)
										throw new InvalidResourceException(Name, typeof(MudgeFont), null);
									attribName = xmlFont["width"];
									if (attribName == null)
										throw new InvalidResourceException(Name, typeof(MudgeFont), null);
									attribName = xmlFont["height"];
									if (attribName == null)
										throw new InvalidResourceException(Name, typeof(MudgeFont), null);

									newCharacter.CharacterOrdinal = Convert.ToInt32(xmlFont["id"]);
									newCharacter.Character = Convert.ToChar(newCharacter.CharacterOrdinal);
									newCharacter.Width = Convert.ToSingle(xmlFont["width"]);
									newCharacter.Height = Convert.ToSingle(xmlFont["height"]);
									newCharacter.Left = Convert.ToSingle(xmlFont["x"]);
									newCharacter.Top = Convert.ToSingle(xmlFont["y"]);
									if (_characters.ContainsKey(newCharacter.Character))
										throw new InvalidResourceException(Name, typeof(MudgeFont), null);
									_characters.Add(newCharacter.Character, newCharacter);
									break;
							}
							break;
					}
				}

				if (!_characters.ContainsKey(' '))
					throw new InvalidResourceException(Name, typeof(MudgeFont), null);

				// Update the coordinates for the characters.
				UpdateTextureCoordinates();
			}
			catch (SharpException sEx)
			{
				throw sEx;
			}
			catch (Exception ex)
			{
				throw new CannotLoadException(_fontPath, typeof(MudgeFont), ex);
			}
			finally
			{
				if (xmlFont != null)
					xmlFont.Close();

				xmlFont = null;
			}
		}

		/// <summary>
		/// Function to read the font from the disk.
		/// </summary>
		/// <param name="fontImage">Image to use for the font.</param>
		/// <param name="imageBasePath">Path that holds the image.</param>
		private void ReadFont(Image fontImage, string imageBasePath)
		{
			if (Path.GetExtension(_fontPath).ToLower() == ".xml")
				ReadXML();
			else
				ReadMudge();
		}

		/// <summary>
		/// Function to save the font to a mudge XML font.
		/// </summary>
		/// <param name="font">Font to export.</param>
		/// <param name="image">Font image.</param>
		/// <param name="filename">Name of the file.</param>
		public static void SaveMudgeXML(Font font, Drawing.Bitmap image, string filename)
		{
			Stream stream = null;				// File stream.
			XmlTextWriter writer = null;		// Binary file writer.

			try
			{
				stream = File.OpenWrite(filename);
				writer = new XmlTextWriter(stream,Encoding.UTF8);
				writer.Formatting = Formatting.Indented;

				// Write header.
				writer.WriteStartDocument();
				writer.WriteStartElement("font");

				// Write characters.
				foreach (FontCharacter character in font)
				{
					// Write character info.
					writer.WriteStartElement("char");
					writer.WriteAttributeString("id", character.CharacterOrdinal.ToString());
					writer.WriteAttributeString("x", character.Left.ToString());
					writer.WriteAttributeString("y", character.Top.ToString());
					writer.WriteAttributeString("width", character.Width.ToString());
					writer.WriteAttributeString("height", character.Height.ToString());
					writer.WriteEndElement();

					// Write kerning info.
					writer.WriteStartElement("spacing");
					writer.WriteAttributeString("id", character.CharacterOrdinal.ToString());
					writer.WriteAttributeString("a", character.A.ToString());
					writer.WriteAttributeString("b", character.B.ToString());
					writer.WriteAttributeString("c", character.C.ToString());
					writer.WriteEndElement();
				}

				writer.WriteEndElement();
				writer.WriteEndDocument();

				// Save the image.
				GorgonDevIL.SaveBitmap(Path.GetDirectoryName(filename) + @"\" + Path.GetFileNameWithoutExtension(filename) + ".tga", image);
			}
			catch (Exception ex)
			{
				throw new CannotSaveException(font.Name, typeof(MudgeFont), ex);
			}
			finally
			{
				if (writer != null)
					writer.Close();
				if (stream != null)
					stream.Close();
				stream = null;
				writer = null;
			}
		}

		/// <summary>
		/// Function to save the font to a mudge binary font.
		/// </summary>
		/// <param name="font">Font to export.</param>
		/// <param name="image">Font image.</param>
		/// <param name="filename">Name of the file.</param>
		public static void SaveMudgeBinary(Font font, Drawing.Image image, string filename)
		{
			Stream stream = null;			// File stream.
			BinaryWriter writer = null;		// Binary file writer.

			try
			{
				stream = File.OpenWrite(filename);
				writer = new BinaryWriter(stream);

				// Write header.
				writer.Write((int)0);
				writer.Write((int)1);

				// Write image dimensions.
				writer.Write(image.Width);
				writer.Write(image.Height);

				// Write border.
				writer.Write(font.LeftBorderOffset);
				writer.Write(font.TopBorderOffset);
				writer.Write(font.RightBorderOffset);
				writer.Write(font.BottomBorderOffset);

				// Write font size.
				writer.Write((int)font.SourceFontSize);

				// Write meta data.
				if (font.IsBold)
					writer.Write((int)700);
				else
					writer.Write((int)400);
				if (font.IsAntiAliased)
					writer.Write((int)4);
				else
					writer.Write((int)3);
				writer.Write((int)1);
				writer.Write(Convert.ToInt32(font.IsItalics));

				// Write face name.
				writer.Write(font.SourceFont.Length);
				writer.Write(Encoding.UTF8.GetBytes(font.SourceFont));
				// Write a null terminator.
				writer.Write((byte)0);

				// Write characters.
				writer.Write(font.Count);
				foreach (FontCharacter character in font)
				{
					writer.Write((byte)character.CharacterOrdinal);
					// Todo: Character borders.
					writer.Write((int)0);
					writer.Write((int)0);
					writer.Write((int)0);
					writer.Write((int)0);
				}
				writer.Write((int)0);
			}
			catch (Exception ex)
			{
				throw new CannotSaveException(font.Name, typeof(MudgeFont), ex);
			}
			finally
			{
				if (stream != null)
					stream.Close();
				if (writer != null)
					writer.Close();
				stream = null;
				writer = null;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name">Name of the font.</param>
		/// <param name="fontPath">Filename/path to the font.</param>		
		internal MudgeFont(string name, string fontPath)
			: base(name)
		{
			_fontPath = fontPath;
			ReadFont(null, null);
		}
		#endregion
	}
}
