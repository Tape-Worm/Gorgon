#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Sunday, May 12, 2013 9:59:38 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Settings to apply when creating a new <see cref="GorgonLibrary.Graphics.GorgonShaderView">shader view</see>.
	/// </summary>
	public interface IViewSettings
	{
		/// <summary>
		/// Property to set or return the format of the resource.
		/// </summary>
		BufferFormat Format
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the first accessible mip level for the resource.
		/// </summary>
		int FirstMipLevel
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the number of mip levels to access in the resource.
		/// </summary>
		int MipCount
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the first accessible array index in the resource 
		/// </summary>
		int FirstArrayIndex
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the number of array levels to access in the resource.
		/// </summary>
		int ArrayCount
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the number of cubes to access in a texture cube array.
		/// </summary>
		int CubeCount
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the offset, relative to element 0, of the first element in the view to access.
		/// </summary>
		int ElementOffset
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the width of each element, in bytes.
		/// </summary>
		int ElementWidth
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the number of bytes between the beginning of a buffer 
		/// </summary>
		int FirstElement
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the number of elements in the view.
		/// </summary>
		int ElementCount
		{
			get;
			set;
		}
	}
	
	/// <summary>
	/// Settings to apply to a shader view for buffers.
	/// </summary>
	public class GorgonBufferShaderViewSettings
		: IViewSettings
	{
		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonBufferShaderViewSettings"/> class.
		/// </summary>
		public GorgonBufferShaderViewSettings()
		{
			ElementOffset = 0;
			ElementWidth = -1;
		}
		#endregion

		#region IViewSettings Members
		/// <summary>
		/// Property to set or return the format of the resource.
		/// </summary>
		public BufferFormat Format
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the first accessible mip level for the resource.
		/// </summary>
		/// <exception cref="System.NotImplementedException">The property is not valid for buffers.</exception>
		int IViewSettings.FirstMipLevel
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Property to set or return the number of mip levels to access in the resource.
		/// </summary>
		/// <exception cref="System.NotImplementedException">The property is not valid for buffers.</exception>
		int IViewSettings.MipCount
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Property to set or return the first accessible array index in the resource
		/// </summary>
		/// <exception cref="System.NotImplementedException">The property is not valid for buffers.</exception>
		int IViewSettings.FirstArrayIndex
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Property to set or return the number of array levels to access in the resource.
		/// </summary>
		/// <exception cref="System.NotImplementedException">The property is not valid for buffers.</exception>
		int IViewSettings.ArrayCount
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}
		
		/// <summary>
		/// Property to set or return the number of cubes to access in a texture cube array.
		/// </summary>
		/// <exception cref="System.NotImplementedException">The property is not valid for buffers.</exception>
		int IViewSettings.CubeCount
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Property to set or return the offset, relative to element 0, of the first element in the view to access.
		/// </summary>
		public int ElementOffset
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the width of each element, in bytes.
		/// </summary>
		/// <remarks>Set this value to -1 to derive the size of the element from the format passed to the view.
		/// <para>The default value for this property is -1.</para>
		/// </remarks>
		public int ElementWidth
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the number of bytes between the beginning of a buffer
		/// </summary>
		int IViewSettings.FirstElement
		{
			get
			{
				return ElementOffset;
			}
			set
			{
				ElementOffset = value;
			}
		}

		/// <summary>
		/// Property to set or return the number of elements in the view.
		/// </summary>
		int IViewSettings.ElementCount
		{
			get
			{
				return ElementWidth;
			}
			set
			{
				ElementWidth = value;
			}
		}
		#endregion
	}
}
