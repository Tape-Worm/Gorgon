
// 
// Gorgon
// Copyright (C) 2018 Michael Winsor
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
// Created: October 29, 2018 4:15:09 PM
// 


using Gorgon.Editor.PlugIns;

namespace Gorgon.Editor.UI;

/// <summary>
/// Common functionality for the an editor view model
/// </summary>
/// <typeparam name="T">The type of dependency injection object. Must be a class, and implement <see cref="IViewModelInjection{IHostServices}"/>.</typeparam>
/// <remarks>
/// <para>
/// Developers can use this to implement basic view models for various UI elements. This type provides basic common functionality for custom views so developers can have more freedom when implementing 
/// their own views. For more complete common content functionality, there are several base view model types that wrap up common functionality. For example, when developing a content editor plug in 
/// developers should use the <see cref="ContentEditorViewModelBase{T}"/> as a base view model type
/// </para>
/// </remarks>
/// <seealso cref="ContentEditorViewModelBase{T}"/>
public abstract class EditorViewModelBase<T>
    : ViewModelBase<T, IHostServices>
    where T : class, IViewModelInjection<IHostServices>
{
}
