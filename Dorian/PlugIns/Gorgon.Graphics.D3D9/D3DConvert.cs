#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Friday, July 22, 2011 3:32:25 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D9;

namespace GorgonLibrary.Graphics.D3D9
{
	/// <summary>
	/// Utility to convert values between Gorgon and Direct 3D.
	/// </summary>
	static class D3DConvert
	{
		#region Methods.
		/// <summary>
		/// Function to convert a D3D swap effect to a Gorgon display function.
		/// </summary>
		/// <param name="displayFunction">D3D swap effect to convert.</param>
		/// <returns>The Gorgon display function.</returns>
		public static GorgonDisplayFunction Convert(SwapEffect displayFunction)
		{
			switch (displayFunction)
			{
				case SwapEffect.Copy:
					return GorgonDisplayFunction.Copy;
				case SwapEffect.Flip:
					return GorgonDisplayFunction.Flip;
				default:
					return GorgonDisplayFunction.Discard;
			}
		}

		/// <summary>
		/// Function to convert a Gorgon display function to a D3D swap effect.
		/// </summary>
		/// <param name="displayFunction">Gorgon display function to convert.</param>
		/// <returns>The D3D swap effect.</returns>
		public static SwapEffect Convert(GorgonDisplayFunction displayFunction)
		{
			switch (displayFunction)
			{
				case GorgonDisplayFunction.Copy:
					return SwapEffect.Copy;
				case GorgonDisplayFunction.Flip:
					return SwapEffect.Flip;
				default:
					return SwapEffect.Discard;
			}
		}

		/// <summary>
		/// Function to convert a D3D presentation interval to a Gorgon vsync interval.
		/// </summary>
		/// <param name="interval">D3D interval to convert.</param>
		/// <returns>The Gorgon vsync interval.</returns>
		public static GorgonVSyncInterval Convert(PresentInterval interval)
		{
			switch (interval)
			{
				case PresentInterval.One:
					return GorgonVSyncInterval.One;
				case PresentInterval.Two:
					return GorgonVSyncInterval.Two;
				case PresentInterval.Three:
					return GorgonVSyncInterval.Three;
				case PresentInterval.Four:
					return GorgonVSyncInterval.Four;
				default:
					return GorgonVSyncInterval.None;
			}			
		}

		/// <summary>
		/// Function to convert a Gorgon vsync interval to a D3D present interval.
		/// </summary>
		/// <param name="interval">Gorgon vsync interval to convert.</param>
		/// <returns>The D3D present interval.</returns>
		public static PresentInterval Convert(GorgonVSyncInterval interval)
		{
			switch (interval)
			{
				case GorgonVSyncInterval.One:
					return PresentInterval.One;
				case GorgonVSyncInterval.Two:
					return PresentInterval.Two;
				case GorgonVSyncInterval.Three:
					return PresentInterval.Three;
				case GorgonVSyncInterval.Four:
					return PresentInterval.Four;
				default:
					return PresentInterval.Immediate;
			}
		}

		/// <summary>
		/// Function to convert a D3D multi sample type to a Gorgon multi sample level.
		/// </summary>
		/// <param name="sampleLevel">D3D multi sample type to convert.</param>
		/// <returns>Gorgon multi sample level.</returns>
		public static GorgonMSAALevel Convert(MultisampleType sampleLevel)
		{
			switch (sampleLevel)
			{
				case MultisampleType.NonMaskable:
					return GorgonMSAALevel.NonMasked;
				case MultisampleType.TwoSamples:
					return GorgonMSAALevel.Level2;
				case MultisampleType.ThreeSamples:
					return GorgonMSAALevel.Level3;
				case MultisampleType.FourSamples:
					return GorgonMSAALevel.Level4;
				case MultisampleType.FiveSamples:
					return GorgonMSAALevel.Level5;
				case MultisampleType.SixSamples:
					return GorgonMSAALevel.Level6;
				case MultisampleType.SevenSamples:
					return GorgonMSAALevel.Level7;
				case MultisampleType.EightSamples:
					return GorgonMSAALevel.Level8;
				case MultisampleType.NineSamples:
					return GorgonMSAALevel.Level9;
				case MultisampleType.TenSamples:
					return GorgonMSAALevel.Level10;
				case MultisampleType.ElevenSamples:
					return GorgonMSAALevel.Level11;
				case MultisampleType.TwelveSamples:
					return GorgonMSAALevel.Level12;
				case MultisampleType.ThirteenSamples:
					return GorgonMSAALevel.Level13;
				case MultisampleType.FourteenSamples:
					return GorgonMSAALevel.Level14;
				case MultisampleType.FifteenSamples:
					return GorgonMSAALevel.Level15;
				case MultisampleType.SixteenSamples:
					return GorgonMSAALevel.Level16;
				default:
					return GorgonMSAALevel.None;
			}
		}

