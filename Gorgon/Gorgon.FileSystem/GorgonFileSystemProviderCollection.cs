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
using System.Linq;
using System.Threading;
using Gorgon.Collections;
using Gorgon.Core;
using Gorgon.IO.Properties;

namespace Gorgon.IO
{
	/// <summary>
	/// A collection of file system providers.
	/// </summary>
	public class GorgonFileSystemProviderCollection
		: GorgonBaseNamedObjectCollection<GorgonFileSystemProvider>
	{
		#region Variables.
		private static readonly object _syncLock = new object();	// Synchronization lock.
		private readonly GorgonFileSystem _fileSystem;			    // File system that owns this collection.
		private int _incVar;										// Synchronization increment.
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
        /// Function to remove an item from the collection by index.
        /// </summary>
        /// <param name="provider">The provider to remove.</param>
        protected override void RemoveItem(GorgonFileSystemProvider provider)
        {
		    // Perform any special clean up.
		    provider.OnUnload();

		    // Remove any files attached to this provider.
		    var files = _fileSystem.FindFiles("*", true).Where(file => file.Provider == provider);

		    foreach (GorgonFileSystemFileEntry file in files)
		    {
			    file.Directory.Files.Remove(file);
		    }

		    base.RemoveItem(provider);
        }

		/// <summary>
		/// Function to search through the plug-in list and load any providers that haven't already been loaded.
		/// </summary>
		/// <remarks>This is a convenience method to allow mass loading of file system providers.</remarks>
		public void LoadAllProviders()
		{
			var plugIns = from plugInList in GorgonApplication.PlugIns
						  where plugInList is GorgonFileSystemProviderPlugIn
						  select plugInList;

			foreach (var plugIn in plugIns)
			{
				LoadProvider(plugIn.Name);
			}
		}

        /// <summary>
        /// Function to load a new provider from its plug-in type.
        /// </summary>
        /// <param name="providerType">Plug-in type for the provider.</param>
        public void LoadProvider(Type providerType)
        {
            if (providerType == null)
            {
                throw new ArgumentNullException("providerType");
            }

            LoadProvider(providerType.FullName);
        }

		/// <summary>
		/// Function to add a new provider based on the provider type name.
		/// </summary>
		/// <param name="providerName">The fully qualified type name of the provider plug-in.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="providerName"/> parameter is NULL (Nothing in VB.Net).</exception>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="providerName"/> parameter is empty.</exception>
        /// <exception cref="System.InvalidCastException">Thrown when the plug-in was not found or was not of the correct type.</exception>
        /// <exception cref="GorgonException">Thrown when the provider interface could not be created.</exception>
		public void LoadProvider(string providerName)
		{
			if (providerName == null)
			{
				throw new ArgumentNullException("providerName");
			}

			if (string.IsNullOrWhiteSpace(providerName))
			{
				throw new ArgumentException(Resources.GORFS_PARAMETER_EMPTY, "providerName");
			}

			// Don't allow two threads to add the same thing.
			lock(_syncLock)
			{
				// Check to see if we've already loaded this type.
				if (Contains(providerName))
				{
					return;
				}

				// Find the plug-in.
				var plugIn =
					GorgonApplication.PlugIns.FirstOrDefault(
						item => string.Equals(providerName, item.Name, StringComparison.OrdinalIgnoreCase)) as
					GorgonFileSystemProviderPlugIn;

				if (plugIn == null)
				{
                    throw new InvalidCastException(string.Format(Resources.GORFS_NO_PROVIDER_PLUGIN, providerName));
				}

				GorgonFileSystemProvider provider = plugIn.CreateProvider();

				if (provider == null)
				{
					throw new GorgonException(GorgonResult.CannotCreate,
					                          string.Format(Resources.GORFS_PROVIDER_INVALID, providerName));
				}

				AddItem(provider);
			}
		}

		/// <summary>
		/// Function to unload the file system provider from the file system.
		/// </summary>
		/// <param name="provider">The provider to remove.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="provider"/> parameter is NULL (Nothing in VB.Net).</exception>
		public void Unload(GorgonFileSystemProvider provider)
		{
			if (provider == null)
			{
				throw new ArgumentNullException("provider");
			}

			lock(_syncLock)
			{
				if (!Contains(provider))
				{
					throw new ArgumentException(string.Format(Resources.GORFS_PROVIDER_NOT_FOUND, provider.GetType().FullName),
					                            "provider");
				}

				RemoveItem(provider);
			}
		}

		/// <summary>
		/// Function to unload a file system provider by index in the collection.
		/// </summary>
		/// <param name="index">Index of the file system provider to unload.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> is less than 0 or greater than or equal to the <see cref="Gorgon.Collections.GorgonBaseNamedObjectCollection{T}.Count">Count</see>.</exception>
		public void Unload(int index)
		{
			lock(_syncLock)
			{
				if ((index < 0) || (index >= Count))
				{
					throw new ArgumentOutOfRangeException("index", string.Format(Resources.GORFS_INDEX_OUT_OF_RANGE, index, Count));
				}

				Unload(GetItem(index));
			}
		}

		/// <summary>
		/// Function to unload a file system provider by its type name.
		/// </summary>
		/// <param name="providerType">Type of the file system provider.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="providerType"/> is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when the <paramref name="providerType"/> was not found in the collection.</exception>
		public void Unload(Type providerType)
		{
            if (providerType == null)
            {
                throw new ArgumentNullException("providerType");
            }

		    Unload(providerType.FullName);
		}

		/// <summary>
		/// Function to unload a file system provider by its type name.
		/// </summary>
		/// <param name="providerTypeName">Name of the file system provider type.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="providerTypeName"/> is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="providerTypeName"/> is empty.</exception>
		/// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when the name in <paramref name="providerTypeName"/> was not found in the collection.</exception>
		public void Unload(string providerTypeName)
		{
			if (providerTypeName == null)
			{
				throw new ArgumentNullException("providerTypeName");
			}

			if (string.IsNullOrWhiteSpace(providerTypeName))
			{
				throw new ArgumentException(Resources.GORFS_PARAMETER_EMPTY, "providerTypeName");
			}

			lock(_syncLock)
			{
				int index = IndexOf(providerTypeName);

				if (index < 0)
				{
					throw new ArgumentException(string.Format(Resources.GORFS_PROVIDER_NOT_FOUND, providerTypeName), "providerTypeName");
				}

				Unload(GetItem(index));
			}
		}

		/// <summary>
		/// Function to remove all of the providers.
		/// </summary>
		public void UnloadAll()
		{
		    try
		    {
		        if (Interlocked.Increment(ref _incVar) > 1)
		        {
		            return;
		        }

		        // Remove file system provider data.
		        foreach (GorgonFileSystemProvider provider in this)
		        {
		            provider.OnUnload();
		        }

		        ClearItems();

		        // Clear any left over items and reset to the root directory.  Add the default folder file system provider.
		        _fileSystem.Clear();
		    }
		    finally
		    {
		        Interlocked.Decrement(ref _incVar);
		    }
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
		}
		#endregion
	}
}
