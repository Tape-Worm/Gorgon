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
using Gorgon.Diagnostics;
using Gorgon.Plugins;

namespace Gorgon.Input
{
	/// <summary>
	/// Plugin interface for an input device factory plugin.
	/// </summary>
	/// <remarks>
	/// This plugin will create a single instance of a <see cref="GorgonInputService2"/>.
	/// </remarks>
	public abstract class GorgonInputServicePlugin
		: GorgonPlugin
	{
		#region Variables.
		// The lazily created input service.
		private readonly Lazy<GorgonInputService2> _inputService2;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return whether the plugin supports game devices like game pads, or joysticks.
		/// </summary>
		public abstract bool SupportsJoysticks
		{
			get;
		}

		/// <summary>
		/// Property to return whether the plugin supports pointing devices like mice, trackballs, etc...
		/// </summary>
		public abstract bool SupportsMice
		{
			get;
		}

		/// <summary>
		/// Property to return whether the plugin supports keyboards.
		/// </summary>
		public abstract bool SupportsKeyboards
		{
			get;
		}

		/// <summary>
		/// Property to return the log file to use with the plug in.
		/// </summary>
		public IGorgonLog Log
		{
			get;
			internal set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to create and return a <see cref="GorgonInputService"/>.
		/// </summary>
		/// <returns>The interface for the input factory.</returns>
		[Obsolete("This is not used anymore.  Get rid of it when refactor is complete.")]
		protected GorgonInputService OnCreateInputService()
		{
			throw new Exception("This is deprecated.  Please remove it.");
		}

		/// <summary>
		/// Function to create and return an input service.
		/// </summary>
		/// <returns>The interface for the input factory.</returns>
		[Obsolete("This is not used anymore.  Get rid of it when refactor is complete.")]
		internal GorgonInputService CreateInputService()
		{
			return null;
		}

		/// <summary>
		/// Function to create and return a <see cref="GorgonInputService"/>.
		/// </summary>
		/// <param name="log">The logging interface to use for debug logging.</param>
		/// <returns>The interface for the input factory.</returns>
		protected abstract GorgonInputService2 OnCreateInputService2(IGorgonLog log);

		/// <summary>
		/// Function to create and return an input service.
		/// </summary>
		/// <returns>The interface for the input factory.</returns>
		internal GorgonInputService2 CreateInputService2()
		{
			return _inputService2.Value;
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
			Log = new GorgonLogDummy();
			_inputService2 = new Lazy<GorgonInputService2>(() => OnCreateInputService2(Log));
		}
		#endregion		
	}
}
