﻿
// 
// Gorgon
// Copyright (C) 2020 Michael Winsor
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
// Created: June 4, 2020 3:06:50 PM
// 

using Gorgon.Graphics.Core.Properties;
using D3D = SharpDX.Direct3D;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Extension methods for the <see cref="FeatureSet"/> enumeration
/// </summary>
public static class GorgonFeatureSetExtensions
{
    /// <summary>
    /// Function to convert the feature set value to a corresponding Direct3D version number.
    /// </summary>
    /// <param name="featureSet">The feature set to evaluate.</param>
    /// <returns>The Direct3D version number.</returns>        
    internal static string D3DVersion(this FeatureSet featureSet) => featureSet switch
    {
        FeatureSet.Level_12_1 or FeatureSet.Level_12_0 => "11.4",
        FeatureSet.Level_11_2 => "11.2",
        _ => Resources.GORGFX_TEXT_FEATURE_SET_UNKNOWN,
    };

    /// <summary>
    /// Function to convert the feature set value to a corresponding Direct3D version number.
    /// </summary>
    /// <param name="featureLevel">The feature set to evaluate.</param>
    /// <returns>The Direct3D version number.</returns>        
    internal static string D3DVersion(this D3D.FeatureLevel featureLevel) => featureLevel switch
    {
        D3D.FeatureLevel.Level_12_1 or D3D.FeatureLevel.Level_12_0 => "11.4",
        D3D.FeatureLevel.Level_11_1 => "11.2",
        _ => Resources.GORGFX_TEXT_FEATURE_SET_UNKNOWN,
    };

    /// <summary>
    /// Function to return a friendly description for a feature set.
    /// </summary>
    /// <param name="featureSet">The feature set to evaluate.</param>
    /// <returns>The friendly description.</returns>
    public static string Description(this FeatureSet featureSet) => featureSet switch
    {
        FeatureSet.Level_12_1 => Resources.GORGFX_TEXT_FEATURE_SET_121,
        FeatureSet.Level_12_0 => Resources.GORGFX_TEXT_FEATURE_SET_120,
        FeatureSet.Level_11_2 => Resources.GORGFX_TEXT_FEATURE_SET_112,
        _ => Resources.GORGFX_TEXT_FEATURE_SET_UNKNOWN,
    };
}
