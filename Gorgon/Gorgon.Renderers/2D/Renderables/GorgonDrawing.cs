#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Sunday, March 04, 2012 4:01:07 PM
// 
#endregion

using System.Drawing;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics;
using SlimMath;

namespace Gorgon.Renderers
{
	/// <summary>
	/// Interface for immediate drawing of renderables.
	/// </summary>
	public class GorgonDrawing
	{
		#region Variables.
		private readonly GorgonRectangle _rect;											// Rectangle.
		private readonly GorgonPoint _point;											// Point.
		private readonly GorgonLine _line;												// Line.
		private readonly GorgonEllipse _ellipse;										// Ellipse.
		private readonly GorgonTriangle _triangle;										// Triangle.
		private readonly GorgonText _string;											// String.
		private GorgonRenderable.DepthStencilStates _depthStencil;						// Depth stencil states.
		private GorgonRenderable.TextureSamplerState _sampler;							// Texture sampler states.
		private GorgonRenderable.BlendState _blend;										// Blending states.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return depth/stencil buffer states for this renderable.
		/// </summary>
		public GorgonRenderable.DepthStencilStates DepthStencil
		{
			get
			{
				return _depthStencil;
			}
			set
			{
			    if (value == null)
			    {
			        return;
			    }

			    _depthStencil = value;
			}
		}

		/// <summary>
		/// Property to set or return advanced blending states for this renderable.
		/// </summary>
		public GorgonRenderable.BlendState Blending
		{
			get
			{
				return _blend;
			}
			set
			{
			    if (value == null)
			    {
			        return;
			    }

			    _blend = value;
			}
		}

		/// <summary>
		/// Property to set or return advanded texture sampler states for this renderable.
		/// </summary>
		public GorgonRenderable.TextureSamplerState TextureSampler
		{
			get
			{
				return _sampler;
			}
			set
			{
			    if (value == null)
			    {
			        return;
			    }

			    _sampler = value;
			}
		}

		/// <summary>
		/// Property to set or return pre-defined smoothing states for the renderable.
		/// </summary>
		public SmoothingMode SmoothingMode
		{
			get
			{
				switch (TextureSampler.TextureFilter)
				{
					case TextureFilter.Point:
						return SmoothingMode.None;
					case TextureFilter.Linear:
						return SmoothingMode.Smooth;
					case TextureFilter.MinLinear | TextureFilter.MipLinear:
						return SmoothingMode.SmoothMinify;
					case TextureFilter.MagLinear | TextureFilter.MipLinear:
						return SmoothingMode.SmoothMagnify;
					default:
						return SmoothingMode.Custom;
				}
			}
			set
			{
				switch (value)
				{
					case SmoothingMode.None:
						TextureSampler.TextureFilter = TextureFilter.Point;
						break;
					case SmoothingMode.Smooth:
						TextureSampler.TextureFilter = TextureFilter.Linear;
						break;
					case SmoothingMode.SmoothMinify:
						TextureSampler.TextureFilter = TextureFilter.MinLinear | TextureFilter.MipLinear;
						break;
					case SmoothingMode.SmoothMagnify:
						TextureSampler.TextureFilter = TextureFilter.MagLinear | TextureFilter.MipLinear;
						break;
				}
			}
		}

