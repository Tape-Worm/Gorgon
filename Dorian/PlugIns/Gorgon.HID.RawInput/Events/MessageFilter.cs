#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Friday, June 24, 2011 10:04:46 AM
// 
#endregion

using System;
using GorgonLibrary.Native;

namespace GorgonLibrary.HID.RawInput
{
	/// <summary>
	/// Object representing a message loop filter.
	/// </summary>
	internal class MessageFilter
		: System.Windows.Forms.IMessageFilter, IDisposable
	{
		#region Events.
		/// <summary>
		/// Event fired when a raw input event occours.
		/// </summary>
		public event EventHandler<RawInputEventArgs> RawInputData = null;
		#endregion

		#region Variables.
		private bool _isDisposed = false;					// Flag to indicate that the object has been disposed.
		private RawInputData _data = null;					// Raw input data.
		#endregion

		#region IMessageFilter Members
		/// <summary>
		/// Filters out a message before it is dispatched.
		/// </summary>
		/// <param name="m">The message to be dispatched. You cannot modify this message.</param>
		/// <returns>
		/// true to filter the message and stop it from being dispatched; false to allow the message to continue to the next filter or control.
		/// </returns>
		public bool PreFilterMessage(ref System.Windows.Forms.Message m)
		{
			// Handle raw input messages.
			if ((WindowMessages)m.Msg == WindowMessages.RawInput)
			{
				if (_data == null)
					_data = new RawInputData();

				_data.GetRawInputData(m.LParam);
				if (RawInputData != null)
					RawInputData(this, new RawInputEventArgs(_data));
			}

			return false;
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			if (!_isDisposed)
			{
				if (disposing)
					_data.Dispose();
				_data = null;
			}
			_isDisposed = true;
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			
		}
		#endregion
	}
}
