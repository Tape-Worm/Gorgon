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
    /// A controller used to handle animations for a <see cref="GorgonTextSprite"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This controller is an implementation of the <see cref="GorgonAnimationController{T}"/> type and is used apply animations to a <see cref="GorgonTextSprite"/>. 
    /// </para>
    /// <para>
    /// A controller will update the <see cref="GorgonTextSprite"/> properties over a certain time frame (or continuously if looped) using a <see cref="IGorgonAnimation"/>.
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
    /// <seealso cref="GorgonTextSprite"/>
    public class GorgonTextSpriteAnimationController
        : GorgonAnimationController<GorgonTextSprite>
    {
        #region Variables.
        /// <summary>
        /// The track registration for the angle of rotation for the sprite.
        /// </summary>
        public static readonly GorgonTrackRegistration AngleTrack = new GorgonTrackRegistration(nameof(GorgonTextSprite.Angle), AnimationTrackKeyType.Single);
        /// <summary>
        /// The track registration for the depth value of the sprite.
        /// </summary>
        public static readonly GorgonTrackRegistration DepthTrack = new GorgonTrackRegistration(nameof(GorgonTextSprite.Depth), AnimationTrackKeyType.Single);
        /// <summary>
        /// The track registration for the opacity of the sprite.
        /// </summary>
        public static readonly GorgonTrackRegistration OpacityTrack = new GorgonTrackRegistration("Opacity", AnimationTrackKeyType.Single);

        /// <summary>
        /// The track registration for the position of the sprite.
        /// </summary>
        public static readonly GorgonTrackRegistration PositionTrack = new GorgonTrackRegistration(nameof(GorgonTextSprite.Position), AnimationTrackKeyType.Vector2);
        /// <summary>
        /// The track registration for the anchor point, in relative coordinates, of the sprite.
        /// </summary>
        public static readonly GorgonTrackRegistration AnchorTrack = new GorgonTrackRegistration(nameof(GorgonTextSprite.Anchor), AnimationTrackKeyType.Vector2);
        /// <summary>
        /// The track registration for the scale, using relative values, of the sprite.
        /// </summary>
        public static readonly GorgonTrackRegistration ScaleTrack = new GorgonTrackRegistration(nameof(GorgonTextSprite.Scale), AnimationTrackKeyType.Vector2);
        /// <summary>
        /// The track registration for the scale, using absolute values, of the sprite.
        /// </summary>
        public static readonly GorgonTrackRegistration ScaledSizeTrack = new GorgonTrackRegistration(nameof(GorgonTextSprite.ScaledSize), AnimationTrackKeyType.Vector2);

        /// <summary>
        /// The track registration for the position, and depth of the sprite.
        /// </summary>
        public static readonly GorgonTrackRegistration Position3DTrack = new GorgonTrackRegistration(nameof(GorgonTextSprite.Position), AnimationTrackKeyType.Vector3);

        /// <summary>
        /// The track registration for the color on a sprite.
        /// </summary>
        public static readonly GorgonTrackRegistration ColorTrack = new GorgonTrackRegistration(nameof(GorgonTextSprite.Color), AnimationTrackKeyType.Color);
        #endregion

        #region Methods.
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
        protected override void OnVector2ValueUpdate(GorgonTrackRegistration track, GorgonTextSprite animObject, DX.Vector2 value)
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

            animObject.ScaledSize = new DX.Size2F(value.X, value.Y);
        }

        /// <summary>Function called when a 3D vector value needs to be updated on the animated object.</summary>
        /// <param name="track">The track currently being processed.</param>
        /// <param name="animObject">The object to update.</param>
        /// <param name="value">The value to apply.</param>
        protected override void OnVector3ValueUpdate(GorgonTrackRegistration track, GorgonTextSprite animObject, DX.Vector3 value)
        {
            if (track.ID != Position3DTrack.ID)
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
        protected override void OnVector4ValueUpdate(GorgonTrackRegistration track, GorgonTextSprite animObject, DX.Vector4 value)
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
        protected override void OnRectangleUpdate(GorgonTrackRegistration track, GorgonTextSprite animObject, DX.RectangleF value)
        {
            // Not needed for text sprites.
        }

        /// <summary>Function called when a texture needs to be updated on the object.</summary>
        /// <param name="track">The track currently being processed.</param>
        /// <param name="animObject">The object to update.</param>
        /// <param name="texture">The texture to switch to.</param>
        /// <param name="textureCoordinates">The new texture coordinates to apply.</param>
        /// <param name="textureArrayIndex">The texture array index.</param>
        protected override void OnTexture2DUpdate(GorgonTrackRegistration track, GorgonTextSprite animObject, GorgonTexture2DView texture, DX.RectangleF textureCoordinates, int textureArrayIndex)
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
        #endregion
    }
}
