#region LGPL.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
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
