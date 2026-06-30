// Gorgon.
// Copyright (C) 2026 Michael Winsor
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
// Created: June 29, 2026 5:23:57 PM
//

using TerraFX.Interop.DirectX;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Defines how line primitives are rasterized.
/// </summary>
public enum LineRasterizationMode
{
    /// <summary>
    /// Standard line drawing is applied. No antialiasing.
    /// </summary>
    Default = D3D12_LINE_RASTERIZATION_MODE.D3D12_LINE_RASTERIZATION_MODE_ALIASED,
    /// <summary>
    /// Alpha based antialiasing. 
    /// </summary>
    AntiAliased = D3D12_LINE_RASTERIZATION_MODE.D3D12_LINE_RASTERIZATION_MODE_ALPHA_ANTIALIASED,
    /// <summary>
    /// Line is expanded into a quad between the range of 1.0 and 1.4. This value only applies to render targets that use multisampling.
    /// </summary>
    QuadrilateralWide = D3D12_LINE_RASTERIZATION_MODE.D3D12_LINE_RASTERIZATION_MODE_QUADRILATERAL_WIDE,
    /// <summary>
    /// Line is expanded into a quad, but uses a true 1.0 width. This value only applies to render targets that use multisampling.
    /// </summary>
    QuadrilateralNarrow = D3D12_LINE_RASTERIZATION_MODE.D3D12_LINE_RASTERIZATION_MODE_QUADRILATERAL_NARROW,
}
