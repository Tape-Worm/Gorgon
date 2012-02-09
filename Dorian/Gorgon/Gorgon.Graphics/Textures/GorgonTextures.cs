using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Textures interface.
	/// </summary>
	public sealed class GorgonTextures
	{
		#region Variables.
		private GorgonGraphics _graphics = null;
		#endregion

		#region Properties.

		#endregion

		#region Methods.
		/// <summary>
		/// Function to create a new 2D texture.
		/// </summary>
		/// <param name="name">Name of the texture.</param>
		/// <param name="settings">Settings for the texture.</param>
		/// <param name="data">Data used to initialize the texture.</param>
		/// <returns>A new 2D texture.</returns>
		public GorgonTexture2D CreateTexture(string name, GorgonTexture2DSettings settings, GorgonTexture2DData? data)
		{
			GorgonTexture2D texture = null;

			GorgonDebug.AssertParamString(name, "name");

			texture = new GorgonTexture2D(_graphics, name, settings);
			texture.Initialize(data);

			_graphics.TrackedObjects.Add(texture);
			return texture;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTextures"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface.</param>
		internal GorgonTextures(GorgonGraphics graphics)
		{
			_graphics = graphics;
		}
		#endregion
	}
}
