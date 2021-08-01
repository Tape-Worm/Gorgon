﻿#region MIT
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
// Created: January 15, 2021 10:00:25 PM
// 
#endregion

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Gorgon.Graphics;
using Gorgon.Renderers.Lights;

namespace Gorgon.Renderers.Data
{
    /// <summary>
    /// Light data to pass to a GPU buffer.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct GorgonGpuLightData
    {
        /// <summary>
        /// The number of bytes for this value type.
        /// </summary>
        public static readonly int SizeInBytes = Unsafe.SizeOf<GorgonGpuLightData>();

        /// <summary>
        /// The position or direction of the light, depending on type.
        /// </summary>
        /// <remarks>
        /// XYZ = Position/Direction, W = Light type.
        /// </remarks>
        public readonly Vector4 PositionDirection;

        /// <summary>
        /// The attenuation values for a point light, these are all 0 for directional lights.
        /// </summary>
        /// <remarks>
        /// The X value represents the constant attenuation value, the Y value represents the linear attenuation value, and the Z value represents the quadratic attenuation value.
        /// </remarks>
        public readonly Vector4 LightAttenuation;

        /// <summary>
        /// Extra attributes for the light.
        /// </summary>
        /// <remarks>
        /// X = Specular Power, Y = Intensity, Z = light range (point light only), W = Specular on/off
        /// </remarks>
        public readonly Vector4 LightAttributes;

        /// <summary>
        /// The color for the light.
        /// </summary>
        public readonly GorgonColor LightColor;

        /// <summary>Initializes a new instance of the <see cref="GorgonGpuLightData" /> struct.</summary>
        /// <param name="positionDirection">The position or direction for the light, depending on the type.</param>
        /// <param name="attenuation">The attenuation values for a point light.</param>
        /// <param name="lightType">Type of the light.</param>
        /// <param name="color">The color of the light.</param>
        /// <param name="range">The range of the light.</param>
        /// <param name="specularEnabled">if set to <c>true</c> [specular enabled].</param>
        /// <param name="specularPower">The specular power.</param>
        /// <param name="intensity">The intensity.</param>
        internal GorgonGpuLightData(Vector3 positionDirection, Vector3 attenuation, LightType lightType, GorgonColor color, float range, bool specularEnabled, float specularPower, float intensity)
        {
            PositionDirection = new Vector4(positionDirection.X, positionDirection.Y, positionDirection.Z, (int)lightType);
            LightAttenuation = new Vector4(attenuation, 0);
            LightColor = color;
            LightAttributes = new Vector4(specularPower, intensity, range, specularEnabled ? 1 : 0);
        }
    }
}
