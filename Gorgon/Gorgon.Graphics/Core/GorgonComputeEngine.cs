using System;
using System.Collections.Generic;
using System.Text;

namespace Gorgon.Graphics.Core;

/// <summary>
/// TBD
/// </summary>
public class GorgonComputeEngine
{
    /// <summary>
    /// Property to return the frequency, in ticks per second, of the timestamps recorded by queries on command lists on the compute queue.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A <see cref="QueryType.Timestamp"/> query records the GPU clock in ticks. To convert the difference between two timestamps into seconds, divide it by this value.
    /// </para>
    /// <para>
    /// The compute queue can run at a different frequency than the graphics queue, so timestamps recorded on the compute queue should only be compared with other timestamps from the compute queue. For command 
    /// lists from the graphics interface, use the <see cref="GorgonGraphics.QueryTimestampFrequency"/> property.
    /// </para>
    /// <para>
    /// If the frequency cannot be retrieved from the video adapter, this value is 0.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonQuery"/>
    public long QueryTimestampFrequency => 0;
}
