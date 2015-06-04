#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Wednesday, June 5, 2013 7:35:18 PM
// 
#endregion

using System;
using Gorgon.Core;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Settings for defining a render target buffer.
	/// </summary>
	public class GorgonRenderTargetBufferSettings
		: IBufferSettings, ICloneable<GorgonRenderTargetBufferSettings>
	{
		#region Properties.
		/// <summary>
		/// Property to return the type of render target.
		/// </summary>
		public RenderTargetType RenderTargetType
		{
			get
			{
				return RenderTargetType.Buffer;
			}
		}

		/// <summary>
		/// Property to set or return the format for the default render target view.
		/// </summary>
		/// <remarks>
		/// This value must not be Unknown, if it is set to Unknown an exception will be thrown.
		/// <para>The default is Unknown.</para>
		/// </remarks>
		public BufferFormat Format
		{
			get;
			set;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonRenderTargetBufferSettings"/> class.
		/// </summary>
		public GorgonRenderTargetBufferSettings()
		{
			SizeInBytes = 0;
			Format = BufferFormat.Unknown;
		}
		#endregion

		#region IBufferSettings Members
		/// <summary>
		/// Property to set or return the size of the buffer, in bytes.
		/// </summary>
		/// <remarks>The default value is 0.</remarks>
		public int SizeInBytes
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the usage for the buffer.
		/// </summary>
		/// <remarks>For render targets, this value is always Default.</remarks>
		/// <exception cref="System.NotSupportedException">Thrown when an attempt to set the value is made.</exception>
		BufferUsage IBufferSettings.Usage
		{
			get
			{
				return BufferUsage.Default;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Property to set or return whether to allow unordered access to the buffer.
		/// </summary>
		/// <remarks>Unordered access views require a video device with SM5 capabilities.
		/// <para>The default value is <b>false</b>.</para>
		/// </remarks>
		public bool AllowUnorderedAccessViews
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether to allow shader resource views for this buffer.
		/// </summary>
		/// <remarks>The default value is <b>false</b>.
		/// </remarks>
		public bool AllowShaderViews
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the format for the default shader view.
		/// </summary>
		/// <remarks>
		/// Setting this value to any other value than Unknown will create a default shader view for the buffer that will encompass the entire buffer with the specified format.
		/// <para>The default value is Unknown.</para>
		/// </remarks>
		public BufferFormat DefaultShaderViewFormat
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether a buffer will allow raw views.
		/// </summary>
		/// <remarks>
		/// This value must be set to <b>false</b> if <see cref="AllowShaderViews" /> or <see cref="AllowUnorderedAccessViews" /> is set to <b>false</b>.
		/// <para>The default value is <b>false</b>.</para>
		/// </remarks>
		public bool AllowRawViews
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether the buffer will be used as an indirect argument buffer.
		/// </summary>
		/// <exception cref="System.NotSupportedException">Thrown when an attempt to set the value is made.</exception>
		/// <remarks>This value does not apply to render target buffers and will always return <b>false</b>.</remarks>
		bool IBufferSettings.AllowIndirectArguments
		{
			get
			{
				return false;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Property to set or return whether to allow this buffer to be used for stream output.
		/// </summary>
		/// <exception cref="System.NotSupportedException">Thrown when an attempt to set the value is made.</exception>
		/// <remarks>This value does not apply to render target buffers and will always return <b>false</b>.</remarks>
		bool IBufferSettings.IsOutput
		{
			get
			{
				return false;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Property to set or return the size, in bytes, of an individual item in a structured buffer.
		/// </summary>
		/// <exception cref="System.NotSupportedException">Thrown when an attempt to set the value is made.</exception>
		/// <remarks>This value does not apply to render target buffers and will always return 0.
		/// </remarks>
		int IBufferSettings.StructureSize
		{
			get
			{
				return 0;
			}
			set
			{
				throw new NotSupportedException();
			}
		}
		#endregion

		#region ICloneable<GorgonRenderTargetBufferSettings> Members
		/// <summary>
		/// Function to return a clone of the settings.
		/// </summary>
		/// <returns>A clone of the settings.</returns>
		public GorgonRenderTargetBufferSettings Clone()
		{
			return new GorgonRenderTargetBufferSettings
				{
					SizeInBytes = SizeInBytes,
					AllowRawViews = AllowRawViews,
					AllowUnorderedAccessViews = AllowUnorderedAccessViews,
					AllowShaderViews = AllowShaderViews,
					DefaultShaderViewFormat = DefaultShaderViewFormat,
					Format = Format
				};
		}
		#endregion
	}
}