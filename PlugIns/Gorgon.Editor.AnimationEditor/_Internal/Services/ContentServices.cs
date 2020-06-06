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
// Created: June 17, 2020 8:11:50 PM
// 
#endregion

using System;
using Gorgon.Editor.AnimationEditor.Services;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.Services;

namespace Gorgon.Editor.AnimationEditor
{
    /// <summary>
    /// A list of services for for the content.
    /// </summary>
    internal class ContentServices
    {
        /// <summary>
        /// Property to return the IO service for the content.
        /// </summary>
        public AnimationIOService IOService
        {
            get;
        }

        /// <summary>
        /// Property to return the factory used to create view models.
        /// </summary>
        public IViewModelFactory ViewModelFactory
        {
            get;
        }

        /// <summary>
        /// Property to return the service used for undo/redo functionality.
        /// </summary>
        public IUndoService UndoService
        {
            get;
        }

        /// <summary>
        /// Property to return the texture cache.
        /// </summary>
        public ITextureCache TextureCache
        {
            get;
        }

        /// <summary>
        /// Property to return the key frame processor service.
        /// </summary>
        public KeyProcessorService KeyProcessor
        {
            get;
        }

        /// <summary>
        /// Property to return the service used set up a new animation.
        /// </summary>
        public NewAnimationService NewAnimation
        {
            get;
        }

        /// <summary>Initializes a new instance of the <see cref="ContentServices"/> class.</summary>
        /// <param name="clipboard">The service used to access the clipboard.</param>
        /// <param name="ioService">The io service.</param>
        /// <param name="textureCache">The texture cache for the plug in.</param>
        /// <param name="undoService">The service used for undo/redo functionality.</param>
        /// <param name="keyProcessor">The key frame processor service.</param>
        /// <param name="newAnimation">The service used to set up a new animation.</param>
        /// <param name="factory">The factory.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of the parameters are <b>null</b>.</exception>
        public ContentServices(AnimationIOService ioService, ITextureCache textureCache, IUndoService undoService, KeyProcessorService keyProcessor, NewAnimationService newAnimation, IViewModelFactory factory)
        {
            IOService = ioService ?? throw new ArgumentNullException(nameof(ioService));
            ViewModelFactory = factory ?? throw new ArgumentNullException(nameof(factory));
            UndoService = undoService ?? throw new ArgumentNullException(nameof(undoService));
            TextureCache = textureCache ?? throw new ArgumentNullException(nameof(textureCache));
            KeyProcessor = keyProcessor ?? throw new ArgumentNullException(nameof(keyProcessor));
            NewAnimation = newAnimation ?? throw new ArgumentNullException(nameof(newAnimation));
        }
    }
}
