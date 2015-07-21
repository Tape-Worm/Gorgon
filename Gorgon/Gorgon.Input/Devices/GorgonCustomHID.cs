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
// Created: Thursday, June 30, 2011 12:51:38 PM
// 
#endregion

using System;
using System.IO;
using Gorgon.Collections;
using Gorgon.Collections.Specialized;
using Gorgon.Input.Properties;

namespace Gorgon.Input
{
	/// <summary>
	/// Event parameters for the <see cref="E:Gorgon.Input.GorgonCustomHID.DataChanged">DataChanged</see> event.
	/// </summary>
	public class GorgonCustomHIDDataChangedEventArgs
		: EventArgs
	{
		#region Variables.
		private readonly GorgonCustomHIDProperty _property;			// Property with the changed data.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the name of the changed property.
		/// </summary>
		public string PropertyName => _property.Name;

		#endregion

		#region Methods.
		/// <summary>
		/// Function to return the data in the property.
		/// </summary>
		/// <returns>The data in the property.</returns>
		public object GetData()
		{
			return _property.GetValue();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonCustomHIDDataChangedEventArgs"/> class.
		/// </summary>
		/// <param name="property">The property that has changed.</param>
		internal GorgonCustomHIDDataChangedEventArgs(GorgonCustomHIDProperty property)
		{
		    _property = property;
		}
		#endregion
	}

	/// <summary>
	/// An unknown input device.
	/// </summary>
	/// <remarks>Unknown devices won't have a class wrapper for them, but instead use specific methods to set/return the values for the device.</remarks>
	public abstract class GorgonCustomHID
		: GorgonInputDevice
	{
		#region Events.
		/// <summary>
		/// Event fired when the data for the device changes.
		/// </summary>
		public event EventHandler<GorgonCustomHIDDataChangedEventArgs> DataChanged;
		#endregion

		#region Variables.
		// Custom properties for the HID.
		private readonly GorgonNamedObjectDictionary<GorgonCustomHIDProperty> _properties = new GorgonNamedObjectDictionary<GorgonCustomHIDProperty>(false);
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return information about the device.
		/// </summary>
		public IGorgonHumanInterfaceDeviceInfo Info
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the user organized data for the device.
		/// </summary>
		public IGorgonNamedObjectReadOnlyDictionary<GorgonCustomHIDProperty> Data => _properties;

		#endregion

		#region Methods.
		/// <summary>
		/// Function to clear the properties and their values.
		/// </summary>
		protected internal void ClearData()
		{
			_properties.Clear();
		}
		
		/// <summary>
		/// Function to set a value for a property.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="value">Value to assign to the property.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="propertyName"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the propertyName parameter is an empty string.</exception>
		protected void SetData(string propertyName, object value)
		{
			if (propertyName == null)
			{
				throw new ArgumentNullException(nameof(propertyName));
			}

			if (string.IsNullOrWhiteSpace(propertyName))
			{
				throw new ArgumentException(Resources.GORINP_ERR_PARAMETER_MUST_NOT_BE_EMPTY, nameof(propertyName));
			}

			if (Data[propertyName] == null)
			{
				throw new InvalidDataException(string.Format(Resources.GORINP_ERR_HID_PROPERTY_NULL, propertyName));
			}

		    GorgonCustomHIDProperty property;

		    if (Data.TryGetValue(propertyName, out property))
		    {
		        property.SetValue(value);
		    }
		    else
		    {
		        _properties.Add(new GorgonCustomHIDProperty(propertyName, value));
		    }

			DataChanged?.Invoke(this, new GorgonCustomHIDDataChangedEventArgs(Data[propertyName]));
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonCustomHID"/> class.
		/// </summary>
		/// <param name="owner">The input service that owns this device.</param>
		/// <param name="deviceInfo">The device information used to create the device.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="owner"/> parameter is <b>null</b> (<i>Nothing</i> in VB.NET).</exception>
		protected GorgonCustomHID(GorgonInputService owner, IGorgonHumanInterfaceDeviceInfo deviceInfo)
			: base(owner, deviceInfo)
		{
			Info = deviceInfo;
		}
		#endregion
	}
}
