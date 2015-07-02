#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Wednesday, October 3, 2012 9:14:18 PM
// 
#endregion

using System;
using System.Drawing;
using Gorgon.Animation.Properties;
using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.IO;
using Gorgon.UI;

namespace Gorgon.Animation
{
	/// <summary>
	/// A key frame that manipulates a 2D texture data type.
	/// </summary>
	public struct GorgonKeyTexture2D
		: IKeyFrame
	{
		#region Variables.
		private readonly Type _dataType;					// Type of data for the key frame.

		private String _textureName;						// Texture name.
		private GorgonTexture2DSettings _settings;			// Texture settings.

		/// <summary>
		/// Value to store in the key frame.
		/// </summary>
		public GorgonTexture2D Value;
		/// <summary>
		/// Region on the texture to update.
		/// </summary>
		public RectangleF TextureRegion;
		/// <summary>
		/// Time for the key frame in the animation.
		/// </summary>
		public float Time;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the texture if it's not assigned.
		/// </summary>
		/// <returns><b>true</b> if the texture was found, <b>false</b> if not.</returns>
		internal bool GetTexture()
		{
			if (!string.IsNullOrEmpty(_textureName))
			{
				// Copy to local values because LINQ doesn't seem to work with members
				// of a struct.
				string textureName = _textureName;
				GorgonTexture2DSettings settings = _settings;

				// Our texture is deferred, so we need to find it in the graphics object tracked
				// object list.
				// In order to do that, we need to find all graphics objects first:
				var graphics = GorgonApplication.GetTrackedObjectsOfType<GorgonGraphics>();

				// We have no graphics, then we can't do display a texture anyway, so throw an exception.
			    if (graphics.Count == 0)
			    {
			        throw new GorgonException(GorgonResult.NotInitialized, Resources.GORANM_KEYFRAME_TEXTURE_NOGFX);
			    }

			    var textures = (from graphicsObject in graphics
			                    from graphicsTexture in graphicsObject.GetTrackedObjectsOfType<GorgonTexture2D>()
			                    select graphicsTexture).ToArray();

			    // Then, we begin our search by looking at -all- the texture information we have:
				Value = (from graphicsTexture in textures
						 where (string.Equals(graphicsTexture.Name, textureName, StringComparison.OrdinalIgnoreCase)) &&
						  (graphicsTexture.Settings.ArrayCount == settings.ArrayCount) &&
						  (graphicsTexture.Settings.Format == settings.Format) &&
						  (graphicsTexture.Settings.Height == settings.Height) &&
						  (graphicsTexture.Settings.Width == settings.Width) &&
						  (graphicsTexture.Settings.IsTextureCube == settings.IsTextureCube) &&
						  (graphicsTexture.Settings.MipCount == settings.MipCount) &&
						  (graphicsTexture.Settings.Multisampling == settings.Multisampling)
						 select graphicsTexture).FirstOrDefault();

				// ReSharper disable ConvertIfStatementToNullCoalescingExpression
				if (Value == null)
				{
					// That one failed, so just try and look it up by name, width, height and format.
					Value = (from graphicsTexture in textures
					         where (string.Equals(graphicsTexture.Name, textureName, StringComparison.OrdinalIgnoreCase)) &&
					               (graphicsTexture.Settings.Format == settings.Format) &&
					               (graphicsTexture.Settings.Height == settings.Height) &&
					               (graphicsTexture.Settings.Width == settings.Width)
					         select graphicsTexture).FirstOrDefault() ?? (from graphicsTexture in textures
					                                                      where (string.Equals(graphicsTexture.Name, textureName, StringComparison.OrdinalIgnoreCase))
					                                                      select graphicsTexture).FirstOrDefault();
				}
				// ReSharper restore ConvertIfStatementToNullCoalescingExpression
			}

			// We have our texture, stop deferring.
			if (Value == null)
			{
				return false;
			}

			_textureName = string.Empty;
			return true;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonKeyTexture2D" /> struct.
		/// </summary>
		/// <param name="time">The time for the key frame.</param>
		/// <param name="value">The value to apply to the key frame.</param>
		/// <param name="region">Region on the texture to update.</param>
		public GorgonKeyTexture2D(float time, GorgonTexture2D value, RectangleF region)
		{
			Time = time;
			_dataType = typeof(GorgonTexture2D);
			Value = value;
			TextureRegion = region;
			if (value != null)
			{
				_textureName = value.Name;
				_settings = value.Settings;
			}
			else
			{
				// We didn't specify a texture, so don't defer it for later.
				_textureName = string.Empty;
				_settings = null;
			}
		}
		#endregion

		#region IKeyFrame Members
		/// <summary>
		/// Property to set or return the time at which the key frame is stored.
		/// </summary>
		float IKeyFrame.Time
		{
			get
			{
				return Time;
			}
		}

		/// <summary>
		/// Property to return the type of data for this key frame.
		/// </summary>
		public Type DataType
		{
			get 
			{
				return _dataType;
			}
		}		

		/// <summary>
		/// Function to clone the key.
		/// </summary>
		/// <returns>The cloned key.</returns>
		public IKeyFrame Clone()
		{
			return new GorgonKeyTexture2D(Time, Value, TextureRegion);
		}

        /// <summary>
        /// Function to read a keyframe from a chunk.
        /// </summary>
        /// <param name="chunk">Chunk to read from.</param>
		void IKeyFrame.FromChunk(GorgonBinaryReader chunk)
		{
            Value = null;
			Time = chunk.ReadSingle();
			bool hasTexture = chunk.ReadBoolean();

			// Get the texture region we're using.
	        TextureRegion = new RectangleF(chunk.ReadSingle(), chunk.ReadSingle(), chunk.ReadSingle(), chunk.ReadSingle());

	        if (!hasTexture)
	        {
		        return;
	        }

	        // Get our texture name.
	        _textureName = chunk.ReadString();

	        // Read in our texture information.
	        _settings = new GorgonTexture2DSettings
	        {
		        ArrayCount = chunk.ReadInt32(),
		        Format = chunk.ReadValue<BufferFormat>(),
		        Size = new Size(chunk.ReadInt32(), chunk.ReadInt32()),
		        IsTextureCube = chunk.ReadBoolean(),
		        MipCount = chunk.ReadInt32(),
		        Multisampling = new GorgonMultisampling(chunk.ReadInt32(), chunk.ReadInt32()),
		        Usage = BufferUsage.Default,
		        ShaderViewFormat = BufferFormat.Unknown
	        };

	        // Defer load the texture.
	        GetTexture();
		}

		/// <summary>
		/// Function to send the key frame data to the data chunk.
		/// </summary>
		/// <param name="chunk">Chunk to write.</param>
		void IKeyFrame.ToChunk(GorgonBinaryWriter chunk)
		{
			chunk.Write(Time);
			chunk.Write(Value != null);
			chunk.Write(TextureRegion.Left);
			chunk.Write(TextureRegion.Top);
			chunk.Write(TextureRegion.Width);
			chunk.Write(TextureRegion.Height);

			if (Value == null)
			{
				return;
			}

			chunk.Write(Value.Name);
			chunk.Write(Value.Settings.ArrayCount);
			chunk.WriteValue(Value.Settings.Format);
			chunk.Write(Value.Settings.Size.Width);
			chunk.Write(Value.Settings.Size.Height);
			chunk.Write(Value.Settings.IsTextureCube);
			chunk.Write(Value.Settings.MipCount);
			chunk.Write(Value.Settings.Multisampling.Count);
			chunk.Write(Value.Settings.Multisampling.Quality);
		}
		#endregion
	}
}
