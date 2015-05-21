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
// Created: Thursday, May 31, 2012 9:08:49 AM
// 
// This code was adapted from the stack overflow answer at http://stackoverflow.com/questions/254129/how-to-i-display-a-sort-arrow-in-the-header-of-a-list-view-column-using-c
#endregion

using System;
using System.Windows.Forms;
using Gorgon.Core;
using GorgonLibrary.Native;
using Gorgon.Core.Properties;

namespace Gorgon.UI
{
	/// <summary>
	/// Extensions used for the list view object.
	/// </summary>
	public static class GorgonListViewExtensions
	{
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
			IntPtr columnHeader = Win32API.SendMessage(listViewControl.Handle, (uint)ListViewMessages.LVM_GETHEADER, IntPtr.Zero, IntPtr.Zero);

			for (int columnNumber = 0; columnNumber < listViewControl.Columns.Count; columnNumber++)
			{
				var columnPtr = new IntPtr(columnNumber);
				var item = new HDITEM
				{
					mask = HeaderMask.Format
				};

				if (Win32API.SendMessage(columnHeader, (uint)HeaderMessages.HDM_GETITEM, columnPtr, ref item) == IntPtr.Zero)
				{
					throw new GorgonException(GorgonResult.CannotEnumerate, Resources.GOR_LISTVIEW_CANNOT_FIND_HEADER);
				}

				if (order != SortOrder.None && columnNumber == headerIndex)
				{
					switch (order)
					{
						case SortOrder.Ascending:
							item.fmt &= ~HeaderFormat.SortDown;
							item.fmt |= HeaderFormat.SortUp;
							break;
						case SortOrder.Descending:
							item.fmt &= ~HeaderFormat.SortUp;
							item.fmt |= HeaderFormat.SortDown;
							break;
					}
				}
				else
				{
					item.fmt &= ~HeaderFormat.SortDown & ~HeaderFormat.SortUp;
				}

				if (Win32API.SendMessage(columnHeader, (uint)HeaderMessages.HDM_SETITEM, columnPtr, ref item) == IntPtr.Zero)
				{
					throw new GorgonException(GorgonResult.CannotWrite, Resources.GOR_LISTVIEW_CANNOT_UPDATE_COLUMN);
				}
			}
		}
	}
}
