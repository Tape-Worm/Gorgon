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
using GorgonLibrary.IO;
using GorgonLibrary.Native;
using GorgonLibrary.Graphics.Properties;

namespace GorgonLibrary.Graphics
{
    // TODO:  This is getting too complicated.  Need to remove the "raw" buffer type and merge the GorgonTypedBuffer into GorgonBuffer.
    //        Put "AllowRaw" flag in settings instead.  
    //        Put "IsIndirect" flag in settings to Indirect argument buffers.
    //        Remove "GorgonShaderBuffer" and merge back into base buffer class.
    //        
    /// <summary>
    /// An interface to create buffers.
    /// </summary>
    public class GorgonBuffers
    {
        #region Variables.
        private GorgonGraphics _graphics;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to perform validation upon the settings for a generic buffer.
        /// </summary>
        private static void ValidateGenericBufferSettings(IBufferSettings2 settings)
        {
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

            if (settings.IsOutput)
            {
                if (settings.AllowUnorderedAccessViews)
                {
                    throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_BUFFER_OUTPUT_NO_UNORDERED);
                }

                if ((settings.Usage == BufferUsage.Dynamic)
                    || (settings.Usage == BufferUsage.Staging))
                {
                    throw new GorgonException(GorgonResult.CannotCreate,
                                              Resources.GORGFX_BUFFER_OUTPUT_NOT_DYNAMIC_OR_STAGING);
                }
            }

