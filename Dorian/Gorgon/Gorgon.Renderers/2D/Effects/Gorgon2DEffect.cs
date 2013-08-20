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
// Created: Monday, April 02, 2012 1:59:48 PM
// 
#endregion

using GorgonLibrary.Graphics;

namespace GorgonLibrary.Renderers
{
	/// <summary>
	/// A base effect for the 2D renderer.
	/// </summary>
	public abstract class Gorgon2DEffect
		: GorgonEffect
	{
		#region Variables.
		private bool _isDisposed;							// Flag to indicate that the object was disposed.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the current render target.
		/// </summary>
		public GorgonRenderTargetView CurrentTarget
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the Gorgon 2D instance that created this object.
		/// </summary>
		public Gorgon2D Gorgon2D
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function called before rendering begins.
		/// </summary>
		/// <returns>
		/// TRUE to continue rendering, FALSE to exit.
		/// </returns>
		protected override bool OnBeforeRender()
		{
			CurrentTarget = Gorgon2D.Target;

			return true;
		}

		/// <summary>
		/// Function called before a pass is rendered.
		/// </summary>
		/// <param name="pass">Pass to render.</param>
		/// <returns>
		/// TRUE to continue rendering, FALSE to stop.
		/// </returns>
		protected override bool OnBeforePassRender(GorgonEffectPass pass)
		{
			StoredShaders.PixelShader = Gorgon2D.PixelShader.Current;
			StoredShaders.VertexShader = Gorgon2D.VertexShader.Current;

			Gorgon2D.PixelShader.Current = pass.PixelShader;
			Gorgon2D.VertexShader.Current = pass.VertexShader;

			return true;
		}

		/// <summary>
		/// Function called after a pass has been rendered.
		/// </summary>
		/// <param name="pass">Pass that was rendered.</param>
		protected override void OnAfterPassRender(GorgonEffectPass pass)
		{
			Gorgon2D.PixelShader.Current = StoredShaders.PixelShader;
			Gorgon2D.VertexShader.Current = StoredShaders.VertexShader;
		}

		/// <summary>
		/// Function called after rendering ends.
		/// </summary>
		protected override void OnAfterRender()
		{
			Gorgon2D.Target = CurrentTarget;
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			if (!_isDisposed)
			{
				if (disposing)
				{
					Gorgon2D.TrackedObjects.Remove(this);
				}
				
				_isDisposed = true;
			}
			base.Dispose(disposing);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Gorgon2DEffect"/> class.
		/// </summary>
		/// <param name="gorgon2D">The gorgon 2D instance that created this object.</param>
		/// <param name="name">The name of the effect.</param>
		/// <param name="passCount">The number of passes..</param>
		protected Gorgon2DEffect(Gorgon2D gorgon2D, string name, int passCount)
			: base(gorgon2D.Graphics, name, passCount)
		{
			Gorgon2D = gorgon2D;
		}
		#endregion
	}
}
