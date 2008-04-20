#region LGPL.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: Saturday, April 19, 2008 12:08:02 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace GorgonLibrary.Serialization
{
	/// <summary>
	/// Object representing a binary reader that can keep its stream open after it's closed.
	/// </summary>
	public class BinaryReaderEx
		: BinaryReader 
	{
		#region Variables.
		private bool _keepOpen = false;			// Flag to keep the underlying stream open.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return whether to keep the underlying stream open or not after the reader is closed.
		/// </summary>
		public bool KeepStreamOpen
		{
			get
			{
				return _keepOpen;
			}
			set
			{
				_keepOpen = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to close the reader and its underlying stream if KeepStreamOpen is false.
		/// </summary>
		public override void Close()
		{
			Dispose(!_keepOpen);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="input">Input stream.</param>
		/// <param name="encoder">Encoding for the binary reader.</param>
		/// <param name="keepStreamOpen">TRUE to keep the underlying stream open when the reader is closed, FALSE for normal behaviour.</param>
		public BinaryReaderEx(Stream input, Encoding encoder, bool keepStreamOpen)
			: base(input, encoder)
		{
			_keepOpen = keepStreamOpen;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="input">Input stream.</param>
		/// <param name="keepStreamOpen">TRUE to keep the underlying stream open when the reader is closed, FALSE for normal behaviour.</param>
		public BinaryReaderEx(Stream input, bool keepStreamOpen)
			: this(input, Encoding.UTF8, keepStreamOpen)
		{
		}
		#endregion
	}
}
