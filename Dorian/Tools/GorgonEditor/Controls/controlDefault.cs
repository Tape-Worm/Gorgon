#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Tuesday, May 22, 2012 12:40:12 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GorgonLibrary.GorgonEditor
{
	/// <summary>
	/// Default document page.
	/// </summary>
	public partial class controlDefault : UserControl
	{
		/// <summary>
		/// Create object event.
		/// </summary>
		public event EventHandler<CreateObjectEventArgs> CreateObject;

		/// <summary>
		/// Initializes a new instance of the <see cref="controlDefault"/> class.
		/// </summary>
		public controlDefault()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Function to call the create object event.
		/// </summary>
		/// <param name="type">Type of object to create.</param>
		private void OnCreateObject(CreateType type)
		{
			if (CreateObject != null)
				CreateObject(this, new CreateObjectEventArgs(type));
		}

		/// <summary>
		/// Handles the Click event of the buttonCreateFont control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonCreateFont_Click(object sender, EventArgs e)
		{
			OnCreateObject(CreateType.Font);
		}
	}

	/// <summary>
	/// Type of object to create.
	/// </summary>
	public enum CreateType
	{
		/// <summary>
		/// Font object.
		/// </summary>
		Font = 1
	}

	/// <summary>
	/// Event arguments for the create object event.
	/// </summary>
	public class CreateObjectEventArgs
		: EventArgs
	{
		#region Properties.
		/// <summary>
		/// Property to return the type of object to create.
		/// </summary>
		public CreateType ObjectType
		{
			get;
			private set;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="CreateObjectEventArgs"/> class.
		/// </summary>
		/// <param name="objectType">Type of the object.</param>
		public CreateObjectEventArgs(CreateType objectType)
		{
			ObjectType = objectType;
		}
		#endregion
	}
}
