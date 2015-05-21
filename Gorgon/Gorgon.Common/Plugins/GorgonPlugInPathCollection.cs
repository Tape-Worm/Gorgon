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
using System.IO;
using Gorgon.Core;
using Gorgon.Core.Properties;
using Gorgon.IO;
using Gorgon.Diagnostics;

namespace Gorgon.PlugIns
{
	/// <summary>
	/// Collection to hold search paths for plug-ins.
	/// </summary>
	/// <remarks>By default, this object is initialized with multiple search paths, in order of preference.  These search paths are:
	/// <para>
	/// <list type="number">
	/// <item><description>The directory of the executable.</description></item>
	/// <item><description>The working directory of the executable.</description></item>
	/// <item><description>The system directory.</description></item>
	/// <item><description>The directories listed in the PATH environment variable.</description></item>
	/// </list>
	/// </para>
	/// <para>Calling the <see cref="M:GorgonLibrary.PlugIns.GorgonPlugInPathCollection.Clear">Clear</see> method will remove these paths and give an empty collection.  
	/// Call the <see cref="M:GorgonLibrary.PlugIns.GorgonPlugInPathCollection.GetDefaultPaths">GetDefaultPaths</see> method to restore these paths.</para>
	/// </remarks>
	public class GorgonPlugInPathCollection
		: IList<string>
    {
        #region Variables.
        private readonly List<string> _paths;
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
                throw new ArgumentNullException("path");
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException(Resources.GOR_PARAMETER_MUST_NOT_BE_EMPTY, "path");
            }
			
			return Path.GetFullPath(path).FormatDirectory(Path.DirectorySeparatorChar);
		}

		/// <summary>
		/// Function to remove a path entry.
		/// </summary>
		/// <param name="item">Item to remove</param>
		public void Remove(string item)
		{
			GorgonDebug.AssertParamString(item, "item");

			_paths.Remove(item);
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
		public void GetDefaultPaths()
		{
			// Add default paths:
			// 1. Local directory.
			Add(GorgonApplication.ApplicationDirectory);
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

		    var path = variables["Path"].ToString().Split(new[]
		        {
		            ';'
		        }, StringSplitOptions.RemoveEmptyEntries);

		    foreach (var pathEntry in path)
		    {
		        Add(pathEntry);
		    }
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonPlugInPathCollection"/> class.
		/// </summary>
		internal GorgonPlugInPathCollection()
		{
			_paths = new List<string>();
			GetDefaultPaths();
		}
		#endregion

		#region IList<string> Members
		#region Properties.
		/// <summary>
		/// Gets or sets the <see cref="System.String"/> at the specified index.
		/// </summary>
		/// <value></value>
		public string this[int index]
		{
			get
			{
				return _paths[index];
			}
			set
			{
				_paths[index] = ValidatePath(value);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Indexes the of.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns></returns>
		public int IndexOf(string item)
		{
			return _paths.IndexOf(item);
		}

		/// <summary>
		/// Inserts the specified index.
		/// </summary>
		/// <param name="index">The index.</param>
		/// <param name="item">The item.</param>
		public void Insert(int index, string item)
		{
			GorgonDebug.AssertParamString(item, "item");

			if (Contains(ValidatePath(item)))
				return;

			_paths.Insert(index, item);
		}

		/// <summary>
		/// Removes at.
		/// </summary>
		/// <param name="index">The index.</param>
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
		public int Count
		{
			get 
			{
				return _paths.Count;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance is read only.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is read only; otherwise, <c>false</c>.
		/// </value>
		bool ICollection<string>.IsReadOnly
		{
			get 
			{
				return false;
			}
		}
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
			Remove(item);
	        return true;
		}

		/// <summary>
		/// Function add a new path to the collection.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <remarks>If the path is already in the collection, it will not be added again.</remarks>
		public void Add(string item)
		{
			if (Contains(ValidatePath(item)))
				return;

			_paths.Add(ValidatePath(item));
		}

		/// <summary>
		/// Function to clear the collection.
		/// </summary>
		/// <remarks>Note that this will not restore the default search paths.  Call <see cref="M:GorgonLibrary.PlugIns.GorgonPlugInPathCollection.GetDefaultPaths">GetDefaultPaths</see> method to restore those paths.</remarks>
		public void Clear()
		{
			_paths.Clear();
		}

		/// <summary>
		/// Determines whether [contains] [the specified item].
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns>
		/// 	<c>true</c> if [contains] [the specified item]; otherwise, <c>false</c>.
		/// </returns>
		public bool Contains(string item)
		{
			return _paths.Contains(item);
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
		/// Gets the enumerator.
		/// </summary>
		/// <returns></returns>
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
