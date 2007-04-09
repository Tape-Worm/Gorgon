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
// Created: Tuesday, September 26, 2006 5:20:42 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Resources;
using System.IO;
using Drawing = System.Drawing;
using GorgonLibrary;
using GorgonLibrary.Graphics;
using GorgonLibrary.Graphics.Fonts;
using GorgonLibrary.Serialization;

namespace GorgonLibrary.GorgonGUI
{
	/// <summary>
	/// Object representing a skin for the gorgon GUI.
	/// </summary>
	public class GUISkin
		: IDisposable, ISerializable, IEnumerable<Sprite>
    {
        #region Variables.
        private Image _guiImage = null;			        // GUI skin image.
		private SpriteManager _pieces = null;			// Sprite pieces.
		private Font _guiFont = null;			        // Default GUI font.
        private Sprite _defaultCursor = null;           // Default cursor.
		private Drawing.Color _fontColor;				// Default font color.
		private string _skinPath = string.Empty;		// Skin path.
		private string _fileName = string.Empty;		// Filename.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the path to the skin.
		/// </summary>
		public string SkinPath
		{
			get
			{
				return _skinPath;
			}
		}

        /// <summary>
        /// Property to set or return the main cursor used by the GUI.
        /// </summary>
        public Sprite Cursor
        {
            get
            {
                return _defaultCursor;
            }
            set
            {
				if (value == null)
					throw new ArgumentNullException();

				_defaultCursor = new Sprite("GUI.Cursor" + value.Name, _guiImage, value.ImageRegion.Left, value.ImageRegion.Top, value.ImageRegion.Width, value.ImageRegion.Height);
            }
        }

		/// <summary>
		/// Property to set or return the image used for rendering the GUI.
		/// </summary>
		public Image Image
		{
			get
			{
				return _guiImage;
			}
			set
			{
				_guiImage = value;
			}
		}

		/// <summary>
		/// Property to set or return the default font for rendering the GUI.
		/// </summary>
		public Font Font
		{
			get
			{
				return _guiFont;
			}
			set
			{
				_guiFont = value;				
			}
		}

		/// <summary>
		/// Property to set or return the default font color.
		/// </summary>
		public Drawing.Color DefaultFontColor
		{
			get
			{
				return _fontColor;
			}
			set
			{
				_fontColor = value;
			}
		}

        /// <summary>
        /// Property to return a sprite piece for this skin.
        /// </summary>
        public Sprite this[string pieceName]
        {
            get
            {
				return _pieces[pieceName];
            }
        }
		#endregion

        #region Methods.
        /// <summary>
        /// Function to create a GUI control type.
        /// </summary>
        /// <param name="pieceName">Name of the type.</param>
		/// <param name="x">Horitzontal position on the skin image.</param>
		/// <param name="y">Vertical position on the skin image.</param>
		/// <param name="width">Width of the piece on the skin image.</param>
		/// <param name="height">Height of the piece on the skin image.</param>
        /// <returns>A new GUI sprite piece.</returns>
        public Sprite Create(string pieceName, float x, float y, float width, float height)
        {
            Sprite piece = null;      // Sprite piece.
			
			piece = new Sprite(pieceName, _guiImage, x, y, width, height);
			_pieces.Add(piece);

            return piece;
        }

		/// <summary>
		/// Function to remove a sprite piece.
		/// </summary>
		/// <param name="pieceName">Name of the sprite piece to remove.</param>
		public void Remove(string pieceName)
		{
			_pieces.Remove(pieceName);
		}
        #endregion

        #region Constructor/Destructor.
        /// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="skinPath">Path to the GUI skin.</param>        
		internal GUISkin(string skinPath)
		{
			_skinPath = skinPath;
			_fontColor = Drawing.Color.White;
			_pieces = new SpriteManager();
			_guiFont = FontManager.DefaultFont;
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		/// <param name="disposing">TRUE to release all resources, FALSE to only release unmanaged.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
				_pieces.Clear();

			// Do unmanaged clean up.
			_guiImage = null;
			_guiFont = null;
            _defaultCursor = null;
		}

		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion

        #region ISerializable Members
		/// <summary>
		/// Property to set or return the filename for the skin.
		/// </summary>
		string ISerializable.Filename
		{
			get
			{
				return _fileName;
			}
			set
			{
				_fileName = value;
			}
		}

        /// <summary>
        /// Function to persist the data into the serializer stream.
        /// </summary>
        /// <param name="serializer">Serializer that's calling this function.</param>
        void ISerializable.WriteData(ISerializer serializer)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// Function to retrieve data from the serializer stream.
        /// </summary>
        /// <param name="serializer">Serializer that's calling this function.</param>
		void ISerializable.ReadData(ISerializer serializer)
        {
			string header = string.Empty;						// Header for the file.
			string controlTypeName = string.Empty;				// Control type name.
			string pieceName = string.Empty;					// Control sprite piece name.
			int controlPieceCount = 0;							// Control piece count.
			string imageName = string.Empty;					// Image name.
			string imagePath = string.Empty;					// Image path.
			string imageLocation = string.Empty;				// Location of the image.
			string cursorName = string.Empty;					// Name of the default cursor.
			string fontName = string.Empty;						// Default font name.
			string fontPath = string.Empty;						// Default font path.
			string fontLocation = string.Empty;					// Location of the font.			
			FontManager fonts = null;							// Font manager.
			Drawing.RectangleF rect = Drawing.RectangleF.Empty;	// Piece rectangle.

			// Get the header.
			header = serializer.Read<string>("HeaderValue");

			if (header.ToLower() != "gorskin1")
				throw new InvalidResourceException("Unable to deserialize GUI skin.  This does not appear to be a Gorgon GUI skin.", null);

			// Get the sprite image.
			imageName = serializer.Read<string>("Name");
			imagePath = serializer.Read<string>("Path");

			// Get the path.
			if (imagePath == @".\")
				imagePath = Path.GetDirectoryName(_skinPath) + @"\";

			// If we're in a resource location, then load from the application resources.
			if (!Gorgon.ImageManager.Contains(Path.GetFileNameWithoutExtension(imageName)))
			{
				if (serializer.Parameters.Contains("ResourceManager"))
					_guiImage = Gorgon.ImageManager.FromResource(imageName, serializer.Parameters.GetParameter<ResourceManager>("ResourceManager"));
				else
					_guiImage = Gorgon.ImageManager.FromFile(imagePath + imageName);
			}
			else
				_guiImage = Gorgon.ImageManager[Path.GetFileNameWithoutExtension(imageName)];

			// Get cursor.
			cursorName = serializer.Read<string>("Name");
			rect.Y = serializer.Read<float>("Top");
			rect.X = serializer.Read<float>("Left");
			rect.Width = serializer.Read<float>("Width");
			rect.Height = serializer.Read<float>("Height");
			_defaultCursor = new Sprite("GUI.Cursor." + cursorName, _guiImage, rect.X, rect.Y, rect.Width, rect.Height);

			// Get default font.
			fonts = new FontManager();
			fontName = serializer.Read<string>("Name");
			fontPath = serializer.Read<string>("Path");
			_fontColor = Drawing.Color.FromArgb(serializer.Read<int>("DefaultColor"));

			if (fontPath == @".\")
				fontPath = Path.GetDirectoryName(_skinPath) + @"\";

			// If we've specified the default font, then use that.
			if (fontName.ToLower() == "@default_font")
				Font = FontManager.DefaultFont;
			else
			{
				if (!fonts.Contains(Path.GetFileNameWithoutExtension(fontName)))
				{
					if (serializer.Parameters.Contains("ResourceManager"))
						Font = fonts.FromResource(fontName, serializer.Parameters.GetParameter<ResourceManager>("ResourceManager"));
					else
						Font = fonts.FromFile(fontPath + fontName);
				}
				else
					Font = fonts[Path.GetFileNameWithoutExtension(fontName)];
			}			
			
			controlPieceCount = serializer.Read<int>("PieceCount");

			// Get sprite pieces.
			for (int i = 0; i < controlPieceCount; i++)
			{
				pieceName = serializer.ReadAttribute("Piece", "Name");
				rect.Y = serializer.Read<float>("Top");
				rect.X = serializer.Read<float>("Left");
				rect.Width = serializer.Read<float>("Width");
				rect.Height = serializer.Read<float>("Height");

				Create(pieceName, rect.X, rect.Y, rect.Width, rect.Height);
			}
        }
		#endregion

		#region IEnumerable<Sprite> Members
		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<Sprite> GetEnumerator()
		{
			foreach (Sprite piece in _pieces)
				yield return piece;
		}

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.
		/// </returns>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return _pieces.GetEnumerator();
		}
		#endregion
	}
}
