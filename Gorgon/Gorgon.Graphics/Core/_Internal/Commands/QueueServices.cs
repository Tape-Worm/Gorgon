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
// Created: July 6, 2025 3:49:07 PM
//

namespace Gorgon.Graphics.Core;

/// <summary>
/// Services for queues.
/// </summary>
internal sealed class QueueServices
    : IDisposable
{
    /// <summary>
    /// Property to return the internal graphics command queue.
    /// </summary>
    public CommandQueue GraphicsQueue
    {
        get;
        init;
    }

    /// <summary>
    /// Property to return the internal compute command queue.
    /// </summary>
    public CommandQueue ComputeQueue
    {
        get;
        init;
    }

    /// <summary>
    /// Property to return the internal copy command queue.
    /// </summary>
    public CommandQueue CopyQueue
    {
        get;
        init;
    }

    /// <summary>
    /// Property to return the global resource barrier state.
    /// </summary>
    internal GlobalBarrierState GlobalBarriers
    {
        get;
    } = new();

    /// <summary>
    /// Property to return the internal copier for setting up resource state.
    /// </summary>
    internal GorgonResourceCopier GlobalCopier
    {
        get;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        GlobalCopier.Dispose();
        ComputeQueue.Dispose();
        CopyQueue.Dispose();
        GraphicsQueue.Dispose();

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Function to collect the garbage on the queues.
    /// </summary>
    public void GarbageCollect()
    {        
        GraphicsQueue.GarbageCollect();
        CopyQueue.GarbageCollect();
        ComputeQueue.GarbageCollect();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="QueueServices"/> class.
    /// </summary>
    /// <param name="graphicsQueue">The graphics queue.</param>
    /// <param name="copyQueue">The copy qeueue.</param>
    /// <param name="computeQueue">The compute queue.</param>
    public QueueServices(CommandQueue graphicsQueue, CommandQueue copyQueue, CommandQueue computeQueue)
    {
        GraphicsQueue = graphicsQueue;
        CopyQueue = copyQueue;
        ComputeQueue = computeQueue;

        GlobalCopier = new GorgonResourceCopier(GraphicsQueue);
    }
}
