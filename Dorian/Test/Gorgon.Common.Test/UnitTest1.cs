using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GorgonLibrary;

namespace Gorgon.Common.Test
{
    [TestClass]
    public class UnitTest1
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
    }
}
