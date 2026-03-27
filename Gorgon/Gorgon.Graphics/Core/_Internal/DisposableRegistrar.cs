
// 
// Gorgon
// Copyright (C) 2025 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: April 7, 2018 11:01:10 PM
// 


using Gorgon.Core;
using Gorgon.Diagnostics;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Allows tracking of objects that implement IDisposable
/// </summary>
internal static class DisposableRegistrar
{
    // The list of disposable objects.
    private static readonly Dictionary<GorgonGraphics, List<WeakReference<IDisposable>>> _disposables = [];
    // Synchronization object for multiple threads.
    private static readonly Lock _syncLock = new();

    extension(GorgonGraphics graphics)
    {
        /// <summary>
        /// Function to retrieve the list of disposables for a given graphics instance.
        /// </summary>
        /// <returns>An enumerable containing the list of resources for the graphics instance.</returns>
        public IEnumerable<WeakReference<IDisposable>> GetDisposables()
        {
            using (_syncLock.EnterScope())
            {
                return _disposables[graphics];
            }
        }

        /// <summary>
        /// Function to dispose all IDisposable objects registered to the graphics interface.
        /// </summary>
        public void DisposeAll()
        {
            using (_syncLock.EnterScope())
            {
                if ((!_disposables.TryGetValue(graphics, out List<WeakReference<IDisposable>>? disposables))
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
                    if (!disposables[0].TryGetTarget(out IDisposable? disposeRef))
                    {
                        disposables.RemoveAt(0);
                        continue;
                    }

                    if (disposeRef is not IGorgonNamedObject named)
                    {
                        graphics.Log.PrintWarning($"Object type {disposeRef.GetType().FullName} is still alive. Applications should dispose of this object type when done with it.", LoggingLevel.Verbose);
                    }
                    else
                    {
                        graphics.Log.PrintWarning($"Object type {disposeRef.GetType().FullName} (Name: '{named.Name}') is still alive. Applications should dispose of this object type when done with it.", LoggingLevel.Verbose);
                    }

                    disposeRef.Dispose();
                }

                disposables.Clear();
                _disposables.Remove(graphics);
            }
        }
    }

    extension(IDisposable disposable)
    {
        /// <summary>
        /// Function to register a disposable object with the graphics interface.
        /// </summary>
        /// <param name="graphics">The graphics object that the IDisposable belongs with.</param>
        public void RegisterDisposable(GorgonGraphics graphics)
        {
            using (_syncLock.EnterScope())
            {
                if (!_disposables.TryGetValue(graphics, out List<WeakReference<IDisposable>>? disposables))
                {
                    _disposables[graphics] = disposables = [];
                }

                disposables.Add(new WeakReference<IDisposable>(disposable));
            }
        }

        /// <summary>
        /// Function to unregister a disposable object from a graphics interface.
        /// </summary>
        /// <param name="graphics">The graphics object that the IDisposable belonged to.</param>
        public void UnregisterDisposable(GorgonGraphics? graphics)
        {
            if (graphics is null)
            {
                return;
            }

            using (_syncLock.EnterScope())
            {
                if (!_disposables.TryGetValue(graphics, out List<WeakReference<IDisposable>>? disposables))
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
                                          !weakRef.TryGetTarget(out IDisposable? disposeRef) || disposeRef == disposable);
            }
        }
    }
}
