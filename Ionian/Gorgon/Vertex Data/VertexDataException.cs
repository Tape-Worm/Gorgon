#region MIT.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
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
// Created: Saturday, March 25, 2006 10:26:34 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// No indices to write.
	/// </summary>
	public class IndexCountException 
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ex">Source exception.</param>
		public IndexCountException(Exception ex)
			: base("There are no indices that can be written to the buffer.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public IndexCountException()
			: this(null)
		{
		}
		#endregion
	}

	/// <summary>
	/// No vertices to write.
	/// </summary>
	public class VertexCountException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ex">Source exception.</param>
		public VertexCountException(Exception ex)
			: base("There are no vertices that can be written to the buffer.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>		
		public VertexCountException()
			: this(null)
		{
		}
		#endregion
	}

	/// <summary>
	/// Invalid vertex type.
	/// </summary>
	public class VertexTypeInvalidException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ex">Source exception.</param>
		public VertexTypeInvalidException(Exception ex)
			: base("Invalid vertex type.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public VertexTypeInvalidException()
			: this(null)
		{
		}
		#endregion
	}

	/// <summary>
	/// Index size is invalid.
	/// </summary>
	public class IndexSizeInvalid
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ex">Source exception.</param>
		public IndexSizeInvalid(Exception ex)
			: base("Index size is invalid for this video card.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public IndexSizeInvalid()
			: this(null)
		{
		}
		#endregion
	}

	/// <summary>
	/// Invalid vertex stream.
	/// </summary>
	public class VertexStreamInvalidException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="stream">Requested stream.</param>
		/// <param name="ex">Source exception.</param>
		public VertexStreamInvalidException(short stream, Exception ex)
			: base("Invalid vertex stream (" + stream.ToString() + ").", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="stream">Requested stream.</param>
		public VertexStreamInvalidException(short stream)
			: this(stream, null)
		{
		}
		#endregion
	}

	/// <summary>
	/// Invalid vertex field context.
	/// </summary>
	public class VertexFieldContextInvalidException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ex">Source exception.</param>
		public VertexFieldContextInvalidException(Exception ex)
			: base("Invalid vertex field context.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public VertexFieldContextInvalidException()
			: this(null)
		{
		}
		#endregion
	}

	/// <summary>
	/// Invalid vertex field type.
	/// </summary>
	public class VertexFieldInvalidException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ex">Source exception.</param>
		public VertexFieldInvalidException(Exception ex)
			: base("Invalid vertex field type.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public VertexFieldInvalidException()
			: this(null)
		{
		}
		#endregion
	}
}
