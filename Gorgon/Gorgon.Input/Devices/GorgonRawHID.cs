#region MIT
// 
// Gorgon
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
// Created: Tuesday, September 07, 2015 2:27:10 PM
// 
#endregion

using System;
using Gorgon.Native;

namespace Gorgon.Input
{
	/// <inheritdoc cref="IGorgonRawHID"/>
	public class GorgonRawHID
		: IGorgonRawHID, IGorgonRawInputDeviceData<GorgonRawHIDData>
	{
		#region Variables.
		// Pre parsed data for this device.
		private GorgonPointer _preParsedData;
		// Synchronization for multiple threads.
		private readonly object _syncLock = new object();
		#endregion

		#region Events.
		/// <inheritdoc/>
		public event EventHandler<GorgonHIDEventArgs> DataReceived;
		#endregion

		#region Properties.
		/// <inheritdoc/>
		public IntPtr Handle => Info.Handle;

		/// <inheritdoc/>
		public RawInputType DeviceType => RawInputType.HID;

		/// <inheritdoc/>
		HIDUsage IGorgonRawInputDevice.DeviceUsage => Info.Usage;

		/// <inheritdoc/>
		public IGorgonRawHIDInfo Info
		{
			get;
		}

		/// <inheritdoc/>
		public GorgonPointerAlias Data
		{
			get;
			private set;
		}

		/// <inheritdoc/>
		public int HIDSize
		{
			get;
			private set;
		}

		/// <inheritdoc/>
		public int Count
		{
			get;
			private set;
		}

		/// <inheritdoc/>
		public GorgonPointer PreParsedData
		{
			get
			{
				lock (_syncLock)
				{
					if (_preParsedData == null)
					{
						_preParsedData = RawInputApi.GetPreparsedDeviceInfoData(Handle);
					}
				}

				return _preParsedData;
			}
		}
		#endregion

		#region Methods.
		/// <inheritdoc/>
		void IGorgonRawInputDeviceData<GorgonRawHIDData>.ProcessData(ref GorgonRawHIDData rawInputData)
		{
			Data = rawInputData.HidData;
			HIDSize = rawInputData.HIDDataSize;
			Count = rawInputData.ItemCount;

			DataReceived?.Invoke(this, new GorgonHIDEventArgs(rawInputData.HidData, rawInputData.HIDDataSize, rawInputData.ItemCount));
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			_preParsedData?.Dispose();
			_preParsedData = null;

			GC.SuppressFinalize(this);
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonRawHID"/> class.
		/// </summary>
		/// <param name="hidInfo">The human interface device information used to determine which keyboard to use.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="hidInfo"/> is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		public GorgonRawHID(GorgonRawHIDInfo hidInfo)
		{
			if (hidInfo == null)
			{
				throw new ArgumentNullException(nameof(hidInfo));
			}

			Info = hidInfo;
		}
		#endregion
	}
}