		/// <summary>
		/// Function to convert a Gorgon multi sample level to a D3D multi-sample type.
		/// </summary>
		/// <param name="sampleLevel">Gorgon multi sample level to convert.</param>
		/// <returns>D3D multi sample type.</returns>
		public static MultisampleType Convert(GorgonMSAALevel sampleLevel)
		{
			switch (sampleLevel)
			{
				case GorgonMSAALevel.NonMasked:
					return MultisampleType.NonMaskable;
				case GorgonMSAALevel.Level2:
					return MultisampleType.TwoSamples;
				case GorgonMSAALevel.Level3:
					return MultisampleType.ThreeSamples;
				case GorgonMSAALevel.Level4:
					return MultisampleType.FourSamples;
				case GorgonMSAALevel.Level5:
					return MultisampleType.FiveSamples;
				case GorgonMSAALevel.Level6:
					return MultisampleType.SixSamples;
				case GorgonMSAALevel.Level7:
					return MultisampleType.SevenSamples;
				case GorgonMSAALevel.Level8:
					return MultisampleType.EightSamples;
				case GorgonMSAALevel.Level9:
					return MultisampleType.NineSamples;
				case GorgonMSAALevel.Level10:
					return MultisampleType.TenSamples;
				case GorgonMSAALevel.Level11:
					return MultisampleType.ElevenSamples;
				case GorgonMSAALevel.Level12:
					return MultisampleType.TwelveSamples;
				case GorgonMSAALevel.Level13:
					return MultisampleType.ThirteenSamples;
				case GorgonMSAALevel.Level14:
					return MultisampleType.FourteenSamples;
				case GorgonMSAALevel.Level15:
					return MultisampleType.FifteenSamples;
				case GorgonMSAALevel.Level16:
					return MultisampleType.SixteenSamples;
				default:
					return MultisampleType.None;
			}
		}

		/// <summary>
		/// Function to convert a Gorgon video mode into a D3D display mode.
		/// </summary>
		/// <param name="mode">D3D Mode to convert.</param>
		/// <returns>The Gorgon video mode.</returns>
		public static GorgonVideoMode Convert(DisplayMode mode)
		{
			return new GorgonVideoMode(mode.Width, mode.Height, Convert(mode.Format), mode.RefreshRate, 1);
		}

