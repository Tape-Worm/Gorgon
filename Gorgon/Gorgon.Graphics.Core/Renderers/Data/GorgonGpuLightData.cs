
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: January 15, 2021 10:00:25 PM
// 


using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Gorgon.Graphics;
using Gorgon.Renderers.Lights;

namespace Gorgon.Renderers.Data;

/// <summary>
/// Light data to pass to a GPU buffer
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct GorgonGpuLightData
{
    /// <summary>
    /// An empty light data structure.
    /// </summary>
    public static readonly GorgonGpuLightData Empty = new();

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
    /// The X value represents the constant attenuation value, the Y value represents the linear attenuation value, the Z value represents the quadratic attenuation value, and W represents the range of the light.
    /// </remarks>
    public readonly Vector4 LightAttenuation;

    /// <summary>
    /// Extra attributes for the light.
    /// </summary>
    /// <remarks>
    /// X = Specular Power, Y = Intensity, Z = specular intensity, W = Specular on/off
    /// </remarks>
    public readonly Vector4 LightAttributes;

    /// <summary>
    /// The color for the light.
    /// </summary>
    public readonly GorgonColor LightColor;

    /// <summary>Initializes a new instance of the <see cref="GorgonGpuLightData" /> struct.</summary>
    /// <param name="lightType">Type of the light.</param>
    /// <param name="positionDirection">The position or direction for the light, depending on the type.</param>
    /// <param name="attenuation">The attenuation values for a point light.</param>
    /// <param name="color">The color of the light.</param>
    /// <param name="attributes">The attributes for the light.</param>
    internal GorgonGpuLightData(LightType lightType, Vector3 positionDirection, Vector4 attenuation, GorgonColor color, Vector4 attributes)
    {
        PositionDirection = new Vector4(positionDirection.X, positionDirection.Y, positionDirection.Z, (int)lightType);
        LightAttenuation = attenuation;
        LightColor = color;
        LightAttributes = attributes;
    }
}
