﻿
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
// Created: August 19, 2018 9:25:19 AM
// 

using System.Numerics;
using Gorgon.Animation.Properties;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Renderers;

namespace Gorgon.Animation;

/// <summary>
/// A controller used to handle animations for a <see cref="GorgonTextSprite"/>
/// </summary>
/// <remarks>
/// <para>
/// This controller is an implementation of the <see cref="GorgonAnimationController{T}"/> type and is used apply animations to a <see cref="GorgonTextSprite"/>. 
/// </para>
/// <para>
/// A controller will update the <see cref="GorgonTextSprite"/> properties over a certain time frame (or continuously if looped) using a <see cref="IGorgonAnimation"/>
/// </para>
/// <para>
/// This controller will advance the time for an animation, and coordinate the changes from interpolation (if supported) between <see cref="IGorgonKeyFrame"/> items on a <see cref="IGorgonAnimationTrack{T}"/>
/// The values from the animation will then by applied to the object properties
/// </para>
/// <para>
/// This controller type contains registrations for the tracks corresponding the many of the properties on a <see cref="GorgonTextSprite"/>. These registrations are available to the developer as static 
/// values on the class, and these should be used to identify the track name (e.g. <c>Builder.EditVector2(GorgonTextSpriteAnimationController.PositionTrack.TrackName);</c>
/// </para>
/// </remarks>
/// <seealso cref="GorgonAnimationController{T}"/>
/// <seealso cref="IGorgonAnimation"/>
/// <seealso cref="GorgonTextSprite"/>
public class GorgonTextSpriteAnimationController
    : GorgonAnimationController<GorgonTextSprite>
{
    /// <summary>
    /// The name of the opacity track.
    /// </summary>
    public const string OpacityTrackName = "Opacity";

    /// <summary>
    /// The track registration for the angle of rotation for the sprite.
    /// </summary>
    public static readonly GorgonTrackRegistration AngleTrack = new(nameof(GorgonTextSprite.Angle), Resources.GORANM_TEXT_ANGLE, AnimationTrackKeyType.Single);
    /// <summary>
    /// The track registration for the depth value of the sprite.
    /// </summary>
    public static readonly GorgonTrackRegistration DepthTrack = new(nameof(GorgonTextSprite.Depth), Resources.GORANM_TEXT_DEPTH, AnimationTrackKeyType.Single);
    /// <summary>
    /// The track registration for the opacity of the sprite.
    /// </summary>
    public static readonly GorgonTrackRegistration OpacityTrack = new(OpacityTrackName, Resources.GORANM_TEXT_OPACITY, AnimationTrackKeyType.Single);

    /// <summary>
    /// The track registration for the position of the sprite.
    /// </summary>
    public static readonly GorgonTrackRegistration PositionTrack = new(nameof(GorgonTextSprite.Position), Resources.GORANM_TEXT_POSITION, AnimationTrackKeyType.Vector2);
    /// <summary>
    /// The track registration for the anchor point, in relative coordinates, of the sprite.
    /// </summary>
    public static readonly GorgonTrackRegistration AnchorTrack = new(nameof(GorgonTextSprite.Anchor), Resources.GORANM_TEXT_ANCHOR, AnimationTrackKeyType.Vector2);
    /// <summary>
    /// The track registration for the scale, using relative values, of the sprite.
    /// </summary>
    public static readonly GorgonTrackRegistration ScaleTrack = new(nameof(GorgonTextSprite.Scale), Resources.GORANM_TEXT_SCALE, AnimationTrackKeyType.Vector2);
    /// <summary>
    /// The track registration for the scale, using absolute values, of the sprite.
    /// </summary>
    public static readonly GorgonTrackRegistration ScaledSizeTrack = new(nameof(GorgonTextSprite.ScaledSize), Resources.GORANM_TEXT_SCALED_SIZE, AnimationTrackKeyType.Vector2);

    /// <summary>
    /// The track registration for the position, and depth of the sprite.
    /// </summary>
    public static readonly GorgonTrackRegistration Position3DTrack = new(nameof(GorgonTextSprite.Position), Resources.GORANM_TEXT_POSITION_3D, AnimationTrackKeyType.Vector3);

    /// <summary>
    /// The track registration for the color on a sprite.
    /// </summary>
    public static readonly GorgonTrackRegistration ColorTrack = new(nameof(GorgonTextSprite.Color), Resources.GORANM_TEXT_COLOR, AnimationTrackKeyType.Color);

    /// <summary>Function called when a single floating point value needs to be updated on the animated object.</summary>
    /// <param name="track">The track currently being processed.</param>
    /// <param name="animObject">The object to update.</param>
    /// <param name="value">The value to apply.</param>
    protected override void OnSingleValueUpdate(GorgonTrackRegistration track, GorgonTextSprite animObject, float value)
    {
        if (track.ID == AngleTrack.ID)
        {
            animObject.Angle = value;
            return;
        }

        if (track.ID == DepthTrack.ID)
        {
            animObject.Depth = value;
            return;
        }

        if (track.ID != OpacityTrack.ID)
        {
            return;
        }

        animObject.Color = new GorgonColor(animObject.Color, value);
    }

    /// <summary>Function called when a 2D vector value needs to be updated on the animated object.</summary>
    /// <param name="track">The track currently being processed.</param>
    /// <param name="animObject">The object to update.</param>
    /// <param name="value">The value to apply.</param>
    protected override void OnVector2ValueUpdate(GorgonTrackRegistration track, GorgonTextSprite animObject, Vector2 value)
    {
        if (track.ID == PositionTrack.ID)
        {
            animObject.Position = value;
            return;
        }

        if (track.ID == AnchorTrack.ID)
        {
            animObject.Anchor = value;
            return;
        }

        if (track.ID == ScaleTrack.ID)
        {
            animObject.Scale = value;
            return;
        }

        if (track.ID != ScaledSizeTrack.ID)
        {
            return;
        }

        animObject.ScaledSize = new Vector2(value.X, value.Y);
    }

    /// <summary>Function called when a 3D vector value needs to be updated on the animated object.</summary>
    /// <param name="track">The track currently being processed.</param>
    /// <param name="animObject">The object to update.</param>
    /// <param name="value">The value to apply.</param>
    protected override void OnVector3ValueUpdate(GorgonTrackRegistration track, GorgonTextSprite animObject, Vector3 value)
    {
        if (track.ID != Position3DTrack.ID)
        {
            return;
        }

        animObject.Position = new Vector2(value.X, value.Y);
        animObject.Depth = value.Z;
    }

    /// <summary>Function called when a 4D vector value needs to be updated on the animated object.</summary>
    /// <param name="track">The track currently being processed.</param>
    /// <param name="animObject">The object to update.</param>
    /// <param name="value">The value to apply.</param>
    protected override void OnVector4ValueUpdate(GorgonTrackRegistration track, GorgonTextSprite animObject, Vector4 value)
    {
        // Not needed for text sprites.
    }

    /// <summary>Function called when a <see cref="GorgonColor"/> value needs to be updated on the animated object.</summary>
    /// <param name="track">The track currently being processed.</param>
    /// <param name="animObject">The object to update.</param>
    /// <param name="value">The value to apply.</param>
    protected override void OnColorUpdate(GorgonTrackRegistration track, GorgonTextSprite animObject, GorgonColor value)
    {
        if (track.ID != ColorTrack.ID)
        {
            return;
        }

        animObject.Color = value;
    }

    /// <summary>Function called when a SharpDX <c>RectangleF</c> value needs to be updated on the animated object.</summary>
    /// <param name="track">The track currently being processed.</param>
    /// <param name="animObject">The object to update.</param>
    /// <param name="value">The value to apply.</param>
    protected override void OnRectangleUpdate(GorgonTrackRegistration track, GorgonTextSprite animObject, GorgonRectangleF value)
    {
        // Not needed for text sprites.
    }

    /// <summary>Function called when a texture needs to be updated on the object.</summary>
    /// <param name="track">The track currently being processed.</param>
    /// <param name="animObject">The object to update.</param>
    /// <param name="texture">The texture to switch to.</param>
    /// <param name="textureCoordinates">The new texture coordinates to apply.</param>
    /// <param name="textureArrayIndex">The texture array index.</param>
    protected override void OnTexture2DUpdate(GorgonTrackRegistration track, GorgonTextSprite animObject, GorgonTexture2DView texture, GorgonRectangleF textureCoordinates, int textureArrayIndex)
    {
        // Not needed for text sprites.
    }

    /// <summary>Function called when a Quaternion value needs to be updated on the animated object.</summary>
    /// <param name="track">The track currently being processed.</param>
    /// <param name="animObject">The object to update.</param>
    /// <param name="value">The value to apply.</param>
    protected override void OnQuaternionValueUpdate(GorgonTrackRegistration track, GorgonTextSprite animObject, Quaternion value)
    {
        // Not needed for text sprites.
    }

    /// <summary>Initializes a new instance of the <see cref="GorgonTextSpriteAnimationController"/> class.</summary>
    public GorgonTextSpriteAnimationController()
    {
        RegisterTrack(AngleTrack);
        RegisterTrack(DepthTrack);
        RegisterTrack(OpacityTrack);
        RegisterTrack(PositionTrack);
        RegisterTrack(AnchorTrack);
        RegisterTrack(ScaleTrack);
        RegisterTrack(ScaledSizeTrack);
        RegisterTrack(Position3DTrack);
        RegisterTrack(ColorTrack);
    }
}
