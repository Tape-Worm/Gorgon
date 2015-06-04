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
// Created: Wednesday, March 18, 2015 11:41:28 PM
// 
#endregion

using System;

namespace Gorgon.Editor
{
	/// <summary>
	/// Event arguments for the get node state event.
	/// </summary>
	class GetNodeStateEventArgs
		: EventArgs
	{
		#region Properties.
		/// <summary>
		/// Property to return the type of node.
		/// </summary>
		public NodeType NodeType
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return whether the node has changes or not.
		/// </summary>
		public bool HasChanges
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether there is an item that the node represents.
		/// </summary>
		public bool IsDetached
		{
			get;
			set;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GetNodeStateEventArgs"/> class.
		/// </summary>
		/// <param name="nodeType">Type of node to query.</param>
		public GetNodeStateEventArgs(NodeType nodeType)
		{
			NodeType = nodeType;
		}
		#endregion
	}
}
