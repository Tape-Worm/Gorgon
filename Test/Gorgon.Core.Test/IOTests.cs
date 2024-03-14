using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Gorgon.Core.Test.Properties;
using Gorgon.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gorgon.Core.Test
{
    [TestClass]
    public class IOTests
    {
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct MyStruct
        {
            public int MyInt;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string MyStr;
        }

        [TestMethod]
        public void DataStreamMarshalTest()
        {
            MyStruct[] items = new MyStruct[50];

            for (int i = 0; i < items.Length; ++i)
            {
                items[i] = new MyStruct
                           {
                               MyInt = i,
                               MyStr = "String #" + i + "\0"
                           };
            }

            using (GorgonDataStream stream = GorgonDataStream.MarshalArrayToStream(items))
            {
                byte[] expected = Resources.DataStreamMarshalOut1;

                // Not sure how I can test the validity of the data?
                byte[] dataBlock = new byte[stream.Length];

                stream.Read(dataBlock, 0, dataBlock.Length);

                Assert.IsTrue(expected.SequenceEqual(dataBlock));
            }
        }

        [TestMethod]
        public void CopyToStreamTest()
        {
            using (MemoryStream stream1 = new MemoryStream(Resources.DataStreamMarshalOut1))
            {
                using (MemoryStream stream2 = new MemoryStream())
                {
                    int copied = stream1.CopyToStream(stream2, Resources.DataStreamMarshalOut1.Length / 2, 2048);

                    Assert.AreEqual(Resources.DataStreamMarshalOut1.Length / 2, copied);

                    stream2.Position = 0;
                    byte[] actual = stream2.ToArray();

                    for (int i = 0; i < copied; ++i)
                    {
                        Assert.AreEqual(Resources.DataStreamMarshalOut1[i], actual[i]);
                    }
                }
            }
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
        public void ChunkReaderWriter()
        {
            const string stringChunk = "STRSCHNK";
            const string intChunk = "INTSCHNK";
            const string header = "TheHeader";

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

            using (MemoryStream stream = new MemoryStream())
            {
                var fileWriter = new GorgonChunkFileWriter(stream, header.ChunkID());
                fileWriter.Open();

                GorgonBinaryWriter writer = fileWriter.OpenChunk(stringChunk.ChunkID());

                writer.Write(strs.Length);

                foreach (string str in strs)
                {
                    writer.Write(str);
                }

                fileWriter.CloseChunk();

                writer = fileWriter.OpenChunk(intChunk.ChunkID());

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
                                                               header.ChunkID()
                                                           });

                fileReader.Open();

                GorgonBinaryReader reader = fileReader.OpenChunk(intChunk.ChunkID());

                int numInts = reader.ReadInt32();

                Assert.AreEqual(expectedIntsLength, numInts);

                int[] intVals = new int[numInts];

                for (int i = 0; i < numInts; ++i)
                {
                    intVals[i] = reader.ReadInt32();
                }

                Assert.IsTrue(ints.SequenceEqual(intVals));

                fileReader.CloseChunk();

                reader = fileReader.OpenChunk(stringChunk.ChunkID());

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
