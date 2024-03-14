
// 
// Gorgon
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


namespace Gorgon.Examples;

/// <summary>
/// Updates the animation frame index for a GIF
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GifAnimator"/> class
/// </remarks>
/// <param name="syncContext">The synchronize context.</param>
/// <exception cref="ArgumentNullException">Thrown when the <paramref name="syncContext"/> parameter is <b>null</b>.</exception>
class GifAnimator(SynchronizationContext syncContext)
{

    // The task that updates the frame index.
    private Task _animationTask;
    // The current sychronization context.
    private readonly SynchronizationContext _syncContext = syncContext ?? throw new ArgumentNullException(nameof(syncContext));
    // Cancellation support.
    private CancellationTokenSource _cancel;
    private int _currentFrame;



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
        if ((FrameTimes is null) || (FrameTimes.Count == 0) || (_animationTask is not null))
        {
            return;
        }

        _cancel = new CancellationTokenSource();
        CancellationToken token = _cancel.Token;

        // We'll use a background task to update the frame index.
        // We didn't set up an Idle loop in this app, so we'll just use another task to continuously execute the frame change.
        _animationTask = Task.Run(async () =>
                                  {
                                      while (!token.IsCancellationRequested)
                                      {
                                          if (frameChangeCallback is not null)
                                          {
                                              // Always execute the callback on the main thread.
                                              // This way we won't have to worry about cross thread stuff.
                                              _syncContext.Post(_ => frameChangeCallback(), null);
                                          }

                                          // Gif frame times are in 1/100th of a second.  We need to scale to milliseconds.
                                          await Task.Delay(FrameTimes[CurrentFrame] * 10, token);

                                          ++CurrentFrame;

                                          if (CurrentFrame >= FrameTimes.Count)
                                          {
                                              CurrentFrame = 0;
                                          }
                                      }
                                  }, token);
    }

    /// <summary>
    /// Function to cancel the animation.
    /// </summary>
    /// <returns>The task that is updating the animation.</returns>
    public async Task CancelAsync()
    {
        if (_animationTask is null)
        {
            return;
        }

        _cancel.Cancel();
        // We await so the animation has time to shut down gracefully.
        await _animationTask;
        _animationTask = null;
        _cancel = null;
    }



}
