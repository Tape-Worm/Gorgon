#region MIT
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

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Gorgon.Graphics;
using Gorgon.Renderers.Lights;
using DX = SharpDX;

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
        /// The position of the light.
        /// </summary>
        /// <remarks>
        /// XYZ = Position, W = Light type.
        /// </remarks>
        public readonly DX.Vector4 Position;

        /// <summary>
        /// The direction of a directonal light.
        /// </summary>
        public readonly DX.Vector4 LightDirection;

        /// <summary>
        /// Extra attributes for the light.
        /// </summary>
        /// <remarks>
        /// X = Specular Power, Y = Intensity, Z = Attenuation (point light only), W = Specular on/off
        /// </remarks>
        public readonly DX.Vector4 LightAttributes;

        /// <summary>
        /// The color for the light.
        /// </summary>
        public readonly GorgonColor LightColor;

        /// <summary>Initializes a new instance of the <see cref="GorgonGpuLightData" /> struct.</summary>
        /// <param name="position">The position.</param>
        /// <param name="lightType">Type of the light.</param>
        /// <param name="direction">The direction.</param>
        /// <param name="color">The color of the light.</param>
        /// <param name="specularEnabled">if set to <c>true</c> [specular enabled].</param>
        /// <param name="specularPower">The specular power.</param>
        /// <param name="intensity">The intensity.</param>
        /// <param name="attenuation">The attenuation.</param>
        internal GorgonGpuLightData(DX.Vector3 position, LightType lightType, DX.Vector3 direction, GorgonColor color, bool specularEnabled, float specularPower, float intensity, float attenuation)
        {
            Position = new DX.Vector4(position.X, position.Y, -position.Z, (int)lightType);
            LightDirection = new DX.Vector4(direction, 0);
            LightColor = color;
            LightAttributes = new DX.Vector4(specularPower, intensity, attenuation, specularEnabled ? 1 : 0);
        }
    }
}
