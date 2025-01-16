
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
// Created: February 7, 2020 3:36:57 PM
// 

using System.Numerics;
using Gorgon.Animation;
using Gorgon.Editor.UI;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Renderers.Cameras;

namespace Gorgon.Editor.Rendering;

/// <summary>
/// Provides animation capability for camera actions
/// </summary>
internal class CameraAnimationController<T>
    : GorgonAnimationController<GorgonOrthoCamera>
    where T : class, IVisualEditorContent
{
    /// <summary>
    /// The track registration for the position.
    /// </summary>
    public static readonly GorgonTrackRegistration PositionTrack = new(nameof(GorgonOrthoCamera.Position), null, AnimationTrackKeyType.Vector3);
    /// <summary>
    /// The track registration for the scale..
    /// </summary>
    public static readonly GorgonTrackRegistration ZoomTrack = new(nameof(GorgonOrthoCamera.Zoom), null, AnimationTrackKeyType.Vector2);

    // The renderer that is animating.
    private readonly DefaultContentRenderer<T> _renderer;

    /// <summary>Function called when a <see cref="GorgonColor"/> value needs to be updated on the animated object.</summary>
    /// <param name="track">The track currently being processed.</param>
    /// <param name="animObject">The object to update.</param>
    /// <param name="value">The value to apply.</param>
    protected override void OnColorUpdate(GorgonTrackRegistration track, GorgonOrthoCamera animObject, GorgonColor value)
    {
        // Not needed.
    }

    /// <summary>Function called when a SharpDX <c>RectangleF</c> value needs to be updated on the animated object.</summary>
    /// <param name="track">The track currently being processed.</param>
    /// <param name="animObject">The object to update.</param>
    /// <param name="value">The value to apply.</param>
    protected override void OnRectangleUpdate(GorgonTrackRegistration track, GorgonOrthoCamera animObject, GorgonRectangleF value)
    {
        // Not needed.
    }

    /// <summary>Function called when a single floating point value needs to be updated on the animated object.</summary>
    /// <param name="track">The track currently being processed.</param>
    /// <param name="animObject">The object to update.</param>
    /// <param name="value">The value to apply.</param>
    protected override void OnSingleValueUpdate(GorgonTrackRegistration track, GorgonOrthoCamera animObject, float value)
    {
        // Not needed.
    }

    /// <summary>Function called when a texture needs to be updated on the object.</summary>
    /// <param name="track">The track currently being processed.</param>
    /// <param name="animObject">The object to update.</param>
    /// <param name="texture">The texture to switch to.</param>
    /// <param name="textureCoordinates">The new texture coordinates to apply.</param>
    /// <param name="textureArrayIndex">The texture array index.</param>
    protected override void OnTexture2DUpdate(GorgonTrackRegistration track, GorgonOrthoCamera animObject, GorgonTexture2DView texture, GorgonRectangleF textureCoordinates, int textureArrayIndex)
    {
        // Not needed.
    }

    /// <summary>Function called when a 2D vector value needs to be updated on the animated object.</summary>
    /// <param name="track">The track currently being processed.</param>
    /// <param name="animObject">The object to update.</param>
    /// <param name="value">The value to apply.</param>
    protected override void OnVector2ValueUpdate(GorgonTrackRegistration track, GorgonOrthoCamera animObject, Vector2 value)
    {
        if (track.ID != ZoomTrack.ID)
        {
            return;
        }

        animObject.Zoom = value;
        _renderer.OnZoom();
    }

    /// <summary>Function called when a 3D vector value needs to be updated on the animated object.</summary>
    /// <param name="track">The track currently being processed.</param>
    /// <param name="animObject">The object to update.</param>
    /// <param name="value">The value to apply.</param>
    protected override void OnVector3ValueUpdate(GorgonTrackRegistration track, GorgonOrthoCamera animObject, Vector3 value)
    {
        if (track.ID != PositionTrack.ID)
        {
            return;
        }

        animObject.Position = value;
        _renderer.OnOffset();
    }

    /// <summary>Function called when a 4D vector value needs to be updated on the animated object.</summary>
    /// <param name="track">The track currently being processed.</param>
    /// <param name="animObject">The object to update.</param>
    /// <param name="value">The value to apply.</param>
    protected override void OnVector4ValueUpdate(GorgonTrackRegistration track, GorgonOrthoCamera animObject, Vector4 value)
    {
        // Not needed.
    }

    /// <summary>Function called when a Quaternion value needs to be updated on the animated object.</summary>
    /// <param name="track">The track currently being processed.</param>
    /// <param name="animObject">The object to update.</param>
    /// <param name="value">The value to apply.</param>
    protected override void OnQuaternionValueUpdate(GorgonTrackRegistration track, GorgonOrthoCamera animObject, Quaternion value)
    {
        // Not needed.
    }

    /// <summary>Initializes a new instance of the <see cref="CameraAnimationController{T}"/> class.</summary>
    public CameraAnimationController(DefaultContentRenderer<T> renderer)
    {
        _renderer = renderer;

        RegisterTrack(ZoomTrack);
        RegisterTrack(PositionTrack);
    }
}
