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
// Created: Saturday, November 15, 2014 9:59:32 PM
// 
#endregion

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Editor.SpriteEditorPlugIn.Properties;
using Gorgon.Graphics;
using Gorgon.IO;

namespace Gorgon.Editor.SpriteEditorPlugIn.Design
{
	/// <summary>
	/// Design time editor interface for grabbing a texture.
	/// </summary>
	class TextureTypeEditor
		: UITypeEditor
	{
		#region Variables.

		#endregion

		#region Properties.
		
		#endregion

		#region Methods.
		/// <summary>
		/// Gets the editor style used by the <see cref="M:System.Drawing.Design.UITypeEditor.EditValue(System.IServiceProvider,System.Object)" /> method.
		/// </summary>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that can be used to gain additional context information.</param>
		/// <returns>
		/// A <see cref="T:System.Drawing.Design.UITypeEditorEditStyle" /> value that indicates the style of editor used by the <see cref="M:System.Drawing.Design.UITypeEditor.EditValue(System.IServiceProvider,System.Object)" /> method. If the <see cref="T:System.Drawing.Design.UITypeEditor" /> does not support this method, then <see cref="M:System.Drawing.Design.UITypeEditor.GetEditStyle" /> will return <see cref="F:System.Drawing.Design.UITypeEditorEditStyle.None" />.
		/// </returns>
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.Modal;
		}

		/// <summary>
		/// Indicates whether the specified context supports painting a representation of an object's value within the specified context.
		/// </summary>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that can be used to gain additional context information.</param>
		/// <returns>
		/// true if <see cref="M:System.Drawing.Design.UITypeEditor.PaintValue(System.Object,System.Drawing.Graphics,System.Drawing.Rectangle)" /> is implemented; otherwise, false.
		/// </returns>
		public override bool GetPaintValueSupported(ITypeDescriptorContext context)
		{
			return true;
		}

		/// <summary>
		/// Paints a representation of the value of an object using the specified <see cref="T:System.Drawing.Design.PaintValueEventArgs" />.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Drawing.Design.PaintValueEventArgs" /> that indicates what to paint and where to paint it.</param>
		public override void PaintValue(PaintValueEventArgs e)
		{
			var sprite = (GorgonSpriteContent)((ContentTypeDescriptor)e.Context.Instance).Content;

			if (sprite.Texture == null)
			{
				e.Graphics.DrawImage(Resources.image_missing_16x16,
				                     e.Bounds,
				                     new Rectangle(0, 0, Resources.image_missing_16x16.Width, Resources.image_missing_16x16.Height),
				                     GraphicsUnit.Pixel);
				return;
			}

			// We cannot convert block compressed images to GDI+ image types, so just put a generic icon up.
			if ((sprite.Texture.FormatInformation.IsCompressed)
				|| (!GorgonImageData.CanConvert(sprite.Texture.Settings.Format, BufferFormat.R8G8B8A8_UIntNormal)))
			{
				e.Graphics.DrawImage(Resources.open_image_16x16,
									 e.Bounds,
									 new Rectangle(0, 0, Resources.open_image_16x16.Width, Resources.open_image_16x16.Height),
									 GraphicsUnit.Pixel);
				return;
			}

			// Copy the image into system memory, convert it and display it.
			using (GorgonImageData imageData = GorgonImageData.CreateFromTexture(sprite.Texture))
			{
				imageData.Resize(e.Bounds.Width, e.Bounds.Height, false, ImageFilter.Fant);
				imageData.ConvertFormat(BufferFormat.R8G8B8A8_UIntNormal);

				using (Image gdiImage = imageData.Buffers[0].ToGDIImage())
				{
					e.Graphics.DrawImage(gdiImage, e.Bounds);
				}
			}
		}

		/// <summary>
		/// Edits the specified object's value using the editor style indicated by the <see cref="M:System.Drawing.Design.UITypeEditor.GetEditStyle" /> method.
		/// </summary>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that can be used to gain additional context information.</param>
		/// <param name="provider">An <see cref="T:System.IServiceProvider" /> that this editor can use to obtain services.</param>
		/// <param name="value">The object to edit.</param>
		/// <returns>
		/// The new value of the object. If the value of the object has not changed, this should return the same object it was passed.
		/// </returns>
		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			var sprite = (GorgonSpriteContent)((ContentTypeDescriptor)context.Instance).Content;
			GorgonTexture2D currentTexture = sprite.Texture;
			string textureName = currentTexture != null ? currentTexture.Name : string.Empty;
			Dependency dependency = sprite.Dependencies.SingleOrDefault(item => string.Equals(item.Type, GorgonSpriteContent.TextureDependencyType));
			
			using(var editorDialog = new EditorFileDialog
			                         {
				                         Text = Resources.GORSPR_DLG_SELECT_TEXTURE,
										 Filename = textureName,
										 StartDirectory = GorgonSpriteEditorPlugIn.Settings.LastTexturePath,
										 FileView = FileViews.Large
			                         })
			{
				editorDialog.FileTypes.Add(sprite.ImageEditor.ContentType);

				if ((editorDialog.ShowDialog() == DialogResult.Cancel)
					|| (string.Equals(editorDialog.Filename, textureName, StringComparison.OrdinalIgnoreCase)))
				{
					// Resharper is just plain wrong here.  EditValue can most certainly return NULL.
					// ReSharper disable once AssignNullToNotNullAttribute
					return currentTexture;
				}

				GorgonTexture2D texture;

				// Read the new texture.
				using (Stream file = editorDialog.OpenFile())
				{
					using (IImageEditorContent image = sprite.ImageEditor.ImportContent(editorDialog.Files[0], file))
					{
						if (image.Image.Settings.ImageType != ImageType.Image2D)
						{
							throw new GorgonException(GorgonResult.CannotRead, Resources.GORSPR_ERR_2D_TEXTURE_ONLY);
						}

						texture = ContentObject.Graphics.Textures.CreateTexture<GorgonTexture2D>(editorDialog.Filename, image.Image);
					}
				}

				// Update the dependency list for this sprite.
				if (dependency != null)
				{
					if (sprite.Dependencies.Contains(dependency.EditorFile, dependency.Type))
					{
						sprite.Dependencies.Remove(dependency.EditorFile, dependency.Type);
					}	
				}
				sprite.Dependencies[editorDialog.Files[0], GorgonSpriteContent.TextureDependencyType] = new Dependency(editorDialog.Files[0], GorgonSpriteContent.TextureDependencyType);
				sprite.Dependencies.CacheDependencyObject(editorDialog.Files[0], GorgonSpriteContent.TextureDependencyType, texture);

				GorgonSpriteEditorPlugIn.Settings.LastTexturePath = Path.GetDirectoryName(editorDialog.Filename).FormatDirectory('/');

				return texture;
			}
		}
		#endregion

		#region Constructor/Destructor.

		#endregion
	}
}
