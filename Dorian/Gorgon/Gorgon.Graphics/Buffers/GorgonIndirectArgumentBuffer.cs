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
// Created: Tuesday, May 28, 2013 8:45:12 AM
// 
#endregion

using D3D = SharpDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
    /// <summary>
    /// Buffer to hold arguments for indirect drawing.
    /// </summary>
    /// <remarks>Some shaders can output data which requires a CPU read to retrieve the count. This will cause a stall in the pipeline.  This buffer type 
    /// mitigates this issue by holding data output by a shader so that it may be passed back without having to perform a read.
    /// <para>This buffer type is meant to be used to be used with the <see cref="GorgonLibrary.Graphics.GorgonOutputMerger.DrawInstancedIndirect">DrawInstancedIndirect</see> method.</para>
    /// <para>This buffer type is only available to video devices that are SM_5 or better.</para>
    /// </remarks>
    public class GorgonIndirectArgumentBuffer
    {
        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonIndirectArgumentBuffer"/> class.
        /// </summary>
        /// <param name="graphics">The graphics interface that owns this object.</param>
        public GorgonIndirectArgumentBuffer(GorgonGraphics graphics)
        {
			/*
			   A resource cannot created with both D3D11_RESOURCE_MISC_DRAWINDIRECT_ARGS and D3D11_RESOURCE_MISC_BUFFER_STRUCTURED. [ STATE_CREATION ERROR #68: CREATEBUFFER_INVALIDMISCFLAGS]
			   When creating a buffer with the MiscFlag D3D11_RESOURCE_MISC_BUFFER_STRUCTURED specified, the StructureByteStride must be greater than zero, no greater than 2048, and a multiple of 4. [ STATE_CREATION ERROR #2097339: CREATEBUFFER_INVALIDSTRUCTURESTRIDE]
			   Buffers cannot be created with both D3D11_RESOURCE_MISC_BUFFER_ALLOW_RAW_VIEWS and D3D11_RESOURCE_MISC_BUFFER_STRUCTURED. [ STATE_CREATION ERROR #68: CREATEBUFFER_INVALIDMISCFLAGS]
			   Buffers created with D3D11_RESOURCE_MISC_BUFFER_STRUCTURED cannot specify any of the following listed bind flags.  The following BindFlags bits (0x81) are set: D3D11_BIND_VERTEX_BUFFER (1), D3D11_BIND_INDEX_BUFFER (0), D3D11_BIND_CONSTANT_BUFFER (0), D3D11_BIND_STREAM_OUTPUT (0), D3D11_BIND_RENDER_TARGET (0), or D3D11_BIND_DEPTH_STENCIL (0). [ STATE_CREATION ERROR #68: CREATEBUFFER_INVALIDMISCFLAGS]
			   Buffers created with D3D11_RESOURCE_MISC_BUFFER_ALLOW_RAW_VIEWS must specify at least one of the D3D11_BIND_SHADER_RESOURCE and D3D11_BIND_UNORDERED_ACCESS bind flags. [ STATE_CREATION ERROR #68: CREATEBUFFER_INVALIDMISCFLAGS]
			   A D3D11_USAGE_DYNAMIC Resource cannot be bound to certain parts of the graphics pipeline, but has at least one invalid BindFlags bit set. The BindFlags bits (0x19) have the following settings: D3D11_BIND_STREAM_OUTPUT (1), D3D11_BIND_RENDER_TARGET (0), D3D11_BIND_DEPTH_STENCIL (0), D3D11_BIND_UNORDERED_ACCESS (0). [ STATE_CREATION ERROR #64: CREATEBUFFER_INVALIDBINDFLAGS]
			   A D3D11_USAGE_DYNAMIC Resource may have only the D3D11_CPU_ACCESS_WRITE CPUAccessFlags set. [ STATE_CREATION ERROR #63: CREATEBUFFER_INVALIDCPUACCESSFLAGS]
			   D3D11_BIND_UNORDERED_ACCESS cannot be combined with the following BindFlags: D3D11_BIND_CONSTANT_BUFFER (0), D3D11_BIND_DEPTH_STENCIL (0), or D3D11_BIND_STREAM_OUTPUT (1). [ STATE_CREATION ERROR #64: CREATEBUFFER_INVALIDBINDFLAGS]
			   Buffers for DrawIndirect can not be created with D3D11_BIND_CONSTANT_BUFFER. [ STATE_CREATION ERROR #68: CREATEBUFFER_INVALIDMISCFLAGS]
			   Buffers for DrawIndirect can not be created with D3D11_USAGE_STAGING. [ STATE_CREATION ERROR #68: CREATEBUFFER_INVALIDMISCFLAGS]
			   A D3D11_USAGE_STAGING Resource must have at least one CPUAccessFlag bit set. [ STATE_CREATION ERROR #63: CREATEBUFFER_INVALIDCPUACCESSFLAGS]
			   A D3D11_USAGE_STAGING Resource cannot be bound to any parts of the graphics pipeline, so therefore cannot have any BindFlags bits set. [ STATE_CREATION ERROR #64: CREATEBUFFER_INVALIDBINDFLAGS]               
			 * 
			 * A D3D11_USAGE_DYNAMIC Resource cannot be bound to certain parts of the graphics pipeline, but must have at least one BindFlags bit set. The BindFlags bits (0) have the following settings: D3D11_BIND_STREAM_OUTPUT (0), D3D11_BIND_RENDER_TARGET (0), D3D11_BIND_DEPTH_STENCIL (0), D3D11_BIND_UNORDERED_ACCESS (0). [ STATE_CREATION ERROR #64: CREATEBUFFER_INVALIDBINDFLAGS]
			 */

			// Indirect Arg buffers cannot be:
            // Structured.
            // Staging.

	        D3D.Buffer buffer = null;

	        try
	        {
		        buffer = new D3D.Buffer(graphics.D3DDevice,
		                                    new D3D.BufferDescription
			                                    {
				                                    BindFlags = D3D.BindFlags.ShaderResource,
				                                    CpuAccessFlags = D3D.CpuAccessFlags.Write,
				                                    OptionFlags = D3D.ResourceOptionFlags.BufferAllowRawViews,
				                                    SizeInBytes = 6144,
				                                    StructureByteStride = 128,
				                                    Usage = D3D.ResourceUsage.Dynamic
			                                    });
	        }
	        catch
	        {
				
	        }
	        finally
	        {
		        if (buffer != null)
		        {
			        buffer.Dispose();
		        }
	        }

			try
			{
				buffer = new D3D.Buffer(graphics.D3DDevice,
											new D3D.BufferDescription
											{
												BindFlags = D3D.BindFlags.ShaderResource,
												CpuAccessFlags = D3D.CpuAccessFlags.Write,
												OptionFlags = D3D.ResourceOptionFlags.BufferStructured,
												SizeInBytes = 6144,
												StructureByteStride = 128,
												Usage = D3D.ResourceUsage.Dynamic
											});
			}
			catch
			{

			}
			finally
			{
				if (buffer != null)
				{
					buffer.Dispose();
				}
			}

			try
			{
				buffer = new D3D.Buffer(graphics.D3DDevice,
											new D3D.BufferDescription
											{
												BindFlags = D3D.BindFlags.ShaderResource,
												CpuAccessFlags = D3D.CpuAccessFlags.Write,
												OptionFlags = D3D.ResourceOptionFlags.DrawIndirectArguments,
												SizeInBytes = 6144,
												StructureByteStride = 128,
												Usage = D3D.ResourceUsage.Dynamic
											});
			}
			catch
			{

			}
			finally
			{
				if (buffer != null)
				{
					buffer.Dispose();
				}
			}
		}
        #endregion
    }
}
