#region MIT.
// 
// Gorgon.
// Copyright (C) 2005 Michael Winsor
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
// Created: Monday, July 11, 2005 5:35:47 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using DX = SlimDX;
using D3D9 = SlimDX.Direct3D9;
using GorgonLibrary.Internal;
using GorgonLibrary.Graphics;

namespace GorgonLibrary
{
	/// <summary>
	/// Class for holding a list of video modes.
	/// </summary>
	/// <remarks>
	/// Since an adapter can have a whole boatload of video modes, we should
	/// have a system of keeping track of the available video modes.
	/// </remarks>
	public class VideoModeList 
		: DynamicArray<VideoMode>
	{
		#region Variables.
		private int _widthFilter;					// Filter for width.
		private int _heightFilter;					// Filter for height.
		private int _bitsPerPixelFilter;			// Filter for BPP.
		private int _refreshFilter;					// Filter for refresh rate.
		private CompareFunctions _filterCompare;	// Comparison function for mode filter.
		private Driver _driver;						// Driver that owns this list.
		#endregion

		#region Properties.
		/// <summary>
		/// Indexer to return a video mode by the parameters passed in the video mode structure.
		/// </summary>		
		public VideoMode this[VideoMode searchmode]
		{
			get
			{
				foreach (VideoMode mode in Items)
				{
					if ((mode.Width == searchmode.Width) && (mode.Height == searchmode.Height) && (mode.RefreshRate == searchmode.RefreshRate) && (mode.Format == searchmode.Format))
						return mode;
				}

				throw new KeyNotFoundException(searchmode.ToString());
			}
		}

		/// <summary>
		/// Property to return or set the width filter.
		/// </summary>
		public int WidthFilter
		{
			get
			{
				return _widthFilter;
			}
			set
			{
				_widthFilter = value;
			}
		}

		/// <summary>
		/// Property to return or set the width filter.		
		/// </summary>
		public int RefreshFilter
		{
			get
			{
				return _refreshFilter;
			}
			set
			{
				_refreshFilter = value;
			}
		}

		/// <summary>
		/// Property to return or set the height filter.
		/// </summary>
		public int HeightFilter
		{
			get
			{
				return _heightFilter;
			}
			set
			{
				_heightFilter = value;
			}
		}

		/// <summary>
		/// Property to return or set the width filter.
		/// </summary>
		public int BppFilter
		{
			get
			{
				return _bitsPerPixelFilter;
			}
			set
			{
				_bitsPerPixelFilter = value;
			}
		}

		/// <summary>
		/// Property to return or set the comparison function for the mode filter.
		/// </summary>
		public CompareFunctions FilterCompare
		{
			get
			{
				return _filterCompare;
			}
			set
			{
				_filterCompare = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to refresh video mode list.
		/// </summary>
		public void Refresh()
		{
			VideoMode vidmode;						// Video mode.
			D3D9.DisplayModeCollection modes;				// List of supported modes.
#if DEBUG
			StringBuilder modeString = null;		// Video mode string.
			int modeCount;							// Mode counter.
#endif

			// Clear current device list.			
			ClearItems();

#if DEBUG
			Gorgon.Log.Print("VideoModeList", "Enumerating video modes...", LoggingLevel.Verbose);
			Gorgon.Log.Print("VideoModeList", "Filtering: {0}, Parameters: {1}x{2}x{3} {4}Hz.", LoggingLevel.Verbose,
				FilterCompare.ToString(), (WidthFilter == -1) ? "*" : WidthFilter.ToString(), (HeightFilter == -1) ? "*" : HeightFilter.ToString(), (BppFilter == -1) ? "*" : BppFilter.ToString(), (RefreshFilter == -1) ? "*" : RefreshFilter.ToString());
			modeCount = 0;
#endif
			// Enumerate video modes.
			foreach (BackBufferFormats format in Enum.GetValues(typeof(BackBufferFormats)))
			{
				modes = Gorgon.Direct3D.Adapters[_driver.DriverIndex].GetDisplayModes(Converter.Convert(format));
				foreach (D3D9.DisplayMode mode in modes)
				{
					if (_driver.ValidBackBufferFormat(Converter.Convert(mode.Format), Converter.Convert(mode.Format), false))
					{
						vidmode = new VideoMode(mode.Width, mode.Height, mode.RefreshRate, Converter.Convert(mode.Format));
						if (vidmode.Bpp != 0)
						{
							// Apply filtering.
							switch (FilterCompare)
							{
								case CompareFunctions.Equal:
									if ((vidmode.Width == WidthFilter) && (vidmode.Height == HeightFilter) && (vidmode.RefreshRate == RefreshFilter) && (vidmode.Bpp == BppFilter))
										Items.Add(vidmode);
									break;
								case CompareFunctions.GreaterThan:
									if ((vidmode.Width > WidthFilter) && (vidmode.Height > HeightFilter) && (vidmode.RefreshRate > RefreshFilter) && (vidmode.Bpp > BppFilter))
										Items.Add(vidmode);
									break;
								case CompareFunctions.GreaterThanOrEqual:
									if ((vidmode.Width >= WidthFilter) && (vidmode.Height >= HeightFilter) && (vidmode.RefreshRate >= RefreshFilter) && (vidmode.Bpp >= BppFilter))
										Items.Add(vidmode);
									break;
								case CompareFunctions.LessThan:
									if ((vidmode.Width < WidthFilter) && (vidmode.Height < HeightFilter) && (vidmode.RefreshRate < RefreshFilter) && (vidmode.Bpp < BppFilter))
										Items.Add(vidmode);
									break;
								case CompareFunctions.LessThanOrEqual:
									if ((vidmode.Width <= WidthFilter) && (vidmode.Height <= HeightFilter) && (vidmode.RefreshRate <= RefreshFilter) && (vidmode.Bpp <= BppFilter))
										Items.Add(vidmode);
									break;
								case CompareFunctions.NotEqual:
									if ((vidmode.Width != WidthFilter) && (vidmode.Height != HeightFilter) && (vidmode.RefreshRate != RefreshFilter) && (vidmode.Bpp != BppFilter))
										Items.Add(vidmode);
									break;
								default:
									// 'Never' won't apply because we will always need a video mode.
									Items.Add(vidmode);
									break;
							}
						}
					}
				}
			}
#if DEBUG
			modeCount = 0;
			modeString = new StringBuilder(256);
			Gorgon.Log.Print("VideoModeList", "W x H x Bpp\tFormat\t\tRefresh Rate\tW x H x Bpp\tFormat\t\tRefresh Rate\tW x H x Bpp\tFormat\t\tRefresh Rate\tW x H x Bpp\tFormat\t\tRefresh Rate", LoggingLevel.Verbose);
			Gorgon.Log.Print("VideoModeList", "===========\t======\t\t============\t===========\t======\t\t============\t===========\t======\t\t============\t===========\t======\t\t============", LoggingLevel.Verbose);
			// Do logging.
			foreach (VideoMode mode in Items)
			{
				if (_driver.ValidBackBufferFormat(mode.Format, mode.Format, true))
				{
					modeString.Append(string.Format("{0}x{1}x{2}\t({4})\t{3} Hz\t\t", mode.Width, mode.Height, mode.Bpp, mode.RefreshRate, mode.Format.ToString()));
					modeCount++;

					if (modeCount >= 4)
					{
						// Flush to the log.
						Gorgon.Log.Print("VideoModeList", modeString.ToString(), LoggingLevel.Verbose);
						modeCount = 0;
						modeString.Length = 0;
					}
				}
			}
#endif

			Gorgon.Log.Print("VideoModeList", "{0} video modes enumerated.", LoggingLevel.Intermediate, Count);
		}
		#endregion

		#region Constructors.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="driver">Driver that contains the video modes.</param>
		internal VideoModeList(Driver driver)
		{
			// Default filters.
			_widthFilter = -1;
			_heightFilter = -1;
			_refreshFilter = -1;
			_bitsPerPixelFilter = -1;
			_filterCompare = CompareFunctions.Always;
			_driver = driver;

			// Get video modes.
			Refresh();
		}
		#endregion
	}
}