		/// <summary>
		/// Property to set or return a pre-defined blending states for the renderable.
		/// </summary>
		public BlendingMode BlendingMode
		{
			get
			{
			    if ((Blending.SourceBlend == BlendType.One)
			        && (Blending.DestinationBlend == BlendType.Zero))
			    {
			        return BlendingMode.None;
			    }

			    if (Blending.SourceBlend == BlendType.SourceAlpha)
				{
				    if (Blending.DestinationBlend == BlendType.InverseSourceAlpha)
				    {
				        return BlendingMode.Modulate;
				    }
				    if (Blending.DestinationBlend == BlendType.One)
				    {
				        return BlendingMode.Additive;
				    }
				}

			    if ((Blending.SourceBlend == BlendType.One)
			        && (Blending.DestinationBlend == BlendType.InverseSourceAlpha))
			    {
			        return BlendingMode.PreMultiplied;
			    }

			    if ((Blending.SourceBlend == BlendType.InverseDestinationColor)
			        && (Blending.DestinationBlend == BlendType.InverseSourceColor))
			    {
			        return BlendingMode.Inverted;
			    }

			    return BlendingMode.Custom;
			}
			set
			{
				switch (value)
				{
					case BlendingMode.Additive:
						Blending.SourceBlend = BlendType.SourceAlpha;
						Blending.DestinationBlend = BlendType.One;
						break;
					case BlendingMode.Inverted:
						Blending.SourceBlend = BlendType.InverseDestinationColor;
						Blending.DestinationBlend = BlendType.InverseSourceColor;
						break;
					case BlendingMode.Modulate:
						Blending.SourceBlend = BlendType.SourceAlpha;
						Blending.DestinationBlend = BlendType.InverseSourceAlpha;
						break;
					case BlendingMode.PreMultiplied:
						Blending.SourceBlend = BlendType.One;
						Blending.DestinationBlend = BlendType.InverseSourceAlpha;
						break;
					case BlendingMode.None:
						Blending.SourceBlend = BlendType.One;
						Blending.DestinationBlend = BlendType.Zero;
						break;
				}
			}
		}

		/// <summary>
		/// Property to set or return the culling mode.
		/// </summary>
		public CullingMode CullingMode
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the range of alpha values to reject on this renderable.
		/// </summary>
		public GorgonRangeF AlphaTestValues
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to set the states for the renderable primitives and blit.
		/// </summary>
		/// <param name="renderable">Renderable object to update.</param>
		private void SetStates(IRenderable renderable)
		{
			renderable.AlphaTestValues = AlphaTestValues;
			renderable.Blending = _blend;
			renderable.DepthStencil = _depthStencil;
			renderable.TextureSampler = _sampler;
			renderable.CullingMode = CullingMode;
		}

		/// <summary>
		/// Function to blit a texture to the current render target.
		/// </summary>
		/// <param name="texture">Texture to blit to the current render target.</param>
		/// <param name="blitRegion">The position, and size of the blitted region.</param>
		/// <param name="textureRegion">The region of the texture to blit.</param>
		/// <remarks>This is for very quickly copying a texture to a render target.  If more control is required, then use a <see cref="Gorgon.Renderers.GorgonSprite">Sprite</see>.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="texture"/> parameter is NULL (Nothing in VB.Net).</exception>
		public void Blit(GorgonTexture2D texture, RectangleF blitRegion, RectangleF textureRegion)
		{
			GorgonDebug.AssertNull(texture, "texture");
			FilledRectangle(blitRegion, Color.White, texture, textureRegion);
		}

		/// <summary>
		/// Function to blit a texture to the current render target.
		/// </summary>
		/// <param name="texture">Texture to blit to the current render target.</param>
		/// <param name="blitRegion">The position, and size of the blitted region.</param>
		/// <remarks>This is for very quickly copying a texture to a render target.  If more control is required, then use a <see cref="Gorgon.Renderers.GorgonSprite">Sprite</see>.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="texture"/> parameter is NULL (Nothing in VB.Net).</exception>
		public void Blit(GorgonTexture2D texture, RectangleF blitRegion)
		{
			Blit(texture, blitRegion, new RectangleF(Vector2.Zero, new Vector2(1)));
		}

		/// <summary>
		/// Function to blit a render target to the current render target.
		/// </summary>
		/// <param name="texture">Texture to blit to the current render target.</param>
		/// <param name="position">Position on the screen to blit onto.</param>
		/// <remarks>This is for very quickly copying a render target to another target.  If more control is required, then use a <see cref="Gorgon.Renderers.GorgonSprite">Sprite</see>.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="texture"/> parameter is NULL (Nothing in VB.Net).</exception>
		public void Blit(GorgonTexture2D texture, Vector2 position)
		{
			Blit(texture, new RectangleF(position, texture.Settings.Size), new RectangleF(Vector2.Zero, new Vector2(1)));
		}

