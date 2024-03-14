#region MIT
// 
// Gorgon.
// Copyright (C) 2016 Michael Winsor
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
// Created: June 15, 2016 9:33:57 PM
// 
#endregion

using Gorgon.Core;
using Gorgon.Diagnostics;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A buffer for constant shader data.
/// </summary>
/// <remarks>
/// <para>
/// To send changing data to a shader from you application to a constant buffer, an application can upload a value type (or primitive) value to the buffer using one of the 
/// <see cref="GorgonBufferCommon.SetData{T}(ReadOnlySpan{T}, int, CopyMode)"/> methods. This allows an application to update the state of a shader to reflect changes in the application. Things like animation or setup 
/// information can easily be sent to modify the state of a shader (hence somewhat making the term <i>constant</i> a bit of a misnomer).
/// </para>
/// <para>
/// A constant buffer must be a minimum of 16 bytes in size (4 float values), and be aligned to 16 bytes. The maximum size of the buffer is limited to 134,217,728 elements (one element = 4x 32 bit float
/// values). However, the GPU can only address 4096 (64K) of these elements at a time. As such the buffer will have to be bound to the GPU using a range defined by a <see cref="GorgonConstantBufferView"/>. 
/// </para>
/// <para>
/// If the <see cref="IGorgonConstantBufferInfo"/> <see cref="IGorgonConstantBufferInfo.SizeInBytes"/> value is less than 16 bytes, or is not aligned to 16 bytes, it will be adjusted before buffer
/// creation to ensure the buffer is adequate.
/// </para>
/// <para>
/// Constant buffers are bound to a finite number of slots in the shader. Typically these are declared as follows:
/// <pre>
/// cbuffer ViewMatrix : register(b0)
/// {
///	   Vector3 viewMatrix;
///    Vector3 other;
/// }
/// </pre>
/// This binds a matrix used for the view to constant buffer slot 0. Note that the register slot name starts with a <b>b</b>.
/// </para>
/// <para> 
/// <example language="csharp">
/// For example, to update a view matrix to shift to the right every frame:
/// <code language="csharp">
/// <![CDATA[
/// Vector3 _lastPosition;
/// GorgonConstantBuffer _viewMatrixBuffer;		// This is created elsewhere with a size of 64 bytes to hold a Matrix.
/// 
/// void IdleMethod()
/// {
///		// Move 2 units to the right every second.
///		_lastPosition = new Vector3(_lastPosition.X + 2 * GorgonTiming.Delta, 0, -2.0f);
///		Matrix viewMatrix = Matrix.Identity;
/// 
///		// Adjust the matrix to perform the translation.
///		// We use ref/out here for better performance.
///		Matrix.Translation(ref _lastPosition, out viewMatrix);
///  
///		// Send to the shader (typically, this would be the vertex shader).
///		_viewMatrixBuffer.SetData<Matrix>(ref viewMatrix);
/// 
///		// Send again to the shader, but this time, at the fourth float value index.
///     // This would skip the first 4 float values in viewMatrix, and write into
///     // the remaining 60 float values, and the first 4 float values in "other".
///		_viewMatrixBuffer.SetData<Matrix>(ref viewMatrix, 4 * sizeof(float));
/// }
/// ]]>
/// </code>
/// </example>
/// </para>
/// </remarks>
/// <seealso cref="GorgonConstantBufferView"/>
public sealed class GorgonConstantBuffer
    : GorgonBufferCommon, IGorgonConstantBufferInfo
{
    #region Constants.
    /// <summary>
    /// The prefix to assign to a default name.
    /// </summary>
    internal const string NamePrefix = nameof(GorgonConstantBuffer);
    #endregion

    #region Variables.
    // The information used to create the buffer.
#if NET48_OR_GREATER
#pragma warning disable IDE0044 // Add readonly modifier
#endif
    private GorgonConstantBufferInfo _info;
#if NET48_OR_GREATER
#pragma warning restore IDE0044 // Add readonly modifier
#endif
    // A list of constant buffer views.
    private List<GorgonConstantBufferView> _cbvs = [];
    #endregion

    #region Properties.
    /// <summary>
    /// Property to return the bind flags used for the D3D 11 resource.
    /// </summary>
    internal override D3D11.BindFlags BindFlags => Native?.Description.BindFlags ?? D3D11.BindFlags.None;

    /// <summary>
    /// Property to return whether or not the user has requested that the buffer be readable from the CPU.
    /// </summary>
    public override bool IsCpuReadable => Usage == ResourceUsage.Staging;

    /// <summary>
    /// Property to return the usage for the resource.
    /// </summary>
    public override ResourceUsage Usage => _info.Usage;

    /// <summary>
    /// Property to return the size, in bytes, of the resource.
    /// </summary>
    public override int SizeInBytes => _info.SizeInBytes;

    /// <summary>
    /// Property to return the name of this object.
    /// </summary>
    public override string Name => _info.Name;

    /// <summary>
    /// Property to return the number of 4 component floating point values (16 bytes) that can be stored in this buffer.
    /// </summary>
    public int TotalConstantCount
    {
        get;
        private set;
    }
    #endregion

    #region Methods.
    /// <summary>
    /// Function to initialize the buffer data.
    /// </summary>
    /// <param name="initialData">The initial data used to populate the buffer.</param>
    private void Initialize(ReadOnlySpan<byte> initialData)
    {
        // If the buffer is not aligned to 16 bytes, then pad the size.
        int newSize = (_info.SizeInBytes + 15) & ~15;

        if (newSize != _info.SizeInBytes)
        {
            _info = _info with
            {
                SizeInBytes = newSize
            };
        }

        TotalConstantCount = _info.SizeInBytes / (sizeof(float) * 4);

        D3D11.CpuAccessFlags cpuFlags = GetCpuFlags(false, D3D11.BindFlags.ConstantBuffer);

        Log.Print($"{Name} Constant Buffer: Creating D3D11 buffer. Size: {_info.SizeInBytes} bytes", LoggingLevel.Simple);

        var desc = new D3D11.BufferDescription
        {
            SizeInBytes = _info.SizeInBytes,
            Usage = (D3D11.ResourceUsage)_info.Usage,
            BindFlags = _info.Usage != ResourceUsage.Staging ? D3D11.BindFlags.ConstantBuffer : D3D11.BindFlags.None,
            OptionFlags = D3D11.ResourceOptionFlags.None,
            CpuAccessFlags = cpuFlags,
            StructureByteStride = 0
        };

        D3DResource = Native = ResourceFactory.Create(Graphics.D3DDevice, Name, in desc, initialData);
    }

    /// <summary>
    /// Function to retrieve a copy of this buffer as a staging resource.
    /// </summary>
    /// <returns>The staging buffer to retrieve.</returns>
    protected override GorgonBufferCommon GetStagingInternal() => GetStaging();

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Objects that override this method should be sure to call this base method or else a memory leak may occur.
    /// </para>
    /// </remarks>
    public override void Dispose()
    {
        List<GorgonConstantBufferView> cbvs = Interlocked.Exchange(ref _cbvs, null);

        if (cbvs is null)
        {
            base.Dispose();
            return;
        }

        // Clean up the cached views.
        foreach (GorgonConstantBufferView cbv in cbvs)
        {
            cbv.Dispose();
        }

        base.Dispose();
    }

    /// <summary>
    /// Function to retrieve a view of the constant buffer elements to pass to a shader.
    /// </summary>
    /// <param name="firstElement">[Optional] The index of the first element in the buffer to view.</param>
    /// <param name="elementCount">[Optional] The number of elements to view.</param>
    /// <returns>A new <see cref="GorgonConstantBufferView"/> used to map a portion of a constant buffer to a shader.</returns>
    /// <remarks>
    /// <para>
    /// This will create a view of the buffer so that a shader can access a portion (or all, if the buffer is less than or equal to 4096 constants in size) of a constant buffer. Shaders can only access
    /// up to 4096 constants in a constant buffer, and this allows us to bind and use a much larger buffer to the shader while still respecting the maximum constant accessibility.
    /// </para>
    /// <para>
    /// A single constant buffer constant is a float4 value (4 floating point values, or 16 bytes). 
    /// </para>
    /// <para>
    /// The <paramref name="firstElement"/> parameter must be between 0 and the total element count (minus one) of the buffer.  If it is not it will be constrained to those values to ensure there is no 
    /// out of bounds access to the buffer.  
    /// </para>
    /// <para>
    /// If the <paramref name="elementCount"/> parameter is omitted (or less than 1), then the remainder of the buffer is mapped to the view up to 256 elements (4096 constants, or 65536 bytes). If it 
    /// is provided, then the number of elements will be mapped to the view, up to a maximum of 256 elements.  If the value exceeds 256, then it will be constrained to 256.
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// Due to the nature of constant buffers on GPU hardware, these views are not aligned to a constant (which is a float4, or 16 bytes), but rather aligned to 16 constants (16 float4 values, or 256 
    /// bytes). This requires that your buffer be set up to be a multiple of 256 bytes in its <see cref="IGorgonConstantBufferInfo.SizeInBytes"/>.  This makes each element in the view the same as 16 float4
    /// values (or 256 bytes). That means when an offset of 2, and a count of 4 is set in the view, it is actually at an offset of 32 float4 values (512 bytes), and covers a range of 64 float4 values
    /// (1024 bytes). Because of this, care should be taken to ensure the buffer matches this alignment if constant buffer offsets/counts are to be used in your application.
    /// </para>
    /// <para>
    /// If no offsetting into the buffer is required, then the above information is not applicable and the method can be called with its default parameters (i.e. no parameters).
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    public GorgonConstantBufferView GetView(int firstElement = 0, int elementCount = 0)
    {
        var result = new GorgonConstantBufferView(this, firstElement, elementCount);
        _cbvs.Add(result);
        return result;
    }

    /// <summary>
    /// Function to retrieve a copy of this buffer as a staging resource.
    /// </summary>
    /// <returns>The staging buffer to retrieve.</returns>
    public GorgonConstantBuffer GetStaging()
    {
        var buffer = new GorgonConstantBuffer(Graphics,
                                                new GorgonConstantBufferInfo(_info)
                                                {
                                                    Name = $"{Name} Staging",
                                                    Usage = ResourceUsage.Staging
                                                });

        CopyTo(buffer);

        return buffer;
    }
    #endregion

    #region Constructor/Finalizer.
    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonConstantBuffer" /> class.
    /// </summary>
    /// <param name="graphics">The <see cref="GorgonGraphics"/> object used to create and manipulate the buffer.</param>
    /// <param name="info">Information used to create the buffer.</param>
    /// <param name="initialData">[Optional] The initial data used to populate the buffer.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, or <paramref name="info"/> parameters are <b>null</b>.</exception>
    /// <exception cref="GorgonException">
    /// Thrown when the size of the constant buffer exceeds the maximum constant buffer size. See <see cref="IGorgonVideoAdapterInfo.MaxConstantBufferSize"/> to determine the maximum size of a constant buffer.
    /// </exception>
    public GorgonConstantBuffer(GorgonGraphics graphics, GorgonConstantBufferInfo info, ReadOnlySpan<byte> initialData = default)
        : base(graphics)
    {
        if (info is null)
        {
            throw new ArgumentNullException(nameof(info));
        }

        _info = new GorgonConstantBufferInfo(info);

        Initialize(initialData);
    }
    #endregion
}