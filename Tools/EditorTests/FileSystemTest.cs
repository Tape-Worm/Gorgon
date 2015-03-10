using System;
using EditorTests.Mocking;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Editor;
using GorgonLibrary.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StructureMap;

namespace EditorTests
{
	[TestClass]
	public class FileSystemTest
	{
		// The structuremap container for our objects.
		private static IContainer _container;
		// Our file system service.
		private IFileSystemService _fileSystemService;

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

					obj.For<IPlugInRegistry>()
					   .Use<PlugInRegistryMock>()
					   .OnCreation(reg => reg.ScanAndLoadPlugIns());

					obj.For<IProxyObject<GorgonFileSystem>>()
					   .Use<ProxyObject<GorgonFileSystem>>();

					obj.For<IProxyObject<GorgonFileSystem>>()
					   .Use<ProxyObject<GorgonFileSystem>>()
					   .Named("packFileSystem");

					obj.For<IEditorSettings>()
					   .Use<EditorSettingsMock>();

					obj.For<IScratchArea>()
					   .Use<ScratchArea>();

					obj.For<IFileSystemService>()
					   .Use<EditorFileSystemService>();
				});
		}

		/// <summary>
		/// Function to perform initialization on the tests.
		/// </summary>
		[TestInitialize]
		public void InitializeTest()
		{
			_fileSystemService = _container.GetInstance<IFileSystemService>();
		}

		/// <summary>
		/// Function to perform clean up for testing objects.
		/// </summary>
		[ClassCleanup]
		public static void TearDown()
		{
			_container.Dispose();
		}

		[TestMethod]
		public void GetProviders()
		{
			string expected =
				"Gorgon packed file (*.gorPack)|*.gorPack|Zip file (*.Zip)|*.Zip|All supported content files (*.gorPack;*.Zip)|*.gorPack;*.Zip|All files|*.*".ToLowerInvariant();
			_fileSystemService.LoadFileSystemProviders();
			
			Assert.IsTrue(_fileSystemService.ReadFileTypes.Length > 0);
			Assert.AreEqual(expected, _fileSystemService.ReadFileTypes.ToLowerInvariant());
		}
	}
}
