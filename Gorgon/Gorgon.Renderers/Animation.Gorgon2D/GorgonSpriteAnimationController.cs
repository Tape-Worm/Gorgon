#region MIT
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: August 19, 2018 9:25:19 AM
// 
#endregion

using System.Numerics;
using Gorgon.Animation.Properties;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Renderers;
using DX = SharpDX;

namespace Gorgon.Animation;

/// <summary>
/// A controller used to handle animations for a <see cref="GorgonSprite"/>.
/// </summary>
/// <remarks>
/// <para>
/// This controller is an implementation of the <see cref="GorgonAnimationController{T}"/> type and is used apply animations to a <see cref="GorgonSprite"/>. 
/// </para>
/// <para>
/// A controller will update the <see cref="GorgonSprite"/> properties over a certain time frame (or continuously if looped) using a <see cref="IGorgonAnimation"/>.
/// </para>
/// <para>
/// This controller will advance the time for an animation, and coordinate the changes from interpolation (if supported) between <see cref="IGorgonKeyFrame"/> items on a <see cref="IGorgonAnimationTrack{T}"/>.
/// The values from the animation will then by applied to the object properties.
/// </para>
/// <para>
/// This controller type contains registrations for the tracks corresponding the many of the properties on a <see cref="GorgonSprite"/>. These registrations are available to the developer as static 
/// values on the class, and these should be used to identify the track name (e.g. <c>Builder.EditVector2(GorgonSpriteAnimationController.PositionTrack.TrackName);</c>.
/// </para>
/// </remarks>
/// <seealso cref="GorgonAnimationController{T}"/>
/// <seealso cref="IGorgonAnimation"/>
/// <seealso cref="GorgonSprite"/>
public class GorgonSpriteAnimationController
    : GorgonAnimationController<GorgonSprite>
{
    #region Constants.
    /// <summary>
    /// The name of the opacity track.
    /// </summary>
    public const string OpacityTrackName = "Opacity";
    /// <summary>
    /// The name of the upper left color track.
    /// </summary>
    public const string UpperLeftColorTrackName = "UpperLeftColor";
    /// <summary>
    /// The name of the upper right color track.
    /// </summary>
    public const string UpperRightColorTrackName = "UpperRightColor";
    /// <summary>
    /// The name of the lower left color track.
    /// </summary>
    public const string LowerLeftColorTrackName = "LowerLeftColor";
    /// <summary>
    /// The name of the lower right color track.
    /// </summary>
    public const string LowerRightColorTrackName = "LowerRightColor";
    /// <summary>
    /// The name of the 3D position track.
    /// </summary>
    public const string Position3DTrackName = nameof(GorgonSprite.Position) + "3D";
    /// <summary>
    /// The name of the upper left 3D position track.
    /// </summary>
    public const string UpperLeft3DTrackName = nameof(GorgonSprite.CornerOffsets.UpperLeft) + "3D";
    /// <summary>
    /// The name of the lower left 3D position track.
    /// </summary>
    public const string LowerLeft3DTrackName = nameof(GorgonSprite.CornerOffsets.LowerLeft) + "3D";
    /// <summary>
    /// The name of the upper right 3D position track.
    /// </summary>
    public const string UpperRight3DTrackName = nameof(GorgonSprite.CornerOffsets.UpperRight) + "3D";
    /// <summary>
    /// The name of the lower right 3D position track.
    /// </summary>
    public const string LowerRight3DTrackName = nameof(GorgonSprite.CornerOffsets.LowerRight) + "3D";
    #endregion

    #region Variables.
    /// <summary>
    /// The track registration for the angle of rotation for the sprite.
    /// </summary>
    public static readonly GorgonTrackRegistration AngleTrack = new(nameof(GorgonSprite.Angle), Resources.GORANM_TEXT_ANGLE, AnimationTrackKeyType.Single);
    /// <summary>
    /// The track registration for the depth value of the sprite.
    /// </summary>
    public static readonly GorgonTrackRegistration DepthTrack = new(nameof(GorgonSprite.Depth), Resources.GORANM_TEXT_DEPTH, AnimationTrackKeyType.Single);
    /// <summary>
    /// The track registration for the texture array index for the sprite.
    /// </summary>
    public static readonly GorgonTrackRegistration TextureArrayIndexTrack = new(nameof(GorgonSprite.TextureArrayIndex), Resources.GORANM_TEXT_TEXTURE_ARRAY_INDEX ,AnimationTrackKeyType.Single, TrackInterpolationMode.None);
    /// <summary>
    /// The track registration for the opacity of the sprite.
    /// </summary>
    public static readonly GorgonTrackRegistration OpacityTrack = new(OpacityTrackName, Resources.GORANM_TEXT_OPACITY, AnimationTrackKeyType.Single);

    /// <summary>
    /// The track registration for the position of the sprite.
    /// </summary>
    public static readonly GorgonTrackRegistration PositionTrack = new(nameof(GorgonSprite.Position), Resources.GORANM_TEXT_POSITION, AnimationTrackKeyType.Vector2);
    /// <summary>
    /// The track registration for the anchor point, in relative coordinates, of the sprite.
    /// </summary>
    public static readonly GorgonTrackRegistration AnchorTrack = new(nameof(GorgonSprite.Anchor), Resources.GORANM_TEXT_ANCHOR, AnimationTrackKeyType.Vector2);
    /// <summary>
    /// The track registration for the anchor point, in absolute coordinates, of the sprite.
    /// </summary>
    public static readonly GorgonTrackRegistration AbsoluteAnchorTrack = new(nameof(GorgonSprite.AbsoluteAnchor), Resources.GORANM_TEXT_ABSOLUTE_ANCHOR, AnimationTrackKeyType.Vector2);
    /// <summary>
    /// The track registration for the size of the sprite.
    /// </summary>
    public static readonly GorgonTrackRegistration SizeTrack = new(nameof(GorgonSprite.Size), Resources.GORANM_TEXT_SIZE, AnimationTrackKeyType.Vector2);
    /// <summary>
    /// The track registration for the scale, using relative values, of the sprite.
    /// </summary>
    public static readonly GorgonTrackRegistration ScaleTrack = new(nameof(GorgonSprite.Scale), Resources.GORANM_TEXT_SCALE, AnimationTrackKeyType.Vector2);
    /// <summary>
    /// The track registration for the scale, using absolute values, of the sprite.
    /// </summary>
    public static readonly GorgonTrackRegistration ScaledSizeTrack = new(nameof(GorgonSprite.ScaledSize), Resources.GORANM_TEXT_SCALED_SIZE, AnimationTrackKeyType.Vector2);
    /// <summary>
    /// The track registration for the position of the upper left corner of the sprite.
    /// </summary>
    public static readonly GorgonTrackRegistration UpperLeftPositionTrack = new(nameof(GorgonSprite.CornerOffsets.UpperLeft), Resources.GORANM_TEXT_UPPER_LEFT_OFFSET, AnimationTrackKeyType.Vector2);
    /// <summary>
    /// The track registration for the position of the upper right corner of the sprite.
    /// </summary>
    public static readonly GorgonTrackRegistration UpperRightPositionTrack = new(nameof(GorgonSprite.CornerOffsets.UpperRight), Resources.GORANM_TEXT_UPPER_RIGHT_OFFSET, AnimationTrackKeyType.Vector2);
    /// <summary>
    /// The track registration for the position of the lower left corner of the sprite.
    /// </summary>
    public static readonly GorgonTrackRegistration LowerLeftPositionTrack = new(nameof(GorgonSprite.CornerOffsets.LowerLeft), Resources.GORANM_TEXT_LOWER_LEFT_OFFSET, AnimationTrackKeyType.Vector2);
    /// <summary>
    /// The track registration for the position of the lower right corner of the sprite.
    /// </summary>
    public static readonly GorgonTrackRegistration LowerRightPositionTrack = new(nameof(GorgonSprite.CornerOffsets.LowerRight), Resources.GORANM_TEXT_LOWER_RIGHT_OFFSET, AnimationTrackKeyType.Vector2);

    /// <summary>
    /// The track registration for the position, and depth of the sprite.
    /// </summary>
    public static readonly GorgonTrackRegistration Position3DTrack = new(Position3DTrackName, Resources.GORANM_TEXT_POSITION_3D, AnimationTrackKeyType.Vector3);
    /// <summary>
    /// The track registration for the position, and depth of the upper left corner of the sprite.
    /// </summary>
    public static readonly GorgonTrackRegistration UpperLeftPosition3DTrack = new(UpperLeft3DTrackName, Resources.GORANM_TEXT_UPPER_LEFT_OFFSET_3D, AnimationTrackKeyType.Vector3);
    /// <summary>
    /// The track registration for the position, and depth of the upper right corner of the sprite.
    /// </summary>
    public static readonly GorgonTrackRegistration UpperRightPosition3DTrack = new(UpperRight3DTrackName, Resources.GORANM_TEXT_UPPER_RIGHT_OFFSET_3D, AnimationTrackKeyType.Vector3);
    /// <summary>
    /// The track registration for the position, and depth of the lower left corner of the sprite.
    /// </summary>
    public static readonly GorgonTrackRegistration LowerLeftPosition3DTrack = new(LowerLeft3DTrackName, Resources.GORANM_TEXT_LOWER_LEFT_OFFSET_3D, AnimationTrackKeyType.Vector3);
    /// <summary>
    /// The track registration for the position, and depth of the lower right corner of the sprite.
    /// </summary>
    public static readonly GorgonTrackRegistration LowerRightPosition3DTrack = new(LowerRight3DTrackName, Resources.GORANM_TEXT_LOWER_RIGHT_OFFSET_3D, AnimationTrackKeyType.Vector3);

    /// <summary>
    /// The track registration for the bounds of the sprite.
    /// </summary>
    public static readonly GorgonTrackRegistration BoundsTrack = new(nameof(GorgonSprite.Bounds), Resources.GORANM_TEXT_BOUNDS, AnimationTrackKeyType.Rectangle);
    /// <summary>
    /// The track registration for the texture coordinates for the sprite.
    /// </summary>
    public static readonly GorgonTrackRegistration TextureCoordinatesTrack = new(nameof(GorgonSprite.TextureRegion), Resources.GORANM_TEXT_TEXTURE_REGION, AnimationTrackKeyType.Rectangle, TrackInterpolationMode.None);

    /// <summary>
    /// The track registration for the color on a sprite.
    /// </summary>
    public static readonly GorgonTrackRegistration ColorTrack = new(nameof(GorgonSprite.Color), Resources.GORANM_TEXT_COLOR, AnimationTrackKeyType.Color);
    /// <summary>
    /// The track registration for the upper left corner color on a sprite.
    /// </summary>
    public static readonly GorgonTrackRegistration UpperLeftColorTrack = new(UpperLeftColorTrackName, Resources.GORANM_TEXT_UPPER_LEFT_COLOR, AnimationTrackKeyType.Color);
    /// <summary>
    /// The track registration for the upper right corner color on a sprite.
    /// </summary>
    public static readonly GorgonTrackRegistration UpperRightColorTrack = new(UpperRightColorTrackName, Resources.GORANM_TEXT_UPPER_RIGHT_COLOR, AnimationTrackKeyType.Color);
    /// <summary>
    /// The track registration for the lower left corner color on a sprite.
    /// </summary>
    public static readonly GorgonTrackRegistration LowerLeftColorTrack = new(LowerLeftColorTrackName, Resources.GORANM_TEXT_LOWER_LEFT_COLOR, AnimationTrackKeyType.Color);
    /// <summary>
    /// The track registration for the lower right corner color on a sprite.
    /// </summary>
    public static readonly GorgonTrackRegistration LowerRightColorTrack = new(LowerRightColorTrackName, Resources.GORANM_TEXT_LOWER_RIGHT_COLOR, AnimationTrackKeyType.Color);

    /// <summary>
    /// The track registration for the texture, texture coordinates, and texture array index on a sprite.
    /// </summary>
    public static readonly GorgonTrackRegistration TextureTrack = new(nameof(GorgonSprite.Texture), Resources.GORANM_TEXT_TEXTURE, AnimationTrackKeyType.Texture2D, TrackInterpolationMode.None);
    #endregion

    #region Methods.
    /// <summary>Function called when a single floating point value needs to be updated on the animated object.</summary>
    /// <param name="track">The track currently being processed.</param>
    /// <param name="animObject">The object to update.</param>
    /// <param name="value">The value to apply.</param>
    protected override void OnSingleValueUpdate(GorgonTrackRegistration track, GorgonSprite animObject, float value)
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

        if (track.ID == TextureArrayIndexTrack.ID)
        {
            animObject.TextureArrayIndex = (int)value;
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
    protected override void OnVector2ValueUpdate(GorgonTrackRegistration track, GorgonSprite animObject, Vector2 value)
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

        if (track.ID == AbsoluteAnchorTrack.ID)
        {
            animObject.AbsoluteAnchor = value;
            return;
        }

        if (track.ID == SizeTrack.ID)
        {
            animObject.Size = new DX.Size2F(value.X, value.Y);
            return;
        }

        if (track.ID == ScaleTrack.ID)
        {
            animObject.Scale = value;
            return;
        }

        if (track.ID == ScaledSizeTrack.ID)
        {
            animObject.ScaledSize = new DX.Size2F(value.X, value.Y);
            return;
        }

        if (track.ID == UpperLeftPositionTrack.ID)
        {
            animObject.CornerOffsets.UpperLeft = new Vector3(value, animObject.Depth);
            return;
        }

        if (track.ID == UpperRightPositionTrack.ID)
        {
            animObject.CornerOffsets.UpperRight = new Vector3(value, animObject.Depth);
            return;
        }

        if (track.ID == LowerLeftPositionTrack.ID)
        {
            animObject.CornerOffsets.LowerLeft = new Vector3(value, animObject.Depth);
            return;
        }

        if (track.ID != LowerRightPositionTrack.ID)
        {
            return;
        }

        animObject.CornerOffsets.LowerRight = new Vector3(value, animObject.Depth);
    }

    /// <summary>Function called when a 3D vector value needs to be updated on the animated object.</summary>
    /// <param name="track">The track currently being processed.</param>
    /// <param name="animObject">The object to update.</param>
    /// <param name="value">The value to apply.</param>
    protected override void OnVector3ValueUpdate(GorgonTrackRegistration track, GorgonSprite animObject, Vector3 value)
    {
        if (track.ID == Position3DTrack.ID)
        {
            animObject.Position = new Vector2(value.X, value.Y);
            animObject.Depth = value.Z;
            return;
        }

        if (track.ID == UpperLeftPosition3DTrack.ID)
        {
            animObject.CornerOffsets.UpperLeft = value;
            return;
        }

        if (track.ID == UpperRightPosition3DTrack.ID)
        {
            animObject.CornerOffsets.UpperRight = value;
            return;
        }

        if (track.ID == LowerLeftPosition3DTrack.ID)
        {
            animObject.CornerOffsets.LowerLeft = value;
            return;
        }

        if (track.ID != LowerRightPosition3DTrack.ID)
        {
            return;
        }

        animObject.CornerOffsets.LowerRight = value;
    }

    /// <summary>Function called when a 4D vector value needs to be updated on the animated object.</summary>
    /// <param name="track">The track currently being processed.</param>
    /// <param name="animObject">The object to update.</param>
    /// <param name="value">The value to apply.</param>
    protected override void OnVector4ValueUpdate(GorgonTrackRegistration track, GorgonSprite animObject, Vector4 value)
    {
        // Not needed for sprites.
    }

    /// <summary>Function called when a SharpDX <c>RectangleF</c> value needs to be updated on the animated object.</summary>
    /// <param name="track">The track currently being processed.</param>
    /// <param name="animObject">The object to update.</param>
    /// <param name="value">The value to apply.</param>
    protected override void OnRectangleUpdate(GorgonTrackRegistration track, GorgonSprite animObject, DX.RectangleF value)
    {
        if (track.ID == BoundsTrack.ID)
        {
            animObject.Bounds = value;
            return;
        }

        if (track.ID != TextureCoordinatesTrack.ID)
        {
            return;
        }

        animObject.TextureRegion = value;
    }

    /// <summary>Function called when a <see cref="GorgonColor"/> value needs to be updated on the animated object.</summary>
    /// <param name="track">The track currently being processed.</param>
    /// <param name="animObject">The object to update.</param>
    /// <param name="value">The value to apply.</param>
    protected override void OnColorUpdate(GorgonTrackRegistration track, GorgonSprite animObject, GorgonColor value)
    {
        if (track.ID == ColorTrack.ID)
        {
            animObject.Color = value;
            return;
        }

        if (track.ID == UpperLeftColorTrack.ID)
        {
            animObject.CornerColors.UpperLeft = value;
            return;
        }

        if (track.ID == UpperRightColorTrack.ID)
        {
            animObject.CornerColors.UpperRight = value;
            return;
        }

        if (track.ID == LowerLeftColorTrack.ID)
        {
            animObject.CornerColors.LowerLeft = value;
            return;
        }

        if (track.ID != LowerRightColorTrack.ID)
        {
            return;
        }

        animObject.CornerColors.LowerRight = value;
    }

    /// <summary>Function called when a texture needs to be updated on the object.</summary>
    /// <param name="track">The track currently being processed.</param>
    /// <param name="animObject">The object to update.</param>
    /// <param name="texture">The texture to switch to.</param>
    /// <param name="textureCoordinates">The new texture coordinates to apply.</param>
    /// <param name="textureArrayIndex">The texture array index.</param>
    protected override void OnTexture2DUpdate(GorgonTrackRegistration track, GorgonSprite animObject, GorgonTexture2DView texture, DX.RectangleF textureCoordinates, int textureArrayIndex)
    {
        if (track.ID != TextureTrack.ID)
        {                
            return;
        }

        animObject.Texture = texture;
        animObject.TextureRegion = textureCoordinates;
        animObject.TextureArrayIndex = textureArrayIndex;
    }

    /// <summary>Function called when a Quaternion value needs to be updated on the animated object.</summary>
    /// <param name="track">The track currently being processed.</param>
    /// <param name="animObject">The object to update.</param>
    /// <param name="value">The value to apply.</param>
    protected override void OnQuaternionValueUpdate(GorgonTrackRegistration track, GorgonSprite animObject, Quaternion value)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="GorgonSpriteAnimationController"/> class.</summary>
    public GorgonSpriteAnimationController()
    {
        RegisterTrack(AngleTrack);
        RegisterTrack(DepthTrack);
        RegisterTrack(TextureArrayIndexTrack);
        RegisterTrack(OpacityTrack);
        RegisterTrack(PositionTrack);
        RegisterTrack(AnchorTrack);
        RegisterTrack(AbsoluteAnchorTrack);
        RegisterTrack(SizeTrack);
        RegisterTrack(ScaleTrack);
        RegisterTrack(ScaledSizeTrack);
        RegisterTrack(UpperLeftPositionTrack);
        RegisterTrack(UpperRightPositionTrack);
        RegisterTrack(LowerLeftPositionTrack);
        RegisterTrack(LowerRightPositionTrack);
        RegisterTrack(Position3DTrack);
        RegisterTrack(UpperLeftPosition3DTrack);
        RegisterTrack(UpperRightPosition3DTrack);
        RegisterTrack(LowerLeftPosition3DTrack);
        RegisterTrack(LowerRightPosition3DTrack);
        RegisterTrack(TextureCoordinatesTrack);
        RegisterTrack(BoundsTrack);
        RegisterTrack(ColorTrack);
        RegisterTrack(UpperLeftColorTrack);
        RegisterTrack(UpperRightColorTrack);
        RegisterTrack(LowerLeftColorTrack);
        RegisterTrack(LowerRightColorTrack);
        RegisterTrack(TextureTrack);
    }
    #endregion
}
