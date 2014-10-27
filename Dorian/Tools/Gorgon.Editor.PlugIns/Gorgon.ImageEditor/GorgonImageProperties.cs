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
// Created: Monday, October 21, 2013 8:37:09 PM
// 
#endregion

using System;
using System.Collections.Generic;
using GorgonLibrary.Configuration;
using GorgonLibrary.IO;

namespace GorgonLibrary.Editor.ImageEditorPlugIn
{
    /// <summary>
    /// Properties for the image editor plugin.
    /// </summary>
    class GorgonImageProperties
        : EditorPlugInSettings
    {
        #region Properties.
        /// <summary>
        /// Property to return the paths to assemblies that hold custom image codecs.
        /// </summary>
        [ApplicationSetting("CustomCodecs", typeof(IList<string>), "Codecs")]
        public IList<string> CustomCodecs
        {
            get;
            private set;
        }

		/// <summary>
		/// Property to set or return the last path used when importing an image file from disk.
		/// </summary>
		[ApplicationSetting("LastImportDiskDirectory", typeof(string), "Paths")]
	    public string LastImageImportDiskPath
	    {
		    get;
		    set;
	    }

		/// <summary>
		/// Property to set or return the last path used when importing an image file from the file system.
		/// </summary>
		[ApplicationSetting("LastImportFSDirectory", typeof(string), "Paths")]
		public string LastImageImportFileSystemPath
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether cropping or resizing is the default value.
		/// </summary>
		[ApplicationSetting("CropDefault", typeof(bool), "CropResize")]
	    public bool CropDefault
	    {
		    get;
		    set;
	    }

		/// <summary>
		/// Property to set or return the image filter to use when resizing an image.
		/// </summary>
		[ApplicationSetting("ResizeImageFilter", typeof(ImageFilter), "CropResize")]
	    public ImageFilter ResizeImageFilter
	    {
		    get;
		    set;
	    }

		/// <summary>
		/// Property to set or return the image filtering to use when generating mip-maps.
		/// </summary>
		[ApplicationSetting("MipFilter", typeof(ImageFilter), "MipMaps")]
	    public ImageFilter MipFilter
	    {
		    get;
		    set;
	    }
		#endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonImageProperties"/> class.
        /// </summary>
        public GorgonImageProperties()
            : base("ImageEditor.PlugIn", new Version(1, 0))
        {
            CustomCodecs = new List<string>();
	        CropDefault = true;
	        ResizeImageFilter = ImageFilter.Point;
			MipFilter = ImageFilter.Fant;
        }
        #endregion
    }
}
