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
// Created: April 10, 2019 11:36:49 AM
// 
#endregion

using System;
using Gorgon.Editor.UI.ViewModels;

namespace Gorgon.Editor.SpriteEditor
{
    /// <summary>
    /// Parameters for the manual input view models.
    /// </summary>
    internal class ManualInputParameters
        : ViewModelInjection
    {
        /// <summary>
        /// Property to return the settings for the sprite editor.
        /// </summary>
        public IEditorPlugInSettings Settings
        {
            get;
        }

        /// <summary>Initializes a new instance of the <see cref="ManualInputParameters"/> class.</summary>
        /// <param name="settings">The settings for the sprite editor.</param>
        /// <param name="commonServices">Common application services.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is <b>null</b>.</exception>
        public ManualInputParameters(IEditorPlugInSettings settings, IViewModelInjection commonServices)
			: base(commonServices) => Settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }
}
