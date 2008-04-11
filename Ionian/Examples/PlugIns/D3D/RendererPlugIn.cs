#region LGPL.
// 
// Examples.
// Copyright (C) 2008 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 
// Created: Monday, April 07, 2008 8:19:04 PM
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

            rendererPlugIn = PlugInFactory.Load(path, "D3DRenderer") as RendererPlugIn;

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
