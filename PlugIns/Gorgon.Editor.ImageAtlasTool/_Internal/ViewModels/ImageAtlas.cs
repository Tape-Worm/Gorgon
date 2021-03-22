﻿#region MIT
// 
// Gorgon.
// Copyright (C) 2019 Michael Winsor
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
// Created: May 7, 2019 6:23:18 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Gorgon.Editor.Content;
using Gorgon.Editor.ImageAtlasTool.Properties;
using Gorgon.Editor.UI;
using Gorgon.Graphics.Core;
using Gorgon.IO;
using Gorgon.Renderers;
using Gorgon.Renderers.Services;
using DX = SharpDX;
using Gorgon.Graphics.Imaging;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Gorgon.Graphics;
using Gorgon.Editor.Services;

namespace Gorgon.Editor.ImageAtlasTool
{
    /// <summary>
    /// The view model for the main UI.
    /// </summary>
    internal class ImageAtlas
		: EditorToolViewModelBase<ImageAtlasParameters>, IImageAtlas
	{
		#region Variables.
		// The settings for the texture atlas.
		private TextureAtlasSettings _settings;
		// The selected images.
		private IContentFile[] _imageFiles = Array.Empty<IContentFile>();
		// The texture atlas service.
		private IGorgonTextureAtlasService _atlasService;
		// The file I/O service used to manage the atlas files.
		private FileIOService _fileIO;
		// The base name for textures generated by the atlas.
		private string _baseTextureName;
		// The texture atlas.
		private GorgonTextureAtlas _atlas;
		// The preview array index.
		private int _previewArrayIndex;
		// The preview texture index.
		private int _previewTextureIndex;
		// The images used to generate the atlas.
		private IReadOnlyDictionary<IContentFile, IGorgonImage> _images = new Dictionary<IContentFile, IGorgonImage>();
		// The currently selected image.
		private (IContentFile, IGorgonImage) _currentImage;
		private int _currentImageIndex;
		// The cancellation token source for loading images.
		private CancellationTokenSource _loadImageCancelSource;
		// The task used for loading the images.
		private Task<IReadOnlyDictionary<IContentFile, IGorgonImage>> _loadImageTask;
		// The list of sprites used for the atlas.
		private IReadOnlyDictionary<IContentFile, GorgonSprite> _atlasSpriteList;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the view model for the image file loader.
		/// </summary>
		public IImageFiles ImageFiles
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return currently selected image.
		/// </summary>
		public (IContentFile file, IGorgonImage image) CurrentImage
		{
			get => _currentImage;
			private set
			{
				if (_currentImage == value)
				{
					return;
				}

				OnPropertyChanging();
				_currentImage = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Property to return the images loaded into the tool.
		/// </summary>
		public IReadOnlyDictionary<IContentFile, IGorgonImage> Images
		{
			get => _images;
			private set
			{
				if (_images == value)
				{
					return;
				}

				OnPropertyChanging();
				if (_images is not null)
				{
					foreach (IGorgonImage image in _images.Values)
					{
						image?.Dispose();
					}
				}

				_images = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Property to return the number of images that were loaded.
		/// </summary>
		public int LoadedImageCount => _imageFiles.Length;

		/// <summary>Property to return the path for the output files.</summary>
		public string OutputPath
		{
			get => _settings.LastOutputDir;
			private set
			{
				if (string.Equals(_settings.LastOutputDir, value, StringComparison.OrdinalIgnoreCase))
				{
					return;
				}

				OnPropertyChanging();
				_settings.LastOutputDir = value?.FormatDirectory('/');
				OnPropertyChanged();
			}
		}

		/// <summary>Property to set or return the maximum size for the atlas texture.</summary>
		public DX.Size2 MaxTextureSize
		{
			get => _settings.MaxTextureSize;
			set
			{
				if (_settings.MaxTextureSize.Equals(value))
				{
					return;
				}

				OnPropertyChanging();
				_settings.MaxTextureSize = value;
				OnPropertyChanged();
			}
		}

		/// <summary>Property to set or return the maximum number of array indices for the atlas texture.</summary>
		public int MaxArrayCount
		{
			get => _settings.MaxArrayCount;
			set
			{
				if (_settings.MaxArrayCount == value)
				{
					return;
				}

				OnPropertyChanging();
				_settings.MaxArrayCount = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Property to set or return the amount of padding, in pixels around each image on the texture.
		/// </summary>
		public int Padding
		{
			get => _settings.Padding;
			set
			{
				if (_settings.Padding == value)
				{
					return;
				}

				OnPropertyChanging();
				_settings.Padding = value;
				OnPropertyChanged();
			}
		}


		/// <summary>
		/// Property to return the base atlas texture name.
		/// </summary>
		public string BaseTextureName
		{
			get => _baseTextureName;
			set
			{
				if (string.Equals(_baseTextureName, value, StringComparison.CurrentCulture))
				{
					return;
				}

				OnPropertyChanging();
				_baseTextureName = value ?? string.Empty;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Property to set or return whether to generate sprites along with the atlas.
		/// </summary>
		public bool GenerateSprites
		{
			get => _settings.GenerateSprites;
			set
			{
				if (_settings.GenerateSprites == value)
				{
					return;
				}

				OnPropertyChanging();
				_settings.GenerateSprites = value;
				OnPropertyChanged();
			}
		}


		/// <summary>
		/// Property to return the atlas after it's been generated.
		/// </summary>
		public GorgonTextureAtlas Atlas
		{
			get => _atlas;
			private set
			{
				if (_atlas == value)
				{
					return;
				}

				OnPropertyChanging();

				if (_atlas?.Textures is not null)
				{
					foreach (GorgonTexture2DView texture in _atlas.Textures)
					{
						texture.Dispose();
					}
				}

				_atlas = value;
				OnPropertyChanged();

				PreviewArrayIndex = 0;
				PreviewTextureIndex = 0;
			}
		}

		/// <summary>
		/// Property to return the preview array index.
		/// </summary>
		public int PreviewArrayIndex
		{
			get => _previewArrayIndex;
			private set
			{
				if (_previewArrayIndex == value)
				{
					return;
				}

				OnPropertyChanging();
				_previewArrayIndex = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Property to return the preview texture index.
		/// </summary>
		public int PreviewTextureIndex
		{
			get => _previewTextureIndex;
			private set
			{
				if (_previewTextureIndex == value)
				{
					return;
				}

				OnPropertyChanging();
				_previewTextureIndex = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Property to return the command used to calculate best fit sizes.
		/// </summary>
		public IEditorCommand<object> CalculateSizesCommand
		{
			get;
		}

		/// <summary>Property to return the folder selection command.</summary>
		public IEditorCommand<object> SelectFolderCommand
		{
			get;
		}

		/// <summary>
		/// Property to return the command used to generate the atlas.
		/// </summary>
		public IEditorCommand<object> GenerateCommand
		{
			get;
		}

		/// <summary>
		/// Property to return the command to move to the next preview item.
		/// </summary>
		public IEditorCommand<object> NextPreviewCommand
		{
			get;
		}

		/// <summary>
		/// Property to return the command to move to the previous preview item.
		/// </summary>
		public IEditorCommand<object> PrevPreviewCommand
		{
			get;
		}


		/// <summary>
		/// Property to return the command used to commit the atlas data back to the file system.
		/// </summary>
		public IEditorCommand<CancelEventArgs> CommitAtlasCommand
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to determine whether images can be loaded or not.
		/// </summary>
		/// <returns><b>true</b> if the images can be loaded, <b>false</b> if not.</returns>
		private bool CanLoadImages() => (ImageFiles.SelectedFiles.Count > 1) && (_loadImageTask is null);

		/// <summary>
		/// Function to load the selected images.
		/// </summary>
		private async Task DoLoadImagesAsync()
		{
			try
			{
				// Unload any current atlas.
				Atlas = null;

				_imageFiles = ImageFiles.SelectedFiles.Select(item => item.File).ToArray();

				if (string.IsNullOrWhiteSpace(_settings.LastOutputDir))
				{
					OutputPath = Path.GetDirectoryName(ImageFiles.SelectedFiles[0].File.Path).FormatDirectory('/');
				}

				if (string.IsNullOrWhiteSpace(BaseTextureName))
				{
					BaseTextureName = Path.GetFileNameWithoutExtension(_imageFiles[0].Name);
				}

				_loadImageCancelSource = new CancellationTokenSource();
				_loadImageTask = _fileIO.LoadImagesAsync(_imageFiles, imageFile => ImageFiles.LoadingImage = imageFile, _loadImageCancelSource.Token);

				IReadOnlyDictionary<IContentFile, IGorgonImage> images = await _loadImageTask;
				
				if (images.Count != 0)
				{
					NotifyPropertyChanging(nameof(LoadedImageCount));
					Images = images;
				}

				_loadImageTask = null;
				_loadImageCancelSource = null;

				if (images.Count != 0)
				{
					NotifyPropertyChanged(nameof(LoadedImageCount));
					CurrentImage = (_imageFiles[0], _images[_imageFiles[0]]);
				}
			}
			catch (Exception ex)
			{
				HostServices.MessageDisplay.ShowError(ex, Resources.GORIAG_ERR_LOAD_IMAGES);
			}
			finally
			{
				_loadImageTask = null;
				_loadImageCancelSource = null;
			}
		}

		/// <summary>
		/// Function to browse folders on the file system.
		/// </summary>
		private void DoBrowseFolders()
		{
			try
			{
				string outputDir = _settings.LastOutputDir.FormatDirectory('/');

				if (!ContentFileManager.DirectoryExists(outputDir))
				{
					outputDir = "/";
				}

				outputDir = HostServices.FolderBrowser.GetFolderPath(outputDir, Resources.GORIAG_CAPTION_FOLDER_SELECT, Resources.GORIAG_DESC_FOLDER_SELECT);

				if (string.IsNullOrWhiteSpace(outputDir))
				{
					return;
				}

				OutputPath = outputDir;
			}
			catch (Exception ex)
			{
				HostServices.MessageDisplay.ShowError(ex, Resources.GORIAG_ERR_FOLDER_SELECT);
			}
		}

		/// <summary>
        /// Function to retrieve sprites for the atlas generation phase.
        /// </summary>
        /// <returns>A list of sprites for each image.</returns>
		private IReadOnlyDictionary<IContentFile, GorgonSprite> GetSprites()
		{
			var result = new Dictionary<IContentFile, GorgonSprite>();

			foreach (KeyValuePair<IContentFile, IGorgonImage> image in _images)
			{				
				var spriteTexture = GorgonTexture2DView.CreateTexture(HostServices.GraphicsContext.Graphics,
																	  new GorgonTexture2DInfo(image.Value.Width, image.Value.Height, image.Value.Format)
																	  {
																		  Name = image.Key.Path,
																		  IsCubeMap = false,
																		  Binding = TextureBinding.ShaderResource,
																		  Usage = ResourceUsage.Immutable
																	 }, image.Value);

				var sprite = new GorgonSprite
				{
					Texture = spriteTexture,
					TextureRegion = new DX.RectangleF(0, 0, 1, 1),
					Size = new DX.Size2F(spriteTexture.Width, spriteTexture.Height)
				};

				result[image.Key] = sprite;
			}

			return result;
		}

		/// <summary>
        /// Function to unload sprite textures from memory.
        /// </summary>
        /// <param name="sprites">The list of sprites to evaluate.</param>
		private void UnloadSpriteTextures(IEnumerable<GorgonSprite> sprites)
		{
			foreach (GorgonTexture2DView texture in sprites.Select(item => item.Texture))
			{
				texture?.Dispose();
			}
		}

		/// <summary>
		/// Function to determine if the size of the atlas can be calculated.
		/// </summary>
		/// <returns><b>true</b> if calculation is allowed, <b>false</b> if not.</returns>
		private bool CanCalculateSize() => (LoadedImageCount > 1) && (_loadImageTask is null);

		/// <summary>
		/// Function to calculate the best fit size of the texture and array count.
		/// </summary>
		private void DoCalculateSize()
		{
			HostServices.BusyService.SetBusy();

			try
			{
				_atlasSpriteList = GetSprites();				

				(DX.Size2 textureSize, int arrayCount) = _atlasService.GetBestFit(_atlasSpriteList.Values, new DX.Size2(256, 256), MaxArrayCount);

				if ((textureSize.Width == 0) || (textureSize.Height == 0) || (arrayCount == 0))
				{
					HostServices.MessageDisplay.ShowError(Resources.GORIAG_ERR_CALC_TOO_LARGE);
					return;
				}

				MaxTextureSize = textureSize;
				MaxArrayCount = arrayCount;
			}
			catch (Exception ex)
			{
				HostServices.MessageDisplay.ShowError(ex, Resources.GORIAG_ERR_CALC);
			}
			finally
			{
				// We no longer need the textures.
				if (_atlasSpriteList is not null)
				{
					UnloadSpriteTextures(_atlasSpriteList.Values);
				}

				HostServices.BusyService.SetIdle();
			}
		}

		/// <summary>
		/// Function to determine if the atlas can be generated.
		/// </summary>
		/// <returns><b>true</b> if the atlas can be generated, <b>false</b> if not.</returns>
		private bool CanGenerate() => (LoadedImageCount > 1) && (!string.IsNullOrWhiteSpace(OutputPath)) && (!string.IsNullOrWhiteSpace(BaseTextureName)) && (_loadImageTask is null);

		/// <summary>
        /// Function to perform the atlas generation.
        /// </summary>
        /// <param name="sprites">The sprites used to build the atlas.</param>
		private GorgonTextureAtlas GenerateAtlas(IEnumerable<GorgonSprite> sprites)
		{
			GorgonTextureAtlas atlas;

			_atlasService.Padding = _settings.Padding;
			_atlasService.ArrayCount = _settings.MaxArrayCount;
			_atlasService.TextureSize = _settings.MaxTextureSize;
			_atlasService.BaseTextureName = $"{_settings.LastOutputDir}{_baseTextureName.FormatFileName()}";

			IReadOnlyDictionary<GorgonSprite, (int textureIndex, DX.Rectangle region, int arrayIndex)> regions = _atlasService.GetSpriteRegions(sprites);

			if ((regions is null) || (regions.Count == 0))
			{
				HostServices.MessageDisplay.ShowError(Resources.GORIAG_ERR_NO_ROOM);
				return null;
			}

			atlas = _atlasService.GenerateAtlas(regions, BufferFormat.R8G8B8A8_UNorm);

			if ((atlas is null) || (atlas.Textures is null) || (atlas.Sprites is null) || (atlas.Textures.Count == 0))
			{
				HostServices.MessageDisplay.ShowError(Resources.GORIAG_ERR_GEN_ATLAS);
				return null;
			}

			return atlas;
		}

		/// <summary>
		/// Function to perform the atlas generation.
		/// </summary>
		private void DoGenerate()
		{
			GorgonTextureAtlas atlas = null;

			HostServices.BusyService.SetBusy();

			void UnloadAtlasTextures()
			{
				if (atlas is null)
				{
					return;
				}

				foreach (GorgonTexture2DView texture in atlas.Textures)
				{
					texture.Dispose();
				}				
			}

			try
			{
				Debug.Assert(_images.Count != 0, "No images were returned.");

				_atlasSpriteList = GetSprites();

				atlas = GenerateAtlas(_atlasSpriteList.Values);
				// Keep this separate so that we can dispose the textures from the new atlas should something go wrong.
				Atlas = atlas;				
			}
			catch (Exception ex)
			{
				if ((atlas?.Textures is not null) && (Atlas != atlas))
				{
					UnloadAtlasTextures();
				}

				HostServices.MessageDisplay.ShowError(ex, Resources.GORIAG_ERR_GEN_ATLAS);
			}
			finally
			{
				// We no longer need the textures.
				if (_atlasSpriteList is not null)
				{
					UnloadSpriteTextures(_atlasSpriteList.Values);
				}
				HostServices.BusyService.SetIdle();
			}
		}


		/// <summary>
		/// Function to determine we can move to the next preview image.
		/// </summary>
		/// <returns><b>true</b> if possible, <b>false</b> if not.</returns>
		private bool CanNextPreview()
		{
			if (_loadImageTask is not null)
			{
				return false;
			}

			if (Atlas is not null)
			{
				if ((Atlas?.Textures is null) || (Atlas.Textures.Count == 0))
				{
					return false;
				}

				GorgonTexture2D texture = Atlas.Textures[PreviewTextureIndex].Texture;

				return ((PreviewArrayIndex + 1 < texture.ArrayCount) || (PreviewTextureIndex + 1 < Atlas.Textures.Count));
			}

            return (Images is not null) && (Images.Count != 0) && (_currentImageIndex + 1 < _imageFiles.Length);
        }

        /// <summary>
        /// Function to move to the next index in the preview.
        /// </summary>
        private void DoNextPreview()
		{
			try
			{
				if (Atlas is null)
				{
					CurrentImage = (_imageFiles[++_currentImageIndex], _images[_imageFiles[_currentImageIndex]]);
					return;
				}

				GorgonTexture2D texture = Atlas.Textures[PreviewTextureIndex].Texture;

				if (PreviewArrayIndex + 1 >= texture.ArrayCount)
				{
					PreviewTextureIndex++;
					PreviewArrayIndex = 0;
				}
				else
				{
					PreviewArrayIndex++;
				}
			}
			catch (Exception ex)
			{
				HostServices.MessageDisplay.ShowError(ex, Resources.GORIAG_ERR_PREVIEW_ARRAY);
			}
		}

		/// <summary>
		/// Function to determine we can move to the next preview image.
		/// </summary>
		/// <returns><b>true</b> if possible, <b>false</b> if not.</returns>
		private bool CanPrevPreview()
		{
			if (_loadImageTask is not null)
			{
				return false;
			}

#pragma warning disable IDE0046 // Convert to conditional expression
            if (Atlas is not null)
            {
                return (Atlas.Textures is not null) && (Atlas.Textures.Count != 0) && ((PreviewArrayIndex - 1 >= 0) || (PreviewTextureIndex - 1 >= 0));
            }

            return (Images is not null) && (Images.Count != 0) && (_currentImageIndex - 1 >= 0);
#pragma warning restore IDE0046 // Convert to conditional expression
		}

		/// <summary>
		/// Function to move to the next index in the preview.
		/// </summary>
		private void DoPrevPreview()
		{
			try
			{
				if (Atlas is null)
				{
					CurrentImage = (_imageFiles[--_currentImageIndex], _images[_imageFiles[_currentImageIndex]]);
					return;
				}

				if (PreviewArrayIndex - 1 < 0)
				{
					PreviewTextureIndex--;
					PreviewArrayIndex = 0;
				}
				else
				{
					PreviewArrayIndex--;
				}
			}
			catch (Exception ex)
			{
				HostServices.MessageDisplay.ShowError(ex, Resources.GORIAG_ERR_PREVIEW_ARRAY);
			}
		}

		/// <summary>
		/// Function to determine if the atlas data can be committed back to the file system.
		/// </summary>
		/// <param name="args">The arguments to pass to the command.</param>
		/// <returns><b>true</b> if the data can be committed, <b>false</b> if not.</returns>
		private bool CanCommitAtlas(CancelEventArgs args) => (Atlas?.Textures is not null) && (Atlas.Textures.Count > 0) && (_images.Count > 0) && (_loadImageTask is null);

		/// <summary>
		/// Function to commit the atlas data back to the file system.
		/// </summary>
		/// <param name="args">The arguments to pass to the command.</param>
		private void DoCommitAtlas(CancelEventArgs args)
		{
			GorgonTextureAtlas atlas = null;

			// Function to unload the textures from the atlas.
			void UnloadTextures()
			{
				if ((atlas is null) || (atlas == Atlas))
				{
					return;
				}

				foreach (GorgonTexture2DView texture in atlas.Textures)
				{
					texture?.Dispose();
				}
			}

			try
			{				
				// If we've changed the base name, regenerate the texture.
				string nameCheck = $"{_settings.LastOutputDir}{_baseTextureName.FormatFileName()}";				

				if ((_atlasSpriteList is null) || (!string.Equals(nameCheck, _atlasService.BaseTextureName, StringComparison.OrdinalIgnoreCase)))
				{
					_atlasSpriteList = GetSprites();
					atlas = GenerateAtlas(_atlasSpriteList.Values);
				}
				else
				{
					atlas = Atlas;
				}
				UnloadSpriteTextures(_atlasSpriteList.Values);

				if (_fileIO.HasExistingFiles(atlas))
				{
					if (HostServices.MessageDisplay.ShowConfirmation(Resources.GORIAG_CONFIRM_OVERWRITE) == MessageResponse.No)
					{
						UnloadTextures();
						args.Cancel = true;
						return;
					}
				}
				
				HostServices.BusyService.SetBusy();
				_fileIO.SaveAtlas(_atlasSpriteList, atlas, _settings.GenerateSprites);

				Atlas = atlas;
			}
			catch (Exception ex)
			{
				// Take out any allocated textures.
				UnloadTextures();

				HostServices.MessageDisplay.ShowError(ex, Resources.GORIAG_ERR_SAVE);
				args.Cancel = true;
			}
			finally
			{
				if (_atlasSpriteList is not null)
				{
					UnloadSpriteTextures(_atlasSpriteList.Values);
				}
				HostServices.BusyService.SetIdle();
			}
		}

		/// <summary>
		/// Function called when the file selection is canceled.
		/// </summary>
		/// <returns>A task for asynchronous operation.</returns>
		private async Task DoCancelAsync()
		{
			try
			{
				if ((_loadImageCancelSource is null) || (_loadImageTask is null))
				{
					return;
				}

				_loadImageCancelSource.Cancel();

				await _loadImageTask;

				_loadImageTask = null;
				_loadImageCancelSource = null;
			}
			catch (Exception ex)
			{
				HostServices.Log.Print("ERROR: There was an error canceling the image atlas operation.", Diagnostics.LoggingLevel.Simple);
				HostServices.Log.LogException(ex);
			}
		}

		/// <summary>Function to inject dependencies for the view model.</summary>
		/// <param name="injectionParameters">The parameters to inject.</param>
		/// <remarks>
		/// Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.
		/// </remarks>
		protected override void OnInitialize(ImageAtlasParameters injectionParameters)
		{
			base.OnInitialize(injectionParameters);

			_settings = injectionParameters.Settings;
			ImageFiles = injectionParameters.ImageFiles;
			_atlasService = injectionParameters.AtlasGenerator;
			_fileIO = injectionParameters.FileIO;
		}

		/// <summary>Function called when the associated view is loaded.</summary>
		public override void OnLoad()
		{
			base.OnLoad();

			if (ImageFiles is not null)
			{
				ImageFiles.ConfirmLoadCommand = new EditorAsyncCommand<object>(DoLoadImagesAsync, CanLoadImages);
				ImageFiles.CancelCommand = new EditorAsyncCommand<object>(DoCancelAsync);
			}
		}

		/// <summary>Function called when the associated view is unloaded.</summary>
		public override void OnUnload()
		{
			CurrentImage = (null, null);

			if (ImageFiles is not null)
			{
				ImageFiles.CancelCommand = null;
				ImageFiles.ConfirmLoadCommand = null;
			}

			_loadImageCancelSource?.Cancel();
			Interlocked.Exchange(ref _loadImageTask, null);

			foreach (IGorgonImage image in _images.Values)
			{
				image?.Dispose();
			}

			_images = new Dictionary<IContentFile, IGorgonImage>();
			_imageFiles = Array.Empty<IContentFile>();
			Atlas = null;

			base.OnUnload();
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>Initializes a new instance of the <see cref="ImageAtlas"/> class.</summary>
		public ImageAtlas()
		{
			SelectFolderCommand = new EditorCommand<object>(DoBrowseFolders);
			CalculateSizesCommand = new EditorCommand<object>(DoCalculateSize, CanCalculateSize);
			GenerateCommand = new EditorCommand<object>(DoGenerate, CanGenerate);
			NextPreviewCommand = new EditorCommand<object>(DoNextPreview, CanNextPreview);
			PrevPreviewCommand = new EditorCommand<object>(DoPrevPreview, CanPrevPreview);
			CommitAtlasCommand = new EditorCommand<CancelEventArgs>(DoCommitAtlas, CanCommitAtlas);
		}
		#endregion
	}
}
