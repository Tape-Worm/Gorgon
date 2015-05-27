#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Wednesday, February 22, 2012 4:36:09 PM
// 
#endregion

using System;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Renderers;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Extensions to the main graphics interface.
	/// </summary>
	public static class GorgonGraphics2DExtensions
	{
		#region Methods.
        /// <summary>
        /// Function to create a new 2D renderer interface.
        /// </summary>
        /// <param name="target">Default target for the renderer.</param>
        /// <param name="systemCreatedSwap"><c>true</c> if the system generated the swap chain, <c>false</c> if not.</param>
        /// <param name="vertexCacheSize">The number of vertices that can be placed in the vertex cache.</param>
        /// <returns>A new 2D graphics interface.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="target"/> parameter is NULL (Nothing in VB.Net).</exception>
        private static Gorgon2D Create2DRenderer(GorgonRenderTargetView target, bool systemCreatedSwap, int vertexCacheSize)
        {
	        if (target == null)
	        {
		        throw new ArgumentNullException("target");
	        }

            var result = new Gorgon2D(target, vertexCacheSize, systemCreatedSwap);
            result.Graphics.ImmediateContext.AddTrackedObject(result);

            return result;
        }

        /// <summary>
		/// Function to create a new 2D renderer interface.
		/// </summary>
		/// <param name="graphics">Graphics interface used to create the 2D interface.</param>
		/// <param name="window">Window to use for rendering.</param>
		/// <returns>A new 2D graphics interface.</returns>
		/// <remarks>This method creates an internal swap chain and uses that for the display.  To have more control over the initial render target, use the <see cref="Create2DRenderer(Gorgon.Graphics.GorgonOutputMerger,Gorgon.Graphics.GorgonRenderTargetView,int)">Create2DRenderer(GorgonRenderTarget)</see> extension overload.</remarks>
		/// <exception cref="System.ArgumentException">Thrown when the target was not created by the same graphics interface as the one creating the 2D interface.
		/// <para>Thrown when the <paramref name="window"/> parameter is NULL (Nothing in VB.Net), and the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application window</see> is NULL.</para>
		/// </exception>
		/// <exception cref="GorgonException">Thrown when the video output could not be determined from the window.
		/// <para>-or-</para>
		/// <para>Thrown when the swap chain is going to full screen mode and another swap chain is already on the video output.</para>
		/// </exception>
		public static Gorgon2D Create2DRenderer(this GorgonOutputMerger graphics, Control window)
		{
            if (window != null)
            {
                return Create2DRenderer(graphics, window, window.ClientSize.Width, window.ClientSize.Height);
            }

            window = GorgonApplication.ApplicationForm;

            if (window == null)
            {
                throw new ArgumentNullException("window");
            }

            return Create2DRenderer(graphics, window, window.ClientSize.Width, window.ClientSize.Height);
		}

		/// <summary>
		/// Function to create a new 2D renderer interface.
		/// </summary>
		/// <param name="graphics">Graphics interface used to create the 2D interface.</param>
		/// <param name="window">Window to use for rendering.</param>
		/// <param name="width">Width of the video mode used for rendering.</param>
		/// <param name="height">Height of the video mode used for rendering.</param>
		/// <param name="format">[Optional] Format of the video mode used for rendering.</param>
		/// <param name="isWindowed">[Optional] <c>true</c> to use windowed mode, <c>false</c> to to use full screen mode.</param>
		/// <param name="depthStencilFormat">[Optional] Depth/stencil buffer format.</param>
		/// <param name="vertexCacheSize">[Optional] The number of vertices that the renderer will cache when drawing.</param>
		/// <returns>A new 2D graphics interface.</returns>
		/// <remarks>This method creates an internal swap chain and uses that for the display.  To have more control over the initial render target, use the <see cref="Create2DRenderer(Gorgon.Graphics.GorgonOutputMerger,Gorgon.Graphics.GorgonRenderTargetView,int)">Create2DRenderer(GorgonRenderTarget)</see> extension overload.
		/// <para>The depth/stencil buffer is optional, and will only be used when <paramref name="depthStencilFormat"/> is not set to Unknown.</para>
		/// <para>The <paramref name="vertexCacheSize"/> allows for adjustment to the size of the cache that stores vertices when rendering.  More vertices means a larger buffer and more memory used, but may 
		/// provide a performance increase by rendering many objects at the same time.  Lower values means a smaller buffer and possibly reduced performance because not as many objects can be drawn 
		/// at a given time.  Any performance increase from this value depends upon multiple factors such as available RAM, video driver, video card, etc...</para>
		/// </remarks>
		/// <exception cref="System.ArgumentException">Thrown when the target was not created by the same graphics interface as the one creating the 2D interface.
		/// <para>Thrown when the <paramref name="window"/> parameter is NULL (Nothing in VB.Net), and the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application window</see> is NULL.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="format"/> parameter cannot be used by the video device for displaying data or for the depth/stencil buffer.</para>
		/// </exception>
		/// <exception cref="GorgonException">Thrown when the video output could not be determined from the window.
		/// <para>-or-</para>
		/// <para>Thrown when the swap chain is going to full screen mode and another swap chain is already on the video output.</para>
		/// </exception>
		public static Gorgon2D Create2DRenderer(this GorgonOutputMerger graphics, Control window, int width, int height, BufferFormat format = BufferFormat.Unknown, bool isWindowed = true, BufferFormat depthStencilFormat = BufferFormat.Unknown, int vertexCacheSize = 32768)
		{
		    GorgonSwapChain swapChain = graphics.CreateSwapChain("Gorgon2D.DefaultSwapChain", new GorgonSwapChainSettings
		    {
		        BufferCount = 2,
		        DepthStencilFormat = depthStencilFormat,
		        Flags = SwapChainUsageFlags.RenderTarget,
		        Format = format,
		        Height = height,
		        IsWindowed = isWindowed,
		        Multisampling = new GorgonMultisampling(1, 0),
		        SwapEffect = SwapEffect.Discard,
		        Width = width,
		        Window = window
		    });

		    return Create2DRenderer(swapChain, true, vertexCacheSize);
		}

		/// <summary>
		/// Function to create a new 2D renderer interface.
		/// </summary>
		/// <param name="graphics">Graphics interface used to create the 2D interface.</param>
		/// <param name="target">Default target for the renderer.</param>
        /// <param name="vertexCacheSize">[Optional] The number of vertices that the renderer will cache when drawing.</param>
		/// <returns>A new 2D graphics interface.</returns>
		/// <remarks>This will create a 2D rendering interface with a previously existing render target as its default target. 
        /// <para>The <paramref name="vertexCacheSize"/> allows for adjustment to the size of the cache that stores vertices when rendering.  More vertices means a larger buffer and more memory used, but may 
        /// provide a performance increase by rendering many objects at the same time.  Lower values means a smaller buffer and possibly reduced performance because not as many objects can be drawn 
        /// at a given time.  Any performance increase from this value depends upon multiple factors such as available RAM, video driver, video card, etc...</para>
        /// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="target"/> parameter is NULL (Nothing in VB.Net).</exception>
		public static Gorgon2D Create2DRenderer(this GorgonOutputMerger graphics, GorgonRenderTargetView target, int vertexCacheSize = 32768)
		{
			return Create2DRenderer(target, false, vertexCacheSize);
		}
		#endregion
	}	
}
