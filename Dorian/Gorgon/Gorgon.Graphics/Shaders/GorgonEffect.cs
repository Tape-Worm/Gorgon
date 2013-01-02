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
// Created: Monday, April 02, 2012 1:05:19 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// An abstract effect used for applying shaders to rendering.
	/// </summary>
	/// <remarks>Users may use this to implement custom shading for objects.</remarks>
	public abstract class GorgonEffect
		: GorgonNamedObject, IDisposable
	{
		#region Variables.
		private bool _disposed = false;					// Flag to indicate that the object has been disposed.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the pixel shader to use.
		/// </summary>
		protected GorgonPixelShader PixelShader
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the vertex shader to use.
		/// </summary>
		protected GorgonVertexShader VertexShader
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return a list of required parameters for the effect.
		/// </summary>
		/// <remarks>These parameters are required upon creation of the effect.  Developers implementing a new effect object 
		/// requiring data to be sent to a shader at creation should fill in this list with the names of the required 
		/// parameters.</remarks>
		protected internal IList<string> RequiredParameters
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return a list of additional parameters for the effect.
		/// </summary>
		/// <remarks>These parameters can be passed in via construction of the effect using the <see cref="M:GorgonLibrary.Graphics.GorgonShaderBinding.CreateEffect{T}">CreateEffect</see> method 
		/// or they may be updated after the object was created.
		/// </remarks>
		public IDictionary<string, object> Parameters
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the graphics interface that created this object.
		/// </summary>
		public GorgonGraphics Graphics
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the number of passes for this effect.
		/// </summary>
		public int PassCount
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to initialize the effect parameters.
		/// </summary>
		/// <remarks>This method is where the parameter set up for the effect is done.</remarks>
		protected virtual internal void InitializeEffectParameters()
		{			
		}

		/// <summary>
		/// Function called before rendering begins.
		/// </summary>
		/// <returns>TRUE to continue rendering, FALSE to exit.</returns>
		protected virtual bool OnBeforeRender()
		{
			return true;
		}

		/// <summary>
		/// Function called after rendering ends.
		/// </summary>
		protected virtual void OnAfterRender()
		{
		}

		/// <summary>
		/// Function called when a pass is about to start rendering.
		/// </summary>
		/// <param name="passIndex">Index of the pass being rendered.</param>
		protected virtual void OnBeforeRenderPass(int passIndex)
		{
		}

		/// <summary>
		/// Function called after a pass has rendered.
		/// </summary>
		/// <param name="passIndex">Index of the pass being rendered.</param>
		protected virtual void OnAfterRenderPass(int passIndex)
		{
		}

		/// <summary>
		/// Function to render a specific pass while using this effect.
		/// </summary>
		/// <param name="renderMethod">Method to use to render the data.</param>
		/// <param name="passIndex">Index of the pass to render.</param>
		/// <remarks>The <paramref name="renderMethod"/> is an action delegate that must be defined with an integer value.  The parameter indicates which pass the rendering is currently on.</remarks>
		protected virtual void RenderImpl(Action<int> renderMethod, int passIndex)
		{
			if (renderMethod != null)
				renderMethod(passIndex);
		}

		/// <summary>
		/// Function to render a specific pass while using this effect.
		/// </summary>
		/// <param name="renderMethod">Method to use to render the data.</param>
		/// <param name="passIndex">Index of the pass to render.</param>
		/// <remarks>The <paramref name="renderMethod"/> is an action delegate that must be defined with an integer value.  The parameter indicates which pass the rendering is currently on.</remarks>
		public void RenderPass(Action<int> renderMethod, int passIndex)
		{
			OnBeforeRenderPass(passIndex);
			RenderImpl(renderMethod, passIndex);
			OnAfterRenderPass(passIndex);
		}

		/// <summary>
		/// Function to render while using this effect.
		/// </summary>
		/// <param name="renderMethod">Method to use to render the data.</param>
		/// <remarks>The <paramref name="renderMethod"/> is an action delegate that must be defined with an integer value.  The parameter indicates which pass the rendering is currently on.</remarks>
		public void Render(Action<int> renderMethod)
		{
			if (!OnBeforeRender())
				return;

			for (int i = 0; i < PassCount; i++)
				RenderPass(renderMethod, i);

			OnAfterRender();
		}

		/// <summary>
		/// Function to free any resources allocated by the effect.
		/// </summary>
		public abstract void FreeResources();

		/// <summary>
		/// Function to render while using this effect.
		/// </summary>
		public void Render()
		{
			Render(null);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonEffect"/> class.
		/// </summary>
		/// <param name="graphics">Graphics interface that created this object.</param>
		/// <param name="name">The name of the effect.</param>
		/// <param name="passCount">Number of passes for the effect.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.</exception>
		protected GorgonEffect(GorgonGraphics graphics, string name, int passCount)
			: base(name)
		{
			GorgonDebug.AssertParamRange(passCount, 1, Int32.MaxValue, false, true, "passCount");

			Parameters = new Dictionary<string, object>();
			RequiredParameters = new List<string>();
			Graphics = graphics;
			PassCount = passCount;
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
					Graphics.RemoveTrackedObject(this);

					FreeResources();
				}

				_disposed = true;
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
