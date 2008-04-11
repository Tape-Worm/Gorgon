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
    /// The main entry point for our plug-in.
    /// </summary>
    public class D3DRendererPlugIn
        : RendererPlugIn
    {
        /// <summary>
        /// Function to create a new object from the plug-in.
        /// </summary>
        /// <param name="parameters">Parameters to pass.</param>
        /// <returns>The new object.</returns>
        protected override object CreateImplementation(object[] parameters)
        {
            return new D3DRenderer(parameters[0] as GorgonLibrary.Internal.D3DObjects);
        }

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="RendererPlugIn"/> class.
        /// </summary>
        /// <param name="plugInPath">The plug in path.</param>
        public D3DRendererPlugIn(string plugInPath)
            : base("D3DRenderer", plugInPath)
        {
        }
        #endregion
    }
}
