
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
// Created: August 9, 2018 8:45:09 AM
// 


using System.Collections;
using System.Numerics;
using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using Gorgon.Patterns;
using Gorgon.Renderers.Geometry;
using Gorgon.Renderers.Properties;
using GorgonTriangulator;

namespace Gorgon.Renderers;

/// <summary>
/// A builder used to create a new <see cref="GorgonPolySprite"/> object
/// </summary>
/// <remarks>
/// <para>
/// Because the <see cref="GorgonPolySprite"/> cannot be created using the <see langword="new"/> keyword, this builder is used to build <see cref="GorgonPolySprite"/> objects. 
/// </para>
/// <para>
/// The polygonal sprite created by this builder is built up by adding multiple <see cref="GorgonPolySpriteVertex"/> objects to the builder and calling the <see cref="Build"/> method. These vertices 
/// make up the "hull" of the polygon (basically the outer shape of the polygon), which gets turned into triangles so it can be rendered by Gorgon. Vertex manipulation is not the only functionality 
/// provided by the builder, other initial values may also be assigned for the created sprite as well
/// <para>
/// <note type="important">
/// A minimum of 3 vertices are required to build a polygonal sprite. Attempting to do so with less will cause an exception
/// </note>
/// </para>
/// </para>
/// <para>
/// This builder is not the only way to create a polygonal sprite, users can define a series of triangle vertices and indices and use 
/// <see cref="GorgonPolySprite.Create(GorgonGraphics, IReadOnlyList{GorgonPolySpriteVertex}, IReadOnlyList{int})"/> on the <see cref="GorgonPolySprite"/>
/// </para>
/// <para>
/// The resulting polygonal sprite from this builder implements <see cref="IDisposable"/>. Therefore, it is the user's responsibility to dispose of the object when they are done with it
/// </para>
/// </remarks>
/// <seealso cref="GorgonPolySpriteVertex"/>
/// <seealso cref="GorgonPolySprite"/>
/// <remarks>
/// Initializes a new instance of the <see cref="GorgonPolySpriteBuilder" /> class
/// </remarks>
/// <param name="renderer">The renderer interface to use for building the polygon sprite.</param>
public class GorgonPolySpriteBuilder(Gorgon2D renderer)
        : IGorgonFluentBuilder<GorgonPolySpriteBuilder, GorgonPolySprite>, IEnumerable<GorgonPolySpriteVertex>, IGorgonGraphicsObject
{

    // The working sprite.
    private readonly GorgonPolySprite _workingSprite = new();
    // The triangulator used to convert the polygon into a triangle mesh.
    private readonly Triangulator _triangulator = new(null);



    /// <summary>
    /// Property to return the number of vertices in the polysprite.
    /// </summary>
    public int VertexCount => _workingSprite.RwVertices.Count;

    /// <summary>
    /// Property to return the graphics interface that built this object.
    /// </summary>
    public GorgonGraphics Graphics
    {
        get;
    } = renderer?.Graphics ?? throw new ArgumentNullException(nameof(renderer));



    /// <summary>
    /// Function to copy the values from one sprite to another.
    /// </summary>
    /// <param name="dest">The destination sprite that will receive the sprite data.</param>
    /// <param name="src">The source sprite that will have its data copied.</param>
    private static void CopySprite(GorgonPolySprite dest, GorgonPolySprite src)
    {
        dest.RwVertices.Clear();
        dest.RwIndices = [];

        for (int i = 0; i < src.RwVertices.Count; ++i)
        {
            dest.RwVertices.Add(src.RwVertices[i]);
        }

        dest.Position = src.Position;
        dest.AlphaTest = src.AlphaTest;
        dest.Anchor = src.Anchor;
        dest.Angle = src.Angle;
        dest.Bounds = src.Bounds;
        dest.Color = src.Color;
        dest.Depth = src.Depth;
        dest.HorizontalFlip = src.HorizontalFlip;
        dest.VerticalFlip = src.VerticalFlip;
        dest.Scale = src.Scale;
        dest.Texture = src.Texture;
        dest.TextureSampler = src.TextureSampler;
        dest.TextureOffset = src.TextureOffset;
        dest.TextureScale = src.TextureScale;
        dest.TextureArrayIndex = src.TextureArrayIndex;
    }

    /// <summary>
    /// Function to assign the inital alpha test value for the sprite.
    /// </summary>
    /// <param name="alphaTest">[Optional] The alpha test value to assign.</param>
    /// <returns>The fluent interface for this builder.</returns>
    /// <remarks>
    /// <para>
    /// If the <paramref name="alphaTest"/> value is <b>null</b>, then alpha testing will be turned off for the sprite.
    /// </para>
    /// </remarks>
    public GorgonPolySpriteBuilder AlphaTest(GorgonRange<float>? alphaTest = null)
    {
        _workingSprite.AlphaTest = alphaTest;
        return this;
    }

    /// <summary>
    /// Function to remove multiple vertices from the polysprite.
    /// </summary>
    /// <param name="start">The starting index to use.</param>
    /// <param name="count">The number of indices to remove.</param>
    /// <returns>The fluent interface for this builder.</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown when the <paramref name="start"/> is less than 0, or greater than/equal to <see cref="VertexCount"/>.</exception>
    public GorgonPolySpriteBuilder RemoveVertices(int start, int count)
    {
        if ((start < 0) || (start >= VertexCount))
        {
            throw new IndexOutOfRangeException();
        }

        if ((start + count) >= VertexCount)
        {
            // Crop to the highest available index.
            count = VertexCount - start;
        }

        _workingSprite.RwVertices.RemoveRange(start, count);
        return this;
    }

    /// <summary>
    /// Function to remove a vertex from the polysprite.
    /// </summary>
    /// <param name="vertex">The vertex to remove.</param>
    /// <returns>The fluent interface for this builder.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="vertex"/> is <b>null</b>.</exception>
    public GorgonPolySpriteBuilder RemoveVertex(GorgonPolySpriteVertex vertex)
    {
        if (vertex is null)
        {
            throw new ArgumentNullException(nameof(vertex));
        }

        _workingSprite.RwVertices.Remove(vertex);
        return this;
    }

    /// <summary>
    /// Function to remove a vertex from the polysprite.
    /// </summary>
    /// <param name="index">The index of the vertex to remove.</param>
    /// <returns>The fluent interface for this builder.</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown when the <paramref name="index"/> is less than 0, or equal to/greater than the <see cref="VertexCount"/>.</exception>
    public GorgonPolySpriteBuilder RemoveVertex(int index)
    {
        if ((index < 0) || (index >= VertexCount))
        {
            throw new IndexOutOfRangeException();
        }

        _workingSprite.RwVertices.RemoveAt(index);
        return this;
    }

    /// <summary>
    /// Function to move the specified vertex to a new location in the list.
    /// </summary>
    /// <param name="vertex">The vertex to move.</param>
    /// <param name="newIndex">The new index for the vertex.</param>
    /// <returns>The fluent interface for the this builder.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="vertex"/> parameter is <b>null</b>.</exception>
    public GorgonPolySpriteBuilder MoveVertex(GorgonPolySpriteVertex vertex, int newIndex)
    {
        if (vertex is null)
        {
            throw new ArgumentNullException(nameof(vertex));
        }

        int index = _workingSprite.RwVertices.IndexOf(vertex);

        return index == -1 ? this : MoveVertex(index, newIndex);
    }

    /// <summary>
    /// Function to move the vertex at the specified index to a new location in the list.
    /// </summary>
    /// <param name="oldIndex">The index of the vertex to move.</param>
    /// <param name="newIndex">The new index for the vertex.</param>
    /// <returns>The fluent interface for the this builder.</returns>
    public GorgonPolySpriteBuilder MoveVertex(int oldIndex, int newIndex)
    {
        oldIndex = oldIndex.Max(0).Min(_workingSprite.RwVertices.Count - 1);
        newIndex = newIndex.Max(0).Min(_workingSprite.RwVertices.Count);

        if (newIndex == oldIndex)
        {
            return this;
        }

        GorgonPolySpriteVertex pass = _workingSprite.RwVertices[oldIndex];

        _workingSprite.RwVertices[oldIndex] = null;
        _workingSprite.RwVertices.Insert(newIndex, pass);
        _workingSprite.RwVertices.Remove(null);

        return this;
    }

    /// <summary>
    /// Function to insert a vertex at the specified index.
    /// </summary>
    /// <param name="index">The index to insert into.</param>
    /// <param name="vertex">The vertex to insert.</param>
    /// <returns>The fluent interface for the this builder.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name=" vertex"/> parameter is <b>null</b>.</exception>
    public GorgonPolySpriteBuilder InsertVertex(int index, GorgonPolySpriteVertex vertex)
    {
        if (vertex is null)
        {
            throw new ArgumentNullException(nameof(vertex));
        }

        _workingSprite.RwVertices.Insert(index, vertex);
        return this;
    }

    /// <summary>
    /// Function to insert multiple vertices at the specified index.
    /// </summary>
    /// <param name="index">The index to insert into.</param>
    /// <param name="vertices">The vertices to insert.</param>
    /// <returns>The fluent interface for the this builder.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name=" vertices"/> parameter is <b>null</b>.</exception>
    public GorgonPolySpriteBuilder InsertVertices(int index, IEnumerable<GorgonPolySpriteVertex> vertices)
    {
        if (vertices is null)
        {
            throw new ArgumentNullException(nameof(vertices));
        }

        _workingSprite.RwVertices.InsertRange(index, vertices);
        return this;
    }

    /// <summary>
    /// Function to add multiple vertices to the polysprite.
    /// </summary>
    /// <param name="vertices">The vertices to add.</param>
    /// <returns>The fluent interface for this builder.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="vertices"/> is <b>null</b>.</exception>
    public GorgonPolySpriteBuilder AddVertices(IEnumerable<GorgonPolySpriteVertex> vertices)
    {
        if (vertices is null)
        {
            throw new ArgumentNullException(nameof(vertices));
        }

        _workingSprite.RwVertices.AddRange(vertices);
        return this;
    }

    /// <summary>
    /// Function to add a vertex to the polysprite.
    /// </summary>
    /// <param name="vertex">The vertex to add.</param>
    /// <returns>The fluent interface for this builder.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="vertex"/> is <b>null</b>.</exception>
    public GorgonPolySpriteBuilder AddVertex(GorgonPolySpriteVertex vertex)
    {
        if (vertex is null)
        {
            throw new ArgumentNullException(nameof(vertex));
        }

        _workingSprite.RwVertices.Add(vertex);
        return this;
    }

    /// <summary>
    /// Function to assign a texture to the polygon sprite.
    /// </summary>
    /// <param name="texture">The texture to assign to the sprite.</param>
    /// <returns>The fluent interface for this builder.</returns>
    public GorgonPolySpriteBuilder Texture(GorgonTexture2DView texture)
    {
        _workingSprite.Texture = texture;
        return this;
    }

    /// <summary>
    /// Function to assign a texture sampler to the polygon sprite.
    /// </summary>
    /// <param name="sampler">The sampler to assign to the sprite.</param>
    /// <returns>The fluent interface for this builder.</returns>
    public GorgonPolySpriteBuilder TextureSampler(GorgonSamplerState sampler)
    {
        _workingSprite.TextureSampler = sampler;
        return this;
    }

    /// <summary>
    /// Function to assign a texture coordinate to a vertex at the specified index.
    /// </summary>
    /// <param name="index">The index of the vertex to update.</param>
    /// <param name="textureCoordinate">The texture coordinate to assign.</param>
    /// <returns>The fluent interface for this builder.</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown when the <paramref name="index"/> is less than 0, or equal to/greater than the <see cref="VertexCount"/>.</exception>
    public GorgonPolySpriteBuilder TextureCoordinate(int index, Vector2 textureCoordinate)
    {
        if ((index < 0) || (index >= VertexCount))
        {
            throw new IndexOutOfRangeException();
        }

        _workingSprite.RwVertices[index].TextureCoordinate = textureCoordinate;
        return this;
    }

    /// <summary>
    /// Function to assign an array index for a texture array to the polygon sprite.
    /// </summary>
    /// <param name="textureArrayIndex">The texture array index to assign.</param>
    /// <returns>The fluent interface for this builder.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="textureArrayIndex"/> is less than 0, or greater than or equal to <see cref="IGorgonVideoAdapterInfo.MaxTextureArrayCount"/>.</exception>
    public GorgonPolySpriteBuilder TextureArrayIndex(int textureArrayIndex)
    {
        if ((textureArrayIndex < 0) || (textureArrayIndex >= Graphics.VideoAdapter.MaxTextureArrayCount))
        {
            throw new ArgumentOutOfRangeException(nameof(textureArrayIndex));
        }

        _workingSprite.TextureArrayIndex = textureArrayIndex;
        return this;
    }

    /// <summary>
    /// Function to assign the transformation to apply to a texture on the sprite.
    /// </summary>
    /// <param name="textureOffset">The translation amount to apply to the UV coordinates of the vertices in the polygon sprite.</param>
    /// <param name="textureScale">The scale amount to apply to the UV coordinates of the vertices in the polygon sprite.</param>
    /// <returns>The fluent interface for this builder.</returns>
    public GorgonPolySpriteBuilder TextureTransform(Vector2 textureOffset, Vector2 textureScale)
    {
        _workingSprite.TextureOffset = textureOffset;
        _workingSprite.TextureScale = textureScale;
        return this;
    }

    /// <summary>
    /// Function to flip the texture coordinates for the polygon sprite.
    /// </summary>
    /// <param name="horizontal">[Optional] <b>true</b> to flip horizontally.</param>
    /// <param name="vertical">[Optional] <b>true</b> to flip vertically.</param>
    /// <returns>The fluent interface for this builder.</returns>
    public GorgonPolySpriteBuilder Flip(bool horizontal = false, bool vertical = false)
    {
        _workingSprite.HorizontalFlip = horizontal;
        _workingSprite.VerticalFlip = vertical;
        return this;
    }

    /// <summary>
    /// Function to assign the color for the polygon sprite.
    /// </summary>
    /// <param name="color">The color to assign.</param>
    /// <returns>The fluent interface for this builder.</returns>
    public GorgonPolySpriteBuilder Color(GorgonColor color)
    {
        _workingSprite.Color = color;
        return this;
    }

    /// <summary>
    /// Function to assign anchor value to the polygon sprite.
    /// </summary>
    /// <param name="anchor">The anchor for the sprite.</param>
    /// <returns>The fluent interface for this builder.</returns>
    /// <remarks>
    /// <para>
    /// This value is in relative unit coordinates.  For example, 0.5, 0.5 would mean the center of the sprite.
    /// </para>
    /// </remarks>
    public GorgonPolySpriteBuilder Anchor(Vector2 anchor)
    {
        _workingSprite.Anchor = anchor;
        return this;
    }

    /// <summary>
    /// Function to assign an initial position to the polygon sprite.
    /// </summary>
    /// <param name="position">The position of the sprite.</param>
    /// <param name="depth">[Optional] A depth value for the sprite.</param>
    /// <returns>The fluent interface for this builder.</returns>
    public GorgonPolySpriteBuilder Position(Vector2 position, float depth = 0)
    {
        _workingSprite.Position = position;
        _workingSprite.Depth = depth;
        return this;
    }

    /// <summary>
    /// Function to assign an initial scale to the polygon sprite.
    /// </summary>
    /// <param name="scale">The scale to assign to the polygon sprite.</param>
    /// <returns>The fluent interface for this builder.</returns>
    public GorgonPolySpriteBuilder Scale(Vector2 scale)
    {
        _workingSprite.Scale = scale;
        return this;
    }

    /// <summary>
    /// Function to assign an initial rotation value to the polygon sprite.
    /// </summary>
    /// <param name="degrees">The angle, in degrees, to rotate.</param>
    /// <returns>The fluent interface for this builder.</returns>
    public GorgonPolySpriteBuilder Angle(float degrees)
    {
        _workingSprite.Angle = degrees;
        return this;
    }

    /// <summary>
    /// Function to return the object.
    /// </summary>
    /// <returns>The object created or updated by this builder.</returns>
    /// <exception cref="GorgonException">Thrown if the polygonal sprite has less than 3 vertices.</exception>
    /// <remarks>
    /// <para>
    /// This will return a <see cref="GorgonPolySprite"/> for use with the <see cref="Gorgon2D.DrawPolygonSprite"/> method. The object returned implements <see cref="IDisposable"/>, so it is the
    /// responsibility of the user to dispose of the object when they are done with it.
    /// </para>
    /// <para>
    /// <note type="warning">
    /// <para>
    /// A polygon sprite must have a minimum of 3 vertices.  If it does not, then this method will throw an exception.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonPolySprite"/>
    /// <seealso cref="Gorgon2D"/>
    public GorgonPolySprite Build()
    {
        if (_workingSprite.RwVertices.Count < 3)
        {
            throw new GorgonException(GorgonResult.CannotCreate, Resources.GOR2D_ERR_POLY_SPRITE_NOT_ENOUGH_VERTS);
        }

        GorgonPolySprite newSprite = new();

        CopySprite(newSprite, _workingSprite);

        newSprite.Renderable.ActualVertexCount = newSprite.RwVertices.Count;
        if ((newSprite.Renderable.Vertices is null) || (newSprite.Renderable.Vertices.Length < newSprite.RwVertices.Count))
        {
            newSprite.Renderable.Vertices = new Gorgon2DVertex[newSprite.RwVertices.Count];
        }

        for (int i = 0; i < newSprite.RwVertices.Count; ++i)
        {
            newSprite.Renderable.Vertices[i] = newSprite.RwVertices[i].Vertex;
        }

        // Enforce clockwise ordering.
        _triangulator.EnsureWindingOrder(newSprite.Renderable.Vertices, WindingOrder.CounterClockwise);

        // Split the polygon hull into triangles.
        (int[] indices, GorgonRectangleF bounds) = _triangulator.Triangulate(newSprite.Renderable.Vertices, WindingOrder.CounterClockwise);

        newSprite.RwIndices = indices;

        newSprite.Renderable.IndexBuffer = new GorgonIndexBuffer(Graphics, new GorgonIndexBufferInfo(indices.Length)
        {
            Binding = VertexIndexBufferBinding.None,
            Usage = ResourceUsage.Immutable
        }, indices);

        newSprite.Renderable.VertexBuffer = GorgonVertexBufferBinding.CreateVertexBuffer<Gorgon2DVertex>(Graphics, new GorgonVertexBufferInfo(newSprite.Renderable.Vertices.Length * Gorgon2DVertex.SizeInBytes)
        {
            Usage = ResourceUsage.Immutable,
            Binding = VertexIndexBufferBinding.None
        }, newSprite.Renderable.Vertices);
        newSprite.Renderable.ActualVertexCount = newSprite.RwVertices.Count;
        newSprite.Renderable.IndexCount = indices.Length;
        newSprite.Bounds = new GorgonRectangleF(newSprite.Position.X, newSprite.Position.Y, bounds.Width, bounds.Height);

        return newSprite;
    }

    /// <summary>
    /// Function to clear the builder to a default state.
    /// </summary>
    /// <returns>The fluent builder interface.</returns>
    public GorgonPolySpriteBuilder Clear()
    {
        _workingSprite.RwVertices.Clear();
        _workingSprite.Renderable.ActualVertexCount = 0;
        _workingSprite.Renderable.IndexCount = 0;
        _workingSprite.Position = Vector2.Zero;
        _workingSprite.AlphaTest = GorgonRange<float>.Empty;
        _workingSprite.Anchor = Vector2.Zero;
        _workingSprite.Angle = 0.0f;
        _workingSprite.Bounds = GorgonRectangleF.Empty;
        _workingSprite.Color = GorgonColors.White;
        _workingSprite.Depth = 0.0f;
        _workingSprite.HorizontalFlip = false;
        _workingSprite.VerticalFlip = false;
        _workingSprite.Scale = Vector2.One;
        _workingSprite.Texture = null;
        _workingSprite.TextureSampler = null;
        return this;
    }

    /// <summary>
    /// Function to reset the builder to the specified object state.
    /// </summary>
    /// <param name="builderObject">[Optional] The specified object state to copy.</param>
    /// <returns>The fluent builder interface.</returns>
    public GorgonPolySpriteBuilder ResetTo(GorgonPolySprite builderObject = null)
    {
        if (builderObject is null)
        {
            Clear();
            return this;
        }

        CopySprite(_workingSprite, builderObject);
        return this;
    }

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the collection.</returns>
    public IEnumerator<GorgonPolySpriteVertex> GetEnumerator() => _workingSprite.RwVertices.GetEnumerator();

    /// <summary>
    /// Returns an enumerator that iterates through a collection.
    /// </summary>
    /// <returns>An <see cref="IEnumerator" /> object that can be used to iterate through the collection.</returns>
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_workingSprite.RwVertices).GetEnumerator();




}
