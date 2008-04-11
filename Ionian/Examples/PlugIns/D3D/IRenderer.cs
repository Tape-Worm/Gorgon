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
    public interface IRenderer
        : IDisposable
    {
        /// <summary>
        /// Function to begin the rendering.
        /// </summary>
        void Begin();

        /// <summary>
        /// Function to render the scene.
        /// </summary>
        /// <param name="frameTime">Frame delta time.</param>
        void Render(float frameTime);

        /// <summary>
        /// Function to end the rendering.
        /// </summary>
        void End();

        /// <summary>
        /// Function to initialize the 3D renderer.
        /// </summary>
        void Initialize();
    }
}
