using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using GorgonLibrary.Data;

namespace TestBuffer
{
	class Program
	{
		struct TestData
		{
			public float x, y, z;
			public float u, v;
			public uint color;
			public float w;
		}

		static void Main(string[] args)
		{			
			GorgonDataBuffer buffer = new GorgonDataBuffer(Marshal.SizeOf(typeof(TestData)) * 10);
			//GorgonDataBuffer.LockRegion lockRegion1 = buffer.Lock(10, Marshal.SizeOf(typeof(TestData)));
			GorgonDataBuffer.LockRegion lockRegion1 = buffer.Lock();
			//GorgonDataBuffer.LockRegion lockRegion2 = buffer.Lock(275, 5);
			
			TestData test1 = new TestData();
			test1.x = 1.0f;
			test1.y = 2.0f;
			test1.z = 3.0f;
			test1.u = 4.0f;
			test1.v = 5.0f;
			test1.color = 0xDEADBEEF;
			test1.w = 5.6f;

			lockRegion1.Write<TestData>(test1, 0);

			byte[] data = new byte[lockRegion1.LockSize];
			lockRegion1.ReadRange(data);

			buffer.Allocate(10);

			TestData test2 = lockRegion1.Read<TestData>(0);

			//lockRegion2.Dispose();
			lockRegion1.Dispose();

		}
	}
}
