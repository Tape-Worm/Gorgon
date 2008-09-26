#region MIT.
// 
// Atlas.
// Copyright (C) 2007 Michael Winsor
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
// Created: Tuesday, November 13, 2007 1:11:05 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Dialogs;

namespace Atlas
{
	/// <summary>
	/// Interface for the atlas creation utility.
	/// </summary>
	public partial class formMain 
		: Form
	{
		#region Classes.
		/// <summary>
		/// Image data value type.
		/// </summary>
		private class ImageData
		{
			/// <summary>
			/// Source image.
			/// </summary>
			public string SourceName;
			/// <summary>
			/// Destination image.
			/// </summary>
			public string Destination;
			/// <summary>
			/// Scaled rectangle.
			/// </summary>
			public RectangleF ScaledRect;
			/// <summary>
			/// Atlas image.
			/// </summary>
			public Image AtlasImage;
		}
		#endregion

		#region Variables.
		private SortedList<string, Image> _images = null;			// List of images.
		private List<string> _imageNames = null;					// Image names.
		private Size _atlasSize = new Size(4096, 4096);				// Atlas size.
		private int _paddingPixels = 0;								// Number of pixels to pad around each image.
		private List<ImageData> _imageData = null;					// Image data.
		private List<Image> _atlasImages = null;					// Atlas images.
		private int _currentIndex = -1;								// Current image.
		#endregion

		#region Properties.

		#endregion

		#region Methods.
		/// <summary>
		/// Handles the Click event of the labelNextAtlas control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void labelNextAtlas_Click(object sender, EventArgs e)
		{
			if (_atlasImages != null)
			{
				_currentIndex++;
				if (_currentIndex > _atlasImages.Count - 1)
					_currentIndex = _atlasImages.Count - 1;
			}

			ValidateControls();
		}

		/// <summary>
		/// Handles the Click event of the labelPreviousAtlas control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void labelPreviousAtlas_Click(object sender, EventArgs e)
		{
			_currentIndex--;
			if (_currentIndex < 0)
				_currentIndex = 0;
			ValidateControls();
		}

		/// <summary>
		/// Handles the MouseEnter event of the labelNextAtlas control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void labelNextAtlas_MouseEnter(object sender, EventArgs e)
		{
			labelNextAtlas.BackColor = Color.Cyan;
		}

		/// <summary>
		/// Handles the MouseLeave event of the labelNextAtlas control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void labelNextAtlas_MouseLeave(object sender, EventArgs e)
		{
			labelNextAtlas.BackColor = Color.FromKnownColor(KnownColor.Control);
		}

		/// <summary>
		/// Handles the MouseEnter event of the labelPreviousAtlas control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void labelPreviousAtlas_MouseEnter(object sender, EventArgs e)
		{
			labelPreviousAtlas.BackColor = Color.Cyan;
		}

		/// <summary>
		/// Handles the MouseLeave event of the labelPreviousAtlas control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void labelPreviousAtlas_MouseLeave(object sender, EventArgs e)
		{
			labelPreviousAtlas.BackColor = Color.FromKnownColor(KnownColor.Control);
		}

		/// <summary>
		/// Handles the Click event of the buttonClear control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonClear_Click(object sender, EventArgs e)
		{
			pictureAtlas.Image = null;

			if (_atlasImages != null)
			{
				foreach (Image image in _atlasImages)
					image.Dispose();

				_atlasImages.Clear();
			}
			
			if (_imageData != null)
			{
				_imageData.Clear();
				_imageData = null;
			}

			foreach (KeyValuePair<string, Image> image in _images)
				image.Value.Dispose();

			_images.Clear();
			buttonSaveAtlas.Enabled = false;
			ValidateControls();
			labelNextAtlas.Enabled = false;
			labelPreviousAtlas.Enabled = false;
			labelCurrentAtlas.Text = "0/0";
			_currentIndex = -1;
		}

		/// <summary>
		/// Handles the Click event of the buttonAtlasSettings control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonAtlasSettings_Click(object sender, EventArgs e)
		{
			formAtlasSettings settings = null;			// Atlas settings interface.

			try
			{
				Settings.Root = "AtlasSettings";
				_atlasSize.Width = Convert.ToInt32(Settings.GetSetting("MaxWidth", "4096"));
				_atlasSize.Height = Convert.ToInt32(Settings.GetSetting("MaxHeight", "4096"));
				_paddingPixels = Convert.ToInt32(Settings.GetSetting("Padding", "0"));

				settings = new formAtlasSettings();
				settings.AtlasWidth = _atlasSize.Width;
				settings.AtlasHeight = _atlasSize.Height;
				settings.PixelPadding = _paddingPixels;

				if (settings.ShowDialog(this) == DialogResult.OK)
				{
					_atlasSize.Width = settings.AtlasWidth;
					_atlasSize.Height = settings.AtlasHeight;

					Settings.SetSetting("MaxWidth", settings.AtlasWidth.ToString());
					Settings.SetSetting("MaxHeight", settings.AtlasHeight.ToString());
					Settings.SetSetting("Padding", settings.PixelPadding.ToString());

					// Create new atlas.
					RefreshAtlas();
				}
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, "Error trying to set the atlas settings.", ex);
			}
			finally
			{
				if (settings != null)
					settings.Dispose();
				settings = null;
				Settings.Root = null;
			}
		}

