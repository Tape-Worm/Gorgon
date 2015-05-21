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
// Created: Saturday, March 2, 2013 12:47:25 PM
// 
#endregion

using Gorgon.PlugIns;

namespace Gorgon.Editor
{
	/// <summary>
	/// Type of plug-in.
	/// </summary>
	public enum PlugInType
	{
		/// <summary>
		/// Plug-in writes out content packed files in a specific format.
		/// </summary>
		/// <remarks>File readers are implemented using the GorgonFileSystem objects, and are not specific to the editor.</remarks>
		FileWriter = 0,
		/// <summary>
		/// Plug-in is used to create content.
		/// </summary>
		Content = 1
	}

	/// <summary>
	/// An interface for editor plug-ins.
	/// </summary>
	public abstract class EditorPlugIn
		: GorgonPlugIn
    {
        #region Properties.
		/// <summary>
		/// Property to return the type of plug-in.
		/// </summary>
		/// <remarks>Implementors must provide one of the PlugInType enumeration values.</remarks>
		public abstract PlugInType PlugInType
		{
			get;
		}
		#endregion

		#region Methods.
        /// <summary>
        /// Function to determine if a plug-in can be used.
        /// </summary>
        /// <returns>A string containing a list of reasons why the plug-in is not valid for use, or an empty string if the control is not valid for use.</returns>
        protected internal abstract string ValidatePlugIn();
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="EditorPlugIn"/> class.
		/// </summary>
		/// <param name="description">Optional description of the plug-in.</param>
		/// <remarks>
		/// Objects that implement this base class should pass in a hard coded description on the base constructor.
		/// </remarks>
		protected EditorPlugIn(string description)
			: base(description)
		{
		}
		#endregion
	}
}
