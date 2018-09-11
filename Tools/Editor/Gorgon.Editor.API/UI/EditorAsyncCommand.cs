﻿#region MIT
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
// Created: August 26, 2018 12:31:41 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gorgon.Editor.UI
{
    /// <summary>
    /// An implementation of the <see cref="IEditorAsyncCommand{T}"/> interface.
    /// </summary>
    /// <typeparam name="T">The type of data to pass to the command.</typeparam>
    public class EditorAsyncCommand<T>
        : IEditorAsyncCommand<T>
    {
        #region Variables.
        // Function called to determine if a command can be executed or not.
        private readonly Func<T, bool> _canExecute;
        // Action called to execute the function.
        private readonly Func<T, Task> _execute;
        // Function called to determine if a command can be executed or not.
        private readonly Func<bool> _canExecuteNoArgs;
        // Action called to execute the function.
        private readonly Func<Task> _executeNoArgs;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to execute the command.
        /// </summary>
        /// <param name="args">The arguments to pass to the command.</param>
        public async void Execute(T args) => await ExecuteAsync(args);

        /// <summary>
        /// Function to execute the command.
        /// </summary>
        /// <param name="args">The arguments to pass to the command.</param>
        public Task ExecuteAsync(T args)
        {
            if (_execute != null)
            {
                return _execute(args);
            }

            return _executeNoArgs();
        }

        /// <summary>
        /// Function to determine if a command can be executed or not.
        /// </summary>
        /// <param name="args">The arguments to check.</param>
        /// <returns><b>true</b> if the command can be executed, <b>false</b> if not.</returns>
        public bool CanExecute(T args)
        {
            if ((_canExecute == null) && (_canExecuteNoArgs == null))
            {
                return true;
            }

            if (_canExecute != null)
            {
                return _canExecute(args);
            }

            return _canExecuteNoArgs();
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="EditorCommand{T}"/> class.
        /// </summary>
        /// <param name="execute">The method to execute when the command is executed.</param>
        /// <param name="canExecute">The method used to determine if the command can execute.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="execute"/> parameter is <b>null</b>.</exception>
        public EditorAsyncCommand(Func<T, Task> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorCommand{T}"/> class.
        /// </summary>
        /// <param name="execute">The method to execute when the command is executed.</param>
        /// <param name="canExecute">The method used to determine if the command can execute.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="execute"/> parameter is <b>null</b>.</exception>
        public EditorAsyncCommand(Func<Task> execute, Func<bool> canExecute = null)
        {
            _executeNoArgs = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecuteNoArgs = canExecute;
        }
        #endregion

    }
}
