using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using GorgonLibrary;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Data;

namespace Test_DataStream
{
	class Program
	{
		private static GorgonTimer _timer = null;
		private static Random _rnd = new Random();

		public struct Test
		{
			public int x;
			public int y;
			public int z;
			public int color;
			public int u;
			public int v;

			public override int GetHashCode()
			{
				return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode() ^ color.GetHashCode() ^ u.GetHashCode() ^ v.GetHashCode();
			}

			public override bool Equals(object obj)
			{
				if (obj is Test)
				{
					Test left = (Test)obj;

					return ((left.x == this.x) && (left.y == this.y) && (left.z == this.z) && (left.color == this.color) && (left.u == this.u) && (left.v == this.v));
				}

				return false;
			}

			public static bool operator ==(Test left, Test right)
			{
				return ((left.x == right.x) && (left.y == right.y) && (left.z == right.z) && (left.color == right.color) && (left.u == right.u) && (left.v == right.v));
			}

			public static bool operator !=(Test left, Test right)
			{
				return !(left == right);
			}
		}

		private static long size = 0;

		static bool CheckCRC(Test[] source, Test[] crc)
		{
			for (int i = 0; i < crc.Length; i++)
			{
				if (source[i] != crc[i])
					return false;
			}

			return true;
		}

		static void Main(string[] args)
		{
			Console.WriteLine("Press a key to start.");
			Console.ReadKey();

			Gorgon.Initialize();
			long byteSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Test));
			long maxSize = Gorgon.AvailablePhysicalRAM;

			if (Gorgon.PlatformArchitecture == PlatformArchitecture.x86)
			{
				// Limit to 2GB per process on x86.
				if (maxSize > Int32.MaxValue)
					maxSize = Int32.MaxValue;
			}

			size = ((long)((double)maxSize * 0.75) / 4);

			if (size >= Int32.MaxValue)
				size = Int32.MaxValue;

			size = (size / byteSize) - byteSize;

			//size = 80000000;

			Test[] crc = new Test[size];
			Test[] items = new Test[size];

			Console.WriteLine("Arrays allocated.");

			Console.Write("Creating {0}...", (size * byteSize).FormatMemory());
			_timer = new GorgonTimer();
			for (int i = 0; i < items.Length; i++)
			{
				Test testItem = new Test();
				testItem.x = _rnd.Next();
				testItem.y = _rnd.Next();
				testItem.z = _rnd.Next();
				testItem.color = _rnd.Next();
				testItem.u = _rnd.Next();
				testItem.v = _rnd.Next();

				items[i] = testItem;
				crc[i] = testItem;
			}			
			Console.WriteLine(": {0:N3}ms", _timer.Milliseconds);

			GorgonDataStream newStream = new GorgonDataStream((int)(byteSize * size));

			try
			{
				Console.Write("Writing {0:N0} structures of {1} bytes individually ({2})...", size, byteSize, (size * byteSize).FormatMemory());

				_timer.Reset();
				for (int i = 0; i < items.Length; i++)
				{
					newStream.Write<Test>(items[i]);

				}
				Console.WriteLine(": {0:N3}ms", _timer.Milliseconds);

				newStream.Position = 0;

				Console.Write("Reading {0:N0} structures of {1} bytes individually ({2})...", size, byteSize, (size * byteSize).FormatMemory());
				_timer.Reset();
				for (int i = 0; i < items.Length; i++)
				{
					items[i] = newStream.Read<Test>();
				}
				Console.WriteLine(": {0:N3}ms", _timer.Milliseconds);

				if (!CheckCRC(items, crc))
				{
					Console.WriteLine("CRC does not match.");
				}

				//newStream.Position = 0;
				//System.IO.FileStream fs = System.IO.File.Open(@"d:\unpak\bigfile.bin", System.IO.FileMode.Create);
				//newStream.CopyTo(fs);
				//fs.Close();

				newStream.Position = 0;

				Console.Write("Writing {0} at once...", (size * byteSize).FormatMemory());
				_timer.Reset();
				newStream.WriteRange<Test>(items);

				Console.WriteLine(": {0:N3}ms", _timer.Milliseconds);

				newStream.Position = 0;

				Console.Write("Reading {0} at once...", (size * byteSize).FormatMemory());
				_timer.Reset();
				items = newStream.ReadRange<Test>((int)size);

				Console.WriteLine(": {0:N3}ms", _timer.Milliseconds);

				if (!CheckCRC(items, crc))
				{
					Console.WriteLine("CRC does not match.");
				}

				// Clear memory.
				items = new Test[0];
				crc = new Test[0];

				newStream.Position = 0;
				newStream.WriteString("C#'s BinaryReader has a function that according to MSDN, reads an integer encoded as $seven bit integer$, and then reads a string with the length of this integer.Is there a clear documentation for the seven bit integer format (I have a rough understanding that the MSB or the LSB marks whether there are more bytes to read, and the rest bits are the data, but I'll be glad for something more exact). Even better, is there a C implementation for reading and writing numbers in this format?");
				newStream.Position = 0;

				string myString = newStream.ReadString();

				newStream.Dispose();

				size = 8;
				int count = 10000;
				byte[] buffer = new byte[size * 1048576];
				newStream = new GorgonDataStream((int)(size * 1048576));
				_rnd.NextBytes(buffer);

				double megabytesPerSecond = 0;

				Console.WriteLine("Timing {0} iterations of a {1} buffer copy...", count, buffer.Length.FormatMemory());
				_timer.Reset();
				for (int i = 0; i < count; i++)
				{
					newStream.Write(buffer, 0, buffer.Length);
					newStream.Position = 0;
				}

				
				double milliSeconds = _timer.Milliseconds;
				TimeSpan time = new TimeSpan(0, 0, 0, 0, (int)_timer.Milliseconds);
				
				megabytesPerSecond = milliSeconds / (double)count;
				megabytesPerSecond = ((double)size * 1073741824.0) / megabytesPerSecond;
				Console.WriteLine("{2} loops took {0} ({3:N3} ms per iteration)\n{1:N}/s", time.ToString(), megabytesPerSecond.FormatMemory(), count, milliSeconds / (double)count);

				//newStream.Handle.ZeroMemory((int)newStream.Length);
				newStream.Position = 0;
				items = new Test[1];
				items[0] = new Test();
				items[0].color = 1;
				items[0].u = 55;

				//int position = (int)newStream.Position;
				//newStream.WriteMarshal<Test>(items[0], false);
				//newStream.Position = position;				

				items[0] = new Test();
				items[0] = newStream.ReadMarshal<Test>();

				crc = null;
				items = null;
			}
			finally
			{
				newStream.Dispose();
				newStream = null;

				GC.Collect();
			}

			Console.ReadKey();
		}
	}
}
