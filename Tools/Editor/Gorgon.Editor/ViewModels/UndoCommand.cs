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
// Created: November 17, 2018 3:29:06 PM
// 
#endregion

using System;
using System.Threading;
using System.Threading.Tasks;
using Gorgon.Core;
using Gorgon.Editor.Services;

namespace Gorgon.Editor.UI;

/// <summary>
/// An editor command with undo capability.
/// </summary>
/// <typeparam name="TU">The type of undo arguments.</typeparam>
/// <typeparam name="TR">The type of redo arguments.</typeparam>
internal class UndoCommand<TU, TR>
    : IUndoCommand, IDisposable
    where TU : class
    where TR : class
{
    #region Variables.
    // Flag to indicate that the undo operation is executing.
    private int _isExecuting;
    // The undo command parameters.
    private TU _undoArgs;
    // The redo command parameters.
    private TR _redoArgs;
    // The action to execute when undoing  instead of a command.
    private readonly Func<TU, CancellationToken, Task> _undoAction;
    // The action to execute when redoing instead of a command.
    private readonly Func<TR, CancellationToken, Task> _redoAction;
    #endregion

    #region Properties.
    /// <summary>
    /// Property to return the service that owns this command.
    /// </summary>
    public IUndoService Service
    {
        get;
    }

    /// <summary>Property to return whether or not the undo operation is executing.</summary>
    /// <value>
    ///   <c>true</c> if this instance is executing; otherwise, <c>false</c>.</value>
    public bool IsExecuting => _isExecuting != 0;

    /// <summary>
    /// Property to return the description of the aceiont to undo/redo.
    /// </summary>
    public string Description
    {
        get;
    }
    #endregion

    #region Methods.
    /// <summary>Function to undo the changes performed by this command.</summary>
    /// <param name="cancelToken">A cancellation token for canceling the operation.</param>
    /// <returns>A the task representing the executing undo operation.</returns>
    public async Task Undo(CancellationToken cancelToken)
    {
        if (Interlocked.Exchange(ref _isExecuting, 1) == 1)
        {
            return;
        }

        try
        {
            await _undoAction(_undoArgs, cancelToken);
        }
        finally
        {
            Interlocked.Exchange(ref _isExecuting, 0);
        }
    }

    /// <summary>Function to redo the changes that were previously undone.</summary>
    /// <param name="cancelToken">A cancellation token for canceling the operation.</param>
    /// <returns>A the task representing the executing redo operation.</returns>
    public async Task Redo(CancellationToken cancelToken)
    {
        if (Interlocked.Exchange(ref _isExecuting, 1) == 1)
        {
            return;
        }

        try
        {
            await _redoAction(_redoArgs, cancelToken);
        }
        finally
        {
            Interlocked.Exchange(ref _isExecuting, 0);
        }
    }

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    public void Dispose()
    {
        TU undoArgs = Interlocked.Exchange(ref _undoArgs, null);
        TR redoArgs = Interlocked.Exchange(ref _redoArgs, null);

        var disposeUndo = undoArgs as IDisposable;
        var disposeRedo = redoArgs as IDisposable;

        disposeUndo?.Dispose();
        disposeRedo?.Dispose();
    }
    #endregion

    #region Constructor/Finalizer.
    /// <summary>
    /// Initializes a new instance of the <see cref="EditorCommand{T}"/> class.
    /// </summary>        
    /// <param name="service">The undo service that owns this command.</param>
    /// <param name="desc">The description of the action.</param>
    /// <param name="undoAction">The undo action execute.</param>
    /// <param name="redoAction">The redo action to execute.</param>
    /// <param name="redoArgs">The arguments to pass to the redo functionality.</param>
    /// <param name="undoArgs">The arguments to pass to the undo functionality.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="service"/>, <paramref name="desc"/>, <paramref name="undoAction"/>, or the <paramref name="redoAction"/> parameter is <b>null</b>.</exception>        
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="desc"/> parameter is empty.</exception>
    public UndoCommand(IUndoService service, string desc, Func<TU, CancellationToken, Task> undoAction, Func<TR, CancellationToken, Task> redoAction, TR redoArgs, TU undoArgs)
    {
        Service = service ?? throw new ArgumentNullException(nameof(service));

        if (desc is null)
        {
            throw new ArgumentNullException(nameof(desc));
        }

        if (string.IsNullOrWhiteSpace(desc))
        {
            throw new ArgumentEmptyException(nameof(desc));
        }

        Description = desc;
        _undoAction = undoAction ?? throw new ArgumentNullException(nameof(undoAction));
        _redoAction = redoAction ?? throw new ArgumentNullException(nameof(redoAction));
        _redoArgs = redoArgs;
        _undoArgs = undoArgs;
    }
    #endregion
}
