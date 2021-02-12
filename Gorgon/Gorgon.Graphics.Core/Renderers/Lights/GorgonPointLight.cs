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
using System.Numerics;
using Gorgon.Core;
using Gorgon.Math;
using Gorgon.Renderers.Data;

namespace Gorgon.Renderers.Lights
{
    /// <summary>
    /// Point light properties for passing to a GPU lighting shader.
    /// </summary>
	public sealed class GorgonPointLight
        : GorgonLightCommon, IGorgonNamedObject, IEquatable<GorgonPointLight>
    {
        #region Variables.
        // The position for a point light.
        private Vector3 _position;
        // The attenuation of a point light.
        private float _attenuation = float.MaxValue.Sqrt();
        // The GPU data for the light.
        private GorgonGpuLightData _lightData;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the type of light to render.
        /// </summary>
        public override LightType LightType => LightType.Point;

        /// <summary>
        /// Property to set or return the position for the light.
        /// </summary>
        public Vector3 Position
        {
            get => _position;
            set
            {
                if (_position.Equals(value))
                {
                    return;
                }

                _position = value;
                IsUpdated = true;
            }
        }

        /// <summary>
        /// Property to set or return the intensity/falloff for the light.
        /// </summary>
        /// <remarks>
        /// This property does not apply if the <see cref="LightType"/> property is set to <see cref="LightType.Directional"/>.
        /// </remarks>
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
                IsUpdated = true;
            }
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
        public bool Equals(GorgonPointLight other) => (other != null) && (LightType == other.LightType)
                && (Attenuation.EqualsEpsilon(other.Attenuation))
                && (Intensity.EqualsEpsilon(other.Intensity))
                && (SpecularPower.EqualsEpsilon(other.SpecularPower))
                && (SpecularEnabled == other.SpecularEnabled)
                && (Color.Equals(other.Color))
                && (Position.Equals(other.Position));

        /// <summary>
        /// Function to return data that can be updated to the GPU for use in shaders.
        /// </summary>
        /// <returns>A reference to the data to send to the GPU.</returns>
        public override ref readonly GorgonGpuLightData GetGpuData()
        {
            if (IsUpdated)
            {
                _lightData = new GorgonGpuLightData(Position, LightType, Vector3.Zero, Color, SpecularEnabled, SpecularPower, Intensity, Attenuation);
                IsUpdated = false;
            }

            return ref _lightData;
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonPointLight"/> class.
        /// </summary>
        /// <param name="name">[Optional] The name of the light.</param>
        public GorgonPointLight(string name = null)
            : base(name)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="GorgonPointLight"/> class.</summary>
        /// <param name="copy">The light data to copy.</param>
        /// <param name="newName">[Optional] The new name for the light.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="copy"/> parameter is <strong>null</strong>.</exception>
        public GorgonPointLight(GorgonPointLight copy, string newName = null)
            : base(copy, newName)
        {
            if (copy == null)
            {
                throw new ArgumentNullException(nameof(copy));
            }

            Position = copy.Position;
            Attenuation = copy.Attenuation;
        }
        #endregion
    }
}
