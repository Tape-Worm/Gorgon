// This control uses the color picker created by Ken Getz.  
// You can check out the article here: http://msdn.microsoft.com/msdnmag/issues/03/07/GDIColorPicker/default.aspx

using System;

namespace GorgonLibrary.Graphics.Tools
{

	internal class ColorChangedEventArgs : EventArgs
	{

		private ColorHandler.RGB mRGB;
		private ColorHandler.HSV mHSV;
		private byte _alpha;

		public ColorChangedEventArgs(ColorHandler.RGB RGB, ColorHandler.HSV HSV, byte Alpha)
		{
			mRGB = RGB;
			mHSV = HSV;
			_alpha = Alpha;
		}

		/// <summary>
		/// Property to return the alpha.
		/// </summary>
		public byte Alpha
		{
			get
			{
				return _alpha;
			}
		}

		public ColorHandler.RGB RGB
		{
			get
			{
				return mRGB;
			}
		}

		public ColorHandler.HSV HSV
		{
			get
			{
				return mHSV;
			}
		}
	}
}