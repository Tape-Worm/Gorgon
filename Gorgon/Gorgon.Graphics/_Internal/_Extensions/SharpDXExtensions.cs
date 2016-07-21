﻿#region MIT
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
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
// Created: Monday, December 14, 2015 8:41:48 PM
// 
#endregion

using Gorgon.Math;
using Gorgon.Native;
using SharpDX.Mathematics.Interop;
using DXGI = SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Extension methods for SharpDX object conversion.
	/// </summary>
	static class SharpDXExtensions
	{
		/// <summary>
		/// Function to convert a <see cref="GorgonBox"/> to a D3D11 resource region.
		/// </summary>
		/// <param name="box">The box value to convert.</param>
		/// <returns>The D3D11 resource region.</returns>
		public static D3D11.ResourceRegion ToResourceRegion(this GorgonBox box)
		{
			return new D3D11.ResourceRegion(box.Left, box.Top, box.Front, box.Right, box.Bottom, box.Back);
		}

		/// <summary>
		/// Function to retrieve the underlying Direct 3D immediate context from a <see cref="IGorgonVideoDevice"/> interface.
		/// </summary>
		/// <param name="videoDevice">The video device to use.</param>
		/// <returns>The Direct 3D immediate context.</returns>
		public static D3D11.DeviceContext1 D3DDeviceContext(this IGorgonVideoDevice videoDevice)
		{
			return ((VideoDevice)videoDevice).D3DDeviceContext;
		}

		/// <summary>
		/// Function to retrieve the underlying Direct 3D device from a <see cref="IGorgonVideoDevice"/> interface.
		/// </summary>
		/// <param name="videoDevice">The video device to use.</param>
		/// <returns>The Direct 3D video device.</returns>
		public static D3D11.Device1 D3DDevice(this IGorgonVideoDevice videoDevice)
		{
			return ((VideoDevice)videoDevice).D3DDevice;
		}

		/// <summary>
		/// Function to retrieve the underlying DXGI adapter from a <see cref="IGorgonVideoDevice"/> interface.
		/// </summary>
		/// <param name="videoDevice">The video device to use.</param>
		/// <returns>The DXGI adapter.</returns>
		public static DXGI.Adapter2 DXGIAdapter(this IGorgonVideoDevice videoDevice)
		{
			return ((VideoDevice)videoDevice).Adapter;
		}

		/// <summary>
		/// Function to convert a sharp dx raw color 4 type to a GorgonColor.
		/// </summary>
		/// <param name="color">The color type to convert.</param>
		/// <returns>The new color type.</returns>
		public static GorgonColor ToGorgonColor(this RawColor4 color)
		{
			return new GorgonColor(color.R, color.G, color.B, color.A);	
		}

		/// <summary>
		/// Function to convert a GorgonColor to a sharp dx raw color 4 type.
		/// </summary>
		/// <param name="color">The color type to convert.</param>
		/// <returns>The new color type.</returns>
		public static RawColor4 ToRawColor4(this GorgonColor color)
		{
			return new RawColor4(color.Red, color.Green, color.Blue, color.Alpha);
		}


		/// <summary>
		/// Function to convert a DXGI rational number to a Gorgon rational number.
		/// </summary>
		/// <param name="rational">The rational number to convert.</param>
		/// <returns>A Gorgon rational number.</returns>
		public static GorgonRationalNumber FromRational(this DXGI.Rational rational)
		{
			return new GorgonRationalNumber(rational.Numerator, rational.Denominator);
		}

		/// <summary>
		/// Function to convert a Gorgon rational number to a DXGI rational number.
		/// </summary>
		/// <param name="rational">The rational number to convert.</param>
		/// <returns>The DXGI ration number.</returns>
		public static DXGI.Rational ToRational(this GorgonRationalNumber rational)
		{
			return new DXGI.Rational(rational.Numerator, rational.Denominator);
		}

		/// <summary>
		/// Function to convert a ModeDescription1 to a ModeDescription.
		/// </summary>
		/// <param name="mode">ModeDescription1 to convert.</param>
		/// <returns>The new mode description.</returns>
		public static DXGI.ModeDescription ToModeDesc(this DXGI.ModeDescription1 mode)
		{
			return new DXGI.ModeDescription
			       {
				       Format = mode.Format,
				       Height = mode.Height,
				       Scaling = mode.Scaling,
				       Width = mode.Width,
				       ScanlineOrdering = mode.ScanlineOrdering,
				       RefreshRate = mode.RefreshRate
			       };
		}

		/// <summary>
		/// Function to convert a ModeDescription to a ModeDescription1.
		/// </summary>
		/// <param name="mode">ModeDescription to convert.</param>
		/// <returns>The new mode description.</returns>
		public static DXGI.ModeDescription1 ToModeDesc1(this DXGI.ModeDescription mode)
		{
			return new DXGI.ModeDescription1
			       {
				       Format = mode.Format,
				       Height = mode.Height,
				       Scaling = mode.Scaling,
				       Width = mode.Width,
				       ScanlineOrdering = mode.ScanlineOrdering,
				       RefreshRate = mode.RefreshRate,
				       Stereo = false
			       };
		}

		/// <summary>
		/// Function to convert a <see cref="GorgonMultiSampleInfo"/> to a DXGI multi sample description.
		/// </summary>
		/// <param name="samplingInfo">The Gorgon multi sample info to convert.</param>
		/// <returns>The DXGI multi sample description.</returns>
		public static DXGI.SampleDescription ToSampleDesc(this GorgonMultiSampleInfo samplingInfo)
		{
			return new DXGI.SampleDescription(samplingInfo.Count, samplingInfo.Quality);
		}

		/// <summary>
		/// Function to convert a <see cref="GorgonSwapChainInfo"/> to a DXGI swap chain description value.
		/// </summary>
		/// <param name="swapChainInfo">The swap chain info to convert.</param>
		/// <returns>A DXGI swap chain description.</returns>
		public static DXGI.SwapChainDescription1 ToSwapChainDesc(this GorgonSwapChainInfo swapChainInfo)
		{
			var swapEffect = DXGI.SwapEffect.Discard;

			// Flip sequential is not supported on Window 7, so ignore it if we're on windows 7.
			if ((swapChainInfo.UseFlipMode) && (Win32API.IsWindows8OrGreater()))
			{
				swapEffect = DXGI.SwapEffect.FlipSequential;
			}

			return new DXGI.SwapChainDescription1
			       {
				       BufferCount = 2,
				       AlphaMode = DXGI.AlphaMode.Unspecified,
				       Flags = DXGI.SwapChainFlags.AllowModeSwitch,
				       Format = swapChainInfo.Format,
				       Width = swapChainInfo.Width,
				       Height = swapChainInfo.Height,
				       Scaling = swapChainInfo.StretchBackBuffer ? DXGI.Scaling.Stretch : DXGI.Scaling.None,
					   SampleDescription = ToSampleDesc(GorgonMultiSampleInfo.NoMultiSampling),
					   SwapEffect = swapEffect,
					   Usage = DXGI.Usage.RenderTargetOutput
			       };
		}
	}
}
