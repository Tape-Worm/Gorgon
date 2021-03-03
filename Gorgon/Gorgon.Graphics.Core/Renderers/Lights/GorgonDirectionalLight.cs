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
using Gorgon.Core;
using Gorgon.Math;
using Gorgon.Renderers.Data;
using DX = SharpDX;

namespace Gorgon.Renderers.Lights
{
    /// <summary>
    /// Directional light properties for passing to a GPU lighting shader.
    /// </summary>
	public sealed class GorgonDirectionalLight
        : GorgonLightCommon, IGorgonNamedObject, IEquatable<GorgonDirectionalLight>
    {
        #region Variables.
        // The direction for a directional light.
        private DX.Vector3 _lightDirection;
        // The GPU data for the light.
        private GorgonGpuLightData _lightData;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the type of light to render.
        /// </summary>
        public override LightType LightType => LightType.Directional;

        /// <summary>
        /// Property to set or return the direction of the light for directional lighting.
        /// </summary>
        /// <remarks>
        /// This property is ignored when the <see cref="LightType"/> property is set to <see cref="LightType.Point"/>.
        /// </remarks>
        public DX.Vector3 LightDirection
        {
            get => _lightDirection;
            set
            {
                if (_lightDirection.Equals(value))
                {
                    return;
                }

                _lightDirection = value;
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
        public bool Equals(GorgonDirectionalLight other) => (other != null) && (LightType == other.LightType)
                && (Intensity.EqualsEpsilon(other.Intensity))
                && (SpecularPower.EqualsEpsilon(other.SpecularPower))
                && (SpecularEnabled == other.SpecularEnabled)
                && (Color.Equals(other.Color))
                && (LightDirection.Equals(other.LightDirection));

        /// <summary>
        /// Function to return data that can be updated to the GPU for use in shaders.
        /// </summary>
        /// <returns>A reference to the data to send to the GPU.</returns>
        public override ref readonly GorgonGpuLightData GetGpuData()
        {
            if (IsUpdated)
            {
                _lightData = new GorgonGpuLightData(DX.Vector3.Zero, LightType, LightDirection, Color, SpecularEnabled, SpecularPower, Intensity, 0);
                IsUpdated = false;
            }

            return ref _lightData;
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonDirectionalLight"/> class.
        /// </summary>
        /// <param name="name">[Optional] The name of the light.</param>
        public GorgonDirectionalLight(string name = null)
            : base(name)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="GorgonDirectionalLight"/> class.</summary>
        /// <param name="copy">The light data to copy.</param>
        /// <param name="newName">[Optional] The new name for the light.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="copy"/> parameter is <strong>null</strong>.</exception>
        public GorgonDirectionalLight(GorgonDirectionalLight copy, string newName = null)
            : base(copy, newName)
        {
            if (copy is null)
            {
                throw new ArgumentNullException(nameof(copy));
            }

            LightDirection = copy.LightDirection;
        }
        #endregion
    }
}
