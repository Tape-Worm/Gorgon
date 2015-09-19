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
// Created: Monday, July 04, 2011 10:14:22 AM
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Gorgon.Core.Properties;
using Gorgon.IO;

namespace Gorgon.Plugins
{
	/// <summary>
	/// Collection to hold search paths for plugin assemblies.
	/// </summary>
	/// <remarks>
	/// <para>
	/// By default, this object is initialized with multiple search paths, in order of preference.  These search paths are:
	/// <list type="number">
	/// <item><description>The directory of the executable.</description></item>
	/// <item><description>The working directory of the executable.</description></item>
	/// <item><description>The system directory.</description></item>
	/// <item><description>The directories listed in the PATH environment variable.</description></item>
	/// </list>
	/// </para>
	/// <para>
	/// Users are free to add and remove paths from this list as they see fit, but that calling the <see cref="Clear"/> method will remove these paths.  In order to refresh these paths, call <see cref="GetDefaultPaths"/>.
	/// </para>
	/// <para>
	/// This collection is -not- thread safe.
	/// </para>
	/// </remarks>
	public sealed class GorgonPluginPathCollection
		: IList<string>, IReadOnlyList<string>
	{
        #region Variables.
		// The list of paths.
        private readonly List<string> _paths = new List<string>();
		#endregion

		#region Methods.
		/// <summary>
		/// Function to validate the path.
		/// </summary>
		/// <param name="path">Path to validate.</param>
		/// <returns>The validated path.</returns>
		private static string ValidatePath(string path)
		{
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException(Resources.GOR_ERR_PARAMETER_MUST_NOT_BE_EMPTY, nameof(path));
            }
			
			return Path.GetFullPath(path).FormatDirectory(Path.DirectorySeparatorChar);
		}

		/// <summary>
		/// Function to remove a path entry from the collection.
		/// </summary>
		/// <param name="path">The path to remove from the list.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="path"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="path"/> parameter is empty.</exception>
		/// <exception cref="DirectoryNotFoundException">Thrown when the <paramref name="path"/> specified is not in the collection.</exception>
		public void Remove(string path)
		{
			path = ValidatePath(path);

			int index = IndexOf(path);

			if (index == -1)
			{
				throw new DirectoryNotFoundException(string.Format(Resources.GOR_ERR_PATH_NOT_FOUND, path));
			}

			_paths.RemoveAt(index);
		}

		/// <summary>
		/// Function to remove a path entry by index.
		/// </summary>
		/// <param name="index">Index of the path to remove.</param>
		public void Remove(int index)
		{
			_paths.RemoveAt(index);
		}

