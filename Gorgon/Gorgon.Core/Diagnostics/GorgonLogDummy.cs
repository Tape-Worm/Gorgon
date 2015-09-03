#region MIT
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
// Created: Friday, May 23, 2015 3:09:45 PM
// 
#endregion

using System;
using System.Threading;

namespace Gorgon.Diagnostics
{
	/// <summary>
	/// An implementation of the <see cref="IGorgonThreadedLog"/> type that does nothing.
	/// </summary>
	sealed public class GorgonLogDummy
		: IGorgonThreadedLog
	{
		#region Properties.
		/// <inheritdoc/>
		public int ThreadID
		{
			get;
		}

		/// <inheritdoc/>
		public LoggingLevel LogFilterLevel
		{
			get;
			set;
		}

		/// <inheritdoc/>
		public string LogApplication => string.Empty;
		#endregion

		#region Methods.
		/// <inheritdoc/>
		public void LogException(Exception ex)
		{
			// Intentionally left blank.
		}

		/// <inheritdoc/>
		public void Print(string formatSpecifier, LoggingLevel level, params object[] arguments)
		{
			// Intentionally left blank.
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonLogDummy"/> class.
		/// </summary>
		public GorgonLogDummy()
		{
			ThreadID = Thread.CurrentThread.ManagedThreadId;
		}
		#endregion
	}
}
