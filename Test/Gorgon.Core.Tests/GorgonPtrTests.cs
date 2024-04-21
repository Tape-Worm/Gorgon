using System;
using Gorgon.Native;

namespace Gorgon.Core.Tests;

[TestClass]
public class GorgonPtrTests
{
    [TestMethod]
    public unsafe void ConstructorNullPointerThrowsNullReferenceException()
    {
        int* nullPtr = null;

        Assert.ThrowsException<NullReferenceException>(() => new GorgonPtr<int>(nullPtr, 1), "Expected NullReferenceException for null pointer.");
        Assert.ThrowsException<NullReferenceException>(() => new GorgonPtr<int>((nint)null, 1), "Expected NullReferenceException for null nint.");
        Assert.ThrowsException<NullReferenceException>(() => new GorgonPtr<int>((nuint)null, 1), "Expected NullReferenceException for null nuint.");
    }

    [TestMethod]
    public unsafe void ConstructorNegativeCountThrowsArgumentException()
    {
        int dummy = 0;
        int* dummyPtr = &dummy;
        nint dummyNint = (nint)dummyPtr;
        nuint dummyNuint = (nuint)dummyPtr;

        Assert.ThrowsException<ArgumentException>(() => new GorgonPtr<int>(dummyPtr, -1), "Expected ArgumentException for negative count with pointer constructor.");
        Assert.ThrowsException<ArgumentException>(() => new GorgonPtr<int>(dummyNint, -1), "Expected ArgumentException for negative count with nint constructor.");
        Assert.ThrowsException<ArgumentException>(() => new GorgonPtr<int>(dummyNuint, -1), "Expected ArgumentException for negative count with nuint constructor.");
    }

    [TestMethod]
    public unsafe void ConstructorZeroCountThrowsArgumentException()
    {
        int dummy = 0;
        int* dummyPtr = &dummy;
        nint dummyNint = (nint)dummyPtr;
        nuint dummyNuint = (nuint)dummyPtr;

        Assert.ThrowsException<ArgumentException>(() => new GorgonPtr<int>(dummyPtr, 0), "Expected ArgumentException for zero count with pointer constructor.");
        Assert.ThrowsException<ArgumentException>(() => new GorgonPtr<int>(dummyNint, 0), "Expected ArgumentException for zero count with nint constructor.");
        Assert.ThrowsException<ArgumentException>(() => new GorgonPtr<int>(dummyNuint, 0), "Expected ArgumentException for zero count with nuint constructor.");
    }

    [TestMethod]
    public unsafe void ConstructorValidParametersConstructsCorrectly()
    {
        int dummy = 0;
        int* dummyPtr = &dummy;
        nint dummyNint = (nint)dummyPtr;
        nuint dummyNuint = (nuint)dummyPtr;

        GorgonPtr<int> gorgonPtrFromPointer = new(dummyPtr, 1);
        GorgonPtr<int> gorgonPtrFromNint = new(dummyNint, 1);
        GorgonPtr<int> gorgonPtrFromNuint = new(dummyNuint, 1);

        Assert.IsTrue(dummyPtr == (int*)gorgonPtrFromPointer, "Expected pointers to be equal.");
        Assert.AreEqual(dummyNint, (nint)gorgonPtrFromNint, "Expected nints to be equal.");
        Assert.AreEqual(dummyNuint, (nuint)gorgonPtrFromNuint, "Expected nuints to be equal.");

        Assert.AreEqual(1, gorgonPtrFromPointer.Length, "Expected length to be 1 for pointer constructor.");
        Assert.AreEqual(1, gorgonPtrFromNint.Length, "Expected length to be 1 for nint constructor.");
        Assert.AreEqual(1, gorgonPtrFromNuint.Length, "Expected length to be 1 for nuint constructor.");
    }

    [TestMethod]
    public unsafe void AddSubtractExtendsBeyondBounds()
    {
        int[] dummyArray = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];

