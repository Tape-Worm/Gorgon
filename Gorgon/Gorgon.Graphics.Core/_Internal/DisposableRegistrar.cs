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
// Created: April 7, 2018 11:01:10 PM
// 
#endregion

using System;
using System.Collections.Generic;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Allows tracking of objects that implement IDisposable.
/// </summary>
internal static class DisposableRegistrar
{
    #region Variables.
    // The list of disposable objects.
    private static readonly Dictionary<GorgonGraphics, List<WeakReference<IDisposable>>> _disposables = new();
    // Synchronization object for multiple threads.
    private static readonly object _syncLock = new();
    #endregion

    #region Methods.
    /// <summary>
    /// Function to retrieve the list of disposables for a given graphics instance.
    /// </summary>
    /// <param name="graphics">The graphics instance that owns the disposable resources.</param>
    /// <returns>An enumerable containing the list of resources for the graphics instance.</returns>
    public static IEnumerable<WeakReference<IDisposable>> GetDisposables(this GorgonGraphics graphics)
    {
        lock (_syncLock)
        {
            return _disposables[graphics];
        }
    }

    /// <summary>
    /// Function to register a disposable object with the graphics interface.
    /// </summary>
    /// <param name="disposable">The disposable object to register.</param>
    /// <param name="graphics">The graphics object that the IDisposable belongs with.</param>
    public static void RegisterDisposable(this IDisposable disposable, GorgonGraphics graphics)
    {
        if ((disposable is null)
            || (graphics is null))
        {
            return;
        }

        lock (_syncLock)
        {
            if (!_disposables.TryGetValue(graphics, out List<WeakReference<IDisposable>> disposables))
            {
                _disposables[graphics] = disposables = new List<WeakReference<IDisposable>>();
            }

            disposables.Add(new WeakReference<IDisposable>(disposable));
        }
    }

    /// <summary>
    /// Function to unregister a disposable object from a graphics interface.
    /// </summary>
    /// <param name="disposable">The disposable object to unregister.</param>
    /// <param name="graphics">The graphics object that the IDisposable belonged to.</param>
    public static void UnregisterDisposable(this IDisposable disposable, GorgonGraphics graphics)
    {
        if ((disposable is null)
            || (graphics is null))
        {
            return;
        }

        lock (_syncLock)
        {
            if (!_disposables.TryGetValue(graphics, out List<WeakReference<IDisposable>> disposables))
            {
                return;
            }

            if ((disposables is null)
                || (disposables.Count == 0))
            {
                _disposables.Remove(graphics);
                return;
            }

            disposables.RemoveAll(weakRef =>
                                      // Remove any dead references.
                                      !weakRef.TryGetTarget(out IDisposable disposeRef) || disposeRef == disposable);
        }
    }

    /// <summary>
    /// Function to dispose all IDisposable objects registered to the graphics interface.
    /// </summary>
    /// <param name="graphics">The graphics interface that holds all the IDisposable objects.</param>
    public static void DisposeAll(this GorgonGraphics graphics)
    {
        lock (_syncLock)
        {
            if ((!_disposables.TryGetValue(graphics, out List<WeakReference<IDisposable>> disposables))
                || (disposables is null))
            {
                return;
            }

            if (disposables.Count == 0)
            {
                _disposables.Remove(graphics);
                return;
            }

            while (disposables.Count > 0)
            {
                if (!disposables[0].TryGetTarget(out IDisposable disposeRef))
                {
                    disposables.RemoveAt(0);
                    continue;
                }

                disposeRef.Dispose();
            }

            disposables.Clear();
            _disposables.Remove(graphics);
        }
    }
    #endregion
}
