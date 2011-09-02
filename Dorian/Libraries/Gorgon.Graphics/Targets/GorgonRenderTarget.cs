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
		private bool _disposed = false;		// Flag to indicate that the object was disposed.
		private bool _cleaned = false;		// Flag to indicate that the object was cleaned up.
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
		/// Property to set or return the default background color for the target.
		/// </summary>
		/// <remarks>The default color is white (1.0f, 1.0f, 1.0f, 1.0f).</remarks>
		public GorgonColor DefaultBackgroundColor
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return whether the target has a depth buffer attached to it.
		/// </summary>
		public abstract bool HasDepthBuffer
		{
			get;
		}

		/// <summary>
		/// Property to return whether the target has a stencil buffer attaached to it.
		/// </summary>
		public abstract bool HasStencilBuffer
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to clear out any outstanding resources when the object is disposed.
		/// </summary>
		/// <remarks>Implementors must call this method to clean up outstanding resources in the Dispose method.</remarks>
		protected virtual void CleanUpResources()
		{
			_cleaned = true;
		}

		/// <summary>
		/// Function to create any resources required by the render target.
		/// </summary>
		/// <remarks>Implementors must use this method to build the internal render target based on the rendering back end.</remarks>
		protected abstract void CreateResources();

		/// <summary>
		/// Function to perform an update on the resources required by the render target.
		/// </summary>
		protected abstract void UpdateResources();

		/// <summary>
		/// Function to initialize the render target.
		/// </summary>
		internal virtual void Initialize()
		{
			CreateResources();
		}

		/// <summary>
		/// Function to clear a target.
		/// </summary>
		/// <param name="color">Color to clear with.</param>
		/// <param name="depthValue">Depth buffer value to clear with.</param>
		/// <param name="stencilValue">Stencil value to clear with.</param>
		/// <remarks>This will only clear a depth/stencil buffer if one has been attached to the target, otherwise it will do nothing.
		/// <para>Pass NULL (Nothing in VB.Net) to <paramref name="color"/>, <paramref name="depthValue"/> or <paramref name="stencilValue"/> to exclude that buffer from being cleared.</para>
		/// </remarks>
		public abstract void Clear(GorgonColor? color, float? depthValue, int? stencilValue);

		/// <summary>
		/// Function to clear a target.
		/// </summary>
		/// <param name="color">Color to clear with.</param>
		/// <param name="depthValue">Depth buffer value to clear with.</param>
		/// <remarks>This will only clear a depth/stencil buffer if one has been attached to the target, otherwise it will do nothing.
		/// <para>Pass NULL (Nothing in VB.Net) to <paramref name="color"/> or <paramref name="depthValue"/> to exclude that buffer from being cleared.</para>
		/// </remarks>
		public void Clear(GorgonColor? color, float? depthValue)
		{
			Clear(color, depthValue, 0);
		}

		/// <summary>
		/// Function to clear a target.
		/// </summary>
		/// <param name="color">Color to clear with.</param>
		public void Clear(GorgonColor color)
		{
			Clear(color, 1.0f, 0);
		}

		/// <summary>
		/// Function to clear a target.
		/// </summary>
		/// <remarks>This will clear the target with the <see cref="P:GorgonLibrary.Graphics.GorgonRenderTarget.DefaultBackgroundColor">default background color</see> value.</remarks>
		public void Clear()
		{
			Clear(DefaultBackgroundColor, 1.0f, 0);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonRenderTarget"/> class.
		/// </summary>
		/// <param name="graphics">The graphics instance that owns this render target.</param>
		/// <param name="name">The name.</param>
		/// <param name="settings">The settings used to define the behaviour/display of the target.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.</exception>
		protected GorgonRenderTarget(GorgonGraphics graphics, string name)
			: base(name)
		{
			if (graphics == null)
				throw new ArgumentNullException("graphics");

			Graphics = graphics;
			DefaultBackgroundColor = new GorgonColor(1.0f, 1.0f, 1.0f, 1.0f);
		}
		#endregion
	
		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					// Ensure that any resources that were allocated are released by this time.
					if (!_cleaned)
						throw new ObjectDisposedException(GetType().FullName, "The object was disposed without calling CleanUpResources().  This may cause memory leaks in native code.");
				}

				_disposed = true;
			}
		}
		
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
