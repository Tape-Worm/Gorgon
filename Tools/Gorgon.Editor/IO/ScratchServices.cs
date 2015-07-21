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
// Created: Monday, March 2, 2015 12:26:47 AM
// 
#endregion

namespace Gorgon.Editor
{
	/// <summary>
	/// Services pertaining to the usage of the scratch area.
	/// </summary>
	sealed class ScratchService : IScratchService
	{
		#region Properties.
		/// <summary>
		/// Property to return the scratch area interface.
		/// </summary>
		public IScratchArea ScratchArea
		{
			get;
		}

		/// <summary>
		/// Property to return the locator UI used to change the location of a scratch area.
		/// </summary>
		public IScratchLocator ScratchLocator
		{
			get;
		}
		#endregion

		#region Constructor/Destructor.

		/// <summary>
		/// Initializes a new instance of the <see cref="ScratchService"/> class.
		/// </summary>
		/// <param name="scratchArea">The scratch area.</param>
		/// <param name="locator">The locator UI to change the path to the scratch area..</param>
		public ScratchService(IScratchArea scratchArea, IScratchLocator locator)
		{
			ScratchArea = scratchArea;
			ScratchLocator = locator;
		}
		#endregion
	}
}
