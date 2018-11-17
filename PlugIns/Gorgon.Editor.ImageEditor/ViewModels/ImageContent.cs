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
// Created: October 30, 2018 7:58:37 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Diagnostics;
using Gorgon.Editor.Content;
using Gorgon.Editor.ImageEditor.Properties;
using Gorgon.Editor.ImageEditor.ViewModels;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Views;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.IO;

namespace Gorgon.Editor.ImageEditor.ViewModels
{
    /// <summary>
    /// The image editor content.
    /// </summary>
    internal class ImageContent
        : EditorContentCommon<ImageContentParameters>, IImageContent
    {
        #region Constants.
        /// <summary>
        /// The attribute key name for the image codec attribute.
        /// </summary>
        public const string CodecAttr = "ImageCodec";
        #endregion

        #region Variables.
        // The image.
        private IGorgonImage _image;
        // The codec used by the image.
        private IGorgonImageCodec _codec;
        // The format support information for the current video card.
        private IReadOnlyDictionary<BufferFormat, IGorgonFormatSupportInfo> _formatSupport;
        // The available pixel formats, based on codec.
        private ObservableCollection<BufferFormat> _pixelFormats = new ObservableCollection<BufferFormat>();
        // The file for the texture converter process.
        private FileInfo _texConvFile;
        // The working directory for temporary data.
        private IGorgonVirtualDirectory _tempDir;
        #endregion

        #region Properties.
        /// <summary>Property to return the type of content.</summary>
        public override string ContentType => ImageEditorCommonConstants.ContentType;

        /// <summary>
        /// Property to set or return the image codec used by the image content.
        /// </summary>
        public IGorgonImageCodec CurrentCodec
        {
            get => _codec;
            private set
            {
                if (_codec == value)
                {
                    return;
                }

                Debug.Assert(value != null, "Codec should never be null.");
                OnPropertyChanging();
                _codec = value;
                File.Metadata.Attributes[CodecAttr] = value.GetType().FullName;
                OnPropertyChanged();                                
            }
        }

        /// <summary>Property to return the name of the content.</summary>
        public string ContentName => File.Name;

        /// <summary>
        /// Property to return the list of codecs available.
        /// </summary>
        public ObservableCollection<IGorgonImageCodec> Codecs
        {
            get;
        } = new ObservableCollection<IGorgonImageCodec>();

        /// <summary>
        /// Property to return the list of available image pixel formats (based on codec).
        /// </summary>
        public ObservableCollection<BufferFormat> PixelFormats
        {
            get => _pixelFormats;
            private set
            {
                if (_pixelFormats == value)
                {
                    return;
                }

                OnPropertyChanging();
                _pixelFormats = value;
                OnPropertyChanged();
            }
        } 

        /// <summary>
        /// Property to return the command used to assign the image codec.
        /// </summary>
        public IEditorCommand<IGorgonImageCodec> SetCodecCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the current pixel format for the image.
        /// </summary>
        public BufferFormat CurrentPixelFormat => _image.Format;

        /// <summary>
        /// Property to return the format support information for the current video card.
        /// </summary>
        public IReadOnlyDictionary<BufferFormat, IGorgonFormatSupportInfo> FormatSupport
        {
            get => _formatSupport;
            private set
            {
                if (_formatSupport == value)
                {
                    return;
                }

                OnPropertyChanging();
                _formatSupport = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Methods.        
        /// <summary>
        /// Function to retrieve the list of pixel formats applicable to each codec type.
        /// </summary>
        /// <param name="codec">The codec to evaluate.</param>
        /// <returns>A list of formats.</returns>
        private ObservableCollection<BufferFormat> GetFilteredFormats(IGorgonImageCodec codec)
        {
            if (codec == null)
            {
                return new ObservableCollection<BufferFormat>();
            }

            var result = new ObservableCollection<BufferFormat>();
            IReadOnlyList<BufferFormat> supportedFormats = codec.SupportedPixelFormats;

            if (_image.FormatInfo.IsCompressed)
            {
                // Assume our block compressed format expands to R8G8B8A8
                supportedFormats = BufferFormat.R8G8B8A8_UNorm.CanConvertToAny(codec.SupportedPixelFormats);

                // Do not provide block compressed formats if we can't convert them.
                if (_texConvFile.Exists)
                {
                    supportedFormats = supportedFormats
                        .Concat(codec.SupportedPixelFormats.Where(item => (new GorgonFormatInfo(item)).IsCompressed))
                        .ToArray();
                }
            }

            foreach (BufferFormat format in supportedFormats.OrderBy(item => item.ToString()))
            {
                if ((!_formatSupport.TryGetValue(format, out IGorgonFormatSupportInfo supportInfo))
                    || (!supportInfo.IsTextureFormat(_image.ImageType))
                    || (supportInfo.IsDepthBufferFormat))
                {
                    continue;
                }

                var formatInfo = new GorgonFormatInfo(format);

                if ((formatInfo.IsPacked) || (formatInfo.IsTypeless))
                {
                    continue;
                }
                                
                if ((!_image.FormatInfo.IsCompressed) && (!formatInfo.IsCompressed) && (!_image.CanConvertToFormat(format)))
                {
                    continue;
                }

                result.Add(format);
            }

            return result;
        }

        /// <summary>
        /// Function to determine if the current image codec can be changed.
        /// </summary>
        /// <param name="codec">The codec to assign.</param>
        /// <returns><b>true</b> if the codec can be changed, <b>false</b> if not.</returns>
        private bool CanSetImageCodec(IGorgonImageCodec codec) => (codec != null) && (Codecs.Contains(codec)) && (codec != CurrentCodec);

        /// <summary>
        /// Function to assign a new image codec.
        /// </summary>
        /// <param name="codec">The codec to assign.</param>
        private void DoSetImageCodec(IGorgonImageCodec codec)
        {
            BusyState.SetBusy();

            try
            {
                CurrentCodec = codec;
                PixelFormats = GetFilteredFormats(codec);
                ContentState = ContentState.Modified;
            }
            catch (Exception ex)
            {
                MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_CODEC_ASSIGN);
            }
            finally
            {
                BusyState.SetIdle();
            }
        }

        /// <summary>Function to initialize the content.</summary>
        /// <param name="injectionParameters">Common view model dependency injection parameters from the application.</param>
        protected override void OnInitialize(ImageContentParameters injectionParameters)
        {
            base.OnInitialize(injectionParameters);

            _codec = injectionParameters.Codec ?? throw new ArgumentMissingException(nameof(injectionParameters.Codec), nameof(injectionParameters));            
            _image = injectionParameters.Image ?? throw new ArgumentMissingException(nameof(injectionParameters.Image), nameof(injectionParameters));
            _formatSupport = injectionParameters.FormatSupport ?? throw new ArgumentMissingException(nameof(injectionParameters.FormatSupport), nameof(injectionParameters));

            IReadOnlyList<IGorgonImageCodec> codecs = injectionParameters.Codecs ?? throw new ArgumentMissingException(nameof(injectionParameters.Codecs), nameof(injectionParameters));

            Codecs.Clear();

            foreach (IGorgonImageCodec codec in codecs.Where(item => item.CanEncode).OrderBy(item => item.Codec))
            {                
                Codecs.Add(codec);
            }

            // The availability of texconv.exe determines whether or not we can use block compressed formats or not.
            Log.Print("Checking for texconv.exe...", LoggingLevel.Simple);
            var pluginDir = new DirectoryInfo(Path.GetDirectoryName(GetType().Assembly.Location));
            _texConvFile = new FileInfo(Path.Combine(pluginDir.FullName, "texconv.exe"));

            if (!_texConvFile.Exists)
            {
                Log.Print($"WARNING: Texconv.exe was not found at {pluginDir.FullName}. Block compressed formats will be unavailable.", LoggingLevel.Simple);
            }
            else
            {
                Log.Print($"Found texconv.exe at '{_texConvFile.FullName}'.", LoggingLevel.Simple);
            }

            _pixelFormats = GetFilteredFormats(_codec);
        }

        /// <summary>
        /// Function to retrieve the image data for displaying on the view.
        /// </summary>
        /// <returns>The underlying image data for display.</returns>
        public IGorgonImage GetImage() => _image;

        /// <summary>Function called when the associated view is loaded.</summary>
        public override void OnLoad()
        {
            _tempDir = ScratchWriter.CreateDirectory("/" + GetType().Assembly.GetName().Name + "_" + Guid.NewGuid().ToString("N"));
        }

        /// <summary>Function called when the associated view is unloaded.</summary>
        public override void OnUnload()
        {
            _image?.Dispose();

            if (_tempDir != null)
            {
                try
                {
                    ScratchWriter.DeleteDirectory(_tempDir.FullPath);
                }
                catch (Exception ex)
                {
                    Log.Print("There was an error cleaning up the image plug in temporary directory.", LoggingLevel.Verbose);
                    Log.LogException(ex);
                }
            }            

            base.OnUnload();
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the ImageContent class.</summary>
        public ImageContent() => SetCodecCommand = new EditorCommand<IGorgonImageCodec>(DoSetImageCodec, CanSetImageCodec);
        #endregion
    }
}
