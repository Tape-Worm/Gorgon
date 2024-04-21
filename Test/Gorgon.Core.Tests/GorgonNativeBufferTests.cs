using System;
using Gorgon.Native;

namespace Gorgon.Core.Tests;

[TestClass]
public class GorgonNativeBufferTests
{
    [TestMethod]
    public void PointerConstructorShouldThrowExceptionWhenPointerIsNull()
    {
        // Arrange
        GorgonPtr<int> pointer = GorgonPtr<int>.NullPtr;

        // Act & Assert
        Assert.ThrowsException<NullReferenceException>(() => new GorgonNativeBuffer<int>(pointer));
    }

    [TestMethod]
    public unsafe void PointerConstructorShouldCreateBufferWhenPointerIsValidAndCountIsWithinRange()
    {
        // Arrange
        int[] array = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
        GorgonPtr<int> pointer;

        fixed (int* p = array)
        {
            pointer = new GorgonPtr<int>(p, array.Length);
            pointer += 2;

            // Act
            using GorgonNativeBuffer<int> buffer = new(pointer[..5]);
            // Assert
            Assert.AreEqual(5, buffer.Length);

            for (int i = 0; i < buffer.Length; i++)
            {
                Assert.AreEqual(array[i + 2], buffer[i]);
            }
        }
    }

    [TestMethod]
    public unsafe void PointerConstructorShouldCreateBufferWhenPointerIsValidAndCountIsNotSpecified()
    {
        // Arrange
        int[] array = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
        GorgonPtr<int> pointer;

        fixed (int* p = array)
        {
            pointer = new GorgonPtr<int>(p, array.Length);

            // Act
            using GorgonNativeBuffer<int> buffer = new(pointer);
            // Assert
            Assert.AreEqual(array.Length, buffer.Length);

            for (int i = 0; i < buffer.Length; i++)
            {
                Assert.AreEqual(array[i], buffer[i]);
            }
        }
    }

    [TestMethod]
    public void SizeConstructorShouldThrowExceptionWhenSizeIsLessThanOne() =>
        // Act & Assert
        Assert.ThrowsException<ArgumentException>(() => new GorgonNativeBuffer<int>(0));

    [TestMethod]
    public void SizeConstructorShouldCreateBufferWhenSizeIsWithinRange()
    {
        // Act
        GorgonNativeBuffer<int> buffer = new(10);

        // Assert
        Assert.AreEqual(10, buffer.Length);
    }

    [TestMethod]
    public void IndexerReturnsCorrectValue()
    {
        // Arrange
        using GorgonNativeBuffer<int> buffer = new(5); // Assuming constructor takes length as parameter.
        int[] testData = [1, 2, 3, 4, 5];
        for (int i = 0; i < testData.Length; i++)
        {
            buffer[i] = testData[i]; // Writing values to the buffer using the indexer.
        }

        // Act
        int value = buffer[2]; // Accessing the third element.

        // Assert
        Assert.AreEqual(testData[2], value); // Expecting the third element of testData.
    }

    [TestMethod]
    public void IndexerRefReturnsCorrectValue()
    {
        // Arrange
        using GorgonNativeBuffer<int> buffer = new(5); // Assuming constructor takes length as parameter.
        int[] testData = [1, 2, 3, 4, 5];
        for (int i = 0; i < testData.Length; i++)
        {
            buffer[i] = testData[i]; // Writing values to the buffer using the indexer.
        }

        // Act
        ref int valueRef = ref buffer[2]; // Accessing the third element as a ref.
        valueRef = 10; // Modifying the value.

        // Assert
        Assert.AreEqual(10, buffer[2]); // Expecting the third element of buffer to be 10.
    }

    [TestMethod]
    public void IndexerThrowsIndexOutOfRangeException()
    {
        // Arrange
        using GorgonNativeBuffer<int> buffer = new(5); // Assuming constructor takes length as parameter.

        // Act & Assert
        Assert.ThrowsException<IndexOutOfRangeException>(() => { int value = buffer[10]; }); // Accessing an out-of-range index.
    }

