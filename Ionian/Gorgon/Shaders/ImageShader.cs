#region LGPL.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
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
// Created: Friday, December 07, 2007 4:17:18 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using DX = SlimDX;
using D3D9 = SlimDX.Direct3D9;
using GorgonLibrary.Internal;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Object representing an image shader interface.
	/// </summary>
	public class ImageShader
		: NamedObject, IDisposable, IDeviceStateObject
	{
		#region Variables.
		private bool _disposed = false;						// Flag to indicate that the object is disposed.
		private D3D9.TextureShader _shader = null;			// Texture shader.
		private ShaderFunction _function = null;			// Shader function.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the D3D shader.
		/// </summary>
		internal D3D9.TextureShader D3DShader
		{
			get
			{				
				return _shader;
			}
		}

		/// <summary>
		/// Property to set or return whether this shader should destroy the function or leave it alone.
		/// </summary>
		public bool AutoDisposeFunction
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the function that this shader is using to fill the image.
		/// </summary>
		public ShaderFunction Function
		{
			get
			{
				return _function;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to set the default values.
		/// </summary>
		public void SetDefaultValues()
		{
			_shader.SetDefaults();
		}		
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="ImageShader"/> class.
		/// </summary>
		/// <param name="name">Name of the image shader.</param>
		/// <param name="function">Shader function to use to fill the image.</param>
		public ImageShader(string name, ShaderFunction function)
			: base(name)
		{
			AutoDisposeFunction = true;

			if (function == null)
				throw new ArgumentNullException("function");

			if (ImageShaderCache.ImageShaders.Contains(name))
				throw new ShaderAlreadyExistsException(name);

			_function = function;

			if (!_function.Target.StartsWith("tx_", StringComparison.CurrentCultureIgnoreCase))
				throw new ArgumentException("The function needs to have been compiled with a texture shader profile (tx_n_n).");

			_shader = new D3D9.TextureShader(function.ByteCode.Data);
			ImageShaderCache.ImageShaders.Add(this);
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (!_disposed)
				{
					if (_shader != null)
						_shader.Dispose();
					if ((_function != null) && (AutoDisposeFunction))
						_function.Dispose();
					_function = null;
					_shader = null;
					_disposed = true;

					// Remove us from the cache.
					if (ImageShaderCache.ImageShaders.Contains(Name))
						ImageShaderCache.ImageShaders.Remove(Name);
				}
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion

		#region IDeviceStateObject Members
		/// <summary>
		/// Function called when the device is in a lost state.
		/// </summary>
		public void DeviceLost()
		{
			if (_shader != null)
				_shader.Dispose();

			_shader = null;
		}

		/// <summary>
		/// Function called when the device is reset.
		/// </summary>
		public void DeviceReset()
		{
			if ((_function != null) && (_shader == null))
				_shader = new D3D9.TextureShader(_function.ByteCode.Data);
		}

		/// <summary>
		/// Function to force the loss of the objects data.
		/// </summary>
		public void ForceRelease()
		{
			DeviceLost();
		}
		#endregion
	}
}
