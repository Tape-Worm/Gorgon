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
// Created: May 23, 2019 4:02:35 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Animation;

namespace Gorgon.Examples
{
    /// <summary>
    /// Defines a layer for our planet.
    /// </summary>
    /// <remarks>
    /// Each layer is composited on top of the previous layer. By doing this we can give the appearance of clouds moving over planetary body, layers of gas, etc...
    /// </remarks>
    internal class PlanetaryLayer
    {
        /// <summary>
        /// Property to return the mesh representing the layer.
        /// </summary>
        public MoveableMesh Mesh
        {
            get;
        }

        /// <summary>
        /// Property to set or return an animation to play for this layer.
        /// </summary>
        public IGorgonAnimation Animation
        {
            get;
            set;
        }

        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Examples.PlanetaryLayer"/> class.</summary>
        /// <param name="mesh">The mesh.</param>
        public PlanetaryLayer(MoveableMesh mesh)
        {
            Mesh = mesh;
        }
    }
}