    [TestMethod]
    public void RangeIndexerReturnsCorrectGorgonPtr()
    {
        // Arrange
        using GorgonNativeBuffer<int> buffer = new(5); // Assuming constructor takes length as parameter.
        int[] testData = [1, 2, 3, 4, 5];
        for (int i = 0; i < testData.Length; i++)
        {
            buffer[i] = testData[i]; // Writing values to the buffer using the indexer.
        }

        // Act
        GorgonPtr<int> ptr = buffer[1..3]; // Accessing a range of elements.

        // Assert
        for (int i = 0; i < ptr.Length; i++)
        {
            Assert.AreEqual(testData[i + 1], ptr[i]); // Expecting the elements of ptr to match the corresponding elements of testData.
        }

        // Act
        ptr[0] = 10; // Writing to the GorgonPtr.

        // Assert
        Assert.AreEqual(10, buffer[1]); // Expecting the second element of buffer to be 10.
    }

    [TestMethod]
    public void RangeIndexerThrowsArgumentOutOfRangeException()
    {
        // Arrange
        using GorgonNativeBuffer<int> buffer = new(5); // Assuming constructor takes length as parameter.

        // Act & Assert
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => { GorgonPtr<int> ptr = buffer[5..10]; }); // Accessing an out-of-range slice.
    }

    [TestMethod]
    public void RangeIndexerThrowsArgumentOutOfRangeExceptionWhenStartIndexIsLessThanZero()
    {
        // Arrange
        using GorgonNativeBuffer<int> buffer = new(5); // Assuming constructor takes length as parameter.

        // Act & Assert
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => { GorgonPtr<int> ptr = buffer[-1..3]; }); // Accessing a slice with a starting index less than 0.
    }

    [TestMethod]
    public void CanImplicitlyCastGorgonNativeBufferToGorgonPtr()
    {
        // Arrange
        int[] testData = [1, 2, 3, 4, 5];
        using GorgonNativeBuffer<int> buffer = new(5);
        for (int i = 0; i < testData.Length; i++)
        {
            buffer[i] = testData[i];
        }

        // Act
        GorgonPtr<int> ptr = buffer; // Implicit cast.

        // Assert
        for (int i = 0; i < ptr.Length; i++)
        {
            Assert.AreEqual(testData[i], ptr[i]);
        }
    }

    [TestMethod]
    public void NullGorgonNativeBufferCastsToNullGorgonPtr()
    {
        // Arrange
        GorgonNativeBuffer<int>? buffer = null;

        // Act
        GorgonPtr<int> ptr = buffer; // Implicit cast.

        // Assert
        Assert.AreEqual(GorgonPtr<int>.NullPtr, ptr);
    }

    [TestMethod]
    public unsafe void FromGorgonPtrCreatesCorrectGorgonNativeBuffer()
    {
        // Arrange
        int[] testData = [1, 2, 3, 4, 5];
        fixed (int* ptr = testData)
        {
            GorgonPtr<int> gorgonPtr = new(ptr, testData.Length);

            // Act
            using GorgonNativeBuffer<int> newBuffer = GorgonNativeBuffer<int>.FromSpan(gorgonPtr);

            // Assert
            for (int i = 0; i < newBuffer.Length; i++)
            {
                Assert.AreEqual(testData[i], newBuffer[i]);
            }
        }
    }

    [TestMethod]
    public void FromSpanCreatesCorrectGorgonNativeBuffer()
    {
        // Arrange
        int[] testData = [1, 2, 3, 4, 5];
        ReadOnlySpan<int> span = new(testData);

        // Act
        using GorgonNativeBuffer<int> newBuffer = GorgonNativeBuffer<int>.FromSpan(span);

        // Assert
        for (int i = 0; i < newBuffer.Length; i++)
        {
            Assert.AreEqual(testData[i], newBuffer[i]);
        }
    }

    [TestMethod]
    public void FromSpanWithCountCreatesCorrectGorgonNativeBuffer()
    {
        // Arrange
        int[] testData = [1, 2, 3, 4, 5];
        ReadOnlySpan<int> span = new(testData, 0, 3);

        // Act
        using GorgonNativeBuffer<int> newBuffer = GorgonNativeBuffer<int>.FromSpan(span);

        // Assert
        for (int i = 0; i < newBuffer.Length; i++)
        {
            Assert.AreEqual(testData[i], newBuffer[i]);
        }
    }

    [TestMethod]
    public void FromGorgonPtrNullPtrThrowsNullReferenceException()
    {
        // Arrange
        GorgonPtr<int> ptr = GorgonPtr<int>.NullPtr;

        // Act & Assert
        Assert.ThrowsException<NullReferenceException>(() => GorgonNativeBuffer<int>.FromSpan(ptr));
    }

    [TestMethod]
    public void FromSpanEmptySpanThrowsArgumentException()
    {
        // Arrange
        ReadOnlySpan<int> span = [];

        // Act
        try
        {
            using GorgonNativeBuffer<int> newBuffer = GorgonNativeBuffer<int>.FromSpan(span);
            Assert.Fail("No exception was thrown.");
        }
        // Assert
        catch (ArgumentEmptyException)
        {
            // Expected exception.
        }
    }

    [TestMethod]
    public void FillFillsBufferWithSpecifiedByte()
    {
        // Arrange
        byte fillValue = 0x7f;
        int bufferSize = 5;
        using GorgonNativeBuffer<byte> buffer = new(bufferSize);

        // Act
        buffer.Fill(fillValue);

        // Assert
        for (int i = 0; i < buffer.Length; i++)
        {
            Assert.AreEqual(fillValue, buffer[i]);
        }
    }

    [TestMethod]
    public void AsRefCastsCorrectly()
    {
        // Arrange
        int[] testData = [1, 2, 3, 4, 5];
        using GorgonNativeBuffer<int> buffer = new(testData.Length);
        for (int i = 0; i < testData.Length; i++)
        {
            buffer[i] = testData[i];
        }

        // Act
        ref byte firstByte = ref buffer.AsRef<byte>();

        // Assert
        Assert.AreEqual((byte)testData[0], firstByte);
    }

    [TestMethod]
    public void AsRefWithOffsetCastsCorrectly()
    {
        // Arrange
        int[] testData = [1, 2, 3, 4, 5];
        using GorgonNativeBuffer<int> buffer = new(testData.Length);
        for (int i = 0; i < testData.Length; i++)
        {
            buffer[i] = testData[i];
        }

        // Act
        ref byte thirdByte = ref buffer.AsRef<byte>(2);

        // Assert
        Assert.AreEqual((byte)(testData[0] >> 16), thirdByte);
    }

    [TestMethod]
    public void AsRefNegativeOffsetThrowsArgumentOutOfRangeException()
    {
        // Arrange
        using GorgonNativeBuffer<int> buffer = new(5);

        // Act & Assert
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => buffer.AsRef<byte>(-1));
    }

    [TestMethod]
    public void AsRefOffsetGreaterThanSizeThrowsArgumentOutOfRangeException()
    {
        // Arrange
        using GorgonNativeBuffer<int> buffer = new(5);

        // Act & Assert
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => buffer.AsRef<byte>(buffer.SizeInBytes));
    }

    [TestMethod]
    public void ToNativeBufferCastsCorrectly()
    {
        // Arrange
        int[] testData = [1, 2, 3, 4, 5];
        using GorgonNativeBuffer<int> buffer = new(testData.Length);
        for (int i = 0; i < testData.Length; i++)
        {
            buffer[i] = testData[i];
        }

        // Act
        using GorgonNativeBuffer<byte> newBuffer = buffer.CopyTo<byte>();

        // Assert
        for (int i = 0; i < testData.Length; i++)
        {
            Assert.AreEqual((byte)testData[i], newBuffer[i * sizeof(int)]);
        }
    }

    [TestMethod]
    public void CopyToSuccess()
    {
        // Arrange
        GorgonNativeBuffer<int> sourceBuffer = new(5);
        GorgonNativeBuffer<int> destinationBuffer = new(5);

        // Fill sourceBuffer with some data
        for (int i = 0; i < sourceBuffer.Length; i++)
        {
            sourceBuffer[i] = i + 1;
        }

        // Act
        sourceBuffer.CopyTo(destinationBuffer);

        // Assert
        for (int i = 0; i < sourceBuffer.Length; i++)
        {
            Assert.AreEqual(sourceBuffer[i], destinationBuffer[i]);
        }
    }

    [TestMethod]
    public void CopyToWithSourceIndexAndCountSuccess()
    {
        // Arrange
        GorgonNativeBuffer<int> sourceBuffer = new(5);
        GorgonNativeBuffer<int> destinationBuffer = new(3);

        // Fill sourceBuffer with some data
        for (int i = 0; i < sourceBuffer.Length; i++)
        {
            sourceBuffer[i] = i + 1;
        }

        // Act
        sourceBuffer.CopyTo(destinationBuffer, 1, 3);

        // Assert
        for (int i = 0; i < destinationBuffer.Length; i++)
        {
            Assert.AreEqual(sourceBuffer[i + 1], destinationBuffer[i]);
        }
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void CopyToWithNegativeSourceIndexFailure()
    {
        // Arrange
        GorgonNativeBuffer<int> sourceBuffer = new(5);
        GorgonNativeBuffer<int> destinationBuffer = new(5);

        // Act
        sourceBuffer.CopyTo(destinationBuffer, -1);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void CopyToWithNegativeDestIndexFailure()
    {
        // Arrange
        GorgonNativeBuffer<int> sourceBuffer = new(5);
        GorgonNativeBuffer<int> destinationBuffer = new(5);

        // Act
        sourceBuffer.CopyTo(destinationBuffer, 0, null, -1);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void CopyToWithInvalidCountFailure()
    {
        // Arrange
        GorgonNativeBuffer<int> sourceBuffer = new(5);
        GorgonNativeBuffer<int> destinationBuffer = new(5);

        // Act
        sourceBuffer.CopyTo(destinationBuffer, 0, 6);
    }

    [TestMethod]
    public void CopyToSpanSuccess()
    {
        // Arrange
        GorgonNativeBuffer<int> sourceBuffer = new(5);
        Span<int> destinationSpan = new(new int[5]);

        // Fill sourceBuffer with some data
        for (int i = 0; i < sourceBuffer.Length; i++)
        {
            sourceBuffer[i] = i + 1;
        }

        // Act
        sourceBuffer.CopyTo(destinationSpan);

        // Assert
        for (int i = 0; i < sourceBuffer.Length; i++)
        {
            Assert.AreEqual(sourceBuffer[i], destinationSpan[i]);
        }
    }

    [TestMethod]
    public void CopyToSpanWithSourceIndexAndCountSuccess()
    {
        // Arrange
        GorgonNativeBuffer<int> sourceBuffer = new(5);
        Span<int> destinationSpan = new(new int[3]);

        // Fill sourceBuffer with some data
        for (int i = 0; i < sourceBuffer.Length; i++)
        {
            sourceBuffer[i] = i + 1;
        }

        // Act
        sourceBuffer.CopyTo(destinationSpan, 1, 3);

        // Assert
        for (int i = 0; i < destinationSpan.Length; i++)
        {
            Assert.AreEqual(sourceBuffer[i + 1], destinationSpan[i]);
        }
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void CopyToSpanWithNegativeSourceIndexFailure()
    {
        // Arrange
        GorgonNativeBuffer<int> sourceBuffer = new(5);
        Span<int> destinationSpan = new(new int[5]);

        // Act
        sourceBuffer.CopyTo(destinationSpan, -1);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void CopyToSpanWithInvalidCountFailure()
    {
        // Arrange
        GorgonNativeBuffer<int> sourceBuffer = new(5);
        Span<int> destinationSpan = new(new int[5]);

        // Act
        sourceBuffer.CopyTo(destinationSpan, 0, 6);
    }
}