		/// <summary>
		/// Function to draw a filled rectangle onto the current target.
		/// </summary>
		/// <param name="rectangle">Rectangle dimensions.</param>
		/// <param name="color">Color for the rectangle.</param>
		/// <param name="texture">Texture to apply to the rectangle.</param>
		/// <param name="textureRegion">Texture dimensions to use.</param>
		public void FilledRectangle(RectangleF rectangle, GorgonColor color, GorgonTexture2D texture, RectangleF textureRegion)
		{
			SetStates(_rect);
			_rect.IsFilled = true;
			_rect.Color = color;
			_rect.Texture = texture;
			_rect.TextureRegion = textureRegion;
			_rect.Rectangle = rectangle;
			_rect.LineThickness = new Vector2(1.0f);
			_rect.Draw();
		}

		/// <summary>
		/// Function to draw a filled rectangle onto the current target.
		/// </summary>
		/// <param name="rectangle">Rectangle dimensions.</param>
		/// <param name="color">Color for the rectangle.</param>
		/// <param name="texture">Texture to apply to the rectangle.</param>
		public void FilledRectangle(RectangleF rectangle, GorgonColor color, GorgonTexture2D texture)
		{
			FilledRectangle(rectangle, color, texture, new RectangleF(Vector2.Zero, new Vector2(1)));
		}

		/// <summary>
		/// Function to draw a filled rectangle onto the current target.
		/// </summary>
		/// <param name="rectangle">Rectangle dimensions.</param>
		/// <param name="color">Color for the rectangle.</param>
		public void FilledRectangle(RectangleF rectangle, GorgonColor color)
		{
			FilledRectangle(rectangle, color, null, RectangleF.Empty);
		}

		/// <summary>
		/// Function to draw a filled ellipse onto the current target.
		/// </summary>
		/// <param name="dimensions">Ellipse dimensions.</param>
		/// <param name="color">Color for the ellipse.</param>
		/// <param name="quality">Quality of rendering for the ellipse.</param>
		/// <param name="texture">Texture to apply to the ellipse.</param>
		/// <param name="textureRegion">Texture dimensions to use.</param>
		/// <remarks>The <paramref name="quality"/> parameter can have a value from 4 to 256.  The higher the quality, the better looking the ellipse, however this will impact performance.</remarks>
		public void FilledEllipse(RectangleF dimensions, GorgonColor color, int quality, GorgonTexture2D texture, RectangleF textureRegion)
		{
			SetStates(_ellipse);
			_ellipse.IsFilled = true;
			_ellipse.Position = dimensions.Location;
			_ellipse.Size = dimensions.Size;
			_ellipse.Color = color;
			_ellipse.Quality = quality;
			_ellipse.Texture = texture;
			_ellipse.TextureRegion = textureRegion;
			_ellipse.Draw();
		}

		/// <summary>
		/// Function to draw a filled ellipse onto the current target.
		/// </summary>
		/// <param name="dimensions">Ellipse dimensions.</param>
		/// <param name="color">Color for the ellipse.</param>
		/// <param name="quality">Quality of rendering for the ellipse.</param>
		/// <param name="texture">Texture to apply to the ellipse.</param>
		/// <remarks>The <paramref name="quality"/> parameter can have a value from 4 to 256.  The higher the quality, the better looking the ellipse, however this will impact performance.</remarks>
		public void FilledEllipse(RectangleF dimensions, GorgonColor color, int quality, GorgonTexture2D texture)
		{
			FilledEllipse(dimensions, color, quality, texture, new RectangleF(Vector2.Zero, new Vector2(1)));
		}

		/// <summary>
		/// Function to draw a filled ellipse onto the current target.
		/// </summary>
		/// <param name="dimensions">Ellipse dimensions.</param>
		/// <param name="color">Color for the ellipse.</param>
		/// <param name="quality">Quality of rendering for the ellipse.</param>
		/// <remarks>The <paramref name="quality"/> parameter can have a value from 4 to 256.  The higher the quality, the better looking the ellipse, however this will impact performance.</remarks>
		public void FilledEllipse(RectangleF dimensions, GorgonColor color, int quality)
		{
			FilledEllipse(dimensions, color, quality, null, RectangleF.Empty);
		}

