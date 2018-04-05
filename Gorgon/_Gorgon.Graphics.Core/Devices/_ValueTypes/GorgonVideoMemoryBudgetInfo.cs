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
// Created: November 6, 2017 5:57:01 PM
// 
#endregion

using DXGI = SharpDX.DXGI;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// Information about a video memory budget.
    /// </summary>
    public struct GorgonVideoMemoryBudgetInfo 
    {
        #region Variables.
        /// <summary>
        /// The amount of memory budgeted by the operating system, in bytes.
        /// </summary>
        /// <remarks>
        /// If the <see cref="Used"/> value is larger than this value, then stuttering may occur because memory is overcommitted.
        /// </remarks>
        public readonly long Budget;

        /// <summary>
        /// The amount of memory used within the budget, in bytes.
        /// </summary>
        /// <remarks>
        /// If this value exceeds the value stored in <see cref="Budget"/>, then stuttering may occur because memory is overcommitted.
        /// </remarks>
        public readonly long Used;

        /// <summary>
        /// The amount of memory available for reservation, in bytes.
        /// </summary>
        public readonly long Available;

        /// <summary>
        /// The amount of memory that is reserved by the application, in bytes. 
        /// </summary>
        /// <remarks>
        /// This value is used as a hint by the operating system to determine the application's minimum working set. Video memory usage should be kept under this amount for best performance.
        /// </remarks>
        public readonly long Reserved;
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonVideoMemoryBudgetInfo"/> struct.
        /// </summary>
        /// <param name="info">The DXGI video memory information.</param>
        internal GorgonVideoMemoryBudgetInfo(ref DXGI.QueryVideoMemoryInformation info)
        {
            Budget = info.Budget;
            Used = info.CurrentUsage;
            Available = info.AvailableForReservation;
            Reserved = info.CurrentReservation;
        }
        #endregion
    }
}
