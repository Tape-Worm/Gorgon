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
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Gorgon.Core;
using Gorgon.Editor.Properties;
using Gorgon.Math;

namespace Gorgon.Editor.UI
{
    /// <summary>
    /// A base view model for the gorgon editor.
    /// </summary>
    /// <typeparam name="T">The type of injection parameters for the view model.  Must implement <see cref="IViewModelInjection"/> and be a reference type.</typeparam>
    /// <seealso cref="IViewModelInjection"/>
    public abstract class ViewModelBase<T>
        : IViewModel
        where T : class, IViewModelInjection
    {
        #region Events.
        /// <summary>
        /// Event triggered when a property is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Event triggered before a property is changed.
        /// </summary>
        public event PropertyChangingEventHandler PropertyChanging;

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
        // The list of properties to use.
        private PropertyDescriptorCollection _properties;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return whether to use property name validation when evaluating property changes.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For DEBUG builds, users should set this value to true so that any mis-type of a property name can be caught.
        /// </para>
        /// </remarks>
        public static bool UsePropertyNameValidation
        {
            get;
            set;
#if DEBUG
        } = true;
#else
        }
#endif
        #endregion

        #region Methods.
		/// <summary>
		/// Function to validate whether the specified property exists on this object.
		/// </summary>
		/// <param name="propertyName">Name of the property to look up.</param>
		[DebuggerStepThrough]
		private void ValidatePropertyName(string propertyName)
		{
			if (_properties == null)
			{
				_properties = TypeDescriptor.GetProperties(this);
				Debug.Assert(_properties != null, "This object does not contain public properties!");
			}

			if (_properties[propertyName] == null)
			{
				throw new MissingMemberException(string.Format(Resources.GOREDIT_ERR_PROPERTY_DOES_NOT_EXIST, propertyName));
			}
		}

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
        /// Function to notify when a property is about to be changed within a property setter.
        /// </summary>
        /// <param name="propertyName">[Automatically set by the compiler] The name of the property that called this method.</param>
        /// <remarks>
        /// <para>
        /// Unlike the <see cref="NotifyPropertyChanging"/>, this method will automatically determine the name of the property that called it. Therefore, the user should <u>not</u> set the 
        /// <paramref name="propertyName"/> parameter manually.
        /// </para>
        /// </remarks>
        protected void OnPropertyChanging([CallerMemberName] string propertyName = "") => NotifyPropertyChanging(propertyName);

        /// <summary>
        /// Function to notify when a property has been changed within a property setter.
        /// </summary>
        /// <param name="propertyName">[Automatically set by the compiler] The name of the property that called this method.</param>
        /// <remarks>
        /// <para>
        /// Unlike the <see cref="NotifyPropertyChanged"/>, this method will automatically determine the name of the property that called it. Therefore, the user should <u>not</u> set the 
        /// <paramref name="propertyName"/> parameter manually.
        /// </para>
        /// </remarks>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "") => NotifyPropertyChanged(propertyName);

        /// <summary>
        /// Function to notify when a property has been changed.
        /// </summary>
        /// <param name="propertyName">Name of the property to change.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="propertyName"/> is NULL (Nothing in VB.Net).</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="propertyName"/> is empty.</exception>
        /// <remarks>
        /// <para>
        /// This method used to notify when a property has changed outside of the property setter, or if a property other than the current property has changed inside of a property setter. The 
        /// user can specify the name of the property manually through the <paramref name="propertyName"/> parameter. 
        /// </para>
        /// <para>
        /// Do not use this method in the setter for the property that is notifying. In that case, call the <see cref="OnPropertyChanged"/> method instead.
        /// </para>
        /// <para>
        /// <note type="warning">
        /// <para>
        /// If the name of the property has changed, then calls to this method <u>must</u> be changed to reflect the new property name. Otherwise, functionality will break as the notification will 
        /// point to an invalid property. To that end, applications should use the C# <see langword="nameof"/> operator when passing a property name to this method. 
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        public void NotifyPropertyChanged(string propertyName)
		{
			if (propertyName == null)
			{
				throw new ArgumentNullException(nameof(propertyName));
			}

			if (string.IsNullOrWhiteSpace(propertyName))
			{
				throw new ArgumentEmptyException(nameof(propertyName));
			}

			if (UsePropertyNameValidation)
			{
				ValidatePropertyName(propertyName);
			}

			PropertyChangedEventHandler handler = PropertyChanged;

			handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		/// <summary>
		/// Function to notify before a property is changed.
		/// </summary>
		/// <param name="propertyName">Name of the property to change.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="propertyName"/> is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="propertyName"/> is empty.</exception>
		/// <remarks>
		/// <para>
		/// This method is used to notify before a property is changed outside of the property setter, or if a property other than the current property is changing inside of a property setter. The 
		/// user can specify the name of the property manually through the <paramref name="propertyName"/> parameter.
		/// </para>
		/// <para>
		/// Do not use this method in the setter for the property that is notifying. In that case, call the <see cref="OnPropertyChanging"/> method instead.
		/// </para>
		/// <para>
		/// <note type="warning">
		/// <para>
		/// If the name of the property has changed, then calls to this method <u>must</u> be changed to reflect the new property name. Otherwise, functionality will break as the notification will 
		/// point to an invalid property. To that end, applications should use the C# <see langword="nameof"/> operator when passing a property name to this method. 
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public void NotifyPropertyChanging(string propertyName)
		{
			if (propertyName == null)
			{
				throw new ArgumentNullException(nameof(propertyName));
			}

			if (string.IsNullOrWhiteSpace(propertyName))
			{
				throw new ArgumentEmptyException(nameof(propertyName));
			}

			if (UsePropertyNameValidation)
			{
				ValidatePropertyName(propertyName);
			}

			PropertyChangingEventHandler handler = PropertyChanging;

			handler?.Invoke(this, new PropertyChangingEventArgs(propertyName));
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
        public void Initialize(T injectionParameters) => OnInitialize(injectionParameters ?? throw new ArgumentNullException(nameof(injectionParameters)));
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
