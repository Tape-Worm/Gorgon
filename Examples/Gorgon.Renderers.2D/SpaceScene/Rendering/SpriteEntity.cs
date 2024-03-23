
// 
// Gorgon
// Copyright (C) 2019 Michael Winsor
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
// Created: May 21, 2019 11:43:27 AM
// 

using System.Numerics;
using Gorgon.Animation;
using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Renderers;

namespace Gorgon.Examples;

/// <summary>
/// An entity for a sprite layer
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="SpriteEntity"/> class.</remarks>
public class SpriteEntity
    : IGorgonNamedObject
{
    /// <inheritdoc/>
    public string Name
    {
        get;
    }

    /// <summary>
    /// Property to set or return the sprite attached to this entity.
    /// </summary>
    public GorgonSprite Sprite
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the scaling amount for the entity.
    /// </summary>
    public float Scale
    {
        get;
        set;
    } = 1.0f;

    /// <summary>
    /// Property to set or return the position of the entity.
    /// </summary>
    /// <remarks>
    /// This is the position of the entity in camera space, after transformation.
    /// </remarks>
    public Vector2 Position
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the local position of the entity.
    /// </summary>
    /// <remarks>
    /// This is the position of the entity in world space, prior to transformation.
    /// </remarks>
    public Vector2 LocalPosition
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the rotation of the entity.
    /// </summary>
    public float Rotation
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the anchor point for the entity.
    /// </summary>
    public Vector2 Anchor
    {
        get;
        set;
    } = new Vector2(0.5f, 0.5f);

    /// <summary>
    /// Property to set or return the color of the sprite.
    /// </summary>
    public GorgonColor Color
    {
        get;
        set;
    } = GorgonColors.White;

    /// <summary>
    /// Property to set or return whether the sprite is lit by deferred lighting.
    /// </summary>
    public bool IsLit
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the animation for the sprite.
    /// </summary>
    public IGorgonAnimation Animation
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the blending state to apply.
    /// </summary>
    public GorgonBlendState BlendState
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return whether this entity is visible or not.
    /// </summary>
    public bool Visible
    {
        get;
        set;
    } = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpriteEntity"/> class.
    /// </summary>
    /// <param name="name">The name of the entity.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
    public SpriteEntity(string name)
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentEmptyException(nameof(name));
        }

        Name = name;
    }
}
