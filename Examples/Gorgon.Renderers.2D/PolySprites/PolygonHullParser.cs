
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
// Created: August 10, 2018 9:35:21 PM
// 


using System.Globalization;
using System.Numerics;
using Gorgon.Graphics;
using Gorgon.Renderers;

namespace Gorgon.Examples;

/// <summary>
/// Provides functionality to parse a string containing polygon hull coordinates
/// </summary>
public static class PolygonHullParser
{
    /// <summary>
    /// Function to parse the string containing the hull data.
    /// </summary>
    /// <param name="data">The string containing the hull data.</param>
    /// <param name="builder">The polygonal sprite builder used to create the sprite.</param>
    private static void ParseString(string data, GorgonPolySpriteBuilder builder)
    {
        string[] lines = data.Split(new[]
                                    {
                                        "\n"
                                    },
                                    StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < lines.Length; ++i)
        {
            string line = lines[i].Replace("\r", string.Empty).Trim();

            string[] components = line.Split(new[]
                                             {
                                                 ','
                                             },
                                             StringSplitOptions.None);

            // If this line lacks at least 4 components, then we cannot use it.
            if (components.Length != 4)
            {
                continue;
            }

            float.TryParse(components[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float x);
            float.TryParse(components[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float y);
            float.TryParse(components[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float u);
            float.TryParse(components[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float v);

            builder.AddVertex(new GorgonPolySpriteVertex(new Vector2(x, y), GorgonColors.White, new Vector2(u, v)));
        }
    }

    /// <summary>
    /// Function to parse the polygon hull string data.
    /// </summary>
    /// <param name="renderer">The renderer used to generate the polygonal sprite.</param>
    /// <param name="polygonHull">The string containing the polygon hull data.</param>
    /// <returns>The polygon sprite from the polygon hull data.</returns>
    public static GorgonPolySprite ParsePolygonHullString(Gorgon2D renderer, string polygonHull)
    {
        GorgonPolySpriteBuilder builder = new(renderer);

        ParseString(polygonHull, builder);

        return builder.Anchor(new Vector2(0.5f, 0.5f))
                      .Build();
    }
}