		/// <summary>
		/// Handles the Click event of the buttonSaveAtlas control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonSaveAtlas_Click(object sender, EventArgs e)
		{
			StreamWriter writer = null;		// Stream writer.

			try
			{
				Settings.Root = "Paths";
				dialogSave.Title = "Save atlas...";
				dialogSave.Filter = "Portable Network Graphics (*.png)|*.png";
				dialogSave.InitialDirectory = Settings.GetSetting("SaveAtlasPath", @".\");
				if (dialogSave.ShowDialog(this) == DialogResult.OK)
				{
					writer = new StreamWriter(Path.ChangeExtension(dialogSave.FileName, ".TAI"));

					for (int i = 0; i < _atlasImages.Count; i++)
					{
						_atlasImages[i].Save(Path.GetDirectoryName(dialogSave.FileName) + Path.DirectorySeparatorChar + i.ToString() + "_" + Path.GetFileName(dialogSave.FileName));
						foreach (ImageData item in _imageData)
						{
							if (item.AtlasImage == _atlasImages[i])
								item.Destination = i.ToString() + "_" + Path.GetFileName(dialogSave.FileName);
						}
					}

					// Write out index data.
					foreach (ImageData item in _imageData)
					{
						string line = string.Empty;

						line = item.SourceName + "\t\t" + item.Destination + ", 0, 2D, " + item.ScaledRect.X.ToString("0.000000") + ", " +
									item.ScaledRect.Y.ToString("0.000000") + ", 0.000000, " + item.ScaledRect.Width.ToString("0.000000") + ", " + item.ScaledRect.Height.ToString("0.000000");

						writer.WriteLine(line);
					}

					Settings.SetSetting("SaveAtlasPath", Path.GetDirectoryName(dialogSave.FileName));
				}
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, "Unable to save the atlas.", ex);
			}
			finally
			{
				if (writer != null)
					writer.Dispose();
				writer = null;
				Settings.Root = null;
			}
		}

		/// <summary>
		/// Function to refresh the atlas image list.
		/// </summary>
		private void RefreshAtlas()
		{
			pictureAtlas.Image = null;
			if (_atlasImages != null)
			{
				foreach (Image image in _atlasImages)
					image.Dispose();				
			}
				
			_imageNames = new List<string>();
			_atlasImages = new List<Image>();

			// Add image names.
			foreach (KeyValuePair<string, Image> image in _images)
				_imageNames.Add(image.Key);

			_imageData = new List<ImageData>();
		
			GenerateAtlas(_imageNames);

			while (_imageNames.Count > 0)
				GenerateAtlas(_imageNames);

			ValidateControls();
		}

