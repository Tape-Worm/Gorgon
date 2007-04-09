/// This class is (c)2004 by David John, davadk@gmail.com
/// permission is hereby granted to use this class in non-profit
/// applications.
/// This code is not recommended for any critical purpose.
/// The lines below has been developed as part of learning-experiement
/// and has not been tested thorougly nor has it been optimzed or
/// modelled strictly after any "good coding standards".


using System;
using System.Runtime.InteropServices;

namespace dk.dava.FontSmoothing
{
	/// <summary>
	/// <b>FontSmoothingTypes</b> Specifies fontsmoothing/antialiasing.
	/// </summary>
	public enum FontSmoothingTypes
	{
		FE_FONTSMOOTHINGSTANDARD = 1,	// 0x0001;
		FE_FONTSMOOTHINGCLEARTYPE = 2	// 0x0002;
	}

	// COMMENT: MW - Aug 5/06
	// Modified to save and restore smoothing settings.
	// Removed the extra constructors, not necessary.

	/// <summary>
	/// <b>ClearTypeController</b> Wrapper class for Win32 FontSmoothing API. Use Redraw() to perform instant changes.
	/// </summary>
	public class FontSmoothingController
		: IDisposable 
	{
		#region DllImport statements
		//RedrawWindow(0, NULL, 0, RDW_ERASE  | RDW_FRAME | RDW_ALLCHILDREN | RDW_INTERNALPAINT | RDW_INVALIDATE | RDW_ERASENOW | RDW_UPDATENOW);
		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool RedrawWindow(int hWnd, int lprcUpdate, int hrgnUpdate, [MarshalAs(UnmanagedType.U4)] int flags);
		// SPI_SETFONTSMOOTHING
		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern int SystemParametersInfo(int uiAction, bool uiParam, int pvParam, int fWinIni);
		// SPI_GETFONTSMOOTHING
		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern int SystemParametersInfo(int uiAction, int uiParam, ref bool pvParam, int fWinIni);
		// SPI_SETFONTSMOOTHINGTYPE &&  SPI_SETFONTSMOOTHINGCONTRAST
		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern int SystemParametersInfo(int uiAction, int uiParam, int pvParam, int fWinIni);
		// SPI_GETFONTSMOOTHINGTYPE &&  SPI_GETFONTSMOOTHINGCONTRAST
		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern int SystemParametersInfo(int uiAction, int uiParam, ref int pvParam, int fWinIni);
		#endregion

		#region Win32 API constants
		private const int RDW_INVALIDATE = 1;	// 0x1;
		private const int RDW_INTERNALPAINT = 2;	// 0x2;
		private const int RDW_ERASE = 4;	// 0x4;
		private const int RDW_ALLCHILDREN = 128;	// 0x80;
		private const int RDW_UPDATENOW = 256;	// 0x100;
		private const int RDW_ERASENOW = 512;	// 0x200;
		private const int RDW_FRAME = 1024;	// 0x400;

		private const int SPI_GETFONTSMOOTHING = 74;	// 0x004A;
		private const int SPI_SETFONTSMOOTHING = 75;	// 0x004B;
		private const int SPI_GETFONTSMOOTHINGTYPE = 8202;	// 0x200A;
		private const int SPI_SETFONTSMOOTHINGTYPE = 8203;	// 0x200B;
		private const int SPI_GETFONTSMOOTHINGCONTRAST = 8204;	// 0x200C;
		private const int SPI_SETFONTSMOOTHINGCONTRAST = 8205;	// 0x200D;

		private const int SPIF_UPDATEINIFILE = 1; // 0x1;
		private const int SPIF_SENDCHANGE = 2; // 0x2;
		private const int SPIF_SENDWININICHANGE = 2; // 0x2;
		private const int SPIF_TELLALL = SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE; // 0x1 | 0x2;
		#endregion

		private static bool oldFontSmoothing;		// reference variable for SPI_GETFONTSMOOTHING 
		private static int oldFontSmoothingType;	// reference variable for SPI_GETFONTSMOOTHINGTYPE 
		private static int oldFontSmoothingContrast;// reference variable for SPI_GETFONTSMOOTHINGCONTRAST 

		private static bool fontSmoothing;			// reference variable for SPI_GETFONTSMOOTHING 
		private static int fontSmoothingType;		// reference variable for SPI_GETFONTSMOOTHINGTYPE 
		private static int fontSmoothingContrast;	// reference variable for SPI_GETFONTSMOOTHINGCONTRAST 

		/// <summary>FontSmoothingController constructor.
		/// Creates an instance of FontSmoothingController
		/// </summary>
		public FontSmoothingController()
		{
			// Get previous values.
			oldFontSmoothing = this.FontSmoothing;
			oldFontSmoothingContrast = this.FontSmoothingContrast;
			oldFontSmoothingType = (int)this.FontSmoothingType;
		}

		// COMMENT: MW Aug 5/06 - Added reset function.
		/// <summary>
		/// Function to reset the font smoothing settings.
		/// </summary>
		public void Reset()
		{			
			this.FontSmoothing = oldFontSmoothing;
			this.FontSmoothingContrast = oldFontSmoothingContrast;
			this.FontSmoothingType = (FontSmoothingTypes)oldFontSmoothingType;
		}

		/// <summary>Gets or sets a value indicating whether FontSmoothing is to be enabled or disabled.</summary>
		public bool FontSmoothing
		{
			get
			{
				SystemParametersInfo(SPI_GETFONTSMOOTHING, 0, ref fontSmoothing, 0);
				return fontSmoothing;
			}
			set
			{
				bool val = value;
				SystemParametersInfo(SPI_SETFONTSMOOTHING, value, 0, SPIF_SENDWININICHANGE | SPIF_UPDATEINIFILE);
			}
		}

		/// <summary>Gets or sets an enum value indicating the kind of FontSmoothing to use.</summary>
		public FontSmoothingTypes FontSmoothingType
		{
			get
			{
				SystemParametersInfo(SPI_GETFONTSMOOTHINGTYPE, 0, ref fontSmoothingType, 0);
				return (FontSmoothingTypes)fontSmoothingType;
			}
			set
			{
				int val = (int)value;
				SystemParametersInfo(SPI_SETFONTSMOOTHINGTYPE, 0, val, SPIF_SENDWININICHANGE | SPIF_UPDATEINIFILE);
			}
		}

		/// <summary>Gets or sets an value in the range 1000 - 2200 indicating the amount of FontSmoothingContrast to apply. Values above and below are reverted to default value (1400)</summary>
		public int FontSmoothingContrast
		{
			get
			{
				SystemParametersInfo(SPI_GETFONTSMOOTHINGCONTRAST, 0, ref fontSmoothingContrast, 0);
				return fontSmoothingContrast;
			}
			set
			{
				int val = value;

				if (val > 2200)
					val = 1400;

				if (val < 1000)
					val = 1400;

				SystemParametersInfo(SPI_SETFONTSMOOTHINGCONTRAST, 0, val, SPIF_SENDWININICHANGE | SPIF_UPDATEINIFILE);
			}
		}

		/// <summary>Redraw the Windows desktop area and active windows. Returns true on success and false if it fails.</summary>
		public bool Redraw()
		{
			bool status = RedrawWindow(0, 0, 0, RDW_ERASE | RDW_FRAME | RDW_ALLCHILDREN | RDW_INTERNALPAINT | RDW_INVALIDATE | RDW_ERASENOW | RDW_UPDATENOW);
			return status;
		}

		#region IDisposable Members
		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		/// <param name="disposing">TRUE to release all resources, FALSE to only release unmanaged.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				// Restore the font smoothing.
				this.FontSmoothing = oldFontSmoothing;
				this.FontSmoothingContrast = oldFontSmoothingContrast;
				this.FontSmoothingType = (FontSmoothingTypes)oldFontSmoothingType;
				Redraw();
			}
		}

		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}