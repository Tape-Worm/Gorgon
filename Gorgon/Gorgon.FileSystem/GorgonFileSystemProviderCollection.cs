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
using System.Collections.Generic;
using System.Linq;
using Gorgon.Collections;
using Gorgon.Core;
using Gorgon.IO.Properties;

namespace Gorgon.IO
{
	/// <summary>
	/// A collection of file system providers.
	/// </summary>
	public class GorgonFileSystemProviderCollection
		: GorgonBaseNamedObjectDictionary<GorgonFileSystemProvider>
	{
		#region Events.
		/// <summary>
		/// Event triggered when a file system provider is unloaded.
		/// </summary>
		public event EventHandler<GorgonFileSystemProviderUnloadedEventArgs> ProviderUnloaded;
		#endregion

		#region Variables.
		// Synchronization lock for threading.
		private static readonly object _syncLock = new object();
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the item with the specified name.
		/// </summary>
		public GorgonFileSystemProvider this[string name]
		{
			get
			{
				return Items[name];
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to trigger the <see cref="ProviderUnloaded"/> event.
		/// </summary>
		/// <param name="provider">The provider being unloaded.</param>
		private void OnFileSystemProviderUnloaded(GorgonFileSystemProvider provider)
		{
			EventHandler<GorgonFileSystemProviderUnloadedEventArgs> handler = ProviderUnloaded;

			if (handler != null)
			{
				handler(this, new GorgonFileSystemProviderUnloadedEventArgs(provider));
			}
		}

		/// <summary>
		/// Function to unload a file system provider.
		/// </summary>
		/// <param name="provider">The provider to unload.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="provider"/> parameter is <c>null</c> (Nothing in VB.Net).</exception>
		/// <remarks>
		/// This method will unload the provider from the file system, and will unload any file entries associated with that provider. However, directories will still be present.
		/// </remarks>
		public void UnloadProvider(GorgonFileSystemProvider provider)
		{
			if (provider == null)
			{
				throw new ArgumentNullException("provider");
			}

			// If the provider isn't in this list, then continue on.
			if (!Contains(provider))
			{
				return;
			}

			lock (_syncLock)
			{
				OnFileSystemProviderUnloaded(provider);

				provider.OnUnload();
				Items.Remove(provider.Name);
			}
		}

		/// <summary>
		/// Function to unload a file system provider.
		/// </summary>
		/// <param name="name">The name of the provider to unload.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/> parameter is <c>null</c> (Nothing in VB.Net).</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
		/// <exception cref="KeyNotFoundException">Thrown when the provider specified by the <paramref name="name"/> parameter is not found in the file system.</exception>
		/// <remarks>
		/// This method will unload the provider from the file system, and will unload any file entries associated with that provider. However, directories will still be present.
		/// </remarks>
		public void UnloadProvider(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}

			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException(Resources.GORFS_PARAMETER_EMPTY, "name");
			}

			GorgonFileSystemProvider provider;

			if (!TryGetValue(name, out provider))
			{
				throw new KeyNotFoundException(string.Format(Resources.GORFS_PROVIDER_NOT_FOUND, name));
			}

			UnloadProvider(provider);
		}

		/// <summary>
		/// Function to remove all providers from the file system.
		/// </summary>
		/// <remarks>
		/// This method will unload all the providers from the file system, and will unload any file entries associated with those providers. However, directories will still be present.
		/// </remarks>
		public void UnloadAllProviders()
		{
			while (Items.Count > 0)
			{
				UnloadProvider(Items.First().Value);
			}
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

				Items.Add(provider.Name, provider);
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonFileSystemProviderCollection"/> class.
		/// </summary>
		internal GorgonFileSystemProviderCollection()
			: base(false)
		{
		}
		#endregion
	}
}
