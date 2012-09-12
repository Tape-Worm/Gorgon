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
// Created: Monday, September 3, 2012 7:46:31 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary.Renderers
{
	/// <summary>
	/// A callback used by the renderable object to set a property value.
	/// </summary>
	/// <typeparam name="T">Type of value to set on the property.</typeparam>
	/// <param name="propertyName">Name of the property to update.</param>
	/// <param name="value">Value to assign to the property.</param>
	public delegate void Animator<T>(string propertyName, T value);

	/// <summary>
	/// Defines an animation for a renderable object.
	/// </summary>
	public class GorgonAnimation
	{
		#region Variables.
		private IRenderable _renderable = null;			// Renderable object that is using this animation.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the list of animated properties on the renderable.
		/// </summary>
		internal IDictionary<string, Type> RenderableProperties
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the renderable object that this animation is assigned to.
		/// </summary>
		public IRenderable Renderable
		{
			get
			{
				return _renderable;
			}
			internal set
			{
				if (value == _renderable)
					return;

				_renderable = value;
				RefreshProperties();
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the animated properties for the renderable.
		/// </summary>
		private void RefreshProperties()
		{
			if (_renderable == null)
			{
				RenderableProperties = new Dictionary<string, Type>();
				return;
			}

			// Get only the properties marked with the animated attribute.
			var properties = _renderable.GetType().GetProperties();
			RenderableProperties = (from property in properties
						  let propertyAttrib = property.GetCustomAttributes(typeof(AnimatedPropertyAttribute), false) as IList<AnimatedPropertyAttribute>
						  where propertyAttrib != null && propertyAttrib.Count > 0
						  select new {Name = property.Name, PropType = property.PropertyType }).ToDictionary(key => key.Name, val => val.PropType);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonAnimation" /> class.
		/// </summary>
		public GorgonAnimation()
		{
			_renderable = null;
			RenderableProperties = new Dictionary<string, Type>();
		}
		#endregion

	}
}
