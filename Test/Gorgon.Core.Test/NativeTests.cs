using System;
using System.Linq;
using Gorgon.Math;
using Gorgon.Native;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SlimMath;

namespace Gorgon.Core.Test
{
    [TestClass]
    public class NativeTests
    {
        private Random _rnd = new Random();
        private Vector2[] _vectors = new Vector2[64];
        private Vector4 _pinVector = new Vector4(4,5,6,7);

        [TestInitialize]
        public void Init()
        {
            for (int i = 0; i < _vectors.Length; ++i)
            {
                _vectors[i] = Vector2.Zero;
            }
        }

        [TestMethod]
        public void PinTest()
        {
            using (IGorgonPointer buffer = new GorgonPointerPinned<Vector2>(_vectors))
            {
                buffer.Write(Vector2.SizeInBytes * 16, 3.0f);
                buffer.Write((Vector2.SizeInBytes * 16) + sizeof(float), 4.0f);

                Vector2 actual = buffer.Read<Vector2>(16 * Vector2.SizeInBytes);

                Assert.IsTrue(_vectors[16].X.EqualsEpsilon(actual.X));
                Assert.IsTrue(_vectors[16].Y.EqualsEpsilon(actual.Y));
            }

            using (var buffer = new GorgonPointerPinned<Vector2>(_vectors, 16, 1))
            {
                Assert.AreEqual(Vector2.SizeInBytes, buffer.Size);
                Assert.AreEqual(Vector2.SizeInBytes * 16, buffer.PinnedOffset);

                buffer.Write(3.0f);
                buffer.Write(sizeof(float), 4.0f);

                Vector2 actual = buffer.Read<Vector2>();

                Assert.IsTrue(_vectors[16].X.EqualsEpsilon(actual.X));
                Assert.IsTrue(_vectors[16].Y.EqualsEpsilon(actual.Y));
            }

            using (GorgonPointerBase buffer = new GorgonPointerPinned<Vector4>(_pinVector))
            {
                Assert.AreEqual(Vector4.SizeInBytes, buffer.Size);

                Vector4 actual = buffer.Read<Vector4>();

                Assert.IsTrue(_pinVector.X.EqualsEpsilon(actual.X));
                Assert.IsTrue(_pinVector.Y.EqualsEpsilon(actual.Y));
                Assert.IsTrue(_pinVector.Z.EqualsEpsilon(actual.Z));
                Assert.IsTrue(_pinVector.W.EqualsEpsilon(actual.W));
            }
        }

        [TestMethod]
        public unsafe void AliasTest()
        {
            Vector4 expected = new Vector4(1, 2, 3, 4);
            Vector4* ptr = &expected;
            using (IGorgonPointer buffer = new GorgonPointerAliasTyped<Vector4>(ptr))
            {
                Assert.AreEqual(16, buffer.Size);

                float value1 = buffer.Read<float>();
                float value2 = buffer.Read<float>(8);

                Assert.IsTrue(1.0f.EqualsEpsilon(value1));
                Assert.IsTrue(3.0f.EqualsEpsilon(value2));
            }

            fixed(Vector2 *vec2 = &_vectors[0])
            {
                using (IGorgonPointer buffer = new GorgonPointerAlias(vec2, Vector2.SizeInBytes * _vectors.Length))
                {
                    buffer.Write(Vector2.SizeInBytes * 16, 3.0f);
                    buffer.Write((Vector2.SizeInBytes * 16) + sizeof(float), 4.0f);

                    Vector2 actual = buffer.Read<Vector2>(16 * Vector2.SizeInBytes);

                    Assert.IsTrue(_vectors[16].X.EqualsEpsilon(actual.X));
                    Assert.IsTrue(_vectors[16].Y.EqualsEpsilon(actual.Y));
                }
            }
        }

        [TestMethod]
        public void DataBufferReadWriteValueTest()
        {
            var expected = new Vector4(1, 2, 3 ,4);

            using (IGorgonPointer buffer = new GorgonPointerTyped<Vector4>())
            {
                buffer.Write(ref expected);

                Vector4 actual = buffer.Read<Vector4>();

                Assert.AreEqual(expected, actual);
            }

            for (int i = 0; i < _vectors.Length; ++i)
            {
                _vectors[i] = new Vector2(i * 2, ((i * 2) + 1));
            }

            using (IGorgonPointer buffer = new GorgonPointer(Vector2.SizeInBytes * _vectors.Length))
            {
                buffer.WriteRange(_vectors, 16, 1);

                Vector2 value = buffer.Read<Vector2>();

                Assert.IsTrue(_vectors[16].X.EqualsEpsilon(value.X));
                Assert.IsTrue(_vectors[16].Y.EqualsEpsilon(value.Y));

                buffer.WriteRange(_vectors);

                value = buffer.Read<Vector2>();

                Assert.IsTrue(_vectors[0].X.EqualsEpsilon(value.X));
                Assert.IsTrue(_vectors[0].Y.EqualsEpsilon(value.Y));

                value = buffer.Read<Vector2>(Vector2.SizeInBytes * 12);

                Assert.IsTrue(_vectors[12].X.EqualsEpsilon(value.X));
                Assert.IsTrue(_vectors[12].Y.EqualsEpsilon(value.Y));

                value = buffer.Read<Vector2>(Vector2.SizeInBytes * (_vectors.Length - 1));

                Assert.IsTrue(_vectors[_vectors.Length - 1].X.EqualsEpsilon(value.X));
                Assert.IsTrue(_vectors[_vectors.Length - 1].Y.EqualsEpsilon(value.Y));

                float[] values = new float[_vectors.Length * 2];
                buffer.ReadRange(values);

                for (int i = 0; i < _vectors.Length; ++i)
                {
                    Assert.IsTrue(_vectors[i].X.EqualsEpsilon(values[i * 2]));
                    Assert.IsTrue(_vectors[i].Y.EqualsEpsilon(values[(i * 2) + 1]));
                }
            }
        }

        [TestMethod]
        public void DataBufferAllocTest()
        {
            using (IGorgonPointer buffer = new GorgonPointer(666))
            {
                byte[] expected = new byte[666];
                byte[] actual = new byte[666];

                Assert.AreEqual(666, buffer.Size);

                for (int i = 0; i < expected.Length; ++i)
                {
                    expected[i] = 0;
                }

                _rnd.NextBytes(actual);

                for (int i = 0; i < 262144; i++)
                {
                    buffer.Zero();
                }

                buffer.ReadRange(0, actual);

                Assert.IsTrue(actual.SequenceEqual(expected));

                for (int i = 0; i < expected.Length; ++i)
                {
                    expected[i] = 0x23;
                }

                buffer.Fill(0x23);

                buffer.ReadRange(0, actual);

                Assert.IsTrue(actual.SequenceEqual(expected));

                buffer.Zero();

                for (int i = 0; i < expected.Length; ++i)
                {
                    if ((i < 50) || (i > 127))
                    {
                        expected[i] = 0x0;
                    }
                    else
                    {
                        expected[i] = 0xAA;
                    }
                }

                buffer.Fill(0xAA, 78, 50);

                buffer.ReadRange(0, actual);

                Assert.IsTrue(actual.SequenceEqual(expected));
            }
        }
    }
}
