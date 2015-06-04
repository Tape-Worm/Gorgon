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
// Created: Tuesday, March 3, 2015 12:34:54 AM
// 
#endregion

using System;
using System.Windows.Forms;
using Gorgon.Graphics;

namespace Gorgon.Editor
{
	/// <summary>
	/// A renderer for the editor content.
	/// </summary>
	/// <remarks>
	/// For content that requires the use of a renderer interface, this interface will provide the ability to 
	/// perform per-content custom rendering.
	/// <para>
	/// This interface is designed to allow any type of renderer to be used with the content. This will allow for multiple renderers to exist for the editor 
	/// should that be necessary.
	/// </para>
	/// </remarks>
	public interface IEditorContentRenderer
		: IDisposable
	{
		#region Properties.
		/// <summary>
		/// Property to return the content to be rendered.
		/// </summary>
		IContentData Content
		{
			get;
		}

		/// <summary>
		/// Property to set or return the presentation interval for the renderer.
		/// </summary>
		/// <remarks>Use this to control the vsync for the rendering.</remarks>
		int RenderInterval
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the color to use when clearing the render surface.
		/// </summary>
		/// <remarks>Set this value to NULL (<i>Nothing</i> in VB.Net) to use the control background color.</remarks>
		GorgonColor? ClearColor
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to perform the actual rendering of graphics to the control surface.
		/// </summary>
		void Draw();

		/// <summary>
		/// Function to stop any rendering.
		/// </summary>
		void StopRendering();

		/// <summary>
		/// Function to start rendering.
		/// </summary>
		void StartRendering();

		/// <summary>
		/// Function to reset the resources used by the renderer.
		/// </summary>
		/// <remarks>
		/// This method can be used to reset the state to reflect a potential content change, or just to restart the renderer. If 
		/// resetting due to a content change, be aware that this method carries a lot of overhead and will impair performance.
		/// <para>
		/// This will only reset custom resources defined by the content renderer, the global objects such as the renderer itself and 
		/// any objects created by the base renderer class will not be affected.
		/// </para>
		/// </remarks>
		void ResetResources();

		/// <summary>
		/// Function to clear all resources for this renderer.
		/// </summary>
		void ClearResources();

		/// <summary>
		/// Function to create the necessary resources for the renderer.
		/// </summary>
		/// <param name="renderControl">Control to render into.</param>
		void CreateResources(Control renderControl);
		#endregion
	}
}