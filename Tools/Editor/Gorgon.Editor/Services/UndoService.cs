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
// Created: November 17, 2018 1:26:14 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Editor.UI;

namespace Gorgon.Editor.Services
{
    /// <summary>
    /// The service used to perform an undo/redo operation.
    /// </summary>
    internal class UndoService
        : IUndoService
    {
        #region Variables.
        // The log used for debug messaging.
        private readonly IGorgonLog _log;
        // The index of the current undo item in the stack.
        private int _undoIndex = -1;
        // The stack of undo items.
        private readonly List<IUndoCommand> _undoStack = new List<IUndoCommand>();
        // The cancellation source cancelling the undo/redo operation.
        private CancellationTokenSource _cancelSource;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return whether or not the service can undo.
        /// </summary>
        public bool CanUndo => _undoIndex >= 0;

        /// <summary>
        /// Property to return whether or not the service can redo.
        /// </summary>
        public bool CanRedo => (_undoStack.Count > 0) && (_undoIndex < _undoStack.Count - 1);

        /// <summary>
        /// Property to return the undo items in the undo stack.
        /// </summary>
        public IEnumerable<string> UndoItems => _undoStack.Select(item => item.Description);
        #endregion

        #region Methods.
        /// <summary>
        /// Function to assign a command to the undo stack.
        /// </summary>
        /// <param name="undoCommand">The command to add to the stack.</param>
        private void AssignUndoStack(IUndoCommand undoCommand)
        {
            // Trim to 256 entries from the top of the stack.
            while (_undoStack.Count >= 256)
            {
                _undoStack.RemoveAt(0);
            }

            if (_undoIndex < 0)
            {
                _undoStack.Clear();
                _undoIndex = -1;
            }
            else 
            {
                int lastIndex = _undoStack.Count - 1;
                int diff = lastIndex - _undoIndex;

                for (int i = 0; i < diff; ++i)
                {
                    _undoStack.RemoveAt(_undoStack.Count - 1);
                }
            }

            _undoIndex++;
            _undoStack.Add(undoCommand);
        }

        /// <summary>Function to cancel the currently executing undo/redo operation.</summary>
        public void Cancel()
        {
            if (_cancelSource == null)
            {
                return;
            }

            _cancelSource.Cancel();
        }

        /// <summary>Function to clear the undo/redo stacks.</summary>
        public void ClearStack()
        {
            Cancel();

            // Perform any clean up required for the undo/redo arguments.
            foreach (IDisposable command in _undoStack.OfType<IDisposable>())
            {
                command.Dispose();
            }

            _undoStack.Clear();
            _undoIndex = -1;

            _log.Print("Undo commands cleared.", LoggingLevel.Simple);
        }

        /// <summary>
        /// Function to record an undo/redo state.
        /// </summary>
        /// <typeparam name="TU">The type of parameters to pass to the undo command.</typeparam>
        /// <typeparam name="TR">The type of parameters to pass to the redo command.</typeparam>
        /// <param name="desc">The description of the action being recorded.</param>
        /// <param name="undoAction">The action to execute when undoing.</param>
        /// <param name="redoAction">The action to execute when redoing.</param>
        /// <param name="undoArgs">The parameters to pass to the undo operation.</param>
        /// <param name="redoArgs">The parameters to pass to the redo oprtation.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="desc"/>, <paramref name="undoAction"/>, or the <paramref name="redoAction"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="desc"/> parameter is empty.</exception>
        /// <remarks>
        /// <para>
        /// This method will do nothing if an undo or redo operation is executing.
        /// </para>
        /// </remarks>
        public void Record<TU, TR>(string desc, Func<TU, CancellationToken, Task> undoAction, Func<TR, CancellationToken, Task> redoAction, TU undoArgs, TR redoArgs)
            where TU : class
            where TR : class
        {            
            // Do not register if we're in the middle of an undo/redo operation.
            if (_undoStack.Any(item => item.IsExecuting))
            {
                return;
            }

            _log.Print($"Adding undo command '{desc}' to stack.", LoggingLevel.Verbose);
            AssignUndoStack(new UndoCommand<TU, TR>(this, desc, undoAction, redoAction, redoArgs, undoArgs));
        }

        /// <summary>Function to perform a redo operation.</summary>
        /// <returns>A task representing the currently executing redo operation.</returns>
        public Task Redo()
        {
            if ((_undoStack.Count == 0) || (_undoIndex >= _undoStack.Count - 1))
            {
                _undoIndex = _undoStack.Count - 1;
                return Task.FromResult<object>(null);
            }

            try
            {
                IUndoCommand redo = _undoStack[++_undoIndex];

                if (redo == null)
                {
                    return Task.FromResult<object>(null);
                }

                _cancelSource = new CancellationTokenSource();
                _log.Print($"Executing redo for '{redo.Description}' command.", LoggingLevel.Simple);
                Task task = redo.Redo(_cancelSource.Token);

                if (_cancelSource.Token.IsCancellationRequested)
                {
                    --_undoIndex;
                }

                return task;
            }
            finally
            {
                _cancelSource?.Dispose();
                _cancelSource = null;
            }
        }

        /// <summary>Function to perform an undo operation.</summary>
        /// <returns>A task representing the currently executing undo operation.</returns>
        public Task Undo()
        {
            if ((_undoStack.Count == 0) || (_undoIndex < 0))
            {
                _undoIndex = -1;
                return Task.FromResult<object>(null);
            }

            try
            {
                IUndoCommand undo = _undoStack[_undoIndex--];

                if (undo == null)
                {
                    return Task.FromResult<object>(null);
                }

                _cancelSource = new CancellationTokenSource();
                _log.Print($"Executing undo command for '{undo.Description}'.", LoggingLevel.Simple);
                Task task = undo.Undo(_cancelSource.Token);

                if (_cancelSource.Token.IsCancellationRequested)
                {
                    ++_undoIndex;
                }

                return task;
            }
            finally
            {
                _cancelSource?.Dispose();
                _cancelSource = null;
            }
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.Services.UndoService"/> class.</summary>
        /// <param name="log">The log used for debug messaging.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="log" /> parameter is <strong>null</strong>.</exception>
        public UndoService(IGorgonLog log) => _log = log ?? GorgonLog.NullLog;
        #endregion
    }
}