        fixed (int* dummyPtr = dummyArray)
        {
            GorgonPtr<int> gorgonPtr = new(dummyPtr, dummyArray.Length);

            for (int i = 0; i < 15; i++)
            {
                gorgonPtr++;
            }

            Assert.AreEqual(GorgonPtr<int>.NullPtr, gorgonPtr, "Pointer should be null when out of bounds");

            gorgonPtr = new GorgonPtr<int>(dummyPtr, dummyArray.Length);

            for (int i = 5; i >= 0; i--)
            {
                gorgonPtr--;
            }

            Assert.AreEqual(GorgonPtr<int>.NullPtr, gorgonPtr, "Pointer should be null when out of bounds");
        }
    }

    [TestMethod]
    public unsafe void OperatorIncrementAndDecrementChangesPointerCorrectly()
    {
        int[] dummyArray = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];

        fixed (int* dummyPtr = dummyArray)
        {
            GorgonPtr<int> gorgonPtr = new(dummyPtr, dummyArray.Length);

            for (int i = 0; i < 4; i++)
            {
                Assert.AreEqual(dummyArray[i], gorgonPtr[0], $"Expected element at index 0 to be {i} before incrementing.");
                gorgonPtr++;
            }

            for (int i = 3; i >= 0; i--)
            {
                gorgonPtr--;
                Assert.AreEqual(dummyArray[i], gorgonPtr[0], $"Expected element at index 0 to be {i} after decrementing.");
            }
        }
    }

    [TestMethod]
    public unsafe void OperatorAdditionAndSubtractionChangesPointerCorrectly()
    {
        int[] dummyArray = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];

        fixed (int* dummyPtr = dummyArray)
        {
            GorgonPtr<int> gorgonPtr = new(dummyPtr, dummyArray.Length);

            for (int i = 0; i < 4; i++)
            {
                Assert.AreEqual(dummyArray[i], gorgonPtr[0], $"Expected element at index 0 to be {i} before addition.");
                gorgonPtr = gorgonPtr + 1;
            }

            for (int i = 3; i >= 0; i--)
            {
                gorgonPtr = gorgonPtr - 1;
                Assert.AreEqual(dummyArray[i], gorgonPtr[0], $"Expected element at index 0 to be {i} after subtraction.");
            }

            for (int i = 0; i < 4; i++)
            {
                Assert.AreEqual(dummyArray[i], gorgonPtr[0], $"Expected element at index 0 to be {i} before addition.");
                gorgonPtr = 1 + gorgonPtr;
            }
        }
    }

    [TestMethod]
    public unsafe void SubtractMethodCorrectlySubtractsPointers()
    {
        int[] dummyArray = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9];

        fixed (int* dummyPtr = dummyArray)
        {
            GorgonPtr<int> gorgonPtr1 = new(dummyPtr, dummyArray.Length);
            GorgonPtr<int> gorgonPtr2 = new(dummyPtr + 5, dummyArray.Length - 5);

            // Subtract pointers
            long result = gorgonPtr2 - gorgonPtr1;

            // The result is the number of bytes between the pointers
            Assert.AreEqual(5 * sizeof(int), result, "Expected the difference between pointers to be 5 * sizeof(int).");
        }
    }

    [TestMethod]
    public unsafe void OperatorEqualityAndInequalityAndEqualsMethodCorrectlyComparesPointers()
    {
        int[] dummyArray = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9];

        fixed (int* dummyPtr = dummyArray)
        {
            GorgonPtr<int> gorgonPtr1 = new(dummyPtr, dummyArray.Length);
            GorgonPtr<int> gorgonPtr2 = new(dummyPtr, dummyArray.Length);
            GorgonPtr<int> gorgonPtr3 = new(dummyPtr + 1, dummyArray.Length - 1);

            // Test equality and inequality operators
            Assert.IsTrue(gorgonPtr1 == gorgonPtr2, "Expected pointers to be equal.");
            Assert.IsFalse(gorgonPtr1 != gorgonPtr2, "Expected pointers not to be unequal.");

            Assert.IsFalse(gorgonPtr1 == gorgonPtr3, "Expected pointers to be unequal.");
            Assert.IsTrue(gorgonPtr1 != gorgonPtr3, "Expected pointers not to be equal.");

            // Test IEquatable<T>.Equals method
            Assert.IsTrue(gorgonPtr1.Equals(gorgonPtr2), "Expected pointers to be equal using Equals method.");
            Assert.IsFalse(gorgonPtr1.Equals(gorgonPtr3), "Expected pointers to be unequal using Equals method.");
        }
    }

    [TestMethod]
    public unsafe void OperatorLessThanAndGreaterThanCorrectlyComparesPointers()
    {
        int[] dummyArray = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9];

        fixed (int* dummyPtr = dummyArray)
        {
            GorgonPtr<int> gorgonPtr1 = new(dummyPtr, dummyArray.Length);
            GorgonPtr<int> gorgonPtr2 = new(dummyPtr + 1, dummyArray.Length - 1);

            Assert.IsTrue(gorgonPtr1 < gorgonPtr2, "Expected gorgonPtr1 to be less than gorgonPtr2.");
            Assert.IsFalse(gorgonPtr1 > gorgonPtr2, "Expected gorgonPtr1 not to be greater than gorgonPtr2.");

            Assert.IsFalse(gorgonPtr2 < gorgonPtr1, "Expected gorgonPtr2 not to be less than gorgonPtr1.");
            Assert.IsTrue(gorgonPtr2 > gorgonPtr1, "Expected gorgonPtr2 to be greater than gorgonPtr1.");
        }
    }

    [TestMethod]
    public unsafe void OperatorLessThanOrEqualToAndGreaterThanOrEqualToCorrectlyComparesPointers()
    {
        int[] dummyArray = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9];

        fixed (int* dummyPtr = dummyArray)
        {
            GorgonPtr<int> gorgonPtr1 = new(dummyPtr, dummyArray.Length);
            GorgonPtr<int> gorgonPtr2 = new(dummyPtr, dummyArray.Length);
            GorgonPtr<int> gorgonPtr3 = new(dummyPtr + 1, dummyArray.Length - 1);

            Assert.IsTrue(gorgonPtr1 <= gorgonPtr2, "Expected gorgonPtr1 to be less than or equal to gorgonPtr2.");
            Assert.IsTrue(gorgonPtr1 >= gorgonPtr2, "Expected gorgonPtr1 to be greater than or equal to gorgonPtr2.");

            Assert.IsTrue(gorgonPtr1 <= gorgonPtr3, "Expected gorgonPtr1 to be less than or equal to gorgonPtr3.");
            Assert.IsFalse(gorgonPtr1 >= gorgonPtr3, "Expected gorgonPtr1 not to be greater than or equal to gorgonPtr3.");

            Assert.IsFalse(gorgonPtr3 <= gorgonPtr1, "Expected gorgonPtr3 not to be less than or equal to gorgonPtr1.");
            Assert.IsTrue(gorgonPtr3 >= gorgonPtr1, "Expected gorgonPtr3 to be greater than or equal to gorgonPtr1.");
        }
    }

    [TestMethod]
    public unsafe void ConversionOperatorsCorrectlyConvertsGorgonPtr()
    {
        int[] dummyArray = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9];

        fixed (int* dummyPtr = dummyArray)
        {
            GorgonPtr<int> gorgonPtr = new(dummyPtr, dummyArray.Length);

            // Test conversion to Span<T>
            Span<int> span = gorgonPtr;
            Assert.IsTrue(span.SequenceEqual(dummyArray), "Expected Span<T> to be equal to the original array.");

            // Test conversion to ReadOnlySpan<T>
            ReadOnlySpan<int> readOnlySpan = gorgonPtr;
            Assert.IsTrue(readOnlySpan.SequenceEqual(dummyArray), "Expected ReadOnlySpan<T> to be equal to the original array.");

            // Test conversion to long
            long longValue = (long)gorgonPtr;
            Assert.AreEqual((long)dummyPtr, longValue, "Expected long value to be equal to the original pointer address.");

            // Test conversion to ulong
            ulong ulongValue = (ulong)gorgonPtr;
            Assert.AreEqual((ulong)dummyPtr, ulongValue, "Expected ulong value to be equal to the original pointer address.");

            // Test conversion to nint
            nint nintValue = (nint)gorgonPtr;
            Assert.AreEqual((nint)dummyPtr, nintValue, "Expected nint value to be equal to the original pointer address.");

            // Test conversion to nuint
            nuint nuintValue = (nuint)gorgonPtr;
            Assert.AreEqual((nuint)dummyPtr, nuintValue, "Expected nuint value to be equal to the original pointer address.");

            // Test conversion to void*
            void* voidPtr = (void*)gorgonPtr;
            Assert.IsTrue(dummyPtr == voidPtr, "Expected void* to be equal to the original pointer address.");

            // Test conversion to GorgonPtr<byte>
            GorgonPtr<byte> gorgonBytePtr = (GorgonPtr<byte>)gorgonPtr;
            byte* bytePtr = (byte*)dummyPtr;
            for (int i = 0; i < gorgonBytePtr.SizeInBytes; i++)
            {
                Assert.AreEqual(bytePtr[i], gorgonBytePtr[i], $"Expected byte at index {i} to be equal in both pointers.");
            }
        }
    }

    [TestMethod]
    public unsafe void IndexerPropertyChangesAndReadsValueCorrectly()
    {
        int[] dummyArray = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9];

        fixed (int* dummyPtr = dummyArray)
        {
            GorgonPtr<int> gorgonPtr = new(dummyPtr, dummyArray.Length);

            // Change a value at index 5 using ref return
            ref int valueAt5 = ref gorgonPtr[5];
            valueAt5 = 50;

            // Read the value at index 5
            int readValue = gorgonPtr[5];

            Assert.AreEqual(50, readValue, "Expected value at index 5 to be 50.");

            // Change a value at index 5 directly without ref
            gorgonPtr[5] = 100;

            // Read the value at index 5
            readValue = gorgonPtr[5];

            Assert.AreEqual(100, readValue, "Expected value at index 5 to be 100.");
        }
    }

    [TestMethod]
    public unsafe void IndexerReturnsCorrectSliceWhenRangeIsValid()
    {
        int[] data = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
        int[] expectedSlice = [4, 5, 6, 7];
        fixed (int* dataPtr = data)
        {
            GorgonPtr<int> tenItems = new(dataPtr, data.Length);
            GorgonPtr<int> sliced = tenItems[3..7];

            Assert.AreEqual(expectedSlice.Length, sliced.Length);
            for (int i = 0; i < sliced.Length; i++)
            {
                Assert.AreEqual(expectedSlice[i], sliced[i]);
            }
        }
    }

    [TestMethod]
    public unsafe void IndexerThrowsArgumentOutOfRangeExceptionWhenRangeIsInvalid()
    {
        int[] data = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
        fixed (int* dataPtr = data)
        {
            GorgonPtr<int> tenItems = new(dataPtr, data.Length);

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { GorgonPtr<int> _ = tenItems[-1..5]; });
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { GorgonPtr<int> _ = tenItems[10..15]; });
        }
    }

    [TestMethod]
    public void IndexerReturnsNullPtrWhenPtrIsNull()
    {
        GorgonPtr<int> nullPtr = GorgonPtr<int>.NullPtr;
        GorgonPtr<int> sliced = nullPtr[3..7];

        Assert.AreEqual(GorgonPtr<int>.NullPtr, sliced);
    }

    [TestMethod]
    public unsafe void ToMethodCorrectlyConvertsGorgonPtr()
    {
        int[] dummyArray = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9];

        fixed (int* dummyPtr = dummyArray)
        {
            GorgonPtr<int> gorgonPtrInt = new(dummyPtr, dummyArray.Length);

            // Convert GorgonPtr<int> to GorgonPtr<byte>
            GorgonPtr<byte> gorgonPtrByte = GorgonPtr<int>.To<byte>(gorgonPtrInt);

            // Check the values after conversion
            byte* bytePtr = (byte*)dummyPtr;
            for (int i = 0; i < gorgonPtrByte.SizeInBytes; i++)
            {
                Assert.AreEqual(bytePtr[i], gorgonPtrByte[i], $"Expected byte at index {i} to be equal in both pointers.");
            }

            // Check the SizeInBytes property
            Assert.AreEqual(gorgonPtrInt.SizeInBytes, gorgonPtrByte.SizeInBytes, "Expected SizeInBytes to be equal in both pointers.");

            // Convert GorgonPtr<byte> back to GorgonPtr<int>
            GorgonPtr<int> gorgonPtrIntNew = GorgonPtr<byte>.To<int>(gorgonPtrByte);

            // Check the values after conversion
            for (int i = 0; i < gorgonPtrIntNew.Length; i++)
            {
                Assert.AreEqual(dummyArray[i], gorgonPtrIntNew[i], $"Expected int at index {i} to be equal in both arrays.");
            }
        }
    }

    [TestMethod]
    public unsafe void ToMethodCorrectlyConvertsIntToByte()
    {
        int dummyInt = 123456789;

        GorgonPtr<int> gorgonPtrInt = new(&dummyInt, 1);

        // Convert GorgonPtr<int> to GorgonPtr<byte>
        GorgonPtr<byte> gorgonPtrByte = GorgonPtr<int>.To<byte>(gorgonPtrInt);

        // Check the values after conversion
        byte* bytePtr = (byte*)&dummyInt;
        for (int i = 0; i < gorgonPtrByte.Length; i++)
        {
            Assert.AreEqual(bytePtr[i], gorgonPtrByte[i], $"Expected byte at index {i} to be equal in both pointers.");
        }

        // Check the SizeInBytes property
        Assert.AreEqual(gorgonPtrInt.SizeInBytes, gorgonPtrByte.SizeInBytes, "Expected SizeInBytes to be equal in both pointers.");
    }

    [TestMethod]
    public unsafe void AsRefMethodCorrectlyReinterpretsMemory()
    {
        int dummyInt = 123456789;

        GorgonPtr<int> gorgonPtrInt = new(&dummyInt, 1);

        // Reinterpret GorgonPtr<int> as byte and check the first byte
        ref byte gorgonPtrByteRef = ref gorgonPtrInt.AsRef<byte>();
        Assert.AreEqual(*((byte*)&dummyInt), gorgonPtrByteRef, "Expected the first byte of the reinterpreted memory to be equal to the first byte of the original integer.");

        // Loop through the bytes of the integer starting from 0
        for (int offset = 0; offset < sizeof(int); offset++)
        {
            // Reinterpret GorgonPtr<int> as byte with offset
            gorgonPtrByteRef = ref gorgonPtrInt.AsRef<byte>(offset);

            // Check the value after reinterpretation
            Assert.AreEqual(*((byte*)&dummyInt + offset), gorgonPtrByteRef, $"Expected the byte at offset {offset} of the reinterpreted memory to be equal to the byte at the same offset of the original integer.");
        }

        // Write 0x7F to the second byte
        gorgonPtrByteRef = ref gorgonPtrInt.AsRef<byte>(1);
        gorgonPtrByteRef = 0x7F;

        // Check the value after writing
        Assert.AreEqual(*((byte*)&dummyInt + 1), gorgonPtrByteRef, "Expected the second byte of the reinterpreted memory to be equal to the written value.");
    }

    [TestMethod]
    public void AsRefMethodThrowsExceptionWhenPointerIsNull()
    {
        // Create a null GorgonPtr<int>
        GorgonPtr<int> gorgonPtrInt = GorgonPtr<int>.NullPtr;

        // Expect a NullReferenceException when trying to reinterpret the null GorgonPtr<int>
        Assert.ThrowsException<NullReferenceException>(() => gorgonPtrInt.AsRef<byte>());
    }

    [TestMethod]
    public unsafe void AsRefMethodThrowsExceptionWhenOffsetIsOutOfRange()
    {
        int dummyInt = 123456789;

        GorgonPtr<int> gorgonPtrInt = new(&dummyInt, 1);

        // Expect an ArgumentOutOfRangeException when trying to reinterpret the GorgonPtr<int> with an offset that is out of range
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => gorgonPtrInt.AsRef<byte>(sizeof(int)));
    }

    [TestMethod]
    public unsafe void AsRefMethodThrowsExceptionWhenOffsetIsLessThanZero()
    {
        int dummyInt = 123456789;

        GorgonPtr<int> gorgonPtrInt = new(&dummyInt, 1);

        // Expect an ArgumentOutOfRangeException when trying to reinterpret the GorgonPtr<int> with an offset that is less than 0
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => gorgonPtrInt.AsRef<byte>(-1));
    }

    [TestMethod]
    public unsafe void FillMethodCorrectlyFillsMemory()
    {
        byte[] dummyArray = new byte[10];

        fixed (byte* dummyPtr = dummyArray)
        {
            GorgonPtr<byte> gorgonPtrByte = new(dummyPtr, dummyArray.Length);

            // Fill GorgonPtr<byte> with 0x7F
            gorgonPtrByte.Fill(0x7F);

            // Check the values after filling
            for (int i = 0; i < gorgonPtrByte.Length; i++)
            {
                Assert.AreEqual(0x7F, gorgonPtrByte[i], $"Expected the byte at index {i} to be 0x7F.");
            }
        }
    }

    [TestMethod]
    public void FillMethodThrowsExceptionWhenPointerIsNull()
    {
        // Create a null GorgonPtr<byte>
        GorgonPtr<byte> gorgonPtrByte = GorgonPtr<byte>.NullPtr;

        // Expect a NullReferenceException when trying to fill the null GorgonPtr<byte>
        Assert.ThrowsException<NullReferenceException>(() => gorgonPtrByte.Fill(0x7F));
    }

    [TestMethod]
    public void ToMethodThrowsExceptionWhenPointerIsNull()
    {
        // Create a null GorgonPtr<int>
        GorgonPtr<int> gorgonPtrInt = GorgonPtr<int>.NullPtr;

        // Expect a NullReferenceException when trying to convert the null GorgonPtr<int>
        Assert.ThrowsException<NullReferenceException>(() => GorgonPtr<int>.To<byte>(gorgonPtrInt));
    }

    [TestMethod]
    public unsafe void ToMethodThrowsInvalidCastException()
    {
        byte dummyArray = 0x7f;

        GorgonPtr<byte> gorgonPtrByte = new(&dummyArray, 1);

        // Expect an InvalidCastException when trying to convert GorgonPtr<byte> to GorgonPtr<int>
        Assert.ThrowsException<InvalidCastException>(() => GorgonPtr<byte>.To<int>(gorgonPtrByte));
    }

    [TestMethod]
    public unsafe void IComparableCorrectlyComparesGorgonPtrs()
    {
        int dummyInt = 123456789;

        GorgonPtr<int> gorgonPtrInt1 = new(&dummyInt, 1);
        GorgonPtr<int> gorgonPtrInt2 = new(&dummyInt + 1, 1);
        GorgonPtr<int> gorgonPtrInt3 = new(&dummyInt - 1, 1);
        GorgonPtr<int> gorgonPtrInt4 = new(&dummyInt, 1); // Points to the same address as gorgonPtrInt1

        // Check the IComparable implementation
        Assert.IsTrue(gorgonPtrInt1.CompareTo(gorgonPtrInt2) < 0, "Expected the address of gorgonPtrInt1 to be less than the address of gorgonPtrInt2.");
        Assert.IsTrue(gorgonPtrInt2.CompareTo(gorgonPtrInt1) > 0, "Expected the address of gorgonPtrInt2 to be greater than the address of gorgonPtrInt1.");
        Assert.IsTrue(gorgonPtrInt1.CompareTo(gorgonPtrInt3) > 0, "Expected the address of gorgonPtrInt1 to be greater than the address of gorgonPtrInt3.");
        Assert.IsTrue(gorgonPtrInt1.CompareTo(gorgonPtrInt4) == 0, "Expected the address of gorgonPtrInt1 to be equal to the address of gorgonPtrInt4.");
    }

    [TestMethod]
    public unsafe void ToArrayCorrectlyConvertsGorgonPtrToArray()
    {
        int[] dummyArray = [1, 2, 3, 4, 5];

        fixed (int* dummyPtr = dummyArray)
        {
            GorgonPtr<int> gorgonPtrInt = new(dummyPtr, dummyArray.Length);

            // Convert GorgonPtr<int> to array
            int[] resultArray = gorgonPtrInt.ToArray();

            // Check the values after conversion
            for (int i = 0; i < gorgonPtrInt.Length; i++)
            {
                Assert.AreEqual(dummyArray[i], resultArray[i], $"Expected the element at index {i} to be equal in both arrays.");
            }
        }
    }

    [TestMethod]
    public unsafe void CompareMemoryReturnsTrueWhenDataIsSame()
    {
        int[] dummyArray1 = [1, 2, 3, 4, 5];
        int[] dummyArray2 = [1, 2, 3, 4, 5];

        fixed (int* dummyPtr1 = dummyArray1, dummyPtr2 = dummyArray2)
        {
            GorgonPtr<int> gorgonPtrInt1 = new(dummyPtr1, dummyArray1.Length);
            GorgonPtr<int> gorgonPtrInt2 = new(dummyPtr2, dummyArray2.Length);

            // Check the CompareMemory method
            Assert.IsTrue(gorgonPtrInt1.CompareMemory(gorgonPtrInt2), "Expected the data pointed at by gorgonPtrInt1 and gorgonPtrInt2 to be the same.");
        }
    }

    [TestMethod]
    public unsafe void CompareMemoryReturnsFalseWhenDataIsDifferent()
    {
        int[] dummyArray1 = [1, 2, 3, 4, 5];
        int[] dummyArray2 = [5, 4, 3, 2, 1];

        fixed (int* dummyPtr1 = dummyArray1, dummyPtr2 = dummyArray2)
        {
            GorgonPtr<int> gorgonPtrInt1 = new(dummyPtr1, dummyArray1.Length);
            GorgonPtr<int> gorgonPtrInt2 = new(dummyPtr2, dummyArray2.Length);

            // Check the CompareMemory method
            Assert.IsFalse(gorgonPtrInt1.CompareMemory(gorgonPtrInt2), "Expected the data pointed at by gorgonPtrInt1 and gorgonPtrInt2 to be different.");
        }
    }

    [TestMethod]
    public unsafe void CompareMemoryReturnsTrueWhenPointersAreSame()
    {
        int[] dummyArray = [1, 2, 3, 4, 5];

        fixed (int* dummyPtr = dummyArray)
        {
            GorgonPtr<int> gorgonPtrInt1 = new(dummyPtr, dummyArray.Length);
            GorgonPtr<int> gorgonPtrInt2 = new(dummyPtr, dummyArray.Length); // Points to the same memory address as gorgonPtrInt1

            // Check the CompareMemory method
            Assert.IsTrue(gorgonPtrInt1.CompareMemory(gorgonPtrInt2), "Expected the data pointed at by gorgonPtrInt1 and gorgonPtrInt2 to be the same because they point to the same memory address.");
        }
    }

    [TestMethod]
    public unsafe void CopyToGorgonPtrCopiesCorrectly()
    {
        int[] sourceArray = [1, 2, 3, 4, 5];
        int[] destinationArray = new int[5];

        fixed (int* sourcePtr = sourceArray, destinationPtr = destinationArray)
        {
            GorgonPtr<int> gorgonPtrSource = new(sourcePtr, sourceArray.Length);
            GorgonPtr<int> gorgonPtrDestination = new(destinationPtr, destinationArray.Length);

            // Copy the memory
            gorgonPtrSource.CopyTo(gorgonPtrDestination);

            // Check the values after copying
            for (int i = 0; i < gorgonPtrSource.Length; i++)
            {
                Assert.AreEqual(sourceArray[i], destinationArray[i], $"Expected the element at index {i} to be equal in both arrays.");
            }
        }
    }

    [TestMethod]
    public unsafe void CopyToSpanCopiesCorrectly()
    {
        int[] sourceArray = [1, 2, 3, 4, 5];
        Span<int> destinationSpan = new int[5];

        fixed (int* sourcePtr = sourceArray)
        {
            GorgonPtr<int> gorgonPtrSource = new(sourcePtr, sourceArray.Length);

            // Copy the memory
            gorgonPtrSource.CopyTo(destinationSpan);

            // Check the values after copying
            for (int i = 0; i < gorgonPtrSource.Length; i++)
            {
                Assert.AreEqual(sourceArray[i], destinationSpan[i], $"Expected the element at index {i} to be equal in both arrays.");
            }
        }
    }

    [TestMethod]
    public unsafe void CopyToArrayCopiesCorrectly()
    {
        int[] sourceArray = [1, 2, 3, 4, 5];
        int[] destinationArray = new int[5];

        fixed (int* sourcePtr = sourceArray)
        {
            GorgonPtr<int> gorgonPtrSource = new(sourcePtr, sourceArray.Length);

            // Copy the memory
            gorgonPtrSource.CopyTo(destinationArray);

            // Check the values after copying
            for (int i = 0; i < gorgonPtrSource.Length; i++)
            {
                Assert.AreEqual(sourceArray[i], destinationArray[i], $"Expected the element at index {i} to be equal in both arrays.");
            }
        }
    }

    [TestMethod]
    public unsafe void CopyToThrowsExceptionWhenSourceIsNull()
    {
        int[] destinationArray = new int[5];

        fixed (int* destinationPtr = destinationArray)
        {
            GorgonPtr<int> gorgonPtrDestination = new(destinationPtr, destinationArray.Length);

            // Create a null GorgonPtr<int>
            GorgonPtr<int> gorgonPtrSource = GorgonPtr<int>.NullPtr;

            // Expect an exception when trying to copy
            Assert.ThrowsException<NullReferenceException>(() => gorgonPtrSource.CopyTo(gorgonPtrDestination));
        }
    }

    [TestMethod]
    public unsafe void CopyToThrowsExceptionWhenDestinationIsNull()
    {
        int[] sourceArray = [1, 2, 3, 4, 5];

        fixed (int* sourcePtr = sourceArray)
        {
            GorgonPtr<int> gorgonPtrSource = new(sourcePtr, sourceArray.Length);

            // Create a null GorgonPtr<int>
            GorgonPtr<int> gorgonPtrDestination = GorgonPtr<int>.NullPtr;

            // Expect an exception when trying to copy
            Assert.ThrowsException<ArgumentNullException>(() => gorgonPtrSource.CopyTo(gorgonPtrDestination));
        }
    }

    [TestMethod]
    public unsafe void CopyToThrowsExceptionWhenDestinationSpanIsEmpty()
    {
        int[] sourceArray = [1, 2, 3, 4, 5];
        Span<int> destinationSpan = [];
        bool exceptionThrown = false;

        fixed (int* sourcePtr = sourceArray)
        {
            GorgonPtr<int> gorgonPtrSource = new(sourcePtr, sourceArray.Length);

            try
            {
                // Attempt to copy to an empty span
                gorgonPtrSource.CopyTo(destinationSpan);
            }
            catch (ArgumentEmptyException)
            {
                // If an ArgumentException is thrown, set the flag to true
                exceptionThrown = true;
            }
        }

        // Assert that an exception was thrown
        Assert.IsTrue(exceptionThrown, "Expected an ArgumentException to be thrown when trying to copy to an empty span.");
    }

    [TestMethod]
    public unsafe void CopyToSpanCopiesCorrectlyWithVaryingCountsAndOffsets()
    {
        int[] sourceArray = [1, 2, 3, 4, 5];
        Span<int> destinationSpan = new int[5];

        fixed (int* sourcePtr = sourceArray)
        {
            GorgonPtr<int> gorgonPtrSource = new(sourcePtr + 1, sourceArray.Length - 1);

            // Copy the memory
            gorgonPtrSource[..2].CopyTo(destinationSpan.Slice(2, 2));

            // Check the values after copying
            for (int i = 0; i < 2; i++)
            {
                Assert.AreEqual(sourceArray[i + 1], destinationSpan[i + 2], $"Expected the element at index {i} to be equal in both arrays.");
            }
        }
    }
}
