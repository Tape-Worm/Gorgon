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
// Created: November 11, 2018 12:43:46 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Gorgon.Editor.Properties;

namespace Gorgon.Editor.UI
{
    /// <summary>
    /// A factory used to create views based on view model types.
    /// </summary>
    public static class ViewFactory
    {
        #region Variables.
        // A list of view builders used to create views.
        private static readonly Dictionary<string, Func<Control>> _viewBuilders = new Dictionary<string, Func<Control>>(StringComparer.OrdinalIgnoreCase);
        #endregion

        #region Methods.
        /// <summary>Function to register the specified view model with a view builder.</summary>
        /// <typeparam name="T">The type of view model. Must implement <see cref="IViewModel"/>.</typeparam>
        /// <param name="constructor">The function that will create and return the view.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="constructor"/> parameter is <b>null</b>.</exception>
        public static void Register<T>(Func<Control> constructor)
            where T : IViewModel => _viewBuilders[typeof(T).AssemblyQualifiedName] = constructor ?? throw new ArgumentNullException(nameof(constructor));

        /// <summary>Function to unregisters the specified view model type.</summary>
        /// <typeparam name="T">The type of view model. Must implement <see cref="IViewModel"/>.</typeparam>
        public static void Unregister<T>()
            where T : IViewModel => _viewBuilders.Remove(typeof(T).AssemblyQualifiedName);

        /// <summary>
        /// Function to assign a view model to the specified control.
        /// </summary>
        /// <param name="viewModel">The view model to assign.</param>
        /// <param name="control">The control that will take the view model.</param>
        public static void AssignViewModel(IViewModel viewModel, Control control)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException(nameof(viewModel));
            }

            if (control == null)
            {
                throw new ArgumentNullException(nameof(control));
            }

            Type dataContextType = typeof(IDataContext<>);
            Type controlType = control.GetType();
            Type controlInterface = controlType.GetInterface(dataContextType.FullName);

            if (controlInterface == null)
            {
                return;
            }

            MethodInfo method = controlInterface.GetMethod("SetDataContext");
            method?.Invoke(control, new[] { viewModel });
        }

		/// <summary>
        /// Function to determine if the view model type has a view registration.
        /// </summary>
        /// <param name="viewModel">The view model type to evaluate.</param>
        /// <returns><b>true</b> if registered, <b>false</b> if not.</returns>
        public static bool IsRegistered(IViewModel viewModel)
        {
            if (viewModel == null)
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

            return interfaceType != null;
        }

        /// <summary>
        /// Function to create a new view based on the view model type passed in.
        /// </summary>
        /// <typeparam name="T">The type of view. Must inherit from <see cref="Control"/>.</typeparam>
        /// <param name="viewModel">The view model that is associated with the view.</param>
        /// <returns>The view for the view model.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="viewModel"/> parameter is <b>null</b>.</exception>
        public static T CreateView<T>(IViewModel viewModel)
            where T : Control
        {
            if (viewModel == null)
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

            if (interfaceType == null)
            {
                throw new KeyNotFoundException(string.Format(Resources.GOREDIT_ERR_CANNOT_FIND_VIEW_FACTORY, typeName));
            }

            return (T)_viewBuilders[interfaceType.AssemblyQualifiedName]();
        }
        #endregion
    }
}
