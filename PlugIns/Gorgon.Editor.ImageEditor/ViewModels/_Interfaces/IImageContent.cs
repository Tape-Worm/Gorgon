#region MIT
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: November 9, 2018 3:30:20 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Editor.UI;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;

namespace Gorgon.Editor.ImageEditor.ViewModels
{
    /// <summary>
    /// The image content view model.
    /// </summary>
    internal interface IImageContent
        : IEditorContent
    {
        #region Properties.
        /// <summary>
        /// Property to return the name of the content.
        /// </summary>
        string ContentName
        {
            get;
        }

        /// <summary>
        /// Property to return the current image codec.
        /// </summary>
        IGorgonImageCodec CurrentCodec
        {
            get;
        }

        /// <summary>
        /// Property to return the list of codecs available.
        /// </summary>
        ObservableCollection<IGorgonImageCodec> Codecs
        {
            get;
        }

        /// <summary>
        /// Property to return the list of available image pixel formats (based on codec).
        /// </summary>
        ObservableCollection<BufferFormat> PixelFormats
        {
            get;
        }

        /// <summary>
        /// Property to return the current pixel format for the image.
        /// </summary>
        BufferFormat CurrentPixelFormat
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to assign the image codec.
        /// </summary>
        IEditorCommand<IGorgonImageCodec> SetCodecCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the format support information for the current video card.
        /// </summary>
        IReadOnlyDictionary<BufferFormat, IGorgonFormatSupportInfo> FormatSupport
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve the image data for displaying on the view.
        /// </summary>
        /// <returns>The underlying image data for display.</returns>
        IGorgonImage GetImage();
        #endregion
    }
}
