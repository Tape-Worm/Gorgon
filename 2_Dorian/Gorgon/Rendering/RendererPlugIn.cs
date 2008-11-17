#region MIT.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: Sunday, November 16, 2008 7:28:28 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GorgonLibrary.PlugIns;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Base class for renderer plug-ins.
	/// </summary>
	public abstract class RendererPlugIn
		: PlugIn
	{
		#region Variables.
		private static Renderer _renderer = null;				// Renderer.
		private static object _sync = new object();				// Lock sync.
		#endregion

		#region Methods.
		/// <summary>
		/// Function to perform clean up when this plug-in is unloaded.
		/// </summary>
		protected internal override void Unload()
		{
			base.Unload();

			if (_renderer != null)
			{
				lock (_sync)
				{
					if (_renderer != null)
						_renderer = null;
				}
			}
		}

		/// <summary>
		/// Function to create 
		/// </summary>
		/// <returns></returns>
		internal Renderer Create()
		{
			if (_renderer == null)
			{
				lock (_sync)
				{
					if (_renderer == null)
					{
						_renderer = CreateImplementation(null) as Renderer;
						if (_renderer == null)
							throw new GorgonException(GorgonErrors.InvalidPlugin, "Error creating plug-in '" + Name + "'.");
					}
				}
			}

			return _renderer;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="RendererPlugIn"/> class.
		/// </summary>
		/// <param name="plugInPath">Path to the plug-in.</param>
		public RendererPlugIn(string plugInPath)
			: base(plugInPath)
		{
		}
		#endregion
	}
}