		/// <summary>
		/// Function to convert a D3D buffer format into a Gorgon format.
		/// </summary>
		/// <param name="format">D3D buffer format to convert.</param>
		/// <returns>A Gorgon buffer format.</returns>
		public static GorgonBufferFormat Convert(Format format)
		{
			switch (format)
			{
				case Format.A8:
					return GorgonBufferFormat.A8_UIntNormal;
				case Format.A1R5G5B5:
					return GorgonBufferFormat.B5G5R5A1_UIntNormal;
				case Format.R5G6B5:
					return GorgonBufferFormat.B5G6R5_UIntNormal;
				case Format.A8B8G8R8:
					return GorgonBufferFormat.B8G8R8A8_UIntNormal;
				case Format.X8B8G8R8:
					return GorgonBufferFormat.B8G8R8X8_UIntNormal;
				case Format.Dxt1:
				case Format.Dxt2:
					return GorgonBufferFormat.BC1_UIntNormal;
				case Format.Dxt3:
				case Format.Dxt4:
					return GorgonBufferFormat.BC2_UIntNormal;
				case Format.Dxt5:
					return GorgonBufferFormat.BC3_UIntNormal;
				case Format.D15S1:
					return GorgonBufferFormat.D15_UIntNormal_S1_UInt;
				case Format.D16:
					return GorgonBufferFormat.D16_UIntNormal;
				case Format.D16Lockable:
					return GorgonBufferFormat.D16_UIntNormal_Lockable;
				case Format.D24X8:
					return GorgonBufferFormat.D24_UIntNormal_X8;
				case Format.D24X4S4:
					return GorgonBufferFormat.D24_UIntNormal_X4S4_UInt;
				case Format.D24S8:
					return GorgonBufferFormat.D24_UIntNormal_S8_UInt;
				case Format.D32:
					return GorgonBufferFormat.D32_UIntNormal;
				case Format.D32SingleLockable:
					return GorgonBufferFormat.D32_Float_Lockable;
				case Format.G8R8_G8B8:
					return GorgonBufferFormat.G8R8_G8B8_UIntNormal;
				case Format.A2R10G10B10:
					return GorgonBufferFormat.R10G10B10A2_UIntNormal;
				case Format.R16F:
					return GorgonBufferFormat.R16_Float;
				case Format.G16R16:
					return GorgonBufferFormat.R16G16;
				case Format.G16R16F:
					return GorgonBufferFormat.R16G16_Float;
				case Format.A16B16G16R16:
					return GorgonBufferFormat.R16G16B16A16;
				case Format.A16B16G16R16F:
					return GorgonBufferFormat.R16G16B16A16_Float;
				case Format.R32F:
					return GorgonBufferFormat.R32_Float;
				case Format.G32R32F:
					return GorgonBufferFormat.R32G32_Float;
				case Format.A32B32G32R32F:
					return GorgonBufferFormat.R32G32B32A32_Float;
				case Format.L8:
					return GorgonBufferFormat.R8;
				case Format.R8G8_B8G8:
					return GorgonBufferFormat.R8G8_B8G8_UIntNormal;
				case Format.A8R8G8B8:
					return GorgonBufferFormat.R8G8B8A8;
				case Format.X8R8G8B8:
					return GorgonBufferFormat.X8_R8G8B8_UIntNormal;
				default:
					return GorgonBufferFormat.Unknown;
			}
		}

