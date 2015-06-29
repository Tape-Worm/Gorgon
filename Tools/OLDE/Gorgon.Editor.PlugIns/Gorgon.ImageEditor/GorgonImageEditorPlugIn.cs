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
// Created: Monday, October 21, 2013 8:00:40 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Gorgon.Core;
using Gorgon.Editor.ImageEditorPlugIn.Controls;
using Gorgon.Editor.ImageEditorPlugIn.Properties;
using Gorgon.IO;
using Gorgon.UI;

namespace Gorgon.Editor.ImageEditorPlugIn
{
    /// <summary>
    /// Image editor plug-in interface.
    /// </summary>
    public class GorgonImageEditorPlugIn
		: ContentPlugIn, IImageEditorPlugIn, IPlugInSettingsUI
	{
		#region Variables.
        private static Dictionary<GorgonFileExtension, GorgonImageCodec> _codecs;           // Codecs for the image editor.
	    private static GorgonImageCodec[] _codecDropDown;									// List of codecs for the drop down list.
		#endregion

		#region Properties.
        /// <summary>
        /// Property to return the settings for the plug-in.
        /// </summary>
        internal static GorgonImageProperties Settings
        {
            get;
            private set;
        }

		/// <summary>
		/// Property to return the list of codecs to show in the codec drop down.
		/// </summary>
	    internal GorgonImageCodec[] CodecDropDownList
	    {
		    get
		    {
			    if (_codecs == null)
			    {
				    GetCodecs();
			    }

			    return _codecDropDown;
		    }
	    }

        /// <summary>
        /// Property to return a list of codecs that can be used to read image file formats.
        /// </summary>
        internal Dictionary<GorgonFileExtension, GorgonImageCodec> Codecs
        {
            get
            {
                if (_codecs == null)
                {
                    GetCodecs();
                }

                return _codecs;
            }
        }
        #endregion

		#region Methods.
        /// <summary>
        /// Function to load an external codec assembly.
        /// </summary>
        /// <param name="assemblyPath">Path to the assembly.</param>
        private static void LoadExternalCodec(string assemblyPath)
        {
            if (string.IsNullOrWhiteSpace(assemblyPath))
            {
                return;
            }

	        if (!File.Exists(assemblyPath))
	        {
				EditorLogging.Print("The codec plug-in '{0}' was not found.", assemblyPath);
		        return;
	        }

            // Check to see if the assembly is an image codec plug-in.
            if (!GorgonApplication.PlugIns.IsPlugInAssembly(assemblyPath))
            {
				EditorLogging.Print("The codec plug-in '{0}' is not a valid .NET assembly.", assemblyPath);
                return;
            }

            if (GorgonApplication.PlugIns.EnumeratePlugIns(assemblyPath).Count == 0)
            {
				EditorLogging.Print("The codec plug-in '{0}' does not contain any image codecs.", assemblyPath);
                return;
            }

            // Load the assembly.
            GorgonApplication.PlugIns.LoadPlugInAssembly(assemblyPath);

            // Get all the plug-ins that support image codecs.
            IEnumerable<GorgonCodecPlugIn> codecPlugIns = from plugIn in GorgonApplication.PlugIns
                                                          let codecPlugIn = plugIn as GorgonCodecPlugIn
														  where codecPlugIn != null
                                                          select codecPlugIn;

            // Enumerate our codecs.
            foreach (GorgonCodecPlugIn plugIn in codecPlugIns)
            {
                GorgonImageCodec codec = plugIn.CreateCodec();

                if (codec == null)
                {
                    continue;
                }

                // Retrieve extensions.
				var description = new StringBuilder(256);
				description.AppendFormat("{0} (*.{1})", codec.CodecDescription, string.Join("; *.", codec.CodecCommonExtensions));
                foreach (string extension in codec.CodecCommonExtensions)
                {
                    _codecs[new GorgonFileExtension(extension, description.ToString())] = codec;
                }
            }
        }

        /// <summary>
        /// Function to create the list of codecs
        /// </summary>
        internal void GetCodecs()
        {
            _codecs = new Dictionary<GorgonFileExtension, GorgonImageCodec>();

            GorgonImageCodec[] codecs =
            {
                new GorgonCodecBMP(),
                new GorgonCodecDDS(), 
                new GorgonCodecGIF(), 
                new GorgonCodecHDP(), 
                new GorgonCodecJPEG(), 
                new GorgonCodecPNG(), 
                new GorgonCodecTGA(), 
                new GorgonCodecTIFF()
            };

            // Get extensions and descriptions from the codecs.
			var description = new StringBuilder(256);
            foreach (var codec in codecs)
            {
	            description.Length = 0;
	            description.AppendFormat("{0} (*.{1})", codec.CodecDescription, string.Join("; *.", codec.CodecCommonExtensions));

                foreach (string extension in codec.CodecCommonExtensions)
                {
                    _codecs[new GorgonFileExtension(extension, description.ToString())] = codec;
                }
            }

            // Load external codecs.
            foreach (string assemblyPath in Settings.CustomCodecs)
            {
                LoadExternalCodec(assemblyPath);
            }

			_codecDropDown = _codecs.Values.Distinct().ToArray();

			FileExtensions.Clear();

			// Update the list of available extensions (because they're not static) when we create our content for display.
			foreach (var codec in _codecs.Where(codec => !FileExtensions.Contains(codec.Key)))
			{
				FileExtensions.Add(codec.Key);
			}
        }

        /// <summary>
        /// Function to determine if a plug-in can be used.
        /// </summary>
        /// <returns>
        /// A string containing a list of reasons why the plug-in is not valid for use, or an empty string if the control is not valid for use.
        /// </returns>        
        protected override string ValidatePlugIn()
        {
            return string.Empty;
        }

        /// <summary>
        /// Function to create a content object interface.
        /// </summary>
        /// <param name="settings">The initial settings for the content.</param>
        /// <returns>
        /// A new content object interface.
        /// </returns>
        protected override ContentObject OnCreateContentObject(ContentSettings settings)
        {
			// Retrieve our codecs.
			GetCodecs();

            return new GorgonImageContent(this, (GorgonImageContentSettings)settings);
        }

        /// <summary>
        /// Funciton to create settings for a content object.
        /// </summary>
        /// <returns>
        /// The settings interface for the content.
        /// </returns>
        protected override ContentSettings OnGetContentSettings()
        {
            return new GorgonImageContentSettings();
        }

        /// <summary>
		/// Function to return the icon for the content.
		/// </summary>
		/// <returns>
		/// The 16x16 image for the content.
		/// </returns>
		public override Image GetContentIcon()
		{
			return Resources.image_16x16;
		}

		/// <summary>
		/// Function to populate file editor attributes for imported content.
		/// </summary>
		/// <param name="stream">Stream to the file.</param>
		/// <param name="attributes">Attributes to populate.</param>
		public override void GetEditorFileAttributes(Stream stream, IDictionary<string, string> attributes)
		{
			GorgonImageCodec codec = CodecDropDownList.FirstOrDefault(item => item.IsReadable(stream));

			if (codec == null)
			{
				return;
			}

			attributes["Codec"] = codec.GetType().FullName;
			attributes["Type"] = Resources.GORIMG_CONTENT_TYPE;
		}
	    #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonImageEditorPlugIn"/> class.
        /// </summary>
        public GorgonImageEditorPlugIn()
            : base(Resources.GORIMG_DESC)
        {
            Settings = new GorgonImageProperties();
			Settings.Load();

			GetCodecs();
        }
        #endregion

		#region IImageEditorPlugIn Members
		/// <summary>
		/// Function to import content from a file system file.
		/// </summary>
		/// <param name="editorFile">The editor file to load.</param>
		/// <param name="imageDataStream">The stream containing the image data.</param>
		/// <returns>
		/// An image editor content object.
		/// </returns>
        IImageEditorContent IImageEditorPlugIn.ImportContent(EditorFile editorFile, Stream imageDataStream)
        {
	        if (editorFile == null)
	        {
		        throw new ArgumentNullException("editorFile");
	        }

            if (imageDataStream == null)
            {
                throw new ArgumentNullException("imageDataStream");
            }

			GetCodecs();

			IImageEditorContent content = new GorgonImageContent(this,
			                                                     new GorgonImageContentSettings
			                                                     {
				                                                     Name = Path.GetFileName(editorFile.FilePath),
				                                                     EditorFile = editorFile
			                                                     });

			content.Load(imageDataStream);

            return content;
        }

		/// <summary>
		/// Property to return the content type for this image editor plug-in.
		/// </summary>
		public string ContentType
		{
			get
			{
				return Resources.GORIMG_CONTENT_TYPE;
			}
		}
		#endregion

		#region IPlugInSettingsUI Members
		/// <summary>
		/// Function to return the UI object for the settings.
		/// </summary>
		/// <returns>
		/// The UI object for the settings.
		/// </returns>
		public PreferencePanel GetSettingsUI()
		{
			return new PanelImagePreferences();	
		}
		#endregion
	}
}
