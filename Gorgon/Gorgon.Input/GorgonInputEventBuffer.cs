// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: February 20, 2025 9:50:47 AM
//

using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Numerics;
using Gorgon.Input.Devices;
using Gorgon.Math;

namespace Gorgon.Input;

/// <summary>
/// A buffer that receives input events from the <see cref="GorgonInput.GetInput"/> method.
/// </summary>
/// <remarks>
/// <para>
/// This buffer type stores a series of input events received from the operating system. It can contain input events that happen between polling attempts, so no input data is lost if maximum accuracy is 
/// required.
/// </para>
/// <para>
/// The buffer input events are ordered by the time they arrive at, and as such, the last input event is the most recent. 
/// </para>
/// </remarks>
/// <seealso cref="GorgonInput"/>
public sealed class GorgonInputEventBuffer
    : IDisposable
{
    // Synchronization object.
    private readonly object _syncLock = new();

    // The backing stores for the buffer.
    private GorgonInputEvent[] _keyboardBuffer = [];
    private GorgonInputEvent[] _mouseBuffer = [];
    private GorgonInputEvent[] _gamingBuffer = [];

    // The maximum size of the buffers.
    private int _maxSize = 16;

    // The indices into each buffer (which will be used as the count when reading).
    private int _keyboardEventCount;
    private int _mouseEventCount;
    private int _gamingDeviceEventCount;

    // The local queue used to process a snapshot of the thread queue.
    private readonly Queue<GorgonInputEvent> _localQueue = new(16);

    /// <summary>
    /// Property to return the number of keyboard events in the buffer.
    /// </summary>
    public int KeyboardEventCount => _keyboardEventCount;

    /// <summary>
    /// Property to return the number of mouse events in the buffer.
    /// </summary>
    public int MouseEventCount => _mouseEventCount;

    /// <summary>
    /// Property to return the number of gaming device events in the buffer.
    /// </summary>
    public int GamingDeviceEventCount => _gamingDeviceEventCount;

    /// <summary>
    /// Property to return the current maximum size of the buffer.
    /// </summary>
    public int BufferLength => _maxSize;

    /// <summary>
    /// Function to resize the buffer to accomodate new input events.
    /// </summary>
    /// <param name="newCount">The new count that exceeds the current buffer count.</param>
    private void Resize(int newCount)
    {
        if (newCount <= _maxSize)
        {
            return;
        }

        int oldMax = _maxSize;
        int maxSize = (int)BitOperations.RoundUpToPowerOf2((uint)newCount) * 2;

        GorgonInputEvent[] keyboardBuffer = ArrayPool<GorgonInputEvent>.Shared.Rent(maxSize);
        GorgonInputEvent[] mouseBuffer = ArrayPool<GorgonInputEvent>.Shared.Rent(maxSize);
        GorgonInputEvent[] gamingBuffer = ArrayPool<GorgonInputEvent>.Shared.Rent(maxSize);

        // Copy the old entries.
        Array.Copy(_keyboardBuffer, keyboardBuffer, oldMax);
        Array.Copy(_mouseBuffer, mouseBuffer, oldMax);
        Array.Copy(_gamingBuffer, gamingBuffer, oldMax);

        // Initialize the remainder.
        for (int i = oldMax; i < maxSize; ++i)
        {
            keyboardBuffer[i] = new GorgonInputEvent();
            mouseBuffer[i] = new GorgonInputEvent();
            gamingBuffer[i] = new GorgonInputEvent();
        }

        ArrayPool<GorgonInputEvent?>.Shared.Return(_keyboardBuffer, true);
        ArrayPool<GorgonInputEvent?>.Shared.Return(_mouseBuffer, true);
        ArrayPool<GorgonInputEvent?>.Shared.Return(_gamingBuffer, true);

        _keyboardBuffer = keyboardBuffer;
        _mouseBuffer = mouseBuffer;
        _gamingBuffer = gamingBuffer;

        _maxSize = maxSize;

        return;
    }

    /// <summary>
    /// Function to retrieve an individual keyboard event from the buffer.
    /// </summary>
    /// <param name="index">The index of the event.</param>
    /// <returns>The <see cref="GorgonInputEvent"/> event.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="index"/> is less than 0, or greater than or equal to <see cref="KeyboardEventCount"/>.</exception>
    /// <remarks>
    /// <para>
    /// Use this to retrieve a keyboard specific input event from the buffer. This value can then be passed to the <see cref="IGorgonKeyboard"/> object.
    /// </para>
    /// </remarks>
    public GorgonInputEvent GetKeyboardEvent(int index)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(index, KeyboardEventCount, nameof(index));

        return _keyboardBuffer[index] ?? GorgonInputEvent.Empty;
    }

    /// <summary>
    /// Function to retrieve an individual mouse event from the buffer.
    /// </summary>
    /// <param name="index">The index of the event.</param>
    /// <returns>The <see cref="GorgonInputEvent"/> event.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="index"/> is less than 0, or greater than or equal to <see cref="MouseEventCount"/>.</exception>
    /// <remarks>
    /// <para>
    /// Use this to retrieve a mouse specific input event from the buffer. This value can then be passed to the <see cref="IGorgonMouse"/> object.
    /// </para>
    /// </remarks>
    public GorgonInputEvent GetMouseEvent(int index)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(index, MouseEventCount, nameof(index));

        return _mouseBuffer[index] ?? GorgonInputEvent.Empty;
    }

    /// <summary>
    /// Function to retrieve an individual gaming device event from the buffer.
    /// </summary>
    /// <param name="index">The index of the event.</param>
    /// <returns>The <see cref="GorgonInputEvent"/> event.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="index"/> is less than 0, or greater than or equal to <see cref="GamingDeviceEventCount"/>.</exception>
    /// <remarks>
    /// <para>
    /// Use this to retrieve a gaming device specific input event from the buffer. This value can then be passed to the <see cref="IGorgonGamingDevice"/> object.
    /// </para>
    /// </remarks>
    public GorgonInputEvent GetGamingDeviceEvent(int index)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(index, GamingDeviceEventCount, nameof(index));

        return _gamingBuffer[index] ?? GorgonInputEvent.Empty;
    }

    /// <summary>
    /// Function to add input events to the buffer.
    /// </summary>
    /// <param name="sourceQueue">A span containing the input events to add.</param>
    /// <param name="filter">The filter to apply to the data.</param>
    internal void AddEvents(ConcurrentQueue<GorgonInputEvent> sourceQueue, InputDeviceType filter)
    {
        lock (_syncLock)
        {
            _keyboardEventCount = _mouseEventCount = _gamingDeviceEventCount = 0;
            _localQueue.Clear();

            if (sourceQueue.IsEmpty)
            {
                return;
            }

            // We sacrifice a bit of performance here for safety by looping twice. 
            // This loop guarantees that we only capture up to a certain number of 
            // events from the main queue. This way, when we process the queue, 
            // we can be sure that we will only be working with n number of events 
            // and that nothing will sneak in while processing. The next call to 
            // capture the input events will empty the rest of the buffer.
            // 
            // The purpose is to handle a very unlikely case of the input thread
            // producing more events than can be emptied and causing a blocking
            // operation. Better to have potential stuttering than a total freeze.

            // Capture the size of the queue before it updates again.
            // If we don't do this, the count could update on the background
            // thread and our local queue will constantly play catch-up. We 
            // really don't want that.
            int maxQueueSize = sourceQueue.Count.Min(1_000_000);

            while ((!sourceQueue.IsEmpty) && (_localQueue.Count < maxQueueSize))
            {
                if (!sourceQueue.TryDequeue(out GorgonInputEvent? inputEvent))
                {
                    continue;
                }

                _localQueue.Enqueue(inputEvent);
            }

            if (_localQueue.Count == 0)
            {
                return;
            }

            // Check and resize if necessary.
            Resize(_localQueue.Count);

            while (_localQueue.Count > 0)
            {
                GorgonInputEvent source = _localQueue.Dequeue();

                GorgonInputEvent inputEvent = (filter & source.DeviceType) switch
                {
                    InputDeviceType.Mouse => _mouseBuffer[_mouseEventCount++],
                    InputDeviceType.Keyboard => _keyboardBuffer[_keyboardEventCount++],
                    InputDeviceType.GamingDevice => _gamingBuffer[_gamingDeviceEventCount++],
                    _ => GorgonInputEvent.Empty
                };

                if (inputEvent == GorgonInputEvent.Empty)
                {
                    continue;
                }

                // This really should never happen as the local queue doesn't change size during emptying, but I'm so very paranoid.
                Debug.Assert(_localQueue.Count == 0 || (_mouseEventCount < _mouseBuffer.Length && _keyboardEventCount < _keyboardBuffer.Length && _gamingDeviceEventCount < _gamingBuffer.Length), "Buffer overrun.");

                source.CopyTo(inputEvent);
            }
        }
    }

    /// <summary>
    /// Function to dispose of resources used by the buffer.
    /// </summary>
    public void Dispose()
    {
        ArrayPool<GorgonInputEvent>.Shared.Return(_keyboardBuffer, true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Initializes an instance of the <see cref="GorgonInputEventBuffer"/> class.
    /// </summary>
    public GorgonInputEventBuffer()
    {
        _keyboardBuffer = ArrayPool<GorgonInputEvent>.Shared.Rent(_maxSize);
        _mouseBuffer = ArrayPool<GorgonInputEvent>.Shared.Rent(_maxSize);
        _gamingBuffer = ArrayPool<GorgonInputEvent>.Shared.Rent(_maxSize);

        // All arrays are the same size.
        for (int i = 0; i < _keyboardBuffer.Length; ++i)
        {
            _keyboardBuffer[i] = new GorgonInputEvent();
            _mouseBuffer[i] = new GorgonInputEvent();
            _gamingBuffer[i] = new GorgonInputEvent();
        }
    }

    /// <summary>
    /// Finalizer.
    /// </summary>
    ~GorgonInputEventBuffer() => ArrayPool<GorgonInputEvent>.Shared.Return(_keyboardBuffer, true);
}
