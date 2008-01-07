#region LGPL.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
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
// Created: Saturday, November 04, 2006 9:58:27 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using SharpUtilities;
using SharpUtilities.Collections;

namespace GorgonLibrary.FileSystems
{
    /// <summary>
    /// Object representing a list of file system file types.
    /// </summary>
    public class FileTypeList
        : SortedCollection<FileType>
    {
        #region Classes.
        /// <summary>
        /// Object to represent a sorter for the file types.
        /// </summary>
        private class FileTypeSorter
            : IComparer<FileType>
        {
            #region IComparer<FileType> Members
            /// <summary>
            /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
            /// </summary>
            /// <param name="x">The first object to compare.</param>
            /// <param name="y">The second object to compare.</param>
            /// <returns>0 if equal, 1 if x &gt; y and -1 if x &lt; y.</returns>
            public int Compare(FileType x, FileType y)
            {
                if (x.SortOrder == y.SortOrder)
                    return 0;

                if (x.SortOrder > y.SortOrder)
                    return 1;
                
                return -1;
            }
            #endregion
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to add a file type to the list.
        /// </summary>
        /// <param name="fileType">File type to add.</param>
        public void Add(FileType fileType)
        {
            if (Contains(fileType.Name))
                throw new DuplicateObjectException(fileType.Name);

            if (Find(fileType.ObjectType) != null)
                throw new CannotRegisterFileTypeException("The object type associated with " + fileType.ToString() + " is already registered.", null);

            AddItem(fileType.Name, fileType);
            // Sort the items.
            Sort();
        }

        /// <summary>
        /// Function to find the file type registered to use the specified object type.
        /// </summary>
        /// <param name="objectType">Type of object that is registered to the file type.</param>
        /// <returns>The file type that is registered to the object type, null if not found.</returns>
        public FileType Find(Type objectType)
        {
            if (objectType == null)
                return null;

            // Find the file type.
            foreach (FileType type in this)
            {
                if (type.ObjectType == objectType)
                    return type;
            }

            return null;
        }
        #endregion

        #region Constructor.
        /// <summary>
        /// Constructor.
        /// </summary>
        internal FileTypeList()
            : base(new FileTypeSorter())
        {            
        }
        #endregion
    }
}
