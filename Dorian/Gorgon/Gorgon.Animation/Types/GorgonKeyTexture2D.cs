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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using SlimMath;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Animation
{
	/// <summary>
	/// A key frame that manipulates a 2D texture data type.
	/// </summary>
	public struct GorgonKeyTexture2D
		: IKeyFrame
	{
		#region Variables.
		private Type _dataType;								// Type of data for the key frame.

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
		/// Function to retrieve key frame data from a binary data reader.
		/// </summary>
		/// <param name="reader">Reader used to read the stream.</param>
		void IKeyFrame.FromStream(IO.GorgonBinaryReader reader)
		{
			GorgonTexture2DSettings settings = new GorgonTexture2DSettings();
			string textureName = string.Empty;

			this.Time = reader.ReadSingle();
			this.Value = null;
			this.TextureRegion = RectangleF.Empty;
			if (this.Time > -1)
			{
				textureName = reader.ReadString();
				settings.ArrayCount = reader.ReadInt32();
				settings.Format = (BufferFormat)reader.ReadInt32();
				settings.Height = reader.ReadInt32();
				settings.Width = reader.ReadInt32();
				settings.IsTextureCube = reader.ReadBoolean();
				settings.MipCount = reader.ReadInt32();
				settings.Multisampling = new GorgonMultisampling(reader.ReadInt32(), reader.ReadInt32());
				settings.Usage = BufferUsage.Default;
				settings.ViewFormat = BufferFormat.Unknown;
				TextureRegion = new RectangleF(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

				// This is a special case, but we'll need to find the texture we were associated with.
				// In order to do that, we need to find all graphics objects:
				var graphics = Gorgon.GetTrackedObjectsOfType<GorgonGraphics>();

				// We have no graphics, then we can't do this anyway, so just throw an exception.
				if (graphics.Count == 0)
					throw new ArgumentException("Cannot load a 2D texture key frame without a graphics instance.", "reader");

				// Then, we begin our search by looking at -all- the texture information we have:
				Value = (from graphicsObject in graphics
						 from graphicsTexture in graphicsObject.GetGraphicsObjectOfType<GorgonTexture2D>()
						 where (string.Compare(graphicsTexture.Name, textureName, true) == 0) &&
						  (graphicsTexture.Settings.ArrayCount == settings.ArrayCount) &&
						  (graphicsTexture.Settings.Format == settings.Format) &&
						  (graphicsTexture.Settings.Height == settings.Height) &&
						  (graphicsTexture.Settings.Width == settings.Width) &&
						  (graphicsTexture.Settings.IsTextureCube == settings.IsTextureCube) &&
						  (graphicsTexture.Settings.MipCount == settings.MipCount) &&
						  (graphicsTexture.Settings.Multisampling == settings.Multisampling)
						 select graphicsTexture).FirstOrDefault();

				if (Value == null)
				{
					// That one failed, so just try and look it up by name, width, height and format.
					Value = (from graphicsObject in graphics
							 from graphicsTexture in graphicsObject.GetGraphicsObjectOfType<GorgonTexture2D>()
							 where (string.Compare(graphicsTexture.Name, textureName, true) == 0) &&
							  (graphicsTexture.Settings.Format == settings.Format) &&
							  (graphicsTexture.Settings.Height == settings.Height) &&
							  (graphicsTexture.Settings.Width == settings.Width)
							 select graphicsTexture).FirstOrDefault();

					if (Value == null)
					{
						// OK, that one failed too, just look by name.
						Value = (from graphicsObject in graphics
								 from graphicsTexture in graphicsObject.GetGraphicsObjectOfType<GorgonTexture2D>()
								 where (string.Compare(graphicsTexture.Name, textureName, true) == 0)
								 select graphicsTexture).FirstOrDefault();

						// If we -still- can't get the texture, then we need to throw an exception.
						if (Value == null)
							throw new ArgumentException("Could not located any 2D texture named '" + textureName + "'.", "reader");
					}
				}
			}
			else
				this.Time = -this.Time;
		}

		/// <summary>
		/// Function to send the key frame data to a binary data writer.
		/// </summary>
		/// <param name="writer">Writer used to write to the stream.</param>
		void IKeyFrame.ToStream(IO.GorgonBinaryWriter writer)
		{
			if (Value == null)
			{
				writer.Write(-Time);
				return;
			}

			writer.Write(Time);
			writer.Write(Value.Name);
			writer.Write(Value.Settings.ArrayCount);
			writer.Write((int)Value.Settings.Format);
			writer.Write(Value.Settings.Height);
			writer.Write(Value.Settings.Width);
			writer.Write(Value.Settings.IsTextureCube);
			writer.Write(Value.Settings.MipCount);
			writer.Write(Value.Settings.Multisampling.Count);
			writer.Write(Value.Settings.Multisampling.Quality);
			writer.Write(this.TextureRegion.X);
			writer.Write(this.TextureRegion.Y);
			writer.Write(this.TextureRegion.Width);
			writer.Write(this.TextureRegion.Height);
		}
		#endregion
	}
}
