#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Thursday, January 05, 2012 8:47:05 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A variable in a shader constant buffer.
	/// </summary>
	public abstract class GorgonShaderVariable
		: GorgonNamedObject, INotifier
	{
		#region Variables.		
		private bool _hasChanged = false;		// Flag to indicate that this value has changed.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the buffer that owns this variable.
		/// </summary>
		protected GorgonConstantBuffer Buffer 
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the offset, in bytes, of the variable within the buffer.
		/// </summary>
		public int Offset
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return whether this type is an array or not.
		/// </summary>
		public abstract bool IsArray
		{
			get;
		}

		/// <summary>
		/// Property to return the length of this array.
		/// </summary>
		public int ArrayLength
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		protected abstract void Write();
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonShaderVariable"/> class.
		/// </summary>
		/// <param name="buffer">Constant buffer that owns this variable.</param>
		/// <param name="name">The name of the variable.</param>
		/// <param name="offset">The offset, in bytes, for the variable.</param>
		protected GorgonShaderVariable(GorgonConstantBuffer buffer, string name, int offset)
			: base(name)
		{
			ArrayLength = 0;
			Offset = offset;
			Buffer = buffer;
		}
		#endregion

		#region INotifier Members
		/// <summary>
		/// Property to set or return whether this variable has been changed.
		/// </summary>
		public bool HasChanged
		{
			get
			{
				return _hasChanged;
			}
			set
			{
				_hasChanged = value;
				if (_hasChanged)
					Buffer.UpdateVariable(this);
			}
		}

		#endregion
	}
}
