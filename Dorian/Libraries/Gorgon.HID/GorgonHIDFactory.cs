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
// Created: Monday, June 27, 2011 9:59:01 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GorgonLibrary.PlugIns;

namespace GorgonLibrary.HID
{
	/// <summary>
	/// A factory for loading various HID device factory interface plug-ins.
	/// </summary>
	public static class GorgonHIDFactory
	{
		#region Variables.
		private static GorgonInputCollection _hidFactories = null;			// HID device factory collection.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the list of HID device factories loaded.
		/// </summary>
		public static GorgonInputCollection HIDDeviceFactories
		{
			get
			{
				return _hidFactories;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to return a new HID device factory object.
		/// </summary>
		/// <param name="plugInType">Type name of the HID device factory.</param>
		/// <returns>The HID device factory object.</returns>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="plugInType"/> parameter is empty or NULL (Nothing in VB.Net).
		/// <para>-or-</para>
		/// <para>Thrown when the HID device factory plug-in type was not found.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the HID device factory plug-in requested is not an HID device factory.</para>
		/// </exception>
		public static GorgonHIDDeviceFactory CreateHIDDeviceFactory(string plugInType)
		{
			GorgonHIDPlugIn plugIn = null;
			GorgonHIDDeviceFactory factory = null;

			GorgonUtility.AssertParamString(plugInType, "plugInType");

			if (!GorgonPlugInFactory.PlugIns.Contains(plugInType))
				throw new ArgumentException("The plug-in '" + plugInType + "' was not found in any of the loaded plug-in assemblies.", "plugInType");

			plugIn = GorgonPlugInFactory.PlugIns[plugInType] as GorgonHIDPlugIn;

			if (plugIn == null)
				throw new ArgumentException("The plug-in '" + plugInType + "' is not an input plug-in.", "plugInType");

			factory = plugIn.CreateFactory();

			if (HIDDeviceFactories.Contains(factory.Name))
				return HIDDeviceFactories[factory.Name];
			else
				HIDDeviceFactories.Add(factory);

			return factory;
		}

		/// <summary>
		/// Function to destroy an HID device factory.
		/// </summary>
		/// <param name="hidDeviceFactory">HID device factory to destroy.</param>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="hidDeviceFactory"/> parameter is NULL (Nothing in VB.Net).
		/// <para>-or-</para>
		/// <para>Thrown when the HID device factory was not found.</para>
		/// </exception>
		public static void DestroyHIDDeviceFactory(GorgonHIDDeviceFactory hidDeviceFactory)
		{
			if (hidDeviceFactory == null)
				throw new ArgumentNullException("hidFactory");

			if (!HIDDeviceFactories.Contains(hidDeviceFactory))
				throw new ArgumentException("The HID device factory '" + hidDeviceFactory.Name + "' was not found.", "hidFactory");

			HIDDeviceFactories.Remove(hidDeviceFactory);
		}

		/// <summary>
		/// Function to destroy an HID device factory by its name.
		/// </summary>
		/// <param name="name">Name of the HID device factory to destroy.</param>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is empty or NULL (Nothing in VB.Net).
		/// <para>-or-</para>
		/// <para>Thrown when the HID device factory was not found.</para>
		/// </exception>
		public static void DestroyHIDDeviceFactory(string name)
		{
			GorgonUtility.AssertParamString(name, "plugInType");

			if (!HIDDeviceFactories.Contains(name))
				throw new ArgumentException("The HID device factory '" + name + "' was not found.", "hidFactory");

			HIDDeviceFactories.Remove(name);
		}

		/// <summary>
		/// Function to destroy an HID device factory by its index.
		/// </summary>
		/// <param name="index">Index of the HID device factory to destroy.</param>
		public static void DestroyHIDDeviceFactory(int index)
		{
			HIDDeviceFactories.Remove(index);
		}

		/// <summary>
		/// Function to destroy all the HID device factories.
		/// </summary>
		public static void DestroyAll()
		{
			HIDDeviceFactories.Clear();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes the <see cref="GorgonHIDFactory"/> class.
		/// </summary>
		static GorgonHIDFactory()
		{
			_hidFactories = new GorgonInputCollection();
		}
		#endregion
	}
}
