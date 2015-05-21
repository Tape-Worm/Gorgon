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
// Created: Monday, April 02, 2012 2:05:45 PM
// 
#endregion

namespace Gorgon.Renderers
{
	/// <summary>
	/// Pre defined effects for Gorgon 2D.
	/// </summary>
	public class Gorgon2DEffects
	{
		#region Variables.
		private static readonly object _syncLock = new object();		// Synchronization object for multiple threads.
		private readonly Gorgon2D _gorgon2D;							// Our 2D interface.
		private Gorgon2D1BitEffect _1BitEffect;							// A 1 bit effect.
		private Gorgon2DBurnDodgeEffect _burnDodgeEffect;				// Burn/dodge effect.
		private Gorgon2DDisplacementEffect _displacementEffect;			// Displacement effect.
		private Gorgon2DGaussianBlurEffect _gaussBlurEffect;			// Gaussian blur effect.
		private Gorgon2DGrayScaleEffect _grayScaleEffect;				// Gray scale effect.
		private Gorgon2DInvertEffect _invertEffect;						// Invert effect.
		private Gorgon2DPosterizedEffect _posterizeEffect;				// Posterize effect.
		private Gorgon2DSharpenEmbossEffect _sharpenEmbossEffect;		// Sharpen/emboss effect.
		private Gorgon2DSobelEdgeDetectEffect _sobelEdgeDetectEffect;	// Sobel edge detection effect.
		private Gorgon2DWaveEffect _waveEffect;							// Wave effect.
		private Gorgon2DOldFilmEffect _oldFilmEffect;					// Old film effect.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the gray scale effect.
		/// </summary>
		public Gorgon2DGrayScaleEffect GrayScale
		{
			get
			{
				lock (_syncLock)
				{
					if (_grayScaleEffect == null)
					{
					    _grayScaleEffect = _gorgon2D.Create2DEffect<Gorgon2DGrayScaleEffect>("Effect.2D.GrayScale");
					}
				}

				return _grayScaleEffect;
			}
		}

		/// <summary>
		/// Property to return the wave effect.
		/// </summary>
		public Gorgon2DWaveEffect Wave
		{
			get
			{
				lock (_syncLock)
				{
					if (_waveEffect == null)
					{
					    _waveEffect = _gorgon2D.Create2DEffect<Gorgon2DWaveEffect>("Effect.2D.Wave");
					}
				}
				return _waveEffect;
			}			
		}

		/// <summary>
		/// Property to return the 1 bit effect.
		/// </summary>
		public Gorgon2D1BitEffect OneBit
		{
			get
			{
				lock (_syncLock)
				{
					if (_1BitEffect == null)
					{
                        _1BitEffect = _gorgon2D.Create2DEffect<Gorgon2D1BitEffect>("Effect.2D.1Bit");
					}					
				}

				return _1BitEffect;
			}			
		}

		/// <summary>
		/// Property to return the sharpen/emboss effect.
		/// </summary>
		public Gorgon2DSharpenEmbossEffect SharpenEmboss
		{
			get
			{
				lock (_syncLock)
				{
					if (_sharpenEmbossEffect == null)
					{
					    _sharpenEmbossEffect = _gorgon2D.Create2DEffect<Gorgon2DSharpenEmbossEffect>("Effect.2D.SharpenEmboss");
					}
				}
				return _sharpenEmbossEffect;
			}			
		}

		/// <summary>
		/// Property to return the invert effect.
		/// </summary>
		public Gorgon2DInvertEffect Invert
		{
			get
			{
				lock (_syncLock)
				{
					if (_invertEffect == null)
					{
						_invertEffect = _gorgon2D.Create2DEffect<Gorgon2DInvertEffect>("Effect.2D.Invert");
					}
				}
				return _invertEffect;
			}
		}

		/// <summary>
		/// Property to return the Gaussian blur effect.
		/// </summary>
		public Gorgon2DGaussianBlurEffect GaussianBlur
		{
			get
			{
				lock (_syncLock)
				{
					if (_gaussBlurEffect == null)
					{
					    _gaussBlurEffect = _gorgon2D.Create2DEffect<Gorgon2DGaussianBlurEffect>("Effect.GaussBlur");
					}
				}
				return _gaussBlurEffect;
			}			
		}

