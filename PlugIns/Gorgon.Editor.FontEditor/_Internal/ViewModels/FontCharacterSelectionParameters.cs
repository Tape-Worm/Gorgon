
// 
// Gorgon
// Copyright (C) 2021 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: September 4, 2021 10:07:26 AM
// 


using Gorgon.Editor.PlugIns;
using Gorgon.Editor.UI;

namespace Gorgon.Editor.FontEditor;

/// <summary>
/// The parameters for the font character selection viewmodel
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="FontCharacterSelectionParameters" /> class.</remarks>
/// <param name="fontService">The service used to generate fonts.</param>
/// <param name="hostServices">The services from the host application.</param>
internal class FontCharacterSelectionParameters(FontService fontService, IHostContentServices hostServices)
        : HostedPanelViewModelParameters(hostServices)
{

    /// <summary>
    /// Property to return the service used to generate fonts.
    /// </summary>
    public FontService FontService
    {
        get;
    } = fontService ?? throw new ArgumentNullException(nameof(fontService));


}
