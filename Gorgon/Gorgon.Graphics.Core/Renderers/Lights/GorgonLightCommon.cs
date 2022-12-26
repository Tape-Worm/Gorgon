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
using Gorgon.Graphics;
using Gorgon.Math;
using Gorgon.Renderers.Data;

namespace Gorgon.Renderers.Lights;

/// <summary>
/// Defines the type of light used when rendering.
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
/// Base common properties for a light.
/// </summary>
	public abstract class GorgonLightCommon
    : IGorgonNamedObject
{
    #region Variables.
    // The color for a light.
    private GorgonColor _color = GorgonColor.White;
    // Flag to indicate whether the specular reflection is enabled.
    private bool _specularEnabled;
    // The specular power.
    private float _specularPower = 64.0f;
    // The specular intensity.
    private float _specIntensity = 1.0f;
    // The intensity of the light.
    private float _intensity = 1.0f;
    #endregion

    #region Properties.
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
        get => _color;
        set
        {
            if (_color.Equals(value))
            {
                return;
            }

            _color = value;
            IsUpdated = true;
        }
    }

    /// <summary>
    /// Property to set or return whether to enable specular highlights.
    /// </summary>
    public bool SpecularEnabled
    {
        get => _specularEnabled;
        set
        {
            if (_specularEnabled == value)
            {
                return;
            }

            _specularEnabled = value;
            IsUpdated = true;
        }
    }

    /// <summary>
    /// Property to set or return the intensity of the specular highlight.
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
            IsUpdated = true;
        }
    }

    /// <summary>
    /// Property to set or return the intensity of the specular highlight.
    /// </summary>
    public float SpecularIntensity
    {
        get => _specIntensity;
        set
        {
            if (_specIntensity.EqualsEpsilon(value))
            {
                return;
            }

            _specIntensity = value;
            IsUpdated = true;
        }
    }

    /// <summary>
    /// Property to set or return how bright the light will be.
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
            IsUpdated = true;
        }
    }

    /// <summary>
    /// Property to return the name of the light.
    /// </summary>
    public string Name
    {
        get;
    }
    #endregion

    #region Methods.
    /// <summary>
    /// Function to return data that can be updated to the GPU for use in shaders.
    /// </summary>
    /// <returns>A reference to the data to send to the GPU.</returns>
    public abstract ref readonly GorgonGpuLightData GetGpuData();
    #endregion

    #region Constructor/Destructor.
    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonLightCommon"/> class.
    /// </summary>
    /// <param name="name">[Optional] The name of the light.</param>
    protected GorgonLightCommon(string name = null) => Name = string.IsNullOrWhiteSpace(name) ? $"{GetType().Name}_{Guid.NewGuid():N}" : name;

    /// <summary>Initializes a new instance of the <see cref="GorgonLightCommon"/> class.</summary>
    /// <param name="copy">The light data to copy.</param>
    /// <param name="newName">[Optional] The new name for the light.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="copy"/> parameter is <strong>null</strong>.</exception>
    protected GorgonLightCommon(GorgonLightCommon copy, string newName = null)
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
    #endregion
}
