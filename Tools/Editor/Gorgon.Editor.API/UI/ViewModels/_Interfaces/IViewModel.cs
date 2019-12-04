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
// Created: August 26, 2018 12:05:43 PM
// 
#endregion

using System;
using System.ComponentModel;

namespace Gorgon.Editor.UI
{
    /// <summary>
    /// The base interface for Gorgon Editor view models.
    /// </summary>
    public interface IViewModel
        : INotifyPropertyChanged, INotifyPropertyChanging
    {
        #region Events.
        /// <summary>
        /// Event triggered when a wait overlay panel needs to be activated.
        /// </summary>
        event EventHandler<WaitPanelActivateArgs> WaitPanelActivated;

        /// <summary>
        /// Event triggered when a wait overlay panel needs to be deactivated.
        /// </summary>
        event EventHandler WaitPanelDeactivated;

        /// <summary>
        /// Event triggered when the progress overlay panel over needs to be updated.
        /// </summary>
        event EventHandler<ProgressPanelUpdateArgs> ProgressUpdated;

        /// <summary>
        /// Event triggered when the progress overlay should be deactivated.
        /// </summary>
        event EventHandler ProgressDeactivated;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the clipboard handler for this view model.
        /// </summary>
        IClipboardHandler Clipboard
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to notify when a property has been changed.
        /// </summary>
        /// <param name="propertyName">Name of the property to change.</param>
        /// <remarks>
        /// <para>
        /// This method used to notify when a property has changed outside of the property setter, or if a property other than the current property has changed inside of a property setter. The 
        /// user can specify the name of the property manually through the <paramref name="propertyName"/> parameter. 
        /// </para>
        /// </remarks>
        void NotifyPropertyChanged(string propertyName);

        /// <summary>
        /// Function to notify before a property is changed.
        /// </summary>
        /// <param name="propertyName">Name of the property to change.</param>
        /// <remarks>
        /// <para>
        /// This method is used to notify before a property is changed outside of the property setter, or if a property other than the current property is changing inside of a property setter. The 
        /// user can specify the name of the property manually through the <paramref name="propertyName"/> parameter.
        /// </para>
        /// </remarks>
        void NotifyPropertyChanging(string propertyName);

        /// <summary>
        /// Function called when the associated view is loaded.
        /// </summary>
        void OnLoad();

        /// <summary>
        /// Function called when the associated view is unloaded.
        /// </summary>
        void OnUnload();
        #endregion
    }
}