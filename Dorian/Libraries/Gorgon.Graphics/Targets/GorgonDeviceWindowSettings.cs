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
// Created: Saturday, August 06, 2011 1:01:21 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GorgonLibrary.Native;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Values for setting up a head on a multi-head display adatper.
	/// </summary>
	public class GorgonDeviceWindowHeadSettings
		: GorgonSwapChainSettings
	{
		#region Variables.
		private int _refreshDenominator = 1;	// Refresh rate denominator.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the master device window settings for this head.
		/// </summary>
		public GorgonDeviceWindowSettings MasterDeviceWindowSettings
		{
			get;
			internal set;
		}

		/// <summary>
		/// Property to return the video output that these settings apply to.
		/// </summary>
		public GorgonVideoOutput Output
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the width, height, format and refresh rate of the device window.
		/// </summary>
		public GorgonVideoMode DisplayMode
		{
			get
			{
				return new GorgonVideoMode(Width, Height, Format, RefreshRateNumerator, RefreshRateDenominator);
			}
		}

		/// <summary>
		/// Property to set or return the refresh rate numerator value.
		/// </summary>
		public int RefreshRateNumerator
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the refresh rate denominator value.
		/// </summary>
		public int RefreshRateDenominator
		{
			get
			{
				return _refreshDenominator;
			}
			set
			{
				if (value < 1)
					value = 1;
				_refreshDenominator = value;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDeviceWindowHeadSettings"/> class.
		/// </summary>
		/// <param name="boundForm">The form that is bound to the device window head.</param>
		/// <remarks>This must be a form because the multi-head settings will only operate in full-screen mode and child controls cannot go full screen.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <param name="output"/> parameter is NULL (Nothing in VB.Net).</exception>
		public GorgonDeviceWindowHeadSettings(Form boundForm, GorgonVideoOutput output)
			: base(boundForm)
		{
			if (output == null)
				throw new ArgumentNullException("output");

			Output = output;
		}
		#endregion
	}

	/// <summary>
	/// Values for setting up a <see cref="GorgonLibrary.Graphics.GorgonDeviceWindow">Gorgon Device Window</see>.
	/// </summary>
	public class GorgonDeviceWindowSettings
		: GorgonSwapChainSettings
	{
		#region Classes.
		/// <summary>
		/// Device window settings collection.
		/// </summary>
		public class GorgonDeviceWindowHeadSettingsCollection
			: IList<GorgonDeviceWindowHeadSettings>
		{
			#region Variables.
			private List<GorgonDeviceWindowHeadSettings> _settings = null;		// Settings.
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Prevents a default instance of the <see cref="GorgonDeviceWindowHeadSettingsCollection"/> class from being created.
			/// </summary>
			internal GorgonDeviceWindowHeadSettingsCollection(IEnumerable<GorgonDeviceWindowHeadSettings> settings)
			{
				_settings = new List<GorgonDeviceWindowHeadSettings>();
				if (settings != null)
					_settings.AddRange(settings);
			}
			#endregion

			#region IList<GorgonDeviceWindowHeadSettings> Members
			#region Properties.
			/// <summary>
			/// Gets or sets the element at the specified index.
			/// </summary>
			/// <returns>
			/// The element at the specified index.
			///   </returns>
			///   
			/// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"/>.
			///   </exception>
			///   
			/// <exception cref="T:System.NotSupportedException">
			/// The property is set and the <see cref="T:System.Collections.Generic.IList`1"/> is read-only.
			///   </exception>
			public GorgonDeviceWindowHeadSettings this[int index]
			{
				get
				{
					return _settings[index];
				}
				set
				{
					throw new NotSupportedException();
				}
			}
			#endregion

			#region Methods.
			/// <summary>
			/// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1"/>.
			/// </summary>
			/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1"/>.</param>
			/// <returns>
			/// The index of <paramref name="item"/> if found in the list; otherwise, -1.
			/// </returns>
			public int IndexOf(GorgonDeviceWindowHeadSettings item)
			{
				return _settings.IndexOf(item);
			}

			/// <summary>
			/// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1"/> at the specified index.
			/// </summary>
			/// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
			/// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1"/>.</param>
			/// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"/>.
			///   </exception>
			///   
			/// <exception cref="T:System.NotSupportedException">
			/// The <see cref="T:System.Collections.Generic.IList`1"/> is read-only.
			///   </exception>
			void IList<GorgonDeviceWindowHeadSettings>.Insert(int index, GorgonDeviceWindowHeadSettings item)
			{
				throw new NotSupportedException();
			}

			/// <summary>
			/// Removes the <see cref="T:System.Collections.Generic.IList`1"/> item at the specified index.
			/// </summary>
			/// <param name="index">The zero-based index of the item to remove.</param>
			/// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"/>.
			///   </exception>
			///   
			/// <exception cref="T:System.NotSupportedException">
			/// The <see cref="T:System.Collections.Generic.IList`1"/> is read-only.
			///   </exception>
			void IList<GorgonDeviceWindowHeadSettings>.RemoveAt(int index)
			{
				throw new NotSupportedException();
			}
			#endregion
			#endregion

			#region ICollection<GorgonDeviceWindowHeadSettings> Members
			#region Properties.
			/// <summary>
			/// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
			/// </summary>
			/// <returns>
			/// The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
			///   </returns>
			public int Count
			{
				get
				{
					return _settings.Count;
				}
			}

			/// <summary>
			/// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
			/// </summary>
			/// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only; otherwise, false.
			///   </returns>
			public bool IsReadOnly
			{
				get
				{
					return true;
				}
			}

			#endregion

			#region Methods.
			/// <summary>
			/// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
			/// </summary>
			/// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
			/// <returns>
			/// true if <paramref name="item"/> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"/>.
			/// </returns>
			/// <exception cref="T:System.NotSupportedException">
			/// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
			///   </exception>
			bool ICollection<GorgonDeviceWindowHeadSettings>.Remove(GorgonDeviceWindowHeadSettings item)
			{
				throw new NotSupportedException();
			}

			/// <summary>
			/// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
			/// </summary>
			/// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
			/// <exception cref="T:System.NotSupportedException">
			/// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
			///   </exception>
			void ICollection<GorgonDeviceWindowHeadSettings>.Add(GorgonDeviceWindowHeadSettings item)
			{
				throw new NotSupportedException();
			}

			/// <summary>
			/// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
			/// </summary>
			/// <exception cref="T:System.NotSupportedException">
			/// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
			///   </exception>
			void ICollection<GorgonDeviceWindowHeadSettings>.Clear()
			{
				throw new NotSupportedException();
			}

			/// <summary>
			/// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"/> contains a specific value.
			/// </summary>
			/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
			/// <returns>
			/// true if <paramref name="item"/> is found in the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false.
			/// </returns>
			public bool Contains(GorgonDeviceWindowHeadSettings item)
			{
				return _settings.Contains(item);
			}

			/// <summary>
			/// Copies to.
			/// </summary>
			/// <param name="array">The array.</param>
			/// <param name="arrayIndex">Index of the array.</param>
			public void CopyTo(GorgonDeviceWindowHeadSettings[] array, int arrayIndex)
			{
				_settings.CopyTo(array, arrayIndex);
			}
			#endregion
			#endregion

			#region IEnumerable<GorgonDeviceWindowHeadSettings> Members
			/// <summary>
			/// Returns an enumerator that iterates through the collection.
			/// </summary>
			/// <returns>
			/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
			/// </returns>
			public IEnumerator<GorgonDeviceWindowHeadSettings> GetEnumerator()
			{
				foreach (var item in _settings)
					yield return item;
			}
			#endregion

			#region IEnumerable Members
			/// <summary>
			/// Returns an enumerator that iterates through a collection.
			/// </summary>
			/// <returns>
			/// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
			/// </returns>
			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return this.GetEnumerator();
			}
			#endregion
		}
		#endregion
		
		#region Variables.
		private int _refreshDenominator = 1;	// Refresh rate denominator.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the video device to use.
		/// </summary>
		public GorgonVideoDevice Device
		{
			get;
			internal set;
		}

		/// <summary>
		/// Property to return the video output to use.
		/// </summary>
		public GorgonVideoOutput Output
		{
			get;
			internal set;
		}

		/// <summary>
		/// Property to return the width, height, format and refresh rate of the device window.
		/// </summary>
		public GorgonVideoMode DisplayMode
		{
			get
			{
				return new GorgonVideoMode(Width, Height, Format, RefreshRateNumerator, RefreshRateDenominator);
			}
		}

		/// <summary>
		/// Property to set or return the refresh rate numerator value.
		/// </summary>
		public int RefreshRateNumerator
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the refresh rate denominator value.
		/// </summary>
		public int RefreshRateDenominator
		{
			get
			{
				return _refreshDenominator;
			}
			set
			{
				if (value < 1)
					value = 1;
				_refreshDenominator = value;
			}
		}

		/// <summary>
		/// Property to set or return whether the device window should go to full screen or windowed mode.
		/// </summary>
		public bool IsWindowed
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the settings for each head.
		/// </summary>
		public GorgonDeviceWindowHeadSettingsCollection HeadSettings
		{
			get;
			private set;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDeviceWindowSettings"/> class.
		/// </summary>
		/// <param name="device">The video device to use.</param>
		/// <param name="output">The video output to use.</param>
		/// <param name="boundWindow">The window to bind to the device window.</param>
		/// <param name="multiHeadSettings">A list of device window settings for each subordinate head.</param>
		/// <remarks>Pass NULL (Nothing in VB.Net) to the <paramref name="boundWindow"/> parameter to use the default Gorgon <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">ApplicationWindow</see>.
		/// <para>Passing NULL (Nothing in VB.Net) to the <paramref name="device"/> or <paramref name="output"/> will use the device/output that holds the window.</para>
		/// <para>This object will be the master head in a multi-head setting, all others are subordinates based on the values passed to the <paramref name="multiHeadSettings"/> parameter.</para>
		/// </remarks>
		public GorgonDeviceWindowSettings(GorgonVideoDevice device, GorgonVideoOutput output, Control boundWindow, IEnumerable<GorgonDeviceWindowHeadSettings> multiHeadSettings)
			: base(boundWindow == null ? Gorgon.ApplicationForm : boundWindow)
		{
			Device = device;
			Output = output;
			HeadSettings = new GorgonDeviceWindowHeadSettingsCollection(multiHeadSettings);

			foreach (var setting in HeadSettings)
				setting.MasterDeviceWindowSettings = this;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDeviceWindowSettings"/> class.
		/// </summary>
		/// <param name="device">The video device to use.</param>
		/// <param name="output">The video output to use.</param>
		/// <param name="boundWindow">The window to bind to the device window.</param>
		/// <remarks>Pass NULL (Nothing in VB.Net) to the <paramref name="boundWindow"/> parameter to use the default Gorgon <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">ApplicationWindow</see>.
		/// <para>Passing NULL (Nothing in VB.Net) to the <paramref name="device"/> or <paramref name="output"/> will use the device/output that holds the window.</para>
		/// </remarks>
		public GorgonDeviceWindowSettings(GorgonVideoDevice device, GorgonVideoOutput output, Control boundWindow)
			: this(device, output, boundWindow, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDeviceWindowSettings"/> class.
		/// </summary>
		/// <param name="boundWindow">The window to bind to the device window.</param>
		/// <remarks>Pass NULL (Nothing in VB.Net) to the <paramref name="boundWindow"/> parameter to use the default Gorgon <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">ApplicationWindow</see>.
		/// </remarks>
		public GorgonDeviceWindowSettings(Control boundWindow)
			: this(null, null, boundWindow, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDeviceWindowSettings"/> class.
		/// </summary>
		public GorgonDeviceWindowSettings()
			: this(null, null, null, null)
		{			
		}
		#endregion
	}
}
