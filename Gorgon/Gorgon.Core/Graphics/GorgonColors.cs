// Gorgon.
// Copyright (C) 2024 Michael Winsor
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
// Created: December 9, 2023 9:25:18 PM
//

using System.Drawing;

namespace Gorgon.Graphics;

/// <summary>
/// A list of pre-defined <see cref="GorgonColor"/> values.
/// </summary>
public static class GorgonColors
{
    /// <summary>
    /// A completely transparent color with an alpha of 0, and RGB values of 1.
    /// </summary>
    public static readonly GorgonColor Transparent = new(1, 1, 1, 0);
    /// <summary>
    /// A completely transparent color with an alpha of 0, and RGB values of 0.
    /// </summary>
    public static readonly GorgonColor BlackTransparent = new(0, 0, 0, 0);
    /// <summary>
    /// The color white.
    /// </summary>
    public static readonly GorgonColor White = new(1, 1, 1, 1);
    /// <summary>
    /// The color black.
    /// </summary>
    public static readonly GorgonColor Black = new(0, 0, 0, 1);
    /// <summary>
    /// Pure red (Red = 1, Green = 0, Blue = 0).
    /// </summary>
    public static readonly GorgonColor Red = new(1, 0, 0);
    /// <summary>
    /// Pure green (Red = 0, Green = 1, Blue = 0).
    /// </summary>
    public static readonly GorgonColor Green = new(0, 1, 0);
    /// <summary>
    /// Pure blue (Red = 0, Green = 0, Blue = 1).
    /// </summary>
    public static readonly GorgonColor Blue = new(0, 0, 1);
    /// <summary>
    /// Pure purple (Red = 1, Green = 0, Blue = 1).
    /// </summary>
    public static readonly GorgonColor Purple = new(1, 0, 1);
    /// <summary>
    /// Pure yellow (Red = 1, Green = 1, Blue = 0).
    /// </summary>
    public static readonly GorgonColor Yellow = new(1, 1, 0);
    /// <summary>
    /// Pure cyan (Red = 0, Green = 1, Blue = 1).
    /// </summary>
    public static readonly GorgonColor Cyan = new(0, 1, 1);
    /// <summary>
    /// 90% gray.
    /// </summary>
    public static readonly GorgonColor Gray90 = new(0.9f, 0.9f, 0.9f);
    /// <summary>
    /// 80% gray.
    /// </summary>
    public static readonly GorgonColor Gray80 = new(0.8f, 0.8f, 0.8f);
    /// <summary>
    /// 75% gray.
    /// </summary>
    public static readonly GorgonColor Gray75 = new(0.75f, 0.75f, 0.75f);
    /// <summary>
    /// 70% gray.
    /// </summary>
    public static readonly GorgonColor Gray70 = new(0.7f, 0.7f, 0.7f);
    /// <summary>
    /// 60% gray.
    /// </summary>
    public static readonly GorgonColor Gray60 = new(0.6f, 0.6f, 0.6f);
    /// <summary>
    /// 50% gray.
    /// </summary>
    public static readonly GorgonColor Gray50 = new(0.5f, 0.5f, 0.5f);
    /// <summary>
    /// 40% gray.
    /// </summary>
    public static readonly GorgonColor Gray40 = new(0.4f, 0.4f, 0.4f);
    /// <summary>
    /// 30% gray.
    /// </summary>
    public static readonly GorgonColor Gray30 = new(0.3f, 0.3f, 0.3f);
    /// <summary>
    /// 25% gray.
    /// </summary>
    public static readonly GorgonColor Gray25 = new(0.25f, 0.25f, 0.25f);
    /// <summary>
    /// 20% gray.
    /// </summary>
    public static readonly GorgonColor Gray20 = new(0.2f, 0.2f, 0.2f);
    /// <summary>
    /// 10% gray.
    /// </summary>
    public static readonly GorgonColor Gray10 = new(0.1f, 0.1f, 0.1f);
    /// <summary>
    /// Corn flower blue.
    /// </summary>
    public static readonly GorgonColor CornFlowerBlue = (GorgonColor)Color.CornflowerBlue;
    /// <summary>
    /// Steel blue.
    /// </summary>
    public static readonly GorgonColor SteelBlue = (GorgonColor)Color.SteelBlue;
    /// <summary>
    /// Yellow green.
    /// </summary>
    public static readonly GorgonColor YellowGreen = (GorgonColor)Color.YellowGreen;
    /// <summary>
    /// Saddle brown.
    /// </summary>
    public static readonly GorgonColor SaddleBrown = (GorgonColor)Color.SaddleBrown;
    /// <summary>
    /// Orange.
    /// </summary>
    public static readonly GorgonColor Orange = (GorgonColor)Color.Orange;
    /// <summary>
    /// Aquamarine.
    /// </summary>
    public static readonly GorgonColor Aquamarine = (GorgonColor)Color.Aquamarine;
    /// <summary>
    /// Beige.
    /// </summary>
    public static readonly GorgonColor Beige = (GorgonColor)Color.Beige;
    /// <summary>
    /// BlueViolet.
    /// </summary>
    public static readonly GorgonColor BlueViolet = (GorgonColor)Color.BlueViolet;
    /// <summary>
    /// CadetBlue.
    /// </summary>
    public static readonly GorgonColor CadetBlue = (GorgonColor)Color.CadetBlue;
    /// <summary>
    /// Brown.
    /// </summary>
    public static readonly GorgonColor Brown = (GorgonColor)Color.Brown;
    /// <summary>
    /// Crimson.
    /// </summary>
    public static readonly GorgonColor Crimson = (GorgonColor)Color.Crimson;
    /// <summary>
    /// Chartreuse.
    /// </summary>
    public static readonly GorgonColor Chartreuse = (GorgonColor)Color.Chartreuse;
    /// <summary>
    /// Gold.
    /// </summary>
    public static readonly GorgonColor Gold = (GorgonColor)Color.Gold;
    /// <summary>
    /// Dark cyan.
    /// </summary>
    public static readonly GorgonColor DarkCyan = (GorgonColor)Color.DarkCyan;
    /// <summary>
    /// Dark purple.
    /// </summary>
    public static readonly GorgonColor DarkPurple = (GorgonColor)Color.DarkMagenta;
    /// <summary>
    /// Dark yellow.
    /// </summary>
    public static readonly GorgonColor DarkYellow = new(0.5f, 0.5f, 0);
    /// <summary>
    /// Dark red.
    /// </summary>
    public static readonly GorgonColor DarkRed = (GorgonColor)Color.DarkRed;
    /// <summary>
    /// Dark green.
    /// </summary>
    public static readonly GorgonColor DarkGreen = (GorgonColor)Color.DarkGreen;
    /// <summary>
    /// Dark blue.
    /// </summary>
    public static readonly GorgonColor DarkBlue = (GorgonColor)Color.DarkBlue;
    /// <summary>
    /// Light cyan.
    /// </summary>
    public static readonly GorgonColor LightCyan = (GorgonColor)Color.LightCyan;
    /// <summary>
    /// Light purple.
    /// </summary>
    public static readonly GorgonColor LightPurple = new(1, 0.5f, 1);
    /// <summary>
    /// Light yellow.
    /// </summary>
    public static readonly GorgonColor LightYellow = new(1, 1, 0.5f);
    /// <summary>
    /// Light red.
    /// </summary>
    public static readonly GorgonColor LightRed = new(1, 0.5f, 0.5f);
    /// <summary>
    /// Light green.
    /// </summary>
    public static readonly GorgonColor LightGreen = new(0.5f, 1.0f, 0.5f);
    /// <summary>
    /// Light blue.
    /// </summary>
    public static readonly GorgonColor LightBlue = new(0.5f, 0.5f, 1.0f);
    /// <summary>
    /// DeepPink.
    /// </summary>
    public static readonly GorgonColor DeepPink = (GorgonColor)Color.DeepPink;
    /// <summary>
    /// DeepSkyBlue.
    /// </summary>
    public static readonly GorgonColor DeepSkyBlue = (GorgonColor)Color.DeepSkyBlue;
    /// <summary>
    /// Firebrick.
    /// </summary>
    public static readonly GorgonColor Firebrick = (GorgonColor)Color.Firebrick;
    /// <summary>
    /// OrangeRed.
    /// </summary>
    public static readonly GorgonColor OrangeRed = (GorgonColor)Color.OrangeRed;
    /// <summary>
    /// SeaGreen.
    /// </summary>
    public static readonly GorgonColor SeaGreen = (GorgonColor)Color.SeaGreen;
    /// <summary>
    /// WhiteSmoke.
    /// </summary>
    public static readonly GorgonColor WhiteSmoke = (GorgonColor)Color.WhiteSmoke;
    /// <summary>
    /// WhiteSmoke.
    /// </summary>
    public static readonly GorgonColor BlanchedAlmond = (GorgonColor)Color.BlanchedAlmond;
}
