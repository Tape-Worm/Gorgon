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
using Gorgon.Diagnostics;
using Gorgon.Editor.UI.ViewModels;
using Gorgon.Math;

namespace Gorgon.Editor.UI
{
    /// <summary>
    /// A base view model for the gorgon editor.
    /// </summary>
    /// <typeparam name="T">The type of injection parameters for the view model.  Must implement <see cref="IViewModelInjection"/> and be a reference type.</typeparam>
    /// <seealso cref="IViewModelInjection"/>
    public abstract class ViewModelBase<T>
        : PropertyMonitor, IViewModel
        where T : class, IViewModelInjection
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

        #region Properties.
        /// <summary>
        /// Property to return the logging interface for debug log messages.
        /// </summary>
        protected IGorgonLog Log
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
            if (args == null)
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
            if (args == null)
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
        /// </remarks>
        protected abstract void OnInitialize(T injectionParameters);

        /// <summary>
        /// Function called when the associated view is loaded.
        /// </summary>
        public virtual void OnLoad()
        {

        }

        /// <summary>
        /// Function called when the associated view is unloaded.
        /// </summary>
        public virtual void OnUnload()
        {

        }

        /// <summary>
        /// Function to inject dependencies for the view model.
        /// </summary>
        /// <param name="injectionParameters">The parameters to inject.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="injectionParameters"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentMissingException">Thrown when a required parameter is <b>null</b> or missing from the <paramref name="injectionParameters"/>.</exception>
        /// <remarks>
        /// <para>
        /// Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.
        /// </para>
        /// </remarks>
        public void Initialize(T injectionParameters)
        {
            if (injectionParameters == null)
            {
                throw new ArgumentNullException(nameof(injectionParameters));
            }

            Log = injectionParameters.Log ?? GorgonLog.NullLog;

            OnInitialize(injectionParameters);
        }
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelBase{T}"/> class.
        /// </summary>
        protected ViewModelBase()
        {
			
        }
        #endregion
    }
}
