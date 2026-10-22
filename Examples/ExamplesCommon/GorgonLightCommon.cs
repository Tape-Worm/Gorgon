
// 
// Gorgon
// Copyright (C) 2025 Michael Winsor
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
// Created: July 25, 2018 8:30:32 PM
// 

using System;
using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Math;

namespace Gorgon.Examples;

/// <summary>
/// Defines the type of light used when rendering
/// </summary>
public enum LightType
{
    /// <summary>
    /// Light is disabled.
    /// </summary>
    Disabled = 0,
    /// <summary>
    /// A point light.
    /// </summary>
    Point = 1,
    /// <summary>
    /// A directional light.
    /// </summary>
    Directional = 2
}

/// <summary>
/// Base common properties for a light
/// </summary>
public abstract class GorgonLightCommon
: IGorgonNamedObject
{

    // The color for a light.

    // Flag to indicate whether the specular reflection is enabled.

    // The specular power.

    // The specular intensity.

    // The intensity of the light.

    /// <summary>
    /// Property to return whether the light was updated or not.
    /// </summary>
    public bool IsUpdated
    {
        get;
        protected set;
    } = true;

    /// <summary>
    /// Property to set or return the type of light to render.
    /// </summary>
    public abstract LightType LightType
    {
        get;
    }

    /// <summary>
    /// Property to set or return the color of the light.
    /// </summary>
    public GorgonColor Color
    {
        get;
        set
        {
            if (field.Equals(value))
            {
                return;
            }

            field = value;
            IsUpdated = true;
        }
    } = GorgonColors.White;

    /// <summary>
    /// Property to set or return whether to enable specular highlights.
    /// </summary>
    public bool SpecularEnabled
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            IsUpdated = true;
        }
    }

    /// <summary>
    /// Property to set or return the intensity of the specular highlight.
    /// </summary>
    public float SpecularPower
    {
        get;
        set
        {
            if (field.EqualsEpsilon(value))
            {
                return;
            }

            field = value;
            IsUpdated = true;
        }
    } = 64.0f;

    /// <summary>
    /// Property to set or return the intensity of the specular highlight.
    /// </summary>
    public float SpecularIntensity
    {
        get;
        set
        {
            if (field.EqualsEpsilon(value))
            {
                return;
            }

            field = value;
            IsUpdated = true;
        }
    } = 1.0f;

    /// <summary>
    /// Property to set or return how bright the light will be.
    /// </summary>
    public float Intensity
    {
        get;
        set
        {
            if (field.EqualsEpsilon(value))
            {
                return;
            }

            field = value;
            IsUpdated = true;
        }
    } = 1.0f;

    /// <summary>
    /// Property to return the name of the light.
    /// </summary>
    public string Name
    {
        get;
    }

    /// <summary>
    /// Function to return data that can be updated to the GPU for use in shaders.
    /// </summary>
    /// <returns>A reference to the data to send to the GPU.</returns>
    public abstract ref readonly GorgonGpuLightData GetGpuData();

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonLightCommon"/> class.
    /// </summary>
    /// <param name="name">[Optional] The name of the light.</param>
    protected GorgonLightCommon(string? name = null) => Name = string.IsNullOrWhiteSpace(name) ? $"{GetType().Name}_{Guid.NewGuid():N}" : name;

    /// <summary>Initializes a new instance of the <see cref="GorgonLightCommon"/> class.</summary>
    /// <param name="copy">The light data to copy.</param>
    /// <param name="newName">[Optional] The new name for the light.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="copy"/> parameter is <strong>null</strong>.</exception>
    protected GorgonLightCommon(GorgonLightCommon copy, string? newName = null)
    {
        if (copy is null)
        {
            throw new ArgumentNullException(nameof(copy));
        }

        Name = string.IsNullOrWhiteSpace(newName) ? copy.Name : newName;
        Color = copy.Color;
        SpecularEnabled = copy.SpecularEnabled;
        SpecularPower = copy.SpecularPower;
        Intensity = copy.Intensity;
    }
}
