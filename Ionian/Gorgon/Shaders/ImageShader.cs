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
using SharpUtilities;
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
		private Shader _root = null;						// Root shader.
		private DX.DataStream _functionData = null;			// Stream to hold the function data.
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
		/// <param name="shader">The shader that holds the image shader function.</param>
		/// <param name="functionName">Name of the function to import.</param>
		public ImageShader(string name, Shader shader, string functionName)
			: base(name)
		{
			D3D9.ShaderBytecode byteCode = null;		// Shader byte code.

			if (shader == null)
				throw new ArgumentNullException("shader");

			if (string.IsNullOrEmpty(functionName))
				throw new ArgumentNullException("functionName");

			if (ImageShaderCache.ImageShaders.Contains(name))
				throw new ShaderAlreadyExistsException(name);

			try
			{
				_root = shader;
				byteCode = shader.GetShaderFunction(functionName);
                _functionData = byteCode.Data;
				_shader = new D3D9.TextureShader(_functionData);

				ImageShaderCache.ImageShaders.Add(this);
			}
			finally
			{
				if (byteCode != null)
					byteCode.Dispose();
				byteCode = null;
			}
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
					if (_functionData != null)
						_functionData.Dispose();
					_functionData = null;
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
			if ((_functionData != null) && (_shader == null))
				_shader = new D3D9.TextureShader(_functionData);
		}

		/// <summary>
		/// Function to force the loss of the objects data.
		/// </summary>
		public void ForceRelease()
		{
			Dispose();
		}
		#endregion
	}
}
