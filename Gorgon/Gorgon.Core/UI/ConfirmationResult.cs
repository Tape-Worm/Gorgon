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
// Created: March 12, 2025 1:40:47 PM
//

namespace Gorgon.UI;

/// <summary>
/// Confirmation dialog result values
/// </summary>
/// <remarks>
/// The <c>Yes</c> and <c>No</c> fields can be OR'd with the <c>ToAll</c> field to indicate "Yes to all", or "No to all"
/// </remarks>
[Flags]
public enum ConfirmationResult
{
    /// <summary>
    /// No confirmation.
    /// </summary>
    None = 0,
    /// <summary>
    /// Yes clicked.
    /// </summary>
    Yes = 1,
    /// <summary>
    /// No clicked.
    /// </summary>
    No = 2,
    /// <summary>
    /// Cancel clicked.
    /// </summary>
    Cancel = 4,
    /// <summary>
    /// To all checked.
    /// </summary>
    ToAll = 8
}
