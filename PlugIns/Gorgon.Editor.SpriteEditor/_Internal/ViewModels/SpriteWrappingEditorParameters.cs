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
// Created: April 17, 2019 9:51:33 AM
// 
#endregion

using System;
using Gorgon.Editor.UI.ViewModels;

namespace Gorgon.Editor.SpriteEditor
{
    /// <summary>
    /// Parameters for the <see cref="ISpriteWrappingEditor"/> view model.
    /// </summary>
    internal class SpriteWrappingEditorParameters
        : ViewModelInjection
    {
        /// <summary>
        /// Property to return the builder used to create samplers.
        /// </summary>
        public ISamplerBuildService SamplerStateBuilder
        {
            get;
        }

        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.SpriteEditor.SpriteWrappingEditorParameters"/> class.</summary>
        /// <param name="builder">The builder used to create samplers.</param>
        /// <param name="commonServices">Common application services.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is <b>null</b>.</exception>
        public SpriteWrappingEditorParameters(ISamplerBuildService builder, IViewModelInjection commonServices)
            : base(commonServices) => SamplerStateBuilder = builder ?? throw new ArgumentNullException(nameof(builder));
    }
}