		/// <summary>
		/// Function to draw a filled ellipse onto the current target.
		/// </summary>
		/// <param name="dimensions">Ellipse dimensions.</param>
		/// <param name="color">Color for the ellipse.</param>
		public void FilledEllipse(RectangleF dimensions, GorgonColor color)
		{
			FilledEllipse(dimensions, color, 64, null, RectangleF.Empty);
		}

		/// <summary>
		/// Function to draw a filled triangle.
		/// </summary>
		/// <param name="position">Position of the triangle.</param>
		/// <param name="point1">First point in the triangle.</param>
		/// <param name="point2">Second point in the triangle.</param>
		/// <param name="point3">Third point in the triangle.</param>
		/// <param name="texture">Texture to apply to the triangle.</param>
        public void FilledTriangle(Vector2 position, GorgonPolygonPoint point1, GorgonPolygonPoint point2, GorgonPolygonPoint point3, GorgonTexture2D texture)
		{
			SetStates(_triangle);
			_triangle.IsFilled = true;
			_triangle.Position = position;
			_triangle.SetPoint(0, point1);
			_triangle.SetPoint(1, point2);
			_triangle.SetPoint(2, point3);
			_triangle.Texture = texture;
			_triangle.LineThickness = new Vector2(1.0f);
			_triangle.Draw();
		}

		/// <summary>
		/// Function to draw a string.
		/// </summary>
		/// <param name="font">Font to use.</param>
		/// <param name="text">Text to draw.</param>
		/// <param name="position">Position of the text.</param>
		/// <param name="color">Color of the text.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="font"/> parameter is NULL (Nothing in VB.Net).</exception>
		public void DrawString(GorgonFont font, string text, Vector2 position, GorgonColor color)
		{
			DrawString(font, text, position, color, false, new Vector2(1), 0.25f);
		}

		/// <summary>
		/// Function to draw a string.
		/// </summary>
		/// <param name="font">Font to use.</param>
		/// <param name="text">Text to draw.</param>
		/// <param name="position">Position of the text.</param>
		/// <param name="color">Color of the text.</param>
		/// <param name="useShadow"><c>true</c> to use a shadow, <c>false</c> to display normally.</param>
		/// <param name="shadowOffset">Offset of the shadow, in pixels.</param>
		/// <param name="shadowOpacity">Opacity of the shadow.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="font"/> parameter is NULL (Nothing in VB.Net).</exception>
		public void DrawString(GorgonFont font, string text, Vector2 position, GorgonColor color, bool useShadow, Vector2 shadowOffset, float shadowOpacity)
		{
			GorgonDebug.AssertNull(font, "font");

			if (string.IsNullOrEmpty(text))
			{
				return;
			}
			
			SetStates(_string);

			_string.Position = position;
			_string.Font = font;
			_string.Text = text;
			_string.Color = color;
			_string.ShadowEnabled = useShadow;
			_string.ShadowOffset = shadowOffset;
			_string.ShadowOpacity = shadowOpacity;
			_string.Draw();
		}

		/// <summary>
		/// Function to measure a string.
		/// </summary>
		/// <param name="font">Font to use when measuring.</param>
		/// <param name="text">Text to measure.</param>
		/// <param name="wordWrap"><c>true</c> if word wrapping should be used.</param>
		/// <param name="bounds">Boundaries for the size of the string.</param>
		/// <returns>The size of the string.</returns>
		public Vector2 MeasureString(GorgonFont font, string text, bool wordWrap, SizeF bounds)
		{
			_string.Font = font;
			Vector2 size = _string.MeasureText(text, wordWrap, bounds.Width);

		    if (size.X > bounds.Width)
		    {
		        size.X = bounds.Width;
		    }
		    if (size.Y > bounds.Height)
            {
				size.Y = bounds.Height;
            }

			return size;
		}

