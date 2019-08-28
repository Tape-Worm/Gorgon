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
// Created: Tuesday, January 1, 2013 7:38:21 PM
// 
#endregion

using Gorgon.PlugIns;

namespace Gorgon.Examples
{
    /// <summary>
    /// Here's where we define the entry point interface for the plug in.
    /// 
    /// This object houses the actual plug in information and is what Gorgon will look for when enumerating
    /// plug in objects from an assembly.  The object inherits from GorgonPlugIn, and this is how Gorgon will
    /// tell if it's a plug in object or not.  This object must be implemented by each plug in assembly so that
    /// it can return the proper objects/information back to the plug in host.
    /// 
    /// In this example, we use the TextColorPlugIn to create our plug in interfaces which are implemented in our 
    /// plug in assemblies.
    /// </summary>	
    public abstract class TextColorPlugIn
        : GorgonPlugIn
    {
        #region Methods.
        /// <summary>
        /// Define our abstract method to create our specified text writers.
        /// </summary>
        /// <returns>The text color writer specific to the plug in.</returns>
        public abstract TextColorWriter CreateWriter();
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="TextColorPlugIn" /> class.
        /// </summary>
        /// <param name="description">This is a friendly description for the plug in.</param>
        protected TextColorPlugIn(string description)
            : base(description)
        {
        }
        #endregion
    }
}
