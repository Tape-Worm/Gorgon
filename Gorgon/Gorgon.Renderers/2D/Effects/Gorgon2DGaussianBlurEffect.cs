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
using System.Diagnostics;
using System.Drawing;
using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.IO;
using Gorgon.Math;
using Gorgon.Native;
using Gorgon.Renderers.Properties;
using SlimMath;

namespace Gorgon.Renderers
{
	/// <summary>
	/// An effect that with blur an image using Gaussian blurring.
	/// </summary>
	public class Gorgon2DGaussianBlurEffect
		: Gorgon2DEffect
    {
        #region Variables.
        private bool _disposed ;													// Flag to indicate that the object was disposed.
		private Vector4[] _xOffsets;										        // Calculated offsets.
		private Vector4[] _yOffsets;										        // Calculated offsets.
		private float[] _kernel;											        // Blur kernel.
		private int _blurRadius = 6;												// Radius for the blur.
		private float _blurAmount = 3.0f;											// Amount to blur.
		private GorgonConstantBuffer _blurBuffer;							        // Buffer for blur data.
		private GorgonConstantBuffer _blurStaticBuffer;					            // Buffer for blur data that does not change very often.
		private GorgonDataStream _blurKernelStream;						            // Stream for the kernel buffer.
		private GorgonRenderTarget2D _hTarget;										// Horizontal blur render target.
		private GorgonRenderTarget2D _vTarget;										// Vertical blur render target.
		private BufferFormat _blurTargetFormat = BufferFormat.R8G8B8A8_UIntNormal;	// Format of the blur render targets.
		private Size _blurTargetSize = new Size(256, 256);							// Size of the render targets used for blurring.
		private GorgonSprite _blurSprite;									        // Sprite used to blur.
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
				if (_blurRadius == value)
				{
					return;
				}

				if (value > 6)
				{
					value = 6;
				}

				if (value < 1)
				{
					value = 1;
				}

				_blurRadius = value;
				UpdateKernel();
				UpdateOffsets();
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
				// ReSharper disable once CompareOfFloatsByEqualityOperator
				if (_blurAmount == value)
				{
					return;
				}

				if (value < 1e-6f)
				{
					value = 1e-6f;
				}

				_blurAmount = value;
				UpdateKernel();
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
				if (_blurTargetFormat == value)
				{
					return;
				}

				_blurTargetFormat = value;
				UpdateRenderTarget();
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
				if (_blurTargetSize == value)
				{
					return;
				}

				// Constrain the size.
				if (value.Width < 1)
				{
					value.Width = 1;
				}

				if (value.Height < 1)
				{
					value.Height = 1;
				}

				if (value.Width >= Graphics.Textures.MaxWidth)
				{
					value.Width = Graphics.Textures.MaxWidth - 1;
				}

				if (value.Height >= Graphics.Textures.MaxHeight)
				{
					value.Height = Graphics.Textures.MaxHeight - 1;
				}

				_blurTargetSize = value;
				UpdateRenderTarget();
			}
		}

		/// <summary>
		/// Property to set or return the function used to render the scene when blurring.
		/// </summary>
		/// <remarks>Use this to render the image to be blurred.</remarks>
		public Action<GorgonEffectPass> RenderScene
		{
			get
			{
				return Passes[0].RenderAction;
			}
			set
			{
				Passes[0].RenderAction = value;
			}
		}

		/// <summary>
		/// Property to return the resulting blurred image data as a texture.
		/// </summary>
		public GorgonTexture2D Output
		{
			get
			{
				return _vTarget;
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

			var settings = new GorgonRenderTarget2DSettings
			{
				Width = BlurRenderTargetsSize.Width,
				Height = BlurRenderTargetsSize.Height,
				Format = BlurTargetFormat,
				DepthStencilFormat = BufferFormat.Unknown,
				Multisampling = GorgonMultisampling.NoMultiSampling
			};

			_hTarget = Graphics.Output.CreateRenderTarget("Effect.GaussBlur.Target_Horizontal", settings);
			_vTarget = Graphics.Output.CreateRenderTarget("Effect.GaussBlur.Target_Vertical", settings);
			_blurSprite.Texture = _hTarget;
			_blurSprite.Size = BlurRenderTargetsSize;
			UpdateOffsets();
		}

		/// <summary>
		/// Function to update the offsets for the shader.
		/// </summary>
		private void UpdateOffsets()
		{
			int index = 0;
			var unitSize = new Vector2(1.0f / BlurRenderTargetsSize.Width, 1.0f / BlurRenderTargetsSize.Height);

			if (_vTarget == null)
			{
				UpdateRenderTarget();

				Debug.Assert(_vTarget != null, "_vTarget != null");
			}

			for (int i = -_blurRadius; i <= _blurRadius; i++)
			{
				_xOffsets[index] = new Vector2(unitSize.X * i, 0);
				_yOffsets[index] = new Vector4(0, unitSize.Y * i, 0, 0);
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
			int blurKernelSize = (_blurRadius * 2) + 1;

			for (int i = -_blurRadius, index = 0; i <= _blurRadius; ++i, ++index)
			{
				float distance = i * i;
				_kernel[index] = (-distance / sqSigmaDouble).Exp() / sigmaRoot;
				total += _kernel[index];
			}

			_blurKernelStream.Position = 0;
			_blurKernelStream.Write(_blurRadius);

			for (int i = 0; i < blurKernelSize; i++)
			{
				_blurKernelStream.Write(new Vector4(0, 0, 0, _kernel[i] / total));
			}
			
			// Send to constant buffer.
			_blurKernelStream.Position = 0;
			_blurStaticBuffer.Update(_blurKernelStream);
		}

        /// <summary>
        /// Function called when the effect is being initialized.
        /// </summary>
        /// <remarks>
        /// Use this method to set up the effect upon its creation.  For example, this method could be used to create the required shaders for the effect.
        /// </remarks>
	    protected override void OnInitialize()
	    {
	        base.OnInitialize();

            _xOffsets = new Vector4[13];
            _yOffsets = new Vector4[13];
            _kernel = new float[13];
            _blurBuffer = Graphics.ImmediateContext.Buffers.CreateConstantBuffer("Gorgon2DGaussianBlurEffect Constant Buffer",
                                                                new GorgonConstantBufferSettings
                                                                {
                                                                    SizeInBytes = DirectAccess.SizeOf<Vector4>() * _xOffsets.Length
                                                                });
            _blurStaticBuffer = Graphics.ImmediateContext.Buffers.CreateConstantBuffer("Gorgon2DGaussianBlurEffect Static Constant Buffer",
                                                                new GorgonConstantBufferSettings
                                                                {
                                                                    SizeInBytes = DirectAccess.SizeOf<Vector4>() * (_kernel.Length + 1)
                                                                });

            _blurKernelStream = new GorgonDataStream(_blurStaticBuffer.SizeInBytes);

            Passes[0].PixelShader = Graphics.ImmediateContext.Shaders.CreateShader<GorgonPixelShader>("Effect.PS.GaussBlur", "GorgonPixelShaderGaussBlur", "#GorgonInclude \"Gorgon2DShaders\"");

            UpdateKernel();

            _blurSprite = Gorgon2D.Renderables.CreateSprite("Gorgon2DGaussianBlurEffect Sprite", new GorgonSpriteSettings
            {
                Size = BlurRenderTargetsSize
            });

            _blurSprite.BlendingMode = BlendingMode.None;
            _blurSprite.SmoothingMode = SmoothingMode.Smooth;
            _blurSprite.TextureRegion = new RectangleF(0, 0, 1, 1);
        }

	    /// <summary>
		/// Function to free any resources allocated by the effect.
		/// </summary>
		public void FreeResources()
		{
			if (_hTarget != null)
			{
				_hTarget.Dispose();
			}
			if (_vTarget != null)
			{
				_vTarget.Dispose();
			}

			_hTarget = null;
			_vTarget = null;
		}

		/// <summary>
		/// Function called before rendering begins.
		/// </summary>
		/// <returns>
		/// <c>true</c> to continue rendering, <c>false</c> to exit.
		/// </returns>
		protected override bool OnBeforeRender()
		{
			if ((_hTarget == null) || (_vTarget == null))
			{
				UpdateRenderTarget();
			}

#if DEBUG
			if ((_hTarget == null) || (_vTarget == null))
			{
				throw new GorgonException(GorgonResult.CannotWrite, Resources.GOR2D_EFFECT_NO_BLUR_TARGETS);
			}
#endif
            RememberConstantBuffer(ShaderType.Pixel, 1);
            RememberConstantBuffer(ShaderType.Pixel, 2);

			// Assign the constant buffers to the pixel shader.
			Gorgon2D.PixelShader.ConstantBuffers[1] = _blurStaticBuffer;
			Gorgon2D.PixelShader.ConstantBuffers[2] = _blurBuffer;

			return base.OnBeforeRender();
		}

		/// <summary>
		/// Function called after rendering ends.
		/// </summary>
		protected override void OnAfterRender()
		{
            RestoreConstantBuffer(ShaderType.Pixel, 1);
            RestoreConstantBuffer(ShaderType.Pixel, 2);

			base.OnAfterRender();
		}

		/// <summary>
		/// Function to render a pass.
		/// </summary>
		/// <param name="pass">Pass that is to be rendered.</param>
		protected override void OnRenderPass(GorgonEffectPass pass)
		{
			Gorgon2D.Target = _hTarget;

			// Render horizontal pass.
			_hTarget.Clear(GorgonColor.Transparent);
			_blurBuffer.Update(_xOffsets);

			// Render the scene.
			pass.RenderAction(pass);

			// Render vertical pass.
			Gorgon2D.Target = _vTarget;
			_blurBuffer.Update(_yOffsets);

			_blurSprite.Draw();
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
					{
						_blurStaticBuffer.Dispose();
					}
					if (_blurBuffer != null)
					{
						_blurBuffer.Dispose();
					}
					if (_blurKernelStream != null)
					{
						_blurKernelStream.Dispose();
					}
					if (Passes[0].PixelShader != null)
					{
						Passes[0].PixelShader.Dispose();
					}

					FreeResources();
				}

				Passes[0].PixelShader = null;
				_disposed = true;
			}

			base.Dispose(disposing);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Gorgon2DGaussianBlurEffect"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface that owns this effect.</param>
		/// <param name="name">The name of the effect.</param>
		internal Gorgon2DGaussianBlurEffect(GorgonGraphics graphics, string name)
			: base(graphics, name, 1)
		{
		}
		#endregion
	}
}