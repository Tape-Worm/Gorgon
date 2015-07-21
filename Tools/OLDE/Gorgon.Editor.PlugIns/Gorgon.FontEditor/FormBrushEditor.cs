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
// Created: Wednesday, November 13, 2013 8:58:39 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Gorgon.Editor.FontEditorPlugIn.Properties;
using Gorgon.Graphics;
using Gorgon.Graphics.Fonts;
using Gorgon.Math;
using Gorgon.UI;

namespace Gorgon.Editor.FontEditorPlugIn
{
    /// <summary>
    /// An editor that will create a brush for a font.
    /// </summary>
    partial class FormBrushEditor 
        : GorgonFlatForm
	{
		#region Classes.
		/// <summary>
		/// A brush type combo box item.
		/// </summary>
	    private class BrushTypeComboItem
	    {
			#region Variables.
			private readonly string _text;			// Item text.
			#endregion

			#region Properties.
			/// <summary>
			/// Property to return the brush type.
			/// </summary>
			public GlyphBrushType BrushType
			{
				get;
			}
			#endregion

			#region Methods.
			/// <summary>
			/// Returns a <see cref="System.String" /> that represents this instance.
			/// </summary>
			/// <returns>
			/// A <see cref="System.String" /> that represents this instance.
			/// </returns>
			public override string ToString()
			{
				return _text;
			}
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="BrushTypeComboItem"/> class.
			/// </summary>
			/// <param name="brushType">Type of the brush.</param>
			/// <param name="text">The text.</param>
			public BrushTypeComboItem(GlyphBrushType brushType, string text)
			{
				BrushType = brushType;
				_text = text;
			}
			#endregion
	    }
		#endregion

		#region Variables.
		// Primary selected color attribute for color selection panels.
		private static ColorPickerPanel.PrimaryAttrib _primaryAttr = ColorPickerPanel.PrimaryAttrib.Hue;

		private readonly GorgonFontContent _currentContent;					// Current content object.
	    private readonly Func<bool> _previousIdle;							// Previously active idle function.
		private GlyphBrushType _originalBrushType = GlyphBrushType.Solid;	// Original brush type.
        #endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the texture brush path.
		/// </summary>
	    public EditorFile TextureBrushFile
	    {
			get
			{
				return panelTextureEditor.TextureBrushFile;
			}
			set
			{
				panelTextureEditor.TextureBrushFile = value;
			}
	    }

		/// <summary>
		/// Property to set or return the current brush type.
		/// </summary>
	    public GlyphBrushType BrushType
	    {
		    get;
		    set;
	    }

		/// <summary>
		/// Property to set or return the current solid brush.
		/// </summary>
	    public GorgonGlyphSolidBrush SolidBrush
	    {
			get;
			set;
	    }

		/// <summary>
		/// Property to set or return the current hatch pattern brush.
		/// </summary>
	    public GorgonGlyphHatchBrush PatternBrush
	    {
		    get;
		    set;
	    }

		/// <summary>
		/// Property to set or return the current gradient brush.
		/// </summary>
	    public GorgonGlyphLinearGradientBrush GradientBrush
	    {
		    get;
		    set;
	    }

		/// <summary>
		/// Property to set or return the current texture brush.
		/// </summary>
	    public GorgonGlyphTextureBrush TextureBrush
	    {
			get;
			set;
	    }
        #endregion

        #region Methods.
		/// <summary>
		/// Handles the BrushChanged event of the gradEditor control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void BrushChanged(object sender, EventArgs e)
		{
			ValidateCommands();
		}

		/// <summary>
		/// Function to validate the command states.
		/// </summary>
	    private void ValidateCommands()
	    {
			switch (BrushType)
			{
				case GlyphBrushType.Solid:
					buttonOK.Enabled = ((colorSolidBrush.SelectedColor != colorSolidBrush.OldColor)
					                    || (BrushType != _originalBrushType));
					break;
				case GlyphBrushType.Texture:
					buttonOK.Enabled = ((panelTextureEditor.Texture != null) && ((TextureBrush == null)
					                                                             || (!string.Equals(panelTextureEditor.Texture.Name,
																									TextureBrush.Texture.Name,
																									StringComparison.OrdinalIgnoreCase))
					                                                             || (panelTextureEditor.TextureRegion != TextureBrush.TextureRegion)
					                                                             || (panelTextureEditor.WrapMode != TextureBrush.WrapMode)
					                                                             || (BrushType != _originalBrushType)));
					break;
				case GlyphBrushType.Hatched:
					buttonOK.Enabled = ((PatternBrush.ForegroundColor != panelHatchEditor.HatchForegroundColor)
					                    || (PatternBrush.BackgroundColor != panelHatchEditor.HatchBackgroundColor)
					                    || (PatternBrush.HatchStyle != panelHatchEditor.HatchStyle)
										|| (BrushType != _originalBrushType));
					break;
				case GlyphBrushType.LinearGradient:
					GorgonGlyphBrushInterpolator[] interpolators = panelGradEditor.GetInterpolators().ToArray();

					buttonOK.Enabled = ((GradientBrush.ScaleAngle != panelGradEditor.ScaleAngle)
					                    || (GradientBrush.GammaCorrection != panelGradEditor.UseGammaCorrection)
					                    || (!GradientBrush.Angle.EqualsEpsilon(panelGradEditor.Angle))
					                    || (interpolators.Length != GradientBrush.Interpolation.Count)
					                    || interpolators.Where((item, index) =>
					                                           !item.Weight.EqualsEpsilon(GradientBrush.Interpolation[index].Weight)
					                                           || !item.Color.Equals(GradientBrush.Interpolation[index].Color)).Any()
					                    || (BrushType != _originalBrushType));
					break;
			}
	    }

		/// <summary>
        /// Handles the Click event of the buttonOK control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void buttonOK_Click(object sender, EventArgs e)
        {
			try
			{
				switch (BrushType)
				{
					case GlyphBrushType.Solid:
						SolidBrush = new GorgonGlyphSolidBrush
						             {
							             Color = colorSolidBrush.SelectedColor
						             };
						break;
					case GlyphBrushType.Texture:
						GorgonTexture2D newTexture = panelTextureEditor.Texture.Graphics.Textures.CreateTexture(panelTextureEditor.Texture.Name,
							                                                           panelTextureEditor.Texture.Settings);

						newTexture.Copy(panelTextureEditor.Texture);

						TextureBrush = new GorgonGlyphTextureBrush(newTexture)
						               {
							               WrapMode = panelTextureEditor.WrapMode,
							               TextureRegion = panelTextureEditor.TextureRegion
						               };

						panelTextureEditor.Texture = null;
						break;
					case GlyphBrushType.Hatched:
						PatternBrush = new GorgonGlyphHatchBrush
						               {
										   ForegroundColor = panelHatchEditor.HatchForegroundColor,
										   BackgroundColor = panelHatchEditor.HatchBackgroundColor,
										   HatchStyle = panelHatchEditor.HatchStyle
						               };
						break;
					case GlyphBrushType.LinearGradient:
						GradientBrush = new GorgonGlyphLinearGradientBrush
						                {
							                Angle = panelGradEditor.Angle,
							                ScaleAngle = panelGradEditor.ScaleAngle,
							                GammaCorrection = panelGradEditor.UseGammaCorrection
						                };

						IEnumerable<GorgonGlyphBrushInterpolator> interpolators = panelGradEditor.GetInterpolators();

						GradientBrush.Interpolation.Clear();

						foreach (GorgonGlyphBrushInterpolator interpolator in interpolators)
						{
							GradientBrush.Interpolation.Add(interpolator);
						}

						break;
				}
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
				DialogResult = DialogResult.None;
			}
        }

        /// <summary>
        /// Handles the ColorChanged event of the colorSolidBrush control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void colorSolidBrush_ColorChanged(object sender, EventArgs e)
        {
			ValidateCommands();
        }

		/// <summary>
		/// Handles the SelectedIndexChanged event of the comboBrushType control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void comboBrushType_SelectedIndexChanged(object sender, EventArgs e)
		{
			var item = comboBrushType.SelectedItem as BrushTypeComboItem;

			if (item == null)
			{
				return;
			}

			BrushType = item.BrushType;
			switch (item.BrushType)
			{
				case GlyphBrushType.Solid:
					tabBrushEditor.SelectedTab = pageSolid;
					break;
				case GlyphBrushType.Texture:
					tabBrushEditor.SelectedTab = pageTexture;
					break;
				case GlyphBrushType.LinearGradient:
					tabBrushEditor.SelectedTab = pageGradient;
					break;
				case GlyphBrushType.Hatched:
					tabBrushEditor.SelectedTab = pagePattern;
					break;
			}

			ValidateCommands();
		}

        /// <summary>
		/// Function to localize the form.
		/// </summary>
	    private void Localize()
        {
	        Text = Resources.GORFNT_DLG_BRUSH_EDITOR_CAPTION;

	        buttonOK.Text = Resources.GORFNT_ACC_TEXT_OK;
	        buttonCancel.Text = Resources.GORFNT_ACC_TEXT_CANCEL;

	        labelBrushType.Text = Resources.GORFNT_TEXT_BRUSH_TYPE;

		    comboBrushType.Items.Clear();
			comboBrushType.Items.Add(new BrushTypeComboItem(GlyphBrushType.Solid, Resources.GORFNT_TEXT_SOLID_BRUSH));
			comboBrushType.Items.Add(new BrushTypeComboItem(GlyphBrushType.LinearGradient, Resources.GORFNT_TEXT_GRADIENT_BRUSH));
			comboBrushType.Items.Add(new BrushTypeComboItem(GlyphBrushType.Hatched, Resources.GORFNT_TEXT_PATTERN_BRUSH));
	        if (_currentContent.ImageEditor != null)
	        {
		        comboBrushType.Items.Add(new BrushTypeComboItem(GlyphBrushType.Texture,
		                                                        Resources.GORFNT_TEXT_TEXTURE_BRUSH));
	        }

	        comboBrushType.SelectedItem = 0;
	    }

		/// <summary>
		/// Handles the SelectedIndexChanged event of the tabBrushEditor control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void tabBrushEditor_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				if (tabBrushEditor.SelectedTab == pageTexture)
				{
					panelTextureEditor.SetupRenderer(_currentContent.Renderer);
				}
				else
				{
					GorgonApplication.IdleMethod = null;
				}
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				ValidateCommands();
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing" /> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs" /> that contains the event data.</param>
	    protected override void OnFormClosing(FormClosingEventArgs e)
	    {
		    base.OnFormClosing(e);

			// Shut down the renderer on the texture editor panel.
			panelTextureEditor.SetupRenderer(null);

			Gorgon.AllowBackground = false;
			GorgonApplication.IdleMethod = _previousIdle;

			// Remember our previous selection for the primary color attribute.
			_primaryAttr = colorSolidBrush.PrimaryAttribute;
	    }


	    /// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load" /> event.
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
			    Localize();
				
			    // Force to the first tab page.
			    tabBrushEditor.SelectedTab = pageSolid;

			    colorSolidBrush.PrimaryAttribute = _primaryAttr;
			    colorSolidBrush.OldColor = SolidBrush.Color;
			    colorSolidBrush.SelectedColor = SolidBrush.Color;

				panelTextureEditor.ImageEditor = _currentContent.ImageEditor;

			    _originalBrushType = BrushType;

			    switch (BrushType)
			    {
				    case GlyphBrushType.LinearGradient:
					    comboBrushType.Text = Resources.GORFNT_TEXT_GRADIENT_BRUSH;

					    panelGradEditor.Angle = GradientBrush.Angle;
					    panelGradEditor.UseGammaCorrection = GradientBrush.GammaCorrection;
					    panelGradEditor.ScaleAngle = GradientBrush.ScaleAngle;
						panelGradEditor.SetInterpolation(GradientBrush.Interpolation);

					    tabBrushEditor.SelectedTab = pageGradient;
					    break;
				    case GlyphBrushType.Hatched:
					    comboBrushType.Text = Resources.GORFNT_TEXT_PATTERN_BRUSH;

					    panelHatchEditor.HatchForegroundColor = PatternBrush.ForegroundColor;
					    panelHatchEditor.HatchBackgroundColor = PatternBrush.BackgroundColor;
					    panelHatchEditor.HatchStyle = PatternBrush.HatchStyle;

					    tabBrushEditor.SelectedTab = pagePattern;
					    break;
				    case GlyphBrushType.Texture:
						if (_currentContent.ImageEditor == null)
						{
							GorgonDialogs.WarningBox(this, Resources.GORFNT_WARN_NO_IMG_EDITOR);
							comboBrushType.SelectedIndex = 0;
							break;
						}

					    comboBrushType.Text = Resources.GORFNT_TEXT_TEXTURE_BRUSH;
					    tabBrushEditor.SelectedTab = pageTexture;
						
					    panelTextureEditor.Texture = TextureBrush.Texture;
					    panelTextureEditor.WrapMode = TextureBrush.WrapMode;
						panelTextureEditor.TextureRegion = TextureBrush.TextureRegion;
					    break;
				    default:
					    comboBrushType.Text = Resources.GORFNT_TEXT_SOLID_BRUSH;
					    break;
			    }
		    }
		    catch (Exception ex)
		    {
			    GorgonDialogs.ErrorBox(this, ex);
			    Close();
		    }
		    finally
		    {
			    ValidateCommands();
		    }
	    }
	    #endregion

        #region Constructor/Destructor.
	    /// <summary>
	    /// Initializes a new instance of the <see cref="FormBrushEditor"/> class.
	    /// </summary>
	    public FormBrushEditor()
	    {
		    InitializeComponent();
	    }

	    /// <summary>
        /// Initializes a new instance of the <see cref="FormBrushEditor"/> class.
        /// </summary>
        /// <param name="fontContent">The current font content interface.</param>
        public FormBrushEditor(GorgonFontContent fontContent)
			: this()
	    {
		    _currentContent = fontContent;
			_previousIdle = GorgonApplication.IdleMethod;
			GorgonApplication.IdleMethod = null;
		    Gorgon.AllowBackground = true;		// Enable background rendering because the application will appear to have lost focus when this dialog is present.
			SolidBrush = new GorgonGlyphSolidBrush();
	        GradientBrush = new GorgonGlyphLinearGradientBrush
	                        {
		                        StartColor = GorgonColor.Black,
		                        EndColor = GorgonColor.White
	                        };
	        PatternBrush = new GorgonGlyphHatchBrush
	                       {
		                       ForegroundColor = GorgonColor.Black,
		                       BackgroundColor = GorgonColor.White,
		                       HatchStyle = HatchStyle.BackwardDiagonal
	                       };
        }
        #endregion
    }
}
