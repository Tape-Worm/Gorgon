#region LGPL.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: Saturday, June 28, 2008 9:51:44 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using DX = SlimDX;
using D3D9 = SlimDX.Direct3D9;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Object representing a pixel shader.
	/// </summary>
	public class PixelShader
		: Shader
	{
		#region Variables.
		private bool _disposed = false;												// Flag to indicate that this object has been disposed.
		private D3D9.PixelShader _shader = null;									// Pixel shader.
		private ShaderFunction _function = null;									// Shader function containing the byte code.
		private string _fileName = string.Empty;									// Filename and path of the pixel shader.
		private bool _isResource = false;											// Flag to indicate that this object is an embedded resource.
		private SortedDictionary<string, D3D9.EffectHandle> _samplers = null;		// Texture samplers.
		private Image _lastImage = null;											// Last image being used.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return whether the shader has been compiled yet or not.
		/// </summary>
		/// <value></value>
		public override bool IsCompiled
		{
			get
			{
				return _function != null;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the parameters for a shader.
		/// </summary>
		protected override void GetParameters()
		{
			ShaderParameterType type;				// Parameter type.
			D3D9.EffectHandle handle = null;		// Parameter handle.
			D3D9.ConstantDescription desc;			// Constant description.			

			// Retrieve the samplers first.
			_samplers.Clear();
			Parameters.Clear();
			type = ShaderParameterType.Unknown;

			// Get parameters.
			for (int i = 0; i < _function.ByteCode.ConstantTable.Description.Constants; i++)
			{
				handle = _function.ByteCode.ConstantTable.GetConstant(null, i);

				if (handle != null)
				{
					desc = _function.ByteCode.ConstantTable.GetConstantDescription(handle);

					switch (desc.Type)
					{
						case SlimDX.Direct3D9.ParameterType.Bool:
							type = ShaderParameterType.Boolean;
							break;
						case SlimDX.Direct3D9.ParameterType.Float:							
							if (desc.Class == SlimDX.Direct3D9.ParameterClass.Vector)
								type = ShaderParameterType.Vector4D;
							else
								type = ShaderParameterType.Float;
							break;
						case SlimDX.Direct3D9.ParameterType.Int:
							type = ShaderParameterType.Integer;
							break;
						case SlimDX.Direct3D9.ParameterType.Sampler:
						case SlimDX.Direct3D9.ParameterType.Sampler2D:
							// Assign to the name.
							_samplers[desc.Name] = handle;
							type = ShaderParameterType.Image;
							break;
						default:
							type = ShaderParameterType.Unknown;
							break;
					}

					Parameters.Add(new ConstantShaderParameter(desc.Name, handle, _function, type));
				}
			}
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (!_disposed)
			{
				if (disposing)
				{
					if (_shader != null)
						_shader.Dispose();

					if (_function != null)
						_function.Dispose();
				}

				_function = null;
				_shader = null;
				_disposed = true;
			}
		}

		/// <summary>
		/// Function called before the rendering begins with this shader.
		/// </summary>
		protected override void OnRenderBegin()
		{
			if (_shader != null)
			{
				Gorgon.Screen.Device.PixelShader = _shader;
				_lastImage = Gorgon.Renderer.GetImage(0);				
				
				// Get parameters bound to the samplers.
				var parameters = from parameter in Parameters
								 join sampler in _samplers on parameter.Name equals sampler.Key
								select new {Parameter = parameter as ConstantShaderParameter, Handle = sampler.Value};

				// Bind textures.
				foreach(var param in parameters)
					param.Parameter.SetTextureSampler(param.Handle);
			}
		}

		/// <summary>
		/// Function called when rendering with this shader.
		/// </summary>
		protected override void OnRender()
		{
			Gorgon.Renderer.DrawCachedTriangles();
		}

		/// <summary>
		/// Function called after the rendering ends with this shader.
		/// </summary>
		protected override void OnRenderEnd()
		{
			Gorgon.Screen.Device.PixelShader = null;
			// Reset the texture stages.
			for (int i = 0; i < Gorgon.CurrentDriver.MaximumTextureStages; i++)
				Gorgon.Renderer.SetImage(i, null);

			Gorgon.Renderer.SetImage(0, _lastImage);
		}

		/// <summary>
		/// Function to bind this pixel shader to a specific function.
		/// </summary>
		/// <param name="function">Function to bind with.</param>
		public void SetFunction(ShaderFunction function)
		{
			if (_function != null)
				_function.Dispose();
			if (_shader != null)
				_shader.Dispose();
			_shader = null;
			_function = null;

			_function = function;
			_fileName = string.Empty;
			_isResource = false;
			IsBinary = false;
			ShaderSource = string.Empty;

			if (function == null)
				return;

			IsBinary = true;
			_function.Shader = this;
			_shader = new D3D9.PixelShader(Gorgon.Screen.Device, _function.ByteCode);
			GetParameters();
		}

		/// <summary>
		/// Function called when the device is in a lost state.
		/// </summary>
		public override void DeviceLost()
		{
			if (_shader != null)
				_shader.Dispose();
			_shader = null;
		}

		/// <summary>
		/// Function called when the device is reset.
		/// </summary>
		public override void DeviceReset()
		{
			if (_function != null)
				_shader = new D3D9.PixelShader(Gorgon.Screen.Device, _function.ByteCode);
		}

		/// <summary>
		/// Function to compile the shader source code.
		/// </summary>
		/// <param name="functionName">Name of the function to compile.</param>
		/// <param name="target">Pixel shader target.</param>
		/// <param name="flags">Options to use for compilation.</param>
		/// <remarks>See <see cref="GorgonLibrary.Graphics.ShaderCompileOptions"/> for more information about the compile time options.
		/// <para>The target parameter can be ps_2_0, ps_2_a, ps_2_b, or ps_3_0.</para>
		/// </remarks>
		public void CompileShader(string functionName, string target, ShaderCompileOptions flags)
		{
			string errors = string.Empty;							// Errors generated by shader compiler.
			string previousDir = string.Empty;						// Previous current directory.
			DX.DataStream data = null;								// Effect data.
			D3D9.EffectHandle functionHandle = null;				// Handle to the function.
			D3D9.EffectCompiler compiler = null;					// Effect compiler.
			D3D9.ShaderFlags d3dflags = (D3D9.ShaderFlags)flags;	// Shader flags.

			if (string.IsNullOrEmpty(functionName))
				throw new ArgumentNullException("functionName");

			if (string.IsNullOrEmpty(target))
				throw new ArgumentNullException("target");

			if (string.IsNullOrEmpty(ShaderSource))
				throw new ShaderCompilerErrorException(Name, "Shader has no source.");

			try
			{
				if ((!string.IsNullOrEmpty(_fileName)) && (File.Exists(_fileName)))
				{
					previousDir = Directory.GetCurrentDirectory();
					Directory.SetCurrentDirectory(Path.GetDirectoryName(Path.GetFullPath(_fileName)));
				}

				// Clean up the shader.
				if (_shader != null)
					_shader.Dispose();
				if (_function != null)
					_function.Dispose();
				_function = null;
				_shader = null;
								
				_isResource = false;
				IsBinary = false;

				compiler = new D3D9.EffectCompiler(ShaderSource, null, null, d3dflags, out errors);
				functionHandle = compiler.GetFunction(functionName);
				_function = new ShaderFunction(functionName, this, compiler.CompileShader(functionHandle, target, d3dflags), target);
				_shader = new D3D9.PixelShader(Gorgon.Screen.Device, _function.ByteCode);

				GetParameters();
			}
			catch (Exception ex)
			{
				if ((!string.IsNullOrEmpty(previousDir)) && (Directory.Exists(previousDir)))
					Directory.SetCurrentDirectory(previousDir);

				throw new ShaderCompilerErrorException(Name, errors, ex);
			}
			finally
			{
				if (compiler != null)
					compiler.Dispose();

				if (data != null)
					data.Dispose();

				data = null;
				compiler = null;
			}			
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="PixelShader"/> class.
		/// </summary>
		/// <param name="name">THe name of the pixel shader.</param>
		public PixelShader(string name)
			: base(name)
		{
			_samplers = new SortedDictionary<string, D3D9.EffectHandle>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PixelShader"/> class.
		/// </summary>
		/// <param name="name">THe name of the pixel shader.</param>
		/// <param name="function">Function that contains the pixel shader.</param>
		public PixelShader(string name, ShaderFunction function)
			: base(name)
		{
			SetFunction(function);
		}
		#endregion
	}
}
