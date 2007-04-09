#region LGPL.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 
// Created: Friday, February 02, 2007 6:31:27 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Resources;
using System.IO;
using SharpUtilities;
using SharpUtilities.Collections;
using GorgonLibrary.Graphics;
using GorgonLibrary.FileSystems;

namespace GorgonLibrary.Internal
{
	/// <summary>
	/// Abstract object for implementing various managers.
	/// </summary>
	public abstract class Manager<T>		
		: BaseCollection<T>
		where T : NamedObject
	{
		#region Variables.
		private bool _caseSensitiveNames = true;			// Flag to indicate that the manager uses case sensitive naming.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return whether the names are case sensitive or not.
		/// </summary>
		public bool NamesAreCaseSensitive
		{
			get
			{
				return _caseSensitiveNames;
			}
		}

		/// <summary>
		/// Property to return an item from the manager with the specified key.
		/// </summary>
		/// <param name="key">Key name of the item.</param>
		public override T this[string key]
		{
			get
			{
				if (!_caseSensitiveNames)
					return base[key.ToLower()];
				else
					return base[key];
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to return an arbitrary object from a stream.
		/// </summary>
		/// <param name="name">Object name or path.</param>
		/// <param name="fileSystem">A file system that contains the object.</param>
		/// <param name="resources">A resource manager that is used to load the file(s).</param>
		/// <param name="stream">Stream that contains the object.</param>
		/// <param name="isXML">TRUE if the stream contains XML data, FALSE if not.</param>
		/// <param name="bytes">Number of bytes to deserialize.</param>
		/// <returns>The object contained within the stream.</returns>
		protected virtual T ObjectFromStream(string name, FileSystem fileSystem, ResourceManager resources, Stream stream, bool isXML, int bytes)
		{
			throw new NotImplementedException(NotImplementedTypes.Method, "ObjectFromStream()", null);
		}

		/// <summary>
		/// Function to return a renderable from a stream.
		/// </summary>
		/// <param name="name">Renderable name or path.</param>
		/// <param name="fileSystem">A file system that contains the renderable.</param>
		/// <param name="resources">A resource manager that is used to load the file(s).</param>
		/// <param name="stream">Stream that contains the object.</param>
		/// <param name="isXML">TRUE if the stream contains XML data, FALSE if not.</param>
		/// <param name="alternateImage">Alternative image to use.</param>
		/// <returns>The object contained within the stream.</returns>
		protected virtual T RenderableFromStream(string name, FileSystem fileSystem, ResourceManager resources, Stream stream, bool isXML, Image alternateImage)
		{
			throw new NotImplementedException(NotImplementedTypes.Method, "SpriteFromStream()", null);
		}

		/// <summary>
		/// Function to return an image from a stream.
		/// </summary>
		/// <param name="name">Image name or path.</param>
		/// <param name="fileSystem">A file system that contains the image.</param>
		/// <param name="resources">A resource manager that is used to load the file(s).</param>
		/// <param name="stream">Stream that contains the object.</param>
		/// <param name="isXML">TRUE if the stream contains XML data, FALSE if not.</param>
		/// <param name="width">Width of the image.</param>
		/// <param name="height">Height of the image.</param>
		/// <param name="bytes">Size in bytes of the image.</param>
		/// <param name="format">Format of the image buffer.</param>
		/// <param name="colorKey">Color key to use for transparency.</param>
		/// <returns>The object contained within the stream.</returns>
		protected virtual T ImageFromStream(string name, FileSystem fileSystem, ResourceManager resources, Stream stream, bool isXML, int width, int height, int bytes, ImageBufferFormats format, System.Drawing.Color colorKey)
		{
			throw new NotImplementedException(NotImplementedTypes.Method, "ImageFromStream()", null);
		}

		/// <summary>
		/// Function to add an object to the manager.
		/// </summary>
		/// <param name="newObject">Object to add.</param>
		protected virtual void AddObject(T newObject)
		{
			string name;		// Name of the object.

			if (!_caseSensitiveNames)
				name = newObject.Name.ToLower();
			else
				name = newObject.Name;

			if (Contains(name))
				throw new DuplicateObjectException(newObject.Name);

			_items.Add(name, newObject);
		}

		/// <summary>
		/// Function to return whether a key exists in the collection or not.
		/// </summary>
		/// <param name="key">Key of the object in the collection.</param>
		/// <returns>TRUE if the object exists, FALSE if not.</returns>
		public override bool Contains(string key)
		{
			if (!_caseSensitiveNames)
				return base.Contains(key.ToLower());
			else
				return base.Contains(key);
		}

		/// <summary>
		/// Function to remove an object from the list by key.
		/// </summary>
		/// <param name="key">Key of the object to remove.</param>
		protected override void RemoveItem(string key)
		{
			if (!_caseSensitiveNames)
				base.RemoveItem(key.ToLower());
			else
				base.RemoveItem(key);
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="useCaseSensitiveNaming">TRUE to use case sensitive names, FALSE to ignore.</param>
		protected Manager(bool useCaseSensitiveNaming)
			: base(16)
		{
			_caseSensitiveNames = useCaseSensitiveNaming;
		}
		#endregion
	}
}
