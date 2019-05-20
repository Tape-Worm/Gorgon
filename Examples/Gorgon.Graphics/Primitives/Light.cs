#region MIT.
// 
// Gorgon.
// Copyright (C) 2014 Michael Winsor
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
// Created: Thursday, August 7, 2014 11:43:55 PM
// 
#endregion

using System;
using Gorgon.Graphics;
using Gorgon.Math;
using DX = SharpDX;

namespace Gorgon.Examples
{
	/// <summary>
	/// A light to shine on our sad primitives
	/// </summary>
	internal class Light
        : IEquatable<Light>
    {
        #region Variables.
        // The diffuse color of the light.
        private GorgonColor _lightColor;
        // The specular hilight color of the light.
        private GorgonColor _specularColor;
        // The specular power of the light.
        private float _specularPower;
        // The attentuation falloff for the light.
        private float _attenuation;
        // The position of the light.
        private DX.Vector3 _position;
		// The intensity of the light.
        private float _intensity;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return whether the state of the light has changed.
        /// </summary>
        public bool IsDirty
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the position of the light.
        /// </summary>
        public DX.Vector3 LightPosition
        {
            get => _position;
            set
            {
                if (_position.Equals(ref value))
                {
                    return;
                }

                _position = value;
                IsDirty = true;
            }
        }
        
        /// <summary>
        /// Property to set or return the diffuse color of the light.
        /// </summary>
        public GorgonColor LightColor
        {
            get => _lightColor;
            set
            {
                if (GorgonColor.Equals(in value, in _lightColor))
                {
                    return;
                }

                _lightColor = value;
                IsDirty = true;
            }
        }

        /// <summary>
        /// Property to set or return the specular color of the light.
        /// </summary>
        public GorgonColor SpecularColor
        {
            get => _specularColor;
            set
            {
                if (GorgonColor.Equals(in value, in _specularColor))
                {
                    return;
                }

                _specularColor = value;
                IsDirty = true;
            }
        }

        /// <summary>
        /// Property to set or return the attenuation falloff for the light.
        /// </summary>
        public float Attenuation
        {
            get => _attenuation;
            set
            {
                if (_attenuation.EqualsEpsilon(value))
                {
                    return;
                }

                _attenuation = value;
                IsDirty = true;
            }
        }

        /// <summary>
        /// Property to set or return the specular power for the light.
        /// </summary>
        public float SpecularPower
        {
            get => _specularPower;
            set
            {
                if (_specularPower.EqualsEpsilon(value))
                {
                    return;
                }

                _specularPower = value;
                IsDirty = true;
            }
        }

		/// <summary>
        /// Property to set or return the intensity of the light.
        /// </summary>
		public float Intensity
        {
            get => _intensity;
            set
            {
                if (_intensity.EqualsEpsilon(value))
                {
                    return;
                }

                _intensity = value;
                IsDirty = true;
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns><see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
        public bool Equals(Light other)
        {
            if (other == null)
            {
                return false;
            }

            if (other == this)
            {
                return true;
            }

            return ((_attenuation.EqualsEpsilon(other._attenuation))
                    && (_specularPower.EqualsEpsilon(other._specularPower))
                    && (_lightColor.Equals(in other._lightColor))
                    && (_specularColor.Equals(in other._specularColor))
                    && (_position.Equals(ref other._position))
					&& (_intensity.EqualsEpsilon(other._intensity)));
        }
        #endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Camera"/> class.
		/// </summary>
		public Light()
		{
		    _position = DX.Vector3.Zero;
		    _attenuation = 6.0f;
		    _lightColor = GorgonColor.White;
		    _specularColor = GorgonColor.White;
		    _specularPower = 512.0f;
            _intensity = 1.0f;
		}
        #endregion
    }
}
