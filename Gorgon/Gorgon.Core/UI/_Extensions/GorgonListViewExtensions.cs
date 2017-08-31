﻿#region MIT.
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
// Created: Thursday, May 31, 2012 9:08:49 AM
// 
// This code was adapted from the stack overflow answer at http://stackoverflow.com/questions/254129/how-to-i-display-a-sort-arrow-in-the-header-of-a-list-view-column-using-c
#endregion

using System;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Core.Properties;
using Gorgon.Native;

namespace Gorgon.UI
{
	/// <summary>
	/// Extensions used for the list view object.
	/// </summary>
	public static class GorgonListViewExtensions
	{
		#region Constants.
		// List view message to return the header.
		private const uint LvmGetHeader = 0x101F;
		// Header message used to retrieve an item.
		private const uint HdmGetItem = 0x120B;
		// Header message used to set an item.
		private const uint HdmSetItem = 0x120C;
		// Mask for header format format
		private const int HeaderFormatMask = 0x4;
		// Flag to make the column sort ascending.
		private const int HeaderSortDown = 0x200;
		// Flag to make the column sort descending.
		private const int HeaderSortUp = 0x400;
		#endregion

		/// <summary>
		/// Function to set the sorting icon on the list view control.
		/// </summary>
		/// <param name="listViewControl">Listview to update.</param>
		/// <param name="headerIndex">Column header index.</param>
		/// <param name="order">Sort order.</param>
		/// <exception cref="GorgonException">Thrown if the column header was not found or could not be updated.</exception>
		/// <remarks>Use this extension method to set a sorting icon for the specified column.  This will give users a clue as to how the 
		/// list view is sorted.</remarks>
		public static void SetSortIcon(this ListView listViewControl, int headerIndex, SortOrder order)
		{
			IntPtr columnHeader = UserApi.SendMessage(listViewControl.Handle, LvmGetHeader, IntPtr.Zero, IntPtr.Zero);

			for (int columnNumber = 0; columnNumber < listViewControl.Columns.Count; columnNumber++)
			{
				IntPtr columnPtr = new IntPtr(columnNumber);
				HDITEM item = new HDITEM
				{
					mask = HeaderFormatMask
				};

				if (UserApi.SendMessage(columnHeader, HdmGetItem, columnPtr, ref item) == IntPtr.Zero)
				{
					throw new GorgonException(GorgonResult.CannotEnumerate, Resources.GOR_ERR_LISTVIEW_CANNOT_FIND_HEADER);
				}

				if (order != SortOrder.None && columnNumber == headerIndex)
				{
					switch (order)
					{
						case SortOrder.Ascending:
							item.fmt &= ~HeaderSortDown;
							item.fmt |= HeaderSortUp;
							break;
						case SortOrder.Descending:
							item.fmt &= ~HeaderSortUp;
							item.fmt |= HeaderSortDown;
							break;
					}
				}
				else
				{
					item.fmt &= ~HeaderSortDown & ~HeaderSortUp;
				}

				if (UserApi.SendMessage(columnHeader, HdmSetItem, columnPtr, ref item) == IntPtr.Zero)
				{
					throw new GorgonException(GorgonResult.CannotWrite, Resources.GOR_ERR_LISTVIEW_CANNOT_UPDATE_COLUMN);
				}
			}
		}
	}
}
