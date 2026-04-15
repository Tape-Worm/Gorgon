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
/// Defines the category for a debug information message.
/// </summary>
public enum DebugInfoCategory
{
    /// <summary>
    /// An application defined category.
    /// </summary>
    ApplicationDefined = D3D12_MESSAGE_CATEGORY.D3D12_MESSAGE_CATEGORY_APPLICATION_DEFINED,
    /// <summary>
    /// Initialization category.
    /// </summary>
    Initialization = D3D12_MESSAGE_CATEGORY.D3D12_MESSAGE_CATEGORY_INITIALIZATION,
    /// <summary>
    /// Execution category.
    /// </summary>
    Execution = D3D12_MESSAGE_CATEGORY.D3D12_MESSAGE_CATEGORY_EXECUTION,
    /// <summary>
    /// Cleanup category.
    /// </summary>
    Cleanup = D3D12_MESSAGE_CATEGORY.D3D12_MESSAGE_CATEGORY_CLEANUP,
    /// <summary>
    /// Resource manipulation category.
    /// </summary>
    ResourceManipulation = D3D12_MESSAGE_CATEGORY.D3D12_MESSAGE_CATEGORY_RESOURCE_MANIPULATION,
    /// <summary>
    /// Compilation category.
    /// </summary>
    Compilation = D3D12_MESSAGE_CATEGORY.D3D12_MESSAGE_CATEGORY_COMPILATION,
    /// <summary>
    /// Miscellaneous category.
    /// </summary>
    Misc = D3D12_MESSAGE_CATEGORY.D3D12_MESSAGE_CATEGORY_MISCELLANEOUS,
    /// <summary>
    /// Shader category.
    /// </summary>
    Shader = D3D12_MESSAGE_CATEGORY.D3D12_MESSAGE_CATEGORY_SHADER,
    /// <summary>
    /// State creation category.
    /// </summary>
    StateCreation = D3D12_MESSAGE_CATEGORY.D3D12_MESSAGE_CATEGORY_STATE_CREATION,
    /// <summary>
    /// State retrieval category.
    /// </summary>
    StateRetrieval = D3D12_MESSAGE_CATEGORY.D3D12_MESSAGE_CATEGORY_STATE_GETTING,
    /// <summary>
    /// State update category.
    /// </summary>
    StateUpdate = D3D12_MESSAGE_CATEGORY.D3D12_MESSAGE_CATEGORY_STATE_SETTING    
}
