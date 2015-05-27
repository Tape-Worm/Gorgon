#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Tuesday, June 14, 2011 8:56:44 PM
// 
#endregion

using System;
using System.Runtime.Serialization;

namespace Gorgon.Core
{
	/// <summary>
	/// Primary exception used for Gorgon.
	/// </summary>
	[Serializable]
	public class GorgonException
		: Exception
	{
		#region Properties.
		/// <summary>
		/// Property to return the exception result code.
		/// </summary>
		public GorgonResult ResultCode
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo"/> with information about the exception.
		/// </summary>
		/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="info"/> parameter is a null reference (Nothing in Visual Basic). </exception>
		/// <PermissionSet>
		/// 	<IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*"/>
		/// 	<IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="SerializationFormatter"/>
		/// </PermissionSet>
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);

			info.AddValue("ResultCode", ResultCode, typeof(GorgonResult));
		}

		/// <summary>
		/// Functon to catch and handle an exception.
		/// </summary>
		/// <typeparam name="T">The type of exception. This value must be or inherit from the <see cref="Exception"/> type.</typeparam>
		/// <param name="ex">Exception to pass to the handler.</param>
		/// <param name="handler">A method that is called to handle the exception.</param>
		/// <param name="logException">[Optional] <c>true</c> to log the exception to the primary application log, <c>false</c> to skip logging.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="ex"/> parameter is NULL (or Nothing in VB.NET).</exception>
		/// <remarks>
		/// This is a convenience method used to catch an exception and then handle it with the supplied <paramref name="handler"/> method. The handler method must take a parameter 
		/// that has a type that is or derives from <see cref="Exception"/>.
		/// </remarks>
		public static void Catch<T>(T ex, Action<T> handler, bool logException = false)
			where T : Exception
		{
			if ((ex == null)
				|| (handler == null))
			{
				return;
			}

			if (logException)
			{
				// Automatically log this exception.
				GorgonApplication.Log.LogException(ex);
			}

			handler(ex);
		}

		/// <summary>
		/// Function to repackage an arbitrary exception as a <see cref="GorgonException"/>.
		/// </summary>
		/// <param name="result">Result code to use.</param>
		/// <param name="message">Message to append to the result.</param>
		/// <param name="ex">Exception to capture and rethrow.</param>
		/// <returns>A new Gorgon exception to throw.</returns>
		/// <remarks>The original exception will be the inner exception of the new <see cref="T:GorgonLibrary.GorgonException"/>.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="ex"/> parameter is NULL (or Nothing in VB.NET).</exception>
		public static GorgonException Repackage(GorgonResult result, string message, Exception ex)
		{
			if (ex == null)
			{
				throw new ArgumentNullException("ex");
			}

			return new GorgonException(result, message, ex);
		}

		/// <summary>
		/// Function to repackage an arbitrary exception as an Gorgon exception.
		/// </summary>
		/// <param name="result">Result code to use.</param>
		/// <param name="ex">Exception to capture and rethrow.</param>
		/// <returns>A new Gorgon exception to throw.</returns>
		/// <remarks>The original exception will be the inner exception of the new <see cref="T:GorgonLibrary.GorgonException"/>.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="ex"/> parameter is NULL (or Nothing in VB.NET).</exception>
		public static GorgonException Repackage(GorgonResult result, Exception ex)
		{
			if (ex == null)
			{
				throw new ArgumentNullException("ex");
			}

			return new GorgonException(result, ex);
		}

		/// <summary>
		/// Function to repackage an arbitrary exception as an Gorgon exception.
		/// </summary>
		/// <param name="message">New message to pass to the new exception.</param>
		/// <param name="ex">Exception to capture and rethrow.</param>
		/// <returns>A new Gorgon exception to throw.</returns>
		/// <remarks>The original exception will be the inner exception of the new <see cref="T:GorgonLibrary.GorgonException"/>.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="ex"/> parameter is NULL (or Nothing in VB.NET).</exception>
		public static GorgonException Repackage(string message, Exception ex)
		{
			if (ex == null)
			{
				throw new ArgumentNullException("ex");
			}

			return new GorgonException(message, ex);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="errorMessage">Error message to display.</param>
		/// <param name="innerException">Inner exception to pass through.</param>
		public GorgonException(string errorMessage, Exception innerException)
			: base(errorMessage, innerException)
		{
			ResultCode = new GorgonResult("GorgonException", HResult, errorMessage);
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="errorMessage">Error message to display.</param>
		public GorgonException(string errorMessage)
			: base(errorMessage)
		{
			ResultCode = new GorgonResult("GorgonException", HResult, errorMessage);
		}

		/// <summary>
		/// Serialized constructor.
		/// </summary>
		/// <param name="info">Serialization info.</param>
		/// <param name="context">Serialization context.</param>
		protected GorgonException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			if (info.FullTypeName == typeof(GorgonResult).FullName)
			{
				ResultCode = (GorgonResult)info.GetValue("ResultCode", typeof(GorgonResult));
			}
			else
			{
				ResultCode = new GorgonResult("Exception", info.GetInt32("HResult"), info.GetString("Message"));
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonException"/> class.
		/// </summary>
		/// <param name="result">The result.</param>
		/// <param name="message">Message data to append to the error.</param>
		/// <param name="inner">The inner exception.</param>
		public GorgonException(GorgonResult result, string message, Exception inner)
			: base(result.Description + (!string.IsNullOrEmpty(message) ? "\n" + message : string.Empty), inner)
		{
			ResultCode = result;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonException"/> class.
		/// </summary>
		/// <param name="result">The result.</param>
		/// <param name="message">Message data to append to the error.</param>
		public GorgonException(GorgonResult result, string message)
			: base(result.Description + (!string.IsNullOrEmpty(message) ? "\n" + message : string.Empty))
		{
			ResultCode = result;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonException"/> class.
		/// </summary>
		/// <param name="result">The result.</param>
		/// <param name="inner">The inner exception.</param>
		public GorgonException(GorgonResult result, Exception inner)
			: this(result, null, inner)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonException"/> class.
		/// </summary>
		/// <param name="result">The result.</param>
		public GorgonException(GorgonResult result)
			: this(result, null, null)
		{
		}

		/// <summary>
		/// Default constructor.
		/// </summary>
		public GorgonException()
		{
			ResultCode = new GorgonResult("GorgonException", int.MinValue, string.Empty);
		}
		#endregion
	}
}
