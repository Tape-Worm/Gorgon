using System;
using Gorgon.Configuration;

namespace Gorgon.Core.Tests;

[TestClass]
public class GorgonOptionBagTests
{
    [TestMethod]
    public void TestNullRefStuff()
    {
        IGorgonOption[] option = [
            GorgonOption.CreateDateTimeOption("Test1", DateTime.Now, "DateTime Test", new DateTime(DateTime.Now.Year - 1, 1, 1), new DateTime(DateTime.Now.Year + 1, 12, 31)),
            GorgonOption.CreateInt32Option("Test2", 1234, "Int Test", 234, 12340),
            GorgonOption.CreateDecimalOption("Test3", 666, "Dec Test")
        ];
        GorgonOptionBag bag = new(option);

        bag.SetOptionValue<DateTime>("Test1", new DateTime(DateTime.Now.Year + 2, 1, 2));
        DateTime val = bag.GetOptionValue<DateTime>("Test1");

        Assert.AreEqual(bag[0].GetMaxValue<DateTime>(), val);

        bag.SetOptionValue<int>("Test2", 0);
        int intVal = bag.GetOptionValue<int>("Test2");

        Assert.AreEqual(bag[1].GetMinValue<int>(), intVal);

        bag.SetOptionValue<decimal>("Test3", 0);
        decimal decVal = bag.GetOptionValue<decimal>("Test3");

        Assert.AreEqual(0, decVal);
    }
}
