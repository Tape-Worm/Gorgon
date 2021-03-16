#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: April 6, 2018 3:01:32 PM
// 
#endregion

using System;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// Defines what functionality is available for a <see cref="BufferFormat"/> and a Compute Shader.
    /// </summary>
    [Flags]
    public enum ComputeShaderFormatSupport
    {
        /// <summary>
        /// No support.
        /// </summary>
        None = D3D11.ComputeShaderFormatSupport.None,
        /// <summary>
        /// <para>
        /// Format supports atomic add.
        /// </para>
        /// </summary>
        AtomicAdd = D3D11.ComputeShaderFormatSupport.AtomicAdd,
        /// <summary>
        /// <para>
        /// Format supports atomic bitwise operations.
        /// </para>
        /// </summary>
        AtomicBitwiseOperations = D3D11.ComputeShaderFormatSupport.AtomicBitwiseOperations,
        /// <summary>
        /// <para>
        /// Format supports atomic compare with store or exchange.
        /// </para>
        /// </summary>
        AtomCompareStoreOrExchange = D3D11.ComputeShaderFormatSupport.AtomicCompareStoreOrCompareExchange,
        /// <summary>
        /// <para>
        /// Format supports atomic exchange.
        /// </para>
        /// </summary>
        AtomicExchange = D3D11.ComputeShaderFormatSupport.AtomicExchange,
        /// <summary>
        /// <para>
        /// Format supports atomic min and max.
        /// </para>
        /// </summary>
        AtomicSignedMinMax = D3D11.ComputeShaderFormatSupport.AtomicSignedMinimumOrMaximum,
        /// <summary>
        /// <para>
        /// Format supports atomic unsigned min and max.
        /// </para>
        /// </summary>
        AtomicUnsignedMinMax = D3D11.ComputeShaderFormatSupport.AtomicUnsignedMinimumOrMaximum,
        /// <summary>
        /// <para>
        /// Format supports a typed load.
        /// </para>
        /// </summary>
        TypedLoad = D3D11.ComputeShaderFormatSupport.TypedLoad,
        /// <summary>
        /// <para>
        /// Format supports a typed store.
        /// </para>
        /// </summary>
        TypedStore = D3D11.ComputeShaderFormatSupport.TypedStore,
        /// <summary>
        /// <para>
        /// Format supports logic operations in blend state.
        /// </para>
        /// </summary>
        OutputMergerLogicOperation = D3D11.ComputeShaderFormatSupport.OutputMergerLogicOperation,
        /// <summary>
        /// <para>
        /// Format supports tiled resources.
        /// </para>
        /// </summary>
        Tiled = D3D11.ComputeShaderFormatSupport.Tiled,
        /// <summary>
        /// <para>
        /// Format supports shareable resources.
        /// </para>
        /// </summary>
        Shareable = D3D11.ComputeShaderFormatSupport.Shareable,
        /// <summary>
        /// <para>
        /// Format supports multi-plane overlays.
        /// </para>
        /// </summary>
        MultiplaneOverlay = D3D11.ComputeShaderFormatSupport.MultiplaneOverlay
    }
}
