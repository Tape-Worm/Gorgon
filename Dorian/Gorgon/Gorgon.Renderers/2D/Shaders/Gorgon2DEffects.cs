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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary.Renderers
{
	/// <summary>
	/// Pre defined effects for Gorgon 2D.
	/// </summary>
	public class Gorgon2DEffects
	{
		#region Variables.
		private static readonly object _syncLock = new object();				// Synchronization object for multiple threads.
		private Gorgon2D _gorgon2D = null;										// Our 2D interface.
		private Gorgon2D1BitEffect _1bitEffect = null;							// A 1 bit effect.
		private Gorgon2DBurnDodgeEffect _burnDodgeEffect = null;				// Burn/dodge effect.
		private Gorgon2DDisplacementEffect _displacementEffect = null;			// Displacement effect.
		private Gorgon2DGaussianBlurEffect _gaussBlurEffect = null;				// Gaussian blur effect.
		private Gorgon2DGrayScaleEffect _grayScaleEffect = null;				// Gray scale effect.
		private Gorgon2DInvertEffect _invertEffect = null;						// Invert effect.
		private Gorgon2DPosterizedEffect _posterizeEffect = null;				// Posterize effect.
		private Gorgon2DSharpenEmbossEffect _sharpenEmbossEffect = null;		// Sharpen/emboss effect.
		private Gorgon2DSobelEdgeDetectEffect _sobelEdgeDetectEffect = null;	// Sobel edge detection effect.
		private Gorgon2DWaveEffect _waveEffect = null;							// Wave effect.
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
						_grayScaleEffect = new Gorgon2DGrayScaleEffect(_gorgon2D);
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
						_waveEffect = new Gorgon2DWaveEffect(_gorgon2D);
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
					if (_1bitEffect == null)
					{
						_1bitEffect = new Gorgon2D1BitEffect(_gorgon2D);
					}					
				}
				return _1bitEffect;
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
						_sharpenEmbossEffect = new Gorgon2DSharpenEmbossEffect(_gorgon2D);
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
						_invertEffect = new Gorgon2DInvertEffect(_gorgon2D);
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
						_gaussBlurEffect = new Gorgon2DGaussianBlurEffect(_gorgon2D);
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
						_posterizeEffect = new Gorgon2DPosterizedEffect(_gorgon2D);
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
						_sobelEdgeDetectEffect = new Gorgon2DSobelEdgeDetectEffect(_gorgon2D);
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
						_burnDodgeEffect = new Gorgon2DBurnDodgeEffect(_gorgon2D);
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
						_displacementEffect = new Gorgon2DDisplacementEffect(_gorgon2D);
					}
				}

				return _displacementEffect;
			}			
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to release the shaders and the resources allocated to them.
		/// </summary>
		public void FreeShaders()
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

			if (_1bitEffect != null)
			{
				_1bitEffect.Dispose();
				_1bitEffect = null;
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

			if (_displacementEffect != null)
			{
				_displacementEffect.Dispose();
				_displacementEffect = null;
			}
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
