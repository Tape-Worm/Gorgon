﻿
// 
// Gorgon
// Copyright (C) 2020 Michael Winsor
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
// Created: February 9, 2020 6:58:59 PM
// 

using System.Numerics;
using Gorgon.Animation;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;

namespace Gorgon.Editor.ImageEditor;

/// <summary>
/// An animation controller for the image view
/// </summary>
internal class ImageAnimationController
    : GorgonAnimationController<ITextureViewer>
{
    // The track used to animate opacity for the texture.
    private static readonly GorgonTrackRegistration _opacityTrack = new(nameof(ITextureViewer.Opacity), nameof(ITextureViewer.Opacity), AnimationTrackKeyType.Single);

    /// <summary>Function called when a single floating point value needs to be updated on the animated object.</summary>
    /// <param name="track">The track currently being processed.</param>
    /// <param name="animObject">The object to update.</param>
    /// <param name="value">The value to apply.</param>
    protected override void OnSingleValueUpdate(GorgonTrackRegistration track, ITextureViewer animObject, float value)
    {
        if (_opacityTrack.ID != track.ID)
        {
            return;
        }

        animObject.Opacity = value;
    }

    /// <summary>Function called when a 2D vector value needs to be updated on the animated object.</summary>
    /// <param name="track">The track currently being processed.</param>
    /// <param name="animObject">The object to update.</param>
    /// <param name="value">The value to apply.</param>
    protected override void OnVector2ValueUpdate(GorgonTrackRegistration track, ITextureViewer animObject, Vector2 value)
    {
    }

    /// <summary>Function called when a 3D vector value needs to be updated on the animated object.</summary>
    /// <param name="track">The track currently being processed.</param>
    /// <param name="animObject">The object to update.</param>
    /// <param name="value">The value to apply.</param>
    protected override void OnVector3ValueUpdate(GorgonTrackRegistration track, ITextureViewer animObject, Vector3 value)
    {
    }

    /// <summary>Function called when a 4D vector value needs to be updated on the animated object.</summary>
    /// <param name="track">The track currently being processed.</param>
    /// <param name="animObject">The object to update.</param>
    /// <param name="value">The value to apply.</param>
    protected override void OnVector4ValueUpdate(GorgonTrackRegistration track, ITextureViewer animObject, Vector4 value)
    {
    }

    /// <summary>Function called when a <see cref="GorgonColor"/> value needs to be updated on the animated object.</summary>
    /// <param name="track">The track currently being processed.</param>
    /// <param name="animObject">The object to update.</param>
    /// <param name="value">The value to apply.</param>
    protected override void OnColorUpdate(GorgonTrackRegistration track, ITextureViewer animObject, GorgonColor value)
    {
    }

    /// <summary>Function called when a SharpDX <c>RectangleF</c> value needs to be updated on the animated object.</summary>
    /// <param name="track">The track currently being processed.</param>
    /// <param name="animObject">The object to update.</param>
    /// <param name="value">The value to apply.</param>
    protected override void OnRectangleUpdate(GorgonTrackRegistration track, ITextureViewer animObject, GorgonRectangleF value)
    {
    }

    /// <summary>Function called when a texture needs to be updated on the object.</summary>
    /// <param name="track">The track currently being processed.</param>
    /// <param name="animObject">The object to update.</param>
    /// <param name="texture">The texture to switch to.</param>
    /// <param name="textureCoordinates">The new texture coordinates to apply.</param>
    /// <param name="textureArrayIndex">The texture array index.</param>
    protected override void OnTexture2DUpdate(GorgonTrackRegistration track, ITextureViewer animObject, GorgonTexture2DView texture, GorgonRectangleF textureCoordinates, int textureArrayIndex)
    {
    }

    /// <summary>Function called when a Quaternion value needs to be updated on the animated object.</summary>
    /// <param name="track">The track currently being processed.</param>
    /// <param name="animObject">The object to update.</param>
    /// <param name="value">The value to apply.</param>
    protected override void OnQuaternionValueUpdate(GorgonTrackRegistration track, ITextureViewer animObject, Quaternion value)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="ImageAnimationController"/> class.</summary>
    public ImageAnimationController() => RegisterTrack(_opacityTrack);
}
