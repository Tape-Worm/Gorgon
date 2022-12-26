#region MIT
// 
// Gorgon.
// Copyright (C) 2021 Michael Winsor
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
// Created: August 28, 2021 9:09:37 PM
// 
#endregion

using System;
using System.Collections.Generic;
using Drawing = System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Gorgon.Editor.FontEditor.Properties;
using Gorgon.Graphics.Fonts;
using Gorgon.IO;
using Gorgon.UI;
using Gorgon.Graphics;
using System.Xml.XPath;

namespace Gorgon.Editor.FontEditor;

/// <summary>
/// Font creation form.
/// </summary>
internal partial class FormNewFont
    : Form, IGorgonFontInfo
{
    #region Variables.
    // Font used for preview.
    private Drawing.Font _font;
    #endregion

    #region Properties.                
    /// <summary>
    /// Property to return the font name.
    /// </summary>
    public string FontName
    {
        get => TextFontName.Text;
        set => TextFontName.Text = value.FormatFileName();
    }

    /// <summary>
    /// Property to return the font family name.
    /// </summary>
    public string FontFamilyName
    {
        get => ComboFontFamilies.Text;
        set => ComboFontFamilies.Text = value;
    }

    /// <summary>
    /// Property to return whether to use points or pixels for the font size.
    /// </summary>
    public FontHeightMode FontHeightMode
    {
        get => RadioPoints.Checked ? FontHeightMode.Points : FontHeightMode.Pixels;
        set => RadioPoints.Checked = value != FontHeightMode.Pixels;
    }

    /// <summary>
    /// Property to return the size of the font.
    /// </summary>
    public float FontSize
    {
        get => (float)NumericFontSize.Value;
        set => NumericFontSize.Value = (decimal)value;
    }

    /// <summary>
    /// Property to return the size of the font.
    /// </summary>
    float IGorgonFontInfo.Size => FontSize;

    /// <summary>
    /// Property to return the font style.
    /// </summary>
    public FontStyle FontStyle
    {
        get
        {
            FontStyle result = FontStyle.Normal;

            if ((CheckItalics.Checked) && (CheckBold.Checked))
            {
                result = FontStyle.BoldItalics;
            }
            else if (CheckBold.Checked)
            {
                result = FontStyle.Bold;
            } 
            else if (CheckItalics.Checked)
            {
                result = FontStyle.Italics;
            }                

            return result;
        }
        set
        {
            switch (value)
            {
                case FontStyle.BoldItalics:
                    CheckBold.Checked = true;
                    CheckItalics.Checked = true;
                    break;
                case FontStyle.Bold:
                    CheckBold.Checked = true;
                    CheckItalics.Checked = false;
                    break;
                case FontStyle.Italics:
                    CheckBold.Checked = false;
                    CheckItalics.Checked = true;
                    break;
                default:
                    CheckBold.Checked = false;
                    CheckItalics.Checked = false;
                    break;
            }
        }
    }

    /// <summary>Property to return the width of the texture(s) used as the backing store for the bitmap font data.</summary>
    /// <remarks>
    ///   <para>
    /// This value will control the width of the textures created.  It can be used to decrease or increase the number of textures used for a font.  This is important because if the number of
    /// requested glyphs cannot fit onto a single texture, a new texture will be created to store the remaining glyphs. Keeping as few textures as possible for a font is beneficial for performance.
    /// </para>
    ///   <para>
    /// Font textures cannot be smaller than 256x256 and the maximum size is dependent upon <see cref="Graphics.Core.FeatureSet" /> for the <see cref="Graphics.Core.IGorgonVideoAdapterInfo" />.
    /// </para>
    ///   <para>
    /// The default width is 256.
    /// </para>
    /// </remarks>
    public int TextureWidth
    {
        get => (int)NumericTextureWidth.Value;
        set => NumericTextureWidth.Value = value;
    }

    /// <summary>Property to return the height of the texture(s) used as the backing store for the bitmap font data.</summary>
    /// <remarks>
    ///   <para>
    /// This value will control the width of the textures created.  It can be used to decrease or increase the number of textures used for a font.  This is important because if the number of
    /// requested glyphs cannot fit onto a single texture, a new texture will be created to store the remaining glyphs. Keeping as few textures as possible for a font is beneficial for performance.
    /// </para>
    ///   <para>
    /// Font textures cannot be smaller than 256x256 and the maximum size is dependent upon <see cref="Graphics.Core.FeatureSet" /> for the <see cref="Graphics.Core.IGorgonVideoAdapterInfo" />.
    /// </para>
    ///   <para>
    /// The default height is 256.
    /// </para>
    /// </remarks>
    public int TextureHeight
    {
        get => (int)NumericTextureHeight.Value;
        set => NumericTextureHeight.Value = value;
    }

    /// <summary>Property to return the list of available characters to use as glyphs within the font.</summary>
    /// <remarks>
    ///   <para>
    /// This will define what characters the font can display when rendering.  Any character not defined in this property will use the default character (typically a space).
    /// </para>
    ///   <para>
    /// The default encompasses characters from ASCII character code 32 to 255.
    /// </para>
    /// </remarks>
    public IEnumerable<char> Characters
    {
        get;
        set;
    } = FontService.DefaultCharacters;

    /// <summary>Property to return the anti-aliasing mode for the font.</summary>
    /// <remarks>
    ///   <para>
    /// This defines the smoothness of font pixel edges.
    /// </para>
    ///   <para>
    ///     <note type="important">
    ///       <para>
    /// Gorgon does not support clear type at this time.
    /// </para>
    ///     </note>
    ///   </para>
    ///   <para>
    /// The default value is <see cref="F:Gorgon.Graphics.Fonts.FontAntiAliasMode.AntiAlias" />.
    /// </para>
    /// </remarks>
    public FontAntiAliasMode AntiAliasingMode
    {
        get => CheckAntiAliased.Checked ? FontAntiAliasMode.AntiAlias : FontAntiAliasMode.None;
        set => CheckAntiAliased.Checked = value == FontAntiAliasMode.AntiAlias;
    }

    /// <summary>Property to return the size of an outline.</summary>
    /// <remarks>
    ///   <para>
    /// This defines the thickness of an outline (in pixels) for each glyph in the font.  A value greater than 0 will draw an outline, 0 or less will turn outlining off.
    /// </para>
    ///   <para>
    /// The default value is 0.
    /// </para>
    /// </remarks>
    int IGorgonFontInfo.OutlineSize => 0;

    /// <summary>Property to return the starting color of the outline.</summary>
    /// <remarks>
    ///   <para>
    /// This defines the starting color for an outline around a font glyph. This can be used to give a gradient effect to the outline and allow for things like glowing effects and such.
    /// </para>
    ///   <para>
    /// If the alpha channel is set to 0.0f and the <see cref="IGorgonFontInfo.OutlineColor2" /> alpha channel is set to 0.0f, then outlining will be disabled since it will be invisible. This will also be ignored
    /// if the <see cref="IGorgonFontInfo.OutlineSize" /> value is not greater than 0.
    /// </para>
    ///   <para>
    /// The default value is <see cref="GorgonColor.Transparent" /> (A=1.0f, R=0.0f, G=0.0f, B=0.0f).
    /// </para>
    /// </remarks>
    GorgonColor IGorgonFontInfo.OutlineColor1 => GorgonColor.BlackTransparent;

    /// <summary>Property to return the ending color of the outline.</summary>
    /// <remarks>
    ///   <para>
    /// This defines the ending color for an outline around a font glyph. This can be used to give a gradient effect to the outline and allow for things like glowing effects and such.
    /// </para>
    ///   <para>
    /// If the alpha channel is set to 0.0f and the <see cref="IGorgonFontInfo.OutlineColor1" /> alpha channel is set to 0.0f, then outlining will be disabled since it will be invisible. This will also be ignored
    /// if the <see cref="IGorgonFontInfo.OutlineSize" /> value is not greater than 3.
    /// </para>
    ///   <para>
    /// The default value is <see cref="GorgonColor.Transparent" /> (A=1.0f, R=0.0f, G=0.0f, B=0.0f).
    /// </para>
    /// </remarks>
    GorgonColor IGorgonFontInfo.OutlineColor2 => GorgonColor.BlackTransparent;

    /// <summary>Property to return whether premultiplied textures are used when generating the glyphs for the font.</summary>
    /// <remarks>
    ///   <para>
    /// This defines whether the textures used to store the glyphs of the font will use premultiplied alpha. Premultiplied alpha is used to give a more accurate representation of blending between
    /// colors, and is accomplished by multiplying the RGB values by the Alpha component of a pixel.
    /// </para>
    ///   <para>
    /// If this value is <b>true</b>, then applications should use the <see cref="Graphics.Core.GorgonBlendState.Premultiplied" /> blending state when rendering text.
    /// </para>
    ///   <para>
    /// The default value is <b>true</b>.
    /// </para>
    /// </remarks>
    bool IGorgonFontInfo.UsePremultipliedTextures => true;

    /// <summary>Property to return a <see cref="Graphics.Fonts.GorgonGlyphBrush" /> to use for special effects on the glyphs for the font.</summary>
    /// <remarks>
    ///   <para>
    /// This value can be used to define how a glyph is rendered when the <see cref="Graphics.Fonts.GorgonFont" /> is generated. Applications can use brushes for gradients, textures, etc... to render the glyphs to give
    /// them a unique look.
    /// </para>
    ///   <para>
    /// This default value is <b>null</b>.
    /// </para>
    /// </remarks>
    GorgonGlyphBrush IGorgonFontInfo.Brush => null;

    /// <summary>Property to return a default character to use in place of a character that cannot be found in the font.</summary>
    /// <remarks>
    ///   <para>
    /// If some characters are unprintable (e.g. have no width/height), or they were not defined in the <see cref="IGorgonFontInfo.Characters" /> property.  This character will be substituted in those cases.
    /// </para>
    ///   <para>
    /// The default value is a space (' ').
    /// </para>
    /// </remarks>
    char IGorgonFontInfo.DefaultCharacter => ' ';

    /// <summary>Property to return the spacing (in pixels) used between font glyphs on the backing texture.</summary>
    /// <remarks>
    ///   <para>
    /// This defines how much space to put between glyphs. Higher values will waste more space (and thus lead to more textures being created), but may resolve rendering issues.
    /// </para>
    ///   <para>
    /// The valid values are between 0 and 8.
    /// </para>
    ///   <para>
    /// The default value is 1 pixel.
    /// </para>
    /// </remarks>
    int IGorgonFontInfo.PackingSpacing => 1;

    /// <summary>Property to return whether to include kerning pair information in the font.</summary>
    /// <remarks>
    ///   <para>
    /// Kerning pairs are used to define spacing between 2 characters in a font.  When this value is set to <b>true</b> the kerning information of the font is retrieved and added to the <see cref="Graphics.Fonts.GorgonFont" />
    /// for use when rendering. This can be used to resolve rendering issues regarding horizontal font spacing.
    /// </para>
    ///   <para>
    ///     <note type="note">
    ///       <para>
    /// Some fonts do not employ the use of kerning pairs, and consequently, this setting will be ignored if that is the case.
    /// </para>
    ///     </note>
    ///   </para>
    ///   <para>
    /// The default value is <b>true</b>.
    /// </para>
    /// </remarks>
    bool IGorgonFontInfo.UseKerningPairs => true;
    #endregion

    #region Methods.
    /// <summary>
    /// Function to validate the controls on the form.
    /// </summary>
    private void ValidateControls()
    {
        if (string.IsNullOrEmpty(ComboFontFamilies.Text))
        {
            ButtonCharacters.Enabled =
                ButtonOK.Enabled =
                CheckBold.Enabled = CheckItalics.Enabled = false;
            return;
        }
        else
        {
            Drawing.FontFamily family = Drawing.FontFamily.Families.SingleOrDefault(item => string.Equals(item.Name, ComboFontFamilies.Text, StringComparison.OrdinalIgnoreCase));

            ButtonCharacters.Enabled = true;

            if (family == null)
            {
                CheckBold.Checked = CheckItalics.Checked = false;
            }
            else
            {
                CheckBold.Enabled = family.IsStyleAvailable(Drawing.FontStyle.Bold) && family.IsStyleAvailable(Drawing.FontStyle.Regular);                    
                CheckItalics.Enabled = family.IsStyleAvailable(Drawing.FontStyle.Italic) && family.IsStyleAvailable(Drawing.FontStyle.Regular);                    

                ButtonOK.Enabled = TextFontName.Text.Length > 0;
            }
        }

        if ((!CheckBold.Enabled) && (CheckBold.Checked))
        {
            CheckBold.Checked = false;
        }

        if ((!CheckItalics.Enabled) && (CheckItalics.Checked))
        {
            CheckItalics.Checked = false;
        }
    }

    /// <summary>
    /// Function to update the preview font.
    /// </summary>
    private void UpdatePreview()
    {
        Drawing.FontStyle style = Drawing.FontStyle.Regular;

        labelPreview.Font = Font;
        if (_font != null)
        {
            _font.Dispose();
        }

        _font = null;

        if (CheckBold.Checked)
        {
            style |= Drawing.FontStyle.Bold;
        }
        if (CheckItalics.Checked)
        {
            style |= Drawing.FontStyle.Italic;
        }

        _font = new Drawing.Font(ComboFontFamilies.Text, FontSize, style, FontHeightMode == FontHeightMode.Pixels ? Drawing.GraphicsUnit.Pixel : Drawing.GraphicsUnit.Point);
        labelPreview.Font = _font;
    }

    /// <summary>
    /// Handles the SelectedIndexChanged event of the comboFonts control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ComboFontFamilies_SelectedIndexChanged(object sender, EventArgs e)
    {
        try
        {
            ValidateControls();
            UpdatePreview();
        }
        catch (Exception ex)
        {
            GorgonDialogs.ErrorBox(this, ex);
            ValidateControls();
        }
    }

    /// <summary>Handles the Click event of the ButtonCharacters control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs">EventArgs</see> instance containing the event data.</param>
    private void ButtonCharacters_Click(object sender, EventArgs e)
    {
        Drawing.Font currentFont = null;
        FormCharacterPicker picker = null;

        try
        {
            currentFont = new Drawing.Font(ComboFontFamilies.Text, 16.0f, Drawing.GraphicsUnit.Pixel);

            picker = new FormCharacterPicker
            {
                Characters = Characters,
                CurrentFont = currentFont
            };

            if (picker.ShowDialog(this) == DialogResult.OK)
            {
                Characters = picker.Characters;
            }
        }
        catch (Exception ex)
        {
            GorgonDialogs.ErrorBox(this, ex);
        }
        finally
        {
            if (currentFont != null)
            {
                currentFont.Dispose();
            }

            if (picker != null)
            {
                picker.Dispose();
            }
        }
    }

    /// <summary>
    /// Handles the TextChanged event of the textName control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void TextFontName_TextChanged(object sender, EventArgs e) => ValidateControls();

    /// <summary>
    /// Raises the <see cref="Form.FormClosing"/> event.
    /// </summary>
    /// <param name="e">A <see cref="FormClosingEventArgs"/> that contains the event data.</param>
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);

        _font?.Dispose();
    }

    /// <summary>
    /// Raises the <see cref="Form.Load"/> event.
    /// </summary>
    /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        RadioPoints.Checked = true;
        NumericTextureWidth.Value = 512;
        NumericTextureHeight.Value = 512;
        
        ValidateControls();

        ComboFontFamilies.Select();
    }
    #endregion

    #region Constructor/Destructor.
    /// <summary>
    /// Initializes a new instance of the <see cref="FormNewFont"/> class.
    /// </summary>
    public FormNewFont()
    {
        InitializeComponent();

        ComboFontFamilies.RefreshFonts(true);
    }
    #endregion

}
