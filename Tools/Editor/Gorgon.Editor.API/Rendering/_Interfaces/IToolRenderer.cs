#region MIT
// 
// Gorgon.
// Copyright (C) 2020 Michael Winsor
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
// Created: February 6, 2020 9:51:28 PM
// 
#endregion

using System;
using Gorgon.Core;
using Gorgon.Graphics;
using DX = SharpDX;

namespace Gorgon.Editor.Rendering
{
    /// <summary>
    /// Defines a rendering interface for rendering content in content editor plug in views.
    /// </summary>
    /// <remarks>
    /// <para>
    /// See the <see cref="DefaultContentRenderer{T}"/> class for more information on a base rendering class that developers can inherit in order to implement their own renderer(s).
    /// </para>
    /// </remarks>
    /// <seealso cref="DefaultContentRenderer{T}"/>
    public interface IToolRenderer
        : IGorgonNamedObject, IDisposable
    {
        #region Properties.
        /// <summary>
        /// Property to return the size of the view client area.
        /// </summary>
        DX.Size2 ClientSize
        {
            get;
        }

        /// <summary>
        /// Property to set or return the color to use when clearing the swap chain.
        /// </summary>
        /// <remarks>
        /// This value defaults to the background color of the view.
        /// </remarks>
        GorgonColor BackgroundColor
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return whether the renderer is enabled.
        /// </summary>
        bool IsEnabled
        {
            get;
            set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to render the content.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is called by the view to render the content.
        /// </para>
        /// </remarks>
        void Render();

        /// <summary>
        /// Function to load resources for the renderer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is used to load any required temporary resources for the renderer prior to rendering content. This must be paired with a call to <see cref="UnloadResources"/> when the renderer is 
        /// no longer in use to ensure efficient memory usage.
        /// </para>
        /// </remarks>
        void LoadResources();

        /// <summary>
        /// Function to unload resources from the renderer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is used to unload temporary resources for the renderer when it is no longer needed. Failure to call this may result in memory leakage.
        /// </para>
        /// </remarks>
        void UnloadResources();
        #endregion
    }
}
