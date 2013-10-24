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

using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using GorgonLibrary.Editor.ImageEditorPlugIn.Properties;
using GorgonLibrary.IO;

namespace GorgonLibrary.Editor.ImageEditorPlugIn
{
    /// <summary>
    /// Image editor plug-in interface.
    /// </summary>
    public class GorgonImageEditorPlugIn
        : ContentPlugIn, IImageEditorPlugIn
	{
		#region Variables.
		private bool _disposed;                                                             // Flag to indicate whether the object was disposed.
        private static Dictionary<GorgonFileExtension, GorgonImageCodec> _codecs;           // Codecs for the image editor.
        private static readonly List<string> _codecPlugInErrors = new List<string>();       // List of errors that may occur when loading a codec plug-in.
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
        /// Property to return a list of codecs that can be used to read image file formats.
        /// </summary>
        internal static Dictionary<GorgonFileExtension, GorgonImageCodec> Codecs
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

        
        /// <summary>
        /// Property to return the file extensions (and descriptions) for this content type.
        /// </summary>
        /// <remarks>
        /// This dictionary contains the file extension including the leading period as the key (in lowercase), and a tuple containing the file extension, and a description of the file (for display).
        /// </remarks>
        public override GorgonFileExtensionCollection FileExtensions
        {
            get
            {
                // Update the list of available extensions (because they're not static) when we create our content for display.
                foreach (var codec in Codecs.Where(codec => !base.FileExtensions.Contains(codec.Key)))
                {
                    base.FileExtensions.Add(codec.Key);
                }

                return base.FileExtensions;
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

            // Check to see if the assembly is an image codec plug-in.
            if (!Gorgon.PlugIns.IsPlugInAssembly(assemblyPath))
            {
                _codecPlugInErrors.Add(string.Format(Resources.GORIMG_CODEC_LOAD_NOT_A_PLUGIN, assemblyPath));
                return;
            }

            if (Gorgon.PlugIns.EnumeratePlugIns(assemblyPath).Count == 0)
            {
                _codecPlugInErrors.Add(string.Format(Resources.GORIMG_CODEC_NONE_FOUND, assemblyPath));
                return;
            }

            // Load the assembly.
            Gorgon.PlugIns.LoadPlugInAssembly(assemblyPath);

            // Get all the plug-ins that support image codecs.
            IEnumerable<GorgonCodecPlugIn> codecPlugIns = from plugIn in Gorgon.PlugIns
                                                          let codecPlugIn = plugIn as GorgonCodecPlugIn
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
        private static void GetCodecs()
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
        }

        /// <summary>
        /// Function to determine if a plug-in can be used.
        /// </summary>
        /// <returns>
        /// A string containing a list of reasons why the plug-in is not valid for use, or an empty string if the control is not valid for use.
        /// </returns>        
        protected override string ValidatePlugIn()
        {
            if (_codecPlugInErrors.Count == 0)
            {
                return string.Empty;
            }

            var codecProblems = new StringBuilder(1024);

            // Build a list of codec problems.
            // The image editor will be disabled until these issues are resolved.
            for (int i = 0; i < _codecPlugInErrors.Count; ++i)
            {
                string errorLine = _codecPlugInErrors[i];

                if (_codecPlugInErrors.Count > 1)
                {
                    if (codecProblems.Length > 0)
                    {
                        codecProblems.Append("\n");
                    }

                    codecProblems.AppendFormat("{0}. {1}", i + 1, errorLine);
                }
                else
                {
                    codecProblems.Append(errorLine);
                }
            }

            return codecProblems.ToString();
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
	        GorgonImageCodec codec;
	        var extension = new GorgonFileExtension(Path.GetExtension(settings.Name));

			// Use the proper codec for the image.
	        if (!Codecs.TryGetValue(extension, out codec))
			{
				throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORIMG_NO_CODEC, settings.Name));
	        }

            return new GorgonImageContent(settings.Name, codec);
        }

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
				}

				_disposed = true;
			}

			base.Dispose(disposing);
		}

        /// <summary>
        /// Funciton to create settings for a content object.
        /// </summary>
        /// <returns>
        /// The settings interface for the content.
        /// </returns>
        public override ContentSettings GetContentSettings()
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
	    #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonImageEditorPlugIn"/> class.
        /// </summary>
        public GorgonImageEditorPlugIn()
            : base(Resources.GORIMG_DESC)
        {
            Settings = new GorgonImageProperties();
        }
        #endregion

		#region IImageEditorPlugIn Members
		/// <summary>
		/// Property to return the image file extensions supported by the plug-in.
		/// </summary>
		public IEnumerable<GorgonFileExtension> Extensions
		{
			get
			{
				return Codecs.Keys;
			}
		}
		#endregion
	}
}
