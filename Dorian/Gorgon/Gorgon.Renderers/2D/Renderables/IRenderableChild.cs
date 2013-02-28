#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Thursday, February 28, 2013 7:33:51 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimMath;

namespace GorgonLibrary.Renderers
{
    /// <summary>
    /// Defines a renderable that is a child of another renderable.
    /// </summary>
    /// <remarks>Objects that implement this will have their position, scale and rotation changed into a relative transformation to their parent.  If there is no parent, then 
    /// the position, scale and rotation are left as absolute values.</remarks>
    public interface IRenderableChild
        : IRenderable, IMoveable, INamedObject
    {
        #region Properties.
        /// <summary>
        /// Property to return the final position of the child renderable.
        /// </summary>
        /// <remarks>This will return the absolute position of the child in screen space.</remarks>
        public Vector2 FinalPosition
        {
            get;
        }

        /// <summary>
        /// Property to return the final rotation of the child renderable.
        /// </summary>
        /// <remarks>This will return the absolute angle, in degrees, of rotation for the child.</remarks>
        public float FinalRotation
        {
            get;
        }

        /// <summary>
        /// Property to return the final scale of the child renderable.
        /// </summary>
        /// <remarks>This will return the absolute scale of the child.</remarks>
        public Vector2 FinalScale
        {
            get;
        }
        
        /// <summary>
        /// Property to set or return whether to inherit the rotation of the parent renderable.
        /// </summary>
        bool InheritRotation
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return whether to inherit the scale of the parent renderable.
        /// </summary>
        bool InheritScale
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the parent of this renderable.
        /// </summary>
        IRenderableChild Parent
        {
            get;
        }

        /// <summary>
        /// Property to return the list of children for this renderable.
        /// </summary>
        GorgonRenderableChildrenList Children
        {
            get;            
        }
        #endregion

        #region Methods.      
        /// <summary>
        /// Function to update the child renderable from its parent transformation.
        /// </summary>
        void UpdateFromParent();

        /// <summary>
        /// Function called when the parent of a renderable has changed.
        /// </summary>
        /// <param name="parent">The parent of the renderable.</param>
        void OnParentChanged(IRenderableChild parent);
        #endregion
    }
}
