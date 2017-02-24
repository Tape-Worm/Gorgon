#region MIT
// 
// Gorgon.
// Copyright (C) 2016 Michael Winsor
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
// Created: July 28, 2016 11:29:06 PM
// 
#endregion

using System;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
	/// <summary>
	/// A state object used to bind texture sampler states to the pipeline.
	/// </summary>
	public class GorgonSamplerState
		: IDisposable
	{
		#region Variables.
		// The state information for this state.
		private readonly GorgonSamplerStateInfo _info;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the Direct 3D 11 sampler state object.
		/// </summary>
		internal D3D11.SamplerState D3DState
		{
			get;
		}

		/// <summary>
		/// Property to return the graphics interface used to create this object.
		/// </summary>
		public GorgonGraphics Graphics
		{
			get;
		}

		/// <summary>
		/// Property to return the <see cref="IGorgonSamplerStateInfo"/> for this state.
		/// </summary>
		public IGorgonSamplerStateInfo Info => _info;
		#endregion

		#region Methods.
		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			D3DState?.Dispose();
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonSamplerState"/> class.
		/// </summary>
		/// <param name="graphics">The graphics object used to create this state.</param>
		/// <param name="info">The <see cref="IGorgonSamplerStateInfo"/> used to create this object.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, or the <paramref name="info"/> parameter is <b>null</b>.</exception>
		public GorgonSamplerState(GorgonGraphics graphics, IGorgonSamplerStateInfo info)
		{
			if (graphics == null)
			{
				throw new ArgumentNullException(nameof(graphics));
			}

			if (info == null)
			{
				throw new ArgumentNullException(nameof(info));
			}

			Graphics = graphics;
			_info = new GorgonSamplerStateInfo(info);
			D3DState = new D3D11.SamplerState(graphics.VideoDevice.D3DDevice(), _info.ToSamplerStateDesc());
		}
		#endregion
	}
}
