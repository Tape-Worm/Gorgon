#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: December 13, 2018 3:11:11 PM
// 
#endregion

using System;
using System.Threading;
using Gorgon.Editor.Services;

namespace Gorgon.Editor.ViewModels
{
    /// <summary>
    /// Data used to copy a node from one location to another.
    /// </summary>
    internal class CopyNodeData
    {
        /// <summary>
        /// Property to set or return the default conflict resolution to provide when a conflict is encountered.
        /// </summary>
        /// <remarks>
        /// This value will be the action that is taken when a node conflict is found, and overrides the <see cref="ConflictHandler"/>. If this value is <b>null</b>, then the 
        /// <see cref="ConflictHandler"/> is used to determine how to best proceed (typically by asking the user).
        /// </remarks>
        public FileSystemConflictResolution? DefaultResolution
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the destination node that will receive the copy.
        /// </summary>
        public IFileExplorerNodeVm Destination
        {
            get;
            set;
        }
                
        /// <summary>
        /// Property to return the action used to show progress for the copy operation.
        /// </summary>
        /// <remarks>
        /// The method assigned to this property takes a node representing the node being copied, the number of items remaining to copy, and the total number of items to copy.
        /// </remarks>
        public Action<IFileExplorerNodeVm, long, long> CopyProgress
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the function used to handle conflicts between nodes.
        /// </summary>
        /// <remarks>
        /// The method assigned to this property takes a node representing the node being copied, another node that represents the destination for the copy, a flag to indicate that the operation can be 
        /// cancelled by the user, and another flag to indicate that the resolution can apply to multiple items.  The method will return a <see cref="FileSystemConflictResolution"/> indicating which 
        /// resolution was chosen.
        /// </remarks>
        public Func<IFileExplorerNodeVm, IFileExplorerNodeVm, bool, bool, FileSystemConflictResolution> ConflictHandler
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the token used to cancel the operation.
        /// </summary>
        public CancellationToken CancelToken
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the total size, in bytes, of the items to copy.
        /// </summary>
        public long? TotalSize
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the total number of bytes copied.
        /// </summary>
        public long BytesCopied
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return whether the conflict handler should reuse the resolution given for all items after the first conflict arises.
        /// </summary>
        public bool UseToAllInConflictHandler
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the buffer to use when writing out data in chunks.
        /// </summary>
        public byte[] WriteBuffer
        {
            get;
            set;
        }
    }
}