		/// <summary>
		/// Function to draw an unfilled triangle.
		/// </summary>
		/// <param name="position">Position of the triangle.</param>
		/// <param name="point1">First point in the triangle.</param>
		/// <param name="point2">Second point in the triangle.</param>
		/// <param name="point3">Third point in the triangle.</param>
		/// <param name="thickness">Line thickness.</param>
		/// <param name="texture">Texture to apply to the triangle.</param>
        public void DrawTriangle(Vector2 position, GorgonPolygonPoint point1, GorgonPolygonPoint point2, GorgonPolygonPoint point3, Vector2 thickness, GorgonTexture2D texture)
		{
			SetStates(_triangle);
			_triangle.IsFilled = false;
			_triangle.Position = position;
			_triangle.SetPoint(0, point1);
			_triangle.SetPoint(1, point2);
			_triangle.SetPoint(2, point3);
			_triangle.Texture = texture;
			_triangle.LineThickness = thickness;
			_triangle.Draw();
		}

		/// <summary>
		/// Function to draw an ellipse onto the current target.
		/// </summary>
		/// <param name="dimensions">Ellipse dimensions.</param>
		/// <param name="color">Color for the ellipse.</param>
		/// <param name="quality">Quality of rendering for the ellipse.</param>
		/// <param name="thickness">Thickness of the line.</param>
		/// <param name="texture">Texture to apply to the ellipse.</param>
		/// <param name="textureRegion">Texture dimensions to use.</param>
		/// <remarks>The <paramref name="quality"/> parameter can have a value from 4 to 256.  The higher the quality, the better looking the ellipse, however this will impact performance.</remarks>
		public void DrawEllipse(RectangleF dimensions, GorgonColor color, int quality, Vector2 thickness, GorgonTexture2D texture, RectangleF textureRegion)
		{
			SetStates(_ellipse);
			_ellipse.IsFilled = false;
			_ellipse.Position = dimensions.Location;
			_ellipse.Size = dimensions.Size;
			_ellipse.Color = color;
			_ellipse.Quality = quality;
			_ellipse.Texture = texture;
			_ellipse.TextureRegion = textureRegion;
			_ellipse.LineThickness = thickness;
			_ellipse.Draw();
		}

		/// <summary>
		/// Function to draw an ellipse onto the current target.
		/// </summary>
		/// <param name="dimensions">Ellipse dimensions.</param>
		/// <param name="color">Color for the ellipse.</param>
		/// <param name="quality">Quality of rendering for the ellipse.</param>
		/// <param name="thickness">Thickness of the line.</param>
		/// <param name="texture">Texture to apply to the ellipse.</param>
		/// <remarks>The <paramref name="quality"/> parameter can have a value from 4 to 256.  The higher the quality, the better looking the ellipse, however this will impact performance.</remarks>
		public void DrawEllipse(RectangleF dimensions, GorgonColor color, int quality, Vector2 thickness, GorgonTexture2D texture)
		{
			DrawEllipse(dimensions, color, quality, thickness, texture, new RectangleF(Vector2.Zero, new Vector2(1)));
		}

		/// <summary>
		/// Function to draw an ellipse onto the current target.
		/// </summary>
		/// <param name="dimensions">Ellipse dimensions.</param>
		/// <param name="color">Color for the ellipse.</param>
		/// <param name="quality">Quality of rendering for the ellipse.</param>
		/// <param name="thickness">Thickness of the line.</param>
		/// <remarks>The <paramref name="quality"/> parameter can have a value from 4 to 256.  The higher the quality, the better looking the ellipse, however this will impact performance.</remarks>
		public void DrawEllipse(RectangleF dimensions, GorgonColor color, int quality, Vector2 thickness)
		{
			DrawEllipse(dimensions, color, quality, thickness, null, RectangleF.Empty);
		}

		/// <summary>
		/// Function to draw an ellipse onto the current target.
		/// </summary>
		/// <param name="dimensions">Ellipse dimensions.</param>
		/// <param name="color">Color for the ellipse.</param>
		/// <param name="quality">Quality of rendering for the ellipse.</param>
		public void DrawEllipse(RectangleF dimensions, GorgonColor color, int quality)
		{
			DrawEllipse(dimensions, color, quality, new Vector2(1.0f), null, RectangleF.Empty);
		}

