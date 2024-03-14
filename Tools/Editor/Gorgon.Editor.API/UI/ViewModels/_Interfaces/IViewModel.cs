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

using System.ComponentModel;

namespace Gorgon.Editor.UI;

/// <summary>
/// A base view model interface for the gorgon editor.
/// </summary>
/// <remarks>
/// <para>
/// This is the base class for all view models used by the editor and provides the bare minimum in functionality. This object already implements the <see cref="INotifyPropertyChanged"/> 
/// and the <see cref="INotifyPropertyChanging"/> interfaces to allow communication with a view. 
/// </para>
/// <para>
/// When implementing a view model, the developers should set up their properties like this:
/// <code lang="csharp">
/// <![CDATA[
/// public ReturnType PropertyName
/// {
///     get => _backingStoreValue;
///     set
///     {
///         // Always check to see if the value has changed. This keeps the view model from being too "chatty" with the UI.
///         if (_backingStoreValue == value)
///         {
///             return;
///         }
///         
///         // Notify that the property is about to change. This allows the view to do any necessary clean up prior to updating the visual side.
///         OnPropertyChanging();
///         _backingStoreValue = value;
///         // Now, notify that the property has changed. The view will intercept the change and update the visual associated with the property.
///         OnPropertyChanged();
///     }
/// }
/// ]]>
/// </code>
/// This setup notifies the view that the property has been updated, and that any associated visual should probably update as well. This is the most common pattern to use, however there will 
/// be times when a property notification is required from a method. If that is the case, the <see cref="PropertyMonitor.NotifyPropertyChanged(string)"/>, and 
/// <see cref="PropertyMonitor.NotifyPropertyChanging(string)"/> methods should be used like this:
/// <code lang="csharp">
/// <![CDATA[
/// // This could be a callback function for an IEditorCommand<T> object.
/// private void DoCommandAction()
/// {
///     NotifyPropertyChanging(nameof(ReadOnlyValue));
///     _readOnlyValue++;
///     NotifyPropertyChanged(nameof(ReadOnlyValue));
/// }
/// ]]>
/// The difference being that for properties, you do not need to specify the name of the property being updated (the compiler figures it out), and in methods you do. 
/// </code>
/// </para>
/// <para>
/// The view model is also equipped with several events that are used to notify the application that a long running operation is executing. Applications can intercept these events and display a 
/// progress panel, or "please wait" panel. These should only be used with asynchronous operations as they will not update correctly if everything is running on the same thread.
/// </para>
/// </remarks>
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
    /// Function to notify that all properties on this type are changing their values.
    /// </summary>
    void NotifyAllPropertiesChanging();

    /// <summary>
    /// Function to notify that all properties on this type have changed their values.
    /// </summary>
    void NotifyAllPropertiesChanged();

    /// <summary>
    /// Function called when the associated view is loaded.
    /// </summary>
    void Load();

    /// <summary>
    /// Function called when the associated view is unloaded.
    /// </summary>
    void Unload();
    #endregion
}
