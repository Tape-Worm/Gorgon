#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Saturday, July 30, 2011 1:11:00 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A render target.
	/// </summary>
	/// <remarks>The render target will receive the graphical data for display.</remarks>
	public abstract class GorgonRenderTarget
		: GorgonNamedObject, IDisposable
	{
		#region Variables.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the graphics interface that created this render target.
		/// </summary>
		protected GorgonGraphics Graphics
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return information about the render target.
		/// </summary>
		/// <remarks>Use this to return the width, height and format of the target.</remarks>
		public GorgonVideoMode TargetInformation
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the format of the attached depth/stencil buffer.
		/// </summary>
		/// <remarks>This will return the format of the depth/stencil buffer if these are created when the target is created.  If neither are created, then this property will return <see cref="E:GorgonLibrary.Graphics.GorgonBufferFormat.Unknown">GorgonBufferFormat.Unknown</see>.</remarks>
		public GorgonBufferFormat DepthStencilFormat
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to create the render target.
		/// </summary>
		/// <remarks>Implementors must use this function to build the internal render target based on the rendering back end.</remarks>
		protected abstract void CreateRenderTarget();

		/// <summary>
		/// Function to update the information about the target.
		/// </summary>
		/// <param name="mode">The dimensions and format of the target.</param>
		/// <param name="depthStencilFormat">The depth buffer format.</param>
		protected void UpdateTargetInformation(GorgonVideoMode mode, GorgonBufferFormat depthStencilFormat)
		{
			TargetInformation = mode;
			DepthStencilFormat = depthStencilFormat;
		}

		/// <summary>
		/// Function to perform an update on the render target.
		/// </summary>
		protected abstract void UpdateRenderTarget();

		/// <summary>
		/// Function to initialize the render target.
		/// </summary>
		internal virtual void Initialize()
		{
			CreateRenderTarget();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonRenderTarget"/> class.
		/// </summary>
		/// <param name="graphics">The graphics instance that owns this render target.</param>
		/// <param name="name">The name.</param>
		/// <param name="mode">A video mode structure defining the width, height and format of the render target.</param>
		/// <param name="depthStencilFormat">The depth buffer format (if required) for the target.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.</exception>
		/// <remarks>Passing <see cref="E:GorgonLibrary.Graphics.GorgonBufferFormat.Unknown">GorgonBufferFormat.Unknown</see> will skip the creation of the depth/stencil buffer.</remarks>
		protected GorgonRenderTarget(GorgonGraphics graphics, string name, GorgonVideoMode mode, GorgonBufferFormat depthStencilFormat)
			: base(name)
		{
			if (graphics == null)
				throw new ArgumentNullException("graphics");

			Graphics = graphics;
			TargetInformation = mode;
			DepthStencilFormat = depthStencilFormat;
		}
		#endregion
	
		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected abstract void Dispose(bool disposing);
		
		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void  Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
