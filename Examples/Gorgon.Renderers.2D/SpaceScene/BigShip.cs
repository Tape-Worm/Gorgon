﻿
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
// Created: May 24, 2019 3:33:44 PM
// 

using System.Numerics;

namespace Gorgon.Examples;

/// <summary>
/// Another space ship object
/// </summary>
/// <remarks>
/// This is a prop space ship that we'll use to add more variety to the scene
/// </remarks>
/// <remarks>Initializes a new instance of the <see cref="BigShip"/> class.</remarks>
/// <param name="layer">The layer.</param>
internal class BigShip(SpritesLayer layer)
{

    // The ship sprite.
    private SpriteEntity _ship;
    // The ship sprite illumination map.
    private SpriteEntity _shipIllum;
    // The layer containing the sprite(s) for this ship.
    private readonly SpritesLayer _layer = layer;
    // The angle of rotation.
    private float _angle;
    // Position of the ship.
    private Vector2 _position;

    /// <summary>
    /// Property to set or return the angle of rotation, in degrees, for the ship.
    /// </summary>
    public float Angle
    {
        get => _angle;
        set
        {
            _angle = value;
            UpdateEntityPositions();
        }
    }

    /// <summary>
    /// Property to set or return the position of the ship, in space.
    /// </summary>
    public Vector2 Position
    {
        get => _position;
        set
        {
            _position = value;
            UpdateEntityPositions();
        }
    }

    /// <summary>
    /// Function to update the entity positions to match the position of the ship.
    /// </summary>
    private void UpdateEntityPositions()
    {
        if (_ship is null)
        {
            return;
        }

        _shipIllum.Rotation = _ship.Rotation = _angle;
        _shipIllum.LocalPosition = _ship.LocalPosition = _position;
    }

    /// <summary>
    /// Function to load the resources required for this ship.
    /// </summary>
    public void LoadResources()
    {
        // We'll need our sprite entities for this object so we can update their position and rotation.
        _ship = _layer.GetSpriteByName("BigShip");
        _shipIllum = _layer.GetSpriteByName("BigShip_Illum");

        _ship.Visible = true;
        _shipIllum.Visible = true;
        _ship.Scale = 4;
        _shipIllum.Scale = 4;
        UpdateEntityPositions();
    }
}
