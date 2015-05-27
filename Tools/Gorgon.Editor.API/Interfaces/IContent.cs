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
// Created: 03/02/2015 7:57 PM
// 
// 
#endregion

using System;

namespace Gorgon.Editor
{
	/// <summary>
	/// Holds content information for the application.
	/// </summary>
	/// <remarks>
	/// Content objects are wrappers for data stored in the file system used by the editor. When content is created, a new content data item should be 
	/// created by the content wrapper (e.g. a sprite, a font, text block, etc...). When the data object is created it will then expose the properties 
	/// that the editor should edit by providing properties on the content object that reflect those of the data object.
	/// <para>
	/// When persisting or restoring content data from the file system, a <see cref="IContentSerializer"/> object will be passed to the constructor of 
	/// the content object. This serializer object is responsible for sending and receiving data to and from the file system and (de)composing the data 
	/// into content data.
	/// </para>
	/// </remarks>
	public interface IContentData
		: IDisposable, INamedObject
	{
		#region Events
		/// <summary>
		/// Event fired before content is renamed.
		/// </summary>
		event EventHandler<BeforeContentRenamedArgs> BeforeContentRenamed;

		/// <summary>
		/// Event fired after content is renamed.
		/// </summary>
		event EventHandler<AfterContentRenamedArgs> AfterContentRenamed;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the serializer for this content.
		/// </summary>
		IContentSerializer Serializer
		{
			get;
		}

		/// <summary>
		/// Property to return whether this content makes use of the property pane in the editor.
		/// </summary>
		/// <remarks>
		/// Return <c>true</c> to enable the use of the property pane, <c>false</c> to disable the pane.  Note that if the pane is disabled, it will be invisible.
		/// </remarks>
		bool HasProperties
		{
			get;
		}

		/// <summary>
		/// Property to return whether the content has pending changes that need saving.
		/// </summary>
		/// <remarks>
		/// If the <see cref="Serializer"/> property is NULL (Nothing in VB.Net), then this property will always return <c>false</c>.
		/// </remarks>
		bool HasChanges
		{
			get;
		}

		/// <summary>
		/// Property to return the type of content.
		/// </summary>
		string ContentType
		{
			get;
		}

		/// <summary>
		/// Property to set or return whether the content data is read-only.
		/// </summary>
		/// <remarks>
		/// The data in the content may be read-only in cases where other content items is dependant upon the content. 
		/// </remarks>
		bool ReadOnly
		{
			get;
			set;
		}
		#endregion
	}
}