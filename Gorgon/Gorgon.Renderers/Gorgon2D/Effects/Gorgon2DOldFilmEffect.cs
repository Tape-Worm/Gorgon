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

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging;
using Gorgon.Math;
using Gorgon.Renderers.Properties;
using DX = SharpDX;

namespace Gorgon.Renderers
{
	/// <summary>
	/// A post process effect to give an old scratched film effect.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This effect animates an image to appear as though it is being displayed on old film. To animate the image, the <see cref="Time"/> property needs to be updated with a time value (in seconds) every frame.
	/// </para>
	/// </remarks>
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
	    // Texture used to hold random noise for the shader.
		private GorgonTexture2DView _randomTexture;					
	    // Constant buffer for timing.
		private GorgonConstantBufferView _timingBuffer;				
	    // Flag to indicate whether the effect has updated parameters or not.
		private bool _isScratchUpdated = true;					
	    // Flag to indicate whether the effect has updated parameters or not.
		private bool _isSepiaUpdated = true;
	    // Settings for film scratches.
		private ScratchSettings _scratchSettings;				
	    // Settings for sepia tone.
		private SepiaSettings _sepiaSettings;					
	    // Constant buffer for scratch settings.
		private GorgonConstantBufferView _scratchBuffer;			
	    // Constant buffer for sepia settings.
		private GorgonConstantBufferView _sepiaBuffer;				
	    // Noise frequency.
		private float _noiseFrequency = 42.0f;
        // The shader used for the film effect.
	    private Gorgon2DShader<GorgonPixelShader> _filmShader;
        // The batch state to use when rendering.
	    private Gorgon2DBatchState _batchState;
        // The current time, in seconds.
	    private float _time;
        // The texture to draw with the effect.
	    private GorgonTexture2DView _drawTexture;
        // The region to draw into.
	    private DX.RectangleF? _drawRegion;
        // The texture coordinates of the texture to draw.
	    private DX.RectangleF _drawTextureCoordinates;
	    #endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the noise frequency used in generating scratches in the film.
		/// </summary>
		/// <remarks>It is not recommended to update this value every frame, doing so may have a significant performance hit.</remarks>
		public float NoiseFrequency
        {
            get => _noiseFrequency;
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
			get => _scratchSettings.ScratchVisibleTime;
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
			get => _scratchSettings.ScratchScrollSpeed;
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
			get => _scratchSettings.ScratchIntensity;
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
			get => _scratchSettings.ScratchWidth;
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
			get => _sepiaSettings.SepiaDesaturationAmount;
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
			get => _sepiaSettings.SepiaToneAmount;
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
			get => _sepiaSettings.SepiaLightColor;
		    set
			{
				if (GorgonColor.Equals(in _sepiaSettings.SepiaLightColor, in value))
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
			get => _sepiaSettings.SepiaDarkColor;
		    set
			{
				if (GorgonColor.Equals(in _sepiaSettings.SepiaDarkColor, in value))
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
	    } = 5;

	    /// <summary>
	    /// Property to set or return the amount of dirt and dust that will appear.
	    /// </summary>
	    public int DirtAmount
	    {
	        get;
	        set;
	    } = 10;

	    /// <summary>
	    /// Property to set or return the current time for the effect in seconds.
	    /// </summary>
	    public float Time
	    {
	        get => _time;
	        set => _time = value;
	    }

	    /// <summary>
	    /// Property to set or return the noise texture width and height.
	    /// </summary>
	    public int NoiseTextureSize
	    {
	        get;
	        set;
	    } = 128;
		#endregion

		#region Methods.
		/// <summary>
		/// Funciton to generate random noise for the effect.
		/// </summary>
		private void GenerateRandomNoise()
		{
			int textureSize = NoiseTextureSize.Min(128).Max(16);

		    using (var image = new GorgonImage(new GorgonImageInfo(ImageType.Image2D, BufferFormat.R8_UNorm)
		                                       {
                                                   Width = textureSize,
                                                   Height = textureSize
		                                       }))
		    {
		        IGorgonImageBuffer imageBuffer = image.Buffers[0];

		        for (int y = 0; y < textureSize; ++y)
		        {
		            for (int x = 0; x < textureSize; ++x)
		            {
		                float simplexNoise = GorgonRandom.SimplexNoise(x * (1.0f / _noiseFrequency), y * (1.0f / _noiseFrequency));

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


		                image.Buffers[0].Data[y * imageBuffer.PitchInformation.RowPitch + x] = (byte)(simplexNoise * 255.0f);
		            }
		        }

                _randomTexture = GorgonTexture2DView.CreateTexture(Graphics, new GorgonTexture2DInfo("Gorgon2D Old Film Effect Random Noise Texture")
                                                                             {
                                                                                 Width = textureSize,
                                                                                 Height = textureSize,
                                                                                 Usage = ResourceUsage.Immutable,
                                                                                 Binding = TextureBinding.ShaderResource,
                                                                                 Format = BufferFormat.R8_UNorm
                                                                             }, image);
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
		    GenerateRandomNoise();

			_timingBuffer = GorgonConstantBufferView.CreateConstantBuffer(Graphics, new GorgonConstantBufferInfo("Gorgon 2D Old Film Effect - Timing data")
			                                                                        {
                                                                                        Usage = ResourceUsage.Dynamic,
                                                                                        SizeInBytes = 16
			                                                                        });

            _scratchBuffer = GorgonConstantBufferView.CreateConstantBuffer(Graphics, ref _scratchSettings, "Gorgon 2D Old Film Effect - Scratch settings");
		    _sepiaBuffer = GorgonConstantBufferView.CreateConstantBuffer(Graphics, ref _sepiaSettings, "Gorgon 2D Old Film Effect - Sepia settings");

		    // Create pixel shader.
		    _filmShader = PixelShaderBuilder
		                  .ConstantBuffer(_timingBuffer, 1)
		                  .ConstantBuffer(_scratchBuffer, 2)
		                  .ConstantBuffer(_sepiaBuffer, 3)
		                  .ShaderResource(_randomTexture, 1)
		                  .SamplerState(GorgonSamplerState.Wrapping, 1)
		                  .Shader(CompileShader<GorgonPixelShader>(Resources.FilmGrain, "GorgonPixelShaderFilmGrain"))
		                  .Build();

            // Build our state.
		    _batchState = BatchStateBuilder
		                  .BlendState(GorgonBlendState.NoBlending)
		                  .PixelShader(_filmShader)
		                  .Build();
		}

		/// <summary>
		/// Function called before rendering begins.
		/// </summary>
		/// <returns>
		/// <b>true</b> to continue rendering, <b>false</b> to exit.
		/// </returns>
		protected override bool OnBeforeRender()
		{
		    GorgonRenderTargetView currentTarget = Graphics.RenderTargets[0];

		    if (currentTarget == null)
		    {
		        return false;
		    }

		    if (_isScratchUpdated)
		    {
		        _scratchBuffer.Buffer.SetData(ref _scratchSettings);
		        _isScratchUpdated = false;
		    }

		    if (_isSepiaUpdated)
		    {
		        _sepiaBuffer.Buffer.SetData(ref _sepiaSettings);
		        _isSepiaUpdated = false;
		    }

		    _timingBuffer.Buffer.SetData(ref _time);

		    return true;
		}

	    /// <summary>
	    /// Function called to build a new (or return an existing) 2D batch state.
	    /// </summary>
	    /// <param name="passIndex">The index of the current rendering pass.</param>
	    /// <param name="statesChanged"><b>true</b> if the blend, raster, or depth/stencil state was changed. <b>false</b> if not.</param>
	    /// <returns>The 2D batch state.</returns>
	    protected override Gorgon2DBatchState OnGetBatchState(int passIndex, bool statesChanged)
	    {
	        return _batchState;
	    }

	    /// <summary>
	    /// Function called to render a single effect pass.
	    /// </summary>
	    /// <param name="passIndex">The index of the pass being rendered.</param>
	    /// <param name="batchState">The current batch state for the pass.</param>
	    /// <param name="camera">The current camera to use when rendering.</param>
	    /// <remarks>
	    /// <para>
	    /// Applications must implement this in order to see any results from the effect.
	    /// </para>
	    /// </remarks>
	    protected override void OnRenderPass(int passIndex, Gorgon2DBatchState batchState, Gorgon2DCamera camera)
	    {
	        Debug.Assert(_drawRegion != null, "No drawing region found.");

	        Renderer.Begin(_batchState, camera);
            Renderer.DrawFilledRectangle(_drawRegion.Value, GorgonColor.White, _drawTexture, _drawTextureCoordinates);
            Renderer.End();
	    }

        /// <summary>
        /// Function to draw a texture using the old film effect.
        /// </summary>
        /// <param name="texture">The texture to draw.</param>
        /// <param name="region">[Optional] The destination region to draw the texture into.</param>
        /// <param name="textureCoordinates">[Optional] The texture coordinates, in texels, to use when drawing the texture.</param>
        /// <param name="camera">[Optional] The camera used to render the image.</param>
        /// <remarks>
        /// <para>
        /// If the <paramref name="region"/> parameter is omitted, then the entire size of the current render target is used.
        /// </para>
        /// <para>
        /// If the <paramref name="textureCoordinates"/> parameter is omitted, then the entire size of the texture is used.
        /// </para>
        /// </remarks>
	    public void RenderEffect(GorgonTexture2DView texture, DX.RectangleF? region = null, DX.RectangleF? textureCoordinates = null, Gorgon2DCamera camera = null)
        {
            texture.ValidateObject(nameof(texture));

            _drawTexture = texture;
            _drawRegion = region ?? new DX.RectangleF(0, 0, CurrentTargetSize.Width, CurrentTargetSize.Height);
            _drawTextureCoordinates = textureCoordinates ?? new DX.RectangleF(0, 0, 1, 1);

            Render(camera: camera);

            _drawTexture = null;
            _drawRegion = null;
        }

	    /// <summary>
	    /// Function called after a pass is finished rendering.
	    /// </summary>
	    /// <param name="passIndex">The index of the pass that was rendered.</param>
	    /// <remarks>
	    /// <para>
	    /// Applications can use this to clean up and/or restore any states after the pass completes.
	    /// </para>
	    /// </remarks>
	    protected override void OnAfterRenderPass(int passIndex)
		{
            Debug.Assert(_drawRegion != null, "No drawing region found.");

            Renderer.Begin();

			for (int i = 0; i < DirtAmount; ++i)
			{
				float grayDust = GorgonRandom.RandomSingle(0.1f, 0.25f);
				var dustColor = new GorgonColor(grayDust, grayDust, grayDust, GorgonRandom.RandomSingle(0.25f, 0.95f));

				// Render dust points.
			    Renderer.DrawFilledRectangle(new DX.RectangleF(GorgonRandom.RandomSingle(_drawRegion.Value.Left, _drawRegion.Value.Right),
			                                                   GorgonRandom.RandomSingle(_drawRegion.Value.Top, _drawRegion.Value.Bottom),
			                                                   1,
			                                                   1),
			                                 dustColor);

				if (GorgonRandom.RandomInt32(100) >= DirtPercent)
				{
					continue;
				}

				// Render dirt/hair lines.
			    var dirtStart = new DX.Vector2(GorgonRandom.RandomSingle(_drawRegion.Value.Left, _drawRegion.Value.Right),
			                                   GorgonRandom.RandomSingle(_drawRegion.Value.Left, _drawRegion.Value.Right));

				float dirtWidth = GorgonRandom.RandomSingle(1.0f, 3.0f);
				bool isHair = GorgonRandom.RandomInt32(100) > 50;
				bool isHairVertical = isHair && GorgonRandom.RandomInt32(100) > 50;

				grayDust = GorgonRandom.RandomSingle(0.1f, 0.15f);
				dustColor = new GorgonColor(grayDust, grayDust, grayDust, GorgonRandom.RandomSingle(0.25f, 0.95f));
				
				for (int j = 0; j < GorgonRandom.RandomInt32(4, CurrentTargetSize.Width / 4); j++)
				{
                    DX.Size2F size = isHair ? new DX.Size2F(1, 1) : new DX.Size2F(dirtWidth, dirtWidth);
				    Renderer.DrawFilledRectangle(new DX.RectangleF(dirtStart.X, dirtStart.Y, size.Width, size.Height), dustColor);

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
            Renderer.End();
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><b>true</b> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
		    if (!disposing)
		    {
		        return;
		    }

		    Gorgon2DShader<GorgonPixelShader> shader = Interlocked.Exchange(ref _filmShader, null);
		    GorgonTexture2DView texture = Interlocked.Exchange(ref _randomTexture, null);
		    GorgonConstantBufferView buffer1 = Interlocked.Exchange(ref _timingBuffer, null);
		    GorgonConstantBufferView buffer2 = Interlocked.Exchange(ref _scratchBuffer, null);
		    GorgonConstantBufferView buffer3 = Interlocked.Exchange(ref _sepiaBuffer, null);

            shader?.Dispose();
            texture?.Dispose();
            buffer1?.Dispose();
		    buffer2?.Dispose();
		    buffer3?.Dispose();
		}
		#endregion

		#region Constructor/Destructor.
	    /// <summary>
	    /// Initializes a new instance of the <see cref="Gorgon2DOldFilmEffect"/> class.
	    /// </summary>
	    /// <param name="renderer">The renderer used to draw with this effect.</param>
	    public Gorgon2DOldFilmEffect(Gorgon2D renderer)
	        : base(renderer, Resources.GOR2D_EFFECT_FILM, Resources.GOR2D_EFFECT_FILM_DESC, 1)
	    {
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
	    #endregion
	}
}
