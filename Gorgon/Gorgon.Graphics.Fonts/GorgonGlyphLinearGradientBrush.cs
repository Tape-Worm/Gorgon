
// 
// Gorgon
// Copyright (C) 2013 Michael Winsor
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
// Created: Saturday, October 12, 2013 10:08:08 PM
// 


using System.Drawing.Drawing2D;
using Gorgon.IO;
using Gorgon.Math;
using DX = SharpDX;

namespace Gorgon.Graphics.Fonts;

/// <summary>
/// A brush used to draw glyphs using a linear gradient value
/// </summary>
/// <remarks>
/// <para>
/// This allows glyphs to be drawn using multiple colors that fade into one another
/// </para>
/// </remarks>
/// <seealso cref="GorgonGlyphSolidBrush"/>
/// <seealso cref="GorgonGlyphHatchBrush"/>
/// <seealso cref="GorgonGlyphPathGradientBrush"/>
/// <seealso cref="GorgonGlyphTextureBrush"/>
public class GorgonGlyphLinearGradientBrush
    : GorgonGlyphBrush
{

    /// <summary>
    /// Property to set or return the region for the gradient.
    /// </summary>
    internal DX.Rectangle GradientRegion
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the type of brush.
    /// </summary>
    public override GlyphBrushType BrushType => GlyphBrushType.LinearGradient;

    /// <summary>
    /// Property to set or return the starting color to use for the gradient.
    /// </summary>
    public GorgonColor StartColor
    {
        get
        {
            // ReSharper disable once InvertIf
            if (Interpolation.Count == 0)
            {
                return GorgonColor.BlackTransparent;
            }

            return Interpolation[0].Color;
        }
        set
        {
            GorgonGlyphBrushInterpolator newValue = new(0, value);

            if (Interpolation.Count == 0)
            {
                Interpolation.Add(newValue);
            }
            else
            {
                Interpolation[0] = newValue;
            }
        }
    }

    /// <summary>
    /// Property to set or return the ending color to use for the gradient.
    /// </summary>
    public GorgonColor EndColor
    {
        get
        {
            if (Interpolation.Count == 0)
            {
                return GorgonColor.BlackTransparent;
            }

            return Interpolation[^1].Color;
        }
        set
        {
            GorgonGlyphBrushInterpolator newValue = new(1, value);

            switch (Interpolation.Count)
            {
                case 0:
                    Interpolation.Add(new GorgonGlyphBrushInterpolator(0, GorgonColor.Black));
                    Interpolation.Add(newValue);
                    break;
                case 1:
                    Interpolation.Add(newValue);
                    break;
                default:
                    Interpolation[^1] = newValue;
                    break;
            }
        }
    }

    /// <summary>
    /// Property to set or return the angle, in degrees, for the gradient.
    /// </summary>
    public float Angle
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return whether to scale the angle.
    /// </summary>
    public bool ScaleAngle
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return whether to enable or disable gamma correction.
    /// </summary>
    public bool GammaCorrection
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the interpolation colors for the gradient.
    /// </summary>
    public IList<GorgonGlyphBrushInterpolator> Interpolation
    {
        get;
    }



    /// <summary>Function to write out the specifics of the font brush data to a file writer.</summary>
    /// <param name="writer">The writer used to write the brush data.</param>
    internal override void WriteBrushData(GorgonBinaryWriter writer)
    {
        writer.Write(Angle);
        writer.Write(ScaleAngle);
        writer.Write(GammaCorrection);
        writer.Write(Interpolation.Count);
        for (int i = 0; i < Interpolation.Count; ++i)
        {
            GorgonGlyphBrushInterpolator interp = Interpolation[i];
            writer.Write(interp.Weight);
            writer.Write(interp.Color.ToARGB());
        }
    }

    /// <summary>Function to read back the specifics of the font brush data from a file reader.</summary>
    /// <param name="reader">The reader used to read the brush data.</param>
    internal override void ReadBrushData(GorgonBinaryReader reader)
    {
        Angle = reader.ReadSingle();
        ScaleAngle = reader.ReadBoolean();
        GammaCorrection = reader.ReadBoolean();

        int interpCount = reader.ReadInt32();
        if (interpCount == 0)
        {
            return;
        }

        Interpolation.Clear();

        for (int i = 0; i < interpCount; ++i)
        {
            Interpolation.Add(new GorgonGlyphBrushInterpolator(reader.ReadSingle(), new GorgonColor(reader.ReadInt32())));
        }
    }

    /// <summary>
    /// Function to convert this brush to the equivalent GDI+ brush type.
    /// </summary>
    /// <returns>
    /// The GDI+ brush type for this object.
    /// </returns>
    internal override Brush ToGDIBrush()
    {
        LinearGradientBrush result = new(new Rectangle(GradientRegion.X, GradientRegion.Y, GradientRegion.Width, GradientRegion.Height),
                                             StartColor,
                                             EndColor,
                                             Angle,
                                             ScaleAngle)
        {
            GammaCorrection = GammaCorrection
        };

        ColorBlend interpolationColors = new(Interpolation.Count);

        for (int i = 0; i < Interpolation.Count; i++)
        {
            interpolationColors.Colors[i] = Interpolation[i].Color;
            interpolationColors.Positions[i] = Interpolation[i].Weight;
        }

        result.InterpolationColors = interpolationColors;

        return result;
    }

    /// <summary>Function to clone an object.</summary>
    /// <returns>The cloned object.</returns>
    public override GorgonGlyphBrush Clone()
    {
        GorgonGlyphLinearGradientBrush brush = new()
        {
            Angle = Angle,
            ScaleAngle = ScaleAngle,
            GammaCorrection = GammaCorrection
        };
        brush.Interpolation.Clear();

        for (int i = 0; i < Interpolation.Count; ++i)
        {
            brush.Interpolation.Add(new GorgonGlyphBrushInterpolator(Interpolation[i].Weight, Interpolation[i].Color));
        }

        return brush;
    }

    /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    ///   <span class="keyword">
    ///     <span class="languageSpecificText">
    ///       <span class="cs">true</span>
    ///       <span class="vb">True</span>
    ///       <span class="cpp">true</span>
    ///     </span>
    ///   </span>
    ///   <span class="nu">
    ///     <span class="keyword">true</span> (<span class="keyword">True</span> in Visual Basic)</span> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <span class="keyword"><span class="languageSpecificText"><span class="cs">false</span><span class="vb">False</span><span class="cpp">false</span></span></span><span class="nu"><span class="keyword">false</span> (<span class="keyword">False</span> in Visual Basic)</span>.
    /// </returns>
    public override bool Equals(GorgonGlyphBrush other)
    {
        GorgonGlyphLinearGradientBrush brush = other as GorgonGlyphLinearGradientBrush;

        return ((brush == this) || ((brush is not null)
            && (brush.Angle.EqualsEpsilon(Angle))
            && (brush.ScaleAngle == ScaleAngle)
            && (brush.GammaCorrection == GammaCorrection)
            && (brush.Interpolation.SequenceEqual(Interpolation))));
    }



    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonGlyphPathGradientBrush"/> class.
    /// </summary>
    public GorgonGlyphLinearGradientBrush() => Interpolation =
                        [
                            new(0, GorgonColor.Black),
                            new(1, GorgonColor.White)
                        ];

}
