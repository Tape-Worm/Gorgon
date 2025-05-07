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
// Created: April 27, 2025 8:29:44 PM
//

using System.Diagnostics;
using Gorgon.Timing;

namespace Gorgon.Examples;

/// <summary>
/// A basic timer that uses a <see cref="Stopwatch"/> to track the time.
/// </summary>
internal class BasicTimer
    : IGorgonTimer
{
    // The internal timer.
    private readonly Stopwatch _timer;

    /// <inheritdoc/>
    public double Milliseconds => _timer.Elapsed.TotalMilliseconds;

    /// <inheritdoc/>
    public double Microseconds => _timer.Elapsed.TotalMicroseconds;

    /// <inheritdoc/>
    public double Nanoseconds => _timer.Elapsed.TotalNanoseconds;

    /// <inheritdoc/>
    public double Seconds => _timer.Elapsed.TotalSeconds;

    /// <inheritdoc/>
    public TimeSpan Elapsed => _timer.Elapsed;

    /// <inheritdoc/>
    public long Ticks => _timer.ElapsedTicks;

    /// <inheritdoc/>
    public bool IsHighResolution => Stopwatch.IsHighResolution;

    /// <inheritdoc/>
    public void Reset() => _timer.Restart();

    /// <summary>
    /// Initializes a new instance of the <see cref="BasicTimer"/> class.
    /// </summary>
    public BasicTimer()
    {
        _timer = Stopwatch.StartNew();
        Reset();
    }
}
