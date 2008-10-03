#region MIT.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: TOBEREPLACED
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using GorgonLibrary;
using GorgonLibrary.PlugIns;

namespace GorgonLibrary.Example
{
    /// <summary>
    /// Interface for the 3D renderer.
    /// </summary>
    public abstract class RendererPlugIn
        : PlugInEntryPoint
    {
        #region Methods.        
        /// <summary>
        /// Function to create a renderer instance.
        /// </summary>
        /// <returns></returns>
        internal IRenderer CreateRenderer()
        {
            return CreateImplementation(new object[] {GetD3DObjects()}) as IRenderer;
        }

        /// <summary>
        /// Function to load the renderer.
        /// </summary>
        /// <param name="path">Path to the renderer DLL.</param>
        /// <returns>A new renderer instance.</returns>
        public static IRenderer LoadRenderer(string path)
        {
            RendererPlugIn rendererPlugIn = null;         // Plug in.

            rendererPlugIn = PlugInFactory.Load(path, "D3DRenderer", false) as RendererPlugIn;

            return rendererPlugIn.CreateRenderer();
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="RendererPlugInEntryPoint"/> class.
        /// </summary>
        /// <param name="plugInName">Name of the plug in.</param>
        /// <param name="plugInPath">The plug in path.</param>
        protected RendererPlugIn(string plugInName, string plugInPath)
            : base(plugInName, plugInPath, PlugInType.UserDefined)
        {
        }
        #endregion    
    }
}