		/// <summary>
		/// Function to draw an ellipse onto the current target.
		/// </summary>
		/// <param name="dimensions">Ellipse dimensions.</param>
		/// <param name="color">Color for the ellipse.</param>
		/// <remarks>The default quality is 64 segments.</remarks>
		public void DrawEllipse(RectangleF dimensions, GorgonColor color)
		{
			DrawEllipse(dimensions, color, 64, new Vector2(1.0f), null, RectangleF.Empty);
		}

		/// <summary>
		/// Function to draw a rectangle onto the current target.
		/// </summary>
		/// <param name="rectangle">Rectangle dimensions.</param>
		/// <param name="color">Color for the rectangle.</param>
		/// <param name="thickness">Thickness of the lines to draw.</param>
		/// <param name="texture">Texture to apply to the rectangle.</param>
		/// <param name="textureRegion">Texture dimensions to use.</param>
		public void DrawRectangle(RectangleF rectangle, GorgonColor color, Vector2 thickness, GorgonTexture2D texture, RectangleF textureRegion)
		{
			SetStates(_rect);
			_rect.IsFilled = false;
			_rect.Color = color;
			_rect.Texture = texture;
			_rect.TextureRegion = textureRegion;
			_rect.Rectangle = rectangle;
			_rect.LineThickness = thickness;
			_rect.Draw();
		}

		/// <summary>
		/// Function to draw a rectangle onto the current target.
		/// </summary>
		/// <param name="rectangle">Rectangle dimensions.</param>
		/// <param name="color">Color for the rectangle.</param>
		/// <param name="thickness">Thickness of the lines to draw.</param>
		/// <param name="texture">Texture to apply to the rectangle.</param>
		public void DrawRectangle(RectangleF rectangle, GorgonColor color, Vector2 thickness, GorgonTexture2D texture)
		{
			DrawRectangle(rectangle, color, thickness, texture, new RectangleF(Vector2.Zero, new Vector2(1)));
		}

		/// <summary>
		/// Function to draw a rectangle onto the current target.
		/// </summary>
		/// <param name="rectangle">Rectangle dimensions.</param>
		/// <param name="color">Color for the rectangle.</param>
		/// <param name="thickness">Thickness of the lines to draw.</param>
		public void DrawRectangle(RectangleF rectangle, GorgonColor color, Vector2 thickness)
		{
			DrawRectangle(rectangle, color, thickness, null, RectangleF.Empty);
		}

		/// <summary>
		/// Function to draw a rectangle onto the current target.
		/// </summary>
		/// <param name="rectangle">Rectangle dimensions.</param>
		/// <param name="color">Color for the rectangle.</param>
		public void DrawRectangle(RectangleF rectangle, GorgonColor color)
		{
			DrawRectangle(rectangle, color, new Vector2(1.0f), null, RectangleF.Empty);
		}

		/// <summary>
		/// Function to draw a single point to the current target.
		/// </summary>
		/// <param name="position">Position of the point.</param>
		/// <param name="color">Color of the point.</param>
		/// <param name="thickness">Thickness of the point.</param>
		public void DrawPoint(Vector2 position, GorgonColor color, Vector2 thickness)
		{
			SetStates(_point);
			_point.Position = position;
			_point.PointThickness = thickness;
			_point.Color = color;
			_point.Draw();
		}

		/// <summary>
		/// Function to draw a single point to the current target.
		/// </summary>
		/// <param name="position">Position of the point.</param>
		/// <param name="color">Color of the point.</param>
		public void DrawPoint(Vector2 position, GorgonColor color)
		{
			DrawPoint(position, color, new Vector2(1.0f));
		}

