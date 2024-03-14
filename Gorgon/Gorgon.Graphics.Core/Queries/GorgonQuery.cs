#region MIT
// 
// Gorgon.
// Copyright (C) 2021 Michael Winsor
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
// Created: January 16, 2021 12:46:29 PM
// 
#endregion

using System;
using System.Threading;
using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Types of queries that can be performed.
/// </summary>
public enum QueryType
{
    /// <summary>
    /// Determines whether or not the GPU is finished processing commands. When the GPU is finished processing commands.
    /// </summary>
    Event = D3D11.QueryType.Event,
    /// <summary>
    /// Get the number of samples that passed the depth and stencil tests.
    /// </summary>
    Occlusion = D3D11.QueryType.Occlusion,
    /// <summary>
    /// Get a timestamp value. This kind of query is only useful if two timestamp queries are done in the middle of a <see cref="TimestampDisjoint"/> query. The difference of two timestamps can be 
    /// used to determine how many ticks have elapsed, and the <see cref="TimestampDisjoint"/> query will determine if that difference is a reliable value and also has a value that shows how to 
    /// convert the number of ticks into seconds.
    /// </summary>
    Timestamp = D3D11.QueryType.Timestamp,
    /// <summary>
    /// Determines whether or not a <see cref="Timestamp"/> is returning reliable values, and also gives the frequency of the processor enabling you to convert the number of elapsed ticks into seconds.
    /// </summary>
    TimestampDisjoint = D3D11.QueryType.TimestampDisjoint,
    /// <summary>
    /// Get pipeline statistics, such as the number of pixel shader invocations.
    /// </summary>
    PipelineStatistics = D3D11.QueryType.PipelineStatistics,
    /// <summary>
    /// Same as <see cref="Occlusion"/>, except this indicates whether any samples passed the depth test.
    /// </summary>
    OcclusionPredicate = D3D11.QueryType.OcclusionPredicate,
    /// <summary>
    /// Get streaming output statistics, such as the number of primitives streamed out.
    /// </summary>
    StreamOutStatistics = D3D11.QueryType.StreamOutputStatistics,
    /// <summary>
    /// Get streaming output statistics for stream 0, such as the number of primitives streamed out
    /// </summary>
    StreamOutStatistics0 = D3D11.QueryType.StreamOutputStatisticsStream0,
    /// <summary>
    /// Get streaming output statistics for stream 1, such as the number of primitives streamed out
    /// </summary>
    StreamOutStatistics1 = D3D11.QueryType.StreamOutputStatisticsStream1,
    /// <summary>
    /// Get streaming output statistics for stream 2, such as the number of primitives streamed out
    /// </summary>
    StreamOutStatistics2 = D3D11.QueryType.StreamOutputStatisticsStream2,
    /// <summary>
    /// Get streaming output statistics for stream 3, such as the number of primitives streamed out
    /// </summary>
    StreamOutStatistics3 = D3D11.QueryType.StreamOutputStatisticsStream3,
    /// <summary>
    /// Determines whether or not any of the streaming output buffers overflowed. If streaming output writes to multiple buffers, and one of the buffers overflows, then it will stop writing to all the output 
    /// buffers.
    /// </summary>
    StreamOutOverflowPredicate = D3D11.QueryType.StreamOutputOverflowPredicate,
    /// <summary>
    /// Determines whether or not stream 0 overflowed. If streaming output writes to multiple buffers, and one of the buffers overflows, then it will stop writing to all the output 
    /// buffers.
    /// </summary>
    StreamOutOverflowPredicate0 = D3D11.QueryType.StreamOutputOverflowPredicateStream0,
    /// <summary>
    /// Determines whether or not stream 1 overflowed. If streaming output writes to multiple buffers, and one of the buffers overflows, then it will stop writing to all the output 
    /// buffers.
    /// </summary>
    StreamOutOverflowPredicate1 = D3D11.QueryType.StreamOutputOverflowPredicateStream1,
    /// <summary>
    /// Determines whether or not stream 2 overflowed. If streaming output writes to multiple buffers, and one of the buffers overflows, then it will stop writing to all the output 
    /// buffers.
    /// </summary>
    StreamOutOverflowPredicate2 = D3D11.QueryType.StreamOutputOverflowPredicateStream2,
    /// <summary>
    /// Determines whether or not stream 3 overflowed. If streaming output writes to multiple buffers, and one of the buffers overflows, then it will stop writing to all the output 
    /// buffers.
    /// </summary>
    StreamOutOverflowPredicate3 = D3D11.QueryType.StreamOutputOverflowPredicateStream3,
}
/// <summary>
/// A query used to retrieve information about rendering.
/// </summary>
public abstract class GorgonQuery<T>
    : IGorgonGraphicsObject, IDisposable
    where T : unmanaged
{
    #region Variables.
    // Flag to indicate that the query is executing.
    private int _isRunning;
    // The D3D query.
    private D3D11.Query _d3dQuery;
    #endregion

    #region Properties.
    /// <summary>
    /// Property to return the native D3D query.
    /// </summary>
    private protected D3D11.Query D3dQuery => _d3dQuery;

    /// <summary>Property to return the graphics interface that built this object.</summary>
    public GorgonGraphics Graphics
    {
        get;
    }

    /// <summary>Property to return the name of this object.</summary>
    public string Name
    {
        get;
    }

    /// <summary>
    /// Property to return whether the data from the query is ready for consumption or not.
    /// </summary>
    public bool HasData => (_isRunning == 0) && (Graphics.D3DDeviceContext.IsDataAvailable(_d3dQuery));

    /// <summary>Property to return the type of query to execute.</summary>
    public abstract QueryType QueryType
    {
        get;
    }
    #endregion

    #region Methods.
    /// <summary>
    /// Function to initialize the query.
    /// </summary>
    private void Initialize()
    {
        var queryDesc = new D3D11.QueryDescription
        {
            Type = (D3D11.QueryType)QueryType
        };

        _d3dQuery = ResourceFactory.CreateQuery(Graphics.D3DDevice, Name, in queryDesc);

        this.RegisterDisposable(Graphics);
    }

    /// <summary>
    /// Function to retrieve the result data for the query.
    /// </summary>
    /// <param name="result">The result of the query.</param>
    /// <returns><b>true</b> if the query results are ready to be consumed, or <b>false</b> if not.</returns>
    protected abstract bool OnGetData(out T result);

    /// <summary>
    /// Function to retrieve the result data for the query.
    /// </summary>
    /// <param name="result">The result of the query.</param>
    /// <returns><b>true</b> if the query results are ready to be consumed, or <b>false</b> if not.</returns>
    public bool GetData(out T result)
    {
        if (_isRunning != 0)
        {
            result = default;
            return false;
        }

        return OnGetData(out result);
    }

    /// <summary>
    /// Function to start the query.
    /// </summary>
    public void Begin()
    {
        if (Interlocked.Exchange(ref _isRunning, 1) == 1)
        {
            return;
        }

        if (_d3dQuery is null)
        {
            throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORGFX_ERR_QUERY_NOT_INITIALIZED, Name));
        }

        // These events don't really require a Begin call.
        if (QueryType is QueryType.Event or QueryType.Timestamp)
        {
            return;
        }

        Graphics.D3DDeviceContext.Begin(_d3dQuery);
    }

    /// <summary>
    /// Function to end the query.
    /// </summary>
    public void End()
    {
        if (_isRunning != 1)
        {
            return;
        }

        Graphics.D3DDeviceContext.End(_d3dQuery);
        Interlocked.Exchange(ref _isRunning, 0);
    }        

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    public void Dispose()
    {
        D3D11.Query query = Interlocked.Exchange(ref _d3dQuery, null);
        this.UnregisterDisposable(Graphics);
        query?.Dispose();            
    }
    #endregion

    #region Constructor/Finalizer.
    /// <summary>Initializes a new instance of the <see cref="GorgonQuery{T}" /> class.</summary>
    /// <param name="graphics">The graphics interface used to build the query.</param>
    /// <param name="name">[Optional] The name for the query.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/> parameter is <b>null</b>.</exception>
    protected GorgonQuery(GorgonGraphics graphics, string name)
    {
        Graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
        Name = string.IsNullOrWhiteSpace(name) ? $"{GetType().Name}_{Guid.NewGuid():N}" : name;            
        Initialize();
    }        
    #endregion
}
