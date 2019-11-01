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
    /// A controller used to handle animations for a <see cref="GorgonPolySprite"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This controller is an implementation of the <see cref="GorgonAnimationController{T}"/> type and is used apply animations to a <see cref="GorgonPolySprite"/>. 
    /// </para>
    /// <para>
    /// A controller will update the <see cref="GorgonPolySprite"/> properties over a certain time frame (or continuously if looped) using a <see cref="IGorgonAnimation"/>.
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
    /// <seealso cref="GorgonPolySprite"/>
    public class GorgonPolySpriteAnimationController
        : GorgonAnimationController<GorgonPolySprite>
    {
        #region Variables.
        // The tracks available for animation on a sprite.

        // Single precision floating point tracks.
        private readonly GorgonTrackRegistration _angleTrack = new GorgonTrackRegistration(nameof(GorgonPolySprite.Angle), AnimationTrackKeyType.Single);
        private readonly GorgonTrackRegistration _depthTrack = new GorgonTrackRegistration(nameof(GorgonPolySprite.Depth), AnimationTrackKeyType.Single);
        private readonly GorgonTrackRegistration _textureArrayIndexTrack = new GorgonTrackRegistration(nameof(GorgonPolySprite.TextureArrayIndex), AnimationTrackKeyType.Single);
        private readonly GorgonTrackRegistration _opacityTrack = new GorgonTrackRegistration("Opacity", AnimationTrackKeyType.Single);

        // 2D vector tracks.
        private readonly GorgonTrackRegistration _positionTrack = new GorgonTrackRegistration(nameof(GorgonPolySprite.Position), AnimationTrackKeyType.Vector2);
        private readonly GorgonTrackRegistration _anchorTrack = new GorgonTrackRegistration(nameof(GorgonPolySprite.Anchor), AnimationTrackKeyType.Vector2);
        private readonly GorgonTrackRegistration _scaleTrack = new GorgonTrackRegistration(nameof(GorgonPolySprite.Scale), AnimationTrackKeyType.Vector2);
        private readonly GorgonTrackRegistration _scaledSizeTrack = new GorgonTrackRegistration(nameof(GorgonPolySprite.ScaledSize), AnimationTrackKeyType.Vector2);
        private readonly GorgonTrackRegistration _texScaleTrack = new GorgonTrackRegistration(nameof(GorgonPolySprite.TextureScale), AnimationTrackKeyType.Vector2);
        private readonly GorgonTrackRegistration _texOffsetTrack = new GorgonTrackRegistration(nameof(GorgonPolySprite.TextureOffset), AnimationTrackKeyType.Vector2);

        // 3D vector tracks.
        private readonly GorgonTrackRegistration _position3DTrack = new GorgonTrackRegistration(nameof(GorgonPolySprite.Position), AnimationTrackKeyType.Vector3);

        // Color tracks.
        private readonly GorgonTrackRegistration _colorTrack = new GorgonTrackRegistration(nameof(GorgonPolySprite.Color), AnimationTrackKeyType.Color);

        // Texture tracks.
        private readonly GorgonTrackRegistration _textureTrack = new GorgonTrackRegistration(nameof(GorgonPolySprite.Texture), AnimationTrackKeyType.Texture2D);
        #endregion

        /// <summary>Function called when a single floating point value needs to be updated on the animated object.</summary>
        /// <param name="track">The track currently being processed.</param>
        /// <param name="animObject">The object to update.</param>
        /// <param name="value">The value to apply.</param>        
        protected override void OnSingleValueUpdate(GorgonTrackRegistration track, GorgonPolySprite animObject, float value)
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
        protected override void OnVector2ValueUpdate(GorgonTrackRegistration track, GorgonPolySprite animObject, DX.Vector2 value)
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

            if (track.ID == _scaleTrack.ID)
            {
                animObject.Scale = value;
                return;
            }

            if (track.ID == _texOffsetTrack.ID)
            {
                animObject.TextureOffset = value;
                return;
            }

            if (track.ID == _texScaleTrack.ID)
            {
                animObject.TextureScale = value;
                return;
            }

            if (track.ID != _scaledSizeTrack.ID)
            {                
                return;
            }

            animObject.ScaledSize = new DX.Size2F(value.X, value.Y);
        }

        /// <summary>Function called when a texture needs to be updated on the object.</summary>
        /// <param name="track">The track currently being processed.</param>
        /// <param name="animObject">The object to update.</param>
        /// <param name="texture">The texture to switch to.</param>
        /// <param name="textureCoordinates">The new texture coordinates to apply.</param>
        /// <param name="textureArrayIndex">The texture array index.</param>
        protected override void OnTexture2DUpdate(GorgonTrackRegistration track, GorgonPolySprite animObject, GorgonTexture2DView texture, DX.RectangleF textureCoordinates, int textureArrayIndex)
        {
            if (track.ID != _textureTrack.ID)
            {
                return;
            }

            animObject.Texture = texture;
            animObject.TextureOffset = textureCoordinates.TopLeft;
            animObject.TextureScale = textureCoordinates.BottomRight;
            animObject.TextureArrayIndex = textureArrayIndex;
        }

        /// <summary>Function called when a <see cref="GorgonColor"/> value needs to be updated on the animated object.</summary>
        /// <param name="track">The track currently being processed.</param>
        /// <param name="animObject">The object to update.</param>
        /// <param name="value">The value to apply.</param>
        protected override void OnColorUpdate(GorgonTrackRegistration track, GorgonPolySprite animObject, GorgonColor value)
        {
            if (track.ID != _colorTrack.ID)
            {                
                return;
            }

            animObject.Color = value;
        }

        /// <summary>Function called when a 3D vector value needs to be updated on the animated object.</summary>
        /// <param name="track">The track currently being processed.</param>
        /// <param name="animObject">The object to update.</param>
        /// <param name="value">The value to apply.</param>
        protected override void OnVector3ValueUpdate(GorgonTrackRegistration track, GorgonPolySprite animObject, DX.Vector3 value)
        {
            if (track.ID != _position3DTrack.ID)
            {
                return;
            }

            animObject.Position = (DX.Vector2)value;
            animObject.Depth = value.Z;
        }

        /// <summary>Function called when a 4D vector value needs to be updated on the animated object.</summary>
        /// <param name="track">The track currently being processed.</param>
        /// <param name="animObject">The object to update.</param>
        /// <param name="value">The value to apply.</param>
        protected override void OnVector4ValueUpdate(GorgonTrackRegistration track, GorgonPolySprite animObject, DX.Vector4 value)
        {
            // Not needed for polygonal sprites.
        }

        /// <summary>Function called when a SharpDX <c>RectangleF</c> value needs to be updated on the animated object.</summary>
        /// <param name="track">The track currently being processed.</param>
        /// <param name="animObject">The object to update.</param>
        /// <param name="value">The value to apply.</param>
        protected override void OnRectangleUpdate(GorgonTrackRegistration track, GorgonPolySprite animObject, DX.RectangleF value)
        {
            // Not needed for sprites.
        }

        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Animation.GorgonPolySpriteAnimationController"/> class.</summary>
        public GorgonPolySpriteAnimationController()
        {
            RegisterTrack(_angleTrack);
            RegisterTrack(_depthTrack);
            RegisterTrack(_textureArrayIndexTrack);
            RegisterTrack(_opacityTrack);
            RegisterTrack(_positionTrack);
            RegisterTrack(_anchorTrack);
            RegisterTrack(_scaleTrack);
            RegisterTrack(_scaledSizeTrack);
            RegisterTrack(_texOffsetTrack);
            RegisterTrack(_texScaleTrack);
            RegisterTrack(_position3DTrack);
            RegisterTrack(_colorTrack);
            RegisterTrack(_textureTrack);
        }
    }
}
