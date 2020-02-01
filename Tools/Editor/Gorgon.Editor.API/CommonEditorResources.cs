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
// Created: November 8, 2018 4:36:54 PM
// 
#endregion

using System.IO;
using Gorgon.Editor.Properties;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;

namespace Gorgon.Editor
{
    /// <summary>
    /// Provides access to common resources used in the editor.
    /// </summary>
    public static class CommonEditorResources
    {
        /// <summary>
        /// Property to return a checkerboard pattern image (encoded as DDS/DXT1 data) for background images
        /// </summary>
        /// <remarks>
        /// The image size is 256x256.
        /// </remarks>
        public static IGorgonImage CheckerBoardPatternImage
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return a keyboard icon (encoded as DDS/DXT5 data).
        /// </summary>
        public static IGorgonImage KeyboardIcon
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return a large (64x64) version of the keybaord icon (encoded as DDS/DXT5 data).
        /// </summary>
        public static IGorgonImage KeyboardIconLarge
        {
            get;
            private set;
        }

        /// <summary>
        /// Function to load the common resources at application start up.
        /// </summary>
        public static void LoadResources()
        {
            IGorgonImageCodec dds = new GorgonCodecDds();

            using (var stream = new MemoryStream(Resources.Bg_Pattern_256x256, false))
            {
                CheckerBoardPatternImage = dds.LoadFromStream(stream);
            }

            using (var stream = new MemoryStream(Resources.manual_input_24x24, false))
            {
                KeyboardIcon = dds.LoadFromStream(stream);
            }

            using (var stream = new MemoryStream(Resources.manual_vertex_edit_64x64, false))
            {
                KeyboardIconLarge = dds.LoadFromStream(stream);
            }
        }

        /// <summary>
        /// Function to unload all resources at the end of the application life cycle.
        /// </summary>
        public static void UnloadResources()
        {
            try
            {
                CheckerBoardPatternImage?.Dispose();
                KeyboardIcon?.Dispose();
                KeyboardIconLarge?.Dispose();
            }
            catch
            {
                // Do nothing here, application should be closing anyway.
            }
        }
    }
}
