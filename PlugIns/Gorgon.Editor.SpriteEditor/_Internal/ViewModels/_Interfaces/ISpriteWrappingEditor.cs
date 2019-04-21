#region MIT
// 
// Gorgon.
// Copyright (C) 2019 Michael Winsor
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
// Created: April 17, 2019 9:07:47 AM
// 
#endregion

using Gorgon.Editor.UI;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;

namespace Gorgon.Editor.SpriteEditor
{
    /// <summary>
    /// The view model for the wrapping editor panel.
    /// </summary>
    internal interface ISpriteWrappingEditor
		: IHostedPanelViewModel
    {
        #region Properties.
		/// <summary>
        /// Property to set or return the current horizontal wrapping state.
        /// </summary>
		TextureWrap HorizontalWrapping
        {
            get;
            set;
        }

		/// <summary>
        /// Property to set or return the current vertical wrapping state.
        /// </summary>
		TextureWrap VerticalWrapping
        {
            get;
            set;
        }

		/// <summary>
        /// Property to set or return the original border color.
        /// </summary>
		GorgonColor OriginalBorderColor
        {
            get;
            set;
        }


		/// <summary>
        /// Property to set or return the current border color.
        /// </summary>
		GorgonColor BorderColor
        {
            get;
            set;
        }
        #endregion

        #region Methods.
		/// <summary>
        /// Function to retrieve the sampler state specified by the settings on this view model.
        /// </summary>
        /// <param name="filter">The current texture filter.</param>
        /// <returns>The sampler state for the sprite.</returns>
        GorgonSamplerState GetSampler(SampleFilter filter);
        #endregion
    }
}
