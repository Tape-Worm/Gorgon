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

using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Renderers;
using DX = SharpDX;


namespace Gorgon.Animation
{
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
    /// Applications can force the playing animation to jump to a specific <see cref="GorgonAnimationController{T}.Time"/>, or increment the time step smoothly using the
    /// <see cref="GorgonAnimationController{T}.Update"/> method.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonAnimationController{T}"/>
    /// <seealso cref="IGorgonAnimation"/>
    /// <seealso cref="GorgonSprite"/>
    public class GorgonSpriteAnimationController
        : GorgonAnimationController<GorgonSprite>
    {
        #region Variables.
        /// <summary>
        /// The track registration for the angle of rotation for the sprite.
        /// </summary>
        public static readonly GorgonTrackRegistration AngleTrack = new GorgonTrackRegistration(nameof(GorgonSprite.Angle), AnimationTrackKeyType.Single);
        /// <summary>
        /// The track registration for the depth value of the sprite.
        /// </summary>
        public static readonly GorgonTrackRegistration DepthTrack = new GorgonTrackRegistration(nameof(GorgonSprite.Depth), AnimationTrackKeyType.Single);
        /// <summary>
        /// The track registration for the texture array index for the sprite.
        /// </summary>
        public static readonly GorgonTrackRegistration TextureArrayIndexTrack = new GorgonTrackRegistration(nameof(GorgonSprite.TextureArrayIndex), AnimationTrackKeyType.Single);
        /// <summary>
        /// The track registration for the opacity of the sprite.
        /// </summary>
        public static readonly GorgonTrackRegistration OpacityTrack = new GorgonTrackRegistration("Opacity", AnimationTrackKeyType.Single);

        /// <summary>
        /// The track registration for the position of the sprite.
        /// </summary>
        public static readonly GorgonTrackRegistration PositionTrack = new GorgonTrackRegistration(nameof(GorgonSprite.Position), AnimationTrackKeyType.Vector2);
        /// <summary>
        /// The track registration for the anchor point, in relative coordinates, of the sprite.
        /// </summary>
        public static readonly GorgonTrackRegistration AnchorTrack = new GorgonTrackRegistration(nameof(GorgonSprite.Anchor), AnimationTrackKeyType.Vector2);
        /// <summary>
        /// The track registration for the anchor point, in absolute coordinates, of the sprite.
        /// </summary>
        public static readonly GorgonTrackRegistration AbsoluteAnchorTrack = new GorgonTrackRegistration(nameof(GorgonSprite.AbsoluteAnchor), AnimationTrackKeyType.Vector2);
        /// <summary>
        /// The track registration for the size of the sprite.
        /// </summary>
        public static readonly GorgonTrackRegistration SizeTrack = new GorgonTrackRegistration(nameof(GorgonSprite.Size), AnimationTrackKeyType.Vector2);
        /// <summary>
        /// The track registration for the scale, using relative values, of the sprite.
        /// </summary>
        public static readonly GorgonTrackRegistration ScaleTrack = new GorgonTrackRegistration(nameof(GorgonSprite.Scale), AnimationTrackKeyType.Vector2);
        /// <summary>
        /// The track registration for the scale, using absolute values, of the sprite.
        /// </summary>
        public static readonly GorgonTrackRegistration ScaledSizeTrack = new GorgonTrackRegistration(nameof(GorgonSprite.ScaledSize), AnimationTrackKeyType.Vector2);
        /// <summary>
        /// The track registration for the position of the upper left corner of the sprite.
        /// </summary>
        public static readonly GorgonTrackRegistration UpperLeftPositionTrack = new GorgonTrackRegistration(nameof(GorgonSprite.CornerOffsets.UpperLeft), AnimationTrackKeyType.Vector2);
        /// <summary>
        /// The track registration for the position of the upper right corner of the sprite.
        /// </summary>
        public static readonly GorgonTrackRegistration UpperRightPositionTrack = new GorgonTrackRegistration(nameof(GorgonSprite.CornerOffsets.UpperRight), AnimationTrackKeyType.Vector2);
        /// <summary>
        /// The track registration for the position of the lower left corner of the sprite.
        /// </summary>
        public static readonly GorgonTrackRegistration LowerLeftPositionTrack = new GorgonTrackRegistration(nameof(GorgonSprite.CornerOffsets.LowerLeft), AnimationTrackKeyType.Vector2);
        /// <summary>
        /// The track registration for the position of the lower right corner of the sprite.
        /// </summary>
        public static readonly GorgonTrackRegistration LowerRightPositionTrack = new GorgonTrackRegistration(nameof(GorgonSprite.CornerOffsets.LowerRight), AnimationTrackKeyType.Vector2);

        /// <summary>
        /// The track registration for the position, and depth of the sprite.
        /// </summary>
        public static readonly GorgonTrackRegistration Position3DTrack = new GorgonTrackRegistration(nameof(GorgonSprite.Position), AnimationTrackKeyType.Vector3);
        /// <summary>
        /// The track registration for the position, and depth of the upper left corner of the sprite.
        /// </summary>
        public static readonly GorgonTrackRegistration UpperLeftPosition3DTrack = new GorgonTrackRegistration(nameof(GorgonSprite.CornerOffsets.UpperLeft), AnimationTrackKeyType.Vector3);
        /// <summary>
        /// The track registration for the position, and depth of the upper right corner of the sprite.
        /// </summary>
        public static readonly GorgonTrackRegistration UpperRightPosition3DTrack = new GorgonTrackRegistration(nameof(GorgonSprite.CornerOffsets.UpperRight), AnimationTrackKeyType.Vector3);
        /// <summary>
        /// The track registration for the position, and depth of the lower left corner of the sprite.
        /// </summary>
        public static readonly GorgonTrackRegistration LowerLeftPosition3DTrack = new GorgonTrackRegistration(nameof(GorgonSprite.CornerOffsets.LowerLeft), AnimationTrackKeyType.Vector3);
        /// <summary>
        /// The track registration for the position, and depth of the lower right corner of the sprite.
        /// </summary>
        public static readonly GorgonTrackRegistration LowerRightPosition3DTrack = new GorgonTrackRegistration(nameof(GorgonSprite.CornerOffsets.LowerRight), AnimationTrackKeyType.Vector3);

        /// <summary>
        /// The track registration for the bounds of the sprite.
        /// </summary>
        public static readonly GorgonTrackRegistration BoundsTrack = new GorgonTrackRegistration(nameof(GorgonSprite.Bounds), AnimationTrackKeyType.Rectangle);
        /// <summary>
        /// The track registration for the texture coordinates for the sprite.
        /// </summary>
        public static readonly GorgonTrackRegistration TextureCoordinatesTrack = new GorgonTrackRegistration(nameof(GorgonSprite.TextureRegion), AnimationTrackKeyType.Rectangle);

        /// <summary>
        /// The track registration for the color on a sprite.
        /// </summary>
        public static readonly GorgonTrackRegistration ColorTrack = new GorgonTrackRegistration(nameof(GorgonSprite.Color), AnimationTrackKeyType.Color);
        /// <summary>
        /// The track registration for the upper left corner color on a sprite.
        /// </summary>
        public static readonly GorgonTrackRegistration UpperLeftColorTrack = new GorgonTrackRegistration(nameof(GorgonSprite.CornerColors.UpperLeft), AnimationTrackKeyType.Color);
        /// <summary>
        /// The track registration for the upper right corner color on a sprite.
        /// </summary>
        public static readonly GorgonTrackRegistration UpperRightColorTrack = new GorgonTrackRegistration(nameof(GorgonSprite.CornerColors.UpperRight), AnimationTrackKeyType.Color);
        /// <summary>
        /// The track registration for the lower left corner color on a sprite.
        /// </summary>
        public static readonly GorgonTrackRegistration LowerLeftColorTrack = new GorgonTrackRegistration(nameof(GorgonSprite.CornerColors.LowerLeft), AnimationTrackKeyType.Color);
        /// <summary>
        /// The track registration for the lower right corner color on a sprite.
        /// </summary>
        public static readonly GorgonTrackRegistration LowerRightColorTrack = new GorgonTrackRegistration(nameof(GorgonSprite.CornerColors.LowerRight), AnimationTrackKeyType.Color);

        /// <summary>
        /// The track registration for the texture, texture coordinates, and texture array index on a sprite.
        /// </summary>
        public static readonly GorgonTrackRegistration TextureTrack = new GorgonTrackRegistration(nameof(GorgonSprite.Texture), AnimationTrackKeyType.Texture2D);
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
        protected override void OnVector2ValueUpdate(GorgonTrackRegistration track, GorgonSprite animObject, DX.Vector2 value)
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
                animObject.CornerOffsets.UpperLeft = new DX.Vector3(value, animObject.Depth);
                return;
            }

            if (track.ID == UpperRightPositionTrack.ID)
            {
                animObject.CornerOffsets.UpperRight = new DX.Vector3(value, animObject.Depth);
                return;
            }

            if (track.ID == LowerLeftPositionTrack.ID)
            {
                animObject.CornerOffsets.LowerLeft = new DX.Vector3(value, animObject.Depth);
                return;
            }

            if (track.ID != LowerRightPositionTrack.ID)
            {
                return;
            }

            animObject.CornerOffsets.LowerRight = new DX.Vector3(value, animObject.Depth);
        }

        /// <summary>Function called when a 3D vector value needs to be updated on the animated object.</summary>
        /// <param name="track">The track currently being processed.</param>
        /// <param name="animObject">The object to update.</param>
        /// <param name="value">The value to apply.</param>
        protected override void OnVector3ValueUpdate(GorgonTrackRegistration track, GorgonSprite animObject, DX.Vector3 value)
        {
            if (track.ID == Position3DTrack.ID)
            {
                animObject.Position = (DX.Vector2)value;
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
        protected override void OnVector4ValueUpdate(GorgonTrackRegistration track, GorgonSprite animObject, DX.Vector4 value)
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
}