		/// <summary>
		/// Function to convert a buffer format to a D3D format.
		/// </summary>
		/// <param name="format">Gorgon buffer format to convert.</param>
		/// <returns>The D3D format.</returns>
		public static Format Convert(GorgonBufferFormat format)
		{
			switch (format)
			{
				case GorgonBufferFormat.A8_UIntNormal:
					return Format.A8;
				case GorgonBufferFormat.B5G5R5A1_UIntNormal:
					return Format.A1R5G5B5;
				case GorgonBufferFormat.B5G6R5_UIntNormal:
					return Format.R5G6B5;
				case GorgonBufferFormat.B8G8R8A8_UIntNormal:
				case GorgonBufferFormat.B8G8R8A8_UIntNormal_sRGB:
				case GorgonBufferFormat.B8G8R8A8:
					return Format.A8B8G8R8;
				case GorgonBufferFormat.B8G8R8X8_UIntNormal:
				case GorgonBufferFormat.B8G8R8X8_UIntNormal_sRGB:
				case GorgonBufferFormat.B8G8R8X8:
					return Format.X8B8G8R8;
				case GorgonBufferFormat.BC1:
				case GorgonBufferFormat.BC1_UIntNormal:
				case GorgonBufferFormat.BC1_UIntNormal_sRGB:
					return Format.Dxt1;
				case GorgonBufferFormat.BC2:
				case GorgonBufferFormat.BC2_UIntNormal:
				case GorgonBufferFormat.BC2_UIntNormal_sRGB:
					return Format.Dxt3;
				case GorgonBufferFormat.BC3:
				case GorgonBufferFormat.BC3_UIntNormal:
				case GorgonBufferFormat.BC3_UIntNormal_sRGB:
					return Format.Dxt5;
				case GorgonBufferFormat.D15_UIntNormal_S1_UInt:
					return Format.D15S1;
				case GorgonBufferFormat.D16_UIntNormal:
					return Format.D16;
				case GorgonBufferFormat.D16_UIntNormal_Lockable:
					return Format.D16Lockable;
				case GorgonBufferFormat.D24_UIntNormal_X8:
					return Format.D24X8;
				case GorgonBufferFormat.D24_UIntNormal_X4S4_UInt:
					return Format.D24X4S4;
				case GorgonBufferFormat.D24_UIntNormal_S8_UInt:
					return Format.D24S8;
				case GorgonBufferFormat.D32_UIntNormal:
					return Format.D32;
				case GorgonBufferFormat.D32_Float_Lockable:
				case GorgonBufferFormat.D32_Float:
					return Format.D32SingleLockable;
				case GorgonBufferFormat.G8R8_G8B8_UIntNormal:					
					return Format.G8R8_G8B8;
				case GorgonBufferFormat.R10G10B10_XR_Bias_A2_UIntNormal:
				case GorgonBufferFormat.R10G10B10A2:
				case GorgonBufferFormat.R10G10B10A2_UInt:
				case GorgonBufferFormat.R10G10B10A2_UIntNormal:
					return Format.A2R10G10B10;
				case GorgonBufferFormat.R16_Float:
					return Format.R16F;
				case GorgonBufferFormat.R16G16_Int:
				case GorgonBufferFormat.R16G16_IntNormal:
				case GorgonBufferFormat.R16G16_UInt:
				case GorgonBufferFormat.R16G16_UIntNormal:
				case GorgonBufferFormat.R16G16:
					return Format.G16R16;
				case GorgonBufferFormat.R16G16_Float:
					return Format.G16R16F;
				case GorgonBufferFormat.R16G16B16A16_Int:
				case GorgonBufferFormat.R16G16B16A16_IntNormal:
				case GorgonBufferFormat.R16G16B16A16_UInt:
				case GorgonBufferFormat.R16G16B16A16_UIntNormal:
				case GorgonBufferFormat.R16G16B16A16:
					return Format.A16B16G16R16;
				case GorgonBufferFormat.R16G16B16A16_Float:
					return Format.A16B16G16R16F;
				case GorgonBufferFormat.R32_Float:
					return Format.R32F;
				case GorgonBufferFormat.R32G32_Float:
					return Format.G32R32F;
				case GorgonBufferFormat.R32G32B32A32_Float:
					return Format.A32B32G32R32F;
				case GorgonBufferFormat.R8:
				case GorgonBufferFormat.R8_Int:
				case GorgonBufferFormat.R8_IntNormal:
				case GorgonBufferFormat.R8_UInt:
				case GorgonBufferFormat.R8_UIntNormal:
					return Format.L8;
				case GorgonBufferFormat.R8G8_B8G8_UIntNormal:
					return Format.R8G8_B8G8;
				case GorgonBufferFormat.R8G8B8A8:
				case GorgonBufferFormat.R8G8B8A8_Int:
				case GorgonBufferFormat.R8G8B8A8_IntNormal:
				case GorgonBufferFormat.R8G8B8A8_UIntNormal:
				case GorgonBufferFormat.R8G8B8A8_UIntNormal_sRGB:
					return Format.A8R8G8B8;
				case GorgonBufferFormat.X8_R8G8B8_UIntNormal:
					return Format.X8R8G8B8;					
				default:
					return Format.Unknown;
			}
		}

