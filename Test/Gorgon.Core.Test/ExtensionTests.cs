using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gorgon.Core.Test
{
    [TestClass]
    public class ExtensionTests
    {
        [TestMethod]
        public void TestGenerateHash()
        {
            var values = new List<int>();

            for (int i = 0; i < 32767; i++)
            {
                int hash = 281;

                for (int mem = 0; mem < i+1; mem++)
                {
                    hash = hash.GenerateHash(mem);
                }

                Assert.IsFalse(values.Contains(hash), "Hash " + hash + " was duplicated.");

                values.Add(hash);
            }
        }

	    [TestMethod]
	    public void TestStringBuilderIndexOfString()
	    {
			var newString = new StringBuilder("The quick brown fox did something fucking quick and stupid.");

			Assert.IsTrue(newString.IndexOf(".") == newString.ToString().IndexOf(".", StringComparison.CurrentCulture));
			Assert.IsTrue(newString.LastIndexOf(".") == newString.ToString().LastIndexOf(".", StringComparison.CurrentCulture));

			Assert.IsTrue(newString.IndexOf("T") == newString.ToString().IndexOf("T", StringComparison.CurrentCulture));
			Assert.IsTrue(newString.LastIndexOf("T") == newString.ToString().LastIndexOf("T", StringComparison.CurrentCulture));

			Assert.IsTrue(newString.IndexOf("w") == newString.ToString().IndexOf("w", StringComparison.CurrentCulture));
			Assert.IsTrue(newString.LastIndexOf("i") == newString.ToString().LastIndexOf("i", StringComparison.CurrentCulture));

			Assert.IsTrue(newString.IndexOf("brown") == newString.ToString().IndexOf("brown", StringComparison.CurrentCulture));
			Assert.IsTrue(newString.LastIndexOf("quick") == newString.ToString().LastIndexOf("quick", StringComparison.CurrentCulture));

			Assert.IsTrue(newString.IndexOf("stupid.") == newString.ToString().IndexOf("stupid.", StringComparison.CurrentCulture));
			Assert.IsTrue(newString.LastIndexOf("stupid.") == newString.ToString().LastIndexOf("stupid.", StringComparison.CurrentCulture));

			Assert.IsTrue(newString.IndexOf("The") == newString.ToString().IndexOf("The", StringComparison.CurrentCulture));
			Assert.IsTrue(newString.LastIndexOf("The") == newString.ToString().LastIndexOf("The", StringComparison.CurrentCulture));

			Assert.IsTrue(newString.IndexOf("ass") == -1);
			Assert.IsTrue(newString.LastIndexOf("ass") == -1);


	    }
    }
}
