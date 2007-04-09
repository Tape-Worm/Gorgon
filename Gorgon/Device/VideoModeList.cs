#region LGPL.
// 
// Gorgon.
// Copyright (C) 2005 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 
// Created: Monday, July 11, 2005 5:35:47 PM
// 
#endregion

using System;
using System.Text;
using SharpUtilities;
using SharpUtilities.Utility;
using SharpUtilities.Collections;
using DX = Microsoft.DirectX;
using D3D = Microsoft.DirectX.Direct3D;
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
	public class VideoModeList : DynamicArray<VideoMode>
	{
		#region Variables.
		private int _widthFilter;					// Filter for width.
		private int _heightFilter;					// Filter for height.
		private int _bitsPerPixelFilter;			// Filter for BPP.
		private int _refreshFilter;					// Filter for refresh rate.
		private CompareFunctions _filterCompare;	// Comparison function for mode filter.
		private int _driverIndex;					// Index of the driver.
		#endregion

		#region Properties.
		/// <summary>
		/// Indexer to return a video mode by the parameters passed in the video mode structure.
		/// </summary>		
		public VideoMode this[VideoMode searchmode]
		{
			get
			{
				foreach (VideoMode mode in _items)
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
#if DEBUG
			StringBuilder modeString = null;		// Video mode string.
			int modeCount;							// Mode counter.
#endif

			// Clear current device list.
			_items.Clear();

#if DEBUG
			Gorgon.Log.Print("VideoModeList", "Enumerating video modes...", LoggingLevel.Verbose);
			Gorgon.Log.Print("VideoModeList", "Filtering: {0}, Parameters: {1}x{2}x{3} {4}Hz.", LoggingLevel.Verbose,
				FilterCompare.ToString(), (WidthFilter == -1) ? "*" : WidthFilter.ToString(), (HeightFilter == -1) ? "*" : HeightFilter.ToString(), (BppFilter == -1) ? "*" : BppFilter.ToString(), (RefreshFilter == -1) ? "*" : RefreshFilter.ToString());
			modeCount = 0;
#endif
			// Enumerate video modes.

			foreach (D3D.DisplayMode mode in D3D.Manager.Adapters[_driverIndex].SupportedDisplayModes)
			{
				if (Renderer.ValidFormat(_driverIndex, mode.Format, mode.Format, false))
				{
					vidmode = new VideoMode(mode.Width, mode.Height, mode.RefreshRate, Converter.Convert(mode.Format));
					if (vidmode.Bpp != 0)
					{
						// Apply filtering.
						// I'm not too keen on the having to repeat the logging, but it appears
						// to be necessary, either that or I'm in need of sleep.
						switch (FilterCompare)
						{
							case CompareFunctions.Equal:
								if ((vidmode.Width == WidthFilter) && (vidmode.Height == HeightFilter) && (vidmode.RefreshRate == RefreshFilter) && (vidmode.Bpp == BppFilter))
									_items.Add(vidmode);
								break;
							case CompareFunctions.GreaterThan:
								if ((vidmode.Width > WidthFilter) && (vidmode.Height > HeightFilter) && (vidmode.RefreshRate > RefreshFilter) && (vidmode.Bpp > BppFilter))
									_items.Add(vidmode);
								break;
							case CompareFunctions.GreaterThanOrEqual:
								if ((vidmode.Width >= WidthFilter) && (vidmode.Height >= HeightFilter) && (vidmode.RefreshRate >= RefreshFilter) && (vidmode.Bpp >= BppFilter))
									_items.Add(vidmode);
								break;
							case CompareFunctions.LessThan:
								if ((vidmode.Width < WidthFilter) && (vidmode.Height < HeightFilter) && (vidmode.RefreshRate < RefreshFilter) && (vidmode.Bpp < BppFilter))
									_items.Add(vidmode);
								break;
							case CompareFunctions.LessThanOrEqual:
								if ((vidmode.Width <= WidthFilter) && (vidmode.Height <= HeightFilter) && (vidmode.RefreshRate <= RefreshFilter) && (vidmode.Bpp <= BppFilter))
									_items.Add(vidmode);
								break;
							case CompareFunctions.NotEqual:
								if ((vidmode.Width != WidthFilter) && (vidmode.Height != HeightFilter) && (vidmode.RefreshRate != RefreshFilter) && (vidmode.Bpp != BppFilter))
									_items.Add(vidmode);
								break;
							default:
								// 'Never' won't apply because we will always need a video mode.
								_items.Add(vidmode);
								break;
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
			foreach (VideoMode mode in _items)
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
#endif

			Gorgon.Log.Print("VideoModeList", "{0} video modes enumerated.", LoggingLevel.Intermediate, Count);
		}
		#endregion

		#region Constructors.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="driverIndex">Index of the video driver to use.</param>
		internal VideoModeList(int driverIndex)
		{
			// Default filters.
			_widthFilter = -1;
			_heightFilter = -1;
			_refreshFilter = -1;
			_bitsPerPixelFilter = -1;
			_filterCompare = CompareFunctions.Always;
			_driverIndex = driverIndex;

			// Get video modes.
			Refresh();
		}
		#endregion
	}
}
