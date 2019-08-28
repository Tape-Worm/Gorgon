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
    /// This controller will advance the time for an animation, and coordinate the changes from interpolation (if supported) between <see cref="IGorgonKeyFrame"/> items on a <see cref="IGorgonTrack{T}"/>.
    /// The values from the animation will then by applied to the object properties.
    /// </para>
    /// <para>
    /// Applications can force the playing animation to jump to a specific <see cref="GorgonAnimationController{T}.Time"/>, or increment the time step smoothly using the
    /// <see cref="GorgonAnimationController{T}.Update"/> method.
    /// </para>
    /// <para>
    /// The following is a list of supported tracks and track key frame components for this controller type:
    /// <list type="bullet">
    ///     <item>
    ///         <description>The <see cref="IGorgonAnimation.PositionTrack"/> uses the X and Y coordinates for <see cref="GorgonSprite.Position"/>, and the Z coordinate for <see cref="GorgonSprite.Depth"/>.</description>
    ///     </item>
    ///     <item>
    ///         <description>The <see cref="IGorgonAnimation.RotationTrack"/> uses the Z value for <see cref="GorgonSprite.Angle"/>.</description>
    ///     </item>
    ///     <item>
    ///         <description>The <see cref="IGorgonAnimation.ScaleTrack"/> uses the X and Y values for <see cref="GorgonSprite.Scale"/>, the Z value is ignored.</description>
    ///     </item>
    ///     <item>
    ///         <description>The <see cref="IGorgonAnimation.ColorTrack"/> is used as-is.</description>
    ///     </item>
    ///     <item>
    ///         <description>The <see cref="IGorgonAnimation.RectBoundsTrack"/> uses the width and height values for <see cref="GorgonSprite.Size"/>, the X and Y values are ignored.</description>
    ///     </item>
    ///     <item>
    ///         <description>The <see cref="IGorgonAnimation.Texture2DTrack"/> is used as-is.</description>
    ///     </item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonAnimationController{T}"/>
    /// <seealso cref="IGorgonAnimation"/>
    /// <seealso cref="GorgonSprite"/>
    public class GorgonSpriteAnimationController
        : GorgonAnimationController<GorgonSprite>
    {
        /// <summary>
        /// Function called when a position needs to be updated on the object.
        /// </summary>
        /// <param name="animObject">The object being animated.</param>
        /// <param name="position">The new position.</param>
        protected override void OnPositionUpdate(GorgonSprite animObject, DX.Vector3 position)
        {
            animObject.Position = (DX.Vector2)position;
            animObject.Depth = position.Z;
        }

        /// <summary>Function called when the size needs to be updated on the object.</summary>
        /// <param name="animObject">The object being animated.</param>
        /// <param name="size">The new size.</param>
        protected override void OnSizeUpdate(GorgonSprite animObject, DX.Vector3 size) => animObject.Size = new DX.Size2F(size.X, size.Y);

        /// <summary>
        /// Function called when the angle of rotation needs to be updated on the object.
        /// </summary>
        /// <param name="animObject">The object being animated.</param>
        /// <param name="rotation">The new angle of rotation, in degrees, on the x, y and z axes.</param>
        protected override void OnRotationUpdate(GorgonSprite animObject, DX.Vector3 rotation) => animObject.Angle = rotation.Z;

        /// <summary>
        /// Function called when a scale needs to be updated on the object.
        /// </summary>
        /// <param name="animObject">The object being animated.</param>
        /// <param name="scale">The new scale.</param>
        protected override void OnScaleUpdate(GorgonSprite animObject, DX.Vector3 scale) => animObject.Scale = (DX.Vector2)scale;

        /// <summary>
        /// Function called when a rectangle boundary needs to be updated on the object.
        /// </summary>
        /// <param name="animObject">The object being animated.</param>
        /// <param name="position">The new bounds.</param>
        protected override void OnRectBoundsUpdate(GorgonSprite animObject, DX.RectangleF position) => animObject.Size = position.Size;

        /// <summary>
        /// Function called when a texture needs to be updated on the object.
        /// </summary>
        /// <param name="animObject">The object being animated.</param>
        /// <param name="texture">The texture to switch to.</param>
        /// <param name="textureCoordinates">The new texture coordinates to apply.</param>
        /// <param name="textureArrayIndex">The texture array index.</param>
        protected override void OnTexture2DUpdate(GorgonSprite animObject, GorgonTexture2DView texture, DX.RectangleF textureCoordinates, int textureArrayIndex)
        {
            animObject.Texture = texture;
            animObject.TextureRegion = textureCoordinates;
            animObject.TextureArrayIndex = textureArrayIndex;
        }

        /// <summary>
        /// Function called when the color needs to be updated on the object.
        /// </summary>
        /// <param name="animObject">The object being animated.</param>
        /// <param name="color">The new color.</param>
        protected override void OnColorUpdate(GorgonSprite animObject, GorgonColor color) => animObject.Color = color;
    }
}
