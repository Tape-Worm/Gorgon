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
        /// <summary>Function called when the color needs to be updated on the object.</summary>
        /// <param name="animObject">The object being animated.</param>
        /// <param name="color">The new color.</param>
        protected override void OnColorUpdate(MoveableMesh animObject, GorgonColor color)
        {
        }

        /// <summary>Function called when a position needs to be updated on the object.</summary>
        /// <param name="animObject">The object being animated.</param>
        /// <param name="position">The new position.</param>
        protected override void OnPositionUpdate(MoveableMesh animObject, Vector3 position)
        {
        }

        /// <summary>Function called when a rectangle boundary needs to be updated on the object.</summary>
        /// <param name="animObject">The object being animated.</param>
        /// <param name="bounds">The new bounds.</param>
        protected override void OnRectBoundsUpdate(MoveableMesh animObject, RectangleF bounds)
        {
        }

        /// <summary>Function called when the angle of rotation needs to be updated on the object.</summary>
        /// <param name="animObject">The object being animated.</param>
        /// <param name="rotation">The new angle of rotation, in degrees, on the x, y and z axes.</param>
        protected override void OnRotationUpdate(MoveableMesh animObject, Vector3 rotation) => animObject.Rotation = rotation;

        /// <summary>Function called when a scale needs to be updated on the object.</summary>
        /// <param name="animObject">The object being animated.</param>
        /// <param name="scale">The new scale.</param>
        protected override void OnScaleUpdate(MoveableMesh animObject, Vector3 scale)
        {
        }

        /// <summary>Function called when the size needs to be updated on the object.</summary>
        /// <param name="animObject">The object being animated.</param>
        /// <param name="size">The new size.</param>
        protected override void OnSizeUpdate(MoveableMesh animObject, Vector3 size)
        {
        }

        /// <summary>Function called when a texture needs to be updated on the object.</summary>
        /// <param name="animObject">The object being animated.</param>
        /// <param name="texture">The texture to switch to.</param>
        /// <param name="textureCoordinates">The new texture coordinates to apply.</param>
        /// <param name="textureArrayIndex">The texture array index.</param>
        protected override void OnTexture2DUpdate(MoveableMesh animObject, GorgonTexture2DView texture, RectangleF textureCoordinates, int textureArrayIndex)
        {
        }
    }
}
