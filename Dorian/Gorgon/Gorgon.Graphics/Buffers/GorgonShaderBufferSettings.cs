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
// Created: Sunday, May 12, 2013 11:50:20 PM
// 
#endregion

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Settings for a buffer.
	/// </summary>
	public interface IShaderBufferSettings
	{
		/// <summary>
		/// Property to set or return whether to allow unordered access to the buffer.
		/// </summary>
		/// <remarks>Unordered access views require a video device feature level of SM_5 or better.
		/// <para>The default value is FALSE.</para>
		/// </remarks>
		bool AllowUnorderedAccess
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether to allow this buffer to be used for stream output.
		/// </summary>
		/// <remarks>The default value is FALSE.</remarks>
		bool IsOutput
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether to allow writing from the CPU.
		/// </summary>
		/// <remarks>The default value is FALSE.</remarks>
		bool AllowCPUWrite
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the size of an element in a structured buffer.
		/// </summary>
		/// <remarks>This value is only applicable on SM_5 video devices and must be non-zero for a structured buffer.
		/// <para>The default value is 0.</para>
		/// </remarks>
		int ElementSize
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the number of an elements in a structured buffer or typed buffer.
		/// </summary>
		/// <remarks>This value is only applicable on SM_5 video devices if used with a structured buffer. The value must be non-zero for all buffer types.
		/// <para>The default value is 0.</para>
		/// </remarks>
		int ElementCount
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the format of the view used when binding a typed buffer to a shader.
		/// </summary>
		/// <remarks>This value must be set to Unknown for structured buffers.
		/// <para>The default value is Unknown.</para>
		/// </remarks>
		BufferFormat ShaderViewFormat
		{
			get;
			set;
		}
	}

	/// <summary>
	/// Settings for a typed buffer.
	/// </summary>
	public sealed class GorgonTypedBufferSettings
		: IShaderBufferSettings
	{
		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTypedBufferSettings"/> class.
		/// </summary>
		public GorgonTypedBufferSettings()
		{
			IsOutput = false;
			AllowCPUWrite = false;
			ElementCount = 0;
			ShaderViewFormat = BufferFormat.Unknown;
			AllowUnorderedAccess = false;
		}
		#endregion

		#region IShaderBufferSettings Members
		/// <summary>
		/// Property to set or return whether to allow unordered access to the buffer.
		/// </summary>
		/// <remarks>Unordered access views require a video device feature level of SM_5 or better.
		/// <para>The default value is FALSE.</para>
		/// </remarks>
		public bool AllowUnorderedAccess
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether to allow this buffer to be used for stream output.
		/// </summary>
		/// <remarks>The default value is FALSE.</remarks>
		public bool IsOutput
		{
			get;
			set;
		}


		/// <summary>
		/// Property to set or return whether to allow writing from the CPU.
		/// </summary>
		/// <remarks>The default value is FALSE.</remarks>
		public bool AllowCPUWrite
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the size of an element in a structured buffer.
		/// </summary>
		/// <remarks>
		/// This does not apply to typed buffers.
		/// <para>The default value is 0.</para>
		/// </remarks>
		int IShaderBufferSettings.ElementSize
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		/// <summary>
		/// Property to set or return the number of an elements in a structured buffer or typed buffer.
		/// </summary>
		/// <remarks>
		/// This value is only applicable on SM_5 video devices if used with a structured buffer. The value must be non-zero for all buffer types.
		/// <para>The default value is 0.</para>
		/// </remarks>
		public int ElementCount
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the format of the view used when binding a typed buffer to a shader.
		/// </summary>
		/// <remarks>If this value is set to unknown, then no default shader view will be created for the buffer.  If no shader view exists for this buffer, 
		/// then it will be unable to be bound to a shader.
		/// <para>The default value is Unknown.</para>
		/// </remarks>
		public BufferFormat ShaderViewFormat
		{
			get;
			set;
		}
		#endregion
	}

	/// <summary>
	/// Settings for a typed buffer.
	/// </summary>
	/// <remarks>Raw buffers are only available on SM5 video devices.</remarks>
	public sealed class GorgonRawBufferSettings
		: IShaderBufferSettings
	{
		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonRawBufferSettings"/> class.
		/// </summary>
		public GorgonRawBufferSettings()
		{
			IsOutput = false;
			AllowCPUWrite = false;
			ElementCount = 0;
			CreateUnderedAccessView = false;
			AllowUnorderedAccess = false;
		}
		#endregion

		#region IShaderBufferSettings Members
		/// <summary>
		/// Property to set or return whether to allow unordered access to the buffer.
		/// </summary>
		/// <remarks>Unordered access views require a video device feature level of SM_5 or better.
		/// <para>The default value is FALSE.</para>
		/// </remarks>
		public bool AllowUnorderedAccess
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether to allow this buffer to be used for stream output.
		/// </summary>
		/// <remarks>The default value is FALSE.</remarks>
		public bool IsOutput
		{
			get;
			set;
		}


		/// <summary>
		/// Property to set or return whether to allow writing from the CPU.
		/// </summary>
		/// <remarks>The default value is FALSE.</remarks>
		public bool AllowCPUWrite
		{
			get;
			set;
		}

		/// <summary>
		/// Propery to set or return whether to create a default unordered access view to the buffer.
		/// </summary>
		/// <remarks>The default value is FALSE.</remarks>
		public bool CreateUnderedAccessView
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the size of an element in a structured buffer.
		/// </summary>
		/// <remarks>
		/// This does not apply to typed buffers.
		/// <para>The default value is 0.</para>
		/// </remarks>
		int IShaderBufferSettings.ElementSize
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		/// <summary>
		/// Property to set or return the number of an elements in a structured buffer or typed buffer.
		/// </summary>
		/// <remarks>
		/// This value is only applicable on SM_5 video devices if used with a structured buffer. The value must be non-zero for all buffer types.
		/// <para>The default value is 0.</para>
		/// </remarks>
		public int ElementCount
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the format of the view used when binding a typed buffer to a shader.
		/// </summary>
		/// <remarks>This value will always return R32.</remarks>
		BufferFormat IShaderBufferSettings.ShaderViewFormat
		{
			get
			{
				return BufferFormat.R32;
			}
			set
			{
			}
		}
		#endregion
	}

	/// <summary>
	/// Settings for a structured buffer.
	/// </summary>
	/// <remarks>Structured buffers are only available on SM_5 video devices.</remarks>
	public sealed class GorgonStructuredBufferSettings
		: IShaderBufferSettings
	{
		#region Properties.
		/// <summary>
		/// Property to set or return the type of structured buffer.
		/// </summary>
		/// <remarks>The default value is standard.</remarks>
		public StructuredBufferType StructuredBufferType
		{
			get;
			set;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonStructuredBufferSettings"/> class.
		/// </summary>
		public GorgonStructuredBufferSettings()
		{
			StructuredBufferType = StructuredBufferType.Standard;
			IsOutput = false;
			AllowCPUWrite = false;
			AllowUnorderedAccess = false;
			ElementCount = 0;
			ElementSize = 0;
		}
		#endregion

		#region IShaderBufferSettings Members
		/// <summary>
		/// Property to set or return whether to allow unordered access to the buffer.
		/// </summary>
		/// <remarks>Unordered access views require a video device feature level of SM_5 or better.
		/// <para>The default value is FALSE.</para>
		/// </remarks>
		public bool AllowUnorderedAccess
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether to allow this buffer to be used for stream output.
		/// </summary>
		/// <remarks>The default value is FALSE.</remarks>
		public bool IsOutput
		{
			get;
			set;
		}


		/// <summary>
		/// Property to set or return whether to allow writing from the CPU.
		/// </summary>
		/// <remarks>The default value is FALSE.</remarks>
		public bool AllowCPUWrite
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the size of an element in a structured buffer.
		/// </summary>
		/// <remarks>
		/// <para>The default value is 0.</para>
		/// </remarks>
		public int ElementSize
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the number of an elements in a structured buffer or typed buffer.
		/// </summary>
		/// <remarks>
		/// This value is only applicable on SM_5 video devices if used with a structured buffer. The value must be non-zero for all buffer types.
		/// <para>The default value is 0.</para>
		/// </remarks>
		public int ElementCount
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the format of the view used when binding a typed buffer to a shader.
		/// </summary>
		/// <remarks>This value is not applicable to structured buffers.</remarks>
		BufferFormat IShaderBufferSettings.ShaderViewFormat
		{
			get;
			set;
		}
		#endregion
	}
}
