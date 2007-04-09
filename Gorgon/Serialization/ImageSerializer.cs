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
// Created: Thursday, October 26, 2006 12:27:02 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SharpUtilities.IO;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Serialization
{
	/// <summary>
	/// Object representing a serializer for binary image files.
	/// </summary>
	public class ImageSerializer
		: Serializer<BinaryReaderEx, BinaryWriterEx>, ISerializer 
	{
		#region Methods.
		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		/// <param name="disposing">TRUE to perform clean up of all objects.  FALSE to clean up only unmanaged objects.</param>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				if (_reader != null)
					_reader.Close();
				if (_writer != null)
					_writer.Close();
			}

			_reader = null;
			_writer = null;
		}

		/// <summary>
		/// Function to serialize an object.
		/// </summary>
		public override void Serialize()
		{
			if (_stream == null)
				throw new SerializerNotOpenException(null);

			if (_writer != null)
				throw new SerializerAlreadyOpenException(null);
			
			// Use the stream for writing binary data.
			_writer = new BinaryWriterEx(_stream, DontCloseStream);
			_serialObject.WriteData(this);
		}

		/// <summary>
		/// Function to deserialize an object.
		/// </summary>
		public override void Deserialize()
		{
			if (_stream == null)
				throw new SerializerNotOpenException(null);

			if (_reader != null)
				throw new SerializerAlreadyOpenException(null);

			// Use the stream for reading binary data.
			_reader = new BinaryReaderEx(_stream, DontCloseStream);
			_serialObject.ReadData(this);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="image">Image to (de)serialize.</param>
		/// <param name="stream">Stream to write or read data through.</param>
		internal ImageSerializer(Image image, Stream stream) 
			: base(image, stream)
		{
		}
		#endregion
	}
}
