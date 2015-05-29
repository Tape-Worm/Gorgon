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
// Created: Sunday, February 26, 2012 9:33:35 PM
// 
#endregion

using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Renderers.Properties;
using SlimMath;

namespace Gorgon.Renderers
{
	/// <summary>
	/// Interface for renderable objects.
	/// </summary>
	/// <remarks>This interface is used to create renderable objects such as sprites and primitives.
	/// <para>Some primitives can be created as filled or unfilled, but each of those primitives have an IsFilled property which can toggle the fill state of the primitive.</para>
	/// </remarks>
	public class GorgonRenderables
	{
		#region Variables.
		private readonly Gorgon2D _gorgon2D;			// Gorgon 2D interface.
	    private readonly Gorgon2DVertexCache _cache;    // Gorgon vertex cache.
        #endregion

		#region Methods.
		/// <summary>
		/// Function to load a renderable from a stream.
		/// </summary>
		/// <typeparam name="T">Type of renderable to load.</typeparam>
		/// <param name="name">The name of the renderable object.</param>
		/// <param name="stream">Stream that contains the renderable.</param>
		/// <returns>A new renderable object of the specified type.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream" /> parameter is NULL (Nothing in VB.Net).
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="name"/> parameter is NULL.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is empty.</exception>
		/// <exception cref="System.IO.IOException">Thrown when the stream parameter is not opened for reading data.</exception>
		/// <exception cref="GorgonException">Thrown when the data in the stream does not contain valid renderable data, or contains a newer version of the renderable than Gorgon can handle.</exception>
		public T FromStream<T>(string name, Stream stream)
			where T : class, IPersistedRenderable
		{
			Type type = typeof(T);

		    if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(Resources.GOR2D_PARAMETER_MUST_NOT_BE_EMPTY, "name");
            }

		    var result = (T)(Activator.CreateInstance(type,
		                BindingFlags.NonPublic | BindingFlags.Instance,
		                null,
		                new object[]
		                {
		                    _gorgon2D,
		                    name
		                },
		                null));

			result.Load(stream);

			return result;
		}

		/// <summary>
		/// Function to load a renderable from memory.
		/// </summary>
		/// <typeparam name="T">Type of renderable to load.</typeparam>
		/// <param name="name">The name of the renderable object.</param>
		/// <param name="data">Array of bytes containing the renderable data to load.</param>
		/// <returns>A new renderable object of the specified type.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="data" /> parameter is NULL (Nothing in VB.Net).
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="name"/> parameter is NULL.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is empty.</exception>
		/// <exception cref="System.IO.IOException">Thrown when the stream parameter is not opened for reading data.</exception>
		/// <exception cref="GorgonException">Thrown when the data in the stream does not contain valid renderable data, or contains a newer version of the renderable than Gorgon can handle.</exception>
		public T FromMemory<T>(string name, byte[] data)
			where T : class, IPersistedRenderable
		{
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
			
			using (var stream = new MemoryStream(data))
			{
				return FromStream<T>(name, stream);
			}
		}

