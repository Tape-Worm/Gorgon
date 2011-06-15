#region MIT.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
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
// Created: Saturday, September 30, 2006 10:04:18 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using GorgonLibrary;
using GorgonLibrary.PlugIns;

namespace GorgonLibrary.InputDevices
{
	/// <summary>
	/// Entry point for the plug-in.
	/// </summary>
    public class GorgonInputPlugInEntry
		: InputPlugIn
	{
		#region Variables.
		private static Input _input = null;					// Input interface object.
		private static object _syncLock = new object();		// Lock synchronization object.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return a description of the input interface.
		/// </summary>
		/// <value></value>
		public override string Description
		{
			get 
			{
				return "Gorgon raw input interface.";
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to unload this plug-in.
		/// </summary>
		protected override void Unload()
		{
			_input = null;
			base.Unload();
		}

		/// <summary>
        /// Function to create an input interface.
        /// </summary>
        /// <param name="parameters">Parameters to pass for construction.</param>
        /// <returns>New input object.</returns>
		protected override object CreateImplementation(object[] parameters)
		{
			if (_input == null)
			{
				// Lock the object in case of multiple threads.
				lock (_syncLock)
				{
					// If the object hasn't been created, create it.
					if (_input == null)
						_input = new GorgonInput(this);
				}
			}

			return _input;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="plugInPath">Path to the plug-in.</param>
		public GorgonInputPlugInEntry(string plugInPath)
			: base("Gorgon.RawInput", plugInPath)
		{
		}
		#endregion
	}
}
