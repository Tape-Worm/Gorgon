#region MIT
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
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
// Created: Wednesday, August 12, 2015 2:59:29 PM
// 
#endregion

using System.Runtime.InteropServices;

// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable InconsistentNaming
namespace Gorgon.Native
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability", "CA1900:ValueTypeFieldsShouldBePortable", MessageId = "Reserved2", Justification = "This is not meant to portable. It is used exclusively for interop only.")]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	[StructLayout(LayoutKind.Explicit)]
	unsafe struct HIDP_VALUE_CAPS
	{
		[FieldOffset(0)]
		public HIDUsagePage UsagePage;
		[FieldOffset(2)]
		public byte ReportID;
		[FieldOffset(3)]
		public byte IsAlias;
		[FieldOffset(4)]
		public ushort BitField;
		[FieldOffset(6)]
		public ushort LinkCollection;
		[FieldOffset(8)]
		public HIDUsage LinkUsage;
		[FieldOffset(10)]
		public HIDUsagePage LinkUsagePage;
		[FieldOffset(12)]
		public byte IsRange;
		[FieldOffset(13)]
		public byte IsStringRange;
		[FieldOffset(14)]
		public byte IsDesignatorRange;
		[FieldOffset(15)]
		public byte IsAbsolute;
		[FieldOffset(16)]
		public byte HasNull;
		[FieldOffset(17)]
		private byte Reserved;
		[FieldOffset(18)]
		public ushort BitSize;
		[FieldOffset(20)]
		public ushort ReportCount;
		[FieldOffset(22)]
		private fixed ushort Reserved2[5];
		[FieldOffset(32)]
		public uint UnitsExp;
		[FieldOffset(36)]
		public uint Units;
		[FieldOffset(40)]
		public int LogicalMin;
		[FieldOffset(44)]
		public int LogicalMax;
		[FieldOffset(48)]
		public int PhysicalMin;
		[FieldOffset(52)]
		public int PhysicalMax;
		[FieldOffset(56)]
		public HIDP_RANGE Range;
		[FieldOffset(56)]
		public HIDP_NOTRANGE NotRange;
	}
}