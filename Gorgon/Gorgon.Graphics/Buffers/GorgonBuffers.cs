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
// Created: Thursday, May 23, 2013 11:24:03 PM
// 
#endregion

using System;
using D3D = SharpDX.Direct3D;
using Gorgon.Core;
using Gorgon.Graphics.Properties;
using Gorgon.Native;

namespace Gorgon.Graphics
{
    /// <summary>
    /// An interface to create buffers.
    /// </summary>
    public class GorgonBuffers
    {
        #region Variables.
        private readonly GorgonGraphics _graphics;
        #endregion

        #region Methods.
		/// <summary>
		/// Function to perform validation upon the settings for a structured buffer.
		/// </summary>
		/// <param name="settings">Settings to test.</param>
		private static void ValidateStructuredBufferSettings(IBufferSettings settings)
		{
			if (settings.StructureSize <= 0)
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_BUFFER_STRUCTURE_SIZE_INVALID);
			}

			// Round up to the next multiple of 4.
			while ((settings.StructureSize % 4) != 0)
			{
				settings.StructureSize++;
			}

			ValidateGenericBufferSettings(settings);
		}

        /// <summary>
        /// Function to perform validation upon the settings for a constant buffer.
        /// </summary>
        private static void ValidateConstantBufferSettings(IBufferSettings settings)
        {
            // Ensure that we can actually put something into our buffer.
            if (settings.SizeInBytes < 16)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_BUFFER_SIZE_TOO_SMALL, 16));
            }

            // Allow only a multiple of 16.
            while ((settings.SizeInBytes % 16) != 0)
            {
	            settings.SizeInBytes++;
            }

			ValidateGenericBufferSettings(settings);
        }

		/// <summary>
		/// Function to perform validation upon the settings for a generic buffer.
		/// </summary>
		internal static void ValidateGenericBufferSettings(IBufferSettings settings)
		{
			if (settings == null)
			{
				return;
			}

			// Ensure that we can actually put something into our buffer.
			if (settings.SizeInBytes <= 4)
			{
				throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_BUFFER_SIZE_TOO_SMALL, 4));
			}

			// Only allow raw views if we've provided a shader view and/or an unordered access view.
			if ((settings.AllowRawViews)
				&& (!settings.AllowShaderViews)
				&& (!settings.AllowUnorderedAccessViews))
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_BUFFER_RAW_ACCESS_REQUIRES_VIEW_ACCESS);
			}

			if ((settings.IsOutput) && (settings.AllowUnorderedAccessViews))
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_BUFFER_OUTPUT_NO_UNORDERED);
			}

			// Do not allow staging usage with any of these.
			if ((settings.Usage == BufferUsage.Staging)
				&& ((settings.AllowIndirectArguments)
					|| (settings.AllowShaderViews)
					|| (settings.AllowRawViews)
					|| (settings.AllowUnorderedAccessViews)
					|| (settings.IsOutput)))
			{

				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_BUFFER_NO_STAGING_INVALID_FLAGS);
			}

			// Do not allow dynamic usage with any of these.
			if ((settings.Usage == BufferUsage.Dynamic)
				&& ((settings.AllowIndirectArguments)
					|| (settings.AllowUnorderedAccessViews)
					|| (settings.IsOutput)))
			{

				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_BUFFER_NO_DYNAMIC_INVALID_FLAGS);
			}

			if ((settings.DefaultShaderViewFormat != BufferFormat.Unknown) && (!settings.AllowShaderViews))
			{
				throw new GorgonException(GorgonResult.CannotBind, Resources.GORGFX_BUFFER_NO_SHADER_VIEWS);
			}
		}
		
		/// <summary>
        /// Function to create a generic buffer.
        /// </summary>
        /// <typeparam name="T">Type of data in the array.  Must be a value type.</typeparam>
        /// <param name="name">Name of the buffer.</param>
        /// <param name="values">Data used to initialize the buffer.</param>
        /// <param name="usage">Usage for the buffer.</param>
        /// <returns>A new generic buffer object populated with the data in <paramref name="values"/>.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (<i>Nothing</i> in VB.Net).
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="values"/> parameter is NULL.</para>
        /// </exception> 
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is empty.
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="values"/> parameter is empty.</para>
        /// </exception>
        /// <exception cref="GorgonException">Thrown when the buffer could not be created.</exception>
        /// <remarks>
        /// This generic buffer type is not capable of being bound to a shader, but can be used as a stream output from a geometry/compute shader.
        /// <para>This method should only be called from an immediate graphics context, if it is called from a deferred context an exception will be thrown.</para>
        /// </remarks>
        public GorgonBuffer_OLDEN CreateBuffer<T>(string name, T[] values, BufferUsage usage)
            where T : struct
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            if (values.Length == 0)
            {
                throw new ArgumentException(Resources.GORGFX_ERR_PARAMETER_MUST_NOT_BE_EMPTY, nameof(values));
            }
            
            using(IGorgonPointer pointer = new GorgonPointerPinned<T>(values))
            {
                return CreateBuffer(name,
                                    new GorgonBufferSettings
                                        {
                                            SizeInBytes = DirectAccess.SizeOf<T>() * values.Length,
                                            IsOutput = false,
                                            Usage = usage
                                        }, pointer);
            }
        }

        /// <summary>
        /// Function to create a generic buffer.
        /// </summary>
        /// <param name="name">Name of the buffer.</param>
        /// <param name="settings">The settings for the buffer.</param>
        /// <param name="data">[Optional] Stream used to initialize the buffer.</param>
        /// <returns>A new generic buffer object.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (<i>Nothing</i> in VB.Net).
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="settings"/> parameter is NULL.</para>
        /// </exception>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
        /// <exception cref="GorgonException">Thrown when the <see cref="Gorgon.Graphics.GorgonBufferSettings.IsOutput">IsOutput</see> property is <b>true</b> and has a usage that is not Default.
        /// <para>-or-</para>
        /// <para>Thrown when the <see cref="Gorgon.Graphics.GorgonBufferSettings.SizeInBytes">SizeInBytes</see> property of the <paramref name="settings"/> parameter is less than 1.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the usage is set to immutable and the <paramref name="data"/> parameter is NULL (<i>Nothing</i> in VB.Net) or has no data.</para>
        /// </exception>
		/// <remarks>The generic buffer is intended to be used with the [RW]Buffer&lt;&gt; HLSL type.
		/// <para>Generic buffers are only available on video devices that are capable of SM4 or better.</para>
        /// <para>This method should only be called from an immediate graphics context, if it is called from a deferred context an exception will be thrown.</para>
		/// </remarks>
        public GorgonBuffer_OLDEN CreateBuffer(string name, GorgonBufferSettings settings, IGorgonPointer data = null)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (name.Length == 0)
            {
                throw new ArgumentException(Resources.GORGFX_ERR_PARAMETER_MUST_NOT_BE_EMPTY, nameof(name));
            }

            if ((settings.Usage == BufferUsage.Immutable) && ((data == null) || (data.Size == 0)))
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_BUFFER_IMMUTABLE_REQUIRES_DATA);
            }

			if ((settings.Usage == BufferUsage.Dynamic) && (!settings.AllowShaderViews))
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_BUFFER_DYNAMIC_NEEDS_SHADER_VIEW);
			}

            // Validate our buffer settings.
            ValidateGenericBufferSettings(settings);

            var buffer = new GorgonBuffer_OLDEN(_graphics, name, settings);

            buffer.Initialize(data);

            _graphics.AddTrackedObject(buffer);
            return buffer;
        }

        /// <summary>
        /// Function to create a constant buffer.
        /// </summary>
        /// <param name="name">The name of the constant buffer.</param>
        /// <param name="settings">The settings for the buffer.</param>
        /// <param name="data">[Optional] Stream used to initialize the buffer.</param>
        /// <returns>A new constant buffer.</returns>
        /// <remarks>The size of the buffer must be a multiple of 16.
        /// <para>This method should only be called from an immediate graphics context, if it is called from a deferred context an exception will be thrown.</para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (<i>Nothing</i> in VB.Net).
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="settings"/> parameter is NULL.</para>
        /// </exception>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
        /// <exception cref="System.DataMisalignedException">Thrown when the <see cref="Gorgon.Graphics.GorgonConstantBufferSettings.SizeInBytes">SizeInBytes</see> property of the <paramref name="settings"/> parameter is not a multiple of 16.</exception>
        /// <exception cref="GorgonException">Thrown when the buffer size is less than 16 bytes.</exception>
        public GorgonConstantBuffer CreateConstantBuffer(string name, GorgonConstantBufferSettings settings, IGorgonPointer data = null)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if ((settings.Usage == BufferUsage.Immutable) && ((data == null) || (data.Size == 0)))
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_BUFFER_IMMUTABLE_REQUIRES_DATA);
            }

            ValidateConstantBufferSettings(settings);

            var buffer = new GorgonConstantBuffer(_graphics, name, settings);

            buffer.Initialize(data);

            _graphics.AddTrackedObject(buffer);
            return buffer;
        }

        /// <summary>
        /// Function to create a constant buffer and initializes it with data.
        /// </summary>
        /// <typeparam name="T">Type of data to pass to the constant buffer.  Must be a value type.</typeparam>
        /// <param name="name">The name of the constant buffer.</param>
        /// <param name="value">Value to write to the buffer</param>
        /// <param name="usage">Usage for the buffer.</param>
        /// <returns>A new constant buffer.</returns>
        /// <remarks>This method should only be called from an immediate graphics context, if it is called from a deferred context an exception will be thrown.</remarks>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
        /// <exception cref="GorgonException">Thrown when the buffer could not be created.</exception>
        public GorgonConstantBuffer CreateConstantBuffer<T>(string name, ref T value, BufferUsage usage)
            where T : struct
        {
            int size = DirectAccess.SizeOf<T>();

            // Ensure the total size of the object is a multiple of 16.
            while ((size % 16) != 0)
            {
                size++;
            }

	        var result = CreateConstantBuffer(name, new GorgonConstantBufferSettings
	        {
		        Usage = usage,
		        SizeInBytes = size
	        });

			result.Update(ref value);

	        return result;
        }

        /// <summary>
        /// Function to create a structured buffer and initialize it with data.
        /// </summary>
        /// <param name="name">The name of the structured buffer.</param>
        /// <param name="value">Value to write to the buffer.</param>
        /// <param name="usage">Usage for the buffer.</param>
        /// <returns>A new structured buffer.</returns>
        /// <typeparam name="T">Type of data to write.  Must be a value type.</typeparam>
        /// <remarks>This method should only be called from an immediate graphics context, if it is called from a deferred context an exception will be thrown.</remarks>
        /// <exception cref="GorgonException">Thrown when the buffer could not be created.</exception>
        public GorgonStructuredBuffer CreateStructuredBuffer<T>(string name, ref T value, BufferUsage usage)
            where T : struct
        {
	        GorgonStructuredBuffer buffer = CreateStructuredBuffer(name,
	                                                               new GorgonStructuredBufferSettings
	                                                               {
		                                                               Usage = usage,
		                                                               SizeInBytes = DirectAccess.SizeOf<T>(),
		                                                               StructureSize = DirectAccess.SizeOf<T>()
	                                                               });

			buffer.Update(ref value, _graphics);

			return buffer;
        }

        /// <summary>
        /// Function to create a structured buffer and initialize it with data.
        /// </summary>
        /// <param name="name">The name of the structured buffer.</param>
        /// <param name="values">Values to write to the buffer.</param>
        /// <param name="usage">Usage for the buffer.</param>
        /// <returns>A new structured buffer.</returns>
        /// <typeparam name="T">Type of data to write.  Must be a value type.</typeparam>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (<i>Nothing</i> in VB.Net).
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="values"/> parameter is NULL (<i>Nothing</i> in VB.Net).</para>
        /// </exception>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
        /// <exception cref="GorgonException">Thrown when the buffer could not be created.</exception>
        /// <remarks>This method should only be called from an immediate graphics context, if it is called from a deferred context an exception will be thrown.</remarks>
        public GorgonStructuredBuffer CreateStructuredBuffer<T>(string name, T[] values, BufferUsage usage)
            where T : struct
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            if (values.Length == 0)
            {
                throw new ArgumentException(Resources.GORGFX_ERR_PARAMETER_MUST_NOT_BE_EMPTY, nameof(values));
            }

            using (IGorgonPointer pointer = new GorgonPointerPinned<T>(values))
			{
				return CreateStructuredBuffer(name, new GorgonStructuredBufferSettings
                {
                    Usage = usage,
					SizeInBytes = DirectAccess.SizeOf<T>() * values.Length,
					StructureSize = DirectAccess.SizeOf<T>()
                }, pointer);
            }
        }

        /// <summary>
        /// Function to create a structured buffer and initialize it with data.
        /// </summary>
        /// <param name="name">The name of the structured buffer.</param>
        /// <param name="settings">Settings used to create the structured buffer.</param>
        /// <param name="data">[Optional] Stream containing the data used to initialize the buffer.</param>
        /// <returns>A new structured buffer.</returns>
        /// <remarks>This buffer type allows structures of data to be processed by the GPU without explicit byte mapping.
        /// <para>This method should only be called from an immediate graphics context, if it is called from a deferred context an exception will be thrown.</para>
        /// <para>Structured buffers can only be used on SM_5 video devices.</para>
        /// </remarks>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is empty.
        /// <para>-or-</para>
        /// <para>Thrown when the ElementSize or ElementCount properties in the <paramref name="settings"/> parameter are not greater than 0.</para>
        /// </exception>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (<i>Nothing</i> in VB.Net).
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="settings"/> parameter is NULL.</para>
        /// </exception>
		/// <exception cref="GorgonException">Thrown when the <see cref="Gorgon.Graphics.GorgonStructuredBufferSettings.StructureSize">StructureSize</see> property of the <paramref name="settings"/> parameter is less than 0 or greater than 2048.
        /// <para>-or-</para>
        /// <para>Thrown when the <see cref="Gorgon.Graphics.GorgonStructuredBufferSettings.SizeInBytes">SizeInBytes</see> property of the <paramref name="settings"/> parameter is less than 1.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the usage is set to immutable and the <paramref name="data"/> parameter is NULL (<i>Nothing</i> in VB.Net) or has no data.</para>
        /// <para>-or-</para>
        /// <para>Thrown when an attempt to create a structured buffer is made on a video device that does not support SM5 or better.</para>
        /// </exception>
        public GorgonStructuredBuffer CreateStructuredBuffer(string name, GorgonStructuredBufferSettings settings, IGorgonPointer data = null)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

			if (_graphics.VideoDevice.RequestedFeatureLevel < FeatureLevelSupport.Level_11_0)
			{
				throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_REQUIRES_FEATURE_LEVEL, FeatureLevelSupport.Level_11_0));
			}

			if ((settings.Usage == BufferUsage.Immutable) && ((data == null) || (data.Size == 0)))
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_BUFFER_IMMUTABLE_REQUIRES_DATA);
			}

			ValidateStructuredBufferSettings(settings);

            var result = new GorgonStructuredBuffer(_graphics, name, settings);
            result.Initialize(data);

            _graphics.AddTrackedObject(result);

            return result;
        }

        /// <summary>
        /// Function to create a vertex buffer.
        /// </summary>
        /// <param name="name">Name of the vertex buffer.</param>
        /// <param name="data">Data used to initialize the buffer.</param>
        /// <param name="usage">[Optional] Usage of the buffer.</param>
        /// <typeparam name="T">Type of data in the array.  Must be a value type.</typeparam>
        /// <returns>A new vertex buffer.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (<i>Nothing</i> in VB.Net).
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="data"/> parameter is NULL.</para>
        /// </exception>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> or the <paramref name="data"/> parameter is empty.</exception>
        /// <exception cref="GorgonException">Thrown when the buffer could not be created.</exception>
        /// <remarks>If creating an immutable vertex buffer, be sure to pre-populate it via the initialData parameter.
        /// <para>This method should only be called from an immediate graphics context, if it is called from a deferred context an exception will be thrown.</para>
        /// </remarks>
        public GorgonVertexBuffer CreateVertexBuffer<T>(string name, T[] data, BufferUsage usage = BufferUsage.Default)
            where T : struct
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (data.Length == 0)
            {
                throw new ArgumentException(Resources.GORGFX_ERR_PARAMETER_MUST_NOT_BE_EMPTY, nameof(data));
            }

            int size = data.Length * DirectAccess.SizeOf<T>();

            using (IGorgonPointer pointer = new GorgonPointerPinned<T>(data))
            {
                return CreateVertexBuffer(name, new GorgonBufferSettings
                {
                    IsOutput = false,
                    SizeInBytes = size,
                    Usage = usage
                }, pointer);
            }
        }

        /// <summary>
        /// Function to create a vertex buffer.
        /// </summary>
        /// <param name="name">Name of the vertex buffer.</param>
        /// <param name="settings">The settings for the buffer.</param>
        /// <param name="initialData">[Optional] Initial data to populate the vertex buffer with.</param>
        /// <returns>A new vertex buffer.</returns>
        /// <remarks>This method should only be called from an immediate graphics context, if it is called from a deferred context an exception will be thrown.</remarks>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (<i>Nothing</i> in VB.Net).
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="settings"/> parameter is NULL.</para>
        /// </exception>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
        /// <exception cref="GorgonException">Thrown when the <see cref="Gorgon.Graphics.GorgonBufferSettings.IsOutput">IsOutput</see> property is <b>true</b> and has a usage that is not Default.
        /// <para>-or-</para>
        /// <para>Thrown when the <see cref="Gorgon.Graphics.GorgonBufferSettings.SizeInBytes">SizeInBytes</see> property of the <paramref name="settings"/> parameter is less than 4.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the usage is set to immutable and the <paramref name="initialData"/> parameter is NULL (<i>Nothing</i> in VB.Net) or has no data.</para>
        /// </exception>
        public GorgonVertexBuffer CreateVertexBuffer(string name, GorgonBufferSettings settings, IGorgonPointer initialData = null)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if ((settings.Usage == BufferUsage.Immutable) && ((initialData == null) || (initialData.Size == 0)))
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_BUFFER_IMMUTABLE_REQUIRES_DATA);
            }

			ValidateGenericBufferSettings(settings);

            var buffer = new GorgonVertexBuffer(_graphics, name, settings);
            buffer.Initialize(initialData);

            _graphics.AddTrackedObject(buffer);
            return buffer;
        }

        /// <summary>
        /// Function to create a index buffer.
        /// </summary>
        /// <param name="name">The name of the buffer.</param>
        /// <param name="data">Data used to initialize the buffer.</param>
        /// <param name="usage">[Optional] Usage of the buffer.</param>
        /// <param name="is32Bit">[Optional] <b>true</b> to indicate that we're using 32 bit indices, <b>false</b> to use 16 bit indices </param>
        /// <typeparam name="T">Type of data in the array.  Must be a value type.</typeparam>
        /// <returns>A new index buffer.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> is NULL (<i>Nothing</i> in VB.Net).
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="data"/> parameter is NULL.</para>
        /// </exception> 
        /// <exception cref="GorgonException">Thrown when the buffer could not be created.</exception>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
        /// <remarks>If creating an immutable index buffer, be sure to pre-populate it via the initialData parameter.
        /// <para>This method should only be called from an immediate graphics context, if it is called from a deferred context an exception will be thrown.</para>
        /// </remarks>
        public GorgonIndexBuffer CreateIndexBuffer<T>(string name, T[] data, BufferUsage usage = BufferUsage.Default, bool is32Bit = true)
            where T : struct
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (data.Length == 0)
            {
                throw new ArgumentException(Resources.GORGFX_ERR_PARAMETER_MUST_NOT_BE_EMPTY, nameof(data));
            }

            int size = data.Length * DirectAccess.SizeOf<T>();

            using (IGorgonPointer pointer = new GorgonPointerPinned<T>(data))
            {
                return CreateIndexBuffer(name, new GorgonIndexBufferSettings
                {
                    SizeInBytes = size,
                    IsOutput = false,
                    Usage = usage,
                    Use32BitIndices = is32Bit
                }, pointer);
            }
        }

        /// <summary>
        /// Function to create a index buffer.
        /// </summary>
        /// <param name="name">The name of the buffer.</param>
        /// <param name="settings">The settings for the buffer.</param>
        /// <param name="initialData">[Optional] Initial data to populate the index buffer with.</param>
        /// <returns>A new index buffer.</returns>
        /// <remarks>This method should only be called from an immediate graphics context, if it is called from a deferred context an exception will be thrown.</remarks>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (<i>Nothing</i> in VB.Net).
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="settings"/> parameter is NULL.</para>
        /// </exception>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
        /// <exception cref="GorgonException">Thrown when the <see cref="Gorgon.Graphics.GorgonIndexBufferSettings.IsOutput">IsOutput</see> property is <b>true</b> and has a usage that is not Default.
        /// <para>-or-</para>
        /// <para>Thrown when the <see cref="Gorgon.Graphics.GorgonIndexBufferSettings.SizeInBytes">SizeInBytes</see> property of the <paramref name="settings"/> parameter is less than 1.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the usage is set to immutable and the <paramref name="initialData"/> parameter is NULL (<i>Nothing</i> in VB.Net) or has no data.</para>
        /// </exception>
        public GorgonIndexBuffer CreateIndexBuffer(string name, GorgonIndexBufferSettings settings, IGorgonPointer initialData = null)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (settings.SizeInBytes < 1)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_BUFFER_SIZE_TOO_SMALL, 1));
            }

            if ((settings.Usage == BufferUsage.Immutable) && ((initialData == null) || (initialData.Size == 0)))
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_BUFFER_IMMUTABLE_REQUIRES_DATA);
            }

			ValidateGenericBufferSettings(settings);

            var buffer = new GorgonIndexBuffer(_graphics, name, settings);
            buffer.Initialize(initialData);

            _graphics.AddTrackedObject(buffer);
            return buffer;
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonBuffers"/> class.
        /// </summary>
        /// <param name="graphics">The graphics interface that owns this interface..</param>
        internal GorgonBuffers(GorgonGraphics graphics)
        {
            _graphics = graphics;
        }
        #endregion
    }
}
