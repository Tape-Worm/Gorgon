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
// Created: Monday, June 27, 2011 9:01:06 AM
// 
#endregion

using System;
using GorgonLibrary;
using GorgonLibrary.Collections;
using GorgonLibrary.PlugIns;

namespace GorgonLibrary.FileSystem
{
	/// <summary>
	/// A collection of file system providers.
	/// </summary>
	public class GorgonFileSystemProviderCollection
		: GorgonBaseNamedObjectCollection<GorgonFileSystemProvider>
	{
		#region Variables.
		private GorgonFileSystem _fileSystem = null;			// File system that owns this collection.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the item at the specified index.
		/// </summary>
		public GorgonFileSystemProvider this[int index]
		{
			get
			{
				return GetItem(index);
			}
		}

		/// <summary>
		/// Property to return the item with the specified name.
		/// </summary>
		public GorgonFileSystemProvider this[string name]
		{
			get
			{
				return GetItem(name);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to add a new provider based on the provider type name.
		/// </summary>
		/// <param name="providerTypeName">Fully qualified type name of the provider to add.</param>
		/// <returns>The new instance of the provider interface.</returns>
		internal GorgonFileSystemProvider Add(string providerTypeName)
		{
			GorgonFileSystemProviderPlugIn plugIn = null;
			GorgonFileSystemProvider provider = null;

			GorgonUtility.AssertParamString(providerTypeName, "providerTypeName");

			// Check to see if we've already loaded this type.
			if (Contains(providerTypeName))
				return this[providerTypeName];

			if (!Gorgon.PlugIns.Contains(providerTypeName))
				throw new GorgonException(GorgonResult.CannotCreate, "There was no plug-in named '" + providerTypeName + "'.");

			plugIn = Gorgon.PlugIns[providerTypeName] as GorgonFileSystemProviderPlugIn;

			if (plugIn == null)
				throw new GorgonException(GorgonResult.CannotCreate, "The plug-in '" + providerTypeName + "' is not an Gorgon file system provider plug-in.");

			provider = plugIn.CreateProvider(_fileSystem);

			AddItem(provider);

			return provider;
		}

		/// <summary>
		/// Function to remove a provider by name.
		/// </summary>
		/// <param name="providerTypeName">Fully qualified type name of the provider to remove.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="providerTypeName"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="providerTypeName"/> parameter is an empty string.</exception>
		internal void Remove(string providerTypeName)
		{
			GorgonUtility.AssertParamString(providerTypeName, "providerTypeName");

			this[providerTypeName].OnUnload();

			// Don't unload the primary provider.
			if (string.Compare(providerTypeName, typeof(GorgonFolderFileSystemProvider).FullName, true) == 0)
				return;

			RemoveItem(providerTypeName);
		}

		/// <summary>
		/// Function to remove a provider by its index in the collection.
		/// </summary>
		/// <param name="index">Index of the provider to remove.</param>
		internal void Remove(int index)
		{
			this[index].OnUnload();

			// Don't unload the primary provider.
			if (this[index] is GorgonFolderFileSystemProvider)
				return;

			RemoveItem(index);
		}

		/// <summary>
		/// Function to remove a specific provider instance.
		/// </summary>
		/// <param name="provider">Provider to remove.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="provider"/> parameter is NULL (Nothing in VB.Net).</exception>
		internal void Remove(GorgonFileSystemProvider provider)
		{
			if (provider == null)
				throw new ArgumentNullException("provider");

			provider.OnUnload();

			// Don't unload the primary provider.
			if (provider is GorgonFolderFileSystemProvider)
				return;

			RemoveItem(provider);
		}

		/// <summary>
		/// Function to remove all of the providers.
		/// </summary>
		internal void Clear()
		{
			foreach (GorgonFileSystemProvider provider in this)
				provider.OnUnload();

			ClearItems();

			// Clear any left over items and reset to the root directory.  Add the default folder file system provider.
			AddItem(new GorgonFolderFileSystemProvider(_fileSystem));
			_fileSystem.Clear();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonFileSystemProviderCollection"/> class.
		/// </summary>
		/// <param name="fileSystem">File system that owns this collection.</param>
		internal GorgonFileSystemProviderCollection(GorgonFileSystem fileSystem)
			: base(false)
		{
			_fileSystem = fileSystem;
			AddItem(new GorgonFolderFileSystemProvider(fileSystem));
		}
		#endregion
	}
}
