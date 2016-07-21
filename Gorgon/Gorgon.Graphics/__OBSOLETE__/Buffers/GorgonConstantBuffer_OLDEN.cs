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
using System.ComponentModel;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Properties;
using Gorgon.Native;
using DX = SharpDX;

namespace Gorgon.Graphics
{
	/// <summary>
	/// A constant buffer for shaders.
	/// </summary>
	/// <remarks>Constant buffers are used to send groups of scalar values to a shader.  The buffer is just a block of allocated memory that is written to by one of the various Write methods.
	/// <para>Typically, the user will define a value type that matches a constant buffer layout.  Then, if the value type uses nothing but blittable types, the user can then write the entire 
	/// value type structure to the constant buffer.  If the value type contains more complex types, such as arrays, then the user can write each item in the value type to a variable in the constant 
	/// buffer.  Please note that the names for the variables in the value type and the shader do -not- have to match, although, for the sake of clarity, it is a good idea that they do.</para>
	/// <para>Constant buffers follow very specific rules, which are explained at http://msdn.microsoft.com/en-us/library/windows/desktop/bb509632(v=vs.85).aspx </para>
	/// <para>When passing a value type to the constant buffer, ensure that the type has a System.Runtime.InteropServices.StructLayout attribute assigned to it, and that the layout is explicit or sequential.  
	/// Also, the size of the value type must be a multiple of 16, so padding of the value type might be necessary.</para>
	/// </remarks>
	public sealed class GorgonConstantBuffer_OLDEN
		: GorgonBaseBuffer
	{
		#region Properties.
        /// <summary>
        /// Property to return the type of buffer.
        /// </summary>
        public override BufferType BufferType => BufferType.Constant;

		/// <summary>
		/// Property to return the settings.
		/// </summary>
		public new GorgonConstantBufferSettings Settings => (GorgonConstantBufferSettings)base.Settings;

		/// <summary>
        /// Property to return the default shader view for this buffer.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
	    public override GorgonShaderView DefaultShaderView => null;

		#endregion

		#region Methods.
		/// <summary>
		/// Function to clean up the resource object.
		/// </summary>
		protected override void CleanUpResource()
		{
			//Graphics.Shaders.UnbindConstantBuffer(this);
			base.CleanUpResource();
		}

        /// <summary>
        /// Function to perform the creation of a default shader view.
        /// </summary>
        /// <returns>Nothing.</returns>
        protected override void OnCreateDefaultShaderView()
        {
            // Do nothing here.  Constant buffers don't support shader resource views.
        }

		/// <summary>
        /// Function to update the buffer.
        /// </summary>
        /// <param name="data">Pointer to the data used to update the buffer.</param>
        /// <param name="offset">Offset, in bytes, into the buffer to start writing at.</param>
        /// <param name="size">The number of bytes to write.</param>
        /// <param name="context">A graphics context to use when updating the buffer.</param>
        /// <remarks>
        /// Use the <paramref name="context" /> parameter to determine the context in which the buffer should be updated. This is necessary to use that context
        /// to update the buffer because 2 threads may not access the same resource at the same time.
        /// </remarks>
		protected override void OnUpdate(GorgonPointerBase data, int offset, int size, GorgonGraphics context)
		{
			context.D3DDeviceContext.UpdateSubresource(
				new DX.DataBox
				{
					DataPointer = new IntPtr(data.Address),
					RowPitch = 0,
					SlicePitch = 0
				}, 
				D3DResource);
		}

		/// <summary>
		/// Function to write an array of value types to the buffer.
		/// </summary>
		/// <typeparam name="T">Type of value type.</typeparam>
		/// <param name="data">Value type data to write into the buffer.</param>
		/// <param name="deferred">[Optional] A deferred context to use when updating the buffer.</param>
		/// <remarks>
		/// This overload is useful for directly copying values into the buffer without needing a data stream.  If the type of value is a
		/// struct and contains reference types (arrays, strings, and objects), then these members will not be copied.  Some form of
		/// marshalling will be required in order to copy structures with reference types.
		/// <para>
		/// If the <paramref name="deferred" /> parameter is NULL (<i>Nothing</i> in VB.Net), the immediate context will be used to update the buffer.  If it is non-NULL, then it
		/// will use the specified deferred context to clear the render target.
		/// <para>If you are using a deferred context, it is necessary to use that context to update the buffer because 2 threads may not access the same resource at the same time.
		/// Passing a separate deferred context will alleviate that.</para>
		/// </para>
		/// <para>This will only work on buffers created with a usage type of [Default].</para>
		/// </remarks>
		public override void Update<T>(T[] data, GorgonGraphics deferred = null)
		{
			data.ValidateObject("data");

#if DEBUG
			if (Settings.Usage != BufferUsage.Default)
			{
				throw new GorgonException(GorgonResult.AccessDenied, Resources.GORGFX_NOT_DEFAULT_USAGE);
			}
#endif

#warning FAILURE: This will not run.
			if (deferred == null)
			{
				//deferred = Graphics;
			}

			deferred.D3DDeviceContext.UpdateSubresource(data, D3DResource, 0, DirectAccess.SizeOf<T>() * data.Length);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonConstantBuffer_OLDEN"/> class.
		/// </summary>
		/// <param name="graphics">Graphics interface that owns this buffer.</param>
		/// <param name="name">Name of the constant buffer.</param>
		/// <param name="settings">Settings for the buffer.</param>
		internal GorgonConstantBuffer_OLDEN(GorgonGraphics graphics, string name, IBufferSettings settings)
			: base(graphics, name, settings)
		{
		}
		#endregion
	}
}
