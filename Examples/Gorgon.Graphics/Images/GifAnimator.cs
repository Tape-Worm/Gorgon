
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
// Created: April 5, 2018 12:49:36 PM
// 

using System.Diagnostics;
using Gorgon.Timing;

namespace Gorgon.Examples;

/// <summary>
/// Updates the animation frame index for a GIF
/// </summary>
internal class GifAnimator
{
    // The thread.
    private readonly Thread _animThread;
    // Event triggered when the thread has exited.
    private readonly ManualResetEventSlim _closeEvent = new(false);
    // The current sychronization context.
    private readonly SynchronizationContext _syncContext;
    // Cancellation support.
    private readonly CancellationTokenSource _cancel = new();
    // The current frame of animation.
    private int _currentFrame;
    // Timer for frame advancement.
    private readonly GorgonTimer _timer = new();

    /// <summary>
    /// Property to return whether the animation is currently executing.
    /// </summary>
    public bool IsRunning => !_cancel.IsCancellationRequested;

    /// <summary>
    /// Property to set or return the position for the GIF image.
    /// </summary>
    public Point GifPosition
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the current frame of animation.
    /// </summary>
    public int CurrentFrame
    {
        get => _currentFrame;
        private set => _currentFrame = value;
    }

    /// <summary>
    /// Property to set or return the time between each frame.
    /// </summary>
    public IReadOnlyList<int> FrameTimes
    {
        get;
        set;
    } = [];

    /// <summary>
    /// Function to perform the animation.
    /// </summary>
    /// <param name="callback">The callback to execute on each frame.</param>
    private void Animation(object? callback)
    {
        Debug.Assert(callback is not null, "No callback registered.");

        Action frameChangeCallback = (Action)callback;
        int currentFrameTime = FrameTimes[0];

        bool IsFinishedDelay() => (_cancel.Token.IsCancellationRequested) || (_timer.Milliseconds >= currentFrameTime);

        while (!_cancel.Token.IsCancellationRequested)
        {
            // Always execute the callback on the main thread.
            // This way we won't have to worry about cross thread problems.
            _syncContext.Send(_ => frameChangeCallback(), null);

            // Gif frame times are in 1/100th of a second.  We need to scale to milliseconds.
            currentFrameTime = FrameTimes[_currentFrame] * 10;

            // Reset before we enter our spin so we're waiting for the frame time, and not the frametime plus 
            // call overhead.
            _timer.Reset();
            SpinWait.SpinUntil(IsFinishedDelay);

            if (Interlocked.Increment(ref _currentFrame) >= FrameTimes.Count)
            {
                _currentFrame = 0;
            }
        }

        _closeEvent.Set();
    }

    /// <summary>
    /// Function to reset the frame index.
    /// </summary>
    public void Reset() => Interlocked.Exchange(ref _currentFrame, 0);

    /// <summary>
    /// Function to start the animation.
    /// </summary>
    /// <param name="frameChangeCallback">The callback to execute when a frame is updated.</param>
    public void Animate(Action frameChangeCallback)
    {
        if ((FrameTimes is null) || (FrameTimes.Count == 0))
        {
            return;
        }

        _animThread.Start(frameChangeCallback);
    }

    /// <summary>
    /// Function to cancel the animation.
    /// </summary>
    /// <returns>A task for asynchronous operation.</returns>
    public async Task CancelAsync()
    {
        if (_cancel.IsCancellationRequested)
        {
            return;
        }

        _cancel.Cancel();

        // We await so the animation has time to shut down gracefully.
        await Task.Run(() => _closeEvent.Wait(3000)).ConfigureAwait(false);

        _closeEvent.Dispose();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GifAnimator"/> class.
    /// </summary>
    /// <param name="syncContext">The synchronization context.</param>
    public GifAnimator(SynchronizationContext syncContext)
    {
        _syncContext = syncContext;
        _animThread = new Thread(Animation);
    }
}