		/// <summary>
		/// Function to load a renderable from a file.
		/// </summary>
		/// <typeparam name="T">Type of renderable to load.</typeparam>
		/// <param name="name">The name of the renderable object.</param>
		/// <param name="filePath">The path to the file to load.</param>
		/// <returns>A new renderable object of the specified type.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="filePath" /> parameter is NULL (Nothing in VB.Net).
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="name"/> parameter is NULL.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is empty.
		/// <para>-or-</para>
		/// <para>Thrown when the filePath parameter is empty.</para>
		/// </exception>
		/// <exception cref="System.IO.IOException">Thrown when the stream parameter is not opened for reading data.</exception>
		/// <exception cref="GorgonException">Thrown when the data in the stream does not contain valid renderable data, or contains a newer version of the renderable than Gorgon can handle.</exception>
		public T FromFile<T>(string name, string filePath)
			where T : class, IPersistedRenderable
		{
            if (filePath == null)
            {
                throw new ArgumentNullException("filePath");
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException(Resources.GOR2D_PARAMETER_MUST_NOT_BE_EMPTY, "filePath");
            }

			using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				return FromStream<T>(name, stream);
			}
		}

		/// <summary>
		/// Function to create a new text object.
		/// </summary>
		/// <param name="name">Name of the text object.</param>
		/// <returns>A new text object.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.</exception>
		public GorgonText CreateText(string name)
		{
			return CreateText(name, null, string.Empty, Color.White, false, new Vector2(1), 0.25f);
		}

		/// <summary>
		/// Function to create a new text object.
		/// </summary>
		/// <param name="name">Name of the text object.</param>
		/// <param name="font">Font to use for the text.</param>
		/// <param name="text">Initial text to display.</param>
		/// <returns>A new text object.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.</exception>
		public GorgonText CreateText(string name, GorgonFont font, string text)
		{
			return CreateText(name, font, text, Color.White, false, new Vector2(1), 0.25f);
		}

		/// <summary>
		/// Function to create a new text object.
		/// </summary>
		/// <param name="name">Name of the text object.</param>
		/// <param name="font">Font to use for the text.</param>
		/// <param name="text">Initial text to display.</param>
		/// <param name="color">Color of the text.</param>
		/// <returns>A new text object.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.</exception>
		public GorgonText CreateText(string name, GorgonFont font, string text, GorgonColor color)
		{
			return CreateText(name, font, text, color, false, new Vector2(1), 0.25f);
		}

		/// <summary>
		/// Function to create a new text object.
		/// </summary>
		/// <param name="name">Name of the text object.</param>
		/// <param name="font">Font to use for the text.</param>
		/// <param name="text">Initial text to display.</param>
		/// <param name="color">Color of the text.</param>
		/// <param name="shadowed"><c>true</c> to place a shadow behind the text, <c>false</c> to display normally.</param>
		/// <param name="shadowOffset">Offset of the shadow.</param>
		/// <param name="shadowOpacity">Opacity for the shadow.</param>
		/// <returns>A new text object.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.</exception>
		public GorgonText CreateText(string name, GorgonFont font, string text, GorgonColor color, bool shadowed, Vector2 shadowOffset, float shadowOpacity)
		{
		    if (name == null)
		    {
		        throw new ArgumentNullException("name");
		    }

		    if (string.IsNullOrWhiteSpace(name))
		    {
		        throw new ArgumentException(Resources.GOR2D_PARAMETER_MUST_NOT_BE_EMPTY, "name");
		    }

			var result = new GorgonText(_gorgon2D, _cache, name, font)
			{
			    ShadowEnabled = shadowed,
			    ShadowOffset = shadowOffset,
			    ShadowOpacity = shadowOpacity,
			    Color = color,
			    Text = text,
				CullingMode = CullingMode.Back,
				AlphaTestValues = GorgonRangeF.Empty,
				BlendingMode = BlendingMode.Modulate
			};

		    return result;
		}

		/// <summary>
		/// Function to create a new sprite object.
		/// </summary>
		/// <param name="name">Name of the sprite.</param>
		/// <param name="settings">Settings for the sprite.</param>
		/// <returns>A new sprite.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.</exception>
		public GorgonSprite CreateSprite(string name, GorgonSpriteSettings settings)
		{
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(Resources.GOR2D_PARAMETER_MUST_NOT_BE_EMPTY, "name");
            }

			return new GorgonSprite(_gorgon2D, name)
			       {
						Size = settings.Size,
						Color = settings.Color,
						Angle = settings.InitialAngle,
						Scale = settings.InitialScale,
						Position = settings.InitialPosition,
						Texture = settings.Texture,
						TextureRegion = settings.TextureRegion,
						Anchor = settings.Anchor
			       };
		}

		/// <summary>
		/// Function to create a new sprite object.
		/// </summary>
		/// <param name="name">Name of the sprite.</param>
		/// <param name="size">Size of the sprite.</param>
		/// <param name="color">Color for the sprite.</param>
		/// <returns>A new sprite.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.</exception>
		public GorgonSprite CreateSprite(string name, Vector2 size, GorgonColor color)
		{
			return CreateSprite(name, new GorgonSpriteSettings
			{
				Color = color,
				InitialScale = new Vector2(1.0f),
				Size = size				
			});
		}

		/// <summary>
		/// Function to create a new sprite object.
		/// </summary>
		/// <param name="name">Name of the sprite.</param>
		/// <param name="size">Size of the sprite.</param>
		/// <param name="texture">Texture to apply to the sprite.</param>
		/// <param name="textureRegion">Region of the texture to map to the sprite.</param>
		/// <returns>A new sprite.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.</exception>
		public GorgonSprite CreateSprite(string name, Vector2 size, GorgonTexture2D texture, RectangleF textureRegion)
		{
		    if (texture == null)
		    {
		        return CreateSprite(name, size, Color.White);
		    }

		    return CreateSprite(name, new GorgonSpriteSettings
			{
				Color = Color.White,
				Size = size,
				InitialScale = new Vector2(1.0f),
				Texture = texture,
				TextureRegion = textureRegion
			});
		}

		/// <summary>
		/// Function to create a new sprite object.
		/// </summary>
		/// <param name="name">Name of the sprite.</param>
		/// <param name="size">Size of the sprite.</param>
		/// <param name="texture">Texture to apply to the sprite.</param>
		/// <param name="textureOffset">Offset into the texture to start mapping at.</param>
		/// <returns>A new sprite.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.</exception>
		public GorgonSprite CreateSprite(string name, Vector2 size, GorgonTexture2D texture, Vector2 textureOffset)
		{
			Vector2 texelSize = Vector2.Zero;

		    if (texture != null)
		    {
		        texelSize = texture.ToTexel(size);
		    }

		    return CreateSprite(name, size, texture, new RectangleF(textureOffset, texelSize));
		}

		/// <summary>
		/// Function to create a new sprite object.
		/// </summary>
		/// <param name="name">Name of the sprite.</param>
		/// <param name="size">Size of the sprite.</param>
		/// <param name="texture">Texture to apply to the sprite.</param>
		/// <returns>A new sprite.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.</exception>
		public GorgonSprite CreateSprite(string name, Vector2 size, GorgonTexture2D texture)
		{
			Vector2 texelSize = Vector2.Zero;

		    if (texture != null)
		    {
		        texelSize = texture.ToTexel(size);
		    }

		    return CreateSprite(name, size, texture, new RectangleF(Vector2.Zero, texelSize));
		}		

		/// <summary>
		/// Function to create a new triangle object.
		/// </summary>
		/// <param name="name">Name of the triangle.</param>
		/// <param name="point1">First point in the triangle.</param>
		/// <param name="point2">Second point in the triangle.</param>
		/// <param name="point3">Third point in the triangle.</param>
		/// <param name="filled"><c>true</c> to create a filled triangle, <c>false</c> to create an unfilled triangle.</param>
		/// <returns>A new triangle primitive object.</returns>
		/// <remarks>The points defined in the triangle use relative coordinates, and are offset from an origin that is defined by the <see cref="P:GorgonLibrary.Renderers.GorgonTriangle.Anchor">Anchor</see> property.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.</exception>
        public GorgonTriangle CreateTriangle(string name, GorgonPolygonPoint point1, GorgonPolygonPoint point2, GorgonPolygonPoint point3, bool filled)
		{
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(Resources.GOR2D_PARAMETER_MUST_NOT_BE_EMPTY, "name");
            }

		    var result = new GorgonTriangle(_gorgon2D, name)
		                 {
		                     IsFilled = filled
		                 };

            result.SetPoint(0, point1);
            result.SetPoint(1, point2);
            result.SetPoint(2, point3);

		    return result;
		}

		/// <summary>
		/// Function to create a rectangle object.
		/// </summary>
		/// <param name="name">Name of the rectangle.</param>
		/// <param name="rectangle">Rectangle dimensions.</param>
		/// <param name="color">Color of the rectangle.</param>
		/// <param name="filled"><c>true</c> to create a filled rectangle, <c>false</c> to create an empty rectangle.</param>
		/// <returns>A new rectangle primitive object.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.</exception>
		public GorgonRectangle CreateRectangle(string name, RectangleF rectangle, GorgonColor color, bool filled)
		{
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(Resources.GOR2D_PARAMETER_MUST_NOT_BE_EMPTY, "name");
            }

            return new GorgonRectangle(_gorgon2D, name, filled)
			{
				TextureRegion = rectangle,
				Color = color,
				Size = new Vector2(rectangle.Width, rectangle.Height),
				Position = new Vector2(rectangle.Left, rectangle.Top)
			};
		}

		/// <summary>
		/// Function to create a rectangle object.
		/// </summary>
		/// <param name="name">Name of the rectangle.</param>
		/// <param name="rectangle">Rectangle dimensions.</param>
		/// <param name="color">Color of the rectangle.</param>
		/// <returns>A new rectangle primitive object.</returns>
		/// <remarks>This method will create an unfilled rectangle.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.</exception>
		public GorgonRectangle CreateRectangle(string name, RectangleF rectangle, GorgonColor color)
		{
			return CreateRectangle(name, rectangle, color, false);
		}

		/// <summary>
		/// Function to create a line object.
		/// </summary>
		/// <param name="name">Name of the line.</param>
		/// <param name="startPosition">Starting point for the line.</param>
		/// <param name="endPosition">Ending point for the line.</param>
		/// <param name="color">Color of the line.</param>
		/// <returns>A new line primitive object.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.</exception>
		public GorgonLine CreateLine(string name, Vector2 startPosition, Vector2 endPosition, GorgonColor color)
		{
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(Resources.GOR2D_PARAMETER_MUST_NOT_BE_EMPTY, "name");
            }

			return new GorgonLine(_gorgon2D, name)
						{
							Color = color,
                            TextureEnd = endPosition,
                            StartPoint = startPosition,
                            EndPoint = endPosition
						};
		}

		/// <summary>
		/// Function to create a point object.
		/// </summary>
		/// <param name="name">Name of the point.</param>
		/// <param name="position">The position of the point.</param>
		/// <param name="color">Color of the point.</param>
		/// <returns>A new point primitive object.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.</exception>
		public GorgonPoint CreatePoint(string name, Vector2 position, GorgonColor color)
		{
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(Resources.GOR2D_PARAMETER_MUST_NOT_BE_EMPTY, "name");
            }

		    return new GorgonPoint(_gorgon2D, name)
		           {
		               Position = position,
		               Color = color
		           };
		}

		/// <summary>
		/// Function to create an ellipse object.
		/// </summary>
		/// <param name="name">Name of the ellipse.</param>
		/// <param name="position">Position of the ellipse.</param>
		/// <param name="size">Size of the ellipse.</param>
		/// <param name="color">Color of the ellipse.</param>
		/// <param name="isFilled"><c>true</c> if the ellipse should be filled, <c>false</c> if not.</param>
		/// <param name="quality">Quality of the ellipse rendering.</param>
		/// <returns>A new ellipse object.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.</exception>
		public GorgonEllipse CreateEllipse(string name, Vector2 position, Vector2 size, GorgonColor color, bool isFilled, int quality)
		{
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(Resources.GOR2D_PARAMETER_MUST_NOT_BE_EMPTY, "name");
            }

		    return new GorgonEllipse(_gorgon2D, name)
		           {
		               Quality = quality,
		               Anchor = size / 2.0f,
		               Position = position,
		               Size = size,
		               Color = color,
		               IsFilled = isFilled
		           };
		}

		/// <summary>
		/// Function to create an ellipse object.
		/// </summary>
		/// <param name="name">Name of the ellipse.</param>
		/// <param name="position">Position of the ellipse.</param>
		/// <param name="size">Size of the ellipse.</param>
		/// <param name="color">Color of the ellipse.</param>
		/// <param name="isFilled"><c>true</c> if the ellipse should be filled, <c>false</c> if not.</param>
		/// <returns>A new ellipse object.</returns>
		/// <remarks>This method creates an ellipse with a quality of 64 segments.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.</exception>
		public GorgonEllipse CreateEllipse(string name, Vector2 position, Vector2 size, GorgonColor color, bool isFilled)
		{
			return CreateEllipse(name, position, size, color, isFilled, 64);
		}

		/// <summary>
		/// Function to create an ellipse object.
		/// </summary>
		/// <param name="name">Name of the ellipse.</param>
		/// <param name="position">Position of the ellipse.</param>
		/// <param name="size">Size of the ellipse.</param>
		/// <param name="color">Color of the ellipse.</param>
		/// <returns>A new ellipse object.</returns>
		/// <remarks>This method creates an unfilled ellipse with a quality of 64 segments.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.</exception>
		public GorgonEllipse CreateEllipse(string name, Vector2 position, Vector2 size, GorgonColor color)
		{
			return CreateEllipse(name, position, size, color, false, 64);
		}

		/// <summary>
		/// Function to create a polygon object.
		/// </summary>
		/// <param name="name">Name of the polygon.</param>
		/// <param name="position">Position of the polygon.</param>
		/// <param name="color">Color of the polygon.</param>
		/// <param name="vertices">[Optional] Vertices for the polygon.</param>
		/// <param name="indices">[Optional] Indices for the polygon.</param>
		/// <param name="type">[Optional] The type of primitive to use when drawing the polygon.</param>
		/// <param name="useDynamicVertexBuffer">[Optional] <c>true</c> to use a dynamic buffer to hold the vertices, <c>false</c> to use a static buffer.</param>
		/// <param name="useDynamicIndexBuffer">[Optional] <c>true</c> to use a dynamic buffer to hold the indices, <c>false</c> to use a static buffer.</param>
		/// <returns>A new polygon object.</returns>
		/// <remarks>Passing NULL (Nothing in VB.Net) to the <paramref name="indices"/> parameter will disable the index buffer on the polygon.  An index buffer can help with performance by 
		/// decreasing the required number of vertices to be sent to the video device.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net). </exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
		public GorgonPolygon CreatePolygon(string name,
			Vector2 position,
			GorgonColor color,
			GorgonPolygonPoint[] vertices = null,
			int[] indices = null,
			PolygonType type = PolygonType.Triangle,
			bool useDynamicVertexBuffer = false,
			bool useDynamicIndexBuffer = false)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}

			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException(Resources.GOR2D_PARAMETER_MUST_NOT_BE_EMPTY, "name");
			}

			var result = new GorgonPolygon(_gorgon2D, name, type)
			             {
			                 UseDynamicVertexBuffer = useDynamicVertexBuffer,
			                 UseDynamicIndexBuffer = useDynamicIndexBuffer,
			                 Color = color,
			                 Position = position
			             };

		    if (vertices != null)
			{
				result.SetVertexData(vertices);
			}

			if (indices != null)
			{
				result.SetIndexData(indices);
			}

			_gorgon2D.TrackedObjects.Add(result);

			return result;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonRenderables"/> class.
		/// </summary>
		/// <param name="gorgon2D">The gorgon 2D interface that owns this object.</param>
		/// <param name="cache">The vertex cache used by the renderer.</param>
		internal GorgonRenderables(Gorgon2D gorgon2D, Gorgon2DVertexCache cache)
		{
			_gorgon2D = gorgon2D;
		    _cache = cache;
		}
		#endregion
	}
}
