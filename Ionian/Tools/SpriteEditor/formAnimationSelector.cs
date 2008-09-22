#region LGPL.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: Sunday, September 21, 2008 1:30:03 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GorgonLibrary.Graphics.Tools
{
	/// <summary>
	/// Animation selection screen.
	/// </summary>
	public partial class formAnimationSelector 
		: Form
	{
		#region Variables.
		private List<Animation> _playList = null;			// Play list.
		private Sprite _sprite = null;						// Sprite that contains the animation.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the current play list.
		/// </summary>
		public List<Animation> CurrentPlaylist
		{
			get
			{
				return _playList;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the ItemCheck event of the checkedAnimations control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.ItemCheckEventArgs"/> instance containing the event data.</param>
		private void checkedAnimations_ItemCheck(object sender, ItemCheckEventArgs e)
		{
			Animation anim = _sprite.Animations[checkedAnimations.Items[e.Index].ToString()];

			if (e.NewValue == CheckState.Checked)
			{
				if (!_playList.Contains(anim))
					_playList.Add(anim);
			}
			else
				_playList.Remove(anim);
		}

		/// <summary>
		/// Function to retrieve the list of animations from the sprite.
		/// </summary>
		/// <param name="exclude">Animation to exclude from the list.</param>
		/// <param name="sprite">Sprite to retrieve animations from.</param>
		public void GetAnimations(Animation exclude, Sprite sprite)
		{
			checkedAnimations.Items.Clear();

			_sprite = sprite;

			var drop = (from anims in _playList
						where !sprite.Animations.Contains(anims.Name)
						select anims).ToArray();

			foreach (Animation anim in drop)
				_playList.Remove(anim);

			foreach (Animation animation in sprite.Animations)
			{
				if (animation != exclude)
					checkedAnimations.Items.Add(animation.Name);
				if (_playList.Contains(animation))
					checkedAnimations.SetItemChecked(checkedAnimations.Items.Count - 1, true);
			}
		}

		/// <summary>
		/// Function to update the playlist.
		/// </summary>
		/// <param name="playList">Play list to use as the source.</param>
		public void UpdatePlaylist(List<Animation> playList)
		{
			_playList.Clear();

			foreach (Animation anim in playList)
				_playList.Add(anim);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="formAnimationSelector"/> class.
		/// </summary>
		public formAnimationSelector()
		{
			InitializeComponent();
			_playList = new List<Animation>();
		}
		#endregion
	}
}
