#region MIT.
// 
// Gorgon.
// Copyright (C) 2014 Michael Winsor
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
// Created: Tuesday, March 11, 2014 9:11:06 PM
// 
#endregion

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Gorgon.Editor.FontEditorPlugIn.Properties;
using Gorgon.Graphics;
using Gorgon.IO;
using Gorgon.Renderers;
using Gorgon.UI;
using SlimMath;

namespace Gorgon.Editor.FontEditorPlugIn.Controls
{
	/// <summary>
	/// An interface for managing the application of a texture to font glyphs.
	/// </summary>
	public partial class PanelTexture 
		: UserControl
	{
		#region Events.
		/// <summary>
		/// Event fired when the brush has been changed.
		/// </summary>
		public event EventHandler BrushChanged;
		#endregion

		#region Variables.
		private readonly StringBuilder _infoText = new StringBuilder(256);	// Text for the info label.
		private WrapMode _wrapMode = WrapMode.Tile;							// Current wrapping mode.
		private RectangleF _region = RectangleF.Empty;						// The selected texture region.
		private GorgonTexture2D _texture;									// Selected texture.
		private GorgonTexture2D _defaultTexture;							// Default texture.
		private GorgonSwapChain _swapChain;									// The swap chain to use for displaying images.
		private Gorgon2DStateRecall _lastState;								// Last 2D state.
		private Gorgon2D _renderer;											// Renderer.
		private GorgonGraphics _graphics;									// Graphics interface.
		private Clipper _clipper;											// Clipper interface.
		private RectangleF _displayRegion;									// The display region for the texture.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the image editor.
		/// </summary>
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public IImageEditorPlugIn ImageEditor
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the path to an existing texture brush.
		/// </summary>
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public EditorFile TextureBrushFile
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the currently active texture.
		/// </summary>
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public GorgonTexture2D Texture
		{
			get
			{
				return _texture;
			}
			set
			{
				if (_graphics == null)
				{
					return;
				}

				if (value != null)
				{
					_texture = _graphics.Textures.CreateTexture(value.Name, value.Settings);
					_texture.Copy(value);
				}
				else
				{
					if (_texture != null)
					{
						_texture.Dispose();
						_texture = null;
					}

					_texture = null;
				}
			}
		}

		/// <summary>
		/// Property to set or return the current wrapping mode.
		/// </summary>
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public WrapMode WrapMode
		{
			get
			{
				if (IsHandleCreated)
				{
					return (WrapModeComboItem)comboWrapMode.SelectedItem;
				}

				return _wrapMode;
			}
			set
			{
				_wrapMode = value;

				if (IsHandleCreated)
				{
					comboWrapMode.SelectedIndex = comboWrapMode.Items.IndexOf(new WrapModeComboItem(value));
				}
			}
		}

		/// <summary>
		/// Property to set or return the region for the selected texture.
		/// </summary>
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public RectangleF TextureRegion
		{
			get
			{
				if ((IsHandleCreated)
					&& (Texture != null))
				{
					return new RectangleF(Texture.ToTexel((float)numericX.Value, (float)numericY.Value),
					                      Texture.ToTexel((float)numericWidth.Value, (float)numericHeight.Value));
				}

				return _region;
			}
			set
			{
				if (_clipper == null)
				{
					return;
				}

				_region = Texture == null ? new RectangleF(0, 0, 1, 1) : Texture.ToPixel(value);

				if (!IsHandleCreated)
				{
					return;
				}

				numericX.Value = (decimal)_region.X;
				numericY.Value = (decimal)_region.Y;
				numericWidth.Value = (decimal)_region.Width;
				numericHeight.Value = (decimal)_region.Height;

				InitializeClipper(_texture, new Rectangle((int)_region.X, (int)_region.Y, (int)_region.Width, (int)_region.Height));

				UpdateLabelInfo();
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function called when the brush is changed.
		/// </summary>
		private void OnBrushChanged()
		{
			if (BrushChanged == null)
			{
				return;
			}

			BrushChanged(this, EventArgs.Empty);
		}

		/// <summary>
		/// Function to disable the limits on the numeric fields.
		/// </summary>
		private void DisableNumericLimits()
		{
			numericWidth.Maximum = Int32.MaxValue;
			numericHeight.Maximum = Int32.MaxValue;
			numericX.Maximum = Int32.MaxValue;
			numericY.Maximum = Int32.MaxValue;
		}

		/// <summary>
		/// Function to enable the limits on the numeric fields.
		/// </summary>
		private void EnableNumericLimits()
		{
			if ((_texture == null)
				|| (_clipper == null))
			{
				return;
			}

			RectangleF selected = _clipper.ClipRegion;

			numericWidth.Maximum = _texture.Settings.Width - (int)selected.X;
			numericHeight.Maximum = _texture.Settings.Height - (int)selected.Y;
			numericX.Maximum = _texture.Settings.Width - (int)selected.Width;
			numericY.Maximum = _texture.Settings.Height - (int)selected.Height;
		}

		/// <summary>
		/// Handles the MouseDown event of the panelTextureDisplay control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void panelTextureDisplay_MouseDown(object sender, MouseEventArgs e)
		{
			try
			{
				if (_clipper == null)
				{
					return;
				}

				if (_clipper.OnMouseDown(e))
				{
					DisableNumericLimits();
				}
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
			finally
			{
				ValidateCommands();
				UpdateLabelInfo();
			}
		}

		/// <summary>
		/// Function to update the information label.
		/// </summary>
		private void UpdateLabelInfo()
		{
			_infoText.Length = 0;

			if ((_clipper == null)
                || (Texture == null))
			{
				labelInfo.Text = string.Empty;
				return;
			}

			RectangleF selected = _clipper.ClipRegion;

            _infoText.AppendFormat("{0}: {1:####0}x{2:####0} - {3:####0}x{4:####0}\n{5}: {6:####0}\n{7}: {8:####0}", Resources.GORFNT_TEXT_REGION,
								   selected.X,
								   selected.Y,
								   selected.Right,
								   selected.Bottom,
                                   Resources.GORFNT_TEXT_WIDTH,
								   selected.Width,
                                   Resources.GORFNT_TEXT_HEIGHT,
								   selected.Height);

			labelInfo.Text = _infoText.ToString();
		}

		/// <summary>
		/// Handles the MouseMove event of the panelTextureDisplay control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void panelTextureDisplay_MouseMove(object sender, MouseEventArgs e)
		{
			if (_clipper == null)
			{
				return;
			}

			if (!_clipper.OnMouseMove(e))
			{
				return;
			}

			Rectangle numericValues = Rectangle.Intersect(Rectangle.Round(_clipper.ClipRegion),
			                                              new Rectangle(0, 0, _texture.Settings.Width, _texture.Settings.Height));

			numericX.Value = numericValues.X;
			numericY.Value = numericValues.Y;
			numericWidth.Value = numericValues.Width;
			numericHeight.Value = numericValues.Height;

			UpdateLabelInfo();

			OnBrushChanged();
		}

		/// <summary>
		/// Handles the MouseUp event of the panelTextureDisplay control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void panelTextureDisplay_MouseUp(object sender, MouseEventArgs e)
		{
			if (_clipper == null)
			{
				return;
			}

			try
			{
				if (_clipper.OnMouseUp(e))
				{
					EnableNumericLimits();
				}
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
			finally
			{
				UpdateLabelInfo();
				ValidateCommands();
			}
		}

		/// <summary>
		/// Handles the SelectedIndexChanged event of the comboWrapMode control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void comboWrapMode_SelectedIndexChanged(object sender, EventArgs e)
		{
			ValidateCommands();
			OnBrushChanged();
		}

		/// <summary>
		/// Handles the ValueChanged event of the numericX control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void numericX_ValueChanged(object sender, EventArgs e)
		{
			try
			{
				if ((_clipper == null)
					|| (_clipper.DragMode != ClipSelectionDragMode.None))
				{
					return;
				}

				_clipper.ClipRegion = new Rectangle((int)numericX.Value,
				                                    (int)numericY.Value,
				                                    (int)numericWidth.Value,
				                                    (int)numericHeight.Value);
				EnableNumericLimits();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
			finally
			{
				ValidateCommands();
				UpdateLabelInfo();
				OnBrushChanged();
			}
		}

		/// <summary>
		/// Function to perform localization for controls.
		/// </summary>
		private void PerformLocalization()
		{
			comboWrapMode.Items.Clear();
			comboWrapMode.Items.Add(new WrapModeComboItem(WrapMode.Tile, Resources.GORFNT_TEXT_TILE));
			comboWrapMode.Items.Add(new WrapModeComboItem(WrapMode.Clamp, Resources.GORFNT_TEXT_CLAMP));
			comboWrapMode.Items.Add(new WrapModeComboItem(WrapMode.TileFlipX, Resources.GORFNT_TEXT_TILE_FILP_HORZ));
			comboWrapMode.Items.Add(new WrapModeComboItem(WrapMode.TileFlipY, Resources.GORFNT_TEXT_TILE_FLIP_VERT));
			comboWrapMode.Items.Add(new WrapModeComboItem(WrapMode.TileFlipXY, Resources.GORFNT_TEXT_TILE_FLIP));

			labelTexHeight.Text = string.Format("{0}:", Resources.GORFNT_TEXT_HEIGHT);
			labelTexWidth.Text = string.Format("{0}:", Resources.GORFNT_TEXT_WIDTH);
			labelTexLeft.Text = string.Format("{0}:", Resources.GORFNT_TEXT_LEFT);
			labelTexTop.Text = string.Format("{0}:", Resources.GORFNT_TEXT_TOP);
			labelWrapMode.Text = string.Format("{0}:", Resources.GORFNT_TEXT_WRAPPING_MODE);
			buttonOpen.Text = Resources.GORFNT_TEXT_OPEN_TEXTURE;
		}

		/// <summary>
		/// Function to perform validation for the controls on the panel.
		/// </summary>
		private void ValidateCommands()
		{
		    buttonOpen.Enabled = ImageEditor != null;
			numericHeight.Enabled = numericWidth.Enabled = numericX.Enabled = numericY.Enabled = comboWrapMode.Enabled = Texture != null;
		}

		/// <summary>
		/// Function to initialize the clipper.
		/// </summary>
		/// <param name="texture">Texture to use with the clipper.</param>
		/// <param name="selectedRegion">The currently selected region.</param>
		private void InitializeClipper(GorgonTexture2D texture, RectangleF selectedRegion)
		{
			var offset = new Vector2(panelTextureDisplay.ClientSize.Width / 2.0f, panelTextureDisplay.ClientSize.Height / 2.0f);
			var scale = new Vector2(1);

			if (texture != null)
			{
				scale.X = panelTextureDisplay.ClientSize.Width / (float)texture.Settings.Width;
				scale.Y = panelTextureDisplay.ClientSize.Height / (float)texture.Settings.Height ;

				if (texture.Settings.Width > texture.Settings.Height)
				{
					scale.Y *= (float)texture.Settings.Height / texture.Settings.Width;
				}
				else
				{
					scale.X *= (float)texture.Settings.Width / texture.Settings.Height;
				}

				var textureSize = new Vector2(texture.Settings.Width * scale.X, texture.Settings.Height * scale.Y);

				offset.X = offset.X - (textureSize.X * 0.5f);
				offset.Y = offset.Y - (textureSize.Y * 0.5f);

				_displayRegion = new RectangleF(offset, textureSize);
			}

			if (_clipper == null)
			{
				_clipper = new Clipper(_renderer, panelTextureDisplay)
				           {
					           SelectorColor = new GorgonColor(0, 0, 0.8f, 0.4f),
					           SelectorPattern = _defaultTexture
				           };
			}

			_clipper.TextureSize = texture == null ? new Size(1, 1) : texture.Settings.Size;
			_clipper.ClipRegion = selectedRegion;
			_clipper.Offset = offset;
			_clipper.Scale = scale;
		}

		/// <summary>
		/// Handles the Click event of the buttonOpen control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonOpen_Click(object sender, EventArgs e)
		{
			DisableNumericLimits();

			numericWidth.ValueChanged -= numericX_ValueChanged;
			numericHeight.ValueChanged -= numericX_ValueChanged;
			numericX.ValueChanged -= numericX_ValueChanged;
			numericY.ValueChanged -= numericX_ValueChanged;

			try
			{
				if (!string.IsNullOrWhiteSpace(GorgonFontEditorPlugIn.Settings.LastTextureImportPath))
				{
					imageFileBrowser.StartDirectory = GorgonFontEditorPlugIn.Settings.LastTextureImportPath;
				}

				imageFileBrowser.FileTypes.Clear();
				imageFileBrowser.FileTypes.Add(ImageEditor.ContentType);

				imageFileBrowser.FileView = GorgonFontEditorPlugIn.Settings.LastTextureImportDialogView;

				if (imageFileBrowser.ShowDialog(ParentForm) == DialogResult.OK)
				{
					// Don't load the same image.
					if ((TextureBrushFile != null) &&
						(string.Equals(TextureBrushFile.FilePath, imageFileBrowser.Files[0].FilePath)))
					{
						return;
					}

					Cursor.Current = Cursors.WaitCursor;

					// Load the image.
					using (Stream stream = imageFileBrowser.OpenFile())
					{
						using (IImageEditorContent imageContent = ImageEditor.ImportContent(imageFileBrowser.Files[0], stream))
						{
							if (imageContent.Image.Settings.ImageType != ImageType.Image2D)
							{
								GorgonDialogs.ErrorBox(ParentForm, string.Format(Resources.GORFNT_ERR_IMAGE_NOT_2D, imageContent.Name));
								return;
							}

							if ((imageContent.Image.Settings.Format != BufferFormat.R8G8B8A8_UIntNormal_sRGB)
							    && (imageContent.Image.Settings.Format != BufferFormat.R8G8B8A8_UIntNormal)
							    && (imageContent.Image.Settings.Format != BufferFormat.B8G8R8A8_UIntNormal)
							    && (imageContent.Image.Settings.Format != BufferFormat.B8G8R8A8_UIntNormal_sRGB))
							{
								GorgonDialogs.ErrorBox(ParentForm,
								                       string.Format(Resources.GORFNT_BRUSH_IMAGE_WRONG_FORMAT, imageFileBrowser.Files[0].Filename, imageContent.Image.Settings.Format));
								return;
							}

							// If we've loaded a texture previously and haven't committed it yet, then get rid of it.
							if (_texture != null)
							{
								_texture.Dispose();
							}

							var settings = (GorgonTexture2DSettings)imageContent.Image.Settings.Clone();
							_texture = _graphics.Textures.CreateTexture<GorgonTexture2D>(imageContent.Name,
																						 imageContent.Image,
																						 settings);
							// Reset the texture brush settings.
							numericWidth.Value = settings.Width;
							numericHeight.Value = settings.Height;
							numericX.Value = 0;
							numericY.Value = 0;
							comboWrapMode.Text = Resources.GORFNT_TEXT_TILE;

							// Function to retrieve the transformation and node point values from the image.
							InitializeClipper(_texture, new RectangleF(0, 0, settings.Width, settings.Height));
							
							TextureBrushFile = imageFileBrowser.Files[0];

							OnBrushChanged();
						}
					}

					GorgonFontEditorPlugIn.Settings.LastTextureImportPath = Path.GetDirectoryName(imageFileBrowser.Files[0].FilePath).FormatDirectory('/');
				}

				GorgonFontEditorPlugIn.Settings.LastTextureImportDialogView = imageFileBrowser.FileView;
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
			finally
			{
				EnableNumericLimits();

				numericWidth.ValueChanged += numericX_ValueChanged;
				numericHeight.ValueChanged += numericX_ValueChanged;
				numericX.ValueChanged += numericX_ValueChanged;
				numericY.ValueChanged += numericX_ValueChanged;

				Cursor.Current = Cursors.Default;
				UpdateLabelInfo();
				ValidateCommands();
			}
		}

		/// <summary>
		/// Function to execute during idle time.
		/// </summary>
		/// <returns>TRUE to continue executing, FALSE to stop.</returns>
		private bool Idle()
		{
			_renderer.Drawing.TextureSampler.HorizontalWrapping = TextureAddressing.Wrap;
			_renderer.Drawing.TextureSampler.VerticalWrapping = TextureAddressing.Wrap;

			_renderer.Drawing.Blit(_defaultTexture,
								   new RectangleF(0, 0, panelTextureDisplay.Width, panelTextureDisplay.Height),
								   _defaultTexture.ToTexel(panelTextureDisplay.ClientRectangle));

			_renderer.Drawing.TextureSampler.HorizontalWrapping = TextureAddressing.Clamp;
			_renderer.Drawing.TextureSampler.VerticalWrapping = TextureAddressing.Clamp;

			if (Texture == null)
			{
				_renderer.Render(2);
				return true;
			}

			_renderer.Drawing.Blit(Texture,
								   _displayRegion,
								   new RectangleF(0, 0, 1, 1));

			_clipper.Draw();

			_renderer.Render(2);
			return true;
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.UserControl.Load" /> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (DesignMode)
			{
				return;
			}

			try
			{
				PerformLocalization();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
			finally
			{
				ValidateCommands();
			}
		}

		/// <summary>
		/// Function to clean up the renderer objects.
		/// </summary>
		private void CleanUpRenderer()
		{
			if (_renderer == null)
			{
				return;
			}

			if (_lastState != null)
			{
				_renderer.End2D(_lastState);
				_lastState = null;
			}

			if (_texture != null)
			{
				_texture.Dispose();
				_texture = null;
			}

			if (_defaultTexture != null)
			{
				_defaultTexture.Dispose();
				_defaultTexture = null;
			}

			if (_swapChain != null)
			{
				_swapChain.Dispose();
				_swapChain = null;
			}

			_clipper = null;
			_graphics = null;
		}

		/// <summary>
		/// Function to set up the renderer.
		/// </summary>
		/// <param name="renderer">Renderer to use.</param>
		public void SetupRenderer(Gorgon2D renderer)
		{
			if (DesignMode)
			{
				return;
			}

			try
			{
				GorgonApplication.ApplicationIdleLoopMethod = null;

				if (_renderer != renderer)
				{
					CleanUpRenderer();
				}

				if (renderer == null)
				{
					return;
				}

				_renderer = renderer;
				_graphics = renderer.Graphics;

				if (_swapChain == null)
				{
					_swapChain = _graphics.Output.CreateSwapChain("ImageDisplay",
					                                              new GorgonSwapChainSettings
					                                              {
						                                              Window = panelTextureDisplay,
						                                              Format = BufferFormat.R8G8B8A8_UIntNormal
					                                              });
				}

				if (_lastState == null)
				{
					_lastState = renderer.Begin2D();
				}

				if (_defaultTexture == null)
				{
					_defaultTexture = _graphics.Textures.CreateTexture<GorgonTexture2D>("DefaultPattern", Resources.Pattern);
				}

				if (_clipper == null)
				{
					InitializeClipper(null, RectangleF.Empty);
				}

				_region = new RectangleF(0, 0, 1, 1);

				_renderer.Target = _swapChain;

				GorgonApplication.ApplicationIdleLoopMethod = Idle;
			}
			finally
			{
				ValidateCommands();
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="PanelTexture"/> class.
		/// </summary>
		public PanelTexture()
		{
			InitializeComponent();
		}
		#endregion
	}
}
