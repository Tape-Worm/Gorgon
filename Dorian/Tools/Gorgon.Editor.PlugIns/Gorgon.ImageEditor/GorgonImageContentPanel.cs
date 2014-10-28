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
// Created: Thursday, March 07, 2013 9:51:37 PM
// 
#endregion

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Editor.ImageEditorPlugIn.Properties;
using GorgonLibrary.Graphics;
using GorgonLibrary.IO;
using GorgonLibrary.Math;
using GorgonLibrary.Renderers;
using GorgonLibrary.UI;
using SlimMath;

namespace GorgonLibrary.Editor.ImageEditorPlugIn
{
    /// <summary>
    /// Control for displaying image data.
    /// </summary>
    partial class GorgonImageContentPanel 
		: ContentPanel
	{
		#region Variables.
		// Image content.
        private GorgonImageContent _content;
		// Texture to display.
	    private GorgonTexture _texture;
		// Background texture.
	    private GorgonTexture2D _backgroundTexture;
        // The current shader view.
        private GorgonTextureShaderView _currentView;
		// Pattern for selector box.
	    private GorgonTexture2D _pattern;
		// The cube face locations.
		private readonly Rectangle[] _cubeFaceLocations = new Rectangle[6];
		// Names for the cube faces.
		private readonly string[] _cubeFaceNames = new string[6];
		// Font for our dislpay.
	    private GorgonFont _font;
		// Text sprite.
	    private GorgonText _text;
		// Scroller for the selector pattern.
	    private Vector2 _selectScroll;
		// Selector sprite.
		private GorgonSprite _selectorSprite;
		// Face we're currently over with the mouse.
	    private int _hoverFace = -1;
		// The index of the current cube we're on.
	    private int _cubeIndex;
        #endregion

        #region Methods.
		/// <summary>
		/// Panels the texture display on mouse wheel.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="mouseEventArgs">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void panelTextureDisplay_MouseWheel(object sender, MouseEventArgs mouseEventArgs)
		{
			if ((_content == null)
				|| (_content.ImageType != ImageType.ImageCube))
			{
				return;
			}

			if (_content.ArrayCount <= 6)
			{
				return;
			}

			int selectedFace = _content.ArrayIndex - _cubeIndex;

			if (mouseEventArgs.Delta < 0)
			{
				_cubeIndex -= 6;

				if (_cubeIndex < 0)
				{
					_cubeIndex = 0;
				}
			}

			if (mouseEventArgs.Delta > 0)
			{
				int maxCubeIndices = _content.ArrayCount - 6;

				_cubeIndex += 6;

				if (_cubeIndex >= maxCubeIndices)
				{
					_cubeIndex = maxCubeIndices;
				}
			}
			
			_content.ArrayIndex = _cubeIndex + selectedFace;

			ValidateControls();
			UpdateBufferInfoLabels();
		}

		/// <summary>
		/// Handles the MouseDown event of the panelTextureDisplay control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void panelTextureDisplay_MouseDown(object sender, MouseEventArgs e)
		{
			panelTextureDisplay.Focus();

			if ((_content == null)
			    || (_content.ImageType != ImageType.ImageCube)
				|| (_hoverFace == -1))
			{
				return;
			}

			// Set the current array index.
			_content.ArrayIndex = _cubeIndex + _hoverFace;
			ValidateControls();
			UpdateBufferInfoLabels();
		}

		/// <summary>
		/// Handles the MouseLeave event of the panelTextureDisplay control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void panelTextureDisplay_MouseLeave(object sender, EventArgs e)
		{
			_hoverFace = -1;
		}

		/// <summary>
		/// Function to perform a hit test for a cube face.
		/// </summary>
		/// <param name="mouseCursor">Cursor position.</param>
		/// <returns>The index of the face that the cursor is over, or -1 if over no faces.</returns>
	    private int CubeFaceHitTest(Point mouseCursor)
	    {
			for (int i = 0; i < _cubeFaceLocations.Length; ++i)
			{
				if (!_cubeFaceLocations[i].Contains(mouseCursor))
				{
					continue;
				}

				return i;
			}

			return -1;
	    }

		/// <summary>
		/// Handles the MouseMove event of the panelTextureDisplay control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void panelTextureDisplay_MouseMove(object sender, MouseEventArgs e)
		{
			if ((_content == null)
			    || (_content.ImageType != ImageType.ImageCube))
			{
				return;
			}

			_hoverFace = CubeFaceHitTest(e.Location);
		}

		/// <summary>
		/// Function to update the cube face location list.
		/// </summary>
	    private void GetCubeFaceLocations()
	    {
		    if ((_texture == null)
		        || (_content == null)
		        || (_content.ImageType != ImageType.ImageCube))
		    {
			    return;
		    }

			Vector2 clientSize = panelTextureDisplay.ClientSize;
			var blockSize = (Size)(new Vector2(clientSize.X / 4.0f, clientSize.Y / 3.0f));
			
			// Set -X
			var position = (Point)(new Vector2(0, (clientSize.Y / 2.0f) - (blockSize.Height / 2.0f)));
			_cubeFaceLocations[1] = new Rectangle(position, blockSize);
			_cubeFaceNames[1] = @"-X";

			// Set +Z
			position = new Point(blockSize.Width, position.Y);
			_cubeFaceLocations[4] = new Rectangle(position, blockSize);
			_cubeFaceNames[4] = @"+Z";

			// Set +X
			position = new Point(blockSize.Width * 2, position.Y);
			_cubeFaceLocations[0] = new Rectangle(position, blockSize);
			_cubeFaceNames[0] = @"+X";

			// Set -Z
			position = new Point(blockSize.Width * 3, position.Y);
			_cubeFaceLocations[5] = new Rectangle(position, blockSize);
			_cubeFaceNames[5] = @"-Z";

			// Set +Y
			position = new Point(blockSize.Width, position.Y - blockSize.Height);
			_cubeFaceLocations[2] = new Rectangle(position, blockSize);
			_cubeFaceNames[2] = @"+Y";

			// Set -Y
			position = new Point(blockSize.Width, position.Y + (blockSize.Height * 2));
			_cubeFaceLocations[3] = new Rectangle(position, blockSize);
			_cubeFaceNames[3] = @"-Y";
	    }

		/// <summary>
		/// Handles the Resize event of the panelTextureDisplay control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void panelTextureDisplay_Resize(object sender, EventArgs e)
		{
			GetCubeFaceLocations();
		}

		/// <summary>
		/// Handles the Click event of the buttonGenerateMips control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonGenerateMips_Click(object sender, EventArgs e)
		{
			try
			{
				if (GorgonDialogs.ConfirmBox(ParentForm, Resources.GORIMG_DLG_GENERATE_MIPS) == ConfirmationResult.No)
				{
					return;
				}

				Cursor.Current = Cursors.WaitCursor;

				_content.GenerateMipMaps();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
			finally
			{
				ValidateControls();
				Cursor.Current = Cursors.Default;
			}
		}

		/// <summary>
		/// Function to confirm whether to overwrite the currently selected image buffer.
		/// </summary>
		/// <returns>TRUE to overwrite, FALSE to stop.</returns>
	    private bool ConfirmBufferOverwrite()
	    {
			// If we have data in this buffer, then ask if we want to over write it.
			if (!_content.ImageBufferHasData())
			{
				return true;
			}

			return GorgonDialogs.ConfirmBox(ParentForm,
			                                string.Format(
			                                              _content.ImageType == ImageType.Image3D
				                                              ? Resources.GORIMG_DLG_OVERWRITE_DEPTH_BUFFER
				                                              : Resources.GORIMG_DLG_OVERWRITE_ARRAY_BUFFER,
														  _content.MipLevel + 1,
														  _content.ImageType == ImageType.Image3D ? _content.DepthSlice + 1 : _content.ArrayIndex + 1)) != ConfirmationResult.No;
	    }

		/// <summary>
		/// Function to confirm whether to crop or resize the image.
		/// </summary>
		/// <param name="path">Path to the image to crop/resize.</param>
		/// <param name="image">Image data to crop/resize</param>
		/// <returns>The selected operation and filter.</returns>
	    private Tuple<bool, ImageFilter, bool> ConfirmCropResize(string path, GorgonImageData image)
	    {
		    if ((_content.Buffer.Width == image.Settings.Width)
				&& (_content.Buffer.Height == image.Settings.Height))
		    {
				return new Tuple<bool, ImageFilter, bool>(true, ImageFilter.Point, true);
		    }

			using (var resizeCrop = new FormResizeCrop(path, new Size(image.Settings.Width, image.Settings.Height), new Size(_content.Buffer.Width, _content.Buffer.Height)))
		    {
			    if (resizeCrop.ShowDialog(ParentForm) != DialogResult.Cancel)
			    {
				    return new Tuple<bool, ImageFilter, bool>(resizeCrop.CropImage, resizeCrop.Filter, resizeCrop.PreserveAspectRatio);
			    }

			    GorgonDialogs.ErrorBox(ParentForm, string.Format(Resources.GORIMG_ERR_CANNOT_LOAD_IMAGE_BUFFER,
			                                                     image.Settings.Width,
			                                                     image.Settings.Height,
																 _content.Buffer.Width,
																 _content.Buffer.Height));
			    return null;
		    }
	    }

	    /// <summary>
        /// Function called after a drag/drop operation is finished.
        /// </summary>
        /// <param name="dragFile">The file being dragged.</param>
        private void OnDrop(DragFile dragFile)
        {
            GorgonImageData image = null;

            try
            {
                Debug.Assert(ParentForm != null, "No parent form!?");
                ParentForm.Focus();
                ParentForm.BringToFront();

                if (_content.ImageType == ImageType.ImageCube)
                {
                    _content.ArrayIndex = _hoverFace + _cubeIndex;
                    UpdateBufferInfoLabels();
                }

                if (!ConfirmBufferOverwrite())
	            {
		            return;
	            }

				Cursor.Current = Cursors.WaitCursor;

	            EditorFile file;
                
                image = GetImageFromFileSystem(dragFile, out file);

	            Tuple<bool, ImageFilter, bool> cropOpts = ConfirmCropResize(dragFile.EditorFile.FilePath, image);

	            if (cropOpts == null)
	            {
		            return;
	            }

	            _content.ConvertImageToBuffer(image, cropOpts.Item1, cropOpts.Item2, cropOpts.Item3);
            }
            catch (Exception ex)
            {
                GorgonDialogs.ErrorBox(ParentForm, ex);
            }
			finally
			{
			    if (image != null)
			    {
			        image.Dispose();
			    }

				ValidateControls();
			    Cursor.Current = Cursors.Default;
			}
        }

        /// <summary>
        /// Function called after a drag/drop operation is finished.
        /// </summary>
        /// <param name="filePath">The path of the file being dragged in from explorer.</param>
        private void OnDrop(string filePath)
        {
            GorgonImageData image = null;

            Cursor.Current = Cursors.WaitCursor;

            try
            {
                Debug.Assert(ParentForm != null, "No parent form!?");
                ParentForm.Focus();
                ParentForm.BringToFront();

                if (_content.ImageType == ImageType.ImageCube)
                {
                    _content.ArrayIndex = _hoverFace + _cubeIndex;
                    UpdateBufferInfoLabels();
                }

				if (!ConfirmBufferOverwrite())
				{
					return;
				}

				Cursor.Current = Cursors.WaitCursor;

                image = GetImageFromDisk(ref filePath);

				Tuple<bool, ImageFilter, bool> cropOpts = ConfirmCropResize(filePath, image);

				if (cropOpts == null)
				{
					return;
				}

                _content.ConvertImageToBuffer(image, cropOpts.Item1, cropOpts.Item2, cropOpts.Item3);
            }
            catch (Exception ex)
            {
                GorgonDialogs.ErrorBox(ParentForm, ex);
            }
            finally
            {
                if (image != null)
                {
                    image.Dispose();
                }

                ValidateControls();
                Cursor.Current = Cursors.Default;
            }
        }

		/// <summary>
		/// Handles the DragDrop event of the panelTextureDisplay control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
		private void panelTextureDisplay_DragDrop(object sender, DragEventArgs e)
		{
		    Cursor.Current = Cursors.WaitCursor;

			try
			{
			    if (e.Data.GetDataPresent(typeof(DragFile)))
			    {
			        BeginInvoke(new Action(() => OnDrop((DragFile)e.Data.GetData(typeof(DragFile)))));
			        return;
			    }

			    if (!e.Data.GetDataPresent(DataFormats.FileDrop, false))
			    {
			        return;
			    }

			    var files = (string[])e.Data.GetData(DataFormats.FileDrop);
			    BeginInvoke(new Action(() => OnDrop(files[0])));
			}
			catch (Exception ex)
			{
			    BeginInvoke(new Action(() => GorgonDialogs.ErrorBox(ParentForm, ex)));
			}
		}

		/// <summary>
		/// Handles the DragOver event of the panelTextureDisplay control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
		private void panelTextureDisplay_DragOver(object sender, DragEventArgs e)
		{
			if (_content == null)
			{
				e.Effect = DragDropEffects.None;
				return;
			}

			if (_content.ImageType != ImageType.ImageCube)
			{
				e.Effect = DragDropEffects.Copy;
				return;
			}

			_hoverFace = CubeFaceHitTest(panelTextureDisplay.PointToClient(new Point(e.X, e.Y)));

			e.Effect = _hoverFace == -1 ? DragDropEffects.None : DragDropEffects.Copy;
		}

		/// <summary>
		/// Handles the DragEnter event of the panelTextureDisplay control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
		private void panelTextureDisplay_DragEnter(object sender, DragEventArgs e)
		{
			try
			{
				_hoverFace = CubeFaceHitTest(panelTextureDisplay.PointToClient(new Point(e.X, e.Y)));

                e.Effect = DragDropEffects.None;
			    
                // Check to see if the file node we're dragging is an image.
			    if (e.Data.GetDataPresent(typeof(DragFile)))
			    {
			        var data = (DragFile)e.Data.GetData(typeof(DragFile));
			        string dataType;

			        if (data.EditorFile == null)
			        {
			            return;
			        }

                    // If this file is not an image, then leave.
			        if ((!data.EditorFile.Attributes.TryGetValue("Type", out dataType))
                        || (!string.Equals(dataType, Resources.GORIMG_CONTENT_TYPE, StringComparison.CurrentCultureIgnoreCase)))
			        {
			            return;
			        }

			        e.Effect = DragDropEffects.Copy;
			        return;
			    }

                // Handle files coming from explorer.
			    if (!e.Data.GetDataPresent(DataFormats.FileDrop, false))
			    {
			        return;
			    }

                Debug.Assert(ParentForm != null, "How don't we have a parent!?");

                ParentForm.BringToFront();

			    var files = (string[])e.Data.GetData(DataFormats.FileDrop);

                // We can only support one file at a time for now.  And the file must be a known image file type.
			    if ((files.Length != 1)
                    || (!GorgonImageEditorPlugIn.Codecs.ContainsKey(new GorgonFileExtension(Path.GetExtension(files[0])))))
			    {
			        return;
			    }
                
                e.Effect = DragDropEffects.Copy;
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
			finally
			{
				ValidateControls();
			}
		}

		/// <summary>
		/// Function to retrieve a list of extensions from the codecs available.
		/// </summary>
		/// <returns>A string containing the list of extensions available for all codecs.</returns>
		private static string GetExtensionsForOpenFileDialog()
		{
			var extensions = GorgonImageEditorPlugIn.Codecs.GroupBy(item => item.Key.Description);
			var result = new StringBuilder();
			var imageDescriptions = new StringBuilder();
			var allImageExtensions = new StringBuilder();
			var imageExtensions = new StringBuilder();

			foreach (var desc in extensions)
			{
				if (imageDescriptions.Length > 0)
				{
					imageDescriptions.Append("|");
				}

				imageDescriptions.Append(desc.Key);

				imageExtensions.Length = 0;
				foreach (var fileExtension in desc)
				{
					if (allImageExtensions.Length > 0)
					{
						allImageExtensions.Append(";");
					}

					allImageExtensions.Append("*.");
					allImageExtensions.Append(fileExtension.Key.Extension);

					if (imageExtensions.Length > 0)
					{
						imageExtensions.Append(";");
					}

					imageExtensions.Append("*.");
					imageExtensions.Append(fileExtension.Key.Extension);
				}

				imageDescriptions.Append("|");
				imageDescriptions.Append(imageExtensions);
			}

			result.Append(Resources.GORIMG_TEXT_ALL_IMAGE_TYPES);
			result.Append("|");
			result.Append(allImageExtensions);
			result.Append("|");
			result.Append(imageDescriptions);

			return result.ToString();
		}

		/// <summary>
		/// Function to retrieve an image from a physical file system.
		/// </summary>
		/// <param name="fileName">The filename of the file to load. If omitted, a file dialog will appear.</param>
		/// <returns>The image data for the file selected, or NULL if cancelled.</returns>
	    private GorgonImageData GetImageFromDisk(ref string fileName)
		{
		    string pathToFile = fileName;

		    if (string.IsNullOrEmpty(pathToFile))
		    {
		        if (string.IsNullOrWhiteSpace(dialogOpenImage.InitialDirectory))
		        {
		            dialogOpenImage.InitialDirectory =
		                !string.IsNullOrWhiteSpace(GorgonImageEditorPlugIn.Settings.LastImageImportDiskPath)
		                    ? GorgonImageEditorPlugIn.Settings.LastImageImportDiskPath
		                    : GorgonComputerInfo.FolderPath(Environment.SpecialFolder.MyPictures);
		        }

		        dialogOpenImage.Filter = GetExtensionsForOpenFileDialog();

		        // Attempt to read the file.
		        if (dialogOpenImage.ShowDialog(ParentForm) == DialogResult.Cancel)
		        {
		            return null;
		        }

                Cursor.Current = Cursors.WaitCursor;
		        pathToFile = dialogOpenImage.FileName;
		    }

            // Load the image data.
		    using(Stream stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
		    {
				GorgonImageData image = _content.ReadFrom(new EditorFile(fileName), stream, true);

				if (!string.IsNullOrEmpty(fileName))
				{
					GorgonImageEditorPlugIn.Settings.LastImageImportDiskPath = Path.GetDirectoryName(pathToFile);
				}

			    return image;
		    }
		}

		/// <summary>
		/// Function to retrieve an image from the currently loaded file system.
		/// </summary>
		/// <param name="dragNode">The node being dragged into the image, or NULL to pop up a file selector.</param>
		/// <param name="editorFile">The file selected from the file selector.</param>
		/// <returns>The image data for the file selected, or NULL if cancelled.</returns>
		private GorgonImageData GetImageFromFileSystem(DragFile dragNode, out EditorFile editorFile)
		{
		    editorFile = dragNode == null ? null : dragNode.EditorFile;
		    Stream fileStream = dragNode == null ? null : dragNode.OpenFile();

            // If we didn't drag a file into this texture, then bring up the file selector.
		    if (dragNode == null)
		    {
		        dialogImportImage.Text = Resources.GORIMG_DLG_CAPTION_IMPORT_IMAGE;

		        if (string.IsNullOrWhiteSpace(dialogImportImage.StartDirectory))
		        {
		            dialogImportImage.StartDirectory =
		                !string.IsNullOrWhiteSpace(GorgonImageEditorPlugIn.Settings.LastImageImportFileSystemPath)
		                    ? GorgonImageEditorPlugIn.Settings.LastImageImportFileSystemPath
		                    : GorgonComputerInfo.FolderPath(Environment.SpecialFolder.MyPictures);
		        }

		        dialogImportImage.FileTypes.Add(_content.ContentType);
		        dialogImportImage.FileView = FileViews.Large;

		        // Attempt to read the file.
		        if ((dialogImportImage.ShowDialog(ParentForm) == DialogResult.Cancel)
		            || ((dialogImportImage.Files[0] == _content.EditorFile)
						&& (_content.MipLevel == 0) && (_content.ArrayIndex == 0) && (_content.DepthSlice == 0)))
		        {
		            return null;
		        }

		        editorFile = dialogImportImage.Files[0];
		        Cursor.Current = Cursors.WaitCursor;
		        fileStream = dialogImportImage.OpenFile();
		    }
			
            // Read the image from the stream.
			using (fileStream)
			{
				GorgonImageData imageData = _content.ReadFrom(editorFile, fileStream, true);

		        if (dragNode == null)
		        {
		            GorgonImageEditorPlugIn.Settings.LastImageImportFileSystemPath =
		                Path.GetDirectoryName(editorFile.FilePath).FormatDirectory('/');
		        }

				return imageData;
			}
		}

		/// <summary>
		/// Handles the Click event of the itemImportFileDisk control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void itemImportFileDisk_Click(object sender, EventArgs e)
		{
			GorgonImageData image = null;

			try
			{
				if (!ConfirmBufferOverwrite())
				{
					return;
				}

				string fileName = null;
				
				image = GetImageFromDisk(ref fileName);

				if (image == null)
				{
					return;
				}

				Cursor.Current = Cursors.WaitCursor;

				Tuple<bool, ImageFilter, bool> cropOpts = ConfirmCropResize(fileName, image);

				if (cropOpts == null)
				{
					return;
				}

                _content.ConvertImageToBuffer(image, cropOpts.Item1, cropOpts.Item2, cropOpts.Item3);
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
			finally
			{
				if (image != null)
				{
					image.Dispose();
				}

				Cursor.Current = Cursors.Default;
				ValidateControls();
			}
		}

		/// <summary>
		/// Handles the Click event of the itemImportFileSystem control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void itemImportFileSystem_Click(object sender, EventArgs e)
		{
			GorgonImageData image = null;

			try
			{
				if (!ConfirmBufferOverwrite())
				{
					return;
				}

				EditorFile file;

				image = GetImageFromFileSystem(null, out file);

				if (image == null)
				{
					return;
				}

				Cursor.Current = Cursors.WaitCursor;

				Tuple<bool, ImageFilter, bool> cropOpts = ConfirmCropResize(file.FilePath, image);

				if (cropOpts == null)
				{
					return;
				}

                _content.ConvertImageToBuffer(image, cropOpts.Item1, cropOpts.Item2, cropOpts.Item3);

			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
			finally
			{
				if (image != null)
				{
					image.Dispose();
				}

				Cursor.Current = Cursors.Default;
				ValidateControls();
			}
		}
		/// <summary>
		/// Handles the Click event of the buttonSave control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonSave_Click(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;
			try
			{
				_content.Commit();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
			finally
			{
				Cursor.Current = Cursors.Default;
				ValidateControls();
			}
		}

		/// <summary>
		/// Handles the Click event of the buttonRevert control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonRevert_Click(object sender, EventArgs e)
		{
			if (GorgonDialogs.ConfirmBox(ParentForm, Resources.GORIMG_DLG_CONFIRM_REVERT) == ConfirmationResult.No)
			{
				return;
			}

			Cursor.Current = Cursors.WaitCursor;

			try
			{
				_content.Revert();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
			finally
			{
				Cursor.Current = Cursors.Default;
				ValidateControls();
			}
		}

		/// <summary>
		/// Handles the Click event of the buttonEditFileExternal control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonEditFileExternal_Click(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			try
			{
				_content.EditExternal();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
			finally
			{
				Cursor.Current = Cursors.Default;
				ValidateControls();
			}
		}

		/// <summary>
		/// Handles the Click event of the buttonPrevDepthSlice control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonPrevDepthSlice_Click(object sender, EventArgs e)
		{
			_content.DepthSlice--;

			UpdateBufferInfoLabels();

			ValidateControls();
		}

		/// <summary>
		/// Handles the Click event of the buttonNextDepthSlice control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonNextDepthSlice_Click(object sender, EventArgs e)
		{
			_content.DepthSlice++;

			UpdateBufferInfoLabels();

			ValidateControls();
		}

		/// <summary>
        /// Handles the Click event of the buttonPrevMipLevel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void buttonPrevMipLevel_Click(object sender, EventArgs e)
        {
			_content.MipLevel--;

			UpdateBufferInfoLabels();
            ValidateControls();
        }

		/// <summary>
		/// Handles the Click event of the buttonNextArrayIndex control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonNextArrayIndex_Click(object sender, EventArgs e)
		{
			_content.ArrayIndex++;

			if (_content.ImageType == ImageType.ImageCube)
			{
				_cubeIndex = ((int)(_content.ArrayIndex / 6.0f).FastFloor()) * 6;
			}

			UpdateBufferInfoLabels();
			ValidateControls();
		}

		/// <summary>
		/// Handles the Click event of the buttonPrevArrayIndex control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonPrevArrayIndex_Click(object sender, EventArgs e)
		{
			_content.ArrayIndex--;

			if (_content.ImageType == ImageType.ImageCube)
			{
				_cubeIndex = ((int)(_content.ArrayIndex / 6.0f).FastFloor()) * 6;
			}

			UpdateBufferInfoLabels();
			ValidateControls();
		}

        /// <summary>
        /// Handles the Click event of the buttonNextMipLevel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void buttonNextMipLevel_Click(object sender, EventArgs e)
        {
			_content.MipLevel++;

			UpdateBufferInfoLabels();
            ValidateControls();
        }

        /// <summary>
        /// Function to validate control state.
        /// </summary>
        private void ValidateControls()
        {
	        buttonSave.Enabled = buttonRevert.Enabled = _content.HasChanges;
	        buttonEditFileExternal.Enabled = !string.IsNullOrWhiteSpace(_content.ExePath);

			buttonPrevMipLevel.Enabled = (_content.MipCount > 1) && (_content.MipLevel > 0);
			buttonNextMipLevel.Enabled = (_content.MipCount > 1) && (_content.MipLevel < _content.MipCount - 1);
            buttonPrevArrayIndex.Enabled = (_content.ArrayCount > 1) && (_content.ArrayIndex > 0);

            buttonNextArrayIndex.Enabled = (_content.ArrayCount > 1) && (_content.ArrayIndex < _content.ArrayCount - 1);

			buttonPrevDepthSlice.Enabled = _content.DepthMipCount > 1 && _content.DepthSlice > 0;
			buttonNextDepthSlice.Enabled = _content.DepthMipCount > 1 && _content.DepthSlice < _content.DepthMipCount - 1;

            // Volume textures don't have array indices.
	        buttonPrevArrayIndex.Visible =
		        buttonNextArrayIndex.Visible =
		        labelArrayIndex.Visible = _content.ImageType != ImageType.Image3D && _content.Codec != null && _content.Codec.SupportsArray;

			buttonPrevMipLevel.Visible = buttonNextMipLevel.Visible = sepMip.Visible = labelMipLevel.Visible = _content.Codec != null && _content.Codec.SupportsMipMaps;

	        buttonPrevDepthSlice.Visible = buttonNextDepthSlice.Visible = labelDepthSlice.Visible = _content.Codec != null && _content.Codec.SupportsDepth && _content.ImageType == ImageType.Image3D;

	        buttonGenerateMips.Enabled = _content.Codec != null && _content.Codec.SupportsMipMaps && _content.MipCount > 1;

	        sepArray.Visible = labelArrayIndex.Visible || labelDepthSlice.Visible;
        }

		/// <summary>
		/// Function to update the image information text.
		/// </summary>
	    private void UpdateImageInfo()
	    {
			buttonEditFileExternal.Text = string.IsNullOrWhiteSpace(_content.ExeName)
											  ? Resources.GORIMG_TEXT_EDIT_EXTERNAL
											  : string.Format(Resources.GORIMG_TEXT_EDIT_EXTERNAL_APPNAME, _content.ExeName);

			switch (_content.ImageType)
			{
				case ImageType.Image1D:
					labelImageInfo.Text = string.Format(Resources.GORIMG_TEXT_IMAGE_INFO_1D,
														_texture.Settings.Width,
														_texture.Settings.Format +
														(_content.BlockCompression != BufferFormat.Unknown ? " (" + _content.BlockCompression + ") " : string.Empty));
					break;
				case ImageType.Image2D:
				case ImageType.ImageCube:
					labelImageInfo.Text = string.Format(Resources.GORIMG_TEXT_IMAGE_INFO_2D,
														_texture.Settings.Width,
														_texture.Settings.Height,
														_texture.Settings.Format +
														(_content.BlockCompression != BufferFormat.Unknown ? " (" + _content.BlockCompression + ") " : string.Empty));
					break;
				case ImageType.Image3D:
					labelImageInfo.Text = string.Format(Resources.GORIMG_TEXT_IMAGE_INFO_3D,
														_texture.Settings.Width,
														_texture.Settings.Height,
														_texture.Settings.Depth,
														_texture.Settings.Format +
														(_content.BlockCompression != BufferFormat.Unknown ? " (" + _content.BlockCompression + ") " : string.Empty));
					break;
			}
	    }

		/// <summary>
		/// Function to create a new texture for display.
		/// </summary>
	    private void CreateTexture()
	    {
			if (_texture != null)
			{
				_texture.Dispose();
			}

			if (_content.MipLevel >= _content.MipCount)
			{
				_content.MipLevel = _content.MipCount - 1;
			}

			if (_content.ArrayIndex >= _content.ArrayCount)
			{
				_content.ArrayIndex = _content.ArrayCount - 1;
			}

			switch (_content.ImageType)
			{
				case ImageType.Image1D:
					_texture = ContentObject.Graphics.Textures.CreateTexture<GorgonTexture1D>("DisplayTexture", _content.Image);
					_currentView = ((GorgonTexture1D)_texture).GetShaderView(_texture.Settings.Format, 0, _content.MipCount, 0, _content.ArrayCount);
					break;
				case ImageType.Image2D:
				case ImageType.ImageCube:
					GorgonTexture2DSettings settings = null;

					if (_content.ImageType == ImageType.ImageCube)
					{
						settings = (GorgonTexture2DSettings)_content.Image.Settings.Clone();
						settings.IsTextureCube = false;
						_cubeIndex = ((int)(_content.ArrayIndex / 6.0f).FastFloor()) * 6;
					}

					_texture = ContentObject.Graphics.Textures.CreateTexture<GorgonTexture2D>("DisplayTexture", _content.Image, settings);
					_currentView = ((GorgonTexture2D)_texture).GetShaderView(_texture.Settings.Format,
					                                                         0,
					                                                         _content.MipCount,
					                                                         0,
					                                                         _content.ArrayCount);
					GetCubeFaceLocations();
					break;
				case ImageType.Image3D:
					_texture = ContentObject.Graphics.Textures.CreateTexture<GorgonTexture3D>("DisplayTexture", _content.Image);
					_currentView = ((GorgonTexture3D)_texture).GetShaderView(_texture.Settings.Format, 0, _content.MipCount);
					break;
			}

			UpdateBufferInfoLabels();

			UpdateImageInfo();
	    }

        /// <summary>
        /// Function to scale the image to the window.
        /// </summary>
        /// <returns>A new rectangle containing the size and location of the scaled image.</returns>
        private RectangleF ScaleImage()
        {
            Vector2 windowSize = panelTextureDisplay.ClientSize;
            var imageSize = new Vector2(_content.Width, _content.Height);
            var location = new Vector2(panelTextureDisplay.ClientSize.Width / 2.0f,
                                       panelTextureDisplay.ClientSize.Height / 2.0f);
            var scale = new Vector2(windowSize.X / imageSize.X, windowSize.Y / imageSize.Y);

            if (scale.Y > scale.X)
            {
                scale.Y = scale.X;
            }
            else
            {
                scale.X = scale.Y;
            }

            Vector2.Modulate(ref imageSize, ref scale, out imageSize);
            location.X = location.X - imageSize.X / 2.0f;
            location.Y = location.Y - imageSize.Y / 2.0f;

            return new RectangleF(location, imageSize);
        }

        /// <summary>
        /// Function to update the labels for the current buffer view.
        /// </summary>
        private void UpdateBufferInfoLabels()
        {
            switch (_content.ImageType)
            {
                case ImageType.Image1D:
					labelArrayIndex.Text = string.Format(Resources.GORIMG_TEXT_ARRAY_INDEX, _content.ArrayIndex + 1, _content.ArrayCount);
                    break;
                case ImageType.Image2D:
                case ImageType.ImageCube:
					labelArrayIndex.Text = string.Format(Resources.GORIMG_TEXT_ARRAY_INDEX, _content.ArrayIndex + 1, _content.ArrayCount);
                    break;
                case ImageType.Image3D:
					labelDepthSlice.Text = string.Format(Resources.GORIMG_TEXT_DEPTH_SLICE, _content.DepthSlice + 1, _content.DepthMipCount);
                    break;
            }

	        labelMipLevel.Text = string.Format(Resources.GORIMG_TEXT_MIP_LEVEL,
											   _content.MipLevel + 1, _content.MipCount,
											   _content.Buffer.Width,
											   _content.Buffer.Height);
        }

		/// <summary>
		/// Function to localize the text of the controls on the form.
		/// </summary>
		protected override void LocalizeControls()
		{
			Text = Resources.GORIMG_DESC;
			labelImageInfo.Text = string.Format(Resources.GORIMG_TEXT_IMAGE_INFO_2D, 0, 0, BufferFormat.Unknown);
		    labelMipLevel.Text = string.Format(Resources.GORIMG_TEXT_MIP_LEVEL, 0, 0, 0, 0);
		    labelArrayIndex.Text = string.Format(Resources.GORIMG_TEXT_ARRAY_INDEX, 0, 0);
			labelDepthSlice.Text = string.Format(Resources.GORIMG_TEXT_DEPTH_SLICE, 0, 0);

			buttonRevert.Text = Resources.GORIMG_TEXT_REVERT;
			buttonSave.Text = Resources.GORIMG_TEXT_SAVE;
			buttonEditFileExternal.Text = string.IsNullOrWhiteSpace(_content.ExeName)
				                              ? Resources.GORIMG_TEXT_EDIT_EXTERNAL
				                              : string.Format(Resources.GORIMG_TEXT_EDIT_EXTERNAL_APPNAME, _content.ExeName);
			buttonNextDepthSlice.Text = Resources.GORIMG_TEXT_NEXT_DEPTH;
			buttonPrevDepthSlice.Text = Resources.GORIMG_TEXT_PREV_DEPTH;
			buttonNextMipLevel.Text = Resources.GORIMG_TEXT_NEXT_MIP;
			buttonPrevMipLevel.Text = Resources.GORIMG_TEXT_PREV_MIP;
			buttonPrevArrayIndex.Text = Resources.GORIMG_TEXT_PREV_ARRAY;
			buttonNextArrayIndex.Text = Resources.GORIMG_TEXT_NEXT_ARRAY;
			buttonImport.Text = Resources.GORIMG_TEXT_IMPORT_IMAGE;
			buttonGenerateMips.Text = Resources.GORIMG_TEXT_GENERATE_MIPS;

			itemImportFileDisk.Text = Resources.GORIMG_TEXT_IMPORT_IMAGE_FROM_DISK;
			itemImportFileSystem.Text = Resources.GORIMG_TEXT_IMPORT_IMAGE_FROM_EDITOR;
		}

		/// <summary>
		/// Function called when a property is changed on the related content.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="value">New value assigned to the property.</param>
	    protected override void OnContentPropertyChanged(string propertyName, object value)
	    {
			switch (propertyName)
			{
				case "Width":
				case "Height":
				case "Depth":
				case "ImageFormat":
				case "ImageType":
				case "MipCount":
				case "ArrayCount":
				case "ImageEditExternal":
				case "Revert":
				case "ImageImport":
				case "MipGenerate":
					CreateTexture();
					break;
				case "Codec":
					UpdateImageInfo();
					break;
			}

			base.OnContentPropertyChanged(propertyName, value);

			ValidateControls();
	    }

	    /// <summary>
		/// Function called when the content has changed.
		/// </summary>
		public override void RefreshContent()
		{
			base.RefreshContent();

			_content = Content as GorgonImageContent;

			if (_content == null)
			{
				throw new InvalidCastException(string.Format(Resources.GORIMG_CONTENT_NOT_IMAGE, Content.Name));
			}

		    if ((_content.Image != null) && (_texture == null))
		    {
				CreateTexture();
		    }

			ValidateControls();
		}

		/// <summary>
		/// Function to scroll the background in the selector.
		/// </summary>
	    private void AnimateSelector()
	    {
			_selectScroll.X += (GorgonTiming.Delta * 16.0f) / _pattern.Settings.Width;
			_selectScroll.Y += (GorgonTiming.Delta * 16.0f) / _pattern.Settings.Height;

			if (_selectScroll.X > 1.0f)
			{
				_selectScroll.X = 0.0f;
			}

			if (_selectScroll.Y > 1.0f)
			{
				_selectScroll.Y = 0.0f;
			}
	    }

		/// <summary>
		/// Function to draw the background pattern for the image.
		/// </summary>
	    private void DrawBackground()
	    {
			_content.Renderer.Drawing.BlendingMode = BlendingMode.Modulate;
			_content.Renderer.Drawing.TextureSampler.HorizontalWrapping = TextureAddressing.Wrap;
			_content.Renderer.Drawing.TextureSampler.VerticalWrapping = TextureAddressing.Wrap;

			_content.Renderer.Drawing.Blit(_backgroundTexture,
										   panelTextureDisplay.ClientRectangle,
										   _backgroundTexture.ToTexel(panelTextureDisplay.ClientRectangle));

			_content.Renderer.Drawing.TextureSampler.HorizontalWrapping = TextureAddressing.Clamp;
			_content.Renderer.Drawing.TextureSampler.VerticalWrapping = TextureAddressing.Clamp;
	    }

		/// <summary>
		/// Draws the unwrapped cube.
		/// </summary>
	    public void DrawUnwrappedCube()
	    {
			DrawBackground();

			if (_texture == null)
			{
				return;
			}

			var texture2D = _texture as GorgonTexture2D;

			AnimateSelector();

			for (int i = 0; i < _cubeFaceLocations.Length; ++i)
			{
				_content.Renderer.PixelShader.Current = _content.PixelShader;
				_content.Renderer.PixelShader.Resources[0] = _currentView;

				_content.SetCubeArrayIndex(_cubeIndex + i);
				_content.Renderer.Drawing.Blit(texture2D, _cubeFaceLocations[i]);

				// Reset our pixel shader.
				_content.Renderer.PixelShader.Current = null;

				// Hilight the selected cube face.
				if ((i + _cubeIndex) == _content.ArrayIndex)
				{
					_selectorSprite.Position = _cubeFaceLocations[i].Location;
					_selectorSprite.Size = _cubeFaceLocations[i].Size;
					_selectorSprite.Color = new GorgonColor(0.25f, 0.25f, 1.0f, 0.4f);
					_selectorSprite.TextureRegion = new RectangleF(_selectScroll.X,
																  _selectScroll.Y,
																  _cubeFaceLocations[i].Width / _pattern.Settings.Width,
																  _cubeFaceLocations[i].Height / _pattern.Settings.Height);
					_selectorSprite.Draw();
				}

				if (_hoverFace == i)
				{
					_selectorSprite.Position = _cubeFaceLocations[i].Location;
					_selectorSprite.Size = _cubeFaceLocations[i].Size;
					_selectorSprite.Color = new GorgonColor(1, 0, 0, 0.4f);
					_selectorSprite.TextureRegion = new RectangleF(_selectScroll.X,
																  _selectScroll.Y,
																  _cubeFaceLocations[i].Width / _pattern.Settings.Width,
																  _cubeFaceLocations[i].Height / _pattern.Settings.Height);
					_selectorSprite.Draw();
				}

				// Draw a border around each.
				_content.Renderer.Drawing.FilledRectangle(new RectangleF(_cubeFaceLocations[i].Location, new SizeF(_cubeFaceLocations[i].Width, _text.Size.Y + 4)), Color.FromArgb(128, Color.Black));
				_content.Renderer.Drawing.DrawRectangle(_cubeFaceLocations[i], Color.Black);

				_text.Text = string.Format(Resources.GORIMG_TEXT_FACE, i + 1, _cubeFaceNames[i]);
				_text.Position = new Vector2((int)(_cubeFaceLocations[i].X + (_cubeFaceLocations[i].Width / 2.0f) - _text.Size.X / 2.0f), _cubeFaceLocations[i].Y + 2);
				_text.Draw();
			}
	    }

		/// <summary>
		/// Function to draw the texture.
		/// </summary>
	    public void Draw()
	    {
			DrawBackground();

			if (_texture == null)
			{
				return;
			}

		    _content.Renderer.PixelShader.Current = _content.PixelShader;

			// Send some data to render to the GPU.  We'll set the texture -after- this so that we can trick the 2D renderer into using
			// a 1D/3D texture.  This works because the blit does not occur until after we have a state change or we force rendering with "Render".
			var texture2D = _texture as GorgonTexture2D;
            
			_content.Renderer.Drawing.Blit(texture2D ?? _backgroundTexture, ScaleImage());

		    _content.Renderer.PixelShader.Resources[0] = _currentView;

            // This triggers a render and resets our pixel shader back to the current.
			_content.Renderer.PixelShader.Current = null;
	    }

		/// <summary>
		/// Function to create the resources used by the content.
		/// </summary>
	    public void CreateResources()
		{
			_backgroundTexture = ContentObject.Graphics.Textures.CreateTexture<GorgonTexture2D>("BackgroundTexture", Resources.Pattern);
			_font = ContentObject.Graphics.Fonts.CreateFont("ImageEditorFont", new GorgonFontSettings
			                                                                   {
				                                                                   FontFamilyName = Font.FontFamily.Name,
																				   FontHeightMode = FontHeightMode.Points,
																				   Size = Font.Size,
																				   AntiAliasingMode = FontAntiAliasMode.AntiAlias,
																				   FontStyle = FontStyle.Bold,
																				   OutlineSize = 2,
																				   OutlineColor1 = Color.Black
			                                                                   });
			_text = _content.Renderer.Renderables.CreateText("ImageEditorCubeLabel", _font, string.Empty, Color.White);
			_pattern = ContentObject.Graphics.Textures.CreateTexture<GorgonTexture2D>("Background.Pattern", Resources.Pattern);
			_selectorSprite = _content.Renderer.Renderables.CreateSprite("Pattern", new GorgonSpriteSettings
			{
				Color = new GorgonColor(1, 0, 0, 0.4f),
				Texture = _pattern,
				TextureRegion = new RectangleF(0, 0, 1, 1),
				Size = _pattern.Settings.Size
			});
			_selectorSprite.TextureSampler.HorizontalWrapping = TextureAddressing.Wrap;
			_selectorSprite.TextureSampler.VerticalWrapping = TextureAddressing.Wrap;
		}
		#endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonImageContentPanel"/> class.
        /// </summary>
        public GorgonImageContentPanel()
        {
            InitializeComponent();

			panelTextureDisplay.MouseWheel += panelTextureDisplay_MouseWheel;
        }
	    #endregion
	}
}
