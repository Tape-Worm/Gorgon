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
// Created: 03/02/2015 8:33 PM
// 
// 
#endregion

using System;
using System.IO;
using Gorgon.Core;

namespace Gorgon.Editor
{
	/// <summary>
	/// Defines how to serialize or deserialize a piece of content to and from the file system.
	/// </summary>
	/// <remarks>
	/// Content items must implement a serializer object in order to persist and restore their data to and from the file system. For example, 
	/// A SpriteSerializer would implement this interface and be attached to the content object. The content controller would then call 
	/// this interface passing the content object as a parameter to be peristed, or to fill the content object with data when restoring.
	/// </remarks>
	public interface IContentSerializer
	{
		#region Events.
		/// <summary>
		/// Event called before the content data is serialized.
		/// </summary>
		event EventHandler<GorgonCancelEventArgs> BeforeSerialize;

		/// <summary>
		/// Event called after the content data is serialized.
		/// </summary>
		event EventHandler AfterSerialize;

		/// <summary>
		/// Event called before content data is deserialized.
		/// </summary>
		event EventHandler<GorgonCancelEventArgs> BeforeDeserialize;

		/// <summary>
		/// Event called after content data is deserialized.
		/// </summary>
		event EventHandler AfterDeserialize;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to serialize the content data into a flattened format.
		/// </summary>
		/// <param name="contentData">The content to be peristed to the stream.</param>
		/// <param name="stream">The stream that will receive the flattened content data.</param>
		/// <returns><b>true</b> if the content was serialized successfully, <b>false</b> if the content serialization was cancelled.</returns>
		/// <remarks>
		/// Both the <paramref name="contentData"/> and <paramref name="stream"/> parameters are guaranteed to never be NULL (<i>Nothing</i> in VB.Net). 
		/// And the <paramref name="stream"/> parameter will always be write-only.
		/// </remarks>
		bool Serialize(object contentData, Stream stream);

		/// <summary>
		/// Function to deserialize the content data from a flattened format.
		/// </summary>
		/// <param name="stream">The stream that will receive the flattened content data.</param>
		/// <returns>The content object data if successful, NULL if the operation was cancelled.</returns>
		/// <remarks>
		/// The parameter is guaranteed to never be NULL (<i>Nothing</i> in VB.Net) and it will always be read-only.
		/// </remarks>
		object Deserialize(Stream stream);
		#endregion
	}
}