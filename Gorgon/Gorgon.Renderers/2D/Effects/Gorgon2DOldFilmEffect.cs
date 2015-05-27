#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Thursday, August 22, 2013 11:54:51 PM
// 
#endregion

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using Gorgon.Math;
using Gorgon.Native;
using Gorgon.Graphics;
using Gorgon.Renderers.Properties;
using SlimMath;

namespace Gorgon.Renderers
{
	/// <summary>
	/// A post process effect to give an old scratched film effect.
	/// </summary>
	public class Gorgon2DOldFilmEffect
		: Gorgon2DEffect
	{
		#region Value Types.
		/// <summary>
		/// Settings for scratches.
		/// </summary>
		[StructLayout(LayoutKind.Sequential, Pack = 16, Size = 16)]
		private struct ScratchSettings
		{
			/// <summary>
			/// Speed of a scratch.
			/// </summary>
			public float ScratchVisibleTime;
			/// <summary>
			/// Speed of a scratch scroll.
			/// </summary>
			public float ScratchScrollSpeed;
			/// <summary>
			/// Intensity of a scratch.
			/// </summary>
			public float ScratchIntensity;
			/// <summary>
			/// Width of a scratch.
			/// </summary>
			public float ScratchWidth;
		}

		/// <summary>
		/// Settings for sepia tone.
		/// </summary>
		[StructLayout(LayoutKind.Sequential, Pack = 16, Size = 48)]
		private struct SepiaSettings
		{
			/// <summary>
			/// Lightest value for sepia.
			/// </summary>
			public GorgonColor SepiaLightColor;
			/// <summary>
			/// Darkest value for sepa.
			/// </summary>
			public GorgonColor SepiaDarkColor;

			/// <summary>
			/// Desaturation of color.
			/// </summary>
			public float SepiaDesaturationAmount;
			/// <summary>
			/// Tone for sepia color.
			/// </summary>
			public float SepiaToneAmount;
		}
		#endregion

		#region Variables.
		private bool _disposed;									// Flag to indicate that the effect was disposed.
		private GorgonTexture2D _randomTexture;					// Texture used to hold random noise for the shader.
		private GorgonPoint _point;								// Point used for dust.
		private GorgonConstantBuffer _timingBuffer;				// Constant buffer for timing.
		private Vector2 _currentTargetSize;						// Current target size.
		private bool _isScratchUpdated = true;					// Flag to indicate whether the effect has updated parameters or not.
		private bool _isSepiaUpdated = true;					// Flag to indicate whether the effect has updated parameters or not.
		private ScratchSettings _scratchSettings;				// Settings for film scratches.
		private SepiaSettings _sepiaSettings;					// Settings for sepia tone.
		private GorgonConstantBuffer _scratchBuffer;			// Constant buffer for scratch settings.
		private GorgonConstantBuffer _sepiaBuffer;				// Constant buffer for sepia settings.
		private float _noiseFrequency = 42.0f;					// Noise frequency.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the noise frequency used in generating scratches in the film.
		/// </summary>
		/// <remarks>It is not recommended to update this value every frame, doing so may have a significant performance hit.</remarks>
		public float NoiseFrequency
		{
			get
			{
				return _noiseFrequency;
			}
			set
			{
				// ReSharper disable once CompareOfFloatsByEqualityOperator
				if (_noiseFrequency == value)
				{
					return;
				}

				if (value < 1)
				{
					value = 1.0f;
				}

				_noiseFrequency = value;

				// Prepare the random data texture for update.
				if (_randomTexture == null)
				{
					return;
				}

				_randomTexture.Dispose();
				_randomTexture = null;
			}
		}

		/// <summary>
		/// Property to set or return the speed of updates to the current set of scratches.
		/// </summary>
		/// <remarks>Smaller values keep the current scratches on the screen longer.</remarks>
		public float UpdateSpeed
		{
			get
			{
				return _scratchSettings.ScratchVisibleTime;
			}
			set
			{
				// ReSharper disable once CompareOfFloatsByEqualityOperator
				if (_scratchSettings.ScratchVisibleTime == value)
				{
					return;
				}

				_scratchSettings.ScratchVisibleTime = value;
				_isScratchUpdated = true;
			}
		}

		/// <summary>
		/// Property to set or return the speed for scratches to move horizontally.
		/// </summary>
		public float ScrollSpeed
		{
			get
			{
				return _scratchSettings.ScratchScrollSpeed;
			}
			set
			{
				// ReSharper disable once CompareOfFloatsByEqualityOperator
				if (_scratchSettings.ScratchVisibleTime == value)
				{
					return;
				}

				_scratchSettings.ScratchVisibleTime = value;
				_isScratchUpdated = true;
			}
		}

		/// <summary>
		/// Property to set or return the itensity multiplier for scratches.
		/// </summary>
		public float Intensity
		{
			get
			{
				return _scratchSettings.ScratchIntensity;
			}
			set
			{
				// ReSharper disable once CompareOfFloatsByEqualityOperator
				if (_scratchSettings.ScratchIntensity == value)
				{
					return;
				}

				_scratchSettings.ScratchIntensity = value;
				_isScratchUpdated = true;
			}
		}

		/// <summary>
		/// Property to set or return the width multiplier for scratches.
		/// </summary>
		public float ScratchWidth
		{
			get
			{
				return _scratchSettings.ScratchWidth;
			}
			set
			{
				// ReSharper disable once CompareOfFloatsByEqualityOperator
				if (_scratchSettings.ScratchWidth == value)
				{
					return;
				}

				_scratchSettings.ScratchWidth = value;
				_isScratchUpdated = true;
			}
		}

		/// <summary>
		/// Property to set or return the amount of desaturation for the current scene.
		/// </summary>
		public float DesaturationAmount
		{
			get
			{
				return _sepiaSettings.SepiaDesaturationAmount;
			}
			set
			{
				// ReSharper disable once CompareOfFloatsByEqualityOperator
				if (_sepiaSettings.SepiaDesaturationAmount == value)
				{
					return;
				}

				_sepiaSettings.SepiaDesaturationAmount = value;
				_isSepiaUpdated = true;
			}
		}

		/// <summary>
		/// Property to set or return the amount of sepia tone for the current scene.
		/// </summary>
		public float ToneAmount
		{
			get
			{
				return _sepiaSettings.SepiaToneAmount;
			}
			set
			{
				// ReSharper disable once CompareOfFloatsByEqualityOperator
				if (_sepiaSettings.SepiaToneAmount == value)
				{
					return;
				}

				_sepiaSettings.SepiaToneAmount = value;
				_isSepiaUpdated = true;
			}
		}

		/// <summary>
		/// Property to set or return the lightest color for sepia tone mapping.
		/// </summary>
		public GorgonColor SepiaLightColor
		{
			get
			{
				return _sepiaSettings.SepiaLightColor;
			}
			set
			{
				if (GorgonColor.Equals(ref _sepiaSettings.SepiaLightColor, ref value))
				{
					return;
				}

				_sepiaSettings.SepiaLightColor = value;
				_isSepiaUpdated = true;
			}
		}

		/// <summary>
		/// Property to set or return the darkest color for sepia tone mapping.
		/// </summary>
		public GorgonColor SepiaDarkColor
		{
			get
			{
				return _sepiaSettings.SepiaDarkColor;
			}
			set
			{
				if (GorgonColor.Equals(ref _sepiaSettings.SepiaDarkColor, ref value))
				{
					return;
				}

				_sepiaSettings.SepiaDarkColor = value;
				_isSepiaUpdated = true;
			}
		}

		/// <summary>
		/// Property to set or return the percentage that random dirt will appear.
		/// </summary>
		public int DirtPercent
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the amount of dirt and dust that will appear.
		/// </summary>
		public int DirtAmount
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the function used to render the scene with the effect.
		/// </summary>
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
		/// Property to set or return the current time for the effect in seconds.
		/// </summary>
		public float Time
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Funciton to generate random noise for the effect.
		/// </summary>
		private void GenerateRandomNoise()
		{
			int textureSize = 128;
		    object parameter;

		    if ((Parameters.TryGetValue("TextureSize", out parameter))
		        && (parameter != null))
		    {
		        var size = (int)parameter;

		        if (size > 16)
		        {
		            textureSize = size;
		        }
		    }

		    using(var image = new GorgonImageData(new GorgonTexture2DSettings
			{
				Width = textureSize,
				Height = textureSize,
				Format = BufferFormat.R8_UIntNormal,
				Usage = BufferUsage.Default
			}))
			{
				unsafe
				{
					var dataPtr = (byte*)image.Buffers[0].Data.UnsafePointer;

					// Write perlin noise to the texture.
					for (int y = 0; y < textureSize; ++y)
					{
						for (int x = 0; x < textureSize; ++x)
						{
							float simplexNoise = GorgonRandom.SimplexNoise(new Vector2(x * (1.0f / _noiseFrequency), y * (1.0f / _noiseFrequency)));

							if (simplexNoise < -0.75f)
							{
								simplexNoise *= -1;
							}
							else
							{
								simplexNoise *= 0.95f;
							}

							if (simplexNoise < 0.125f)
							{
								simplexNoise = 0.0f;
							}

							*(dataPtr++) = (byte)(simplexNoise * 255.0f);
						}
					}
				}

				_randomTexture = Graphics.Textures.CreateTexture<GorgonTexture2D>("Effect.OldFilm.RandomTexture", image);
			}
		}

		/// <summary>
		/// Function called when the effect is being initialized.
		/// </summary>
		/// <remarks>
		/// Use this method to set up the effect upon its creation.  For example, this method could be used to create the required shaders for the effect.
		/// <para>When creating a custom effect, use this method to initialize the effect.  Do not put initialization code in the effect constructor.</para>
		/// </remarks>
		protected override void OnInitialize()
		{
			base.OnInitialize();

			_point = Gorgon2D.Renderables.CreatePoint("Effect.OldFilm.DustPoint", Vector2.Zero, GorgonColor.Black);

			// Create pixel shader.
			Passes[0].PixelShader = Graphics.Shaders.CreateShader<GorgonPixelShader>("Effect.OldFilm.PS",
				"GorgonPixelShaderFilmGrain", Encoding.UTF8.GetString(Resources.FilmGrain));

			_timingBuffer = Graphics.Buffers.CreateConstantBuffer("Effect.OldFilm.TimingBuffer", new GorgonConstantBufferSettings
			{
				SizeInBytes = 16
			});

			_scratchBuffer = Graphics.Buffers.CreateConstantBuffer("Effect.OldFilm.ScratchSettingsBuffer",
				new GorgonConstantBufferSettings
				{
					SizeInBytes = DirectAccess.SizeOf<ScratchSettings>()
				});

			_sepiaBuffer = Graphics.Buffers.CreateConstantBuffer("Effect.OldFilm.SepaSettingsBuffer",
				new GorgonConstantBufferSettings
				{
					SizeInBytes = DirectAccess.SizeOf<SepiaSettings>()
				});

			DirtPercent = 5;
			DirtAmount = 10;

			_scratchSettings = new ScratchSettings
			{
				ScratchIntensity = 0.49f,
				ScratchScrollSpeed = 0.01f,
				ScratchVisibleTime = 0.003f,
				ScratchWidth = 0.01f
			};

			_sepiaSettings = new SepiaSettings
			{
				SepiaDesaturationAmount = 0.0f,
				SepiaToneAmount = 0.5f,
				SepiaLightColor = new GorgonColor(1, 0.9f, 0.65f, 1.0f),
				SepiaDarkColor = new GorgonColor(0.2f, 0.102f, 0, 1.0f)
			};
		}

		/// <summary>
		/// Function called before rendering begins.
		/// </summary>
		/// <returns>
		/// <c>true</c> to continue rendering, <c>false</c> to exit.
		/// </returns>
		protected override bool OnBeforeRender()
		{
			GorgonRenderTargetView target = Gorgon2D.Target;

			if (_randomTexture == null)
			{
				GenerateRandomNoise();
			}

			if (target == null)
			{
				target = Gorgon2D.DefaultTarget;
			}

			switch (target.Resource.ResourceType)
			{
				case ResourceType.Buffer:
					var buffer = (GorgonBuffer)target.Resource;
					var info = GorgonBufferFormatInfo.GetInfo(buffer.Settings.DefaultShaderViewFormat);

					_currentTargetSize = new Vector2(buffer.SizeInBytes / info.BitDepth, 1);
					break;
				case ResourceType.Texture1D:
				case ResourceType.Texture2D:
				case ResourceType.Texture3D:
					var texture = (GorgonTexture)target.Resource;
					_currentTargetSize = new Vector2(texture.Settings.Width, texture.Settings.Height);
					break;
			}

            RememberTextureSampler(ShaderType.Pixel, 1);
            RememberShaderResource(ShaderType.Pixel, 1);
            RememberConstantBuffer(ShaderType.Pixel, 1);
            RememberConstantBuffer(ShaderType.Pixel, 2);
            RememberConstantBuffer(ShaderType.Pixel, 3);

			Gorgon2D.PixelShader.Resources[1] = _randomTexture;
			Gorgon2D.PixelShader.TextureSamplers[1] = new GorgonTextureSamplerStates
			{
				TextureFilter = TextureFilter.Linear,
				HorizontalAddressing = TextureAddressing.Wrap,
				VerticalAddressing = TextureAddressing.Wrap,
				DepthAddressing = TextureAddressing.Clamp,
				MipLODBias = 0.0f,
				MaxAnisotropy = 1,
				ComparisonFunction = ComparisonOperator.Never,
				BorderColor = Color.White,
				MinLOD = -3.402823466e+38f,
				MaxLOD = 3.402823466e+38f
			};
			
			Gorgon2D.PixelShader.ConstantBuffers[1] = _timingBuffer;
			Gorgon2D.PixelShader.ConstantBuffers[2] = _scratchBuffer;
			Gorgon2D.PixelShader.ConstantBuffers[3] = _sepiaBuffer;

			return base.OnBeforeRender();
		}

		/// <summary>
		/// Function called before a pass is rendered.
		/// </summary>
		/// <param name="pass">Pass to render.</param>
		/// <returns>
		/// <c>true</c> to continue rendering, <c>false</c> to stop.
		/// </returns>
		protected override bool OnBeforePassRender(GorgonEffectPass pass)
		{
			float renderTime = Time;

			if (_isScratchUpdated)
			{
				_scratchBuffer.Update(ref _scratchSettings);
				_isScratchUpdated = false;
			}

			if (_isSepiaUpdated)
			{
				_sepiaBuffer.Update(ref _sepiaSettings);
				_isSepiaUpdated = false;
			}

			_timingBuffer.Update(ref renderTime);

			return base.OnBeforePassRender(pass);
		}

		/// <summary>
		/// Function called after a pass has been rendered.
		/// </summary>
		/// <param name="pass">Pass that was rendered.</param>
		protected override void OnAfterPassRender(GorgonEffectPass pass)
		{
			// We must set the pixel shader back to the default here.
			// Otherwise our current shader may not like that we're trying to 
			// render lines and points with no textures attached.

			// If this shader is nested with another (e.g. gauss blur), then 
			// this situation can throw warnings up in the debug spew.

			// Setting the pixel shader to the proper shader when rendering
			// primitives with no textures is the best way to correct this.
			Gorgon2D.PixelShader.Current = null;

			var blend = Gorgon2D.Drawing.Blending;

			// Render dust and dirt.
			for (int i = 0; i < DirtAmount; ++i)
			{
				float grayDust = GorgonRandom.RandomSingle(0.1f, 0.25f);
				var dustColor = new GorgonColor(grayDust, grayDust, grayDust, GorgonRandom.RandomSingle(0.25f, 0.95f));

				// Render dust points.
				_point.Color = dustColor;
				_point.PointThickness = new Vector2(1);
				_point.Position = new Vector2(GorgonRandom.RandomSingle(0, _currentTargetSize.X - 1),
					GorgonRandom.RandomSingle(0, _currentTargetSize.Y - 1));
				_point.Draw();

				if (GorgonRandom.RandomInt32(100) >= DirtPercent)
				{
					continue;
				}

				// Render dirt/hair lines.
				var dirtStart = new Vector2(GorgonRandom.RandomSingle(0, _currentTargetSize.X - 1),
					GorgonRandom.RandomSingle(0, _currentTargetSize.Y - 1));

				float dirtWidth = GorgonRandom.RandomSingle(1.0f, 3.0f);
				bool isHair = GorgonRandom.RandomInt32(100) > 50;
				bool isHairVertical = isHair && GorgonRandom.RandomInt32(100) > 50;

				grayDust = GorgonRandom.RandomSingle(0.1f, 0.15f);
				dustColor = new GorgonColor(grayDust, grayDust, grayDust, GorgonRandom.RandomSingle(0.25f, 0.95f));
				
				for (int j = 0; j < GorgonRandom.RandomInt32(4, (int)_currentTargetSize.X / 4); j++)
				{
					_point.Color = dustColor;
					_point.Position = new Vector2(dirtStart.X, dirtStart.Y);
					_point.PointThickness = isHair ? new Vector2(1) : new Vector2(dirtWidth, dirtWidth);
					_point.Draw();

					if ((!isHair) || (isHairVertical))
					{
						if (GorgonRandom.RandomInt32(100) > 50)
						{
							dirtStart.X++;
						}
						else
						{
							dirtStart.X--;
						}
					}
					else
					{
						if (grayDust < 0.25f)
						{
							dirtStart.X++;
						}
						else
						{
							dirtStart.X--;
						}
					}

					if ((!isHair) || (!isHairVertical))
					{
						if (GorgonRandom.RandomInt32(100) > 50)
						{
							dirtStart.Y++;
						}
						else
						{
							dirtStart.Y--;
						}
					}
					else
					{
						if (dirtWidth < 1.5f)
						{
							dirtStart.Y++;
						}
						else
						{
							dirtStart.Y--;
						}
					}
				}
			}

			// Restore the previous blend state.
			Gorgon2D.Drawing.Blending = blend;

			base.OnAfterPassRender(pass);
		}

		/// <summary>
		/// Function called after rendering ends.
		/// </summary>
		protected override void OnAfterRender()
		{
            RestoreTextureSampler(ShaderType.Pixel, 1);
            RestoreShaderResource(ShaderType.Pixel, 1);
            RestoreConstantBuffer(ShaderType.Pixel, 1);
            RestoreConstantBuffer(ShaderType.Pixel, 2);
            RestoreConstantBuffer(ShaderType.Pixel, 3);

			base.OnAfterRender();
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			if (!_disposed)
			{
			    if (disposing)
			    {
			        FreeResources();

			        if (Passes[0].PixelShader != null)
			        {
			            Passes[0].PixelShader.Dispose();
			            Passes[0].PixelShader = null;
			        }

			        if (_timingBuffer != null)
			        {
			            _timingBuffer.Dispose();
			            _timingBuffer = null;
			        }

			        if (_scratchBuffer != null)
			        {
			            _scratchBuffer.Dispose();
			            _scratchBuffer = null;
			        }

			        if (_sepiaBuffer != null)
			        {
			            _sepiaBuffer.Dispose();
			            _sepiaBuffer = null;
			        }
			    }

			    _disposed = true;
			}
			base.Dispose(disposing);
		}

		/// <summary>
		/// Function to clean up resources used by the effect.
		/// </summary>
		public void FreeResources()
		{
			if (_randomTexture == null)
			{
				return;
			}

			_randomTexture.Dispose();
			_randomTexture = null;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Gorgon2DOldFilmEffect"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface that owns this effect.</param>
		/// <param name="name">The name of the effect.</param>
		internal Gorgon2DOldFilmEffect(GorgonGraphics graphics, string name)
			: base(graphics, name, 1)
		{
		}
		#endregion
	}
}
