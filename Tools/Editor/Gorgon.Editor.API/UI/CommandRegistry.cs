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
// Created: September 8, 2018 3:52:23 PM
// 
#endregion

using System;
using System.Collections.Concurrent;
using Gorgon.Core;

namespace Gorgon.Editor.UI
{
    /// <summary>
    /// A registry used to route ribbon button events.
    /// </summary>
    public static class CommandRegistry
    {
        #region Variables.
        // The list of registrations.
        private static ConcurrentDictionary<string, Action> _registrations = new ConcurrentDictionary<string, Action>();
        #endregion

        #region Methods.
        /// <summary>
        /// Function to register a command with the system so the action for the command can be routed.
        /// </summary>
        /// <param name="commandItem">The name of the command item.</param>
        /// <param name="action">The action to execute.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="commandItem"/> parameter is <b>null.</b></exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="commandItem"/> parameter is empty.</exception>
        public static void Register(string commandItem, Action action)
        {
            if (commandItem == null)
            {
                throw new ArgumentNullException(nameof(commandItem));
            }

            if (string.IsNullOrWhiteSpace(commandItem))
            {
                throw new ArgumentEmptyException(nameof(commandItem));
            }

            _registrations.AddOrUpdate(commandItem, action, (k, v) => action);
        }

        /// <summary>
        /// Function to remove a registration for a command.
        /// </summary>
        /// <param name="commandItem">Command to unregister.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="commandItem"/> parameter is <b>null.</b></exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="commandItem"/> parameter is empty.</exception>
        public static void Unregister(string commandItem)
        {
            if (commandItem == null)
            {
                throw new ArgumentNullException(nameof(commandItem));
            }

            if (string.IsNullOrWhiteSpace(commandItem))
            {
                throw new ArgumentEmptyException(nameof(commandItem));
            }

            _registrations.TryRemove(commandItem, out Action _);
        }

        /// <summary>
        /// Function to execute a command action.
        /// </summary>
        /// <param name="commandItem">The command action to execute.</param>
        public static void Execute(string commandItem)
        {
            if (commandItem == null)
            {
                throw new ArgumentNullException(nameof(commandItem));
            }

            if (string.IsNullOrWhiteSpace(commandItem))
            {
                throw new ArgumentEmptyException(nameof(commandItem));
            }

            if (!_registrations.TryGetValue(commandItem, out Action action))
            {
                return;
            }

            action();
        }
        #endregion
    }
}
