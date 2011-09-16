using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using GorgonLibrary.Data;

namespace Test_DataStream
{
	class Program
	{
		private static Stopwatch _timer = null;
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

		private static int size = 89478460;

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

			size = (Int32.MaxValue / System.Runtime.InteropServices.Marshal.SizeOf(typeof(Test))) - System.Runtime.InteropServices.Marshal.SizeOf(typeof(Test));

			Test[] crc = new Test[size];
			Test[] items = new Test[size];

			Console.WriteLine("Arrays allocated.");
			Console.ReadKey();			

			_timer = Stopwatch.StartNew();
			Console.Write("Creating {0} vertices...", size);
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
			Console.WriteLine(": {0}ms", _timer.ElapsedMilliseconds);

			GorgonDataStream newStream = new GorgonDataStream(System.Runtime.InteropServices.Marshal.SizeOf(typeof(Test)) * size);

			try
			{				
				Console.Write("Writing {0} vertices individually...", size);

				_timer = Stopwatch.StartNew();
				for (int i = 0; i < items.Length; i++)
				{
					newStream.Write<Test>(items[i]);

				}
				_timer.Stop();
				Console.WriteLine(": {0}ms", _timer.ElapsedMilliseconds);

				newStream.Position = 0;

				Console.Write("Reading {0} vertices individually...", size);
				_timer = Stopwatch.StartNew();
				for (int i = 0; i < items.Length; i++)
				{
					items[i] = newStream.Read<Test>();
				}
				_timer.Stop();
				Console.WriteLine(": {0}ms", _timer.ElapsedMilliseconds);

				if (!CheckCRC(items, crc))
				{
					Console.WriteLine("CRC does not match.");
				}

				newStream.Position = 0;

				Console.Write("Writing {0} vertices at once...", size);
				_timer = Stopwatch.StartNew();
				newStream.Write<Test>(items);
				_timer.Stop();

				Console.WriteLine(": {0}ms", _timer.ElapsedMilliseconds);

				newStream.Position = 0;

				Console.Write("Reading {0} vertices at once...", size);
				_timer = Stopwatch.StartNew();
				items = newStream.Read<Test>(size);
				_timer.Stop();

				if (!CheckCRC(items, crc))
				{
					Console.WriteLine("CRC does not match.");
				}

				Console.WriteLine(": {0}ms", _timer.ElapsedMilliseconds);

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
