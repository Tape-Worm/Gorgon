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
// Created: Saturday, November 15, 2008 12:33:31 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using DX = SlimDX;
using D3D = SlimDX.Direct3D9;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.SlimDXRenderer
{
	/// <summary>
	/// Renderer for SlimDX.
	/// </summary>
	public class SlimDXRenderer
		: Renderer
	{
		#region Variables.
		private bool _disposed = false;					// Flag to indicate that the object was disposed.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return whether the D3D runtime is in debug mode or not.
		/// </summary>
		/// <returns>TRUE if in debug, FALSE if in retail.</returns>
		private bool IsD3DDebug
		{
			get
			{
				RegistryKey regKey = null;		// Registry key.
				int keyValue = 0;				// Registry key value.

				try
				{
					// Get the registry setting.
					regKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Direct3D");

					if (regKey == null)
						return false;

					// Get value.
					keyValue = Convert.ToInt32(regKey.GetValue("LoadDebugRuntime", (int)0));
					if (keyValue != 0)
						return true;
				}
				finally
				{
					if (regKey != null)
						regKey.Close();
					regKey = null;
				}

				return false;
			}
		}

		/// <summary>
		/// Property to return whether to use the Ex interfaces or not.
		/// </summary>
		internal bool HasEXInterfaces
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return a Direct 3D interface.
		/// </summary>
		internal D3D.Direct3D Direct3D
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return a Direct 3DEx interface.
		/// </summary>
		internal D3D.Direct3DEx Direct3DEx
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to check whether the system supports the Ex interfaces for Direct 3D 9.
		/// </summary>
		private void CheckD3D9Ex()
		{
			IntPtr d3d9Handle = IntPtr.Zero;			// Handle to D3D9.DLL.
			UIntPtr exFunction = UIntPtr.Zero;			// Handle to Direct3DCreate9Ex function.

			try
			{
				HasEXInterfaces = false;
				d3d9Handle = Win32.LoadLibrary("D3D9.DLL");
				if (d3d9Handle != IntPtr.Zero)
				{
					exFunction = Win32.GetProcAddress(d3d9Handle, "Direct3DCreate9Ex");
					if (exFunction != UIntPtr.Zero)
						HasEXInterfaces = true;
				}
			}
			catch
			{
				HasEXInterfaces = false;
			}
			finally
			{
				if (d3d9Handle != IntPtr.Zero)
					Win32.FreeLibrary(d3d9Handle);
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="SlimDXRenderer"/> class.
		/// </summary>
		/// <param name="plugIn">The plug in used to create the renderer.</param>
		public SlimDXRenderer(SlimDXRendererPlugIn plugIn)
			: base(plugIn)
		{		
			if (IsD3DDebug)
				Gorgon.Log.Print("Renderer", "[*WARNING*] The Direct 3D runtime is currently set to DEBUG mode.  Performance will be hindered. [*WARNING*]", LoggingLevel.Verbose);

			CheckD3D9Ex();

			if (HasEXInterfaces)
			{
				Direct3DEx = new D3D.Direct3DEx();
				Direct3D = Direct3DEx as D3D.Direct3D;
			}
			else
				Direct3D = new D3D.Direct3D();
		}
		#endregion
	
		#region IDisposable Members
		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		/// <param name="disposing">TRUE to remove all objects, FALSE to remove only unmanaged.</param>
		protected override void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					if (Direct3D != null)
						Direct3D.Dispose();

					Direct3D = null;
					Direct3DEx = null;
				}

				_disposed = true;
			}
		}
		#endregion
	}
}
