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
// Created: Wednesday, November 13, 2013 9:22:57 PM
// 
#endregion

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using GorgonLibrary.Editor.FontEditorPlugIn.Properties;
using GorgonLibrary.Graphics;
using GorgonLibrary.Math;

namespace GorgonLibrary.Editor.FontEditorPlugIn
{
	/// <summary>
	/// Font brush editor.
	/// </summary>
	class FontBrushEditor
		: UITypeEditor
	{
		#region Methods.
        /// <summary>
        /// Function to create a GDI+ linear gradient brush from a gorgon linear gradient font brush.
        /// </summary>
        /// <param name="region">The region to paint.</param>
        /// <param name="brush">Brush to convert.</param>
        /// <returns>The GDI+ brush.</returns>
        internal static Brush CreateLinearGradBrush(Rectangle region, GorgonGlyphBrush brush)
        {
            var linearBrush = (GorgonGlyphLinearGradientBrush)brush;
			var result = new LinearGradientBrush(region, linearBrush.StartColor, linearBrush.EndColor, linearBrush.Angle, linearBrush.ScaleAngle)
			{
				GammaCorrection = linearBrush.GammaCorrection
			};

			if (linearBrush.Interpolation.Count == 0)
			{
				return result;
			}

			var interpColors = new ColorBlend(linearBrush.Interpolation.Count.Max(1));

			for (int i = 0; i < linearBrush.Interpolation.Count; i++)
			{
				interpColors.Colors[i] = linearBrush.Interpolation[i].Color;
				interpColors.Positions[i] = linearBrush.Interpolation[i].Weight;
			}

			result.InterpolationColors = interpColors;

            return result;
        }

        /// <summary>
		/// Edits the specified object's value using the editor style indicated by the <see cref="M:System.Drawing.Design.UITypeEditor.GetEditStyle"/> method.
		/// </summary>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that can be used to gain additional context information.</param>
		/// <param name="provider">An <see cref="T:System.IServiceProvider"/> that this editor can use to obtain services.</param>
		/// <param name="value">The object to edit.</param>
		/// <returns>
		/// The new value of the object. If the value of the object has not changed, this should return the same object it was passed.
		/// </returns>
		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			var descriptor = (ContentTypeDescriptor)context.Instance;
            var document = (GorgonFontContent)descriptor.Content;
		    FormBrushEditor brushEditor = null;

	        try
	        {
		        EditorFile textureBrushFile = null;

		        if (document.Brush.BrushType == GlyphBrushType.Texture)
		        {
			        Dependency dependency = document.Dependencies.SingleOrDefault(item => string.Equals(item.Type, GorgonFontContent.TextureBrushTextureType));

			        if (dependency != null)
			        {
				        textureBrushFile = dependency.EditorFile;
			        }
		        }

		        brushEditor = new FormBrushEditor(document)
				              {
					              BrushType = document.Brush.BrushType
				              };

				switch (document.Brush.BrushType)
				{
					case GlyphBrushType.Texture:
						brushEditor.TextureBrush = (GorgonGlyphTextureBrush)document.Brush;
						brushEditor.TextureBrushFile = textureBrushFile;
						break;
					case GlyphBrushType.Hatched:
						brushEditor.PatternBrush = (GorgonGlyphHatchBrush)document.Brush;
						break;
					case GlyphBrushType.LinearGradient:
						brushEditor.GradientBrush = (GorgonGlyphLinearGradientBrush)document.Brush;
						break;
					case GlyphBrushType.Solid:
						brushEditor.SolidBrush = (GorgonGlyphSolidBrush)document.Brush;
						break;
				}
                GorgonGlyphBrush brush = document.Brush;

			    if (brushEditor.ShowDialog() != DialogResult.OK)
			    {
			        return brush;
			    }
				
			    // Destroy the previous brush.
			    if (brush.BrushType == GlyphBrushType.Texture)
			    {
				    var textureBrush = (GorgonGlyphTextureBrush)document.Brush;

					if ((brushEditor.BrushType != GlyphBrushType.Texture)
						&& (textureBrush.Texture != null))
					{
						textureBrush.Texture.Dispose();
					}

					// Remove the old texture brush dependency (if it existed).

				    if (textureBrushFile != null)
					{
						if (document.Dependencies.Contains(textureBrushFile, GorgonFontContent.TextureBrushTextureType))
						{
							document.Dependencies.Remove(textureBrushFile, GorgonFontContent.TextureBrushTextureType);
						}
					}
			    }

			    switch (brushEditor.BrushType)
			    {
			        case GlyphBrushType.Texture:
			            brush = brushEditor.TextureBrush;

						// Add the updated texture brush dependency path.
						if (textureBrushFile != null)
						{
							document.Dependencies[brushEditor.TextureBrushFile, GorgonFontContent.TextureBrushTextureType] = new Dependency(brushEditor.TextureBrushFile,
							                                                                                                                GorgonFontContent.TextureBrushTextureType);
						}
			            break;
			        case GlyphBrushType.Solid:
			            brush = brushEditor.SolidBrush;
			            break;
			        case GlyphBrushType.LinearGradient:
			            brush = brushEditor.GradientBrush;
			            break;
			        case GlyphBrushType.Hatched:
			            brush = brushEditor.PatternBrush;
			            break;
			    }

				return brush;
			}
			finally
			{
			    if (brushEditor != null)
			    {
			        brushEditor.Dispose();
			    }
			}
		}

