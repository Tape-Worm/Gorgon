using System;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
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
		public void ChunkIDTest()
		{
			const UInt64 expectedId1 = 0x4B4E484354534554;
			const UInt64 expectedId2 = 0x54534554;
			string chunk1 = "TESTCHNK";
			string chunk2 = "TEST";
			string chunk3 = "TESTCHNKABCDEF";


			UInt64 id = chunk1.ChunkID();

			Assert.AreEqual(expectedId1, id);

			id = chunk2.ChunkID();

			Assert.AreEqual(expectedId2, id);

			id = chunk3.ChunkID();

			Assert.AreEqual(expectedId1, id);
		}

		[TestMethod]
		public void StreamWrapperTest()
		{
			byte[] block1 =
			{
				0,
				1,
				2,
				3,
				4,
				5,
				6,
				7,
				8,
				9,
				10
			};

			using (MemoryStream stream = new MemoryStream())
			{
				stream.Write(block1, 0, block1.Length);
				stream.Position = 0;

				using (GorgonStreamWrapper wrapper = new GorgonStreamWrapper(stream, 6, 5))
				{
					byte[] actualBytes = new byte[5];

					Assert.AreEqual(5, wrapper.Read(actualBytes, 0, 5));

					for (int i = 0; i < actualBytes.Length; ++i)
					{
						Assert.AreEqual(block1[i + 6], actualBytes[i]);
					}

					Assert.AreEqual(wrapper.Length, wrapper.Position);
				}

				Assert.AreEqual(0, stream.Position);

				using (GorgonStreamWrapper wrapper = new GorgonStreamWrapper(stream, 6, 5))
				{
					byte[] actualBytes = new byte[8];

					Assert.AreEqual(5, wrapper.Read(actualBytes, 0, 8));
				}

				using (GorgonStreamWrapper wrapper = new GorgonStreamWrapper(stream, 2))
				{
					byte[] actualBytes = new byte[3];

					wrapper.Seek(3, SeekOrigin.Begin);
					wrapper.Read(actualBytes, 0, 3);

					for (int i = 0; i < actualBytes.Length; ++i)
					{
						Assert.AreEqual(block1[i + 5], actualBytes[i]);	
					}

					wrapper.Seek(-3, SeekOrigin.End);
					wrapper.Read(actualBytes, 0, 3);

					for (int i = 0; i < actualBytes.Length; ++i)
					{
						Assert.AreEqual(block1[block1.Length - 3 + i], actualBytes[i]);
					}
				}
			}
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
				var fileWriter = new GorgonChunkFileWriter(stream, Header.ChunkID());
				fileWriter.Open();

				GorgonBinaryWriter writer = fileWriter.OpenChunk(StringChunk.ChunkID());

				writer.Write(strs.Length);

				foreach (string str in strs)
				{
					writer.Write(str);
				}

				fileWriter.CloseChunk();

				writer = fileWriter.OpenChunk(IntChunk.ChunkID());

				writer.Write(ints.Length);

				foreach (int intVal in ints)
				{
					writer.Write(intVal);
				}

				fileWriter.CloseChunk();
				
				fileWriter.Close();

				stream.Position = 0;

				var fileReader = new GorgonChunkFileReader(stream,
				                                           new[]
				                                           {
					                                           Header.ChunkID()
				                                           });

				fileReader.Open();

				GorgonBinaryReader reader = fileReader.OpenChunk(IntChunk.ChunkID());

				int numInts = reader.ReadInt32();

				Assert.AreEqual(expectedIntsLength, numInts);

				int[] intVals = new int[numInts];

				for (int i = 0; i < numInts; ++i)
				{
					intVals[i] = reader.ReadInt32();
				}

				Assert.IsTrue(ints.SequenceEqual(intVals));

				fileReader.CloseChunk();

				reader = fileReader.OpenChunk(StringChunk.ChunkID());

				int numStrs = reader.ReadInt32();

				Assert.AreEqual(expectedStrLength, numStrs);

				string[] strVals = new string[numStrs];

				for (int i = 0; i < numStrs; ++i)
				{
					strVals[i] = reader.ReadString();
				}

				Assert.IsTrue(strs.SequenceEqual(strVals));

				fileReader.CloseChunk();

				fileReader.Close();
			}
		}
	}
}
