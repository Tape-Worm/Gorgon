﻿#region LGPL.
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
		: BaseShader<ImageShader>
	{
		#region Variables.
		private D3D9.TextureShader _shader = null;			// Texture shader.
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
		/// Function to retrieve the shader target profile.
		/// </summary>
		/// <param name="target">Version of the profile.</param>
		/// <returns>The shader target profile.</returns>
		/// <remarks>For a texture shader this is always tx_1_0.</remarks>
		protected override string ShaderProfile(Version target)
		{
			return "tx_1_0";
		}

		/// <summary>
		/// Function called before the rendering begins with this shader.
		/// </summary>
		protected override void OnRenderBegin()
		{
			throw new ShaderNotValidException();
		}

		/// <summary>
		/// Function called when rendering with this shader.
		/// </summary>
		protected override void OnRender()
		{
			throw new ShaderNotValidException();
		}

		/// <summary>
		/// Function called after the rendering ends with this shader.
		/// </summary>
		protected override void OnRenderEnd()
		{
			throw new ShaderNotValidException();
		}

		/// <summary>
		/// Function called to create the actual shader object.
		/// </summary>
		protected override void CreateShader()
		{
			if (Function != null)
				_shader = new D3D9.TextureShader(Function.ByteCode.Data);
		}

		/// <summary>
		/// Function called to destroy the shader object.
		/// </summary>
		protected override void DestroyShader()
		{
			if (_shader != null)
				_shader.Dispose();

			_shader = null;
		}

		/// <summary>
		/// Function to compile the shader source code.
		/// </summary>
		/// <param name="functionName">Name of the function to compile.</param>
		/// <param name="flags">Options to use for compilation.</param>
		/// <remarks>See <see cref="GorgonLibrary.Graphics.ShaderCompileOptions"/> for more information about the compile time options.
		/// </remarks>
		public void CompileShader(string functionName, ShaderCompileOptions flags)
		{
			CompileShaderImplementation(functionName, new Version(1,0), flags);
		}

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
		public ImageShader(string name)
			: base(name, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ImageShader"/> class.
		/// </summary>
		/// <param name="name">Name for this object.</param>
		/// <param name="function">Function to bind to the shader as an entry point.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the name parameter is NULL or a zero length string.</exception>
		public ImageShader(string name, ShaderFunction function)
			: base(name, function)
		{
			if (function == null)
				throw new ArgumentNullException("function");
		}
		#endregion
	}
}