        /// <summary>
        /// Paints a representation of the value of an object using the specified <see cref="T:System.Drawing.Design.PaintValueEventArgs" />.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Drawing.Design.PaintValueEventArgs" /> that indicates what to paint and where to paint it.</param>
	    public override void PaintValue(PaintValueEventArgs e)
	    {
            var sourceBrush = e.Value as GorgonGlyphBrush;
            Brush brush = null;

            try
            {
                // If we haven't assigned a brush, then fall back to a solid brush.
                if (sourceBrush == null)
                {
	                sourceBrush = new GorgonGlyphSolidBrush();
                }

                switch (sourceBrush.BrushType)
                {
                    case GlyphBrushType.Hatched:
                        var hatchBrush = (GorgonGlyphHatchBrush)sourceBrush;

                        brush = new HatchBrush(hatchBrush.HatchStyle, hatchBrush.ForegroundColor, hatchBrush.BackgroundColor);
                        e.Graphics.FillRectangle(brush, e.Bounds);
                        break;
                    case GlyphBrushType.LinearGradient:
                        brush = CreateLinearGradBrush(e.Bounds, sourceBrush);
                        e.Graphics.FillRectangle(brush, e.Bounds);
                        break;
                    case GlyphBrushType.Texture:

		                var textureBrush = (GorgonGlyphTextureBrush)sourceBrush;

						// If we can't find the image, then display the broken image icon.
		                if (textureBrush.Texture == null)
		                {
							e.Graphics.DrawImage(Resources.image_missing_16x16,
												 e.Bounds,
												 new Rectangle(0, 0, Resources.image_missing_16x16.Width, Resources.image_missing_16x16.Height),
												 GraphicsUnit.Pixel);
			                return;
		                }

						// Convert to a GDI image so the property pane can display the texture.
		                Image gdiImage = textureBrush.Texture.ToGDIImage()[0];

		                e.Graphics.DrawImage(gdiImage,
		                                     e.Bounds,
			                                 textureBrush.Texture.ToPixel(textureBrush.TextureRegion),
		                                     GraphicsUnit.Pixel);

		                gdiImage.Dispose();
                        break;
                    case GlyphBrushType.Solid:
		                var solidBrush = (GorgonGlyphSolidBrush)sourceBrush;

                        brush = new SolidBrush(solidBrush.Color);
                        e.Graphics.FillRectangle(brush, e.Bounds);
                        break;
                    default:
                        return;
                }
            }
            finally
            {
                if (brush != null)
                {
                    brush.Dispose();
                }
            }
	    }

	    /// <summary>
        /// Indicates whether the specified context supports painting a representation of an object's value within the specified context.
        /// </summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that can be used to gain additional context information.</param>
        /// <returns>
        /// true if <see cref="M:System.Drawing.Design.UITypeEditor.PaintValue(System.Object,System.Drawing.Graphics,System.Drawing.Rectangle)" /> is implemented; otherwise, false.
        /// </returns>
	    public override bool GetPaintValueSupported(ITypeDescriptorContext context)
	    {
            var descriptor = (ContentTypeDescriptor)context.Instance;
            var document = (GorgonFontContent)descriptor.Content;

	        if (document.Brush == null)
	        {
	            return false;
	        }

            switch (document.Brush.BrushType)
            {
                case GlyphBrushType.Hatched:
                case GlyphBrushType.LinearGradient:
                case GlyphBrushType.Texture:
                case GlyphBrushType.Solid:
                    return true;
                default:
                    return false;
            }
	    }

	    /// <summary>
		/// Gets the editor style used by the <see cref="M:System.Drawing.Design.UITypeEditor.EditValue(System.IServiceProvider,System.Object)"/> method.
		/// </summary>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that can be used to gain additional context information.</param>
		/// <returns>
		/// A <see cref="T:System.Drawing.Design.UITypeEditorEditStyle"/> value that indicates the style of editor used by the <see cref="M:System.Drawing.Design.UITypeEditor.EditValue(System.IServiceProvider,System.Object)"/> method. If the <see cref="T:System.Drawing.Design.UITypeEditor"/> does not support this method, then <see cref="M:System.Drawing.Design.UITypeEditor.GetEditStyle"/> will return <see cref="F:System.Drawing.Design.UITypeEditorEditStyle.None"/>.
		/// </returns>
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
	    {
	        return !GetPaintValueSupported(context) ? UITypeEditorEditStyle.None : UITypeEditorEditStyle.Modal;
	    }
	    #endregion
	}
}
