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
// Created: Thursday, September 26, 2013 9:22:57 PM
// 
#endregion

using System.IO;
using GorgonLibrary.IO;

namespace GorgonLibrary.Editor.ImageEditorPlugIn
{
    /// <summary>
    /// Settings for image content.
    /// </summary>
    class GorgonImageContentSettings
        : ContentSettings
    {
        #region Methods.
        /// <summary>
        /// Function used to validate the name of the content object.
        /// </summary>
        /// <param name="name">Current name of the object.</param>
        /// <returns>
        /// The updated and validated name of the content object.
        /// </returns>
        /// <remarks>
        /// This method should be used to determine if a name for a content object is valid or not.  Implementors should return NULL (Nothing in VB.Net) to indicate that the
        /// name was not valid.
        /// </remarks>
        protected override string ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

			var extension = new GorgonFileExtension(Path.GetExtension(name));

	        if (string.IsNullOrWhiteSpace(extension.Extension))
	        {
		        return null;
	        }

	        return !GorgonImageEditorPlugIn.Codecs.ContainsKey(extension) ? null : name;
        }

        /// <summary>
        /// Function to initialize the settings for the content.
        /// </summary>
        /// <returns>
        /// TRUE if the object was set up, FALSE if not.
        /// </returns>
        public override bool PerformSetup()
        {
            return true;
        }
        #endregion
    }
}
