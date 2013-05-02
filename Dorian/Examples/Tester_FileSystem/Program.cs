using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GorgonLibrary;
using GorgonLibrary.IO;

namespace Tester_FileSystem
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main(string[] args)
		{
			GorgonFileSystem fileSystem = null;

			fileSystem = new GorgonFileSystem();
			fileSystem.WriteLocation = @"d:\unpak\test123\";
			fileSystem.Mount(@"D:\unpak\test\", "/");

			byte[] testData = fileSystem.ReadFile("/SaveData/testFile.txt");
			
			fileSystem.Mount(@"D:\unpak\SubDir1\", "/SubDir1");
			fileSystem.WriteLocation = string.Empty;
			
			testData = fileSystem.ReadFile("/SubDir1/SubDir4/rt.dds");
			fileSystem.Unmount(@"D:\unpak\SubDir1\", "/SubDir1");
			if (fileSystem.GetFile("/SubDir1/SubDir4/rt.dds") != null)
			{
				testData = fileSystem.ReadFile("/SubDir1/SubDir4/rt.dds");
			}
			testData = fileSystem.ReadFile("/SaveData/testFile.txt");


			/*byte[] testData = Encoding.UTF8.GetBytes("Test 1 2 3");
			fileSystem.WriteFile("/testFile.txt", testData);*/

			Console.ReadKey();
		}
	}
}
