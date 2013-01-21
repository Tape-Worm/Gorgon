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
using System.Drawing;
using SlimMath;
using GorgonLibrary.Math;
using GorgonLibrary.Graphics;
using GorgonLibrary.IO;

namespace GorgonLibrary.Renderers
{
	/// <summary>
	/// An effect that with blur an image using Gaussian blurring.
	/// </summary>
	/// <remarks>
	/// This effect differs from the others in that it does not take an Action to render data.  It merely uses a <see cref="P:GorgonLibrary.Renderers.Gorgon2DGaussianBlurEffect.SourceTexture">source texture</see> and outputs to a destination render target.
	/// </remarks>
	public class Gorgon2DGaussianBlurEffect
		: Gorgon2DEffect
	{
		#region Variables.
		private bool _disposed = false;												// Flag to indicate that the object was disposed.
		private Vector4[] _xOffsets = null;											// Calculated offsets.
		private Vector4[] _yOffsets = null;											// Calculated offsets.
		private Vector4[] _kernel = null;											// Blur kernel.
		private int _blurRadius = 6;												// Radius for the blur.
		private float _blurAmount = 3.0f;											// Amount to blur.
		private GorgonConstantBuffer _blurBuffer = null;							// Buffer for blur data.
		private GorgonConstantBuffer _blurStaticBuffer = null;						// Buffer for blur data that does not change very often.
		private GorgonDataStream _blurStream = null;								// Stream for the buffer.
		private GorgonRenderTarget _hTarget = null;									// Horizontal blur render target.
		private GorgonRenderTarget _vTarget = null;									// Vertical blur render target.
		private BufferFormat _blurTargetFormat = BufferFormat.R8G8B8A8_UIntNormal;	// Format of the blur render targets.
		private Size _blurTargetSize = new Size(256, 256);							// Size of the render targets used for blurring.
		private GorgonRenderTarget _currentTarget = null;							// Current render target.
		private SmoothingMode _lastSmoothMode = SmoothingMode.None;					// Last smoothing mode.
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
		/// Property to set or return the format of the internal render targets used for blurring.
		/// </summary>
		public BufferFormat BlurTargetFormat
		{
			get
			{
				return _blurTargetFormat;
			}
			set
			{
				if (_blurTargetFormat != value)
				{
					_blurTargetFormat = value;
					UpdateRenderTarget();
				}
			}
		}

		/// <summary>
		/// Property to set or return the size of the internal render targets used for blurring.
		/// </summary>
		public Size BlurRenderTargetsSize
		{
			get
			{
				return _blurTargetSize;
			}
			set
			{
				// Constrain the size.
				if (value.Width < 1)
					value.Width = 1;
				if (value.Height < 1)
					value.Height = 1;
				if (value.Width >= Graphics.Textures.MaxWidth)
					value.Width = Graphics.Textures.MaxWidth - 1;
				if (value.Height >= Graphics.Textures.MaxHeight)
					value.Height = Graphics.Textures.MaxHeight - 1;

				if (_blurTargetSize != value)
				{
					_blurTargetSize = value;
					UpdateRenderTarget();
				}
			}
		}

		/// <summary>
		/// Property to return the resulting blurred image data as a texture.
		/// </summary>
		public GorgonTexture2D BlurredTexture
		{
			get
			{
				if (_vTarget == null)
					return null;

				return _vTarget.Texture;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the render target.
		/// </summary>
		private void UpdateRenderTarget()
		{
			FreeResources();

			GorgonRenderTargetSettings settings = new GorgonRenderTargetSettings()
			{
				Width = BlurRenderTargetsSize.Width,
				Height = BlurRenderTargetsSize.Height,
				Format = BlurTargetFormat,
				DepthStencilFormat = BufferFormat.Unknown,
				MultiSample = new GorgonMultisampling(1, 0)
			};

			_hTarget = Graphics.Output.CreateRenderTarget("Effect.GaussBlur.Target_Horizontal", settings);
			_vTarget = Graphics.Output.CreateRenderTarget("Effect.GaussBlur.Target_Vertical", settings);
			UpdateOffsets();
		}

		/// <summary>
		/// Function to update the offsets for the shader.
		/// </summary>
		private void UpdateOffsets()
		{
			int index = 0;

			if (_vTarget == null)
			{
				UpdateRenderTarget();
			}


			for (int i = -_blurRadius; i <= _blurRadius; i++)
			{
				_xOffsets[index] = new Vector4((1.0f / _vTarget.Settings.Width) * (float)i, 0, 0, 0);
				_yOffsets[index] = new Vector4(0, (1.0f / _vTarget.Settings.Height) * (float)i, 0, 0);
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
			float sigmaRoot = (sqSigmaDouble * (float)System.Math.PI).Sqrt();
			float total = 0.0f;
			float distance = 0.0f;
			int blurKernelSize = (_blurRadius * 2) + 1;
			int index = 0;

			for (int i = -_blurRadius; i <= _blurRadius; i++)
			{
				distance = i * i;
				_kernel[index] = new Vector4(0, 0, 0, (-distance / sqSigmaDouble).Exp() / sigmaRoot);
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
		/// Function to free any resources allocated by the effect.
		/// </summary>
		public override void FreeResources()
		{
			if (_hTarget != null)
				_hTarget.Dispose();
			if (_vTarget != null)
				_vTarget.Dispose();

			_hTarget = null;
			_vTarget = null;
		}

		/// <summary>
		/// Function called when a pass is about to start rendering.
		/// </summary>
		/// <param name="passIndex">Index of the pass being rendered.</param>
		protected override void OnBeforeRenderPass(int passIndex)
		{
			base.OnBeforeRenderPass(passIndex);

			if ((_hTarget == null) || (_vTarget == null))
				UpdateRenderTarget();

			_blurStream.Position = 0;
			if (passIndex == 0)
			{
				// Get the current target.
				_currentTarget = Gorgon2D.Target;

				_hTarget.Clear(GorgonColor.Transparent);
				Gorgon2D.Target = _hTarget;
				_blurStream.WriteRange(_xOffsets);
			}
			else
			{
				Gorgon2D.Target = _vTarget;
				_blurStream.WriteRange(_yOffsets);
			}

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
			if ((_hTarget == null) || (_vTarget == null))
				UpdateRenderTarget();

			if (passIndex == 0)
				Gorgon2D.Target = _hTarget;
			else			
			{
				// Copy the horizontal pass to the vertical pass target and blur the result.
				_lastSmoothMode = Gorgon2D.Drawing.SmoothingMode;
				Gorgon2D.Drawing.SmoothingMode = SmoothingMode.Smooth;
				Gorgon2D.Drawing.Blit(_hTarget, Vector2.Zero);
				Gorgon2D.Target = _currentTarget;
			}

			base.RenderImpl(renderMethod, passIndex);
		}

		/// <summary>
		/// Function called after a pass has rendered.
		/// </summary>
		/// <param name="passIndex">Index of the pass being rendered.</param>
		protected override void OnAfterRenderPass(int passIndex)
		{
			base.OnAfterRenderPass(passIndex);

			if (passIndex == 1)
				Gorgon2D.Drawing.SmoothingMode = _lastSmoothMode;
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
			PixelShader = Graphics.Shaders.CreateShader<GorgonPixelShader>("Effect.PS.GaussBlur", "GorgonPixelShaderGaussBlur", "#GorgonInclude \"Gorgon2DShaders\"", true);
#else
			PixelShader = Graphics.Shaders.CreateShader<GorgonPixelShader>("Effect.PS.GaussBlur", "GorgonPixelShaderGaussBlur", "#GorgonInclude \"Gorgon2DShaders\"", true);
#endif
			UpdateKernel();
		}
		#endregion
	}
}