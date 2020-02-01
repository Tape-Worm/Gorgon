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
// Created: August 26, 2018 12:30:48 PM
// 
#endregion

namespace Gorgon.Editor.UI
{
    /// <summary>
    /// An command used to carry out an operation.
    /// </summary>
    /// <typeparam name="T">The type of data to pass to the command.</typeparam>
    /// <remarks>
    /// <para>
    /// Commands are used to perform actions on a view model. They work similarly to events in that they are usually called in response to a UI action like a button click. 
    /// </para>
    /// </remarks>
    public interface IEditorCommand<in T>
    {
        /// <summary>
        /// Function to determine if a command can be executed or not.
        /// </summary>
        /// <param name="args">The arguments to check.</param>
        /// <returns><b>true</b> if the command can be executed, <b>false</b> if not.</returns>
        bool CanExecute(T args);

        /// <summary>
        /// Function to execute the command.
        /// </summary>
        /// <param name="args">The arguments to pass to the command.</param>
        void Execute(T args);
    }
}
