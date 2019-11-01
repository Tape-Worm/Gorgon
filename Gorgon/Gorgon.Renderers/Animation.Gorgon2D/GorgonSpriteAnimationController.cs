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
        // The tracks available for animation on a sprite.

        // Single precision floating point tracks.
        private readonly GorgonTrackRegistration _angleTrack = new GorgonTrackRegistration(nameof(GorgonSprite.Angle), AnimationTrackKeyType.Single);
        private readonly GorgonTrackRegistration _depthTrack = new GorgonTrackRegistration(nameof(GorgonSprite.Depth), AnimationTrackKeyType.Single);
        private readonly GorgonTrackRegistration _textureArrayIndexTrack = new GorgonTrackRegistration(nameof(GorgonSprite.TextureArrayIndex), AnimationTrackKeyType.Single);
        private readonly GorgonTrackRegistration _opacityTrack = new GorgonTrackRegistration("Opacity", AnimationTrackKeyType.Single);

        // 2D vector tracks.
        private readonly GorgonTrackRegistration _positionTrack = new GorgonTrackRegistration(nameof(GorgonSprite.Position), AnimationTrackKeyType.Vector2);
        private readonly GorgonTrackRegistration _anchorTrack = new GorgonTrackRegistration(nameof(GorgonSprite.Anchor), AnimationTrackKeyType.Vector2);
        private readonly GorgonTrackRegistration _absAnchorTrack = new GorgonTrackRegistration(nameof(GorgonSprite.AbsoluteAnchor), AnimationTrackKeyType.Vector2);
        private readonly GorgonTrackRegistration _sizeTrack = new GorgonTrackRegistration(nameof(GorgonSprite.Size), AnimationTrackKeyType.Vector2);
        private readonly GorgonTrackRegistration _scaleTrack = new GorgonTrackRegistration(nameof(GorgonSprite.Scale), AnimationTrackKeyType.Vector2);
        private readonly GorgonTrackRegistration _scaledSizeTrack = new GorgonTrackRegistration(nameof(GorgonSprite.ScaledSize), AnimationTrackKeyType.Vector2);
        private readonly GorgonTrackRegistration _nwPosTrack = new GorgonTrackRegistration(nameof(GorgonSprite.CornerOffsets.UpperLeft), AnimationTrackKeyType.Vector2);
        private readonly GorgonTrackRegistration _nePosTrack = new GorgonTrackRegistration(nameof(GorgonSprite.CornerOffsets.UpperRight), AnimationTrackKeyType.Vector2);
        private readonly GorgonTrackRegistration _swPosTrack = new GorgonTrackRegistration(nameof(GorgonSprite.CornerOffsets.LowerLeft), AnimationTrackKeyType.Vector2);
        private readonly GorgonTrackRegistration _sePosTrack = new GorgonTrackRegistration(nameof(GorgonSprite.CornerOffsets.LowerRight), AnimationTrackKeyType.Vector2);

        // 3D vector tracks.
        private readonly GorgonTrackRegistration _position3DTrack = new GorgonTrackRegistration(nameof(GorgonSprite.Position), AnimationTrackKeyType.Vector3);
        private readonly GorgonTrackRegistration _nwPos3DTrack = new GorgonTrackRegistration(nameof(GorgonSprite.CornerOffsets.UpperLeft), AnimationTrackKeyType.Vector3);
        private readonly GorgonTrackRegistration _nePos3DTrack = new GorgonTrackRegistration(nameof(GorgonSprite.CornerOffsets.UpperRight), AnimationTrackKeyType.Vector3);
        private readonly GorgonTrackRegistration _swPos3DTrack = new GorgonTrackRegistration(nameof(GorgonSprite.CornerOffsets.LowerLeft), AnimationTrackKeyType.Vector3);
        private readonly GorgonTrackRegistration _sePos3DTrack = new GorgonTrackRegistration(nameof(GorgonSprite.CornerOffsets.LowerRight), AnimationTrackKeyType.Vector3);

        // Rectangle tracks.
        private readonly GorgonTrackRegistration _boundsTrack = new GorgonTrackRegistration(nameof(GorgonSprite.Bounds), AnimationTrackKeyType.Rectangle);
        private readonly GorgonTrackRegistration _texCoordsTrack = new GorgonTrackRegistration(nameof(GorgonSprite.TextureRegion), AnimationTrackKeyType.Rectangle);

        // Color tracks.
        private readonly GorgonTrackRegistration _colorTrack = new GorgonTrackRegistration(nameof(GorgonSprite.Color), AnimationTrackKeyType.Color);
        private readonly GorgonTrackRegistration _nwColorTrack = new GorgonTrackRegistration(nameof(GorgonSprite.CornerColors.UpperLeft), AnimationTrackKeyType.Color);
        private readonly GorgonTrackRegistration _neColorTrack = new GorgonTrackRegistration(nameof(GorgonSprite.CornerColors.UpperRight), AnimationTrackKeyType.Color);
        private readonly GorgonTrackRegistration _swColorTrack = new GorgonTrackRegistration(nameof(GorgonSprite.CornerColors.LowerLeft), AnimationTrackKeyType.Color);
        private readonly GorgonTrackRegistration _seColorTrack = new GorgonTrackRegistration(nameof(GorgonSprite.CornerColors.LowerRight), AnimationTrackKeyType.Color);

        // Texture tracks.
        private readonly GorgonTrackRegistration _textureTrack = new GorgonTrackRegistration(nameof(GorgonSprite.Texture), AnimationTrackKeyType.Texture2D);
        #endregion

        #region Methods.
        /// <summary>Function called when a single floating point value needs to be updated on the animated object.</summary>
        /// <param name="track">The track currently being processed.</param>
        /// <param name="animObject">The object to update.</param>
        /// <param name="value">The value to apply.</param>
        protected override void OnSingleValueUpdate(GorgonTrackRegistration track, GorgonSprite animObject, float value)
        {
            if (track.ID == _angleTrack.ID)
            {
                animObject.Angle = value;
                return;
            }

            if (track.ID == _depthTrack.ID)
            {
                animObject.Depth = value;
                return;
            }

            if (track.ID == _textureArrayIndexTrack.ID)
            {
                animObject.TextureArrayIndex = (int)value;
                return;
            }

            if (track.ID != _opacityTrack.ID)
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
            if (track.ID == _positionTrack.ID)
            {
                animObject.Position = value;
                return;
            }

            if (track.ID == _anchorTrack.ID)
            {
                animObject.Anchor = value;
                return;
            }

            if (track.ID == _absAnchorTrack.ID)
            {
                animObject.AbsoluteAnchor = value;
                return;
            }

            if (track.ID == _sizeTrack.ID)
            {
                animObject.Size = new DX.Size2F(value.X, value.Y);
                return;
            }

            if (track.ID == _scaleTrack.ID)
            {
                animObject.Scale = value;
                return;
            }

            if (track.ID == _scaledSizeTrack.ID)
            {
                animObject.ScaledSize = new DX.Size2F(value.X, value.Y);
                return;
            }

            if (track.ID == _nwPosTrack.ID)
            {
                animObject.CornerOffsets.UpperLeft = new DX.Vector3(value, animObject.Depth);
                return;
            }

            if (track.ID == _nePosTrack.ID)
            {
                animObject.CornerOffsets.UpperRight = new DX.Vector3(value, animObject.Depth);
                return;
            }

            if (track.ID == _swPosTrack.ID)
            {
                animObject.CornerOffsets.LowerLeft = new DX.Vector3(value, animObject.Depth);
                return;
            }

            if (track.ID != _sePosTrack.ID)
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
            if (track.ID == _position3DTrack.ID)
            {
                animObject.Position = (DX.Vector2)value;
                animObject.Depth = value.Z;
                return;
            }

            if (track.ID == _nwPos3DTrack.ID)
            {
                animObject.CornerOffsets.UpperLeft = value;
                return;
            }

            if (track.ID == _nePos3DTrack.ID)
            {
                animObject.CornerOffsets.UpperRight = value;
                return;
            }

            if (track.ID == _swPos3DTrack.ID)
            {
                animObject.CornerOffsets.LowerLeft = value;
                return;
            }

            if (track.ID != _sePos3DTrack.ID)
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
            if (track.ID == _boundsTrack.ID)
            {
                animObject.Bounds = value;
                return;
            }

            if (track.ID != _texCoordsTrack.ID)
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
            if (track.ID == _colorTrack.ID)
            {
                animObject.Color = value;
                return;
            }

            if (track.ID == _nwColorTrack.ID)
            {
                animObject.CornerColors.UpperLeft = value;
                return;
            }

            if (track.ID == _neColorTrack.ID)
            {
                animObject.CornerColors.UpperRight = value;
                return;
            }

            if (track.ID == _swColorTrack.ID)
            {
                animObject.CornerColors.LowerLeft = value;
                return;
            }

            if (track.ID != _seColorTrack.ID)
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
            if (track.ID != _textureTrack.ID)
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
            RegisterTrack(_angleTrack);
            RegisterTrack(_depthTrack);
            RegisterTrack(_textureArrayIndexTrack);
            RegisterTrack(_opacityTrack);
            RegisterTrack(_positionTrack);
            RegisterTrack(_anchorTrack);
            RegisterTrack(_absAnchorTrack);
            RegisterTrack(_sizeTrack);
            RegisterTrack(_scaleTrack);
            RegisterTrack(_scaledSizeTrack);
            RegisterTrack(_nwPosTrack);
            RegisterTrack(_nePosTrack);
            RegisterTrack(_swPosTrack);
            RegisterTrack(_sePosTrack);
            RegisterTrack(_position3DTrack);
            RegisterTrack(_nwPos3DTrack);
            RegisterTrack(_nePos3DTrack);
            RegisterTrack(_swPos3DTrack);
            RegisterTrack(_sePos3DTrack);
            RegisterTrack(_texCoordsTrack);
            RegisterTrack(_boundsTrack);
            RegisterTrack(_colorTrack);
            RegisterTrack(_nwColorTrack);
            RegisterTrack(_neColorTrack);
            RegisterTrack(_swColorTrack);
            RegisterTrack(_seColorTrack);
            RegisterTrack(_textureTrack);
        }
        #endregion
    }
}
