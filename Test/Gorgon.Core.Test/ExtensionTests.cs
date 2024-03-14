using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gorgon.IO;
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
        public void TestReadWriteString()
        {
            string theString = "The quick brown fox.";
            string readValue = string.Empty;

            using (MemoryStream stream = new MemoryStream())
            {
                theString.WriteToStream(stream);

                Assert.IsTrue(stream.Position >= theString.Length + 1);

                stream.Position = 0;

                readValue = stream.ReadString();

                Assert.AreEqual(theString, readValue);

                stream.Position = 0;

                theString.WriteToStream(stream, Encoding.Unicode);

                Assert.IsTrue(stream.Position >= (theString.Length  * 2) + 1);

                stream.Position = 0;

                readValue = stream.ReadString(Encoding.Unicode);

                Assert.AreEqual(theString, readValue);

                theString = new string('A', 10000);

                stream.Position = 0;

                theString.WriteToStream(stream, Encoding.Unicode);

                Assert.IsTrue(stream.Position >= (theString.Length * 2) + 1);

                stream.Position = 0;

                readValue = stream.ReadString(Encoding.Unicode);

                Assert.AreEqual(theString, readValue);
            }
        }

        [TestMethod]
        public void TestPathPart()
        {
            string myPath = @"part||";
            string expected = "part__";

            Assert.AreEqual(expected, myPath.FormatPathPart());

            myPath = @"c:\windows\system32\";
            expected = "c:_windows_system32_";

            Assert.AreEqual(expected, myPath.FormatPathPart());
        }

        [TestMethod]
        public void TestFormatFilename()
        {
            string myFilename = @"c:\windows\System32\dd||<>.dll";
            string expected = "dd____.dll";

            Assert.AreEqual(expected, myFilename.FormatFileName());

            myFilename = @"c:\windows\System32\";
            expected = string.Empty;

            Assert.AreEqual(expected, myFilename.FormatFileName());

            myFilename = @"dd||<>.dll";
            expected = "dd____.dll";

            Assert.AreEqual(expected, myFilename.FormatFileName());

            myFilename = "ddraw.dll";
            expected = "ddraw.dll";

            Assert.AreEqual(expected, myFilename.FormatFileName());

            myFilename = @"c:\windows\system32\ddraw.dll";
            expected = "ddraw.dll";

            Assert.AreEqual(expected, myFilename.FormatFileName());

            myFilename = @"c:/windows/system32/ddraw.dll";
            expected = "ddraw.dll";

            Assert.AreEqual(expected, myFilename.FormatFileName());
        }

        [TestMethod]
        public void TestFormatDirectory()
        {
            string myPath = @"c:\Windows/System32\\dd<>.dll";
            string expected = @"c:\Windows\System32\dd__.dll\";

            Assert.AreEqual(expected, myPath.FormatDirectory(Path.DirectorySeparatorChar));

            myPath = @"c:/windows/System32/ddraw.dll";
            expected = @"c:\windows\System32\ddraw.dll\";

            Assert.AreEqual(expected, myPath.FormatDirectory(Path.DirectorySeparatorChar));

            myPath = @"c:\windows\System32\";
            expected = @"c:/windows/System32/";

            Assert.AreEqual(expected, myPath.FormatDirectory(Path.AltDirectorySeparatorChar));
        }

        [TestMethod]
        public void TestFormatPath()
        {
            string myPath = @"c:\Windows/System32\\dd<>.dll";
            string expected = @"c:\Windows\System32\dd__.dll";

            Assert.AreEqual(expected, myPath.FormatPath(Path.DirectorySeparatorChar));

            expected = @"c:/Windows/System32/dd__.dll";

            Assert.AreEqual(expected, myPath.FormatPath(Path.AltDirectorySeparatorChar));

            // Dir3 will be treated as though it were a file name.
            myPath = @"\Dir1\Dir2\Dir3";
            expected = @"\Dir1\Dir2\Dir3";

            Assert.AreEqual(expected, myPath.FormatPath(Path.DirectorySeparatorChar));

            myPath = @"\Dir1\Dir2\Dir3\";
            expected = @"\Dir1\Dir2\Dir3\";

            Assert.AreEqual(expected, myPath.FormatPath(Path.DirectorySeparatorChar));
        }

        [TestMethod]
        public void TestGetPathParts()
        {
            const string myPath = @"C:\Windows\System32\ddraw.dll";
            string[] parts = myPath.GetPathParts(Path.DirectorySeparatorChar);
            string[] expected = {
                                    "C:",
                                    "Windows",
                                    "System32",
                                    "ddraw.dll"
                                };


            Assert.IsTrue(expected.SequenceEqual(parts));
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