		/// <summary>
		/// Function to convert D3D comparison caps to Gorgon comparison flags.
		/// </summary>
		/// <param name="compareFlags">D3D Comparison caps.</param>
		/// <returns>Gorgon Comparison flags.</returns>
		public static GorgonCompareFlags Convert(CompareCaps compareFlags)
		{
			GorgonCompareFlags result = GorgonCompareFlags.Always;
			
			if ((compareFlags & CompareCaps.Always) == CompareCaps.Always)
				result |= GorgonCompareFlags.Always;

			if ((compareFlags & CompareCaps.Equal) == CompareCaps.Equal)
				result |= GorgonCompareFlags.Equal;

			if ((compareFlags & CompareCaps.Greater) == CompareCaps.Greater)
				result |= GorgonCompareFlags.Greater;

			if ((compareFlags & CompareCaps.GreaterEqual) == CompareCaps.GreaterEqual)
				result |= GorgonCompareFlags.GreaterEqual;

			if ((compareFlags & CompareCaps.Less) == CompareCaps.Less)
				result |= GorgonCompareFlags.Less;

			if ((compareFlags & CompareCaps.LessEqual) == CompareCaps.LessEqual)
				result |= GorgonCompareFlags.LessEqual;

			if ((compareFlags & CompareCaps.Never) == CompareCaps.Never)
				result |= GorgonCompareFlags.LessEqual;

			if ((compareFlags & CompareCaps.NotEqual) == CompareCaps.NotEqual)
				result |= GorgonCompareFlags.NotEqual;

			return result;
		}

		/// <summary>
		/// Function to convert Gorgon comparison flags to the D3D compare value.
		/// </summary>
		/// <param name="compareFlags">Comparison flags to convert.</param>
		/// <returns>The Direct3D comparison flag.</returns>
		public static Compare Convert(GorgonCompareFlags compareFlags)
		{
			switch (compareFlags)
			{
				case GorgonCompareFlags.Equal:
					return Compare.Equal;
				case GorgonCompareFlags.Greater:
					return Compare.Greater;
				case GorgonCompareFlags.GreaterEqual:
					return Compare.GreaterEqual;
				case GorgonCompareFlags.Less:
					return Compare.Less;
				case GorgonCompareFlags.LessEqual:
					return Compare.LessEqual;
				case GorgonCompareFlags.Never:
					return Compare.Never;
				case GorgonCompareFlags.NotEqual:
					return Compare.NotEqual;
				default:
					return Compare.Always;
			}
		}

		/// <summary>
		/// Function to convert D3D comparison values to a Gorgon comparison flag.
		/// </summary>
		/// <param name="compareFlags">D3D Comparison value.</param>
		/// <returns>The Gorgon comparison flag.</returns>
		public static GorgonCompareFlags Convert(Compare compareFlags)
		{
			switch (compareFlags)
			{
				case Compare.Equal:
					return GorgonCompareFlags.Equal;
				case Compare.Greater:
					return GorgonCompareFlags.Greater;
				case Compare.GreaterEqual:
					return GorgonCompareFlags.GreaterEqual;
				case Compare.Less:
					return GorgonCompareFlags.Less;
				case Compare.LessEqual:
					return GorgonCompareFlags.LessEqual;
				case Compare.Never:
					return GorgonCompareFlags.Never;
				case Compare.NotEqual:
					return GorgonCompareFlags.NotEqual;
				default:
					return GorgonCompareFlags.Always;
			}
		}

