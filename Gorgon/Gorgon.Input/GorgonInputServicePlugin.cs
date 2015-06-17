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
// Created: Sunday, June 26, 2011 1:57:01 PM
// 
#endregion

using System;
using System.Runtime.Remoting.Messaging;
using Gorgon.Plugins;

namespace Gorgon.Input
{
	/// <summary>
	/// Plugin interface for an input device factory plugin.
	/// </summary>
	/// <remarks>
	/// This plugin will create a single instance of a concrete <see cref="GorgonInputService"/>. If the <see cref="CreateInputService"/> is called multiple times then this single instance will be sent back 
	/// instead of a new instance.
	/// </remarks>
	public abstract class GorgonInputServicePlugin
		: GorgonPlugin
	{
		#region Variables.
		// The lazily created input service.
		private readonly Lazy<GorgonInputService> _inputService;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return whether the plugin supports game devices like game pads, or joysticks.
		/// </summary>
		public abstract bool SupportsGameDevices
		{
			get;
		}

		/// <summary>
		/// Property to return whether the plugin supports pointing devices like mice, trackballs, etc...
		/// </summary>
		public abstract bool SupportsPointingDevices
		{
			get;
		}

		/// <summary>
		/// Property to return whether the plugin supports keyboard devices.
		/// </summary>
		public abstract bool SupportsKeyboardDevices
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to create and return a <see cref="GorgonInputService"/>.
		/// </summary>
		/// <returns>The interface for the input factory.</returns>
		protected abstract GorgonInputService OnCreateInputService();

		/// <summary>
		/// Function to create and return an input service.
		/// </summary>
		/// <returns>The interface for the input factory.</returns>
		internal GorgonInputService CreateInputService()
		{
			return _inputService.Value;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonInputServicePlugin"/> class.
		/// </summary>
		/// <param name="description">Optional description of the plugin.</param>
		protected GorgonInputServicePlugin(string description)
			: base(description)
		{			
			_inputService = new Lazy<GorgonInputService>(OnCreateInputService);
		}
		#endregion		
	}
}
