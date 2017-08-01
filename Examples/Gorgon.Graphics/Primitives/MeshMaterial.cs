﻿#region MIT
// 
// Gorgon.
// Copyright (C) 2017 Michael Winsor
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
// Created: July 30, 2017 2:55:45 PM
// 
#endregion

using DX = SharpDX;
using Gorgon.Graphics.Core;

namespace Gorgon.Graphics.Example
{
    /// <summary>
    /// A material for a mesh.
    /// </summary>
    class MeshMaterial
    {
        #region Properties.
        /// <summary>
        /// Property to return the textures for this mesh.
        /// </summary>
        public GorgonMonitoredArray<string> Textures
        {
            get;
        }

        /// <summary>
        /// Property to set or return an offset for the textures.
        /// </summary>
        public DX.Vector2 TextureOffset
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the specular power.
        /// </summary>
        public float SpecularPower
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the vertex shader for this mesh.
        /// </summary>
        public string VertexShader
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the pixel shader for this mesh.
        /// </summary>
        public string PixelShader
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the blending state for this mesh.
        /// </summary>
        public GorgonBlendState BlendState
        {
            get;
            set;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="MeshMaterial"/> class.
        /// </summary>
        public MeshMaterial()
        {
            SpecularPower = 1.0f;
            Textures = new GorgonMonitoredArray<string>(3);
            BlendState = GorgonBlendState.NoBlending;
        }
        #endregion
    }
}
