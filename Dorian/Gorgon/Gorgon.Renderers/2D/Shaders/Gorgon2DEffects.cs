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
		private Gorgon2D _gorgon2D = null;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the gray scale effect.
		/// </summary>
		public Gorgon2DGrayScaleEffect GrayScale
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the wave effect.
		/// </summary>
		public Gorgon2DWaveEffect Wave
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the 1 bit effect.
		/// </summary>
		public Gorgon2D1BitEffect OneBit
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the sharpen/emboss effect.
		/// </summary>
		public Gorgon2DSharpenEmbossEffect SharpenEmboss
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the invert effect.
		/// </summary>
		public Gorgon2DInvertEffect Invert
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the Gaussian blur effect.
		/// </summary>
		public Gorgon2DGaussianBlurEffect GaussianBlur
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the posterize effect.
		/// </summary>
		public Gorgon2DPosterizedEffect Posterize
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the Sobel edge detection effect.
		/// </summary>
		public Gorgon2DSobelEdgeDetectEffect SobelEdgeDetection
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the burn/dodge effect.
		/// </summary>
		public Gorgon2DBurnDodgeEffect BurnDodge
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to clean up internal objects.
		/// </summary>
		internal void CleanUp()
		{
			if (Posterize != null)
				Posterize.Dispose();

			if (GrayScale != null)
				GrayScale.Dispose();

			if (Wave != null)
				Wave.Dispose();

			if (OneBit != null)
				OneBit.Dispose();

			if (SharpenEmboss != null)
				SharpenEmboss.Dispose();

			if (Invert != null)
				Invert.Dispose();

			if (GaussianBlur != null)
				GaussianBlur.Dispose();

			if (SobelEdgeDetection != null)
				SobelEdgeDetection.Dispose();

			if (BurnDodge != null)
				BurnDodge.Dispose();

			BurnDodge = null;
			SobelEdgeDetection = null;
			Invert = null;
			SharpenEmboss = null;
			OneBit = null;
			Wave = null;
			GrayScale = null;
			Posterize = null;
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
			GrayScale = new Gorgon2DGrayScaleEffect(_gorgon2D);
			Wave = new Gorgon2DWaveEffect(_gorgon2D);
			OneBit = new Gorgon2D1BitEffect(_gorgon2D);
			SharpenEmboss = new Gorgon2DSharpenEmbossEffect(_gorgon2D);
			Invert = new Gorgon2DInvertEffect(_gorgon2D);
			GaussianBlur = new Gorgon2DGaussianBlurEffect(_gorgon2D);
			Posterize = new Gorgon2DPosterizedEffect(_gorgon2D);
			SobelEdgeDetection = new Gorgon2DSobelEdgeDetectEffect(_gorgon2D);
			BurnDodge = new Gorgon2DBurnDodgeEffect(_gorgon2D);
		}
		#endregion
	}
}
