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

using System.ComponentModel;

namespace Gorgon.Editor.UI
{
    /// <summary>
    /// A data context for a view and <see cref="IViewModel"/>
    /// </summary>
    /// <typeparam name="T">The type of view model to use with the view. Ideally an interface for the view model type should be used here.</typeparam>
    /// <remarks>
    /// <para>
    /// A data context is used to make the view react to changes to data. This is done by hooking the <see cref="INotifyPropertyChanged.PropertyChanged"/>, and 
    /// <see cref="INotifyPropertyChanging.PropertyChanging"/> events and retrieving the name of the property that has been changed and updating the view by modifying one or more controls on the view. 
    /// This is the cornerstone of MVVM (Model-View-ViewModel), and the Gorgon Editor uses (a rather bastardized version of) MVVM so the view can react to users changes. 
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// This pattern is not completely correct in the editor since WinForms is not really designed to use it, so some liberties have been taken. And, unfortunately, makes the job of building 
    /// a control for a plug in a little more work. The results are worth it however as the communication lines between view model and view are rigidly enforced and make it easier to maintain for larger 
    /// code bases.
    /// </para>
    /// </note>
    /// </para>
    /// <para>
    /// If you wish to know more about MVVM, please visit <a href="https://docs.microsoft.com/en-us/xamarin/xamarin-forms/enterprise-application-patterns/mvvm">here</a>. 
    /// </para>
    /// <para>
    /// This interface must be applied to views that wish to use a <see cref="IViewModel"/> as a data context. 
    /// </para>
    /// </remarks>
    public interface IDataContext<T>
        where T : class, IViewModel
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
        /// Function to assign a data context to the view as a view model.
        /// </summary>
        /// <param name="dataContext">The data context to assign.</param>
        /// <remarks>
        /// <para>
        /// Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.
        /// </para>
        /// </remarks>
        void SetDataContext(T dataContext);
        #endregion
    }
}
