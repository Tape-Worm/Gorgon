#region MIT
// 
// Gorgon.
// Copyright (C) 2021 Michael Winsor
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
// Created: September 1, 2021 6:49:26 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Drawing = System.Drawing;
using Gorgon.Editor.Content;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Fonts;
using Gorgon.Graphics.Fonts.Codecs;
using Gorgon.IO;
using System.Windows.Forms;
using Gorgon.Editor.FontEditor.Properties;
using System.Diagnostics;

namespace Gorgon.Editor.FontEditor
{
    /// <summary>
    /// The service used to handle fonts.
    /// </summary>
    internal class FontService
        : IDisposable
    {
        #region Variables.
        // The factory used to build fonts.
        private readonly GorgonFontFactory _factory;
        // The worker font.
        private GorgonFont _font;
        // The file manager for the application.
        private readonly IContentFileManager _fileManager;
        // The file codec.
        private readonly GorgonFontCodec _codec;
        // The lock used to ensure that the font generation remains non-reentrant.
        private int _lock;
        // The textures for the font.
        private readonly List<GorgonTexture2DView> _textures = new();
        // The list of glyphs, grouped by texture.
        private readonly Dictionary<GorgonTexture2DView, IReadOnlyList<GorgonGlyph>> _textureGlyphs = new();

        /// <summary>
        /// Default character list.
        /// </summary>
        public static readonly IEnumerable<char> DefaultCharacters = Enumerable.Range(32, 224)
                                                                               .Select(Convert.ToChar)
                                                                               .Where(c => !char.IsControl(c));
        #endregion

        #region Events.
        /// <summary>
        /// Event fired before the font is updated.
        /// </summary>
        private event EventHandler FontUpdatingEvent;
        /// <summary>
        /// Event fired after the font is updated.
        /// </summary>
        private event EventHandler FontUpdatedEvent;
        
        /// <summary>
        /// Event fired before the font is updated.
        /// </summary>
        public event EventHandler FontUpdating
        {
            add
            {
                if (value is null)
                {
                    FontUpdatingEvent = null;
                    return;
                }

                FontUpdatingEvent += value;
            }
            remove
            {
                if (value is null)
                {
                    return;
                }

                FontUpdatingEvent -= value;
            }
        }

        /// <summary>
        /// Event fired after the font is updated.
        /// </summary>
        public event EventHandler FontUpdated
        {
            add
            {
                if (value is null)
                {
                    FontUpdatedEvent = null;
                    return;
                }

                FontUpdatedEvent += value;
            }
            remove
            {
                if (value is null)
                {
                    return;
                }

                FontUpdatedEvent -= value;
            }
        }
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the textures for the font.
        /// </summary>
        public IReadOnlyList<GorgonTexture2DView> Textures => _textures;

        /// <summary>
        /// Property to return all the glyphs, grouped by texture.
        /// </summary>
        public IReadOnlyDictionary<GorgonTexture2DView, IReadOnlyList<GorgonGlyph>> TextureGlyphs => _textureGlyphs;

        /// <summary>
        /// Property to return the total number of textures, and array indices used by the font.
        /// </summary>
        public int TotalTextureArrayCount
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the current worker font.
        /// </summary>
        public GorgonFont WorkerFont => _font;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve the list of textures for the font.
        /// </summary>
        private void GetTextures()
        {
            _textures.Clear();

            if (_font is null)
            {
                TotalTextureArrayCount = 0;
                return;
            }

            _textures.AddRange(_font.Glyphs.Where(item => item.TextureView is not null).Select(item => item.TextureView).Distinct());

            _textureGlyphs.Clear();
            foreach (IGrouping<GorgonTexture2DView, GorgonGlyph> g in (from glyphs in _font.Glyphs
                                                                       where glyphs.TextureView is not null
                                                                       group glyphs by glyphs.TextureView))
            {
                _textureGlyphs[g.Key] = g.ToArray();
            }

            TotalTextureArrayCount = _textures.Sum(item => item.ArrayCount);
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            _textures.Clear();
            TotalTextureArrayCount = 0;
            FontUpdatingEvent = null;
            FontUpdatedEvent = null;

            _font = null;
            _factory?.InvalidateCache();
        }

        /// <summary>
        /// Function to generate a new font information data structure.
        /// </summary>
        /// <param name="baseFontName">The base name for the font.</param>
        /// <param name="fontDirectory">The directory containing the new font.</param>
        /// <param name="prevFont">Previous font information.</param>
        /// <returns>A new font information data structure or <b>null</b> if cancelled.</returns>
        public (string NewName, GorgonFontInfo FontInfo) GetNewFontInfo(string baseFontName, string fontDirectory, GorgonFontInfo prevFont)
        {
            string fontPath = fontDirectory.FormatDirectory('/') + baseFontName.FormatFileName();

            int counter = 0;
            while (_fileManager.GetFile(fontPath) is not null)
            {
                string filename = Path.GetFileNameWithoutExtension(baseFontName);
                string extension = Path.GetExtension(baseFontName);

                fontPath = $"{fontDirectory}{filename} ({++counter}){extension}";
            }

            using FormNewFont newFont = new()
            {
                FontName = Path.GetFileName(fontPath) ?? string.Empty,
                FontSize = prevFont?.Size ?? 9.0f,
                FontHeightMode = prevFont?.FontHeightMode ?? FontHeightMode.Points,                
                FontStyle = prevFont?.FontStyle ?? FontStyle.Normal,
                AntiAliasingMode = prevFont?.AntiAliasingMode ?? FontAntiAliasMode.AntiAlias,
                TextureWidth = prevFont?.TextureWidth ?? 512,
                TextureHeight = prevFont?.TextureHeight ?? 512,
                Characters = prevFont?.Characters ?? DefaultCharacters,
                FontFamilyName = prevFont?.FontFamilyName ?? string.Empty
            };
    
            if (newFont.ShowDialog() == DialogResult.Cancel)
            {
                return (string.Empty, null);
            }

            return (newFont.FontName, new GorgonFontInfo(newFont));
        }

        /// <summary>
        /// Function to save the current version of the font.
        /// </summary>
        /// <param name="filePath">The file used to store the font data.</param>
        /// <returns>The content file.</returns>
        public async Task<IContentFile> SaveAsync(string filePath)
        {
            Stream outStream = null;

            void SaveFont() => _codec.Save(_font, outStream);

            IContentFile result = _fileManager.GetFile(filePath);

            // This file is no longer "new".
            try
            {
                outStream = _fileManager.OpenStream(filePath, FileMode.Create);
                await Task.Run(SaveFont);
                outStream.Close();

                result = _fileManager.GetFile(filePath);

                Debug.Assert(result is not null, "No file is found after saving. This is not right human.");

                return result;
            }
            finally
            {
                outStream?.Dispose();
                result.Refresh();
                result.IsOpen = true;
            }
        }

        /// <summary>
        /// Function to update the font with new information.
        /// </summary>
        /// <param name="fontInfo">The font information used to update the font.</param>
        /// <returns>A font information object with updated data.</returns>
        public async Task<GorgonFontInfo> UpdateFontAsync(GorgonFontInfo fontInfo)
        {
            if (Interlocked.Exchange(ref _lock, 1) != 0)
            {
                return fontInfo;
            }

            try
            {
                FontUpdatingEvent?.Invoke(this, EventArgs.Empty);

                _font?.Dispose();
                _font = null;
                // We have to do this -after- we dispose, else the cache will think the font is still valid.
                _font = await _factory.GetFontAsync(fontInfo);

                GetTextures();

                FontUpdatedEvent?.Invoke(this, EventArgs.Empty);

                return fontInfo;
            }
            finally
            {
                Interlocked.Exchange(ref _lock, 0);
            }            
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="FontService" /> class.</summary>
        /// <param name="factory">The factory used to build the font.</param>
        /// <param name="font">The current font to use as the worker font.</param>
        /// <param name="codec">The codec used to write the font data.</param>
        /// <param name="fileManager">The file manager used to write data.</param>
        public FontService(GorgonFontFactory factory, GorgonFont font, GorgonFontCodec codec, IContentFileManager fileManager)
        {
            _codec = codec;
            _fileManager = fileManager;
            _factory = factory;
            _font = font;

            GetTextures();
        }
        #endregion        
    }
}
