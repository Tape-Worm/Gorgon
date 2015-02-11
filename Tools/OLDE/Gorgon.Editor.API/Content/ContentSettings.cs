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
// Created: Thursday, September 26, 2013 9:20:29 PM
// 
#endregion

using GorgonLibrary.IO;

namespace GorgonLibrary.Editor
{
    /// <summary>
    /// Content settings.
    /// </summary>
    /// <remarks>
    /// This interface is used to provide initial settings to the content.  It can provide a user interface via the <see cref="PerformSetup"/> method to accomplish this.
    /// <para>Implementors of this interface should provide default settings upon object creation.</para>
    /// </remarks>
    public abstract class ContentSettings
    {
        #region Variables.
        private string _name = string.Empty;                // Name for the content object.
        #endregion

        #region Properties.
		/// <summary>
		/// Property to return the plug-in that created the settings object.
		/// </summary>
	    public ContentPlugIn PlugIn
	    {
		    get;
		    internal set;
	    }

        /// <summary>
        /// Property to return whether the content should be created.
        /// </summary>
        /// <remarks>
        /// When set to TRUE this property will determine if the content object creates content after the content object has been initialized.
        /// </remarks>
        public bool CreateContent
        {
            get;
            internal set;
        }

		/// <summary>
        /// Property to set or return the name of the content.
        /// </summary>
        /// <remarks>This property will return NULL (Nothing in VB.Net) if the name is not valid.</remarks>
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (value == null)
                {
                    value = string.Empty;
                }

	            value = string.IsNullOrWhiteSpace(value) ? null : value.FormatFileName();

                if (value == null)
                {
                    _name = null;
                    return;
                }

                _name = value;
            }
        }
        #endregion

        #region Methods.
	    /// <summary>
        /// Function to initialize the settings for the content.
        /// </summary>
        /// <returns>TRUE if the object was set up, FALSE if not.</returns>
        public abstract bool PerformSetup();
        #endregion
    }
}
