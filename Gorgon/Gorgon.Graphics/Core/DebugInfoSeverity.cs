// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: July 6, 2025 3:49:07 PM
//

using TerraFX.Interop.DirectX;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Defines the severity of a debug information message.
/// </summary>
public enum DebugInfoSeverity
{
    /// <summary>
    /// Message indicates data corruption.
    /// </summary>
    Corruption = D3D12_MESSAGE_SEVERITY.D3D12_MESSAGE_SEVERITY_CORRUPTION,
    /// <summary>
    /// Message indicates an error.
    /// </summary>
    Error = D3D12_MESSAGE_SEVERITY.D3D12_MESSAGE_SEVERITY_ERROR,
    /// <summary>
    /// Message indicates a warning.
    /// </summary>
    Warning = D3D12_MESSAGE_SEVERITY.D3D12_MESSAGE_SEVERITY_WARNING,
    /// <summary>
    /// Message indicates information.
    /// </summary>
    Information = D3D12_MESSAGE_SEVERITY.D3D12_MESSAGE_SEVERITY_INFO,
    /// <summary>
    /// A general message.
    /// </summary>
    Message = D3D12_MESSAGE_SEVERITY.D3D12_MESSAGE_SEVERITY_MESSAGE
}
