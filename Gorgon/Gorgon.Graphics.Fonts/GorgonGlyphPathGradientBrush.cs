#region MIT.
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: Saturday, October 12, 2013 9:10:02 PM
// 
#endregion

using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Numerics;
using Gorgon.IO;
using Gorgon.Math;

namespace Gorgon.Graphics.Fonts;

/// <summary>
/// A brush that paints the font glyphs using a gradient that follows a specific path.
/// </summary>
/// <remarks>
/// <para>
/// This allows glyphs to be drawn using multiple colors that fade into one another.
/// </para>
/// </remarks>
/// <seealso cref="GorgonGlyphSolidBrush"/>
/// <seealso cref="GorgonGlyphHatchBrush"/>
/// <seealso cref="GorgonGlyphLinearGradientBrush"/>
/// <seealso cref="GorgonGlyphTextureBrush"/>
public class GorgonGlyphPathGradientBrush
    : GorgonGlyphBrush
{
    #region Properties.
    /// <summary>
    /// Property to return the type of brush.
    /// </summary>
    public override GlyphBrushType BrushType => GlyphBrushType.PathGradient;

    /// <summary>
    /// Property to set or return the wrapping mode for the gradient fill.
    /// </summary>
    public GlyphBrushWrapMode WrapMode
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the points for the path to follow in the gradient fill.
    /// </summary>
    public IList<Vector2> Points
    {
        get;
    }

    /// <summary>
    /// Property to return the list of blending factors for the gradient falloff.
    /// </summary>
    public IList<float> BlendFactors
    {
        get;
    }

    /// <summary>
    /// Property to return the list of blending positions for the gradient falloff.
    /// </summary>
    public IList<float> BlendPositions
    {
        get;
    }

    /// <summary>
    /// Property to set or return the color of the center point in the gradient.
    /// </summary>
    public GorgonColor CenterColor
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the center point position in the gradient.
    /// </summary>
    public Vector2 CenterPoint
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the focus point for the gradient falloff.
    /// </summary>
    public Vector2 FocusScales
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the interpolation values for the gradient.
    /// </summary>
    public IList<GorgonGlyphBrushInterpolator> Interpolation
    {
        get;
    }

    /// <summary>
    /// Property to set or return the surrounding colors for the gradient path.
    /// </summary>
    public IList<GorgonColor> SurroundColors
    {
        get;
    }
    #endregion

    #region Methods.
    /// <summary>Function to write out the specifics of the font brush data to a file writer.</summary>
    /// <param name="writer">The writer used to write the brush data.</param>
    internal override void WriteBrushData(GorgonBinaryWriter writer)
    {
        writer.Write((int)WrapMode);

        writer.Write(Points.Count);

        for (int i = 0; i < Points.Count; ++i)
        {
            writer.WriteValue(Points[i]);
        }

        writer.Write(BlendFactors.Count);

        for (int i = 0; i < BlendFactors.Count; ++i)
        {
            writer.Write(BlendFactors[i]);
        }

        writer.Write(BlendPositions.Count);

        for (int i = 0; i < BlendPositions.Count; ++i)
        {
            writer.Write(BlendPositions[i]);
        }

        writer.Write(CenterColor.ToARGB());
        writer.WriteValue(CenterPoint);
        writer.WriteValue(FocusScales);

        writer.Write(Interpolation.Count);
        for (int i = 0; i < Interpolation.Count; ++i)
        {
            GorgonGlyphBrushInterpolator interp = Interpolation[i];
            writer.Write(interp.Weight);
            writer.Write(interp.Color.ToARGB());
        }

        writer.Write(SurroundColors.Count);

        for (int i = 0; i < SurroundColors.Count; ++i)
        {
            writer.Write(SurroundColors[i].ToARGB());
        }
    }

    /// <summary>Function to read back the specifics of the font brush data from a file reader.</summary>
    /// <param name="reader">The reader used to read the brush data.</param>
    internal override void ReadBrushData(GorgonBinaryReader reader)
    {
        WrapMode = (GlyphBrushWrapMode)reader.ReadInt32();
        int count = reader.ReadInt32();

        Points.Clear();
        for (int i = 0; i < count; ++i)
        {
            Points.Add(reader.ReadValue<Vector2>());
        }

        BlendFactors.Clear();
        count = reader.ReadInt32();
        for (int i = 0; i < count; ++i)
        {
            BlendFactors.Add(reader.ReadSingle());
        }

        BlendPositions.Clear();
        count = reader.ReadInt32();
        for (int i = 0; i < count; ++i)
        {
            BlendPositions.Add(reader.ReadSingle());
        }

        CenterColor = new GorgonColor(reader.ReadInt32());
        CenterPoint = reader.ReadValue<Vector2>();
        FocusScales = reader.ReadValue<Vector2>();

        count = reader.ReadInt32();
        Interpolation.Clear();

        for (int i = 0; i < count; ++i)
        {
            Interpolation.Add(new GorgonGlyphBrushInterpolator(reader.ReadSingle(), new GorgonColor(reader.ReadInt32())));
        }

        count = reader.ReadInt32();
        SurroundColors.Clear();

        for (int i = 0; i < count; ++i)
        {
            SurroundColors.Add(new GorgonColor(reader.ReadInt32()));
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
        var result = new PathGradientBrush(Points.Select(item => new PointF(item.X, item.Y)).ToArray(), (WrapMode)WrapMode);

        var blend = new Blend(BlendFactors.Count.Max(BlendPositions.Count).Max(1));

        if (Interpolation.Count > 2)
        {
            var interpolationColors = new ColorBlend(Interpolation.Count);

            for (int i = 0; i < Interpolation.Count; i++)
            {
                interpolationColors.Colors[i] = Interpolation[i].Color;
                interpolationColors.Positions[i] = Interpolation[i].Weight;
            }

            result.InterpolationColors = interpolationColors;
        }

        for (int i = 0; i < blend.Factors.Length; i++)
        {
            if (i < BlendFactors.Count)
            {
                blend.Factors[i] = BlendFactors[i];
            }

            if (i < BlendPositions.Count)
            {
                blend.Positions[i] = BlendPositions[i];
            }
        }

        result.Blend = blend;
        result.CenterColor = CenterColor;
        result.CenterPoint = new PointF(CenterPoint.X, CenterPoint.Y);
        result.FocusScales = new PointF(FocusScales.X, FocusScales.Y);

        result.SurroundColors = SurroundColors.Select(item => item.ToColor()).ToArray();

        return result;
    }

    /// <summary>Function to clone an object.</summary>
    /// <returns>The cloned object.</returns>
    public override GorgonGlyphBrush Clone()
    {
        var brush = new GorgonGlyphPathGradientBrush
        {
            WrapMode = WrapMode,
            CenterColor = CenterColor,
            CenterPoint = CenterPoint,
            FocusScales = FocusScales
        };

        for (int i = 0; i < Points.Count; ++i)
        {
            brush.Points.Add(Points[i]);
        }

        for (int i = 0; i < BlendFactors.Count; ++i)
        {
            brush.BlendFactors.Add(BlendFactors[i]);
        }

        for (int i = 0; i < BlendPositions.Count; ++i)
        {
            brush.BlendPositions.Add(BlendPositions[i]);
        }

        for (int i = 0; i < Interpolation.Count; ++i)
        {
            brush.Interpolation.Add(new GorgonGlyphBrushInterpolator(Interpolation[i].Weight, Interpolation[i].Color));
        }

        for (int i = 0; i < SurroundColors.Count; ++i)
        {
            brush.SurroundColors.Add(SurroundColors[i]);
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
        var brush = other as GorgonGlyphPathGradientBrush;

        return ((brush == this) || ((brush is not null)
            && (brush.WrapMode == WrapMode)
            && (brush.CenterColor == CenterColor)
            && (brush.CenterPoint.Equals(CenterPoint))
            && (brush.FocusScales.Equals(FocusScales))
            && (brush.Points.SequenceEqual(Points))
            && (brush.BlendFactors.SequenceEqual(BlendFactors))
            && (brush.BlendPositions.SequenceEqual(BlendPositions))
            && (brush.Interpolation.SequenceEqual(Interpolation))
            && (brush.SurroundColors.SequenceEqual(SurroundColors))));
    }
    #endregion

    #region Constructor
    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonGlyphPathGradientBrush"/> class.
    /// </summary>
    public GorgonGlyphPathGradientBrush()
    {
        Points = [];
        BlendFactors = [];
        BlendPositions = [];
        Interpolation = [];
        SurroundColors = [];
    }
    #endregion
}
