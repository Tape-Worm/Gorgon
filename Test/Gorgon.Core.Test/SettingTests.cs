using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Gorgon.Core.Test.Support;
using Gorgon.Math;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gorgon.Core.Test
{
    [TestClass]
    public class SettingTests
    {
        [TestMethod]
        public void TestCreate()
        {
            Settings settings = new Settings();

            Assert.AreEqual(123, settings.Value);

            settings.Value = 999;

            Assert.AreEqual(999, settings.Value);

            settings.Reset();

            Assert.AreEqual(123, settings.Value);
        }

        public void TestSave()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\test_settings.xml";

            Settings settings = new Settings();
            settings.Path = path;
            settings.StrValue = null;
            settings.Rectangle = new Rectangle(5, 5, 25, 28);
            settings.TehColor = Color.FromArgb(178, 199, 212, 86);

            for (int i = 0; i < settings.IntArray.Length; ++i)
            {
                settings.IntArray[i] = (i + 1) * (i + 1);
            }

            settings.TehDate = DateTime.Now.Date;

            settings.NamesOfStuff = new string[4];
            var names = GetType().Assembly.GetTypes().Take(4).ToArray();

            for (int i = 0; i < names.Length; ++i)
            {
                settings.NamesOfStuff[i] = names[i].FullName;
            }

            float val = settings.NamesOfStuff[2].Length / 3.12f;

            for (int i = 0; i < 5; ++i)
            {
                settings.Floats.Add(val * val / (i + 1));
            }

            for (int i = 0; i < 10; ++i)
            {
                settings.DateGuids[new Guid(i * i, (short)i, 0, 0, 0, 0, 0, 0, 0, 0, 1).ToString("B")] =
                    new DateTime(2015, 6, 5, 11, i, 13, i * i);
            }

            settings.Save();
        }

        [TestMethod]
        public void TestLoad()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\test_settings.xml";

            TestSave();

            Settings settings = new Settings
                                     {
                                         Path = path
                                     };
            settings.Load();

            float[] expectedFloats =
            {
                267.1968f,
                133.5984f,
                89.0656f,
                66.79919f,
                53.43936f
            };

            int[] expectedInts =
            {
                1,
                4,
                9,
                16,
                25
            };

            DateTime expectedDate = DateTime.Now.Date;
            Dictionary<string, DateTime> expectedDateGuids = new Dictionary<string, DateTime>();

            for (int i = 0; i < 10; ++i)
            {
                expectedDateGuids[new Guid(i * i, (short)i, 0, 0, 0, 0, 0, 0, 0, 0, 1).ToString("B")] =
                    new DateTime(2015, 6, 5, 11, i, 13, i * i);
            }

            Console.WriteLine(DateTime.Now.ToString("o"));

            Assert.IsTrue(expectedInts.SequenceEqual(settings.IntArray));

            for (int i = 0; i < expectedFloats.Length; ++i)
            {
                Assert.IsTrue(expectedFloats[i].EqualsEpsilon(settings.Floats[i]));
            }

            Assert.AreEqual(expectedDate, settings.TehDate);

            Assert.IsTrue(expectedDateGuids.SequenceEqual(settings.DateGuids));

            Assert.AreEqual(123, settings.Value);

            Assert.AreEqual(SettingEnum.Enum2, settings.EnumValue);

            Assert.AreEqual(new Rectangle(5, 5, 25, 28), settings.Rectangle);

            Assert.AreEqual(Color.FromArgb(178, 199, 212, 86), settings.TehColor);
        }

        [TestMethod]
        public void Versiontest()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\test_settings.xml";
            Settings settings = new Settings(new Version(1,0,0,0));
            settings.Path = path;
            settings.StrValue = "Version 1 string";

            settings.Save();

            settings = new Settings(new Version(1, 0, 0, 1));
            settings.Path = path;
            settings.StrValue = "Version 1.0.0.1 string";

            settings.Load();

            Assert.AreEqual("Version 1 string", settings.StrValue);

            settings.Save();

            settings = new Settings(new Version(1, 0, 0, 1));
            settings.Path = path;
            settings.StrValue = "Version 1.0.0.1 string";

            settings.Load();

            Assert.AreEqual("Version 1 string", settings.StrValue);
            Assert.AreEqual(new Version(1, 0, 0, 1), settings.Version);


            settings = new Settings(new Version(0, 0, 0, 1));
            settings.Path = path;
            settings.StrValue = "Version 0.0.0.1 string";

            settings.Load();

            Assert.IsNull(settings.StrValue);
        }
    }
}
