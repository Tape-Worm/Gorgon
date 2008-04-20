#region LGPL.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
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
		private TrackFrame _currentTrack = null;								// Current track.
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
		public TrackFrame CurrentTrack
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
		/// Function to validate the interface.
		/// </summary>
		private void ValidateForm()
		{
			buttonOK.Enabled = false;
			foreach (ListViewItem item in listSprites.Items)
			{
				if (item.Checked)
					buttonOK.Enabled = true;
			}
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
			foreach (SpriteDocument image in _spriteList)
			{
				if (_owner.Name != image.Sprite.Name)
				{
					item = new ListViewItem(image.Name);
					item.SubItems.Add(image.Sprite.Width.ToString("0.0") + "x" + image.Sprite.Height.ToString("0.0"));
					item.Name = image.Name;
					item.Checked = true;
					listSprites.Items.Add(item);
				}
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
				foreach (ListViewItem item in listSprites.Items)
				{
					if (item.Checked)
						frameCount++;
				}

				interval = (float)numericDelay.Value;
				if (frameCount != listSprites.Items.Count)
				{
					if (UI.ConfirmBox("The selected frame count differs from the total frame count.  This will cause a delay at the end of the animation.\nWould you like to recalculate the interval?") == ConfirmationResult.Yes)
						interval = _animLength / (float)frameCount;
				}
					

				// Go through each item and add a key.
				foreach (ListViewItem item in listSprites.Items)
				{
					if (item.Checked)
					{
						sprite = _spriteList[item.Name];
						_currentTrack.CreateKey(startTime, new Frame(sprite.Sprite.Image, sprite.Sprite.ImageOffset, sprite.Sprite.Size));						
						startTime += interval;
					}
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
	}
}