#region MIT.
// 
// Gorgon.
// Copyright (C) 2014 Michael Winsor
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
// Created: Monday, April 21, 2014 12:23:25 AM
// 
#endregion

namespace Gorgon.Editor
{
	/// <summary>
	/// The state of the load operation.
	/// </summary>
	public enum DependencyLoadState
	{
		/// <summary>
		/// The load operation is successful.
		/// </summary>
		Successful = 0,
		/// <summary>
		/// An error has occurred, but is not fatal.
		/// </summary>
		ErrorContinue = 1,
		/// <summary>
		/// An error has occurred, but is fatal.  The content will not load.
		/// </summary>
		FatalError = 2
	}

	/// <summary>
	/// The result of the dependency load operation.
	/// </summary>
	public struct DependencyLoadResult
	{
		#region Variables.
		/// <summary>
		/// The state of the load operation.
		/// </summary>
		public readonly DependencyLoadState State;
		
		/// <summary>
		/// The error message, if applicable.
		/// </summary>
		public readonly string Message;
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="DependencyLoadResult"/> struct.
		/// </summary>
		/// <param name="state">The state of the result.</param>
		/// <param name="errorMessage">The error message for the result.  Pass NULL (Nothing in VB.Net) for a successful result.</param>
		public DependencyLoadResult(DependencyLoadState state, string errorMessage)
		{
			State = state;
			Message = errorMessage;
		}
		#endregion
	}
}
