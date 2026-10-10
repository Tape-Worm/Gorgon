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
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: September 20, 2026 4:04:44 PM
//

namespace Gorgon.Graphics.Core;

/// <summary>
/// Defines specific methods required for 
/// </summary>
/// <typeparam name="TS">The type of PSO state.</typeparam>
internal interface IPsoState<TS>
{
    /// <summary>
    /// Function to determine if a given state matches one of the predefined states.
    /// </summary>
    /// <param name="state">The state to evaluate.</param>
    /// <returns>If the state matches one of the predefined states, then one of the predefined states is returned. Otherwise, the <paramref name="state"/> parameter is returned.</returns>
    /// <remarks>
    /// <para>
    /// Use this method to determine whether a state matches one of the predefined states for the state type. This allows applications to cut down duplicating state objects.
    /// </para>
    /// </remarks>
    abstract static TS GetDefinedState(TS state);
}
