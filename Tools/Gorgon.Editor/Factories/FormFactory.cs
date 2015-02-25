#region MIT.
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
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
// Created: Wednesday, February 25, 2015 12:23:57 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace GorgonLibrary.Editor
{
	class FormFactory
		: IFormFactory
	{
		#region Variables.
		// Synchronization object for threading.
		private readonly static object _syncLock = new object();
		// The list of shared forms.
		private readonly IDictionary<Type, Form> _sharedForms;
		// Flag to indicate that the object was disposed.
		private bool _disposed;
		#endregion

		#region Properties.

		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the shared form list.
		/// </summary>
		/// <typeparam name="T">The type of the form.</typeparam>
		/// <param name="form">Form to add to the list.</param>
		private void UpdateSharedForms<T>(T form)
			where T : Form
		{
			Type type = form.GetType();

			if (_sharedForms.ContainsKey(type))
			{
				return;
			}

			_sharedForms.Add(type, form);
		}

		/// <summary>
		/// Function to retrieve a cached form from the shared form list.
		/// </summary>
		/// <typeparam name="T">Type of form.</typeparam>
		/// <returns>The cached form if found, NULL if not.</returns>
		private T GetForm<T>()
			where T : Form
		{
			Type type = typeof(T);
			Form result;

			if (!_sharedForms.TryGetValue(type, out result))
			{
				return null;
			}

			return (T)result;
		}

		/// <summary>
		/// Function to create an instance of a form.
		/// </summary>
		/// <typeparam name="T">Type of the form.</typeparam>
		/// <param name="unique">TRUE to create a new instance, FALSE to try and use a cached instance.</param>
		/// <param name="parameters">The parameters for the form.</param>
		/// <returns>The form.</returns>
		private T CreateInstanceOfForm<T>(bool unique, object[] parameters)
			where T : Form
		{
			T result;

			if (!unique)
			{
				result = GetForm<T>();

				if (result != null)
				{
					return result;
				}
			}

			lock (_syncLock)
			{
				result = (T)Activator.CreateInstance(typeof(T),
				                                     BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
				                                     null,
				                                     parameters,
				                                     null);

				if (!unique)
				{
					UpdateSharedForms(result);
				}
			}

			return result;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="FormFactory"/> class.
		/// </summary>
		public FormFactory()
		{
			_sharedForms = new Dictionary<Type, Form>();
		}

		/// <summary>
		/// Finalizes an instance of the <see cref="FormFactory"/> class.
		/// </summary>
		~FormFactory()
		{
			Dispose(false);
		}
		#endregion
		
		#region IFormFactory Members
		/// <summary>
		/// Function to create an instance of a form.
		/// </summary>
		/// <typeparam name="T">Type of form to create.</typeparam>
		/// <typeparam name="TP0">Type of the first parameter for the constructor of the form.</typeparam>
		/// <typeparam name="TP1">Type of the second parameter for the constructor of the form.</typeparam>
		/// <typeparam name="TP2">Type of the third parameter for the constructor of the form.</typeparam>
		/// <typeparam name="TP3">Type of the fourth parameter for the constructor of the form.</typeparam>
		/// <param name="param0">The first parameter to pass to the constructor.</param>
		/// <param name="param1">The second parameter to pass to the constructor.</param>
		/// <param name="param2">The third parameter to pass to the constructor.</param>
		/// <param name="param3">The fourth parameter to pass to the constructor.</param>
		/// <param name="initialize">[Optional] A function to execute after the form is instanced.</param>
		/// <param name="unique">[Optional] TRUE to create a unique instance for the form, FALSE to reuse an existing instance.</param>
		/// <returns>The new form.</returns>
		/// <remarks>
		/// Use this method to create a new form that has a constructor with 4 parameters.
		/// <para>
		/// If the <paramref name="initialize"/> action is specified, then it can be used to provide extra initialization functionality to 
		/// the form that is not convered by the constructor. The function passed to this action will provide access to the instance of the 
		/// form in the parameter.
		/// </para>
		/// <para>
		/// When <paramref name="unique"/> is set to FALSE, then the instance that is created will be cached on the first call to this method. 
		/// Subsequent calls to this method with <paramref name="unique"/> set to FALSE will return the previous instance.  Non unique forms 
		/// will be disposed by a call to <see cref="IDisposable.Dispose"/>.  Forms that are not unique are NOT tracked and must be disposed 
		/// by the developer when their usefulness is at an end.
		/// </para>
		/// </remarks>
		public T CreateForm<T, TP0, TP1, TP2, TP3>(TP0 param0, TP1 param1, TP2 param2, TP3 param3, Action<T> initialize = null, bool unique = true) where T : Form
		{
			return CreateInstanceOfForm<T>(unique,
			                               new object[]
			                               {
				                               param0,
				                               param1,
				                               param2,
				                               param3
			                               });
		}

		/// <summary>
		/// Function to create an instance of a form.
		/// </summary>
		/// <typeparam name="T">Type of form to create.</typeparam>
		/// <typeparam name="TP0">Type of the first parameter for the constructor of the form.</typeparam>
		/// <typeparam name="TP1">Type of the second parameter for the constructor of the form.</typeparam>
		/// <typeparam name="TP2">Type of the third parameter for the constructor of the form.</typeparam>
		/// <param name="param0">The first parameter to pass to the constructor.</param>
		/// <param name="param1">The second parameter to pass to the constructor.</param>
		/// <param name="param2">The third parameter to pass to the constructor.</param>
		/// <param name="initialize">[Optional] A function to execute after the form is instanced.</param>
		/// <param name="unique">[Optional] TRUE to create a unique instance for the form, FALSE to reuse an existing instance.</param>
		/// <returns>The new form.</returns>
		/// <remarks>
		/// Use this method to create a new form that has a constructor with 3 parameters.
		/// <para>
		/// If the <paramref name="initialize"/> action is specified, then it can be used to provide extra initialization functionality to 
		/// the form that is not convered by the constructor. The function passed to this action will provide access to the instance of the 
		/// form in the parameter.
		/// </para>
		/// <para>
		/// When <paramref name="unique"/> is set to FALSE, then the instance that is created will be cached on the first call to this method. 
		/// Subsequent calls to this method with <paramref name="unique"/> set to FALSE will return the previous instance.  Non unique forms 
		/// will be disposed by a call to <see cref="IDisposable.Dispose"/>.  Forms that are not unique are NOT tracked and must be disposed 
		/// by the developer when their usefulness is at an end.
		/// </para>
		/// </remarks>
		public T CreateForm<T, TP0, TP1, TP2>(TP0 param0, TP1 param1, TP2 param2, Action<T> initialize = null, bool unique = true) where T : Form
		{
			return CreateInstanceOfForm<T>(unique,
										   new object[]
			                               {
				                               param0,
				                               param1,
				                               param2
			                               });
		}

		/// <summary>
		/// Function to create an instance of a form.
		/// </summary>
		/// <typeparam name="T">Type of form to create.</typeparam>
		/// <typeparam name="TP0">Type of the first parameter for the constructor of the form.</typeparam>
		/// <typeparam name="TP1">Type of the second parameter for the constructor of the form.</typeparam>
		/// <param name="param0">The first parameter to pass to the constructor.</param>
		/// <param name="param1">The second parameter to pass to the constructor.</param>
		/// <param name="initialize">[Optional] A function to execute after the form is instanced.</param>
		/// <param name="unique">[Optional] TRUE to create a unique instance for the form, FALSE to reuse an existing instance.</param>
		/// <returns>The new form.</returns>
		/// <remarks>
		/// Use this method to create a new form that has a constructor with 2 parameters.
		/// <para>
		/// If the <paramref name="initialize"/> action is specified, then it can be used to provide extra initialization functionality to 
		/// the form that is not convered by the constructor. The function passed to this action will provide access to the instance of the 
		/// form in the parameter.
		/// </para>
		/// <para>
		/// When <paramref name="unique"/> is set to FALSE, then the instance that is created will be cached on the first call to this method. 
		/// Subsequent calls to this method with <paramref name="unique"/> set to FALSE will return the previous instance.  Non unique forms 
		/// will be disposed by a call to <see cref="IDisposable.Dispose"/>.  Forms that are not unique are NOT tracked and must be disposed 
		/// by the developer when their usefulness is at an end.
		/// </para>
		/// </remarks>
		public T CreateForm<T, TP0, TP1>(TP0 param0, TP1 param1, Action<T> initialize = null, bool unique = true) where T : Form
		{
			return CreateInstanceOfForm<T>(unique,
										   new object[]
			                               {
				                               param0,
				                               param1
			                               });
		}

		/// <summary>
		/// Function to create an instance of a form.
		/// </summary>
		/// <typeparam name="T">Type of form to create.</typeparam>
		/// <typeparam name="TP">Type of the first parameter for the constructor of the form.</typeparam>
		/// <param name="param">Type of the first parameter for the constructor of the form.</param>
		/// <param name="initialize">[Optional] A function to execute after the form is instanced.</param>
		/// <param name="unique">[Optional] TRUE to create a unique instance for the form, FALSE to reuse an existing instance.</param>
		/// <returns>The new form.</returns>
		/// <remarks>
		/// Use this method to create a new form that has a constructor with 1 parameter.
		/// <para>
		/// If the <paramref name="initialize"/> action is specified, then it can be used to provide extra initialization functionality to 
		/// the form that is not convered by the constructor. The function passed to this action will provide access to the instance of the 
		/// form in the parameter.
		/// </para>
		/// <para>
		/// When <paramref name="unique"/> is set to FALSE, then the instance that is created will be cached on the first call to this method. 
		/// Subsequent calls to this method with <paramref name="unique"/> set to FALSE will return the previous instance.  Non unique forms 
		/// will be disposed by a call to <see cref="IDisposable.Dispose"/>.  Forms that are not unique are NOT tracked and must be disposed 
		/// by the developer when their usefulness is at an end.
		/// </para>
		/// </remarks>
		public T CreateForm<T, TP>(TP param, Action<T> initialize = null, bool unique = true) where T : Form
		{
			return CreateInstanceOfForm<T>(unique,
										   new object[]
			                               {
				                               param
			                               });
		}

		/// <summary>
		/// Function to create an instance of a form.
		/// </summary>
		/// <typeparam name="T">Type of form to create.</typeparam>
		/// <param name="initialize">[Optional] A function to execute after the form is instanced.</param>
		/// <param name="unique">[Optional] TRUE to create a unique instance for the form, FALSE to reuse an existing instance.</param>
		/// <returns>The new form.</returns>
		/// <remarks>
		/// Use this method to create a new form that only has a default constructor.
		/// <para>
		/// If the <paramref name="initialize"/> action is specified, then it can be used to provide extra initialization functionality to 
		/// the form that is not convered by the constructor. The function passed to this action will provide access to the instance of the 
		/// form in the parameter.
		/// </para>
		/// <para>
		/// When <paramref name="unique"/> is set to FALSE, then the instance that is created will be cached on the first call to this method. 
		/// Subsequent calls to this method with <paramref name="unique"/> set to FALSE will return the previous instance.  Non unique forms 
		/// will be disposed by a call to <see cref="IDisposable.Dispose"/>.  Forms that are not unique are NOT tracked and must be disposed 
		/// by the developer when their usefulness is at an end.
		/// </para>
		/// </remarks>
		public T CreateForm<T>(Action<T> initialize = null, bool unique = true) where T : Form, new()
		{
			T form;
			
			if (!unique)
			{
				form = GetForm<T>();

				if (form != null)
				{
					return form;
				}
			}

			lock (_syncLock)
			{
				form = new T();

				if (initialize != null)
				{
					initialize(form);
				}

				if (!unique)
				{
					UpdateSharedForms(form);
				}
			}

			return form;
		}
		
		/// <summary>
		/// Function to release shared forms of the type specified.
		/// </summary>
		/// <typeparam name="T">Type of the form to release.</typeparam>
		public void ReleaseForm<T>()
		{
			Type type = typeof(T);

			lock (_syncLock)
			{
				Form result;

				if (!_sharedForms.TryGetValue(type, out result))
				{
					return;
				}

				result.Dispose();
				_sharedForms.Remove(type);
			}
		}

		/// <summary>
		/// Function to release any shared forms from the factory.
		/// </summary>
		public void ReleaseForms()
		{
			lock (_syncLock)
			{
				foreach (KeyValuePair<Type, Form> item in _sharedForms)
				{
					item.Value.Dispose();
				}

				_sharedForms.Clear();
			}
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			if (disposing)
			{
				ReleaseForms();
			}

			_disposed = true;
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
