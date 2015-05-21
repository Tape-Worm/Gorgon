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
// Created: Monday, March 2, 2015 8:44:39 PM
// 
#endregion

using System;
using System.IO;

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
	public abstract class EditorContentSerializer
		: IContentSerializer
	{
		#region Variables.
		#endregion

		#region Methods.
		/// <summary>
		/// Function to serialize the content data into a flattened format.
		/// </summary>
		/// <param name="contentData">The content to be peristed to the stream.</param>
		/// <param name="stream">The stream that will receive the flattened content data.</param>
		/// <remarks>
		/// Both the <paramref name="contentData"/> and <paramref name="stream"/> parameters are guaranteed to never be NULL (Nothing in VB.Net). 
		/// And the <paramref name="stream"/> parameter will always be write-only.
		/// </remarks>
		protected abstract void OnSerialize(object contentData, Stream stream);

		/// <summary>
		/// Function to deserialize the content data from a flattened format.
		/// </summary>
		/// <param name="stream">The stream that will receive the flattened content data.</param>
		/// <returns>The content object data if successful, NULL if not.</returns>
		/// <remarks>
		/// The parameter is guaranteed to never be NULL (Nothing in VB.Net) and it will always be read-only.
		/// </remarks>
		protected abstract object OnDeserialize(Stream stream);
		#endregion

		#region IContentSerializer<T> Members
		#region Events.
		/// <summary>
		/// Event called before the content data is serialized.
		/// </summary>
		public event EventHandler<GorgonCancelEventArgs> BeforeSerialize;

		/// <summary>
		/// Event called after the content data is serialized.
		/// </summary>
		public event EventHandler AfterSerialize;

		/// <summary>
		/// Event called before content data is deserialized.
		/// </summary>
		public event EventHandler<GorgonCancelEventArgs> BeforeDeserialize;

		/// <summary>
		/// Event called after content data is deserialized.
		/// </summary>
		public event EventHandler AfterDeserialize;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to serialize the content data into a flattened format.
		/// </summary>
		/// <param name="contentData">The content to be peristed to the stream.</param>
		/// <param name="stream">The stream that will receive the flattened content data.</param>
		/// <returns>TRUE if the content was serialized successfully, FALSE if the content serialization was cancelled.</returns>
		/// <remarks>
		/// Both the <paramref name="contentData"/> and <paramref name="stream"/> parameters are guaranteed to never be NULL (Nothing in VB.Net). 
		/// And the <paramref name="stream"/> parameter will always be write-only.
		/// </remarks>
		public bool Serialize(object contentData, Stream stream)
		{
			var args = new GorgonCancelEventArgs(false);

			if (BeforeSerialize != null)
			{
				BeforeSerialize(this, args);

				if (args.Cancel)
				{
					return false;
				}
			}

			OnSerialize(contentData, stream);

			if (AfterSerialize == null)
			{
				return true;
			}

			AfterSerialize(this, EventArgs.Empty);
			return true;
		}

		/// <summary>
		/// Function to deserialize the content data from a flattened format.
		/// </summary>
		/// <param name="stream">The stream that will receive the flattened content data.</param>
		/// <returns>The content object data if successful, NULL if the operation was cancelled.</returns>
		/// <remarks>
		/// The parameter is guaranteed to never be NULL (Nothing in VB.Net) and it will always be read-only.
		/// </remarks>
		public object Deserialize(Stream stream)
		{
			var args = new GorgonCancelEventArgs(false);

			if (BeforeDeserialize != null)
			{
				BeforeDeserialize(this, args);

				if (args.Cancel)
				{
					return null;
				}
			}

			object contentData = OnDeserialize(stream);

			if (AfterDeserialize == null)
			{
				return contentData;
			}

			AfterDeserialize(this, EventArgs.Empty);
			return contentData;
		}
		#endregion
		#endregion
	}
}
