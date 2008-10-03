#region MIT.
// 
// Gorgon.
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
// Created: Monday, November 12, 2007 2:17:33 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Linq;
using Dialogs;

namespace GorgonLibrary.Graphics.Tools
{
	/// <summary>
	/// Interface for sprite frame importer.
	/// </summary>
	public partial class formListAll : Form
	{
		#region Variables.
		private System.Windows.Forms.ImageList _images = null;					// Image list.		
		private SpriteDocumentList _spriteList = null;							// Sprite list.
		private Track _currentTrack = null;										// Current track.
		private float _animLength = 0.0f;										// Animation length.
		private Sprite _owner = null;											// Sprite that owns this animation.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the sprite that owns this animation.
		/// </summary>
		public Sprite SpriteOwner
		{
			get
			{
				return _owner;
			}
			set
			{
				_owner = value;
			}
		}

		/// <summary>
		/// Property to set or return the animation length.
		/// </summary>
		public float AnimationLength
		{
			get
			{
				return _animLength;
			}
			set
			{
				_animLength = value;
			}
		}

		/// <summary>
		/// Property to set or return the current track.
		/// </summary>
		public Track CurrentTrack
		{
			get
			{
				return _currentTrack;
			}
			set
			{
				_currentTrack = value;
			}
		}

		/// <summary>
		/// Property to set or return the image frame list.
		/// </summary>
		public System.Windows.Forms.ImageList Frames
		{
			get
			{
				return _images;
			}
			set
			{
				_images = value;
			}
		}

		/// <summary>
		/// Property to set or return the image list.
		/// </summary>
		public SpriteDocumentList SpriteList
		{
			get
			{
				return _spriteList;
			}
			set
			{
				_spriteList = value;
			}
		}

		/// <summary>
		/// Property to set or return the delay interval.
		/// </summary>
		public decimal Interval
		{
			get
			{
				return numericDelay.Value;
			}
			set
			{
				numericDelay.Minimum = value;
				numericDelay.Value = value;
				numericDelay.Increment = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the SelectedIndexChanged event of the listSprites control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void listSprites_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				if (listSprites.SelectedItems.Count > 0)
					pictureSprite.Image = _images.Images[listSprites.SelectedItems[0].Name + ".@Image"];
				else
					pictureSprite.Image = null;
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, "Unable to get the selected image", ex);
			}
			finally
			{
				ValidateForm();
			}
		}

		/// <summary>
		/// Handles the Click event of the buttonOK control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonOK_Click(object sender, EventArgs e)
		{
			float startTime = 0.0f;					// Starting time.
			SpriteDocument sprite;					// Sprite we're adding.
			int frameCount = 0;						// Number of frames.
			float interval = 0.0f;					// Time interval.

			Cursor.Current = Cursors.WaitCursor;

			try
			{
				_currentTrack.ClearKeys();
				var items = from ListViewItem listItems in listSprites.Items
							where listItems.Checked
							select listItems;
				frameCount = items.Count();

				interval = (float)numericDelay.Value;
				if (frameCount != listSprites.Items.Count)
				{
					if (UI.ConfirmBox("The selected frame count differs from the total frame count.  This will cause a delay at the end of the animation.\nWould you like to recalculate the interval?") == ConfirmationResult.Yes)
						interval = _animLength / (float)frameCount;
				}


				// Go through each item and add a key.
				foreach (ListViewItem item in items)
				{
					KeyImage key = null;			// Image key.

					sprite = _spriteList[item.Name];
					key = new KeyImage(startTime, sprite.Sprite.Image);
					key.ImageOffset = sprite.ImageLocation;
					key.ImageSize = sprite.Size;
					_currentTrack.AddKey(key);
					startTime += interval;
				}				
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, "Error trying to retrieve the frames.", ex);
			}
			finally
			{
				Cursor.Current = Cursors.Default;
			}
		}

		/// <summary>
		/// Function to validate the interface.
		/// </summary>
		private void ValidateForm()
		{
			buttonOK.Enabled = (from ListViewItem listItems in listSprites.Items
							   where listItems != null && listItems.Checked
							   select listItems).Count() > 0;
		}

		/// <summary>
		/// Handles the ItemChecked event of the listSprites control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.ItemCheckedEventArgs"/> instance containing the event data.</param>
		private void listSprites_ItemChecked(object sender, ItemCheckedEventArgs e)
		{
			ValidateForm();
		}
	
		/// <summary>
		/// Function to refresh the list.
		/// </summary>
		public void RefreshList()
		{
			ListViewItem item = null;		// List view item.

			listSprites.BeginUpdate();
			listSprites.Items.Clear();

			// Add images.
			var sprites = _spriteList.Where((sprite) => _owner.Name != sprite.Sprite.Name);
			foreach (SpriteDocument image in sprites)
			{
				item = new ListViewItem(image.Name);
				item.SubItems.Add(image.Sprite.Width.ToString("0.0") + "x" + image.Sprite.Height.ToString("0.0"));
				item.Name = image.Name;
				item.Checked = true;
				listSprites.Items.Add(item);
			}
						
			listSprites.EndUpdate();
			listSprites.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

			ValidateForm();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="formListAll"/> class.
		/// </summary>
		public formListAll()
		{
			InitializeComponent();
		}
		#endregion		
	}
}