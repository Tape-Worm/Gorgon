using System;
using System.Collections.Generic;
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
		}

		public void TestSave()
		{
			string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\test_settings.xml";

			Settings settings = new Settings();
			settings.Path = path;
			settings.StrValue = null;

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
		}
	}
}
