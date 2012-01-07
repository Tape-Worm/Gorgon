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
// Created: Thursday, December 15, 2011 1:44:18 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using D3D = SharpDX.Direct3D11;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Native;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A constant buffer for shaders.
	/// </summary>
	/// <remarks>Constant buffers are used to send groups of scalar values to a shader.  The buffer is just a block of allocated memory that is written to by one of the various Write methods.
	/// <para>Typically, the user will define a value type that matches a constant buffer layout.  Then, if the value type uses nothing but blittable types, the user can then write the entire 
	/// value type structure to the constant buffer.  If the value type contains more complex types, such as arrays, then the user can write each item in the value type to a variable in the constant 
	/// buffer.  Please note that the names for the variables in the value type and the shader do -not- have to match, although, for the sake of clarity, it is a good idea that they do.</para>
	/// <para>In order to write to a constant buffer, the user must <see cref="GorgonLibrary.Graphics.GorgonConstantBuffer.Lock">lock</see> the buffer beforehand, and unlock it when done.  Failure to do so will result in an exception.</para>
	/// <para>Constant buffers follow very specific rules, which are explained at http://msdn.microsoft.com/en-us/library/windows/desktop/bb509632(v=vs.85).aspx </para>
	/// <para>When passing a value type to the constant buffer, ensure that the type has a System.Runtime.InteropServices.StructLayout attribute assigned to it, and that the layout is explicit.  Also, the size of the 
	/// value type must be a multiple of 16, so padding variables may be required.</para>
	/// </remarks>
	public class GorgonConstantBuffer
		: IDisposable
	{
		#region Variables.
		private bool _disposed = false;							// Flag to indicate that the buffer was disposed.
		private SharpDX.DataStream _data = null;				// Stream for writing.
		private IDictionary<string, int> _offsets = null;		// List of member offsets.
		private bool _locked = false;							// Flag to indicate that the buffer was locked for writing.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the Direct3D buffer.
		/// </summary>
		internal D3D.Buffer D3DBuffer
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the type of data used for the constant buffer.
		/// </summary>
		public Type DataType
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the size of buffer, in bytes.
		/// </summary>
		public int Size
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the graphics interface that owns this object.
		/// </summary>
		public GorgonGraphics Graphics
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return whether to allow CPU write access.
		/// </summary>
		public bool AllowCPUWrite
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to initialize the buffer.
		/// </summary>
		/// <param name="value">Value used to initialize the buffer.</param>
		/// <typeparam name="T">Type of data to read/write.</typeparam>
		internal void Initialize<T>(T? value)
			where T : struct
		{
			D3D.ResourceUsage usage = D3D.ResourceUsage.Default;
			D3D.CpuAccessFlags cpuFlags = D3D.CpuAccessFlags.None;

			if (D3DBuffer != null)
				D3DBuffer.Dispose();

			if (AllowCPUWrite)
			{
				usage = D3D.ResourceUsage.Dynamic;
				cpuFlags = D3D.CpuAccessFlags.Write;
			}

			_data = new SharpDX.DataStream(Size, true, true);
			if (value.HasValue)
			{
				_data.Write<T>(value.Value);
				D3DBuffer = new D3D.Buffer(Graphics.VideoDevice.D3DDevice, _data, new D3D.BufferDescription()
				{
					BindFlags = D3D.BindFlags.ConstantBuffer,
					CpuAccessFlags = cpuFlags,
					OptionFlags = D3D.ResourceOptionFlags.None,
					SizeInBytes = Size,
					StructureByteStride = 0,
					Usage = usage
				});
			}
			else
			{
				D3DBuffer = new D3D.Buffer(Graphics.VideoDevice.D3DDevice, new D3D.BufferDescription()
				{
					BindFlags = D3D.BindFlags.ConstantBuffer,
					CpuAccessFlags = cpuFlags,
					OptionFlags = D3D.ResourceOptionFlags.None,
					SizeInBytes = Size,
					StructureByteStride = 0,
					Usage = usage
				});
			}

			D3DBuffer.DebugName = "Gorgon Constant Buffer '" + DataType.FullName + "'";
		}

		/// <summary>
		/// Function to lock the buffer for writing.
		/// </summary>
		public void Lock()
		{
			if (!AllowCPUWrite)
			{
				_locked = true;
				return;
			}

			if (_data != null)
			{
				_data.Dispose();
				_data = null;
			}
			
			Graphics.Context.MapSubresource(D3DBuffer, D3D.MapMode.WriteDiscard, D3D.MapFlags.DoNotWait, out _data);
			_locked = true;
		}

		/// <summary>
		/// Function unlock the buffer after writing is complete.
		/// </summary>
		public void Unlock()
		{
			if (!_locked)
				return;

			if (!AllowCPUWrite)
			{
				Graphics.Context.UpdateSubresource(new SharpDX.DataBox(_data.DataPointer, 0, 0), D3DBuffer, 0);
				_locked = false;
				return;
			}

			_data.Dispose();
			_data = null;
			Graphics.Context.UnmapSubresource(D3DBuffer, 0);
			_locked = false;
		}

		/// <summary>
		/// Function to retrieve data about the type used for the constant buffer.
		/// </summary>
		private void GetTypeData()
		{
			if (!DataType.IsExplicitLayout)
				throw new GorgonException(GorgonResult.CannotCreate, "Cannot use the type '" + DataType.FullName + "'.  The type must have a System.RuntimeInteropServices.StructLayout attribute, and must use an explicit layout");

			Size = DataType.StructLayoutAttribute.Size;
			if (Size <= 0)
				Size = Marshal.SizeOf(DataType);

			if (((Size % 16) != 0) || (Size == 0))
				throw new GorgonException(GorgonResult.CannotCreate, "Cannot use the type '" + DataType.FullName + "'.  The size of the type (" + Size.ToString() + " bytes) is not on a 16 byte boundary or is 0.");

			// Get members.
			_offsets = (from member in DataType.GetMembers()						  
						  let memberAttrib = member.GetCustomAttributes(typeof(FieldOffsetAttribute), false) as IList<FieldOffsetAttribute>
						  where ((memberAttrib != null) && (memberAttrib.Count > 0) && ((member.MemberType == System.Reflection.MemberTypes.Field) || (member.MemberType == System.Reflection.MemberTypes.Property)))
						  select new { member.Name, memberAttrib[0].Value }).ToDictionary(item => item.Name, item => item.Value);			
		}
			

		/// <summary>
		/// Function to write a value type to the entire buffer.
		/// </summary>
		/// <typeparam name="T">Type of value to write.</typeparam>
		/// <param name="item">Value to write to the buffer.</param>
		/// <remarks>
		/// Unlike the other write methods, this writes the values in a value type directly to the buffer instead of writing to a specific member within the buffer.  Use this 
		/// method if the types within the value type are blittable (i.e. don't require marshalling).
		/// <para>The <paramref name="item"/> parameter is not bounds checked, and can exceed the size of the constant buffer, please use caution when writing data.</para>
		/// </remarks>
		/// <exception cref="System.InvalidOperationException">Thrown when the buffer hasn't been locked before writing.</exception>
		public void Write<T>(T item)
			where T : struct
		{
			if (!_locked)
				throw new InvalidOperationException("Cannot write to an unlocked buffer.");

			_data.Position = 0;
			_data.Write<T>(item);
		}
		
		/// <summary>
		/// Function to write an item to a variable within the buffer.
		/// </summary>
		/// <param name="member">Name of the variable to write.</param>
		/// <param name="item">Item to write.</param>
		/// <typeparam name="T">Type of data to read/write.</typeparam>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="member"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the member parameter is an empty string.</exception>
		/// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when the member name was not found in the constant buffer.</exception>
		/// <exception cref="System.InvalidOperationException">Thrown when the buffer hasn't been locked before writing.</exception>
		/// <remarks>This method will only write to a specific variable member in the constant buffer.</remarks>
		public void Write<T>(string member, T item)
			where T : struct
		{
			GorgonDebug.AssertParamString(member, "member");

			if (!_offsets.ContainsKey(member))
				throw new KeyNotFoundException("The variable '" + member + "' does not exist within this constant buffer.");

			if (!_locked)
				throw new InvalidOperationException("Cannot write to an unlocked buffer.");

			_data.Position = _offsets[member];
			_data.Write<T>(item);
		}

		/// <summary>
		/// Function to write an array of items to a variable within the buffer.
		/// </summary>
		/// <param name="member">Name of the variable to write.</param>
		/// <param name="item">Item to write.</param>
		/// <typeparam name="T">Type of data to read/write.</typeparam>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="member"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the member parameter is an empty string.</exception>
		/// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when the member name was not found in the constant buffer.</exception>
		/// <exception cref="System.InvalidOperationException">Thrown when the buffer hasn't been locked before writing.</exception>
		/// <remarks>This method will only write to a specific variable member in the constant buffer.  Please note that bounds checking is not enabled on this, and consequently the array may exceed the length of the buffer.
		/// <para>Buffers must be locked with the <see cref="M:GorgonLibrary.Graphics.GorgonConstantBuffer.Lock">Lock</see> method before writing.</para>
		/// </remarks>
		public void WriteArray<T>(string member, T[] item)
			where T : struct
		{
			if ((item == null) || (item.Length == 0))
				return;

			GorgonDebug.AssertParamString(member, "member");

			if (!_offsets.ContainsKey(member))
				throw new KeyNotFoundException("The variable '" + member + "' does not exist within this constant buffer.");

			if (!_locked)
				throw new InvalidOperationException("Cannot write to an unlocked buffer.");

			int offset = _offsets[member];
			int itemSize = DirectAccess.SizeOf<T>();

			for (int i = 0; i < item.Length; i++)
			{
				_data.Position = offset + (itemSize * i);
				_data.Write<T>(item[i]);
			}
		}

		/// <summary>
		/// Function to write a value to a variable within the buffer using marshalling.
		/// </summary>
		/// <param name="member">Name of the variable to write.</param>
		/// <param name="value">Value to marhsal and write.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="member"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the member parameter is an empty string.</exception>
		/// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when the member name was not found in the constant buffer.</exception>
		/// <exception cref="System.InvalidOperationException">Thrown when the buffer hasn't been locked before writing.</exception>
		/// <remarks>This method will only write to a specific variable member in the constant buffer.  Please note that bounds checking is not enabled on this, and consequently the array may exceed the length of the buffer.
		/// <para>This method should be used only when marshalling of a value is required.  This method is quite slow and should be used sparingly.</para>
		/// <para>Buffers must be locked with the <see cref="M:GorgonLibrary.Graphics.GorgonConstantBuffer.Lock">Lock</see> method before writing.</para>
		/// </remarks>
		public void WriteMarshalled(string member, object value)
		{
			GorgonDebug.AssertParamString(member, "member");

			if (!_offsets.ContainsKey(member))
				throw new KeyNotFoundException("The variable '" + member + "' does not exist within this constant buffer.");

			if (!_locked)
				throw new InvalidOperationException("Cannot write to an unlocked buffer.");

			_data.Position = _offsets[member];
			Marshal.StructureToPtr(value, _data.PositionPointer, false);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonConstantBuffer"/> class.
		/// </summary>
		/// <param name="graphics">Graphics interface that owns this buffer.</param>
		/// <param name="allowCPUWrite">TRUE to allow the CPU write access to the buffer, FALSE to disallow.</param>
		/// <param name="type">Type of data to write to the constant buffer.</param>
		internal GorgonConstantBuffer(GorgonGraphics graphics, bool allowCPUWrite, Type type)
		{
			Graphics = graphics;
			AllowCPUWrite = allowCPUWrite;
			DataType = type;
			GetTypeData();
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					if (_locked)
						_locked = false;

					if (_data != null)
						_data.Dispose();

					if (D3DBuffer != null)
						D3DBuffer.Dispose();					
				}

				D3DBuffer = null;
				_disposed = true;
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
	}
}
