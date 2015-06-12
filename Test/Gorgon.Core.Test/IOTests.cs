using System;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using Gorgon.IO;
using Gorgon.Math;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gorgon.Core.Test
{
	[TestClass]
	public class IOTests
	{
		[StructLayout(LayoutKind.Sequential)]
		struct TehStruct
		{
			public int Value;
			public uint Value2;
			public float Value3;
		}

		[StructLayout(LayoutKind.Sequential)]
		struct NewStruct
		{
			public int Value;
			public uint Value2;
			public float Value3;
			public double Value4;
		}

		[TestMethod]
		public void ChunkReaderWriterMoveNext()
		{
			NewStruct value = new NewStruct
			{
				Value = 123,
				Value2 = 456,
				Value3 = 42.5f,
				Value4 = 82.12
			};

			string tehString = "TehString";

			using (MemoryStream stream = new MemoryStream())
			{
				using (GorgonChunkWriter writer = new GorgonChunkWriter(stream))
				{
					writer.Begin("TESTCHNK");
					writer.Write(value);
					writer.End();
					writer.Begin("TESTSTRC");
					writer.WriteString(tehString);
					writer.End();
				}

				stream.Position = 0;

				using (GorgonChunkReader reader = new GorgonChunkReader(stream))
				{
					reader.Begin("TESTCHNK");
					TehStruct newValue = reader.Read<TehStruct>();
					reader.End();

					reader.Begin("TESTSTRC");
					string readStr = reader.ReadString();
					reader.End();

					Assert.AreEqual(value.Value, newValue.Value);
					Assert.AreEqual(value.Value2, newValue.Value2);
					Assert.AreEqual(value.Value3, newValue.Value3);
					Assert.AreEqual(tehString, readStr);
				}
			}
		}

		[TestMethod]
		public void ChunkReaderWriterStruct()
		{
			TehStruct value = new TehStruct
			                  {
				                  Value = 123,
				                  Value2 = 456,
				                  Value3 = 42.5f
			                  };
			using (MemoryStream stream = new MemoryStream())
			{
				uint expectedStructSize;

				using (GorgonChunkWriter writer = new GorgonChunkWriter(stream))
				{
					writer.Begin("TESTCHNK");
					writer.Write(value);
					expectedStructSize = writer.End();
				}

				stream.Position = 0;

				using (GorgonChunkReader reader = new GorgonChunkReader(stream))
				{
					uint actualHeaderSize = reader.Begin("TESTCHNK");

					Assert.AreEqual(expectedStructSize, actualHeaderSize);

					TehStruct newValue = reader.Read<TehStruct>();

					reader.End();

					Assert.AreEqual(value.Value, newValue.Value);
					Assert.AreEqual(value.Value2, newValue.Value2);
					Assert.AreEqual(value.Value3, newValue.Value3);
				}

				stream.Position = 0;

				TehStruct[] values = new TehStruct[15];

				for (int i = 0; i < values.Length; ++i)
				{
					values[i] = new TehStruct
					            {
						            Value = GorgonRandom.RandomInt32(32, 128),
						            Value2 = (uint)GorgonRandom.RandomInt32(327, 18902),
						            Value3 = GorgonRandom.RandomSingle(12.2f, 98.9f)
					            };
				}

				using (GorgonChunkWriter writer = new GorgonChunkWriter(stream))
				{
					writer.Begin("TESTCHNK");
					writer.WriteInt32(values.Length);
					writer.WriteRange(values);
					expectedStructSize = writer.End();
				}

				stream.Position = 0;

				using (GorgonChunkReader reader = new GorgonChunkReader(stream))
				{
					Assert.IsTrue(reader.HasChunk("TESTCHNK"));
					uint actualSize = reader.Begin("TESTCHNK");
					Assert.AreEqual(expectedStructSize, actualSize);

					int len = reader.ReadInt32();
					Assert.AreEqual(15, len);

					TehStruct[] readValues = new TehStruct[len];

					reader.ReadRange(readValues);

					reader.End();

					for (int i = 0; i < readValues.Length; ++i)
					{
						Assert.AreEqual(values[i].Value, readValues[i].Value);
						Assert.AreEqual(values[i].Value2, readValues[i].Value2);
						Assert.IsTrue(values[i].Value3.EqualsEpsilon(readValues[i].Value3));
					}
				}
				
			}
		}

		[TestMethod]
		public void ChunkReaderWriter()
		{
			const string HeaderChunk = "HEADCHNK";
			const string StringChunk = "STRSCHNK";
			const string IntChunk = "INTSCHNK";
			const string Header = "TheHeader";

			string[] strs =
			{
				"Cow",
				"Dog",
				"Cat",
				"Rabbit",
				"Duck"
			};

			int[] ints =
			{
				32,
				11,
				89,
				64,
				87,
				77,
				16,
				2,
				42
			};

			int expectedStrLength = strs.Length;
			int expectedIntsLength = ints.Length;
			uint expectedHeadSize;
			uint expectedStrsSize;
			uint expectedIntsSize;

			using (MemoryStream stream = new MemoryStream())
			{
				using (GorgonChunkWriter writer = new GorgonChunkWriter(stream))
				{
					writer.Begin(HeaderChunk);
					writer.WriteString(Header);
					expectedHeadSize = writer.End();

					writer.Begin(StringChunk);
					writer.WriteInt32(strs.Length);
					foreach (string str in strs)
					{
						writer.WriteString(str);
					}
					expectedStrsSize = writer.End();

					writer.Begin(IntChunk);
					writer.WriteInt32(ints.Length);
					foreach (int intval in ints)
					{
						writer.WriteInt32(intval);
					}
					expectedIntsSize = writer.End();
				}

				Assert.IsTrue(stream.Position > 0);
				Assert.IsTrue(expectedHeadSize > 0);
				Assert.IsTrue(expectedStrsSize > 0);
				Assert.IsTrue(expectedIntsSize > 0);

				stream.Position = 0;

				int[] readInts;
				string[] readStrs;
				
				using (GorgonChunkReader reader = new GorgonChunkReader(stream))
				{
					uint actualHeaderSize = reader.Begin(HeaderChunk);
					
					Assert.AreEqual(expectedHeadSize, actualHeaderSize);
					
					string headValue = reader.ReadString();
					
					Assert.AreEqual(Header, headValue);
					
					actualHeaderSize = reader.End();
					
					Assert.AreEqual(expectedHeadSize, actualHeaderSize);

					uint actualStrsSize = reader.Begin(StringChunk);

					Assert.AreEqual(expectedStrsSize, actualStrsSize);

					int strLen = reader.ReadInt32();

					Assert.AreEqual(expectedStrLength, strLen);

					readStrs = new string[strLen];

					for (int i = 0; i < strLen; ++i)
					{
						readStrs[i] = reader.ReadString();
						Assert.AreEqual(strs[i], readStrs[i]);
					}

					actualStrsSize = reader.End();

					Assert.AreEqual(expectedStrsSize, actualStrsSize);

					uint actualIntSize = reader.Begin(IntChunk);

					Assert.AreEqual(expectedIntsSize, actualIntSize);

					int intLen = reader.ReadInt32();

					Assert.AreEqual(expectedIntsLength, intLen);

					readInts = new int[intLen];

					for (int i = 0; i < intLen; ++i)
					{
						readInts[i] = reader.ReadInt32();
						Assert.AreEqual(ints[i], readInts[i]);
					}

					actualIntSize = reader.End();

					Assert.AreEqual(expectedIntsSize, actualIntSize);
				}
			}
		}
	}
}
