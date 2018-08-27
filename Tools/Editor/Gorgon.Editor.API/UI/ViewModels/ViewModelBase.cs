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

namespace Gorgon.Editor.UI
{
    /// <summary>
    /// A base view model for the gorgon editor.
    /// </summary>
    public class ViewModelBase
        : IViewModel
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
		/// Function to notify when a property is about to be changed within a property setter.
		/// </summary>
		/// <param name="propertyName">[Automatically set by the compiler] The name of the property that called this method.</param>
		/// <remarks>
		/// <para>
		/// Unlike the <see cref="NotifyPropertyChanging"/>, this method will automatically determine the name of the property that called it. Therefore, the user should <u>not</u> set the 
		/// <paramref name="propertyName"/> parameter manually.
		/// </para>
		/// </remarks>
		protected void OnPropertyChanging([CallerMemberName] string propertyName = "")
		{
			NotifyPropertyChanging(propertyName);
		}

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
		protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			NotifyPropertyChanged(propertyName);
		}

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
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelBase"/> class.
        /// </summary>
        protected ViewModelBase()
        {
			
        }
        #endregion
    }
}
