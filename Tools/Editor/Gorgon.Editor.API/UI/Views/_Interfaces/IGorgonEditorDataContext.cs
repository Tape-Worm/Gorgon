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
// Created: August 26, 2018 12:07:23 PM
// 
#endregion

namespace Gorgon.Editor.UI
{
    /// <summary>
    /// A data context for a view and <see cref="IViewModel"/>
    /// </summary>
    /// <typeparam name="T">The type of view model to use with the view.</typeparam>
    /// <remarks>
    /// <para>
    /// This interface must be applied to views that wish to use a <see cref="IViewModel"/> as a data context. 
    /// </para>
    /// </remarks>
    public interface IDataContext<T>
        where T : IViewModel
    {
        #region Properties.
        /// <summary>
        /// Property to return the data context assigned to this view.
        /// </summary>
        T DataContext
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to assign a data context to the view.
        /// </summary>
        /// <param name="dataContext">The data context to assign.</param>
        void SetDataContext(T dataContext);
        #endregion
    }
}
