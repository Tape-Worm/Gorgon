#region MIT
// 
// Gorgon.
// Copyright (C) 2017 Michael Winsor
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
// Created: July 22, 2017 10:16:16 AM
// 
#endregion

using System.Numerics;
using Gorgon.Diagnostics;
using D3D11 = SharpDX.Direct3D11;
using DX = SharpDX;

namespace Gorgon.Graphics.Core;

/// <summary>
/// An readable and writable (unordered access) view for a <see cref="GorgonGraphicsResource"/>.
/// </summary>
/// <remarks>
/// <para>
/// This type of view allows for unordered access to a <see cref="GorgonGraphicsResource"/> like one of the <see cref="GorgonBuffer"/> types, a texture, etc... Any resource that needs a 
/// unordered access must be created with the the <c>UnorderedAccess</c> flag.
/// </para>
/// <para>
/// The unordered access allows a shader to read/write any part of a <see cref="GorgonGraphicsResource"/> by multiple threads without memory contention. This is done through the use of 
/// <a target="_blank" href="https://msdn.microsoft.com/en-us/library/windows/desktop/ff476334(v=vs.85).aspx">atomic functions</a>.
/// </para>
/// <para>
/// These types of views are most useful for <see cref="GorgonComputeShader"/> shaders, but can also be used by a <see cref="GorgonPixelShader"/> by passing a list of these views in to a 
/// <see cref="GorgonDrawCallCommon">draw call</see>.
/// </para>
/// <para>
/// <note type="warning">
/// <para>
/// Unordered access views do not support a multisampled <see cref="GorgonTexture2D"/>.
/// </para>
/// </note>
/// </para>
/// </remarks>
/// <seealso cref="GorgonGraphicsResource"/>
/// <seealso cref="GorgonBuffer"/>
/// <seealso cref="GorgonTexture2D"/>
/// <seealso cref="GorgonComputeShader"/>
/// <seealso cref="GorgonPixelShader"/>
/// <seealso cref="GorgonDrawCallCommon"/>
/// <remarks>
/// Initializes a new instance of the <see cref="GorgonShaderResourceView"/> class.
/// </remarks>
/// <param name="resource">The resource to bind to the view.</param>
/// <exception cref="ArgumentNullException">Thrown when the <paramref name="resource"/> parameter is <b>null</b>.</exception>
public abstract class GorgonReadWriteView(GorgonGraphicsResource resource)
        : GorgonResourceView(resource)
{
    #region Variables.
    // The D3D11 UAV descriptor.
    private D3D11.UnorderedAccessViewDescription1 _uavDesc;
    #endregion

    #region Properties.
    /// <summary>
    /// Property to return a reference to the D3D11 UAV descriptor.
    /// </summary>
    private protected ref D3D11.UnorderedAccessViewDescription1 UavDesc => ref _uavDesc;

    /// <summary>
    /// Property to return the native Direct 3D 11 view.
    /// </summary>
    internal D3D11.UnorderedAccessView1 Native
    {
        get;
        private set;
    }
    #endregion

    #region Methods.
    /// <summary>
    /// Function to perform the creation of a specific kind of view.
    /// </summary>
    /// <returns>The view that was created.</returns>
    private protected sealed override D3D11.ResourceView OnCreateNativeView()
    {
        ref readonly D3D11.UnorderedAccessViewDescription1 desc = ref OnGetUavParams();

        Graphics.Log.Print($"Creating D3D11 {Resource.D3DResource.Dimension} unordered access view for {Resource.Name}.", LoggingLevel.Simple);

        Native = new D3D11.UnorderedAccessView1(Graphics.D3DDevice, Resource.D3DResource, desc)
        {
            DebugName = $"{Resource.Name}_UAV"
        };

        Graphics.Log.Print($"Unordered Access View for '{Resource.Name}': {Resource.ResourceType} -> Start: {desc.Buffer.FirstElement}, Count: {desc.Buffer.ElementCount}", LoggingLevel.Verbose);

        return Native;
    }

    /// <summary>
    /// Function to retrieve the necessary parameters to create the native view.
    /// </summary>
    /// <returns>The D3D11 UAV descriptor.</returns>
    private protected abstract ref readonly D3D11.UnorderedAccessViewDescription1 OnGetUavParams();

    /// <summary>
    /// Function to clear the unordered access value with the specified values.
    /// </summary>
    /// <param name="value1">First value.</param>
    /// <param name="value2">Second value.</param>
    /// <param name="value3">Third value.</param>
    /// <param name="value4">Fourth value.</param>
    /// <remarks>
    /// <para>
    /// This method will copy the lower n[i] bits (where n is the number of bits in a channel, i is the index of the channel) to the proper channel.
    /// </para>
    /// <para>
    /// This method works on any unordered access view that does not require format conversion.  Unordered access views for raw/structured buffers only use the first value.
    /// </para>
    /// </remarks>
    public void Clear(int value1, int value2, int value3, int value4) => Resource.Graphics.D3DDeviceContext.ClearUnorderedAccessView(Native, new DX.Int4(value1, value2, value3, value4));

    /// <summary>
    /// Function to clear the unordered access value with the specified value.
    /// </summary>
    /// <param name="value">Value used to clear.</param>
    /// <remarks>
    /// <para>
    /// This method will copy the lower n[i] bits (where n is the number of bits in a channel, i is the index of the channel) to the proper channel.
    /// </para>
    /// <para>
    /// This method works on any unordered access view that does not require format conversion.
    /// </para>
    /// </remarks>
    public void Clear(int value) => Resource.Graphics.D3DDeviceContext.ClearUnorderedAccessView(Native, new DX.Int4(value));

    /// <summary>
    /// Function to clear the unordered access value with the specified values.
    /// </summary>
    /// <param name="value1">First value.</param>
    /// <param name="value2">Second value.</param>
    /// <param name="value3">Third value.</param>
    /// <param name="value4">Fourth value.</param>
    /// <remarks>
    /// <para>
    /// This method will copy the lower n[i] bits (where n is the number of bits in a channel, i is the index of the channel) to the proper channel.
    /// </para>
    /// <para>
    /// This method works on any unordered access view that does not require format conversion.  Unordered access views for raw/structured buffers only use the first value.
    /// </para>
    /// </remarks>
    public void Clear(float value1, float value2, float value3, float value4) => Resource.Graphics.D3DDeviceContext.ClearUnorderedAccessView(Native, 
                                                                             new DX.Mathematics.Interop.RawInt4((int)value1, (int)value2, (int)value3, (int)value4));

    /// <summary>
    /// Function to clear the unordered access value with the specified value.
    /// </summary>
    /// <param name="value">Value used to clear.</param>
    /// <remarks>
    /// <para>
    /// This method will copy the lower n[i] bits (where n is the number of bits in a channel, i is the index of the channel) to the proper channel.
    /// </para>
    /// <para>
    /// This method works on any unordered access view that does not require format conversion.
    /// </para>
    /// </remarks>
    public void Clear(float value) => Resource.Graphics.D3DDeviceContext.ClearUnorderedAccessView(Native, new DX.Mathematics.Interop.RawInt4((int)value, (int)value, (int)value, (int)value));

    /// <summary>
    /// Function to clear the unordered access value with the specified values.
    /// </summary>
    /// <param name="values">Values used to clear.</param>
    /// <remarks>
    /// <para>
    /// This method will copy the lower n[i] bits (where n is the number of bits in a channel, i is the index of the channel) to the proper channel.
    /// </para>
    /// <para>
    /// This method works on any unordered access view that does not require format conversion.  Unordered access views for raw/structured buffers only use the first value in the vector.
    /// </para>
    /// </remarks>
    public void Clear(Vector4 values) => Resource.Graphics.D3DDeviceContext.ClearUnorderedAccessView(Native, new DX.Mathematics.Interop.RawInt4((int)values.X, (int)values.Y, (int)values.Z, (int)values.W));

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public override void Dispose()
    {
        Native = null;
        base.Dispose();
    }

    #endregion
}
