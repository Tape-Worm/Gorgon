﻿
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: July 21, 2016 11:05:38 AM
// 

using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using D3D11 = SharpDX.Direct3D11;
using DX = SharpDX;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Base class for shader resource views
/// </summary>
/// <remarks>
/// <para>
/// This base class is used to define shader resource views for strongly typed resources like textures and buffers
/// </para>
/// </remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="GorgonShaderResourceView"/> class
/// </remarks>
/// <param name="resource">The resource to bind to the view.</param>
/// <exception cref="ArgumentNullException">Thrown when the <paramref name="resource"/> parameter is <b>null</b>.</exception>
public abstract class GorgonShaderResourceView(GorgonGraphicsResource resource)
        : GorgonResourceView(resource), IEquatable<GorgonShaderResourceView>
{

    // The shader resource view descriptor.
    private D3D11.ShaderResourceViewDescription1 _srvDesc;

    /// <summary>
    /// Property to return a reference to the shader resource view descriptor.
    /// </summary>
    private protected ref D3D11.ShaderResourceViewDescription1 SrvDesc => ref _srvDesc;

    /// <summary>
    /// Property to return the native Direct 3D 11 view.
    /// </summary>
    internal D3D11.ShaderResourceView1 Native
    {
        get;
        set;
    }

    /// <summary>Function to perform the creation of a specific kind of view.</summary>
    /// <returns>The view that was created.</returns>
    private protected sealed override D3D11.ResourceView OnCreateNativeView()
    {
        Graphics.Log.Print($"Creating D3D11 {Resource.D3DResource.Dimension} shader resource view for {Resource.Name}.", LoggingLevel.Simple);

        ref readonly D3D11.ShaderResourceViewDescription1 desc = ref OnGetSrvParams();

        try
        {
            Native = new D3D11.ShaderResourceView1(Graphics.D3DDevice, Resource.D3DResource, desc)
            {
                DebugName = $"{Resource.Name}_SRV"
            };

            Graphics.Log.Print($"Shader Resource View for '{Resource.Name}': {Resource.ResourceType} -> Start: {desc.Buffer.ElementOffset}, Count: {desc.Buffer.ElementCount}", LoggingLevel.Verbose);

            return Native;
        }
        catch (DX.SharpDXException sDXEx)
        {
            if ((uint)sDXEx.ResultCode.Code == 0x80070057)
            {
                throw new GorgonException(GorgonResult.CannotCreate,
                                          string.Format(Resources.GORGFX_ERR_VIEW_CANNOT_CAST_FORMAT,
                                                        (BufferFormat)desc.Format));
            }

            throw;
        }
    }

    /// <summary>
    /// Function to retrieve the necessary parameters to create the native view.
    /// </summary>
    /// <returns>A shader resource view descriptor.</returns>
    private protected abstract ref readonly D3D11.ShaderResourceViewDescription1 OnGetSrvParams();

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public override void Dispose()
    {
        Native = null;
        base.Dispose();
    }

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns><see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
    public bool Equals(GorgonShaderResourceView other) => base.Equals(other);

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="obj">An object to compare with this object.</param>
    /// <returns><see langword="true" /> if the current object is equal to the <paramref name="obj" /> parameter; otherwise, <see langword="false" />.</returns>
    public override bool Equals(object obj) => Equals(obj as GorgonShaderResourceView);

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
    /// </returns>
    public override int GetHashCode() => base.GetHashCode();

}
