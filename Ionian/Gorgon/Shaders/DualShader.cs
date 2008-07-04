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
// Created: Friday, July 04, 2008 2:06:25 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Object representing a single interface for vertex and pixel shaders.
	/// </summary>
	public class DualShader
		: Shader
	{
		#region Variables.
		private PixelShader _pixelShader = null;			// Pixel shader to use.
		private VertexShader _vertexShader = null;			// Vertex shader to use.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return whether this shader is a binary (no source code) shader.
		/// </summary>
		public override bool IsBinary
		{
			get
			{
				bool result = false;

				if (VertexShader != null)
					result = VertexShader.IsBinary;
				if (PixelShader != null)
					result = (result) && (PixelShader.IsBinary);

				return result;
			}
			protected set
			{				
			}
		}

		/// <summary>
		/// Property to set or return the source code for this shader.
		/// </summary>
		public override string ShaderSource
		{
			get
			{
				StringBuilder result = new StringBuilder();

				if (VertexShader != null)
				{
					result.Append("// Vertex shader:");
					result.Append(VertexShader.ShaderSource);
				}

				if (PixelShader != null)
				{
					if (result.Length > 0)
						result.Append("\n\n");
					result.Append("// Pixel shader:");
					result.Append(PixelShader.ShaderSource);
				}

				return result;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Property to return whether the shader has been compiled yet or not.
		/// </summary>
		/// <value></value>
		public override bool IsCompiled
		{
			get
			{
				bool result = false;

				if (VertexShader != null)
					result = VertexShader.IsCompiled;
				if (PixelShader != null)
					result = (result) && (PixelShader.IsCompiled);

				return result;
			}
		}

		/// <summary>
		/// Property to set or return the pixel shader to use.
		/// </summary>
		public PixelShader PixelShader
		{
			get
			{
				return _pixelShader;
			}
			set
			{
				_pixelShader = value;

				if ((_pixelShader != null) && (!_pixelShader.IsCompiled))
					throw new ArgumentException("The pixel shader must be compiled.");
				GetParameters();
			}
		}

		/// <summary>
		/// Property to set or return the vertex shader to use.
		/// </summary>
		public VertexShader VertexShader
		{
			get
			{
				return _vertexShader;
			}
			set
			{
				_vertexShader = value;

				if ((_vertexShader != null) && (!_vertexShader.IsCompiled))
					throw new ArgumentException("The vertex shader must be compiled.");
				GetParameters();
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the parameters for a shader.
		/// </summary>
		protected override void GetParameters()
		{
			Parameters.Clear();

			if (VertexShader != null)
			{
				foreach (ConstantShaderParameter param in VertexShader.Parameters)
					Parameters.Add(param);
			}

			if (PixelShader != null)
			{
				foreach (ConstantShaderParameter param in PixelShader.Parameters)
					Parameters.Add(param);
			}
		}

		/// <summary>
		/// Function called before the rendering begins with this shader.
		/// </summary>
		protected override void OnRenderBegin()
		{
			if ((VertexShader == null) && (PixelShader == null))
				throw new ShaderNotValidException();

			if (VertexShader != null)
				((IShaderRenderer)VertexShader).Begin();
			if (PixelShader != null)
				((IShaderRenderer)PixelShader).Begin();
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
			if (VertexShader != null)
				((IShaderRenderer)VertexShader).End();
			if (PixelShader != null)
				((IShaderRenderer)PixelShader).End();
		}

		/// <summary>
		/// Function called when the device is in a lost state.
		/// </summary>
		public override void DeviceLost()
		{
			if (VertexShader != null)
				VertexShader.DeviceLost();
			if (PixelShader != null)
				PixelShader.DeviceLost();
		}

		/// <summary>
		/// Function called when the device is reset.
		/// </summary>
		public override void DeviceReset()
		{
			if (VertexShader != null)
				VertexShader.DeviceReset();
			if (PixelShader != null)
				PixelShader.DeviceReset();
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="DualShader"/> class.
		/// </summary>
		/// <param name="name">The name of the shader.</param>
		/// <param name="pixelShader">The pixel shader to use.</param>
		/// <param name="vertexShader">The vertex shader to use.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the name parameter is NULL or a zero length string.</exception>
		public DualShader(string name, PixelShader pixelShader, VertexShader vertexShader)
			: base(name)
		{
			PixelShader = pixelShader;
			VertexShader = vertexShader;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DualShader"/> class.
		/// </summary>
		/// <param name="name">Name of the shader..</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the name parameter is NULL or a zero length string.</exception>
		public DualShader(string name)
			: this(name, null, null)
		{
		}
		#endregion
	}
}
