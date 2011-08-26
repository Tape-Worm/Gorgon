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
	/// This is used to pass multi sample information around for creating multi sample/anti-aliased graphics.
	/// </summary>
	public struct GorgonMSAAQualityLevel
	{
		/// <summary>
		/// Multi sample level to assign.
		/// </summary>
		public GorgonMSAALevel Level;
		/// <summary>
		/// Quality of the multi sample level.
		/// </summary>
		public int Quality;
				
		/// <summary>
		/// Gorgons the multi sample level.
		/// </summary>
		/// <param name="level">The level.</param>
		/// <param name="quality">The quality.</param>
		public GorgonMSAAQualityLevel(GorgonMSAALevel level, int quality)
		{
			Level = level;
			Quality = quality;
		}
	}

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
		/// Property to return the width of the render target.
		/// </summary>
		public int Width
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the height of the render target.
		/// </summary>
		public int Height
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

		/// <summary>
		/// Property to return the quality level for multisampling anti-aliasing.
		/// </summary>
		public GorgonMSAAQualityLevel? MultiSampleAALevel
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return whether the target is ready to receive rendering data.
		/// </summary>
		public abstract bool IsReady
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
		/// Function to update the information about the target.
		/// </summary>
		/// <param name="width">Width of the render target in pixels.</param>
		/// <param name="height">Height of the render target in pixels.</param>
		/// <param name="depthStencilFormat">The depth buffer format.</param>
		/// <param name="msaaLevel">Multi sampling anti-aliasing quality level.</param>
		protected void UpdateTargetInformation(int width, int height, GorgonBufferFormat depthStencilFormat, GorgonMSAAQualityLevel? msaaLevel)
		{
			Width = width;
			Height = height;
			DepthStencilFormat = depthStencilFormat;
			MultiSampleAALevel = msaaLevel;
		}

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
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonRenderTarget"/> class.
		/// </summary>
		/// <param name="graphics">The graphics instance that owns this render target.</param>
		/// <param name="name">The name.</param>
		/// <param name="width">Width of the render target.</param>
		/// <param name="height">Height of the render target.</param>
		/// <param name="depthStencilFormat">The depth buffer format (if required) for the target.</param>
		/// <param name="msaaLevel">The quality level for multi-sampling anti-aliasing.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.</exception>
		/// <remarks>Passing <see cref="E:GorgonLibrary.Graphics.GorgonBufferFormat.Unknown">GorgonBufferFormat.Unknown</see> will skip the creation of the depth/stencil buffer.</remarks>
		protected GorgonRenderTarget(GorgonGraphics graphics, string name, int width, int height, GorgonBufferFormat depthStencilFormat, GorgonMSAAQualityLevel? msaaLevel)
			: base(name)
		{
			if (graphics == null)
				throw new ArgumentNullException("graphics");

			Graphics = graphics;
			UpdateTargetInformation(width, height, depthStencilFormat, msaaLevel);
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
