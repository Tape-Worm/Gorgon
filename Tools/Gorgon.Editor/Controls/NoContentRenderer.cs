#region MIT.
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
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
// Created: Tuesday, March 3, 2015 1:09:52 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// Renderer used to display the no content default panel.
	/// </summary>
	class NoContentRenderer
		: EditorContentRenderer2D
	{
		/// <summary>
		/// Function to clean up any resources provided to the renderer.
		/// </summary>
		protected override void OnCleanUpResources()
		{
			
		}

		/// <summary>
		/// Function to create any resources that may be required by the renderer.
		/// </summary>
		protected override void OnCreateResources()
		{
			
		}

		/// <summary>
		/// Function to perform the actual rendering of graphics to the control surface.
		/// </summary>
		public override void Draw()
		{
			
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NoContentRenderer"/> class.
		/// </summary>
		/// <param name="graphicsProxy">The graphics proxy.</param>
		/// <param name="defaultContent">The default content.</param>
		public NoContentRenderer(IProxyObject<GorgonGraphics> graphicsProxy, IContent defaultContent)
			: base(graphicsProxy, defaultContent)
		{
			ClearColor = Color.Pink;
		}
	}
}
