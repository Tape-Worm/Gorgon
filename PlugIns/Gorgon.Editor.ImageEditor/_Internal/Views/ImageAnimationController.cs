#region MIT
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: January 10, 2019 6:45:15 PM
// 
#endregion

using Gorgon.Animation;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using SharpDX;

namespace Gorgon.Editor.ImageEditor
{
    /// <summary>
    /// An animation controller for the image view.
    /// </summary>
    internal class ImageAnimationController
        : GorgonAnimationController<ITextureViewerService>
    {
        // The track used to animate opacity for the texture.
        private static GorgonTrackRegistration _opacityTrack = new GorgonTrackRegistration("Opacity", AnimationTrackKeyType.Single);
        // The track used to animate the texture coordinate boundaries.
        private static GorgonTrackRegistration _textureBoundsTrack = new GorgonTrackRegistration("TextureBounds", AnimationTrackKeyType.Rectangle);

        /// <summary>Function called when a single floating point value needs to be updated on the animated object.</summary>
        /// <param name="track">The track currently being processed.</param>
        /// <param name="animObject">The object to update.</param>
        /// <param name="value">The value to apply.</param>
        protected override void OnSingleValueUpdate(GorgonTrackRegistration track, ITextureViewerService animObject, float value)
        {
            if (_opacityTrack.ID != track.ID)
            {
                return;
            }

            animObject.Alpha = value;
        }

        /// <summary>Function called when a 2D vector value needs to be updated on the animated object.</summary>
        /// <param name="track">The track currently being processed.</param>
        /// <param name="animObject">The object to update.</param>
        /// <param name="value">The value to apply.</param>
        protected override void OnVector2ValueUpdate(GorgonTrackRegistration track, ITextureViewerService animObject, Vector2 value)
        {        
        }

        /// <summary>Function called when a 3D vector value needs to be updated on the animated object.</summary>
        /// <param name="track">The track currently being processed.</param>
        /// <param name="animObject">The object to update.</param>
        /// <param name="value">The value to apply.</param>
        protected override void OnVector3ValueUpdate(GorgonTrackRegistration track, ITextureViewerService animObject, Vector3 value)
        {
        }

        /// <summary>Function called when a 4D vector value needs to be updated on the animated object.</summary>
        /// <param name="track">The track currently being processed.</param>
        /// <param name="animObject">The object to update.</param>
        /// <param name="value">The value to apply.</param>
        protected override void OnVector4ValueUpdate(GorgonTrackRegistration track, ITextureViewerService animObject, Vector4 value)
        {
        }

        /// <summary>Function called when a <see cref="T:Gorgon.Graphics.GorgonColor"/> value needs to be updated on the animated object.</summary>
        /// <param name="track">The track currently being processed.</param>
        /// <param name="animObject">The object to update.</param>
        /// <param name="value">The value to apply.</param>
        protected override void OnColorUpdate(GorgonTrackRegistration track, ITextureViewerService animObject, GorgonColor value)
        {
        }

        /// <summary>Function called when a SharpDX <c>RectangleF</c> value needs to be updated on the animated object.</summary>
        /// <param name="track">The track currently being processed.</param>
        /// <param name="animObject">The object to update.</param>
        /// <param name="value">The value to apply.</param>
        protected override void OnRectangleUpdate(GorgonTrackRegistration track, ITextureViewerService animObject, RectangleF value)
        {
            if (track.ID != _textureBoundsTrack.ID)
            {
                return;
            }

            animObject.TextureBounds = value;
        }

        /// <summary>Function called when a texture needs to be updated on the object.</summary>
        /// <param name="track">The track currently being processed.</param>
        /// <param name="animObject">The object to update.</param>
        /// <param name="texture">The texture to switch to.</param>
        /// <param name="textureCoordinates">The new texture coordinates to apply.</param>
        /// <param name="textureArrayIndex">The texture array index.</param>
        protected override void OnTexture2DUpdate(GorgonTrackRegistration track, ITextureViewerService animObject, GorgonTexture2DView texture, RectangleF textureCoordinates, int textureArrayIndex)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.ImageEditor.ImageAnimationController"/> class.</summary>
        public ImageAnimationController()
        {
            RegisterTrack(_opacityTrack);
            RegisterTrack(_textureBoundsTrack);
        }
    }
}
