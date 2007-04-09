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
using SharpUtilities;

namespace GorgonLibrary.Internal
{
	/// <summary>
	/// Enumeration containing vertex error codes.
	/// </summary>
	public enum VertexErrors
	{
		/// <summary>Field type is invalid.</summary>
		InvalidVertexFieldType = 0x7FFF0001,
		/// <summary>Context of vertex element is invalid.</summary>
		InvalidVertexFieldContext = 0x7FFF0002,
		/// <summary>Stream requested falls outside of the range provided by the hardware.</summary>
		InvalidStream = 0x7FFF0003,
		/// <summary>32 bit indices not supported on this driver.</summary>
		InvalidIndexSize = 0x7FFF0004,
		/// <summary>Vertex type specified was invalid.</summary>
		InvalidVertexType = 0x7FFF0005,
		/// <summary>No vertices to write.</summary>
		NoVertices = 0x7FFF0006,
		/// <summary>No indices to write.</summary>
		NoIndices = 0x7FFF0007,
		/// <summary>No face found matching request.</summary>
		InvalidFace = 0x7FFF0008
	}

	/// <summary>
	/// Base class for vertex data exceptions.
	/// </summary>
	public abstract class VertexDataException
		: ResourceException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Error message.</param>
		/// <param name="code">Error code.</param>
		/// <param name="ex">Source exception.</param>
		public VertexDataException(string message, VertexErrors code, Exception ex)
			: base(message, (int)code, ex)
		{
			_errorCodeType = code.GetType();
		}
		#endregion
	}

	/// <summary>
	/// No indices to write exception.
	/// </summary>
	public class NoIndicesToWriteException 
		: VertexDataException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Description of the error.</param>
		/// <param name="ex">Source exception.</param>
		public NoIndicesToWriteException(string message, Exception ex)
			: base(message, VertexErrors.NoIndices, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ex">Source exception.</param>
		public NoIndicesToWriteException(Exception ex)
			: this("There are no indices that can be written to the buffer.", ex)
		{
		}
		#endregion
	}

	/// <summary>
	/// No vertices to write exception.
	/// </summary>
	public class NoVerticesToWriteException
		: VertexDataException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Description of the error.</param>
		/// <param name="ex">Source exception.</param>
		public NoVerticesToWriteException(string message, Exception ex)
			: base(message, VertexErrors.NoVertices, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ex">Source exception.</param>
		public NoVerticesToWriteException(Exception ex)
			: this("There are no vertices that can be written to the buffer.", ex)
		{
		}
		#endregion
	}

	/// <summary>
	/// Invalid vertex type exception.
	/// </summary>
	public class InvalidVertexTypeException
		: VertexDataException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Description of the error.</param>
		/// <param name="ex">Source exception.</param>
		public InvalidVertexTypeException(string message, Exception ex)
			: base(message, VertexErrors.InvalidVertexType, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ex">Source exception.</param>
		public InvalidVertexTypeException(Exception ex)
			: this("Invalid vertex type.", ex)
		{
		}
		#endregion
	}

	/// <summary>
	/// Invalid index size exception.
	/// </summary>
	public class InvalidIndexSizeException
		: VertexDataException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Description of the error.</param>
		/// <param name="ex">Source exception.</param>
		public InvalidIndexSizeException(string message, Exception ex)
			: base(message, VertexErrors.InvalidIndexSize, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ex">Source exception.</param>
		public InvalidIndexSizeException(Exception ex)
			: this("Index size is invalid for this video card.", ex)
		{
		}
		#endregion
	}

	/// <summary>
	/// Invalid stream exception.
	/// </summary>
	public class InvalidStreamException
		: VertexDataException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Description of the error.</param>
		/// <param name="ex">Source exception.</param>
		public InvalidStreamException(string message, Exception ex)
			: base(message, VertexErrors.InvalidStream, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="stream">Requested stream.</param>
		/// <param name="ex">Source exception.</param>
		public InvalidStreamException(short stream, Exception ex)
			: this("Invalid vertex stream (" + stream.ToString() + ").", ex)
		{
		}
		#endregion
	}

	/// <summary>
	/// Invalid vertex field context exception.
	/// </summary>
	public class InvalidVertexFieldContextException
		: VertexDataException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Description of the error.</param>
		/// <param name="ex">Source exception.</param>
		public InvalidVertexFieldContextException(string message, Exception ex)
			: base(message, VertexErrors.InvalidVertexFieldContext, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ex">Source exception.</param>
		public InvalidVertexFieldContextException(Exception ex)
			: this("Invalid vertex field context.", ex)
		{
		}
		#endregion
	}

	/// <summary>
	/// Invalid vertex field type exception.
	/// </summary>
	public class InvalidVertexFieldTypeException
		: VertexDataException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Description of the error.</param>
		/// <param name="ex">Source exception.</param>
		public InvalidVertexFieldTypeException(string message, Exception ex)
			: base(message, VertexErrors.InvalidVertexFieldType, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ex">Source exception.</param>
		public InvalidVertexFieldTypeException(Exception ex)
			: this("Invalid vertex field type.", ex)
		{
		}
		#endregion
	}
}
