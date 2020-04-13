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
// Created: February 23, 2020 3:19:13 PM
// 
#endregion

using Gorgon.Editor.ImageEditor.Fx;
using Gorgon.Editor.UI;
using Gorgon.Graphics.Imaging;

namespace Gorgon.Editor.ImageEditor
{
    /// <summary>
    /// The context view model for the effects context options.
    /// </summary>
    internal interface IFxContext
        : IEditorContext
    {
        /// <summary>
        /// Property to return whether effects have been applied to the image.
        /// </summary>
        bool EffectsUpdated
        {
            get;
        }

        /// <summary>
        /// Property to return the service used to apply effects and generate previews for effects.
        /// </summary>
        IFxService FxService
        {
            get;
        }

        /// <summary>
        /// Property to return the view model for the blur fx settings.
        /// </summary>
        IFxBlur BlurSettings
        {
            get;
        }

        /// <summary>
        /// Property to return the view model for the sharpen effect settings.
        /// </summary>
        IFxSharpen SharpenSettings
        {
            get;
        }

        /// <summary>
        /// Property to return the view model for the emboss effect settings.
        /// </summary>
        IFxEmboss EmbossSettings
        {
            get;
        }

        /// <summary>
        /// Property to return the view model for the edge detection effect settings.
        /// </summary>
        IFxEdgeDetect EdgeDetectSettings
        {
            get;
        }

        /// <summary>
        /// Property to return the view model for the posterize effect settings.
        /// </summary>
        IFxPosterize PosterizeSettings
        {
            get;
        }

        /// <summary>
        /// Property to return the view model for the one bit effect settings.
        /// </summary>
        IFxOneBit OneBitSettings
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to assign the working image
        /// </summary>
        IEditorCommand<IGorgonImage> SetImageCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to show the blur settings.
        /// </summary>
        IEditorCommand<object> ShowBlurCommand
        {
            get;
        }

        /// <summary>
        /// Property to set or return the command to apply the effects to the final image.
        /// </summary>
        IEditorCommand<object> ApplyCommand
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the command to cancel the effects operations.
        /// </summary>
        IEditorCommand<object> CancelCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command to apply the grayscale effect.
        /// </summary>
        IEditorCommand<object> GrayScaleCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command to apply the invert effect.
        /// </summary>
        IEditorCommand<object> InvertCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command to show the sharpen effect settings.
        /// </summary>
        IEditorCommand<object> ShowSharpenCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command to show the emboss effect settings.
        /// </summary>
        IEditorCommand<object> ShowEmbossCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command to show the edge detection settings.
        /// </summary>
        IEditorCommand<object> ShowEdgeDetectCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command to show the posterize settings.
        /// </summary>
        IEditorCommand<object> ShowPosterizeCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command to show the one bit effect settings.
        /// </summary>
        IEditorCommand<object> ShowOneBitCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command to apply the burn effect.
        /// </summary>
        IEditorCommand<object> BurnCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command to apply the dodge effect.
        /// </summary>
        IEditorCommand<object> DodgeCommand
        {
            get;
        }
    }
}
