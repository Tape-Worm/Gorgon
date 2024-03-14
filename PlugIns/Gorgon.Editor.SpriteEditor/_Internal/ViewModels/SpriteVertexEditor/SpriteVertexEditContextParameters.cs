#region MIT
// 
// Gorgon.
// Copyright (C) 2020 Michael Winsor
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
// Created: May 15, 2020 11:30:22 PM
// 
#endregion

using Gorgon.Editor.PlugIns;
using Gorgon.Editor.UI.ViewModels;

namespace Gorgon.Editor.SpriteEditor;

/// <summary>
/// Parameters for the <see cref="ISpriteVertexEditContext"/> view model.
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="SpriteVertexEditContextParameters"/> class.</remarks>
/// <param name="spriteContent">The image content being edited.</param>
/// <param name="hostServices">The services from the host application.</param>
/// <exception cref="ArgumentNullException">Thrown when the parameters are <b>null</b>.</exception>
internal class SpriteVertexEditContextParameters(ISpriteContent spriteContent, IHostContentServices hostServices)
        : ViewModelInjection<IHostContentServices>(hostServices)
{
    /// <summary>
    /// Property to return the sprite content for the currently being edited.
    /// </summary>
    public ISpriteContent SpriteContent
    {
        get;
    } = spriteContent ?? throw new ArgumentNullException(nameof(spriteContent));
}
