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
// Created: Sunday, December 9, 2012 4:43:49 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using D3D = SharpDX.Direct3D11;
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Used to define an object as a resource with a name.
	/// </summary>
	/// <remarks>Objects that inherit from this class will be considered a resource object that may (depending on usage) be bound to the pipeline.</remarks>
	public abstract class GorgonNamedResource
		: GorgonResource, INamedObject
	{
		#region Variables.
		private string _name = string.Empty;	// Name of the resource.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the Direct 3D resource object bound to this object.
		/// </summary>
		internal override D3D.Resource D3DResource
		{
			get
			{
				return base.D3DResource;
			}
			set
			{
#if DEBUG
				if (value != null)
				{
					value.DebugName = this.GetType().Name + " " + Name;
				}
#endif
				base.D3DResource = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to rename this object to a new name.
		/// </summary>
		/// <param name="newName">New name for the object.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="newName"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the newName parameter is an empty string.</exception>
		protected void Rename(string newName)
		{
			GorgonDebug.AssertParamString(newName, "newName");

			_name = newName;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonNamedResource" /> class.
		/// </summary>
		/// <param name="graphics">The graphics interface that owns this resource.</param>
		/// <param name="name">Name of the resource.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.</exception>
		protected GorgonNamedResource(GorgonGraphics graphics, string name)
			: base(graphics)
		{
			GorgonDebug.AssertParamString(name, "name");

			_name = name;
		}
		#endregion
	
		#region INamedObject Members
		/// <summary>
		/// Property to return the name of the resource.
		/// </summary>
		public string Name
		{
			get 
			{
				return _name;
			}
		}
		#endregion
	}
}
