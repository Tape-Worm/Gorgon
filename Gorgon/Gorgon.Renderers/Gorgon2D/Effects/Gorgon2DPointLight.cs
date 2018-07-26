#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: July 25, 2018 8:30:32 PM
// 
#endregion

using Gorgon.Graphics;
using DX = SharpDX;

namespace Gorgon.Renderers
{
    /// <summary>
    /// Defines the properties for a 2D point light.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This light type is for use with the <see cref="Gorgon2DDeferredLightingEffect"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="Gorgon2DDeferredLightingEffect"/>
	public class Gorgon2DPointLight
	{
		#region Properties.
		/// <summary>
		/// Property to set or return the direction of the light for directional lighting.
		/// </summary>
		public DX.Vector3 LightDirection
		{
			get;
		    set;
		}

		/// <summary>
		/// Property to set or return the position for the light.
		/// </summary>
		public DX.Vector3 Position
		{
			get;
		    set;
		}

		/// <summary>
		/// Property to set or return the color of the light.
		/// </summary>
		public GorgonColor Color
		{
			get;
		    set;
		}

		/// <summary>
		/// Property to set or return whether to enable specular hilighting.
		/// </summary>
		public bool SpecularEnabled
		{
			get;
		    set;
		}

		/// <summary>
		/// Property to set or return the intensity of the specular hilight.
		/// </summary>
		public float SpecularPower
		{
			get;
		    set;
		}

		/// <summary>
		/// Property to set or return the intensity/falloff for the light.
		/// </summary>
		public float Attenuation
		{
			get;
		    set;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Gorgon2DPointLight"/> class.
		/// </summary>
		public Gorgon2DPointLight()
		{
		    Color = GorgonColor.White;
		    Position = DX.Vector3.Zero;
            LightDirection = DX.Vector3.Zero;
		    Attenuation = 1.0f;
		    SpecularEnabled = false;
		    SpecularPower = 0.0f;
		}
		#endregion
	}
}