		/// <summary>
		/// Function to convert Gorgon swap chain settings into D3D presentation parameters.
		/// </summary>
		/// <param name="settings">Settings to convert.</param>
		/// <returns>The D3D presentation parameters.</returns>
		public static PresentParameters Convert(GorgonSwapChainSettings settings)
		{
			if (settings.DisplayFunction == GorgonDisplayFunction.Copy)
				settings.BackBufferCount = 1;

			PresentParameters result = new PresentParameters()
			{
				AutoDepthStencilFormat = Format.Unknown,
				BackBufferCount = settings.BackBufferCount,
				BackBufferFormat = D3DConvert.Convert(settings.Format),
				BackBufferHeight = settings.Height,
				BackBufferWidth = settings.Width,
				DeviceWindowHandle = settings.BoundWindow.Handle,
				EnableAutoDepthStencil = false,
				Windowed = false,
				FullScreenRefreshRateInHertz = 0,
				Multisample = D3DConvert.Convert(settings.MSAAQualityLevel.Level),
				MultisampleQuality = (settings.MSAAQualityLevel.Level != GorgonMSAALevel.None ? settings.MSAAQualityLevel.Quality - 1 : 0),
				PresentationInterval = PresentInterval.Immediate,
				PresentFlags = (settings.WillUseVideo ? PresentFlags.Video : PresentFlags.None),
				SwapEffect = D3DConvert.Convert(settings.DisplayFunction)
			};

			return result;
		}

		/// <summary>
		/// Function to convert Gorgon device window settings into D3D presentation parameters.
		/// </summary>
		/// <param name="settings">Settings to convert.</param>
		/// <returns>The D3D presentation parameters.</returns>
		public static PresentParameters Convert(GorgonDeviceWindowSettings settings)
		{
			Format depthFormat = Convert(settings.DepthStencilFormat);

			if (settings.DisplayFunction == GorgonDisplayFunction.Copy)
				settings.BackBufferCount = 1;

			// Turn off vsync and refresh rates when in windowed mode.
			if (settings.IsWindowed)
			{
				if (settings.VSyncInterval != GorgonVSyncInterval.None)
					settings.VSyncInterval = GorgonVSyncInterval.None;
				if (settings.RefreshRateNumerator > 0)
					settings.RefreshRateNumerator = 0;
			}

			PresentParameters result = new PresentParameters()
			{
				AutoDepthStencilFormat = depthFormat,
				BackBufferCount = settings.BackBufferCount,
				BackBufferFormat = D3DConvert.Convert(settings.Format),
				BackBufferHeight = settings.Height,
				BackBufferWidth = settings.Width,
				DeviceWindowHandle = settings.BoundWindow.Handle,
				EnableAutoDepthStencil = (depthFormat != SlimDX.Direct3D9.Format.Unknown),
				Windowed = settings.IsWindowed,
				FullScreenRefreshRateInHertz = settings.RefreshRateNumerator,
				Multisample = D3DConvert.Convert(settings.MSAAQualityLevel.Level),
				MultisampleQuality = (settings.MSAAQualityLevel.Level != GorgonMSAALevel.None ? settings.MSAAQualityLevel.Quality - 1 : 0),
				PresentationInterval = D3DConvert.Convert(settings.VSyncInterval),
				PresentFlags = (settings.WillUseVideo ? PresentFlags.Video : PresentFlags.None),
				SwapEffect = D3DConvert.Convert(settings.DisplayFunction)
			};

			return result;
		}

		/// <summary>
		/// Function to convert a gorgon color value to a D3D 4 part color value.
		/// </summary>
		/// <param name="color">Color to convert.</param>
		/// <returns>The D3D color.</returns>
		public static SlimDX.Color4 Convert(GorgonColor color)
		{
			return new SlimDX.Color4(color.Alpha, color.Red, color.Green, color.Blue);
		}

		/// <summary>
		/// Function to convert a D3D color value to a color part color value.
		/// </summary>
		/// <param name="color">D3D color to convert.</param>
		/// <returns>The Gorgon color.</returns>
		public static GorgonColor Convert(SlimDX.Color4 color)
		{
			return new GorgonColor(color.Alpha, color.Red, color.Green, color.Blue);
		}

		/// <summary>
		/// Function to convert a D3D color value to a color part color value.
		/// </summary>
		/// <param name="color">D3D color to convert.</param>
		/// <returns>The Gorgon color.</returns>
		public static GorgonColor Convert(SlimDX.Color3 color)
		{
			return new GorgonColor(1.0f, color.Red, color.Green, color.Blue);
		}
		#endregion
	}
}
