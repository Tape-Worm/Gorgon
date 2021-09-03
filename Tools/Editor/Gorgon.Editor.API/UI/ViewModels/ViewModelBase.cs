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
// Created: August 26, 2018 12:02:30 PM
// 
#endregion

using System;
using System.ComponentModel;
using System.Threading;
using Gorgon.Editor.PlugIns;
using Gorgon.Math;

namespace Gorgon.Editor.UI
{
    /// <summary>
    /// A base view model for the gorgon editor.
    /// </summary>
    /// <typeparam name="T">The type of injection parameters for the view model.  Must implement <see cref="IViewModelInjection{T}"/> and be a reference type.</typeparam>
    /// <typeparam name="THs">The type of services passed from the host application. Must implement <see cref="IHostServices"/>.</typeparam>
    /// <seealso cref="IViewModelInjection{T}"/>
    /// <remarks>
    /// <para>
    /// This is the base class for all view models used by the editor and provides the bare minimum in functionality. This object already implements the <see cref="INotifyPropertyChanged"/> 
    /// and the <see cref="INotifyPropertyChanging"/> interfaces to allow communication with a view. 
    /// </para>
    /// <para>
    /// Common services used by the application, such as a message display service, content plug in service, etc... are provided through the <see cref="HostServices"/> property so that custom 
    /// view models can use standardized functionality to communicate with the user should the need arise.
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
    /// private void DoCommandAction()
    /// {
    ///     NotifyPropertyChanging(nameof(ReadOnlyValue));
    ///     _readOnlyValue++;
    ///     NotifyPropertyChanged(nameof(ReadOnlyValue));
    /// }
    /// ]]>
    /// The difference being that for properties, you do not need to specify the name of the property being updated, and in methods you do. 
    /// </code>
    /// </para>
    /// <para>
    /// The view model is also equipped with several events that are used to notify the application that a long running operation is executing. Applications can intercept these events and display a 
    /// progress panel, or "please wait" panel. These should only be used with asynchronous operations as they will not update correctly if everything is running on the same thread.
    /// </para>
    /// </remarks>
    public abstract class ViewModelBase<T, THs>
        : PropertyMonitor, IViewModel
        where THs : IHostServices
        where T : class, IViewModelInjection<THs>        
    {
        #region Events.
        /// <summary>
        /// Event triggered when a wait overlay panel needs to be activated.
        /// </summary>
        public event EventHandler<WaitPanelActivateArgs> WaitPanelActivated;

        /// <summary>
        /// Event triggered when a wait overlay panel needs to be deactivated.
        /// </summary>
        public event EventHandler WaitPanelDeactivated;

        /// <summary>
        /// Event triggered when the progress overlay panel over needs to be updated.
        /// </summary>
        public event EventHandler<ProgressPanelUpdateArgs> ProgressUpdated;

        /// <summary>
        /// Event triggered when the progress overlay should be deactivated.
        /// </summary>
        public event EventHandler ProgressDeactivated;
        #endregion

        #region Variables.
        // Flag to indicate that the view model has been loaded.
        private int _loaded;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the services passed in from the host application.
        /// </summary>
        protected THs HostServices
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to activate and/or update the progress panel overlay on the view, if the view supports it.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="percentage">The percentage complete as a normalized value (0..1).</param>
        /// <param name="title">[Optional] The title for the panel.</param>
        /// <param name="cancelAction">[Optional] The action to execute if the operation is cancelled.</param>
        protected void UpdateProgress(string message, float percentage, string title = null, Action cancelAction = null) => UpdateProgress(new ProgressPanelUpdateArgs
        {
            IsMarquee = false,
            Message = message,
            Title = title,
            PercentageComplete = percentage.Max(0).Min(1.0f),
            CancelAction = cancelAction
        });

        /// <summary>
        /// Function to activate and/or update the progress panel overlay on the view, if the view supports it.
        /// </summary>
        /// <param name="args">The event message arguments.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="args"/> parameter is <b>null</b>.</exception>
        protected void UpdateProgress(ProgressPanelUpdateArgs args)
        {
            if (args is null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            EventHandler<ProgressPanelUpdateArgs> handler = ProgressUpdated;
            handler?.Invoke(this, args);
        }

        /// <summary>
        /// Function to activate and/or update the progress panel overlay on the view as a marquee progress meter, if the view supports it.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="title">[Optional] The title for the panel.</param>
        /// <param name="cancelAction">[Optional] The action to execute if the operation is cancelled.</param>
        protected void UpdateMarequeeProgress(string message, string title = null, Action cancelAction = null) => UpdateProgress(new ProgressPanelUpdateArgs
        {
            IsMarquee = true,
            Message = message,
            Title = title,
            PercentageComplete = 0,
            CancelAction = cancelAction
        });

        /// <summary>
        /// Function to hide the progress overlay panel on the view, if the view supports it.
        /// </summary>
        protected void HideProgress()
        {
            EventHandler handler = ProgressDeactivated;
            ProgressDeactivated?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Function to activate a wait overlay panel on the view, if the view supports it.
        /// </summary>
        /// <param name="args">The event message arguments.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="args"/> parameter is <b>null</b>.</exception>
        protected void ShowWaitPanel(WaitPanelActivateArgs args)
        {
            if (args is null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            EventHandler<WaitPanelActivateArgs> handler = WaitPanelActivated;
            handler?.Invoke(this, args);
        }

        /// <summary>
        /// Function to activate a wait overlay panel on the view, if the view supports it.
        /// </summary>
        /// <param name="message">The message for the wait overlay.</param>
        /// <param name="title">[Optional] The title for the overlay.</param>
        protected void ShowWaitPanel(string message, string title = null) => ShowWaitPanel(new WaitPanelActivateArgs(message, title));

        /// <summary>
        /// Function to deactivate an active wait panel overlay on the view, if the view supports it.
        /// </summary>
        protected void HideWaitPanel()
        {
            EventHandler handler = WaitPanelDeactivated;
            handler?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Function to inject dependencies for the view model.
        /// </summary>
        /// <param name="injectionParameters">The parameters to inject.</param>
        /// <remarks>
        /// <para>
        /// Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.
        /// </para>
        /// <para>
        /// This method is only ever called after the view model has been created, and never again during the lifetime of the view model.
        /// </para>
        /// </remarks>
        protected abstract void OnInitialize(T injectionParameters);

        /// <summary>
        /// Function called when the associated view is loaded.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method should be overridden when there is a need to set up functionality (e.g. events) when the UI first loads with the view model attached. Unlike the <see cref="Initialize(T)"/> method, this 
        /// method may be called multiple times during the lifetime of the application.
        /// </para>
        /// <para>
        /// Anything that requires tear down should have their tear down functionality in the accompanying <see cref="Unload"/> method.
        /// </para>
        /// </remarks>
        /// <seealso cref="Initialize(T)"/>
        /// <seealso cref="Unload"/>
        protected virtual void OnLoad()
        {
        }

        /// <summary>
        /// Function called when the associated view is unloaded.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is used to perform tear down and clean up of resources.
        /// </para>
        /// </remarks>
        /// <seealso cref="Load"/>
        protected virtual void OnUnload()
        {
        }

        /// <summary>
        /// Function called when the associated view is loaded.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method should be overridden when there is a need to set up functionality (e.g. events) when the UI first loads with the view model attached. Unlike the <see cref="Initialize(T)"/> method, this 
        /// method may be called multiple times during the lifetime of the application.
        /// </para>
        /// <para>
        /// Anything that requires tear down should have their tear down functionality in the accompanying <see cref="Unload"/> method.
        /// </para>
        /// </remarks>
        /// <seealso cref="Initialize(T)"/>
        /// <seealso cref="Unload"/>
        public void Load()
        {
            if (Interlocked.Exchange(ref _loaded, 1) == 1)
            {
                return;
            }

            OnLoad();
        }

        /// <summary>
        /// Function called when the associated view is unloaded.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is used to perform tear down and clean up of resources.
        /// </para>
        /// </remarks>
        /// <seealso cref="Load"/>
        public void Unload()
        {
            if (Interlocked.Exchange(ref _loaded, 0) == 0)
            {
                return;
            }

            OnUnload();
        }

        /// <summary>
        /// Function to inject dependencies for the view model.
        /// </summary>
        /// <param name="injectionParameters">The parameters to inject.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="injectionParameters"/> parameter is <b>null</b>.</exception>
        /// <remarks>
        /// <para>
        /// Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.
        /// </para>
        /// <para>
        /// This method is only ever called after the view model has been created, and never again during the lifetime of the view model.
        /// </para>
        /// </remarks>
        public void Initialize(T injectionParameters)
        {
            if (injectionParameters is null)
            {
                throw new ArgumentNullException(nameof(injectionParameters));
            }
            
            HostServices = injectionParameters.HostServices;

            OnInitialize(injectionParameters);
        }
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelBase{T, THs}"/> class.
        /// </summary>
        protected ViewModelBase()
        {

        }
        #endregion
    }
}