		/// <summary>
		/// Property to return the posterize effect.
		/// </summary>
		public Gorgon2DPosterizedEffect Posterize
		{
			get
			{
				lock (_syncLock)
				{
					if (_posterizeEffect == null)
					{
					    _posterizeEffect = _gorgon2D.Create2DEffect<Gorgon2DPosterizedEffect>("Effect.2D.Posterize");
					}
				}
				return _posterizeEffect;
			}			
		}

		/// <summary>
		/// Property to return the Sobel edge detection effect.
		/// </summary>
		public Gorgon2DSobelEdgeDetectEffect SobelEdgeDetection
		{
			get
			{
				lock (_syncLock)
				{
					if (_sobelEdgeDetectEffect == null)
					{
						_sobelEdgeDetectEffect = _gorgon2D.Create2DEffect<Gorgon2DSobelEdgeDetectEffect>("Effect.2D.SobelEdgeDetection");
					}
				}

				return _sobelEdgeDetectEffect;
			}
		}

		/// <summary>
		/// Property to return the burn/dodge effect.
		/// </summary>
		public Gorgon2DBurnDodgeEffect BurnDodge
		{
			get
			{
				lock (_syncLock)
				{
					if (_burnDodgeEffect == null)
					{
					    _burnDodgeEffect = _gorgon2D.Create2DEffect<Gorgon2DBurnDodgeEffect>("Effect.2D.BurnDodge");
					}					
				}

				return _burnDodgeEffect;
			}
		}

		/// <summary>
		/// Property to return the displacement effect.
		/// </summary>
		public Gorgon2DDisplacementEffect Displacement
		{
			get
			{
				lock (_syncLock)
				{
					if (_displacementEffect == null)
					{
					    _displacementEffect = _gorgon2D.Create2DEffect<Gorgon2DDisplacementEffect>("Effect.2D.Displacement");
					}
				}

				return _displacementEffect;
			}			
		}

		/// <summary>
		/// Property to return the old film effect.
		/// </summary>
		public Gorgon2DOldFilmEffect OldFilm
		{
			get
			{
				lock(_syncLock)
				{
					if (_oldFilmEffect == null)
					{
						_oldFilmEffect = _gorgon2D.Create2DEffect<Gorgon2DOldFilmEffect>("Effect.2D.OldFilm");
					}
				}

				return _oldFilmEffect;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to release the shaders and the resources allocated to them.
		/// </summary>
		public void FreeEffects()
		{
			if (_posterizeEffect != null)
			{
				_posterizeEffect.Dispose();
				_posterizeEffect = null;
			}

			if (_grayScaleEffect != null)
			{
				_grayScaleEffect.Dispose();
				_grayScaleEffect = null;
			}

			if (_waveEffect != null)
			{				
				_waveEffect.Dispose();
				_waveEffect = null;
			}

			if (_1BitEffect != null)
			{
				_1BitEffect.Dispose();
				_1BitEffect = null;
			}

			if (_sharpenEmbossEffect != null)
			{
				_sharpenEmbossEffect.Dispose();
				_sharpenEmbossEffect = null;
			}

			if (_invertEffect != null)
			{
				_invertEffect.Dispose();
				_invertEffect = null;
			}

			if (_gaussBlurEffect != null)
			{				
				_gaussBlurEffect.Dispose();
				_gaussBlurEffect = null;
			}

			if (_sobelEdgeDetectEffect != null)
			{
				_sobelEdgeDetectEffect.Dispose();
				_sobelEdgeDetectEffect = null;
			}

			if (_burnDodgeEffect != null)
			{
				_burnDodgeEffect.Dispose();
				_burnDodgeEffect = null;
			}

			if (_displacementEffect == null)
			{
				return;
			}

			_displacementEffect.Dispose();
			_displacementEffect = null;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Gorgon2DEffects"/> class.
		/// </summary>
		/// <param name="gorgon2D">The gorgon 2D interface that owns this object.</param>
		internal Gorgon2DEffects(Gorgon2D gorgon2D)
		{
			_gorgon2D = gorgon2D;
		}
		#endregion
	}
}