            // Do not allow staging usage with any of these.
            if ((settings.Usage == BufferUsage.Staging)
                && ((settings.AllowIndirectArguments) || (settings.AllowRawViews)
                    || (settings.AllowRenderTarget) || (settings.AllowShaderViews)
                    || (settings.AllowUnorderedAccessViews)))
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_BUFFER_NO_STAGING_INVALID_FLAGS);
            }

            if ((settings.DefaultShaderViewFormat != BufferFormat.Unknown) &&  (!settings.AllowShaderViews))
            {
                throw new GorgonException(GorgonResult.CannotBind, Resources.GORGFX_BUFFER_NO_SHADER_VIEWS);
            }
        }

        /// <summary>
        /// Function to perform validation upon the settings for a generic buffer.
        /// </summary>
        private static void ValidateConstantBufferSettings(IBufferSettings2 settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            if (settings.Usage == BufferUsage.Staging)
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_BUFFER_NO_STAGING_BUFFER_TYPE);
            }

            // Ensure that we can actually put something into our buffer.
            if (settings.SizeInBytes < 16)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_BUFFER_SIZE_TOO_SMALL, 16));
            }

            // Allow only a multiple of 16.
            if ((settings.SizeInBytes % 16) != 0)
            {
                throw new DataMisalignedException(string.Format(Resources.GORGFX_BUFFER_NOT_MULTIPLE, 16));
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
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="values"/> parameter is NULL.</para>
        /// </exception> 
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is empty.
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="values"/> parameter is empty.</para>
        /// </exception>
        /// <remarks>This generic buffer type is not capable of being bound to a shader, but can be used as a stream output from a geometry/compute shader.</remarks>
        public GorgonBuffer CreateBuffer<T>(string name, T[] values, BufferUsage usage)
            where T : struct
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }

            if (values.Length == 0)
            {
                throw new ArgumentException(Resources.GORGFX_PARAMETER_MUST_NOT_BE_EMPTY, "values");
            }
            
            using(var stream = new GorgonDataStream(values))
            {
                return CreateBuffer(name,
                                    new GorgonBufferSettings
                                        {
                                            SizeInBytes = DirectAccess.SizeOf<T>() * values.Length,
                                            IsOutput = false,
                                            Usage = usage
                                        }, stream);
            }
        }

        /// <summary>
        /// Function to create a generic buffer.
        /// </summary>
        /// <param name="name">Name of the buffer.</param>
        /// <param name="settings">The settings for the buffer.</param>
        /// <param name="stream">[Optional] Stream used to initialize the buffer.</param>
        /// <returns>A new generic buffer object.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="settings"/> parameter is NULL.</para>
        /// </exception>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
        /// <exception cref="GorgonLibrary.GorgonException">Thrown when the <see cref="GorgonLibrary.Graphics.GorgonBufferSettings.IsOutput">IsOutput</see> property is TRUE and has a usage that is not Default.
        /// <para>-or-</para>
        /// <para>Thrown when the <see cref="GorgonLibrary.Graphics.GorgonBufferSettings.SizeInBytes">SizeInBytes</see> property of the <paramref name="settings"/> parameter is less than 1.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the usage is set to immutable and the <paramref name="stream"/> parameter is NULL (Nothing in VB.Net) or has no data.</para>
        /// </exception>
        /// <remarks>This generic buffer type is not capable of being bound to a shader, but can be used as a stream output from a geometry/compute shader.</remarks>
        public GorgonBuffer CreateBuffer(string name, GorgonBufferSettings settings, GorgonDataStream stream = null)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            if (name.Length == 0)
            {
                throw new ArgumentException(Resources.GORGFX_PARAMETER_MUST_NOT_BE_EMPTY, "name");
            }

            if ((settings.Usage == BufferUsage.Immutable) && ((stream == null) || (stream.Length == 0)))
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_BUFFER_IMMUTABLE_REQUIRES_DATA);
            }

            // Validate our buffer settings.
            ValidateGenericBufferSettings(settings);

            var buffer = new GorgonBuffer(_graphics, name, settings);

            buffer.Initialize(stream);

            _graphics.AddTrackedObject(buffer);
            return buffer;
        }

        /// <summary>
        /// Function to create a constant buffer.
        /// </summary>
        /// <param name="name">The name of the constant buffer.</param>
        /// <param name="settings">The settings for the buffer.</param>
        /// <param name="stream">[Optional] Stream used to initialize the buffer.</param>
        /// <returns>A new constant buffer.</returns>
        /// <remarks>The size of the buffer must be a multiple of 16.</remarks>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="settings"/> parameter is NULL.</para>
        /// </exception>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
        /// <exception cref="System.DataMisalignedException">Thrown when the <see cref="GorgonLibrary.Graphics.GorgonConstantBufferSettings.SizeInBytes">SizeInBytes</see> property of the <paramref name="settings"/> parameter is not a multiple of 16.</exception>
        /// <exception cref="GorgonLibrary.GorgonException">Thrown when the buffer size is less than 16 bytes.</exception>
        public GorgonConstantBuffer CreateConstantBuffer(string name, GorgonConstantBufferSettings settings, GorgonDataStream stream = null)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            if ((settings.Usage == BufferUsage.Immutable) && ((stream == null) || (stream.Length == 0)))
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_BUFFER_IMMUTABLE_REQUIRES_DATA);
            }

            ValidateConstantBufferSettings(settings);

            var buffer = new GorgonConstantBuffer(_graphics, name, settings);

            buffer.Initialize(stream);

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
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
        public GorgonConstantBuffer CreateConstantBuffer<T>(string name, T value, BufferUsage usage)
            where T : struct
        {
            using (GorgonDataStream stream = GorgonDataStream.ValueToStream(value))
            {
                return CreateConstantBuffer(name, new GorgonConstantBufferSettings
                {
                    Usage = usage,
                    SizeInBytes = (int)stream.Length
                }, stream);
            }
        }

        /// <summary>
        /// Function to create a structured buffer and initialize it with data.
        /// </summary>
        /// <param name="name">The name of the structured buffer.</param>
        /// <param name="value">Value to write to the buffer.</param>
        /// <param name="usage">Usage for the buffer.</param>
        /// <returns>A new structured buffer.</returns>
        /// <typeparam name="T">Type of data to write.  Must be a value type.</typeparam>
        public GorgonStructuredBuffer CreateStructuredBuffer<T>(string name, T value, BufferUsage usage)
            where T : struct
        {
            using (var stream = GorgonDataStream.ValueToStream(value))
            {
                return CreateStructuredBuffer(name, new GorgonStructuredBufferSettings
                {
                    IsOutput = false,
                    AllowUnorderedAccess = false,
                    Usage = usage,
                    ElementCount = 1,
                    ElementSize = DirectAccess.SizeOf<T>()
                }, stream);
            }
        }

        /// <summary>
        /// Function to create a structured buffer and initialize it with data.
        /// </summary>
        /// <param name="name">The name of the structured buffer.</param>
        /// <param name="values">Values to write to the buffer.</param>
        /// <param name="usage">Usage for the buffer.</param>
        /// <returns>A new structured buffer.</returns>
        /// <typeparam name="T">Type of data to write.  Must be a value type.</typeparam>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="values"/> parameter is NULL (Nothing in VB.Net).</para>
        /// </exception>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
        public GorgonStructuredBuffer CreateStructuredBuffer<T>(string name, T[] values, BufferUsage usage)
            where T : struct
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }

            if (values.Length == 0)
            {
                throw new ArgumentException(Resources.GORGFX_PARAMETER_MUST_NOT_BE_EMPTY, "values");
            }

            using (var stream = new GorgonDataStream(values))
            {
                return CreateStructuredBuffer(name, new GorgonStructuredBufferSettings
                {
                    IsOutput = false,
                    AllowUnorderedAccess = false,
                    Usage = usage,
                    ElementCount = values.Length,
                    ElementSize = DirectAccess.SizeOf<T>()
                }, stream);
            }
        }

        /// <summary>
        /// Function to create a structured buffer and initialize it with data.
        /// </summary>
        /// <param name="name">The name of the structured buffer.</param>
        /// <param name="settings">Settings used to create the structured buffer.</param>
        /// <param name="stream">[Optional] Stream containing the data used to initialize the buffer.</param>
        /// <returns>A new structured buffer.</returns>
        /// <remarks>This buffer type allows structures of data to be processed by the GPU without implicit byte mapping.
        /// <para>Structured buffers can only be used on SM_5 video devices.</para></remarks>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is empty.
        /// <para>-or-</para>
        /// <para>Thrown when the ElementSize or ElementCount properties in the <paramref name="settings"/> parameter are not greater than 0.</para>
        /// </exception>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="settings"/> parameter is NULL.</para>
        /// </exception>
        /// <exception cref="GorgonLibrary.GorgonException">Thrown when the <see cref="GorgonLibrary.Graphics.GorgonStructuredBufferSettings.IsOutput">IsOutput</see> property is TRUE and has a usage that is not Default.
        /// <para>-or-</para>
        /// <para>Thrown when the <see cref="GorgonLibrary.Graphics.GorgonStructuredBufferSettings.ElementCount">ElementCount</see> or the <see cref="GorgonLibrary.Graphics.GorgonStructuredBufferSettings.ElementSize">ElementSize</see> property of the <paramref name="settings"/> parameter is less than 1.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the usage is set to immutable and the <paramref name="stream"/> parameter is NULL (Nothing in VB.Net) or has no data.</para>
        /// <para>-or-</para>
        /// <para>Thrown when an attempt to create a structured buffer is made on a video device that does not support SM5 or better.</para>
        /// </exception>
        public GorgonStructuredBuffer CreateStructuredBuffer(string name, GorgonStructuredBufferSettings settings, GorgonDataStream stream = null)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            if (_graphics.VideoDevice.SupportedFeatureLevel < DeviceFeatureLevel.SM5)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_REQUIRES_SM, DeviceFeatureLevel.SM5));
            }

            if ((settings.Usage == BufferUsage.Immutable) && ((stream == null) || (stream.Length == 0)))
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_BUFFER_IMMUTABLE_REQUIRES_DATA);
            }

            if (settings.ElementCount <= 0)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_BUFFER_ELEMENT_COUNT_INVALID, 1));
            }

            if (settings.ElementSize <= 0)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_BUFFER_SIZE_TOO_SMALL, 1));
            }

            if ((settings.IsOutput)
                && (settings.Usage != BufferUsage.Default))
            {
                throw new GorgonException(GorgonResult.CannotBind, Resources.GORGFX_BUFFER_OUTPUT_NOT_DYNAMIC_OR_STAGING);
            }

            var result = new GorgonStructuredBuffer(_graphics, name, settings);
            result.Initialize(stream);

            _graphics.AddTrackedObject(result);

            return result;
        }

        /// <summary>
        /// Function to create a typed buffer and initialize it with a single value.
        /// </summary>
        /// <typeparam name="T">The type of data to store in the buffer. Must be a value type.</typeparam>
        /// <param name="name">Name of the typed buffer.</param>
        /// <param name="value">Value to write to the buffer.</param>
        /// <param name="shaderViewFormat">Format for the shader resource view.</param>
        /// <param name="usage">The usage of the buffer.</param>
        /// <returns>A new typed buffer.</returns>
        /// <remarks>This buffer type allows typed elements of data to be processed by the GPU.  The shader view format must be the same size, in bytes, as the type parameter.</remarks>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is empty.
        /// <para>Thrown when the <paramref name="shaderViewFormat"/> is Unknown.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="shaderViewFormat"/> is not the same size, in bytes, as the type parameter.</para>
        /// </exception>
        public GorgonTypedBuffer<T> CreateTypedBuffer<T>(string name, T value, BufferFormat shaderViewFormat, BufferUsage usage)
            where T : struct
        {
            using (var stream = GorgonDataStream.ValueToStream(value))
            {
                return CreateTypedBuffer(name, new GorgonTypedBufferSettings<T>
                {
                    Usage = usage,
                    ElementCount = 1,
                    IsOutput = false,
                    ShaderViewFormat = shaderViewFormat,
                    AllowUnorderedAccess = false
                }, stream);
            }
        }

        /// <summary>
        /// Function to create a typed buffer and initialize it with multiple values.
        /// </summary>
        /// <typeparam name="T">The type of data to store in the buffer. Must be a value type.</typeparam>
        /// <param name="name">Name of the typed buffer.</param>
        /// <param name="values">Values to write to the buffer.</param>
        /// <param name="shaderViewFormat">Format for the shader resource view.</param>
        /// <param name="usage">The usage of the buffer.</param>
        /// <returns>A new typed buffer.</returns>
        /// <remarks>This buffer type allows typed elements of data to be processed by the GPU.  The shader view format must be the same size, in bytes, as the type parameter.</remarks>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="values"/> parameter is NULL.</para>
        /// </exception>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is empty.
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="values"/> parameter is empty.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="shaderViewFormat"/> is Unknown.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="shaderViewFormat"/> is not the same size, in bytes, as the type parameter.</para>
        /// </exception>
        public GorgonTypedBuffer<T> CreateTypedBuffer<T>(string name, T[] values, BufferFormat shaderViewFormat, BufferUsage usage)
            where T : struct
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }

            if (values.Length == 0)
            {
                throw new ArgumentException(Resources.GORGFX_PARAMETER_MUST_NOT_BE_EMPTY, "values");
            }

            using (var stream = new GorgonDataStream(values))
            {
                return CreateTypedBuffer(name, new GorgonTypedBufferSettings<T>
                {
                    Usage = usage,
                    ElementCount = values.Length,
                    IsOutput = false,
                    ShaderViewFormat = shaderViewFormat,
                    AllowUnorderedAccess = false
                }, stream);
            }
        }


        /// <summary>
        /// Function to create a typed buffer and initialize it with data.
        /// </summary>
        /// <typeparam name="T">The type of data to store in the buffer. Must be a value type.</typeparam>
        /// <param name="name">Name of the typed buffer.</param>
        /// <param name="settings">Settings used to create the typed buffer.</param>
        /// <param name="stream">[Optional] Stream containing the data used to initialize the buffer.</param>
        /// <returns>A new typed buffer.</returns>
        /// <remarks>This buffer type allows typed elements of data to be processed by the GPU.  The shader view format must be the same size, in bytes, as the type parameter.
        /// <para>When creating a raw byte buffer, the number of elements and the size in bytes of an element must be a multiple of 4.</para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="settings"/> parameter is NULL.</para>
        /// </exception>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is empty.
        /// <para>-or-</para>
        /// <para>Thrown when the ElementCount property in the <paramref name="settings"/> parameter is not greater than 1.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the <see cref="GorgonLibrary.Graphics.GorgonTypedBufferSettings{T}.ShaderViewFormat">settings.ShaderViewFormat</see> is Unknown.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the <see cref="GorgonLibrary.Graphics.GorgonTypedBufferSettings{T}.ShaderViewFormat">settings.ShaderViewFormat</see> is not the same size, in bytes, as the type parameter.</para>
        /// </exception>
        /// <exception cref="System.DataMisalignedException">Thrown when the buffer has raw access and the total size of the buffer is not a multiple of 4.</exception>
        /// <exception cref="GorgonLibrary.GorgonException">Thrown when the <see cref="GorgonLibrary.Graphics.GorgonTypedBufferSettings{T}.IsOutput">IsOutput</see> property is TRUE and has a usage that is not Default.
        /// <para>-or-</para>
        /// <para>Thrown when the <see cref="GorgonLibrary.Graphics.GorgonTypedBufferSettings{T}.ElementCount">ElementCount</see> property of the <paramref name="settings"/> parameter is less than 1.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the usage is set to immutable and the <paramref name="stream"/> parameter is NULL (Nothing in VB.Net) or has no data.</para>
        /// </exception>
        public GorgonTypedBuffer<T> CreateTypedBuffer<T>(string name, GorgonTypedBufferSettings<T> settings, GorgonDataStream stream = null)
            where T : struct
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            if (settings.ShaderViewFormat == BufferFormat.Unknown)
            {
                throw new ArgumentException(Resources.GORGFX_VIEW_UNKNOWN_FORMAT, "settings");
            }

            if ((settings.Usage == BufferUsage.Immutable) && ((stream == null) || (stream.Length == 0)))
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_BUFFER_IMMUTABLE_REQUIRES_DATA);
            }

            var info = GorgonBufferFormatInfo.GetInfo(settings.ShaderViewFormat);

            if (settings.ElementSize != info.SizeInBytes)
            {
                throw new ArgumentException(
                    string.Format(
                        Resources.GORGFX_BUFFER_FORMAT_SIZE_MISMATCH,
                        info.SizeInBytes,
                        settings.ElementSize),
                    "settings");
            }

            if ((settings.IsOutput)
                && (settings.Usage != BufferUsage.Default))
            {
                throw new GorgonException(GorgonResult.CannotBind, Resources.GORGFX_BUFFER_OUTPUT_NOT_DYNAMIC_OR_STAGING);
            }

            if (settings.ElementCount <= 0)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_BUFFER_SIZE_TOO_SMALL, DirectAccess.SizeOf<T>()));
            }

            var result = new GorgonTypedBuffer<T>(_graphics, name, settings);
            result.Initialize(stream);

            _graphics.AddTrackedObject(result);

            return result;
        }

        /// <summary>
        /// Function to create a raw buffer and initialize it with multiple values.
        /// </summary>
        /// <typeparam name="T">The type of data in the buffer.  Must be a value type.</typeparam>
        /// <param name="name">The name of the raw buffer.</param>
        /// <param name="values">Byte values to write to the buffer.</param>
        /// <param name="usage">The usage for the buffer.</param>
        /// <returns>A new typed buffer.</returns>
        /// <remarks>This buffer type allows raw data to be processed by the GPU.
        /// <para>When creating the buffer, the number of bytes must be a multiple of 4.</para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="values"/> parameter is NULL.</para>
        /// </exception>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is empty.
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="values"/> parameter is empty.</para>
        /// </exception>
        /// <exception cref="GorgonLibrary.GorgonException">Thrown when attempting to create a raw buffer on a video device that does not support SM_5 or better.</exception>
        public GorgonRawBuffer CreateRawBuffer<T>(string name, T[] values, BufferUsage usage)
            where T : struct
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }

            if (values.Length == 0)
            {
                throw new ArgumentException(Resources.GORGFX_PARAMETER_MUST_NOT_BE_EMPTY, "values");
            }

            int length = values.Length * DirectAccess.SizeOf<T>();

            // Increase the length of the buffer.
            while ((length % 4) != 0)
            {
                length++;
            }

            using (var stream = new GorgonDataStream(values))
            {
                return CreateRawBuffer(name, new GorgonRawBufferSettings
                {
                    Usage = usage,
                    SizeInBytes = length,
                    IsOutput = false,
                    AllowUnorderedAccess = false
                }, stream);
            }
        }

        /// <summary>
        /// Function to create a raw buffer and initialize it with data.
        /// </summary>
        /// <param name="name">Name of the buffer.</param>
        /// <param name="settings">Settings used to create the typed buffer.</param>
        /// <param name="stream">[Optional] Stream containing the data used to initialize the buffer.</param>
        /// <returns>A new typed buffer.</returns>
        /// <remarks>This buffer type allows raw data to be processed by the GPU.
        /// <para>When creating the buffer, the number of elements and the size in bytes of an element must be a multiple of 4.</para>
        /// <para>Raw buffers can only be used on SM_5 video devices.</para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="settings"/> parameter is NULL.</para>
        /// </exception>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is empty.
        /// <para>-or-</para>
        /// <para>Thrown when the ElementCount property in the <paramref name="settings"/> parameter is not greater than 1.</para>
        /// </exception>
        /// <exception cref="System.DataMisalignedException">Thrown when the buffer has raw access and the total size of the buffer is not a multiple of 4.</exception>
        /// <exception cref="GorgonLibrary.GorgonException">Thrown when the <see cref="GorgonLibrary.Graphics.GorgonRawBufferSettings.IsOutput">IsOutput</see> property is TRUE and has a usage that is not Default.
        /// <para>-or-</para>
        /// <para>Thrown when the <see cref="GorgonLibrary.Graphics.GorgonRawBufferSettings.SizeInBytes">SizeInBytes</see> property of the <paramref name="settings"/> parameter is less than 1.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the usage is set to immutable and the <paramref name="stream"/> parameter is NULL (Nothing in VB.Net) or has no data.</para>
        /// <para>-or-</para>
        /// <para>Thrown when an attempt to create a structured buffer is made on a video device that does not support SM5 or better.</para>
        /// </exception>
        public GorgonRawBuffer CreateRawBuffer(string name, GorgonRawBufferSettings settings, GorgonDataStream stream = null)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            if (_graphics.VideoDevice.SupportedFeatureLevel < DeviceFeatureLevel.SM5)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_REQUIRES_SM, DeviceFeatureLevel.SM5));
            }

            // Raw buffer size must be a multiple of 4.
            if ((settings.SizeInBytes % 4) != 0)
            {
                throw new DataMisalignedException(string.Format(Resources.GORGFX_BUFFER_NOT_MULTIPLE,
                                                                settings.SizeInBytes, 4));
            }

            if ((settings.Usage == BufferUsage.Immutable) && ((stream == null) || (stream.Length == 0)))
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_BUFFER_IMMUTABLE_REQUIRES_DATA);
            }

            if ((settings.IsOutput)
                && (settings.Usage != BufferUsage.Default))
            {
                throw new GorgonException(GorgonResult.CannotBind, Resources.GORGFX_BUFFER_OUTPUT_NOT_DYNAMIC_OR_STAGING);
            }

            if (settings.SizeInBytes < 1)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_BUFFER_SIZE_TOO_SMALL, 1));
            }

            var result = new GorgonRawBuffer(_graphics, name, settings);
            result.Initialize(stream);

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
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="data"/> parameter is NULL.</para>
        /// </exception>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> or the <paramref name="data"/> parameter is empty.</exception>
        /// <remarks>If creating an immutable vertex buffer, be sure to pre-populate it via the initialData parameter.
        /// </remarks>
        public GorgonVertexBuffer CreateVertexBuffer<T>(string name, T[] data, BufferUsage usage = BufferUsage.Default)
            where T : struct
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            if (data.Length == 0)
            {
                throw new ArgumentException(Resources.GORGFX_PARAMETER_MUST_NOT_BE_EMPTY, "data");
            }

            int size = data.Length * DirectAccess.SizeOf<T>();

            using (var dataStream = new GorgonDataStream(data))
            {
                return CreateVertexBuffer(name, new GorgonVertexBufferSettings
                {
                    IsOutput = false,
                    SizeInBytes = size,
                    Usage = usage
                }, dataStream);
            }
        }

        /// <summary>
        /// Function to create a vertex buffer.
        /// </summary>
        /// <param name="name">Name of the vertex buffer.</param>
        /// <param name="settings">The settings for the buffer.</param>
        /// <param name="initialData">[Optional] Initial data to populate the vertex buffer with.</param>
        /// <returns>A new vertex buffer.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="settings"/> parameter is NULL.</para>
        /// </exception>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
        /// <exception cref="GorgonLibrary.GorgonException">Thrown when the <see cref="GorgonLibrary.Graphics.GorgonVertexBufferSettings.IsOutput">IsOutput</see> property is TRUE and has a usage that is not Default.
        /// <para>-or-</para>
        /// <para>Thrown when the <see cref="GorgonLibrary.Graphics.GorgonVertexBufferSettings.SizeInBytes">SizeInBytes</see> property of the <paramref name="settings"/> parameter is less than 1.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the usage is set to immutable and the <paramref name="initialData"/> parameter is NULL (Nothing in VB.Net) or has no data.</para>
        /// </exception>
        public GorgonVertexBuffer CreateVertexBuffer(string name, GorgonVertexBufferSettings settings, GorgonDataStream initialData = null)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            if (settings.SizeInBytes < 1)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_BUFFER_SIZE_TOO_SMALL, 1));
            }

            if ((settings.Usage == BufferUsage.Immutable) && ((initialData == null) || (initialData.Length == 0)))
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_BUFFER_IMMUTABLE_REQUIRES_DATA);
            }

            if ((settings.IsOutput)
                && (settings.Usage != BufferUsage.Default))
            {
                throw new GorgonException(GorgonResult.CannotBind, Resources.GORGFX_BUFFER_OUTPUT_NOT_DYNAMIC_OR_STAGING);
            }

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
        /// <param name="is32Bit">[Optional] TRUE to indicate that we're using 32 bit indices, FALSE to use 16 bit indices </param>
        /// <typeparam name="T">Type of data in the array.  Must be a value type.</typeparam>
        /// <returns>A new index buffer.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> is NULL (Nothing in VB.Net).
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="data"/> parameter is NULL.</para>
        /// </exception> 
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
        /// <remarks>If creating an immutable index buffer, be sure to pre-populate it via the initialData parameter.</remarks>
        public GorgonIndexBuffer CreateIndexBuffer<T>(string name, T[] data, BufferUsage usage = BufferUsage.Default, bool is32Bit = true)
            where T : struct
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            if (data.Length == 0)
            {
                throw new ArgumentException(Resources.GORGFX_PARAMETER_MUST_NOT_BE_EMPTY, "data");
            }

            int size = data.Length * DirectAccess.SizeOf<T>();

            using (var dataStream = new GorgonDataStream(data))
            {
                return CreateIndexBuffer(name, new GorgonIndexBufferSettings
                {
                    SizeInBytes = size,
                    IsOutput = false,
                    Usage = usage,
                    Use32BitIndices = is32Bit
                }, dataStream);
            }
        }

        /// <summary>
        /// Function to create a index buffer.
        /// </summary>
        /// <param name="name">The name of the buffer.</param>
        /// <param name="settings">The settings for the buffer.</param>
        /// <param name="initialData">[Optional] Initial data to populate the index buffer with.</param>
        /// <returns>A new index buffer.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="settings"/> parameter is NULL.</para>
        /// </exception>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
        /// <exception cref="GorgonLibrary.GorgonException">Thrown when the <see cref="GorgonLibrary.Graphics.GorgonIndexBufferSettings.IsOutput">IsOutput</see> property is TRUE and has a usage that is not Default.
        /// <para>-or-</para>
        /// <para>Thrown when the <see cref="GorgonLibrary.Graphics.GorgonIndexBufferSettings.SizeInBytes">SizeInBytes</see> property of the <paramref name="settings"/> parameter is less than 1.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the usage is set to immutable and the <paramref name="initialData"/> parameter is NULL (Nothing in VB.Net) or has no data.</para>
        /// </exception>
        public GorgonIndexBuffer CreateIndexBuffer(string name, GorgonIndexBufferSettings settings, GorgonDataStream initialData = null)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            if (settings.SizeInBytes < 1)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_BUFFER_SIZE_TOO_SMALL, 1));
            }

            if ((settings.Usage == BufferUsage.Immutable) && ((initialData == null) || (initialData.Length == 0)))
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_BUFFER_IMMUTABLE_REQUIRES_DATA);
            }

            if ((settings.IsOutput)
                && (settings.Usage != BufferUsage.Default))
            {
                throw new GorgonException(GorgonResult.CannotBind, Resources.GORGFX_BUFFER_OUTPUT_NOT_DYNAMIC_OR_STAGING);
            }

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
