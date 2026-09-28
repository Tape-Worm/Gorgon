
// 
// Gorgon
// Copyright (C) 2025 Michael Winsor
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
// Created: July 29, 2016 7:31:42 PM
// 

using TerraFX.Interop.DirectX;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Information used to define the parameters for the stencil portion of a <see cref="GorgonDepthStencilState"/>.
/// </summary>
public sealed record class GorgonStencilOperation    
{
    /// <summary>
    /// Property to return the comparison function to use for stencil testing.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This specifies the function used to compare the stencil reference value against the existing value in the stencil buffer. Both values are masked with the <see cref="ReadMask"/> before the comparison is 
    /// made. If the comparison evaluates to <b>true</b>, then the stencil test passes.
    /// </para>
    /// <para>
    /// The default value is <see cref="ComparisonFunction.Always"/>.
    /// </para>
    /// </remarks>
    public ComparisonFunction StencilFunction
    {
        get;
        init;
    } = ComparisonFunction.Always;

    /// <summary>
    /// Property to return the operation to perform when stencil testing passes, but depth testing fails.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The default value is <see cref="StencilOperation.Keep"/>.
    /// </para>
    /// </remarks>
    public StencilOperation DepthFailOperation
    {
        get;
        init;
    } = StencilOperation.Keep;

    /// <summary>
    /// Property to return the operation to perform when stencil testing fails.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The default value is <see cref="StencilOperation.Keep"/>.
    /// </para>
    /// </remarks>
    public StencilOperation FailOperation
    {
        get;
        init;
    } = StencilOperation.Keep;

    /// <summary>
    /// Property to return the operation to perform when both stencil testing and depth testing pass.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The default value is <see cref="StencilOperation.Keep"/>.
    /// </para>
    /// </remarks>
    public StencilOperation PassOperation
    {
        get;
        init;
    } = StencilOperation.Keep;

    /// <summary>
    /// Property to return the mask value used to select the bits to use with the <see cref="StencilFunction"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This value is used to select the bits to use with the <see cref="StencilFunction"/> on the reference value, and the stencil buffer value: 
    /// <![CDATA[pass = StencilFunction((ref & ReadMask), (stencilBufferValue & ReadMask))]]>. This implies that the bits marked as 1 are used, and 0 are ignored within the comparison.
    /// </para>
    /// <para>
    /// The default value is 0xff.
    /// </para>
    /// </remarks>
    public byte ReadMask
    {
        get;
        init;
    } = 0xff;

    /// <summary>
    /// Property to return the mask value used to select which bits can be updated by the stencil operations.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This value is used to select which bits can be written when using one of the stencil operations (<see cref="PassOperation"/>, <see cref="FailOperation"/>, or <see cref="DepthFailOperation"/>):
    /// <![CDATA[newValue = (computedValue & WriteMask) | (oldValue & ~WriteMask)]]>. This implies that bits marked as 1 will be written, and 0 will be ignored.
    /// </para>
    /// <para>
    /// The default value is 0xff.
    /// </para>
    /// </remarks>
    public byte WriteMask
    {
        get;
        init;
    } = 0xff;

    /// <summary>
    /// Function to retrieve the D3D12 description for the stencil operation.
    /// </summary>
    /// <returns>The D3D12 description object.</returns>
    internal D3D12_DEPTH_STENCILOP_DESC1 GetDesc() => new()
    {
        StencilFunc = (D3D12_COMPARISON_FUNC)StencilFunction,
        StencilDepthFailOp = (D3D12_STENCIL_OP)DepthFailOperation,
        StencilFailOp = (D3D12_STENCIL_OP)FailOperation,
        StencilPassOp = (D3D12_STENCIL_OP)PassOperation,
        StencilReadMask = ReadMask,
        StencilWriteMask = WriteMask
    };
}