		/// <summary>
		/// Function to append the default paths to the collection.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This will add a list of paths to the collection. The paths retrieved are (in order):
		/// <list type="number">
		/// <item><description>The directory of the application executable.</description></item>
		/// <item><description>The working directory of the executable.</description></item>
		/// <item><description>The system directory.</description></item>
		/// <item><description>The directories listed in the PATH environment variable.</description></item>
		/// </list>
		/// The first item in the list will only work in assemblies that are not called from unmanaged code, or from web applications. Gorgon will attempt to use the directory of the assembly 
		/// that called this method in those cases, but that path may not be accurate.
		/// </para>
		/// <para>
		/// If any of the paths are already present in the collection, they will not be added a second time.
		/// </para>
		/// </remarks>
		public void GetDefaultPaths()
		{
			// Attempt to get the executable directory. If we can't find that, then try to get the assembly that called this method.
			Assembly currentAssembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();

			// This should never be the case.
			Debug.Assert(currentAssembly != null, "Current assembly is <b>null</b>!!");

			Uri assemblyUri = new Uri(currentAssembly.CodeBase);
			
			// Add default paths:
			// 1. Application directory.
			if (!string.IsNullOrWhiteSpace(assemblyUri.AbsolutePath))
			{
				string assemblyPath = Uri.UnescapeDataString(assemblyUri.AbsolutePath);
				assemblyPath = Path.GetDirectoryName(assemblyPath).FormatDirectory(Path.DirectorySeparatorChar);

				if (!string.IsNullOrWhiteSpace(assemblyPath))
				{
					Add(assemblyPath);
				}
			}

			// 2. Working directory.
			Add(Environment.CurrentDirectory);
			// 3. System directory.
			Add(Environment.GetFolderPath(Environment.SpecialFolder.System));
			// 4. x86 system directory.
			Add(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86));
			// 5. PATH.
			var variables = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Process);

		    if (!variables.Contains("Path"))
		    {
		        return;
		    }

			IEnumerable<string> paths = from envPath in variables["Path"].ToString().Split(new[]
			                                                                               {
				                                                                               ';'
			                                                                               },
			                                                                               StringSplitOptions.RemoveEmptyEntries)
			                            where !string.IsNullOrWhiteSpace(envPath)
			                            let formattedPath = ValidatePath(envPath)
			                            where !string.IsNullOrWhiteSpace(envPath)
			                            select formattedPath;

			foreach (string path in paths)
			{
				Add(path);
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonPluginPathCollection"/> class.
		/// </summary>
		internal GorgonPluginPathCollection()
		{
		}
		#endregion

		#region IList<string> Members
		#region Properties.
		/// <summary>
		/// Gets or sets the path at the specified index.
		/// </summary>
		public string this[int index]
		{
			get
			{
				return _paths[index];
			}
			set
			{
				if (string.IsNullOrWhiteSpace(value))
				{
					return;
				}

				value = ValidatePath(value);

				if ((IndexOf(value) != -1)
					|| (string.IsNullOrWhiteSpace(value)))
				{
					return;
				}

				_paths[index] = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Determines the index of a specific item in the collection.
		/// </summary>
		/// <param name="path">The object to locate in the collection.</param>
		/// <returns>
		/// The index of <paramref name="path" /> if found in the list; otherwise, -1.
		/// </returns>
		public int IndexOf(string path)
		{
			if (string.IsNullOrWhiteSpace(path))
			{
				return -1;
			}

			path = ValidatePath(path);

			return _paths.FindIndex(_ => string.Equals(path, _, StringComparison.OrdinalIgnoreCase));
		}

		/// <summary>
		/// Inserts an item to the collection at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which <paramref name="path" /> should be inserted.</param>
		/// <param name="path">The object to insert into the <see cref="T:System.Collections.Generic.IList`1" />.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="path"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="path"/> parameter is empty.</exception>
		public void Insert(int index, string path)
		{
			path = ValidatePath(path);

			if ((IndexOf(path) != -1)
				|| (string.IsNullOrWhiteSpace(path)))
			{
				return;
			}

			_paths.Insert(index, path);
		}

		/// <summary>
		/// Removes the <see cref="T:System.Collections.Generic.IList`1" /> item at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the item to remove.</param>
		void IList<string>.RemoveAt(int index)
		{
			Remove(index);
		}
		#endregion
		#endregion

		#region ICollection<string> Members
		#region Properties.
		/// <summary>
		/// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </summary>
		/// <returns>
		/// The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		///   </returns>
		public int Count => _paths.Count;

		/// <summary>
		/// Gets a value indicating whether this instance is read only.
		/// </summary>
		/// <value>
		/// 	<b>true</b> if this instance is read only; otherwise, <b>false</b>.
		/// </value>
		bool ICollection<string>.IsReadOnly => false;

		#endregion

		#region Methods.
		/// <summary>
		/// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <returns>
		/// true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </returns>
	    bool ICollection<string>.Remove(string item)
		{
			if (string.IsNullOrWhiteSpace(item))
			{
				return false;
			}

			int index = IndexOf(ValidatePath(item));

			if (index == -1)
			{
				return false;
			}

			_paths.RemoveAt(index);
			return true;
		}

		/// <summary>
		/// Function add a new path to the collection.
		/// </summary>
		/// <param name="path">The path to add to the collection.</param>
		/// <remarks>If the path is already in the collection, it will not be added again.</remarks>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="path"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="path"/> is empty.</exception>
		public void Add(string path)
		{
			path = ValidatePath(path);

			if (IndexOf(path) == -1)
			{
				return;
			}

			_paths.Add(path);
		}

		/// <summary>
		/// Function to clear the collection.
		/// </summary>
		/// <remarks>Note that this will not restore the default search paths.  Call <see cref="GetDefaultPaths"/> method to restore those paths.</remarks>
		public void Clear()
		{
			_paths.Clear();
		}

		/// <summary>
		/// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
		/// </summary>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <returns>
		/// true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.
		/// </returns>
		public bool Contains(string item)
		{
			if (string.IsNullOrWhiteSpace(item))
			{
				return false;
			}

			item = ValidatePath(item);

			return IndexOf(item) != -1;
		}

		/// <summary>
		/// Copies to.
		/// </summary>
		/// <param name="array">The array.</param>
		/// <param name="arrayIndex">Index of the array.</param>
        public void CopyTo(string[] array, int arrayIndex)
        {
            _paths.CopyTo(array, arrayIndex);
        }
        #endregion
		#endregion

		#region IEnumerable<string> Members
		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<string> GetEnumerator()
		{
		    return _paths.GetEnumerator();
		}
		#endregion

		#region IEnumerable Members
		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
		    return ((IEnumerable) _paths).GetEnumerator();
		}
		#endregion
	}
}
