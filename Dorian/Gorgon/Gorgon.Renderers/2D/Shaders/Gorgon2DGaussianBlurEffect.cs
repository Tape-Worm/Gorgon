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
// Created: Wednesday, April 04, 2012 3:59:56 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimMath;
using GorgonLibrary.Math;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Renderers
{
	/// <summary>
	/// An effect that with blur an image using Gaussian blurring.
	/// </summary>
	/// <remarks>This effect differs from the others in that it does not take an Action to render data.  It merely uses a <see cref="P:GorgonLibrary.Renderers.Gorgon2DGaussianBlurEffect.SourceTexture">source texture</see> and outputs to a destination render target.</remarks>
	public class Gorgon2DGaussianBlurEffect
		: Gorgon2DEffect
	{
		#region Variables.
		private bool _disposed = false;										// Flag to indicate that the object was disposed.
		private Vector4[] _xOffsets = null;									// Calculated offsets.
		private Vector4[] _yOffsets = null;									// Calculated offsets.
		private Vector4[] _kernel = null;									// Blur kernel.
		private int _blurRadius = 6;										// Radius for the blur.
		private float _blurAmount = 3.0f;									// Amount to blur.
		private GorgonConstantBuffer _blurBuffer = null;					// Buffer for blur data.
		private GorgonConstantBuffer _blurStaticBuffer = null;				// Buffer for blur data that does not change very often.
		private GorgonDataStream _blurStream = null;						// Stream for the buffer.
		private GorgonRenderTarget _hTarget = null;							// Horizontal blur render target.
		private GorgonRenderTarget _vTarget = null;							// Vertical blur render target.
		private GorgonTexture2D _sourceTexture = null;						// Source texture to blur.
		private Vector2 _outputLocation = Vector2.Zero;						// Output location.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the radius for the blur effect.
		/// </summary>
		/// <remarks>Higher values will increase the quality of the blur, but will cause the shader to run slower.
		/// <para>The valid range of values are 1-6.  Higher values are not allowed due to the limit on the number of constants (32) for SM2_a_b video devices.</para></remarks>
		public int BlurRadius
		{
			get
			{
				return _blurRadius;
			}
			set
			{
				if (value > 6)
					value = 6;
				if (value < 1)
					value = 1;

				if (_blurRadius != value)
				{
					_blurRadius = value;
					UpdateKernel();
					UpdateOffsets();
				}
			}
		}

		/// <summary>
		/// Property to set or return the amount to blur.
		/// </summary>
		/// <remarks>Lower values will be more blurry, higher will result in a sharper image.</remarks>
		public float BlurAmount
		{
			get
			{
				return _blurAmount;
			}
			set
			{
				if (value < 1e-6f)
					value = 1e-6f;

				if (_blurAmount != value)
				{
					_blurAmount = value;
					UpdateKernel();
				}
			}
		}

		/// <summary>
		/// Property to set or return the texture that is to be blurred.
		/// </summary>
		public GorgonTexture2D SourceTexture
		{
			get
			{
				return _sourceTexture;
			}
			set
			{
				if (value != _sourceTexture)
				{
					_sourceTexture = value;
					UpdateRenderTarget();
				}
			}
		}

		/// <summary>
		/// Property to set or return the target write to.
		/// </summary>
		public GorgonRenderTarget OutputTarget
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the render target.
		/// </summary>
		private void UpdateRenderTarget()
		{
			if (_sourceTexture == null)
				throw new GorgonException(GorgonResult.CannotCreate, "Cannot create blurred effect.  No source texture is present.");

			if ((_vTarget == null) ||
				(_vTarget.Settings.Width != _sourceTexture.Settings.Width) ||
				(_vTarget.Settings.Height != _sourceTexture.Settings.Height) ||
				(_vTarget.Settings.Format != _sourceTexture.Settings.Format) ||
				(_hTarget.Settings.Width != _sourceTexture.Settings.Width) ||
				(_hTarget.Settings.Height != _sourceTexture.Settings.Height) ||
				(_hTarget.Settings.Format != _sourceTexture.Settings.Format))
			{
				if (_vTarget != null)
					_vTarget.Dispose();
				if (_hTarget != null)
					_hTarget.Dispose();
				
				_vTarget = Graphics.Output.CreateRenderTarget("Effect.Blur.Target_Vertical", new GorgonRenderTargetSettings()
				{
					Width = _sourceTexture.Settings.Width,
					Height = _sourceTexture.Settings.Height,
					Format = _sourceTexture.Settings.Format,
					DepthStencilFormat = BufferFormat.Unknown,
					MultiSample = _sourceTexture.Settings.Multisampling
				});

				_hTarget = Graphics.Output.CreateRenderTarget("Effect.Blur.Target_Horizontal", _vTarget.Settings);

				_vTarget.Clear(System.Drawing.Color.Transparent);
				_hTarget.Clear(System.Drawing.Color.Transparent);
				UpdateOffsets();
			}
		}

		/// <summary>
		/// Function to update the offsets for the shader.
		/// </summary>
		private void UpdateOffsets()
		{
			int index = 0;

			for (int i = -_blurRadius; i <= _blurRadius; i++)
			{
				_xOffsets[index] = new Vector4((1.0f / _sourceTexture.Settings.Width) * (float)i, 0, 0, 0);
				_yOffsets[index] = new Vector4(0, (1.0f / _sourceTexture.Settings.Height) * (float)i, 0, 0);
				index++;
			}
		}

		/// <summary>
		/// Function to update the blur kernel.
		/// </summary>
		/// <remarks>This implementation is ported from the Java code appearing in "Filthy Rich Clients: Developing Animated and Graphical Effects for Desktop Java".</remarks>
		private void UpdateKernel()
		{
			float sigma = _blurRadius / _blurAmount;
			float sqSigmaDouble = 2.0f * sigma * sigma;
			float sigmaRoot = GorgonMathUtility.Sqrt(sqSigmaDouble * GorgonMathUtility.PI);
			float total = 0.0f;
			float distance = 0.0f;
			int blurKernelSize = (_blurRadius * 2) + 1;
			int index = 0;

			for (int i = -_blurRadius; i <= _blurRadius; i++)
			{
				distance = i * i;
				_kernel[index] = new Vector4(0, 0, 0, GorgonMathUtility.Exp(-distance / sqSigmaDouble) / sigmaRoot);
				total += _kernel[index].W;
				index++;
			}

			for (int i = 0; i < blurKernelSize; i++)
				_kernel[i].X /= total;


			// Send to constant buffer.
			_blurStream.Position = 0;
			_blurStream.Write(_blurRadius);
			_blurStream.WriteRange<Vector4>(_kernel);
			_blurStream.Position = 0;
			_blurStaticBuffer.Update(_blurStream);
		}

		/// <summary>
		/// Function called when a pass is about to start rendering.
		/// </summary>
		/// <param name="passIndex">Index of the pass being rendered.</param>
		protected override void OnBeforeRenderPass(int passIndex)
		{
			base.OnBeforeRenderPass(passIndex);

			if (SourceTexture == null)
				throw new GorgonException(GorgonResult.CannotWrite, "Cannot create the blurred image.  No source texture was bound.");

			_blurStream.Position = 0;
			if (passIndex == 0)
				_blurStream.WriteRange(_xOffsets);
			else
				_blurStream.WriteRange(_yOffsets);
			_blurStream.Position = 0;
			_blurBuffer.Update(_blurStream);

			if (Gorgon2D.PixelShader.ConstantBuffers[2] != _blurStaticBuffer)
				Gorgon2D.PixelShader.ConstantBuffers[2] = _blurStaticBuffer;
			if (Gorgon2D.PixelShader.ConstantBuffers[3] != _blurBuffer)
				Gorgon2D.PixelShader.ConstantBuffers[3] = _blurBuffer;
		}

		/// <summary>
		/// Function to render a specific pass while using this effect.
		/// </summary>
		/// <param name="renderMethod">Method to use to render the data.</param>
		/// <param name="passIndex">Index of the pass to render.</param>
		protected override void RenderImpl(Action<int> renderMethod, int passIndex)
		{
			SmoothingMode previousMode = Gorgon2D.Drawing.SmoothingMode;
			Gorgon2D.Drawing.SmoothingMode = SmoothingMode.Smooth;
			if (passIndex == 0)
			{
				Gorgon2D.Target = _hTarget;
				Gorgon2D.Drawing.Blit(SourceTexture, Vector2.Zero, new Vector2((float)SourceTexture.Settings.Width / (float)_vTarget.Settings.Width, (float)SourceTexture.Settings.Height / (float)_vTarget.Settings.Height));			
			}
			else			
			{
				Gorgon2D.Target = _vTarget;
				Gorgon2D.Drawing.Blit(_hTarget, Vector2.Zero);
				Gorgon2D.Target = OutputTarget;				
				Gorgon2D.Drawing.Blit(_vTarget, _outputLocation, new Vector2((float)Gorgon2D.Target.Settings.Width / (float)_vTarget.Settings.Width, (float)Gorgon2D.Target.Settings.Height / (float)_vTarget.Settings.Height));
			}
			Gorgon2D.Drawing.SmoothingMode = previousMode;
		}

		/// <summary>
		/// Function to render the blurred image at the specified location.
		/// </summary>
		/// <param name="position">Position to place the image.</param>
		public void Render(Vector2 position)
		{
			_outputLocation = position;
			Render();
			_outputLocation = Vector2.Zero;
		}

		/// <summary>
		/// Function called after a pass has rendered.
		/// </summary>
		/// <param name="passIndex">Index of the pass being rendered.</param>
		protected override void OnAfterRenderPass(int passIndex)
		{
			base.OnAfterRenderPass(passIndex);
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					if (_hTarget != null)
					    _hTarget.Dispose();
					if (_vTarget != null)
					    _vTarget.Dispose();
					if (_blurStaticBuffer != null)
						_blurStaticBuffer.Dispose();
					if (_blurBuffer != null)
						_blurBuffer.Dispose();
					if (_blurStream != null)
						_blurStream.Dispose();
					if (PixelShader != null)
						PixelShader.Dispose();
				}

				PixelShader = null;
				_disposed = true;
			}

			base.Dispose(disposing);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Gorgon2DGaussianBlurEffect"/> class.
		/// </summary>
		/// <param name="graphics">Graphics interface that created this object.</param>
		internal Gorgon2DGaussianBlurEffect(Gorgon2D graphics)
			: base(graphics, "Effect.GaussBlur", 2)
		{
			_xOffsets = new Vector4[13];
			_yOffsets = new Vector4[13];
			_kernel = new Vector4[13];
			_blurBuffer = Graphics.Shaders.CreateConstantBuffer(256, false);
			_blurStaticBuffer = Graphics.Shaders.CreateConstantBuffer(256, false);
			_blurStream = new GorgonDataStream(256);
#if DEBUG
			PixelShader = Graphics.Shaders.CreateShader<GorgonPixelShader>("Effect.PS.GaussBlur", "GorgonGaussBlur", "#GorgonInclude \"Gorgon2DShaders\"", true);
#else
			PixelShader = Graphics.Shaders.CreateShader<GorgonPixelShader>("Effect.PS.GaussBlur", "GorgonGaussBlur", "#GorgonInclude \"Gorgon2DShaders\"", true);
#endif
			UpdateKernel();
		}
		#endregion
	}
}