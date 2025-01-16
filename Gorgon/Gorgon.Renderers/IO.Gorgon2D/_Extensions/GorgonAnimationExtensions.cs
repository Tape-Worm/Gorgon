
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: August 25, 2018 10:21:07 PM
// 

using System.Text.Json;
using Gorgon.Animation;

namespace Gorgon.IO;

/// <summary>
/// Extension methods for IO on animation objects
/// </summary>
public static class GorgonAnimationExtensions
{
    /// <summary>
    /// Function to convert a <see cref="IGorgonAnimation"/> to a JSON string.
    /// </summary>
    /// <param name="animation">The animation to convert.</param>
    /// <param name="prettyFormat">[Optional] <b>true</b> to use pretty formatting for the string, or <b>false</b> to use a compact form.</param>
    /// <returns>The animation encoded as a JSON string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="animation"/> parameter is <b>null</b>.</exception>
    /// <seealso cref="IGorgonAnimation"/>
    public static string ToJson(this IGorgonAnimation animation, bool prettyFormat = false)
    {
        if (animation is null)
        {
            throw new ArgumentNullException(nameof(animation));
        }

        JsonSerializerOptions options = new()
        {
            WriteIndented = prettyFormat,
            Converters =
            {
                new JsonAnimationConverter(null, null, null)
            }
        };

        return JsonSerializer.Serialize(animation, options);
    }
}
