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
// Created: Sunday, July 23, 2006 10:02:12 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;
using System.Xml;
using System.Resources;
using SharpUtilities;
using SharpUtilities.Mathematics;
using GorgonLibrary.Serialization;
using GorgonLibrary.FileSystems;

namespace GorgonLibrary.Graphics.Fonts
{
	/// <summary>
	/// Object representing a bitmapped font.
	/// </summary>		
	public class Font	
		: NamedObject, IEnumerable<Font.FontCharacter>, ISerializable
	{
		#region Value Types.
		/// <summary>
		/// Value type representing the font character.
		/// </summary>
		public struct FontCharacter
		{
			/// <summary>Index of the character.</summary>
			public int CharacterOrdinal;
			/// <summary>Character the font character will represent.</summary>
			public char Character;
			/// <summary>Left offset for the character.</summary>
			public float Left;
			/// <summary>Top offset for the character.</summary>
			public float Top;
			/// <summary>Width of the character.</summary>
			public float Width;
			/// <summary>Height of the character.</summary>
			public float Height;
			/// <summary>ABC A width.</summary>
			public int A;
			/// <summary>ABC B width.</summary>
			public uint B;
			/// <summary>ABC C width.</summary>
			public int C;
			/// <summary>Texture coordinates used by the character.</summary>
			internal RectangleF _textureCoordinates;
		}
		#endregion

		#region Variables.		
		private string _fontImageName = string.Empty;		// Path and filename of the font that holds the font image.

		/// <summary>Font path.</summary>
		protected string _fontPath;
		/// <summary>List of font characters.</summary>
		protected SortedList<char, FontCharacter> _characters;
		/// <summary>Offset within the image to start the texture at.</summary>
		protected Vector2D _offset;
		/// <summary>Image that holds the font.</summary>
		protected Image _fontImage;
		/// <summary>Name of the font that this font is derived from.</summary>
		protected string _sourceFont;
		/// <summary>Point size of the source font.</summary>
		protected float _sourceFontSize;
		/// <summary>Flag to indicate whether this font represents a bolded font.</summary>
		protected bool _bold;
		/// <summary>Flag to indicate whether this font represents an italics font.</summary>
		protected bool _italics;
		/// <summary>Flag to indicate whether this font represents an underlined font.</summary>
		protected bool _underline;
		/// <summary>Flag to indicate whether this font is antialiased or not.</summary>
		protected bool _antiAliased;
		/// <summary>Offset of the left border.</summary>
		protected int _leftOffset;
		/// <summary>Offset of the top border.</summary>
		protected int _topOffset;
		/// <summary>Offset of the right border.</summary>
		protected int _rightOffset;
		/// <summary>Offset of the bottom border.</summary>
		protected int _bottomOffset;
		/// <summary>Font is outlined.</summary>
		protected bool _outlined;
		/// <summary>Color of the outline.</summary>
		protected Color _outlineColor;
		/// <summary>Size of the outline.</summary>
		protected int _outlineSize;
		/// <summary>Font has a shadow.</summary>
		protected bool _shadowed;
		/// <summary>Color of the shadow.</summary>
		protected Color _shadowColor;
		/// <summary>Offset of the shadow.</summary>
		protected int _shadowOffset;
		/// <summary>Shadow direction.</summary>
		protected FontShadowDirection _shadowDirection;
		/// <summary>Flag to indicate that an image atlas holds the font image.</summary>
		protected bool _useAtlas;
		/// <summary>Font image size.</summary>		
		protected Vector2D _imageSize;
		/// <summary>Font base color.</summary>
		protected Color _baseColor;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the base color for the font.
		/// </summary>		
		public Color BaseColor
		{
			get
			{
				return _baseColor;
			}
		}

		/// <summary>
		/// Property to return the size of the image for the font.
		/// </summary>		
		public Vector2D ImageSize
		{
			get
			{
				return _imageSize;
			}
		}

		/// <summary>
		/// Property to return whether the font image is a texture atlas or not.
		/// </summary>		
		public bool ImageIsAtlas
		{
			get
			{
				return _useAtlas;
			}
			set
			{
				_useAtlas = value;				
				UpdateTextureCoordinates();
			}
		}

		/// <summary>
		/// Property to return whether the font is shadowed.
		/// </summary>
		public bool IsShadowed
		{
			get
			{
				return _shadowed;
			}
		}

		/// <summary>
		/// Property to return the color of the shadow.
		/// </summary>
		public Color ShadowColor
		{
			get
			{
				return _shadowColor;
			}
		}

		/// <summary>
		/// Property to return the offset of the shadow.
		/// </summary>
		public int ShadowOffset
		{
			get
			{
				return _shadowOffset;
			}
		}

		/// <summary>
		/// Property to return the shadow direction.
		/// </summary>
		public FontShadowDirection ShadowDirection
		{
			get
			{
				return _shadowDirection;
			}
		}

		/// <summary>
		/// Property to return whether the font has an outline or not.
		/// </summary>
		public bool IsOutlined
		{
			get
			{
				return _outlined;
			}
		}

		/// <summary>
		/// Property to return the color of the outline.
		/// </summary>
		public Color OutlineColor
		{
			get
			{
				return _outlineColor;
			}
		}

		/// <summary>
		/// Property to return the size of the outline.
		/// </summary>
		public int OutlineSize
		{
			get
			{
				return _outlineSize;
			}
		}

		/// <summary>
		/// Property to return the left border offset.
		/// </summary>
		public int LeftBorderOffset
		{
			get
			{
				return _leftOffset;
			}
		}

		/// <summary>
		/// Property to return the right border offset.
		/// </summary>
		public int RightBorderOffset
		{
			get
			{
				return _rightOffset;
			}
		}

		/// <summary>
		/// Property to return the top border offset.
		/// </summary>
		public int TopBorderOffset
		{
			get
			{
				return _topOffset;
			}
		}

		/// <summary>
		/// Property to return the bottom border offset.
		/// </summary>
		public int BottomBorderOffset
		{
			get
			{
				return _bottomOffset;
			}
		}

		/// <summary>
		/// Property to return the name of the font that this font is derived from.
		/// </summary>
		public string SourceFont
		{
			get
			{
				return _sourceFont;
			}
		}

		/// <summary>
		/// Property to return whether this font is anti-aliased.
		/// </summary>
		public bool IsAntiAliased
		{
			get
			{
				return _antiAliased;
			}
		}

		/// <summary>
		/// Property to return the size of the source font.
		/// </summary>
		public float SourceFontSize
		{
			get
			{
				return _sourceFontSize;
			}
		}

		/// <summary>
		/// Property to return whether the source font is bold or not.
		/// </summary>
		public bool IsBold
		{
			get
			{
				return _bold;
			}
		}

		/// <summary>
		/// Property to return whether the source font is underlined or not.
		/// </summary>
		public bool IsUnderlined
		{
			get
			{
				return _underline;
			}
		}

		/// <summary>
		/// Property to return whether the source font is in italics or not.
		/// </summary>
		public bool IsItalics
		{
			get
			{
				return _italics;
			}
		}

		/// <summary>
		/// Property to set or return the image used by the font.
		/// </summary>
		public Image FontImage
		{
			get
			{
				// Attempt to find the image.
				if ((_fontImage == null) && (_fontImageName != string.Empty))
				{
					if (Gorgon.ImageManager.Contains(_fontImageName))
						_fontImage = Gorgon.ImageManager[_fontImageName];
				}

				return _fontImage;
			}
			set
			{
				_fontImage = value;
				if (value != null)
					_fontImageName = value.Name;
				else
					_fontImageName = string.Empty;
			}
		}

		/// <summary>
		/// Property to return the character count.
		/// </summary>
		public int Count
		{
			get
			{
				return _characters.Count;
			}
		}

		/// <summary>
		/// Property to set or return the offset within the image to start the font from.
		/// </summary>
		public Vector2D ImageOffset
		{
			get
			{
				return _offset;
			}
			set
			{
				_offset = value;
				UpdateTextureCoordinates();
			}
		}

		/// <summary>
		/// Property to return the filename for the font.
		/// </summary>
		public string Filename
		{
			get
			{
				return _fontPath;
			}
		}

		/// <summary>
		/// Property to return the index of the font character.
		/// </summary>
		public Font.FontCharacter this[int index]
		{
			get
			{
				if ((index < 0) || (index >= _characters.Count))
					throw new SharpUtilities.Collections.IndexOutOfBoundsException(index);

				return _characters[_characters.Keys[index]];
			}
		}

		/// <summary>
		/// Property to return the index of the font character.
		/// </summary>
		public Font.FontCharacter this[char key]
		{
			get
			{
				if (!_characters.ContainsKey(key))
				{
					if (_characters.ContainsKey(' '))
						return _characters[' '];
					else
						throw new SharpUtilities.Collections.KeyNotFoundException(key.ToString());
				}

				return _characters[key];
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the texture coordinates for the font.
		/// </summary>
		protected virtual void UpdateTextureCoordinates()
		{
			if (_fontImage == null)
				return;

			// Update the texture coordinates.
			for (int i = 0; i < _characters.Count;i++)
			{
				RectangleF coords;			// Texture coordinates.
				FontCharacter character;	// Character.

				character = this[i];
				coords = character._textureCoordinates;
				if (_useAtlas)
				{
					coords.X = ((character.Left + _offset.X) + 0.5f) / _fontImage.ActualWidth;
					coords.Y = ((character.Top + _offset.Y) + 0.5f) / _fontImage.ActualHeight;
				}
				else
				{
					coords.X = ((character.Left) + 0.5f) / _fontImage.ActualWidth;
					coords.Y = ((character.Top) + 0.5f) / _fontImage.ActualHeight;
				}
				coords.Width = (character.Width) / _fontImage.ActualWidth;
				coords.Height = (character.Height) / _fontImage.ActualHeight;
				character._textureCoordinates = coords;
				_characters[character.Character] = character;
			}
		}

		/// <summary>
		/// Function to set the offset within an image to get the font from.
		/// </summary>
		/// <param name="x">Horizontal offset.</param>
		/// <param name="y">Vertical offset.</param>
		public void SetImageOffset(float x, float y)
		{
			ImageOffset = new Vector2D(x, y);
		}

		/// <summary>
		/// Function to return whether this font contains a specific character or not.
		/// </summary>
		/// <param name="character">Character to check for.</param>
		/// <returns>TRUE if the character exists, FALSE if not.</returns>
		public bool Contains(char character)
		{
			if (_characters.ContainsKey(character))
				return true;
			else
				return false;
		}

		/// <summary>
		/// Function to save the font to a stream.
		/// </summary>
		/// <param name="stream">Stream to save the font into.</param>
		/// <param name="isXML">TRUE if this is an xml file, FALSE for binary.</param>
		/// <param name="fileImagePath">Substitute file image path.</param>
		public virtual void Save(Stream stream, bool isXML, string fileImagePath)
		{
			ISerializer fontSerializer = null;		// Font serializer.
			string fontImagePath = string.Empty;	// Path to the font image.
			string fontPath = string.Empty;			// Path for the font.

			try
			{
				// If we didn't specify a file path for the image, then use the actual image path.
				if ((fileImagePath == null) || (fileImagePath == string.Empty))
					fontImagePath = FontImage.Filename;
				else
					fontImagePath = fileImagePath;

				// If the font image has no path, then we cannot save the font.
				if (fontImagePath == string.Empty)
					throw new CannotSaveException("Font image has no path, or is a memory resource.\nThe font image must exist in the same medium as the font.", null);

				if (stream is FileStream)
					fontPath = ((FileStream)stream).Name;
				else
					fontPath = "@Memory.GorgonFont";

				// Extract the directory name, if it's the same as the font, then just use the current directory.
				if ((Path.GetDirectoryName(fontImagePath) == Path.GetDirectoryName(fontPath)) || (Path.GetDirectoryName(fontImagePath) == string.Empty))
					fontImagePath = @".\" + Path.GetFileName(fontImagePath);

				// Serialize.
				if (isXML)
					fontSerializer = new XMLSerializer(this, stream);
				else
					fontSerializer = new BinarySerializer(this, stream);

				// Don't close the file stream, leave that to the user.
				fontSerializer.DontCloseStream = true;

				// Setup serializer.
				fontSerializer.Parameters.AddParameter<string>("ImagePath", fontImagePath);
				fontSerializer.Serialize();

				_fontPath = fontPath;
			}
			catch (Exception ex)
			{
				throw new CannotSaveException(fontPath, typeof(Font), ex);
			}
			finally
			{
				if (fontSerializer != null)
					fontSerializer.Dispose();
				fontSerializer = null;
			}
		}

		/// <summary>
		/// Function to save the binary font to a stream.
		/// </summary>
		/// <param name="stream">Stream to save the font into.</param>
		/// <param name="fileImagePath">Substitute file image path.</param>
		public void Save(Stream stream, string fileImagePath)
		{
			Save(stream, false, fileImagePath);
		}

		/// <summary>
		/// Function to save the font to a stream.
		/// </summary>
		/// <param name="stream">Stream to save the font into.</param>
		/// <param name="isXML">TRUE if the file is XML, FALSE for binary.</param>
		public void Save(Stream stream, bool isXML)
		{
			Save(stream, isXML, string.Empty);
		}

		/// <summary>
		/// Function to save the binary font to a stream.
		/// </summary>
		/// <param name="stream">Stream to save the font into.</param>
		public void Save(Stream stream)
		{
			Save(stream, false, string.Empty);
		}

		/// <summary>
		/// Function to save the font to a file.
		/// </summary>
		/// <param name="fileName">Path and filename of the font.</param>
		/// <param name="isXML">TRUE if this is an xml file, FALSE for binary.</param>
		/// <param name="fileImagePath">Substitute file image path.</param>
		public virtual void Save(string fileName, bool isXML, string fileImagePath)
		{
			Stream stream = null;		// Stream to save into.

			if ((fileName == null) || (fileName.Trim().Length == 0))
				throw new InvalidFilenameException(null);

			try
			{
				// Attach extension.
				if (Path.GetExtension(fileName) == "")
				{
					if (!isXML)
						fileName += ".gorFont";
					else
						fileName += ".xml";
				}

				// Open the file stream.
				stream = File.Open(fileName, FileMode.Create);

				// Serialize.
				Save(stream, isXML, fileImagePath);
			}
			catch (Exception ex)
			{
				throw new CannotSaveException(fileName, typeof(Font), ex);
			}
			finally
			{
				if (stream != null)
					stream.Dispose();
				stream = null;
			}
		}

		/// <summary>
		/// Function to save the font to a file within a file system.
		/// </summary>
		/// <param name="fileSystem">File system to save the font within.</param>
		/// <param name="fileName">Path and filename of the font.</param>
		/// <param name="isXML">TRUE if this is an xml file, FALSE for binary.</param>
		/// <param name="fileImagePath">Substitute file image path.</param>
		public virtual void Save(FileSystem fileSystem, string fileName, bool isXML, string fileImagePath)
		{
			MemoryStream stream = null;		// Stream to save into.
			byte[] data = null;				// Binary font data.

			if ((fileName == null) || (fileName.Trim().Length == 0))
				throw new InvalidFilenameException(null);

			try
			{
				// Attach extension.
				if (Path.GetExtension(fileName) == "")
				{
					if (!isXML)
						fileName += ".gorFont";
					else
						fileName += ".xml";
				}

				// Open the file stream.
				stream = new MemoryStream();
				Save(stream, isXML, fileImagePath);

				// Get binary data.
				stream.Position = 0;
				data = stream.ToArray();

				// Write to the file system.
				fileSystem.WriteFile(fileName, data);
			}
			catch (Exception ex)
			{
				throw new CannotSaveException(fileName, typeof(Font), ex);
			}
			finally
			{
				if (stream != null)
					stream.Dispose();
				stream = null;
			}
		}

		/// <summary>
		/// Function to save the font to a file within a file system.
		/// </summary>
		/// <param name="fileSystem">File system to save the font within.</param>
		/// <param name="fileName">Path and filename of the font.</param>
		/// <param name="isXML">TRUE if this is an xml file, FALSE for binary.</param>		
		public virtual void Save(FileSystem fileSystem, string fileName, bool isXML)
		{
			Save(fileSystem, fileName, isXML, string.Empty);
		}
		
		/// <summary>
		/// Function to save the font to a file within a file system.
		/// </summary>
		/// <param name="fileSystem">File system to save the font within.</param>
		/// <param name="fileName">Path and filename of the font.</param>
		/// <param name="fileImagePath">Substitute file image path.</param>
		public virtual void Save(FileSystem fileSystem, string fileName, string fileImagePath)
		{
			if (Path.GetExtension(fileName).ToLower() == ".xml")
				Save(fileSystem, fileName, true, fileImagePath);
			else
				Save(fileSystem, fileName, false, fileImagePath);
		}

		/// <summary>
		/// Function to save the font to a file within a file system.
		/// </summary>
		/// <param name="fileSystem">File system to save the font within.</param>
		/// <param name="fileName">Path and filename of the font.</param>
		public virtual void Save(FileSystem fileSystem, string fileName)
		{
			if (Path.GetExtension(fileName).ToLower() == ".xml")
				Save(fileSystem, fileName, true, string.Empty);
			else
				Save(fileSystem, fileName, false, string.Empty);
		}

		/// <summary>
		/// Function to save the font to a file.
		/// </summary>
		/// <param name="fileName">Path and filename of the font.</param>
		/// <param name="isXML">TRUE to save as XML, FALSE to save as binary.</param>
		public void Save(string fileName, bool isXML)
		{
			Save(fileName, isXML, string.Empty);
		}

		/// <summary>
		/// Function to save the font to a file.
		/// </summary>
		/// <param name="fileName">Path and filename of the font.</param>
		/// <param name="fileImagePath">Substitute file image path.</param>
		public void Save(string fileName, string fileImagePath)
		{
			if (Path.GetExtension(fileName).ToLower() == ".xml")
				Save(fileName, true, fileImagePath);
			else
				Save(fileName, false, fileImagePath);
		}

		/// <summary>
		/// Function to save font to a file.
		/// </summary>
		/// <param name="fileName">Path and filename of the font.</param>
		public void Save(string fileName)
		{
			if (Path.GetExtension(fileName).ToLower() == ".xml")
				Save(fileName, true, string.Empty);
			else
				Save(fileName, false, string.Empty);
		}

		/// <summary>
		/// Function to copy a font into this font.
		/// </summary>
		/// <param name="source">Font to copy.</param>
		public virtual void Copy(Font source)
		{
			Font newFont = new Font(source.Name);	// New font.
			((ISerializable)newFont).Filename = source.Filename;

			// Copy metadata.
			_antiAliased = source._antiAliased;
			_baseColor = source._baseColor;
			_bold = source._bold;
			_bottomOffset = source._bottomOffset;
			_fontImage = source._fontImage;
			_fontPath = source._fontPath;
			_imageSize = source._imageSize;
			_italics = source._italics;
			_leftOffset = source._leftOffset;
			_offset = source._offset;
			_outlineColor = source._outlineColor;
			_outlined = source._outlined;
			_outlineSize = source._outlineSize;
			_rightOffset = source._rightOffset;
			_shadowColor = source._shadowColor;
			_shadowDirection = source._shadowDirection;
			_shadowed = source._shadowed;
			_shadowOffset = source._shadowOffset;
			_sourceFont = source._sourceFont;
			_sourceFontSize = source._sourceFontSize;
			_topOffset = source._topOffset;
			_underline = source._underline;
			_useAtlas = source._useAtlas;

			// Copy the characters.
			_characters.Clear();

			// Copy characters.
			foreach (KeyValuePair<char, FontCharacter> character in source._characters)
				_characters.Add(character.Value.Character, character.Value);

			UpdateTextureCoordinates();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name">Name of the font.</param>
		protected internal Font(string name)
			: base(name)
		{
			_fontPath = string.Empty;
			_characters = new SortedList<char, FontCharacter>(256);
			_sourceFont = "Unknown";
			_sourceFontSize = 0.0f;
			_underline = false;
			_bold = false;
			_italics = false;
			_antiAliased = false;
			_topOffset = _bottomOffset = _rightOffset = _leftOffset = 0;
			_outlineSize = 1;
			_outlined = false;
			_outlineColor = Color.FromArgb(0, 0, 0);
			_shadowed = false;
			_shadowOffset = 2;
			_shadowDirection = FontShadowDirection.LowerRight;
			_shadowColor = Color.FromArgb(192, 0, 0, 0);
			_useAtlas = false;
			_imageSize = Vector2D.Zero;
			_baseColor = Color.White;
		}
		#endregion

		#region IEnumerable<FontCharacter> Members
		/// <summary>
		/// Gets the enumerator.
		/// </summary>
		/// <returns></returns>
		public IEnumerator<FontCharacter> GetEnumerator()
		{
			foreach (KeyValuePair<char, FontCharacter> pair in _characters)
				yield return pair.Value;
		}

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		#endregion	
	
		#region ISerializable<Font,FontSerializer> Members
		/// <summary>
		/// Property to set or return the filename of the serializable object.
		/// </summary>
		string ISerializable.Filename
		{
			get
			{
				return _fontPath;
			}
			set
			{
				if (value == null)
					value = string.Empty;

				_fontPath = value;
			}
		}

		/// <summary>
		/// Function to persist the data into the serializer stream.
		/// </summary>
		/// <param name="serializer">Serializer that's calling this function.</param>
		void ISerializable.WriteData(ISerializer serializer)
		{
			string fontImagePath = string.Empty;		// Font image path.

			// Get the font image path.
			if (serializer.Parameters.Contains("ImagePath"))
				fontImagePath = serializer.Parameters.GetParameter<string>("ImagePath");

			serializer.WriteComment("Gorgon Font - " + _objectName);
			serializer.WriteComment("Written by Michael Winsor (Tape_Worm) for the Gorgon library.");
			serializer.WriteComment("Use at your own peril.");

			// Root region.
			serializer.WriteGroupBegin("GorgonFont");
			// Header.
			serializer.WriteGroupBegin("Header");			
			serializer.Write<string>("HeaderValue", "GORFNT1");
			// Header.
			serializer.WriteGroupEnd();
			// Metadata.
			serializer.WriteGroupBegin("MetaData");
			serializer.Write<string>("Name", _objectName);
			serializer.Write<string>("SourceFont", _sourceFont);
			serializer.Write<float>("SourceFontSize", _sourceFontSize);
			serializer.Write<int>("BaseFontColor", _baseColor.ToArgb());
			serializer.Write<bool>("IsBold", _bold);
			serializer.Write<bool>("IsUnderlined", _underline);
			serializer.Write<bool>("IsItalic", _italics);
			serializer.Write<bool>("IsAntiAliased", _antiAliased);
			serializer.Write<bool>("IsOutlined", _outlined);
			serializer.Write<bool>("IsShadowed", _shadowed);
			if (_outlined)
			{
				serializer.WriteComment("These lines only appear when IsOutlined = TRUE");
				serializer.Write<int>("OutlineColor", _outlineColor.ToArgb());
				serializer.Write<int>("OutlineSize", _outlineSize);
			}
			if (_shadowed)
			{
				serializer.WriteComment("These lines only appear when IsShadowed = TRUE");
				serializer.Write<int>("ShadowColor", _shadowColor.ToArgb());
				serializer.Write<int>("ShadowOffset", _shadowOffset);
				serializer.Write<int>("ShadowDirection", (int)_shadowDirection);
			}

			// Metadata.
			serializer.WriteGroupEnd();
			// Font data.
			serializer.WriteGroupBegin("Font");
			// Font image.
			if (fontImagePath != string.Empty)
			{
				serializer.WriteGroupBegin("FontImage");
				serializer.Write<string>("ImagePath", Path.GetDirectoryName(fontImagePath) + @"\");
				serializer.Write<string>("ImageName", Path.GetFileName(fontImagePath));
				serializer.Write<float>("ImageSizeWidth", _imageSize.X);
				serializer.Write<float>("ImageSizeHeight", _imageSize.Y);
				serializer.Write<float>("OffsetX", _offset.X);
				serializer.Write<float>("OffsetY", _offset.Y);
				serializer.Write<bool>("IsAtlas", _useAtlas);
				// Font image.
				serializer.WriteGroupEnd();
			}
			// Borders.
			serializer.WriteGroupBegin("Borders");
			serializer.Write<int>("LeftOffset", _leftOffset);
			serializer.Write<int>("TopOffset", _topOffset);
			serializer.Write<int>("RightOffset", _rightOffset);
			serializer.Write<int>("BottomOffset", _bottomOffset);
			// Borders.
			serializer.WriteGroupEnd();
			// Characters.
			serializer.WriteGroupBegin("Characters");
			serializer.Write<int>("Count", _characters.Count);
			foreach (KeyValuePair<char, FontCharacter> character in _characters)
			{
				serializer.Write<int>("Ordinal", character.Value.CharacterOrdinal);
				serializer.Write<float>("X", character.Value.Left);
				serializer.Write<float>("Y", character.Value.Top);
				serializer.Write<float>("Width", character.Value.Width);
				serializer.Write<float>("Height", character.Value.Height);
				serializer.Write<int>("A", character.Value.A);
				serializer.Write<uint>("B", character.Value.B);
				serializer.Write<int>("C", character.Value.C);
			}
			// Characters.
			serializer.WriteGroupEnd();
			// Font data.
			serializer.WriteGroupEnd();
			// Gorgon font.
			serializer.WriteGroupEnd();
		}

		/// <summary>
		/// Function to retrieve data from the serializer stream.
		/// </summary>
		/// <param name="serializer">Serializer that's calling this function.</param>
		void ISerializable.ReadData(ISerializer serializer)
		{
			string header = string.Empty;		// Header.
			string imagePath = string.Empty;	// Image path.
			string imageName = string.Empty;	// Image name.
			int charCount = 0;					// Character count.
			FileSystem fileSystem = null;		// File system to use.

			// Retrieve the file system if it exists.
			if (serializer.Parameters.Contains("FileSystem"))
				fileSystem = serializer.Parameters.GetParameter<FileSystem>("FileSystem");

			// Get the header value.
			header = serializer.Read<string>("HeaderValue");

			if (header.ToLower() != "gorfnt1")
				throw new InvalidResourceException("Unable to deserialize the font.  This does not appear to be a Gorgon font.", null);

			// Get data.
			_objectName = serializer.Read<string>("Name");
			_sourceFont = serializer.Read<string>("SourceFont");
			_sourceFontSize = serializer.Read<float>("SourceFontSize");
			_baseColor = Color.FromArgb(serializer.Read<int>("BaseFontColor"));
			_bold = serializer.Read<bool>("IsBold");
			_underline = serializer.Read<bool>("IsUnderlined");
			_italics = serializer.Read<bool>("IsItalic");
			_antiAliased = serializer.Read<bool>("IsAntiAliased");
			_outlined = serializer.Read<bool>("IsOutlined");
			_shadowed = serializer.Read<bool>("IsShadowed");

			if (_outlined)
			{
				_outlineColor = Color.FromArgb(serializer.Read<int>("OutlineColor"));
				_outlineSize = serializer.Read<int>("OutlineSize");
			}

			if (_shadowed)
			{
				_shadowColor = Color.FromArgb(serializer.Read<int>("ShadowColor"));
				_shadowOffset = serializer.Read<int>("ShadowOffset");
				_shadowDirection = (FontShadowDirection)serializer.Read<int>("ShadowDirection");
			}

			imagePath = serializer.Read<string>("ImagePath");
			imageName = serializer.Read<string>("ImageName");
			_imageSize.X = serializer.Read<float>("ImageSizeWidth");
			_imageSize.Y = serializer.Read<float>("ImageSizeHeight");
			_offset.X = serializer.Read<float>("OffsetX");
			_offset.Y = serializer.Read<float>("OffsetY");
			_useAtlas = serializer.Read<bool>("IsAtlas");

			_leftOffset = serializer.Read<int>("LeftOffset");
			_topOffset = serializer.Read<int>("TopOffset");
			_rightOffset = serializer.Read<int>("RightOffset");
			_bottomOffset = serializer.Read<int>("BottomOffset");

			charCount = serializer.Read<int>("Count");

			// Get characters.
			for (int i = 0; i < charCount; i++)
			{
				FontCharacter character = new FontCharacter();	// Font character.

				character.CharacterOrdinal = serializer.Read<int>("Ordinal");
				character.Character = Convert.ToChar(character.CharacterOrdinal);
				character.Left = serializer.Read<float>("X");
				character.Top = serializer.Read<float>("Y");
				character.Width = serializer.Read<float>("Width");
				character.Height = serializer.Read<float>("Height");
				character.A = serializer.Read<int>("A");
				character.B = serializer.Read<uint>("B");
				character.C = serializer.Read<int>("C");

				// Add to list.
				_characters.Add(character.Character, character);
			}

			if ((imageName == null) || (imageName == string.Empty) || (imagePath == null) || (imagePath == string.Empty))
				throw new CannotLoadException(_objectName, typeof(Font), null);

			// Finally, grab the image.
			if (serializer.Parameters.Contains("Image"))
				FontImage = serializer.Parameters.GetParameter<Image>("Image");
			else
			{
				// Get font image path and name.
				_fontImageName = Path.GetFileNameWithoutExtension(imageName);

				// If the image already exists, then use that.
				if (Gorgon.ImageManager.Contains(_fontImageName))
					FontImage = Gorgon.ImageManager[_fontImageName];
				else
				{
					// Adjust the font path.
					if (imagePath == @".\")
						imagePath = Path.GetDirectoryName(_fontPath) + @"\";

					if (fileSystem == null)
					{
						// Load from disk if we're not a resource.
						if (!serializer.Parameters.Contains("ResourceManager"))
							FontImage = Gorgon.ImageManager.FromFile(imagePath + imageName);
						else
						{
							// Load the image from a resource if this font is resource based.
							ResourceManager resManager = serializer.Parameters.GetParameter<ResourceManager>("ResourceManager");
							FontImage = Gorgon.ImageManager.FromResource(_fontImageName, resManager);
						}
					}
					else
					{
						// If the image exists, load it.
						if (fileSystem.FileExists(imagePath + imageName))
							FontImage = Gorgon.ImageManager.FromFileSystem(fileSystem, imagePath + imageName);
						else
						{
							FileSystemEntry fontEntry = null;		// Font image file system entry.

							// Find the nearest entry and use it.
							fontEntry = fileSystem.FindFile(imageName);
							if (fontEntry != null)
								FontImage = Gorgon.ImageManager.FromFileSystem(fileSystem, fontEntry.FullPath);
						}
					}
				}
			}

			// Update texture coordinates.
			UpdateTextureCoordinates();
		}
		#endregion
	}
}