		/// <summary>
		/// Function to generate the atlas.
		/// </summary>
		/// <param name="images">List of images to use as a source for an atlas image.</param>
		private void GenerateAtlas(List<string> sourceImages)
		{
			ImageTree tree = null;			// Image tree.
			Rectangle newPosition;			// New rectangle.
			Graphics g = null;				// Graphics context.
			ImageData data;					// Image data.
			Image atlas = null;				// Current atlas image.
			List<string> images = null;		// List of images.

			Cursor.Current = Cursors.WaitCursor;
			try
			{
				buttonSaveAtlas.Enabled = false;
				if (_images.Count < 1)
					return;

				Settings.Root = "AtlasSettings";
				_atlasSize.Width = Convert.ToInt32(Settings.GetSetting("MaxWidth", "4096"));
				_atlasSize.Height = Convert.ToInt32(Settings.GetSetting("MaxHeight", "4096"));
				_paddingPixels = Convert.ToInt32(Settings.GetSetting("Padding", "0"));

				// Reset.
				atlas = new Bitmap(_atlasSize.Width, _atlasSize.Height, PixelFormat.Format32bppArgb);

				// Begin adding to the image tree.
				tree = new ImageTree(_atlasSize);

				images = new List<string>();
				foreach (string imageName in sourceImages)
					images.Add(imageName);

				// Add to tree.
				g = Graphics.FromImage(atlas);
				g.Clear(Color.FromArgb(255, 0, 255));
				
				g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
				g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
				g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;				

				while(images.Count > 0)
				{
					try
					{
						Image image = _images[images[0]];		// Get image.

						newPosition = tree.Add(images[0], new Size(image.Width + _paddingPixels, image.Height + _paddingPixels));
						if (newPosition != Rectangle.Empty)
						{
							g.DrawImage(image, newPosition.Location.X, newPosition.Location.Y, image.Width, image.Height);

							// Convert to data structure.
							data = new ImageData();
							data.Destination = string.Empty;
							data.SourceName = images[0];
							data.AtlasImage = atlas;
							data.ScaledRect = new RectangleF((float)newPosition.X / (float)atlas.Width, (float)newPosition.Y / (float)atlas.Height,
										(float)(newPosition.Width - _paddingPixels) / (float)atlas.Width, (float)(newPosition.Height - _paddingPixels) / (float)atlas.Height);

							_imageData.Add(data);
							_imageNames.Remove(images[0]);
							if (!_atlasImages.Contains(atlas))
								_atlasImages.Add(atlas);
						}
					}
					catch (OverflowException)
					{
						UI.ErrorBox(this, "The image '" + images[0] + "' is too large to fit into the constraints of the image.\nImage will be removed from the list.");
						_imageNames.Remove(images[0]);
					}

					images.RemoveAt(0);
				}

				if (_currentIndex == -1)
					_currentIndex = 0;
				buttonSaveAtlas.Enabled = true;
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, "There was an error while trying to generate the atlas.", ex);
			}
			finally
			{
				this.Refresh();
				if (g != null)
					g.Dispose();
				g = null;
				Cursor.Current = Cursors.Default;
				Settings.Root = null;
			}
		}

		/// <summary>
		/// Handles the Click event of the buttonLoadImage control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonLoadImage_Click(object sender, EventArgs e)
		{
			try
			{
				pictureAtlas.Image = null;
				Settings.Root = "Paths";				
				dialogOpen.Title = "Open image files...";
				dialogOpen.Filter = "Portable Network Graphics (*.png)|*.png|Windows bitmap (*.bmp)|*.bmp|Joint Photographic Experts Group (*.jpg, *.jpeg)|*.jpg;*.jpeg";
				dialogOpen.InitialDirectory = Settings.GetSetting("SourceImagePath", @".\");
				if (dialogOpen.ShowDialog(this) == DialogResult.OK)
				{
					// Load each image.
					foreach (string file in dialogOpen.FileNames)
					{
						if (!_images.ContainsKey(file))
							_images.Add(file, Image.FromFile(file));
					}

					Settings.SetSetting("SourceImagePath", Path.GetDirectoryName(dialogOpen.FileNames[0]));

					// Generate the atlas.
					RefreshAtlas();
				}
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, "Unable to load the selected images.", ex);
			}
			finally
			{
				Settings.Root = null;
			}
		}

		/// <summary>
		/// Function to validate the controls for the form.
		/// </summary>
		private void ValidateControls()
		{
			pictureAtlas.Image = null;
			if ((_imageData != null) && (_imageData.Count > 0) && (_atlasImages != null) && (_atlasImages.Count > 0))
			{
				if (_currentIndex >= _atlasImages.Count)
					_currentIndex = _atlasImages.Count - 1;

				if (_currentIndex < 0)
					pictureAtlas.Image = null;
				else
					pictureAtlas.Image = _atlasImages[_currentIndex];

				labelCurrentAtlas.Text = string.Format("{0}", _currentIndex + 1) + "/" + _atlasImages.Count.ToString();

				labelNextAtlas.Enabled = false;
				labelPreviousAtlas.Enabled = false;
				if (_atlasImages.Count > 1)
				{
					if (_currentIndex > 0)
						labelPreviousAtlas.Enabled = true;
					else
						labelPreviousAtlas.BackColor = Color.FromKnownColor(KnownColor.Control);

					if (_currentIndex < _atlasImages.Count - 1)
						labelNextAtlas.Enabled = true;
					else
						labelNextAtlas.BackColor = Color.FromKnownColor(KnownColor.Control);
				}
				else
				{
					labelNextAtlas.Enabled = false;
					labelPreviousAtlas.Enabled = false;
				}
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load"></see> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			// Attempt to load the configuration file.
			Settings.Load("config.xml");
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Closing"></see> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.ComponentModel.CancelEventArgs"></see> that contains the event data.</param>
		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);

			buttonClear_Click(this, EventArgs.Empty);

			Settings.Save("config.xml");
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="formMain"/> class.
		/// </summary>
		public formMain()
		{
			InitializeComponent();
			_images = new SortedList<string, Image>();
		}
		#endregion
	}
}