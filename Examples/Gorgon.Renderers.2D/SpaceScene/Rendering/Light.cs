﻿
// 
// Gorgon
// Copyright (C) 2019 Michael Winsor
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
// Created: May 23, 2019 11:13:17 PM
// 

using System.Numerics;
using Gorgon.Renderers.Lights;

namespace Gorgon.Examples;

/// <summary>
/// Represents a light source in the application
/// </summary>
/// <remarks>
/// We wrap up the light object so we can filter out which layer gets lit.  Lights are applied to a scene, so whether a layer or layers get lit is up to the developer
/// </remarks>
internal class Light
{
    /// <summary>
    /// Property to return the list of layers that this light is applied to.
    /// </summary>
    public HashSet<Layer> Layers
    {
        get;
    } = [];

    /// <summary>
    /// Property to return the point light wrapped by this object.
    /// </summary>
    public GorgonPointLight PointLight
    {
        get;
    }

    /// <summary>
    /// Property to return the directional light wrapped by this object.
    /// </summary>
    public GorgonDirectionalLight DirectionalLight
    {
        get;
    }

    /// <summary>
    /// Property to return the common light data.
    /// </summary>
    public GorgonLightCommon LightData
    {
        get;
    }

    /// <summary>
    /// Property to set or return the local position for the light before transformation in layer space.
    /// </summary>
    public Vector3 LocalLightPosition
    {
        get;
        set;
    }

    /// <summary>Initializes a new instance of the <see cref="Light" /> class.</summary>
    /// <param name="pointLight">The point light to wrap.</param>
    public Light(GorgonPointLight pointLight) => LightData = PointLight = pointLight;

    /// <summary>Initializes a new instance of the <see cref="Light" /> class.</summary>
    /// <param name="dirLight">The directional light to wrap.</param>
    public Light(GorgonDirectionalLight dirLight) => LightData = DirectionalLight = dirLight;
}
