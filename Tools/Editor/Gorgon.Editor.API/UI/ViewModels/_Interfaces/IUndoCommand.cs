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
// Created: November 17, 2018 1:11:01 PM
// 
#endregion

using System.Threading;
using System.Threading.Tasks;
using Gorgon.Editor.Services;

namespace Gorgon.Editor.UI;

/// <summary>
/// A command used to perform an undo (or redo) action that will store commands (and associated data) to reset state or the opposite.
/// </summary>
/// <remarks>
/// <para>
/// When developing a UI for a content plug in, there will come a time when the user of the content editor will make a mistake. To handle this the undo command interface is used to record steps that 
/// will reverse the operation, and, optionally, restore it.
/// </para>
/// <para>
/// This type is used internally by the <see cref="IUndoService"/> to store state, and does not typically need to be implemented by the developer.
/// </para>
/// </remarks>
public interface IUndoCommand
{
    #region Properties.
    /// <summary>
    /// Property to return the service that owns this command.
    /// </summary>
    IUndoService Service
    {
        get;
    }

    /// <summary>
    /// Property to return whether or not the undo operation is executing.
    /// </summary>
    bool IsExecuting
    {
        get;
    }

    /// <summary>
    /// Property to return the description of the aceiont to undo/redo.
    /// </summary>
    string Description
    {
        get;
    }
    #endregion

    #region Methods.
    /// <summary>
    /// Function to undo the changes performed by this command.
    /// </summary>
    /// <param name="cancelToken">A cancellation token for canceling the operation.</param>
    /// <returns>A the task representing the executing undo operation.</returns>        
    Task Undo(CancellationToken cancelToken);

    /// <summary>
    /// Function to redo the changes that were previously undone.
    /// </summary>
    /// <param name="cancelToken">A cancellation token for canceling the operation.</param>
    /// <returns>A the task representing the executing redo operation.</returns>
    Task Redo(CancellationToken cancelToken);
    #endregion
}
