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
// Created: Sunday, June 26, 2011 1:57:01 PM
// 
#endregion

using Gorgon.PlugIns;

namespace Gorgon.Input
{
	/// <summary>
	/// Plug-in interface for an input device factory plug-in.
	/// </summary>
	public abstract class GorgonInputPlugIn
		: GorgonPlugIn
	{
		#region Methods.
		/// <summary>
		/// Function to perform the actual creation of the input factory object.
		/// </summary>
		/// <returns>The interface for the input factory.</returns>
		protected abstract GorgonInputFactory CreateFactory();

		/// <summary>
		/// Function to return the input factory.
		/// </summary>
		/// <returns>The interface for the input factory.</returns>
		internal GorgonInputFactory GetFactory()
		{
			return CreateFactory();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonInputPlugIn"/> class.
		/// </summary>
		/// <param name="description">Optional description of the plug-in.</param>
		protected GorgonInputPlugIn(string description)
			: base(description)
		{			
		}
		#endregion		
	}
}
