﻿
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: January 16, 2021 1:57:57 PM
// 

using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A query for performing occlusion testing
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="GorgonTimestampDisjointQuery" /> class.</remarks>
/// <param name="graphics">The graphics interface used to build the query.</param>
/// <param name="name">[Optional] The name for the query.</param>
public class GorgonTimestampDisjointQuery(GorgonGraphics graphics, string name = null)
        : GorgonQuery<GorgonTimestampDisjointResult>(graphics, name)
{
    /// <summary>Property to return the type of query to execute.</summary>
    public override QueryType QueryType => QueryType.TimestampDisjoint;

    /// <summary>
    /// Function to retrieve the result data for the query.
    /// </summary>
    /// <param name="result">The result of the query.</param>
    /// <returns><b>true</b> if the query results are ready to be consumed, or <b>false</b> if not.</returns>
    protected override bool OnGetData(out GorgonTimestampDisjointResult result)
    {
        if (!Graphics.D3DDeviceContext.GetData(D3dQuery, out D3D11.QueryDataTimestampDisjoint data))
        {
            result = default;
            return false;
        }

        result = new GorgonTimestampDisjointResult(data);
        return true;
    }
}
