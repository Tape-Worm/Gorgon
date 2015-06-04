#region MIT.
//  
// Gorgon
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
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
// EXPRESS OR FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR 
// ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION 
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// Created: 03/11/2015 11:48 PM
// 
// 
#endregion

using System;

namespace Gorgon.Editor
{
	/// <summary>
	/// A model to retrieve the content interface and any ancillary interfaces.
	/// </summary>
	public interface IContentModel
		: IDisposable
	{
		/// <summary>
		/// Property to return whether the content provides a view or not.
		/// </summary>
		bool HasView
		{
			get;
		}

		/// <summary>
		/// Property to return the content interface.
		/// </summary>
		IContentData Content
		{
			get;
		}

		/// <summary>
		/// Property to return the content view.
		/// </summary>
		/// <remarks>
		/// If the content does not support views, then this value will be NULL (<i>Nothing</i> in VB.Net).
		/// </remarks>
		IContentPanel View
		{
			get;
		}

		/// <summary>
		/// Function to load the content and related items.
		/// </summary>
		void Load();

		/// <summary>
		/// Function used to unload the content and any other related items.
		/// </summary>
		void Unload();
	}
}