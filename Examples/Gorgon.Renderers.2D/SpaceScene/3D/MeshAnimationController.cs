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
// Created: May 20, 2019 11:07:41 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Animation;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using SharpDX;

namespace Gorgon.Examples
{
    /// <summary>
    /// An animation controller for animating our 3D entitis.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This shows how we can use Gorgon's animation system with a 3D object. By overriding the methods and telling the controller which property or properties to update, the system will continuously 
    /// update the object with each call to the Update method.
    /// </para>
    /// </remarks>
    internal class MeshAnimationController
        : GorgonAnimationController<MoveableMesh>
    {
        // Track used for rotating the mesh.
        private static readonly GorgonTrackRegistration _rotationTrack = new GorgonTrackRegistration("Rotation", AnimationTrackKeyType.Vector3);

        /// <summary>Function called when a texture needs to be updated on the object.</summary>
        /// <param name="track">The track currently being processed.</param>
        /// <param name="animObject">The object to update.</param>
        /// <param name="texture">The texture to switch to.</param>
        /// <param name="textureCoordinates">The new texture coordinates to apply.</param>
        /// <param name="textureArrayIndex">The texture array index.</param>
        protected override void OnTexture2DUpdate(GorgonTrackRegistration track, MoveableMesh animObject, GorgonTexture2DView texture, RectangleF textureCoordinates, int textureArrayIndex)
        {
        }

        /// <summary>Function called when a 2D vector value needs to be updated on the animated object.</summary>
        /// <param name="track">The track currently being processed.</param>
        /// <param name="animObject">The object to update.</param>
        /// <param name="value">The value to apply.</param>
        protected override void OnVector2ValueUpdate(GorgonTrackRegistration track, MoveableMesh animObject, Vector2 value)
        {
        }

        /// <summary>Function called when a 3D vector value needs to be updated on the animated object.</summary>
        /// <param name="track">The track currently being processed.</param>
        /// <param name="animObject">The object to update.</param>
        /// <param name="value">The value to apply.</param>
        protected override void OnVector3ValueUpdate(GorgonTrackRegistration track, MoveableMesh animObject, Vector3 value)
        {
            // Check the track ID against the registered ID. This will avoid updating the wrong property while the animation is running.
            if (track.ID != _rotationTrack.ID)
            {
                return;
            }
            
            animObject.Rotation = value;
        }

        /// <summary>Function called when a 4D vector value needs to be updated on the animated object.</summary>
        /// <param name="track">The track currently being processed.</param>
        /// <param name="animObject">The object to update.</param>
        /// <param name="value">The value to apply.</param>
        protected override void OnVector4ValueUpdate(GorgonTrackRegistration track, MoveableMesh animObject, Vector4 value)
        {
        }

        /// <summary>Function called when a <see cref="T:Gorgon.Graphics.GorgonColor"/> value needs to be updated on the animated object.</summary>
        /// <param name="track">The track currently being processed.</param>
        /// <param name="animObject">The object to update.</param>
        /// <param name="value">The value to apply.</param>
        protected override void OnColorUpdate(GorgonTrackRegistration track, MoveableMesh animObject, GorgonColor value)
        {        
        }

        /// <summary>Function called when a SharpDX <c>RectangleF</c> value needs to be updated on the animated object.</summary>
        /// <param name="track">The track currently being processed.</param>
        /// <param name="animObject">The object to update.</param>
        /// <param name="value">The value to apply.</param>
        protected override void OnRectangleUpdate(GorgonTrackRegistration track, MoveableMesh animObject, RectangleF value)
        {        
        }

        /// <summary>Function called when a single floating point value needs to be updated on the animated object.</summary>
        /// <param name="track">The track currently being processed.</param>
        /// <param name="animObject">The object to update.</param>
        /// <param name="value">The value to apply.</param>
        protected override void OnSingleValueUpdate(GorgonTrackRegistration track, MoveableMesh animObject, float value)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="MeshAnimationController"/> class.</summary>
        public MeshAnimationController() => RegisterTrack(_rotationTrack);
    }
}
