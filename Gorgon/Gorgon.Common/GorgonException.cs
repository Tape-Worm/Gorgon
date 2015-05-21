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
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Gorgon.Core.Properties;
using Gorgon.Diagnostics;

namespace Gorgon.Core
{
	/// <summary>
	/// Delegate to define an exception handler.
	/// </summary>
	public delegate void GorgonExceptionHandler();

	/// <summary>
	/// Primary exception used for Gorgon.
	/// </summary>
	[Serializable]
	public class GorgonException
		: Exception
	{
		#region Properties.
		/// <summary>
		/// Property to set or return the log system to use when dumping exceptions to the log.
		/// </summary>
		/// <remarks>This allows Gorgon to log exceptions to a log file.
		/// <para>The property uses an <see cref="IList{T}"/> to allow broadcasting of logging information to multiple log files if desired.</para>
		/// <para>By default, only the main Gorgon library log file is assigned.</para>
		/// </remarks>
		public static IList<GorgonLogFile> Logs
		{
			get;
			private set;
		}

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
		/// Function to format a stack trace to be more presentable.
		/// </summary>
		/// <param name="log">The current log file being processed.</param>
		/// <param name="stack">Stack trace to format.</param>
		/// <param name="indicator">Inner exception indicator.</param>
		/// <param name="logLevel">Logging level to use.</param>
		private static void FormatStackTrace(GorgonLogFile log, string stack, string indicator, LoggingLevel logLevel)
		{
		    if (string.IsNullOrEmpty(stack))
				return;

			stack = stack.Replace('\t', ' ');
			string[] lines = stack.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

			// Output to each log file.
			log.Print("{0}Stack trace:", logLevel, indicator);
			for (int i = lines.Length - 1; i >= 0; i--)
			{
				int inIndex = lines[i].LastIndexOf(") in ", StringComparison.Ordinal);
				int pathIndex = lines[i].LastIndexOf(@"\", StringComparison.Ordinal);

				if ((inIndex > -1) && (pathIndex > -1))
					lines[i] = lines[i].Substring(0, inIndex + 5) + lines[i].Substring(pathIndex + 1);

				log.Print("{1}{0}", logLevel, lines[i], indicator);
			}

			log.Print("{0}<<<{1}>>>", logLevel, indicator, Resources.GOR_DLG_ERR_STACK_END);
		}

		/// <summary>
		/// Function to format the exception message for the log output.
		/// </summary>
		/// <param name="log">The current log file being processed.</param>
		/// <param name="message">Message to format.</param>
		/// <param name="indicator">Inner exception indicator.</param>
		/// <param name="logLevel">Logging level to use.</param>
		private static void FormatMessage(GorgonLogFile log, string message, string indicator, LoggingLevel logLevel)
		{
		    if (string.IsNullOrEmpty(message))
				return;

			message = message.Replace('\t', ' ');
			string[] lines = message.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

			for (int i = 0; i < lines.Length; i++)
			{
				log.Print(i == 0 ? "{1}{2}: {0}" : "{1}           {0}", logLevel, lines[i], indicator, Resources.GOR_LOG_EXCEPTION);
			}
		}

		/// <summary>
		/// Function to send the exception to the log file.
		/// </summary>
		private static void LogException(Exception ex)
		{
		    string indicator = string.Empty;	// Inner exception indicator.
			string branch = string.Empty;		// Branching character.

			if ((ex == null)
				|| (Logs.Count == 0))
			{
				return;
			}

			foreach (GorgonLogFile log in Logs)
			{
				if ((log == null)
					|| (log.LogFilterLevel == LoggingLevel.NoLogging)
				    || (log.IsClosed))
				{
					continue;
				}

				log.Print("", LoggingLevel.All);
				log.Print("================================================", LoggingLevel.All);
				log.Print("\t{0}!!", LoggingLevel.All, Resources.GOR_LOG_EXCEPTION.ToUpper());
				log.Print("================================================", LoggingLevel.All);

				Exception inner = ex;

				while (inner != null)
				{
					var gorgonException = inner as GorgonException;

					FormatMessage(log, inner.Message, indicator, (inner == ex) ? LoggingLevel.All : LoggingLevel.Verbose);
					log.Print("{1}{2}: {0}", (inner == ex) ? LoggingLevel.All : LoggingLevel.Verbose, inner.GetType().FullName, indicator, Resources.GOR_DLG_ERR_EXCEPT_TYPE);
					if (inner.Source != null)
					{
						log.Print("{1}{2}: {0}", (inner == ex) ? LoggingLevel.All : LoggingLevel.Verbose, inner.Source, indicator, Resources.GOR_DLG_ERR_SRC);
					}

					if ((inner.TargetSite != null) && (inner.TargetSite.DeclaringType != null))
					{
						log.Print("{1}{2}: {0}",
						          (inner == ex) ? LoggingLevel.All : LoggingLevel.Verbose,
						          inner.TargetSite.DeclaringType.FullName + "." + inner.TargetSite.Name,
						          indicator,
						          Resources.GOR_DLG_ERR_TARGET_SITE);
					}

					if (gorgonException != null)
					{
						log.Print("{3}{4}: [{0}] {1} (0x{2:X})",
						          (inner == ex) ? LoggingLevel.All : LoggingLevel.Verbose,
						          gorgonException.ResultCode.Name,
						          gorgonException.ResultCode.Description,
						          gorgonException.ResultCode.Code,
						          indicator,
						          Resources.GOR_DLG_ERR_GOREXCEPT_RESULT);
					}

					IDictionary extraInfo = inner.Data;

					// Print custom information.
					if (extraInfo.Count > 0)
					{
						log.Print("{0}", LoggingLevel.Verbose, indicator);
						log.Print("{0}{1}:", LoggingLevel.Verbose, indicator, Resources.GOR_DLG_ERR_CUSTOM_INFO);
						log.Print("{0}------------------------------------------------------------", LoggingLevel.Verbose, indicator);
						foreach (DictionaryEntry item in extraInfo)
						{
							if (item.Value != null)
							{
								log.Print("{0}{1}:  {2}", LoggingLevel.Verbose, indicator, item.Key, item.Value);
							}
						}
						log.Print("{0}------------------------------------------------------------", LoggingLevel.Verbose, indicator);
						log.Print("{0}", LoggingLevel.Verbose, indicator);
					}

					FormatStackTrace(log, inner.StackTrace, indicator, (inner == ex) ? LoggingLevel.All : LoggingLevel.Verbose);

					if (inner.InnerException != null)
					{
						if (!string.IsNullOrWhiteSpace(indicator))
						{
							log.Print("{0}================================================================================================", LoggingLevel.Verbose, branch + "|->   ");
							branch += "  ";
							indicator = branch + "|   ";
						}
						else
						{
							log.Print("{0}================================================================================================", LoggingLevel.Verbose, branch + "|-> ");
							indicator = "|   ";
						}

						log.Print("{0}  {2} \"{1}\"", LoggingLevel.Verbose, indicator, inner.Message, Resources.GOR_DLG_ERR_NEXT_EXCEPTION);
						log.Print("{0}================================================================================================", LoggingLevel.Verbose, indicator);
					}

					inner = inner.InnerException;
				}
				log.Print("", LoggingLevel.All);
			}
		}

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
		/// <param name="ex">Exception to pass to the handler.</param>
		/// <param name="handler">[Optional] Handler to handle the exception.</param>
		/// <returns>The exception that was caught.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="ex"/> parameter is NULL (or Nothing in VB.NET).</exception>
		public static Exception Catch(Exception ex, GorgonExceptionHandler handler = null)
		{
			if (ex == null)
			{
				throw new ArgumentNullException("ex");
			}

			LogException(ex);

		    if (handler != null)
		    {
		        handler();
		    }

		    return ex;
		}

		/// <summary>
		/// Function to repackage an arbitrary exception as an Gorgon exception.
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

		/// <summary>
		/// Initializes the <see cref="GorgonException"/> class.
		/// </summary>
		static GorgonException()
		{
			Logs = new List<GorgonLogFile>();
		}
		#endregion
	}
}
