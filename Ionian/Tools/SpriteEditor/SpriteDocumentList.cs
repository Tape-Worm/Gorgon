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
// Created: Tuesday, May 22, 2007 4:11:02 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Linq;
using Dialogs;

namespace GorgonLibrary.Graphics.Tools
{
	/// <summary>
	/// Object representing a list of sprite documents.
	/// </summary>
	public class SpriteDocumentList
		: BaseDictionaryCollection<SpriteDocument>
	{
		#region Variables.
		private formMain _owner = null;						// Owning form.		
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return a sprite document by name.
		/// </summary>
		public SpriteDocument this[string key]
		{
			get
			{
				return GetItem(key);
			}
		}

		/// <summary>
		/// Property to return whether there are changes.
		/// </summary>
		public bool HasChanges
		{
			get
			{
				foreach (SpriteDocument document in this)
				{
					if (document.Changed)
						return true;
				}

				return false;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to add a sprite document to the list.
		/// </summary>
		/// <param name="document">Document to add.</param>
		public void Add(SpriteDocument document)
		{
			if (Contains(document.Name))
			{
				UI.ErrorBox(_owner, "The sprite '" + document.Name + "' already exists.");
				return;
			}

			AddItem(document.Name, document);
		}

		/// <summary>
		/// Function to re-bind a sprite image to another image.
		/// </summary>
		/// <param name="sourceImage">Image to rebind.</param>
		/// <param name="destinationImage">Image to bind with.</param>
		public void ReplaceImage(Image sourceImage, Image destinationImage)
		{
			foreach (SpriteDocument sprite in 
				this.Where((doc) => sourceImage == doc.Sprite.Image))
			{
				sprite.Sprite.Image = destinationImage;
				sprite.Changed = true;
			}			
		}

		/// <summary>
		/// Function to update all the sprites.
		/// </summary>
		public void Refresh()
		{
			foreach (SpriteDocument sprite in this)
				sprite.Sprite.Refresh();
		}

		/// <summary>
		/// Function to clear the sprite list.
		/// </summary>
		/// <param name="confirm">TRUE to provide confirmation, FALSE to force a clearing.</param>
		public void Clear(bool confirm)
		{
			if (confirm)
			{
				if (UI.ConfirmBox(_owner, "Are you sure to you wish to remove all the sprites?") != ConfirmationResult.Yes)
					return;
			}

			ClearItems();
		}

		/// <summary>
		/// Function to create a sprite document.
		/// </summary>
		/// <param name="name">Name of the sprite.</param>
		/// <param name="boundImage">Image to bind with.</param>
		/// <returns>A new sprite document.</returns>
		public SpriteDocument Create(string name, Image boundImage)
		{
			SpriteDocument document = null;		// New sprite.

			if (Contains(name))
			{
				UI.ErrorBox(_owner, "The sprite '" + name + "' already exists.");
				return null;
			}

			document = new SpriteDocument(name, _owner);
			document.Create(boundImage);

			AddItem(name, document);

			return document;
		}

		/// <summary>
		/// Function to load a sprite.
		/// </summary>
		/// <param name="filePath">Path and filename of the sprite.</param>
		/// <returns>A new sprite document containing the sprite from the disk.</returns>
		public SpriteDocument Load(string filePath)
		{
			SpriteDocument document = null;		// New sprite.

			document = new SpriteDocument("NotLoaded", _owner);
			document.Load(filePath);
			document.SpriteChanged += new EventHandler(_owner.SpriteManager.sprite_SpriteChanged);

			// Fix the name if we have a duplicate.
			while (Contains(document.Name))
			{
				document = (SpriteDocument)this[document.Name].Clone();
				document.SpriteChanged += new EventHandler(_owner.SpriteManager.sprite_SpriteChanged);
				document.Name = document.Name.Substring(0, document.Name.Length - 6) + ".Copy";
			}

			// Force all animations to be enabled.
			foreach (Animation anim in document.Sprite.Animations)
				anim.Enabled = true;

			// Add to list.
			AddItem(document.Name, document);

			return document;
		}

		/// <summary>
		/// Function to remove by document name.
		/// </summary>
		/// <param name="documentName">Sprite document to remove.</param>
		public void Remove(string documentName)
		{			
			if ((_owner.SpriteManager.CurrentSprite != null) && (string.Compare(documentName, _owner.SpriteManager.CurrentSprite.Name, true) == 0))
				_owner.SpriteManager.CurrentSprite = null;

			RemoveItem(documentName);
		}

		/// <summary>
		/// Function to remove a sprite document.
		/// </summary>
		/// <param name="documents">List of documents to remove.</param>
		public void Remove(ListView.SelectedListViewItemCollection documents)
		{
			ConfirmationResult result = ConfirmationResult.None;		// Confirmation result.
			string selectedSprite = string.Empty;						// Selected sprite.

			try
			{
				// Remove each selected document.
				foreach (ListViewItem item in documents)
				{
					// Keep asking until we cancel or check 'to all'.
					if ((result & ConfirmationResult.ToAll) == 0)
						result = UI.ConfirmBox(_owner, "Are you sure you wish to remove the sprite '" + item.Text + "'?", true, true);

					if (result == ConfirmationResult.Cancel)
						return;

					if ((result & ConfirmationResult.Yes) == ConfirmationResult.Yes)
					{
						if ((_owner.SpriteManager.CurrentSprite != null) && (item.Name == _owner.SpriteManager.CurrentSprite.Name))
							selectedSprite = _owner.SpriteManager.CurrentSprite.Name;

						RemoveItem(item.Name);
					}
				}

				// Remove current sprite.
				if (selectedSprite != string.Empty)
					_owner.SpriteManager.CurrentSprite = null;
			}
			catch (Exception ex)
			{
				UI.ErrorBox(_owner, "Unable to remove the selected sprites.", ex);
			}
		}

		/// <summary>
		/// Function to rename the specified document.
		/// </summary>
		/// <param name="document">Document to rename.</param>
		/// <param name="newName">New name.</param>
		public void Rename(SpriteDocument document, string newName)
		{
			if (string.IsNullOrEmpty(newName))
				return;

			if (Contains(newName))
			{
				UI.ErrorBox(_owner, "The sprite '" + newName + "' already exists.");
				return;
			}

			// Remove the previous document.
			RemoveItem(document.Name);

			// Rename.
			document.Name = newName;			

			// Re-add.
			AddItem(newName, document);

			document.Changed = true;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="owner">Form that owns this document list.</param>
		public SpriteDocumentList(formMain owner)
			: base(31, true)
		{
			_owner = owner;
		}
		#endregion
	}
}
