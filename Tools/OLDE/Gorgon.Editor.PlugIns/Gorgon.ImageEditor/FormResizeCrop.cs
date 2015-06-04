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
// Created: Sunday, October 26, 2014 10:54:27 PM
// 
#endregion

using System;
using System.Drawing;
using Gorgon.Editor.ImageEditorPlugIn.Properties;
using Gorgon.IO;
using Gorgon.UI;

namespace Gorgon.Editor.ImageEditorPlugIn
{
	/// <summary>
	/// UI to determine whether to crop or resize an image.
	/// </summary>
	partial class FormResizeCrop 
		: FlatForm
	{
		#region Value Types.
		/// <summary>
		/// Item for a filter combo box.
		/// </summary>
		private struct FilterComboItem
			: IEquatable<FilterComboItem>, IComparable
		{
			#region Variables.
			/// <summary>
			/// The filter.
			/// </summary>
			public readonly ImageFilter Filter;
			#endregion

			#region Methods.
			/// <summary>
			/// Function to compare two instances for equality.
			/// </summary>
			/// <param name="left">The left instance to compare.</param>
			/// <param name="right">The right instance to compare.</param>
			/// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
			private static bool Equals(ref FilterComboItem left, ref FilterComboItem right)
			{
				return (left.Filter == right.Filter);
			}

			/// <summary>
			/// Determines whether the specified <see cref="System.Object"/>, is equal to this instance.
			/// </summary>
			/// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
			/// <returns>
			///   <b>true</b> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <b>false</b>.
			/// </returns>
			public override bool Equals(object obj)
			{
				if (obj is FilterComboItem)
				{
					return ((FilterComboItem)obj).Equals(this);
				}

				return base.Equals(obj);
			}

			/// <summary>
			/// Returns a hash code for this instance.
			/// </summary>
			/// <returns>
			/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
			/// </returns>
			public override int GetHashCode()
			{
				return Filter.GetHashCode();
			}

			/// <summary>
			/// Returns a <see cref="System.String" /> that represents this instance.
			/// </summary>
			/// <returns>
			/// A <see cref="System.String" /> that represents this instance.
			/// </returns>
			public override string ToString()
			{
				switch (Filter)
				{
					case ImageFilter.Linear:
						return Resources.GORIMG_TEXT_FILTER_LINEAR;
					case ImageFilter.Cubic:
						return Resources.GORIMG_TEXT_FILTER_CUBIC;
					case ImageFilter.Fant:
						return Resources.GORIMG_TEXT_FILTER_FANT;
					default:
						return Resources.GORIMG_TEXT_FILTER_POINT;
				}
			}
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="FilterComboItem"/> struct.
			/// </summary>
			/// <param name="filter">The filter.</param>
			public FilterComboItem(ImageFilter filter)
			{
				Filter = filter;
			}
			#endregion

			#region IEquatable<FilterComboItem> Members
			/// <summary>
			/// Indicates whether the current object is equal to another object of the same type.
			/// </summary>
			/// <param name="other">An object to compare with this object.</param>
			/// <returns>
			/// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
			/// </returns>
			public bool Equals(FilterComboItem other)
			{
				return Equals(ref this, ref other);
			}
			#endregion

			#region IComparable Members
			/// <summary>
			/// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
			/// </summary>
			/// <param name="obj">An object to compare with this instance.</param>
			/// <returns>
			/// A value that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance precedes <paramref name="obj" /> in the sort order. Zero This instance occurs in the same position in the sort order as <paramref name="obj" />. Greater than zero This instance follows <paramref name="obj" /> in the sort order.
			/// </returns>
			int IComparable.CompareTo(object obj)
			{
				var other = (FilterComboItem)obj;

				if (Filter < other.Filter)
				{
					return -1;
				}

				return Filter > other.Filter ? 1 : 0;
			}
			#endregion
		}
		#endregion

		#region Variables.
		// Flag to indicate that we are not cropping the image.
		private readonly bool _noCrop;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the anchor for the image.
		/// </summary>
		public ContentAlignment ImageAnchor
		{
			get
			{
				if (radioTopLeft.Checked)
				{
					return ContentAlignment.TopLeft;
				}

				if (radioTopCenter.Checked)
				{
					return ContentAlignment.TopCenter;
				}

				if (radioTopRight.Checked)
				{
					return ContentAlignment.TopRight;
				}

				if (radioMiddleLeft.Checked)
				{
					return ContentAlignment.MiddleLeft;
				}

				if (radioMiddleRight.Checked)
				{
					return ContentAlignment.MiddleRight;
				}

				if (radioBottomLeft.Checked)
				{
					return ContentAlignment.BottomLeft;
				}

				if (radioBottomCenter.Checked)
				{
					return ContentAlignment.BottomCenter;
				}

				return radioBottomRight.Checked ? ContentAlignment.BottomRight : ContentAlignment.MiddleCenter;
			}
			private set
			{
				switch (value)
				{
					case ContentAlignment.TopLeft:
						radioTopLeft.Checked = true;
						break;
					case ContentAlignment.TopCenter:
						radioTopCenter.Checked = true;
						break;
					case ContentAlignment.TopRight:
						radioTopRight.Checked = true;
						break;
					case ContentAlignment.MiddleLeft:
						radioMiddleLeft.Checked = true;
						break;
					case ContentAlignment.MiddleRight:
						radioMiddleRight.Checked = true;
						break;
					case ContentAlignment.BottomLeft:
						radioBottomLeft.Checked = true;
						break;
					case ContentAlignment.BottomCenter:
						radioBottomCenter.Checked = true;
						break;
					case ContentAlignment.BottomRight:
						radioBottomRight.Checked = true;
						break;
					default:
						radioCenter.Checked = true;
						break;
				}
			}
		}

		/// <summary>
        /// Property to return whether to preserve the aspect ratio for the image.
        /// </summary>
	    public bool PreserveAspectRatio
	    {
	        get
	        {
	            return checkPreserveAspect.Checked;
	        }
	    }

		/// <summary>
		/// Property to return the selected filter.
		/// </summary>
		public ImageFilter Filter
		{
			get
			{
				return comboFilter.SelectedItem == null ? ImageFilter.Point : ((FilterComboItem)comboFilter.SelectedItem).Filter;
			}
		}

		/// <summary>
		/// Property to return the flag that indicates whether to crop or resize the image.
		/// </summary>
		public bool CropImage
		{
			get
			{
				return radioCrop.Checked;
			}
		}
		#endregion

		#region Methods.
        /// <summary>
        /// Handles the Click event of the buttonOK control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void buttonOK_Click(object sender, EventArgs e)
        {
            GorgonImageEditorPlugIn.Settings.ResizeImageFilter = Filter;
            GorgonImageEditorPlugIn.Settings.PreserveAspectRatio = checkPreserveAspect.Checked;
            GorgonImageEditorPlugIn.Settings.CropDefault = radioCrop.Checked;
	        GorgonImageEditorPlugIn.Settings.ImageAlign = ImageAnchor;
        }

		/// <summary>
		/// Handles the CheckedChanged event of the radioCrop control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void radioCrop_CheckedChanged(object sender, EventArgs e)
		{
			ValidateControls();
		}

		/// <summary>
		/// Function to validate the controls on the form.
		/// </summary>
		private void ValidateControls()
		{
			checkPreserveAspect.Enabled = comboFilter.Enabled = labelFilter.Enabled = radioResize.Checked;
		}

		/// <summary>
		/// Function to localize the controls on the form.
		/// </summary>
		private void LocalizeControls()
		{
			if (!_noCrop)
			{
				Text = Resources.GORIMG_DLG_CAPTION_RESIZE_CROP;
				labelDesc.Text = Resources.GORIMG_TEXT_CROP_RESIZE_TEXT;
			}
			else
			{
				Text = Resources.GORIMG_DLG_CAPTION_RESIZE_KEEP;
				labelDesc.Text = Resources.GORIMG_TEXT_KEEP_RESIZE_TEXT;
			}

			buttonOK.Text = Resources.GORIMG_ACC_TEXT_OK;
			buttonCancel.Text = Resources.GORIMG_ACC_TEXT_CANCEL;

			labelFilter.Text = string.Format("{0}:", Resources.GORIMG_TEXT_FILTER);
			labelAnchor.Text = string.Format("{0}:", Resources.GORIMG_TEXT_ANCHOR);

		    checkPreserveAspect.Text = Resources.GORIMG_TEXT_PRESERVE_ASPECT;
			
			comboFilter.Items.Add(new FilterComboItem(ImageFilter.Point));
			comboFilter.Items.Add(new FilterComboItem(ImageFilter.Linear));
			comboFilter.Items.Add(new FilterComboItem(ImageFilter.Cubic));
			comboFilter.Items.Add(new FilterComboItem(ImageFilter.Fant));
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load" /> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			LocalizeControls();

			radioCrop.Checked = GorgonImageEditorPlugIn.Settings.CropDefault;
			radioResize.Checked = !GorgonImageEditorPlugIn.Settings.CropDefault;
			comboFilter.SelectedItem = new FilterComboItem(GorgonImageEditorPlugIn.Settings.ResizeImageFilter);
		    checkPreserveAspect.Checked = GorgonImageEditorPlugIn.Settings.PreserveAspectRatio;
			ImageAnchor = GorgonImageEditorPlugIn.Settings.ImageAlign;

			ValidateControls();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="FormResizeCrop"/> class.
		/// </summary>
		/// <param name="filePath">The file path.</param>
		/// <param name="sourceSize">Size of the source.</param>
		/// <param name="destSize">Size of the dest.</param>
		public FormResizeCrop(string filePath, Size sourceSize, Size destSize)
			: this()
		{
			_noCrop = ((destSize.Width > sourceSize.Width)
			           && (destSize.Height > sourceSize.Height));
			labelFilePath.Text = string.Format(Resources.GORIMG_TEXT_IMAGE_FILE_PATH, filePath);
			labelSourceDimensions.Text = string.Format(Resources.GORIMG_TEXT_SRC_DIMENSIONS, sourceSize.Width, sourceSize.Height);
			labelDestDimensions.Text = string.Format(Resources.GORIMG_TEXT_DEST_DIMENSIONS, destSize.Width, destSize.Height);
			radioCrop.Text = !_noCrop ? string.Format(Resources.GORIMG_TEXT_CROP_TO, destSize.Width, destSize.Height) : Resources.GORIMG_TEXT_NO_CHANGE_SIZE;

			radioResize.Text = string.Format(Resources.GORIMG_TEXT_RESIZE_TO, destSize.Width, destSize.Height);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FormResizeCrop"/> class.
		/// </summary>
		public FormResizeCrop()
		{
			InitializeComponent();
		}
		#endregion
	}
}
