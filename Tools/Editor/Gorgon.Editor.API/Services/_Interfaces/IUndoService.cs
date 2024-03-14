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
// Created: November 17, 2018 1:17:48 PM
// 
#endregion

namespace Gorgon.Editor.Services;

/// <summary>
/// The service used to supply undo/redo functionality.
/// </summary>
public interface IUndoService
{
    #region Properties.
    /// <summary>
    /// Property to return whether or not the service can undo.
    /// </summary>
    bool CanUndo
    {
        get;
    }

    /// <summary>
    /// Property to return whether or not the service can redo.
    /// </summary>
    bool CanRedo
    {
        get;
    }

    /// <summary>
    /// Property to return the undo items in the undo stack.
    /// </summary>
    IEnumerable<string> UndoItems
    {
        get;
    }
    #endregion

    #region Methods.
    /// <summary>
    /// Function to cancel the currently executing undo/redo operation.
    /// </summary>
    void Cancel();

    /// <summary>
    /// Function to perform an undo operation.
    /// </summary>
    /// <returns>A task representing the currently executing undo operation.</returns>
    Task Undo();

    /// <summary>
    /// Function to perform a redo operation.
    /// </summary>
    /// <returns>A task representing the currently executing redo operation.</returns>
    Task Redo();

    /// <summary>
    /// Function to record an undo/redo state.
    /// </summary>
    /// <typeparam name="TU">The type of undo parameters to pass. Must be a reference type.</typeparam>
    /// <typeparam name="TR">The type of redo parameters to pass. Must be a reference type.</typeparam>
    /// <param name="desc">The description of the action being recorded.</param>
    /// <param name="undoAction">The action to execute when undoing.</param>
    /// <param name="redoAction">The action to execute when redoing.</param>
    /// <param name="undoArgs">The parameters to pass to the undo operation.</param>
    /// <param name="redoArgs">The parameters to pass to the redo oprtation.</param>
    /// <remarks>
    /// <para>
    /// This method will do nothing if an undo or redo operation is executing.
    /// </para>
    /// </remarks>
    void Record<TU, TR>(string desc, Func<TU, CancellationToken, Task> undoAction, Func<TR, CancellationToken, Task> redoAction, TU undoArgs, TR redoArgs)
        where TU : class
        where TR : class;

    /// <summary>
    /// Function to clear the undo/redo stacks.
    /// </summary>
    void ClearStack();
    #endregion
}
