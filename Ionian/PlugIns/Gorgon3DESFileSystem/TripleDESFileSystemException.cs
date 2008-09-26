#region MIT.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
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
// Created: Wednesday, November 01, 2006 5:33:47 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace GorgonLibrary.FileSystems
{
	/// <summary>
	/// File system authentication exception.
	/// </summary>
	public class InvalidAuthenticationDataException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="fileSystemName">Name of the file system that previously existed.</param>
		/// <param name="ex">Source exception.</param>
		public InvalidAuthenticationDataException(string fileSystemName, Exception ex)
			: base("The authentication for the file system '" + fileSystemName + "' is not valid.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="fileSystemName">Name of the file system that previously existed.</param>
        public InvalidAuthenticationDataException(string fileSystemName)
			: this(fileSystemName, null)
		{
		}
		#endregion
	}
}
