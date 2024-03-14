
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: November 11, 2018 12:43:46 AM
// 


using System.Reflection;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.Properties;

namespace Gorgon.Editor.UI;

/// <summary>
/// A factory used to create views based on view model types
/// </summary>
/// <remarks>
/// <para>
/// When developing a plug in with a UI, developers have to register their views and view models with the system so that the host application can build up the UI and assign it to the data context. 
/// </para>
/// </remarks>
public static class ViewFactory
{

    // A list of view builders used to create views.
    private static readonly Dictionary<string, Func<Control>> _viewBuilders = new(StringComparer.OrdinalIgnoreCase);



    /// <summary>Function to register the specified view model with a view builder.</summary>
    /// <typeparam name="T">The type of view model. Must implement <see cref="IViewModel"/>.</typeparam>
    /// <param name="constructor">The function that will create and return the view.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="constructor"/> parameter is <b>null</b>.</exception>
    /// <remarks>
    /// <para>
    /// Content plug in developers must call this so that the UI will be created by the host application. Developers will pass in a function to the <paramref name="constructor"/> that will be used to 
    /// create the view object (must inherit from <see cref="Control"/>). Typically, this is just a call to <c>new</c> on the object type, although other initialization steps may be passed into the 
    /// callback function.
    /// </para>
    /// <para>
    /// For best results, this should be called as early in the plug in initialization cycle as possible, typically in the <see cref="ContentPlugIn.OnInitialize"/> method (if the plug in has such a 
    /// method).
    /// </para>
    /// </remarks>
    /// <seealso cref="ContentPlugIn"/>
    /// <seealso cref="ToolPlugIn"/>
    public static void Register<T>(Func<Control> constructor)
        where T : IViewModel => _viewBuilders[typeof(T).AssemblyQualifiedName] = constructor ?? throw new ArgumentNullException(nameof(constructor));

    /// <summary>Function to unregisters the specified view model type.</summary>
    /// <typeparam name="T">The type of view model. Must implement <see cref="IViewModel"/>.</typeparam>
    /// <remarks>
    /// <para>
    /// When the plug in is shut down (typically when the plug in UI is closed), this method should be called to remove the registration. This is typically done in the 
    /// <see cref="ContentPlugIn.OnShutdown"/> method.
    /// </para>
    /// </remarks>
    /// <seealso cref="ContentPlugIn"/>
    /// <seealso cref="ToolPlugIn"/>
    public static void Unregister<T>()
        where T : IViewModel => _viewBuilders.Remove(typeof(T).AssemblyQualifiedName);

    /// <summary>
    /// Function to assign a view model to the specified control.
    /// </summary>
    /// <param name="viewModel">The view model to assign.</param>
    /// <param name="control">The control that will take the view model.</param>
    /// <remarks>
    /// <para>
    /// This will assign a <paramref name="viewModel"/> to the given <paramref name="control"/> (if the control implements <see cref="IDataContext{T}"/>). Users should not need to call this method as 
    /// it will be done by the editor during plug in UI initialization.
    /// </para>
    /// </remarks>
    public static void AssignViewModel(IViewModel viewModel, Control control)
    {
        if (control is null)
        {
            throw new ArgumentNullException(nameof(control));
        }

        Type dataContextType = typeof(IDataContext<>);
        Type controlType = control.GetType();
        Type controlInterface = controlType.GetInterface(dataContextType.FullName);

        if (controlInterface is null)
        {
            return;
        }

        MethodInfo method = controlInterface.GetMethod("SetDataContext");
        method?.Invoke(control, new[] { viewModel });
    }

    /// <summary>
    /// Function to retrieve the view model assigned to a control.
    /// </summary>
    /// <param name="control">The control to evaluate.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="control"/> parameter is <b>null</b>.</exception>
    /// <exception cref="InvalidCastException">Thrown if the <paramref name="control"/> parameter does not implement <see cref="IDataContext{T}"/>.</exception>
    /// <remarks>
    /// <para>
    /// This will return the view model assigned to the specified <paramref name="control"/>. If the control does not implement <see cref="IDataContext{T}"/>, then an exception will be thrown.
    /// </para>
    /// </remarks>
    public static IViewModel GetViewModel(Control control)
    {
        if (control is null)
        {
            throw new ArgumentNullException(nameof(control));
        }

        Type dataContextType = typeof(IDataContext<>);
        Type controlType = control.GetType();
        Type controlInterface = controlType.GetInterface(dataContextType.FullName) ?? throw new InvalidCastException(string.Format(Resources.GOREDIT_ERR_HOSTED_CTL_NOT_DATACONTEXT, control.Name));

        PropertyInfo property = controlInterface.GetProperty("DataContext");
        return property?.GetValue(control) as IViewModel;
    }

    /// <summary>
    /// Function to determine if the view model type has a view registration.
    /// </summary>
    /// <param name="viewModel">The view model type to evaluate.</param>
    /// <returns><b>true</b> if registered, <b>false</b> if not.</returns>
    public static bool IsRegistered(IViewModel viewModel)
    {
        if (viewModel is null)
        {
            return false;
        }

        Type viewModelType = viewModel.GetType();
        string typeName = viewModel.GetType().AssemblyQualifiedName;

        if (_viewBuilders.ContainsKey(typeName))
        {
            return true;
        }

        Type[] interfaces = viewModelType.GetInterfaces();

        Type interfaceType = interfaces.FirstOrDefault(item => _viewBuilders.ContainsKey(item.AssemblyQualifiedName));

        return interfaceType is not null;
    }

    /// <summary>
    /// Function to create a new view based on the view model type passed in.
    /// </summary>
    /// <typeparam name="T">The type of view. Must inherit from <see cref="Control"/>.</typeparam>
    /// <param name="viewModel">The view model that is associated with the view.</param>
    /// <returns>The view for the view model.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="viewModel"/> parameter is <b>null</b>.</exception>
    /// <remarks>
    /// <para>
    /// This method creates a view registered to a <paramref name="viewModel"/>. Users should never need to call this method, the editor will build the view on behalf of the plug in.
    /// </para>
    /// </remarks>
    public static T CreateView<T>(IViewModel viewModel)
        where T : Control
    {
        if (viewModel is null)
        {
            throw new ArgumentNullException(nameof(viewModel));
        }

        Type viewModelType = viewModel.GetType();
        string typeName = viewModel.GetType().AssemblyQualifiedName;

        if (_viewBuilders.TryGetValue(typeName, out Func<Control> constructor))
        {
            return (T)constructor();
        }

        Type[] interfaces = viewModelType.GetInterfaces();

        Type interfaceType = interfaces.FirstOrDefault(item => _viewBuilders.ContainsKey(item.AssemblyQualifiedName));

        return interfaceType is null
            ? throw new KeyNotFoundException(string.Format(Resources.GOREDIT_ERR_CANNOT_FIND_VIEW_FACTORY, typeName))
            : (T)_viewBuilders[interfaceType.AssemblyQualifiedName]();
    }

}
