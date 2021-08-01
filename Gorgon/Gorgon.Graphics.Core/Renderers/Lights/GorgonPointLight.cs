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
        private float _attenuationA = 1.0f;
        private float _attenuationB = 0.0f;
        private float _attenuationC = 0.0f;
        // The range for the light.
        private float _range = float.MaxValue.Sqrt();
        // The GPU data for the light.
        private GorgonGpuLightData _lightData;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the type of light to render.
        /// </summary>
        public override LightType LightType => LightType.Point;

        /// <summary>
        /// Property to set or return the range for the point light.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The range is the area that is affected by the light. If an object vertex minus the position of the light is greater than the range value, it will not be lit.
        /// </para>
        /// <para>
        /// The default value is the square root of the floating point <see cref="float.MaxValue"/>.
        /// </para>
        /// </remarks>
        public float Range
        {
            get => _range;
            set
            {
                value = value.Max(1e-06f);
                if (_range.EqualsEpsilon(value))
                {
                    return;
                }

                _range = value;
                IsUpdated = true;
            }
        }

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
        /// Property to set or return the constant intensity falloff for the light.
        /// </summary>
        public float ConstantAttenuation
        {
            get => _attenuationA;
            set
            {
                if (_attenuationA.EqualsEpsilon(value))
                {
                    return;
                }

                _attenuationA = value;
                IsUpdated = true;
            }
        }

        /// <summary>
        /// Property to set or return the linear intensity falloff for the light.
        /// </summary>
        public float LinearAttenuation
        {
            get => _attenuationB;
            set
            {
                if (_attenuationB.EqualsEpsilon(value))
                {
                    return;
                }

                _attenuationB = value;
                IsUpdated = true;
            }
        }

        /// <summary>
        /// Property to set or return the quadratic intensity falloff for the light.
        /// </summary>
        public float QuadraticAttenuation
        {
            get => _attenuationC;
            set
            {
                if (_attenuationC.EqualsEpsilon(value))
                {
                    return;
                }

                _attenuationC = value;
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
        public bool Equals(GorgonPointLight other) => (other is not null) && (LightType == other.LightType)
                && (_attenuationA.EqualsEpsilon(other._attenuationA))
                && (_attenuationB.EqualsEpsilon(other._attenuationB))
                && (_attenuationC.EqualsEpsilon(other._attenuationC))
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
                _lightData = new GorgonGpuLightData(new Vector3(Position.X, Position.Y, -Position.Z),
                                                    new Vector3(_attenuationA, _attenuationB, _attenuationC),
                                                    LightType, 
                                                    Color, 
                                                    _range,
                                                    SpecularEnabled, 
                                                    SpecularPower, 
                                                    Intensity);
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
            if (copy is null)
            {
                throw new ArgumentNullException(nameof(copy));
            }

            Position = copy.Position;
            _attenuationA = copy._attenuationA;
            _attenuationB = copy._attenuationB;
            _attenuationC = copy._attenuationC;
        }
        #endregion
    }
}
