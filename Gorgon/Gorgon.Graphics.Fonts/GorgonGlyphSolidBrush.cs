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
// Created: Saturday, October 12, 2013 11:22:36 PM
// 
#endregion

using System.Drawing;
using Gorgon.IO;

namespace Gorgon.Graphics.Fonts
{
	/// <summary>
	/// A brush used to draw glyphs using a solid fill color.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This is the default brush type used when no brush is specified when creating font glyphs.
	/// </para>
	/// </remarks>
	/// <seealso cref="GorgonGlyphTextureBrush"/>
	/// <seealso cref="GorgonGlyphHatchBrush"/>
	/// <seealso cref="GorgonGlyphLinearGradientBrush"/>
	/// <seealso cref="GorgonGlyphPathGradientBrush"/>
	public class GorgonGlyphSolidBrush
		: GorgonGlyphBrush
	{
		#region Properties.
		/// <summary>
		/// Property to return the type of brush.
		/// </summary>
		public override GlyphBrushType BrushType => GlyphBrushType.Solid;

		/// <summary>
		/// Property to set or return the color for the brush.
		/// </summary>
		public GorgonColor Color
		{
			get;
			set;
		}
        #endregion

        #region Methods.
        /// <summary>Function to write out the specifics of the font brush data to a file writer.</summary>
        /// <param name="writer">The writer used to write the brush data.</param>
        internal override void WriteBrushData(GorgonBinaryWriter writer) => writer.Write(Color.ToARGB());

        /// <summary>Function to read back the specifics of the font brush data from a file reader.</summary>
        /// <param name="reader">The reader used to read the brush data.</param>
        internal override void ReadBrushData(GorgonBinaryReader reader) => Color = new GorgonColor(reader.ReadInt32());

        /// <summary>
        /// Function to convert this brush to the equivalent GDI+ brush type.
        /// </summary>
        /// <returns>
        /// The GDI+ brush type for this object.
        /// </returns>
        internal override Brush ToGDIBrush() => new SolidBrush(Color);

        /// <summary>Function to clone an object.</summary>
        /// <returns>The cloned object.</returns>
        public override GorgonGlyphBrush Clone() => new GorgonGlyphSolidBrush
        {
            Color = Color
        };
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonGlyphSolidBrush"/> class.
        /// </summary>
        public GorgonGlyphSolidBrush() => Color = GorgonColor.White;
        #endregion
    }
}
