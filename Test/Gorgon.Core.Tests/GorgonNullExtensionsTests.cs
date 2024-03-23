using System;

namespace Gorgon.Core.Tests;

[TestClass]
public class GorgonNullExtensionsTests
{
    [TestMethod]
    public void IsNull()
    {
        object? obj = null;
        object? notNull = new();
        object? dbNull = DBNull.Value;

        Assert.IsTrue(obj.IsNull());
        Assert.IsFalse(notNull.IsNull());
        Assert.IsTrue(dbNull.IsNull());
    }

    [TestMethod]
    public void IfNull()
    {
        string? strValue = null;
        object? doubleValue = DBNull.Value;
        object? intValue = 123;

        Assert.AreEqual("Substituted Value", strValue.IfNull("Substituted Value"));
        Assert.AreEqual(1.5, doubleValue.IfNull(1.5));
        Assert.AreNotEqual(42, intValue.IfNull(42));
    }

    public enum TestEnum
    {
        E1,
        E2,
        E3,
        E4,
        E5,
        E6
    }

    [TestMethod]
    public void AsNullable()
    {
        object? numericValue = null;

        Assert.IsNull(numericValue.AsNullable<int>());

        numericValue = 1;
        Assert.IsInstanceOfType<int?>(numericValue.AsNullable<int>());

        numericValue = TestEnum.E3;
        TestEnum? asNull = numericValue.AsNullable<TestEnum>();

        Assert.IsNotNull(asNull);
        Assert.AreEqual(TestEnum.E3, asNull.Value);

        numericValue = 4;
        asNull = numericValue.AsNullable<TestEnum>();

        Assert.IsNotNull(asNull);
        Assert.AreEqual(TestEnum.E5, asNull.Value);
    }
}