		/// <summary>
		/// Function to draw a line to the current target.
		/// </summary>
		/// <param name="startPosition">Starting position.</param>
		/// <param name="endPosition">Ending position.</param>
		/// <param name="color">Color of the line.</param>
		/// <param name="thickness">Thickness of the line.</param>
		/// <param name="texture">Texture to apply to the line.</param>
		/// <param name="textureStart">Starting point on the texture.</param>
		/// <param name="textureEnd">Ending point on the texture.</param>
		public void DrawLine(Vector2 startPosition, Vector2 endPosition, GorgonColor color, Vector2 thickness, GorgonTexture2D texture, Vector2 textureStart, Vector2 textureEnd)
		{
			SetStates(_line);
			_line.StartPoint = startPosition;
			_line.EndPoint = endPosition;
			_line.Color = color;
			_line.LineThickness = thickness;
			_line.Texture = texture;
			_line.TextureStart = textureStart;
			_line.TextureEnd = textureEnd;
			_line.Draw();
		}

		/// <summary>
		/// Function to draw a line to the current target.
		/// </summary>
		/// <param name="startPosition">Starting position.</param>
		/// <param name="endPosition">Ending position.</param>
		/// <param name="color">Color of the line.</param>
		/// <param name="thickness">Thickness of the line.</param>
		/// <param name="texture">Texture to apply to the line.</param>
		public void DrawLine(Vector2 startPosition, Vector2 endPosition, GorgonColor color, Vector2 thickness, GorgonTexture2D texture)
		{
			DrawLine(startPosition, endPosition, color, thickness, texture, Vector2.Zero, new Vector2(1));
		}

		/// <summary>
		/// Function to draw a line to the current target.
		/// </summary>
		/// <param name="startPosition">Starting position.</param>
		/// <param name="endPosition">Ending position.</param>
		/// <param name="color">Color of the line.</param>
		/// <param name="thickness">Thickness of the line.</param>
		public void DrawLine(Vector2 startPosition, Vector2 endPosition, GorgonColor color, Vector2 thickness)
		{
			DrawLine(startPosition, endPosition, color, thickness, null, Vector2.Zero, Vector2.Zero);
		}

		/// <summary>
		/// Function to draw a line to the current target.
		/// </summary>
		/// <param name="startPosition">Starting position.</param>
		/// <param name="endPosition">Ending position.</param>
		/// <param name="color">Color of the line.</param>
		public void DrawLine(Vector2 startPosition, Vector2 endPosition, GorgonColor color)
		{
			DrawLine(startPosition, endPosition, color, new Vector2(1.0f), null, Vector2.Zero, Vector2.Zero);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDrawing"/> class.
		/// </summary>
		/// <param name="gorgon2D">The gorgon 2D interface that owns this object.</param>
		internal GorgonDrawing(Gorgon2D gorgon2D)
		{
			_depthStencil = new GorgonRenderable.DepthStencilStates();
			_sampler = new GorgonRenderable.TextureSamplerState();
			_blend = new GorgonRenderable.BlendState();

			// Default to modulated blending for drawing operations.
			BlendingMode = BlendingMode.Modulate;

			CullingMode = CullingMode.Back;

			_rect = new GorgonRectangle(gorgon2D, "Gorgon2D.Rectangle", false)
			{
				Position = Vector2.Zero,
				Size = Vector2.Zero
			};
		    _point = new GorgonPoint(gorgon2D, "Gorgon2D.Point")
		             {
		                 Position = Vector2.Zero,
		                 Color = GorgonColor.White
		             };
			_line = new GorgonLine(gorgon2D, "Gorgon2D.Line")
			{
				Color = GorgonColor.White,
    			StartPoint = Vector2.Zero,
	    		EndPoint = Vector2.Zero
			};
			_ellipse = new GorgonEllipse(gorgon2D, "Gorgon2D.Ellipse")
			           {
			               Quality = 64,
			               Color = GorgonColor.White
			           };
		    _triangle = new GorgonTriangle(gorgon2D,
		                                   "Gorgon2D.Triangle")
		                {
		                    IsFilled = false
		                };
			_string = gorgon2D.Renderables.CreateText("Gorgon2D.String");
		}
		#endregion
	}
}