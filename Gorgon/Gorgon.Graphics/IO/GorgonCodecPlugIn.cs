#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Monday, October 21, 2013 8:48:58 PM
// 
#endregion

using Gorgon.Plugins;

namespace Gorgon.IO
{
    /// <summary>
    /// A plug-in to allow for loading of custom image codecs.
    /// </summary>
    public abstract class GorgonCodecPlugIn
        : GorgonPlugin
    {
        #region Methods.
        /// <summary>
        /// Function to create an image codec object.
        /// </summary>
        /// <returns>The custom image codec.</returns>
        public abstract GorgonImageCodec CreateCodec();
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonCodecPlugIn"/> class.
        /// </summary>
        /// <param name="description">Optional description of the plug-in.</param>
        /// <remarks>
        /// Objects that implement this base class should pass in a hard coded description on the base constructor.
        /// </remarks>
        protected GorgonCodecPlugIn(string description)
            : base(description)
        {
        }
        #endregion
    }
}
