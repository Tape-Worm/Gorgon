using System;
using System.IO;
using EditorTests.Mocking;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Editor;
using GorgonLibrary.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StructureMap;

namespace EditorTests
{
	[TestClass]
	public class ScratchAreaTests
	{
		// The structuremap container for our objects.
		private static IContainer _container;
		// Our scratch area.
		private IScratchArea _scratchArea;

		/// <summary>
		/// Initializer for our test class.
		/// </summary>
		/// <param name="context">Context for the test.</param>
		[ClassInitialize]
		public static void Initialize(TestContext context)
		{
			_container = new Container(
				obj =>
				{
					obj.ForConcreteType<GorgonLogFile>()
					   .Configure
					   .Ctor<string>("appname").Is("EditorTest")
					   .Ctor<string>("extraPath").Is("EditorTest")
					   .OnCreation("initLog", logFile =>
					   {
						   logFile.LogFilterLevel = LoggingLevel.NoLogging;
					   });

					obj.For<IProxyObject<GorgonFileSystem>>()
					   .Use<FileSystemProxyMock>();

					obj.For<IEditorSettings>()
					   .Use<EditorSettingsMock>();

					obj.For<IScratchArea>()
					   .Use<ScratchArea>();
				});
		}

		/// <summary>
		/// Function to perform initialization on the tests.
		/// </summary>
		[TestInitialize]
		public void InitializeTest()
		{
			_scratchArea = _container.GetInstance<IScratchArea>();
		}

		/// <summary>
		/// Function to perform clean up for testing objects.
		/// </summary>
		[ClassCleanup]
		public static void TearDown()
		{
			_container.Dispose();
		}

		/// <summary>
		/// Test to set a path for the scratch area.
		/// </summary>
		[TestMethod,
		TestCategory("Initialization")]
		public void SetPath()
		{
			Assert.AreEqual(@"A:\scratch_path\", _scratchArea.ScratchDirectory);

			string expected = Path.GetTempPath().FormatDirectory(Path.DirectorySeparatorChar) + "scratch_path" + Path.DirectorySeparatorChar;

			Assert.AreEqual(ScratchAccessibility.Accessible, _scratchArea.SetScratchDirectory(expected));

			Assert.AreEqual(expected, _scratchArea.ScratchDirectory);
		}

		/// <summary>
		/// Test to ensure the paths we pass in can be used.
		/// </summary>
		[TestMethod]
		[TestCategory("Initialization")]
		public void PathSafety()
		{
			string path1 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles).FormatDirectory(Path.DirectorySeparatorChar);
			string path2 = Path.GetTempPath().FormatDirectory(Path.DirectorySeparatorChar) + "scratch_path" + Path.DirectorySeparatorChar;
			string path3 = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)).FormatDirectory(Path.DirectorySeparatorChar);

			Assert.AreNotEqual(ScratchAccessibility.Accessible, _scratchArea.CanAccessScratch(path1));
			Assert.AreEqual(ScratchAccessibility.Accessible, _scratchArea.CanAccessScratch(path2));
			Assert.AreNotEqual(ScratchAccessibility.Accessible, _scratchArea.CanAccessScratch(path3));

			CleanAllPaths();
		}

		/// <summary>
		/// Test to clean up a scratch path.
		/// </summary>
		[TestMethod]
		[TestCategory("Cleanup")]
		public void CleanPath()
		{
			SetPath();

			Assert.IsTrue(_scratchArea.CleanUp());
		}

		/// <summary>
		/// Test to clean up all previous items.
		/// </summary>
		[TestMethod]
		[TestCategory("Cleanup")]
		public void CleanAllPaths()
		{
			SetPath();

			Assert.IsTrue(_scratchArea.CleanUp(true));

			Assert.AreEqual(0, Directory.GetDirectories(_scratchArea.ScratchDirectory).Length);
		}

		/// <summary>
		/// Test to ensure that we cannot set a system path.
		/// </summary>
		[TestMethod]
		[TestCategory("Initialization")]
		public void DoNotSetSystemPath()
		{
			// This will fail.
			Assert.AreNotEqual(ScratchAccessibility.Accessible, _scratchArea.SetScratchDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)));
		}
	}
}
