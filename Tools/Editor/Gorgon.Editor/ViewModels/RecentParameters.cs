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
// Created: October 28, 2018 4:05:50 PM
// 
#endregion


namespace Gorgon.Editor.ViewModels
{
    /// <summary>
    /// Parameters for the <see cref="Recent"/> view model.
    /// </summary>
    internal class RecentParameters
        : ViewModelCommonParameters
    {
        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the RecentVmParameters class.</summary>
        /// <param name="factory">The view model factory used to create the view model.</param>
        public RecentParameters(ViewModelFactory factory)
            : base(factory)
        {

        }
        #endregion
    }
}
