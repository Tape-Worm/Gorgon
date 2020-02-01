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
// Created: August 26, 2018 12:31:41 PM
// 
#endregion

using System;

namespace Gorgon.Editor.UI
{
    /// <summary>
    /// An implementation of the <see cref="IEditorCommand{T}"/> interface.
    /// </summary>
    /// <typeparam name="T">The type of data to pass to the command.</typeparam>
    /// <remarks>
    /// <para>
    /// Commands are used to perform actions on a view model. They work similarly to events in that they are usually called in response to a UI action like a button click. 
    /// </para>
    /// </remarks>
    public class EditorCommand<T>
        : IEditorCommand<T>
    {
        #region Variables.
        // Function called to determine if a command can be executed or not.
        private readonly Func<T, bool> _canExecute;
        // Action called to execute the function.
        private readonly Action<T> _execute;
        // Function called to determine if a command can be executed or not.
        private readonly Func<bool> _canExecuteNoArgs;
        // Action called to execute the function.
        private readonly Action _executeNoArgs;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to determine if a command can be executed or not.
        /// </summary>
        /// <param name="args">The arguments to check.</param>
        /// <returns><b>true</b> if the command can be executed, <b>false</b> if not.</returns>
        public bool CanExecute(T args) => (_canExecute == null) && (_canExecuteNoArgs == null) ? true : _canExecute != null ? _canExecute(args) : _canExecuteNoArgs();

        /// <summary>
        /// Function to execute the command.
        /// </summary>
        /// <param name="args">The arguments to pass to the command.</param>
        public void Execute(T args)
        {
            if (_execute != null)
            {
                _execute(args);
                return;
            }

            _executeNoArgs();
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="EditorCommand{T}"/> class.
        /// </summary>
        /// <param name="execute">The method to execute when the command is executed.</param>
        /// <param name="canExecute">The method used to determine if the command can execute.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="execute"/> parameter is <b>null</b>.</exception>
        public EditorCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorCommand{T}" /> class.
        /// </summary>
        /// <param name="execute">The method to execute when the command is executed.</param>
        /// <param name="canExecute">The method used to determine if the command can execute.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="execute"/> parameter is <b>null</b>.</exception>
        public EditorCommand(Action execute, Func<bool> canExecute = null)
        {
            _executeNoArgs = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecuteNoArgs = canExecute;
        }
        #endregion
    }
}
