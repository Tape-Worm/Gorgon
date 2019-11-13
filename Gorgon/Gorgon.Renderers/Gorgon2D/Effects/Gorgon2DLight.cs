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

using System;
using Gorgon.Graphics;
using Gorgon.Math;
using DX = SharpDX;

namespace Gorgon.Renderers
{
    /// <summary>
    /// Defines the type of light used when rendering.
    /// </summary>
    public enum LightType
    {
        /// <summary>
        /// A point light.
        /// </summary>
        Point = 0,
        /// <summary>
        /// A directional light.
        /// </summary>
        Directional = 1
    }

    /// <summary>
    /// Defines the properties for a 2D light.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This light type is for use with the <see cref="Gorgon2DLightingEffect"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="Gorgon2DLightingEffect"/>
	public class Gorgon2DLight
        : IEquatable<Gorgon2DLight>
    {
        #region Properties.
        /// <summary>
        /// Property to set or return the type of light to render.
        /// </summary>
        public LightType LightType
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the direction of the light for directional lighting.
        /// </summary>
        /// <remarks>
        /// This property is ignored when the <see cref="LightType"/> property is set to <see cref="Renderers.LightType.Point"/>.
        /// </remarks>
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
        /// Property to set or return how bright the light will be.
        /// </summary>
        public float Intensity
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the intensity/falloff for the light.
        /// </summary>
        /// <remarks>
        /// This property does not apply if the <see cref="LightType"/> property is set to <see cref="LightType.Directional"/>.
        /// </remarks>
        public float Attenuation
        {
            get;
            set;
        }
        #endregion

        #region Methods.
        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <span class="keyword">
        ///     <span class="languageSpecificText">
        ///       <span class="cs">true</span>
        ///       <span class="vb">True</span>
        ///       <span class="cpp">true</span>
        ///     </span>
        ///   </span>
        ///   <span class="nu">
        ///     <span class="keyword">true</span> (<span class="keyword">True</span> in Visual Basic)</span> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <span class="keyword"><span class="languageSpecificText"><span class="cs">false</span><span class="vb">False</span><span class="cpp">false</span></span></span><span class="nu"><span class="keyword">false</span> (<span class="keyword">False</span> in Visual Basic)</span>.
        /// </returns>
        public bool Equals(Gorgon2DLight other)
        {
            if (other == null)
            {
                return false;
            }

            return ((LightType == other.LightType)
                && (Attenuation.EqualsEpsilon(other.Attenuation))
                && (Intensity.EqualsEpsilon(other.Intensity))
                && (SpecularPower.EqualsEpsilon(other.SpecularPower))
                && (SpecularEnabled == other.SpecularEnabled)
                && (Color.Equals(other.Color))
                && (Position.Equals(other.Position))
                && (LightDirection.Equals(other.LightDirection)));
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="Gorgon2DLight"/> class.
        /// </summary>
        public Gorgon2DLight()
        {
            Color = GorgonColor.White;
            Position = DX.Vector3.Zero;
            LightDirection = DX.Vector3.Zero;
            Attenuation = 1.0f;
            SpecularEnabled = false;
            SpecularPower = 0.0f;
            Intensity = 1.0f;
        }

        /// <summary>Initializes a new instance of the <see cref="Gorgon2DLight"/> class.</summary>
        /// <param name="copy">The light data to copy.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="copy"/> parameter is <strong>null</strong>.</exception>
        public Gorgon2DLight(Gorgon2DLight copy)
        {
            if (copy == null)
            {
                throw new ArgumentNullException(nameof(copy));
            }

            Color = copy.Color;
            Position = copy.Position;
            LightDirection = copy.LightDirection;
            Attenuation = copy.Attenuation;
            SpecularEnabled = copy.SpecularEnabled;
            SpecularPower = copy.SpecularPower;
            Intensity = copy.Intensity;
        }
        #endregion
    }
}
