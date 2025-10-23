using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Gorgon.Core;
using Gorgon.IO.FileSystem;

namespace Gorgon.FileSystem.Tests;

[TestClass]
public partial class GorgonFileSystemTests
{

    [TestMethod]
    public void TestMountException()
    {
        GorgonFileSystem fileSystem = new();
        MockProvider mockProvider = new();

        Assert.ThrowsExactly<ArgumentEmptyException>(() => fileSystem.Mount(string.Empty, provider: mockProvider));
        Assert.ThrowsExactly<FileNotFoundException>(() => fileSystem.Mount(@$".\ThisFileWillNotExist{Guid.NewGuid()}.zip", provider: mockProvider));
        Assert.ThrowsExactly<DirectoryNotFoundException>(() => fileSystem.Mount(@$".\ThisDirWillNotExist{Guid.NewGuid()}\", provider: mockProvider));
    }

    [TestMethod]
    public void TestMountAndUnmount()
    {
        GorgonFileSystem fileSystem = new();
        MockProvider mockProvider = new();

        GorgonFileSystemMountPoint mount = fileSystem.Mount(@"::\\mock", provider: mockProvider);

        int dirCount = fileSystem.RootDirectory.GetDirectoryCount();
        int fileCount = fileSystem.RootDirectory.GetFileCount();

        Assert.AreEqual(mockProvider.DirCount, dirCount);
        Assert.AreEqual(mockProvider.FileCount, fileCount);

        fileSystem.Unmount(mount);

        dirCount = fileSystem.RootDirectory.GetDirectoryCount();
        fileCount = fileSystem.RootDirectory.GetFileCount();

        Assert.AreEqual(0, dirCount);
        Assert.AreEqual(0, fileCount);
    }

    [TestMethod]
    public void TestMountAndUnmountWithRoot()
    {
        GorgonFileSystem fileSystem = new();
        MockProvider mockProvider = new();

        GorgonFileSystemMountPoint mount = fileSystem.Mount(@"::\\mock", "/ARoot/Under/", mockProvider);

        IGorgonVirtualDirectory? dir = fileSystem.RootDirectory.Directories["ARoot"];
        Assert.IsNotNull(dir);

        dir = dir.Directories["Under"];
        Assert.IsNotNull(dir);

        int dirCount = dir.GetDirectoryCount();
        int fileCount = dir.GetFileCount();

        Assert.AreEqual(mockProvider.DirCount, dirCount);
        Assert.AreEqual(mockProvider.FileCount, fileCount);

        fileSystem.Unmount(mount);

        dirCount = fileSystem.RootDirectory.GetDirectoryCount();
        fileCount = fileSystem.RootDirectory.GetFileCount();

        Assert.AreEqual(0, dirCount);
        Assert.AreEqual(0, fileCount);
    }

    [TestMethod]
    public void TestGetDirectoriesException()
    {
        GorgonFileSystem fileSystem = new();

        Assert.ThrowsExactly<ArgumentEmptyException>(() => fileSystem.GetDirectory(string.Empty));
    }

    [TestMethod]
    public void TestGetDirectories()
    {
        GorgonFileSystem fileSystem = new();
        MockProvider mockProvider = new();

        fileSystem.Mount(@"::\\mock", provider: mockProvider);

        IGorgonVirtualDirectory? dir = fileSystem.GetDirectory("/Gorgon.FileSystem/Providers");

        Assert.IsNotNull(dir);

        dir = fileSystem.GetDirectory("/DoesNotExist");

        Assert.IsNull(dir);
    }

    [TestMethod]
    public void TestGetFileException()
    {
        GorgonFileSystem fileSystem = new();
        MockProvider mockProvider = new();

        fileSystem.Mount(@"::\\mock", provider: mockProvider);

        Assert.ThrowsExactly<ArgumentEmptyException>(() => fileSystem.GetFile(string.Empty));
    }

    [TestMethod]
    public void TestGetFile()
    {
        GorgonFileSystem fileSystem = new();
        MockProvider mockProvider = new();

        fileSystem.Mount(@"::\\mock", provider: mockProvider);

        IGorgonVirtualFile? file = fileSystem.GetFile("/Gorgon.FileSystem/Providers/GorgonFileSystemProvider.cs");

        Assert.IsNotNull(file);

        file = fileSystem.GetFile("/DoesNotExist/TheFile.txt");

        Assert.IsNull(file);
    }

    [TestMethod]
    public void TestFindDirectoriesException()
    {
        GorgonFileSystem fileSystem = new();
        MockProvider mockProvider = new();

        fileSystem.Mount(@"::\\mock", provider: mockProvider);

        void Body(string dir)
        {
            foreach (IGorgonVirtualDirectory d in fileSystem.FindDirectories(dir))
            {
                Console.WriteLine(d.FullPath);
            }
        }

        Assert.ThrowsExactly<ArgumentEmptyException>(() => Body(string.Empty));
        Assert.ThrowsExactly<DirectoryNotFoundException>(() => Body("/DoesNotExist/"));
    }

    [TestMethod]
    public void TestFindDirectories()
    {
        GorgonFileSystem fileSystem = new();
        MockProvider mockProvider = new();

        fileSystem.Mount(@"::\\mock", provider: mockProvider);

        IGorgonVirtualDirectory[] dirs = [.. fileSystem.FindDirectories("/", "*")];

        Assert.AreEqual(430, dirs.Length);

        dirs = [.. fileSystem.FindDirectories("/", "*lease")];

        Assert.IsTrue(dirs.All(d => d.FullPath.EndsWith("Release/", StringComparison.OrdinalIgnoreCase)));

        dirs = [.. fileSystem.FindDirectories("/", "ob*")];

        Assert.IsTrue(dirs.All(d => d.FullPath.EndsWith("obj/", StringComparison.OrdinalIgnoreCase)));

        dirs = [.. fileSystem.FindDirectories("/", "*_*")];

        Assert.IsTrue(dirs.All(d => d.FullPath.Contains('_', StringComparison.OrdinalIgnoreCase)));

        dirs = [.. fileSystem.FindDirectories("/", "net*.0")];

        Assert.IsTrue(dirs.All(d => d.FullPath.Contains("net8.0", StringComparison.OrdinalIgnoreCase)
                                                   || d.FullPath.Contains("net6.0", StringComparison.OrdinalIgnoreCase)));

        dirs = [.. fileSystem.FindDirectories("/", "Gorgon.FileSystem")];

        Assert.IsTrue(dirs.All(d => d.Name.Equals("Gorgon.FileSystem", StringComparison.OrdinalIgnoreCase)));
    }

    [TestMethod]
    public void TestFindFilesException()
    {
        GorgonFileSystem fileSystem = new();
        MockProvider mockProvider = new();

        fileSystem.Mount(@"::\\mock", provider: mockProvider);

        void Body(string dir)
        {
            foreach (IGorgonVirtualFile d in fileSystem.FindFiles(dir))
            {
                Console.WriteLine(d.FullPath);
            }
        }

        Assert.ThrowsExactly<ArgumentEmptyException>(() => Body(string.Empty));
        Assert.ThrowsExactly<DirectoryNotFoundException>(() => Body("/DoesNotExist/"));
    }

    [TestMethod]
    public void TestFindFiles()
    {
        GorgonFileSystem fileSystem = new();
        MockProvider mockProvider = new();

        fileSystem.Mount(@"::\\mock", provider: mockProvider);

        IGorgonVirtualFile[] files = [.. fileSystem.FindFiles("/", "*")];

        Assert.AreEqual(2833, files.Length);

        files = [.. fileSystem.FindFiles("/", "*provider.cs")];

        Assert.IsTrue(files.All(d => d.Name.EndsWith("Provider.cs", StringComparison.OrdinalIgnoreCase)));

        files = [.. fileSystem.FindFiles("/", "IGorgonFileSystemProvider*")];

        Assert.IsTrue(files.All(d => d.Name.Contains("IGorgonFileSystemProvider", StringComparison.OrdinalIgnoreCase)));

        files = [.. fileSystem.FindFiles("/", "*_*")];

        Assert.IsTrue(files.All(d => d.Name.Contains('_', StringComparison.OrdinalIgnoreCase)));

        files = [.. fileSystem.FindFiles("/", "Gorgon*.cs")];

        Assert.IsTrue(files.All(d => d.Name.StartsWith("Gorgon", StringComparison.OrdinalIgnoreCase)
                                                   && d.Name.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)));

        files = [.. fileSystem.FindFiles("/", "Gorgon.FileSystem.dll")];

        Assert.IsTrue(files.All(d => d.Name.Equals("Gorgon.FileSystem.dll", StringComparison.OrdinalIgnoreCase)));
    }

    [TestMethod]
    public void TestOpenFileReadException()
    {
        GorgonFileSystem fileSystem = new();
        MockProvider mockProvider = new();

        fileSystem.Mount(@"::\\mock", provider: mockProvider);

        Assert.ThrowsExactly<ArgumentEmptyException>(() => fileSystem.OpenStream(string.Empty, false));
        Assert.ThrowsExactly<DirectoryNotFoundException>(() => fileSystem.OpenStream("/FakeDir/File.txt", false));
        Assert.ThrowsExactly<FileNotFoundException>(() => fileSystem.OpenStream("/Gorgon.FileSystem/Providers/", false));
        Assert.ThrowsExactly<FileNotFoundException>(() => fileSystem.OpenStream("/Gorgon.FileSystem/Providers/DoesNotExist.txt", false));
        Assert.ThrowsExactly<GorgonException>(() => fileSystem.OpenStream("/Gorgon.Animation/Gorgon2.ico", true));
    }

    [TestMethod]
    public void TestOpenFileRead()
    {
        GorgonFileSystem fileSystem = new();
        MockProvider mockProvider = new();

        fileSystem.Mount(@"::\\mock", provider: mockProvider);

        IGorgonVirtualFile? file = fileSystem.GetFile("/Gorgon.FileSystem/Providers/GorgonFileSystemProvider.cs");

        Assert.IsNotNull(file);

        using Stream stream = fileSystem.OpenStream("/Gorgon.FileSystem/Providers/GorgonFileSystemProvider.cs", false);

        Assert.IsTrue(stream.CanRead);
        Assert.AreEqual(0, stream.Position);
        Assert.AreEqual(file.Size, stream.Length);

        byte[] bytes = new byte[file.Size];
        stream.ReadExactly(bytes.AsSpan());
        stream.Close();

        Assert.IsFalse(bytes.All(item => item != 0));
    }

    [TestMethod]
    public void TestRefresh()
    {
        GorgonFileSystem fileSystem = new();
        MockProvider mockProvider = new();

        fileSystem.Mount(@"::\\mock", provider: mockProvider);

        int dirCount = fileSystem.RootDirectory.GetDirectoryCount();
        int fileCount = fileSystem.RootDirectory.GetFileCount();

        fileSystem.Refresh();

        Assert.AreEqual(dirCount, fileSystem.RootDirectory.GetDirectoryCount());
        Assert.AreEqual(fileCount, fileSystem.RootDirectory.GetFileCount());

        IGorgonVirtualDirectory? dir = fileSystem.GetDirectory("/Gorgon.FileSystem/Providers");
        Assert.IsNotNull(dir);

        dirCount = dir.GetDirectoryCount();
        fileCount = dir.GetFileCount();

        fileSystem.Refresh("/Gorgon.FileSystem/Providers");

        Assert.AreEqual(dir.GetDirectoryCount(), dirCount);
        Assert.AreEqual(dir.GetFileCount(), fileCount);
    }

    [TestMethod]
    public void TestRefreshSubRoot()
    {
        GorgonFileSystem fileSystem = new();
        MockProvider mockProvider = new();

        fileSystem.Mount(@"::\\mock", "/SubRoot/", mockProvider);

        int dirCount = fileSystem.RootDirectory.GetDirectoryCount();
        int fileCount = fileSystem.RootDirectory.GetFileCount();

        fileSystem.Refresh();

        Assert.AreEqual(dirCount, fileSystem.RootDirectory.GetDirectoryCount());
        Assert.AreEqual(fileCount, fileSystem.RootDirectory.GetFileCount());

        IGorgonVirtualDirectory? dir = fileSystem.GetDirectory("/SubRoot/Gorgon.FileSystem/Providers");
        Assert.IsNotNull(dir);

        dirCount = dir.GetDirectoryCount();
        fileCount = dir.GetFileCount();

        fileSystem.Refresh("/SubRoot/Gorgon.FileSystem/Providers");

        Assert.AreEqual(dir.GetDirectoryCount(), dirCount);
        Assert.AreEqual(dir.GetFileCount(), fileCount);
    }

    [TestMethod]
    public void TestMountWriteArea()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\write");

        Assert.AreEqual(2, fileSystem.MountPoints.Count);
    }

    [TestMethod]
    public void TestFailMountWriteArea()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        Assert.ThrowsExactly<GorgonException>(() => fileSystem.MountWriteArea(@"Not a valid path"));
    }

    [TestMethod]
    public void TestCreateDirectory()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\mock");

        IGorgonVirtualDirectory dir = fileSystem.CreateDirectory("/TestDirectory/Child");

        Assert.IsNotNull(dir.Parent);
        Assert.AreEqual("TestDirectory", dir.Parent.Name);
        Assert.AreEqual("Child", dir.Name);

        IGorgonVirtualDirectory? other = fileSystem.GetDirectory("/TestDirectory/Child");

        Assert.IsNotNull(other);
        Assert.AreEqual(dir, other);
    }

    [TestMethod]
    public void TestCreateDirectoryReadOnlyFileSystem()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);

        Assert.ThrowsExactly<GorgonException>(() => fileSystem.CreateDirectory("/TestDirectory/Child"));
    }

    [TestMethod]
    public void TestCreateDirectoryEmptyPath()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\mock");

        Assert.ThrowsExactly<ArgumentEmptyException>(() => fileSystem.CreateDirectory(string.Empty));
    }

    [TestMethod]
    public void TestCreateDirectoryDuplicatePathPart()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\mock");

        Assert.ThrowsExactly<IOException>(() => fileSystem.CreateDirectory("/Gorgon.Animation/bin/dummy.txt"));
    }

    [TestMethod]
    public void TestDeleteDirectory()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\mock");

        fileSystem.DeleteDirectory("/Gorgon.Animation/bin/Debug/net48");

        Assert.IsNull(fileSystem.GetDirectory("/Gorgon.Animation/bin/Debug/net48"));

        // Test child delete.
        fileSystem.DeleteDirectory("/Gorgon.Animation/bin");

        Assert.IsNull(fileSystem.GetDirectory("/Gorgon.Animation/bin/Debug"));
    }

    [TestMethod]
    public void TestDeleteDirectoryReadOnlyFileSystem()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);

        Assert.ThrowsExactly<GorgonException>(() => fileSystem.DeleteDirectory("/Gorgon.Animation/bin/Debug/net48"));
    }

    [TestMethod]
    public void TestDeleteDirectoryEmptyPath()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);

        Assert.ThrowsExactly<ArgumentEmptyException>(() => fileSystem.DeleteDirectory(string.Empty));
    }

    [TestMethod]
    public void TestDeleteDirectoryMissingDir()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\mock");

        Assert.ThrowsExactly<DirectoryNotFoundException>(() => fileSystem.DeleteDirectory("/Directory/That/Does/Not/Exist/"));
    }

    [TestMethod]
    public void TestDeleteDirectoryReadOnlyProvider()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\AnotherPlace");

        Assert.ThrowsExactly<GorgonException>(() => fileSystem.DeleteDirectory("/Gorgon.Animation/bin"));
    }

    [TestMethod]
    public void TestRenameDirectory()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\mock");

        fileSystem.RenameDirectory("/Gorgon.Animation/bin/Debug/net48", "DotNot");

        Assert.IsNull(fileSystem.GetDirectory("/Gorgon.Animation/bin/Debug/net48"));
        Assert.IsNotNull(fileSystem.GetDirectory("/Gorgon.Animation/bin/Debug/DotNot/"));
        Assert.IsNull(fileSystem.GetFile("/Gorgon.Animation/bin/Debug/net48/Gorgon.Animation.dll"));
        Assert.IsNotNull(fileSystem.GetFile("/Gorgon.Animation/bin/Debug/DotNot/Gorgon.Animation.dll"));
    }

    [TestMethod]
    public void TestRenameDirectoryReadOnlyFileSystem()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);

        Assert.ThrowsExactly<GorgonException>(() => fileSystem.RenameDirectory("/Gorgon.Animation/bin/Debug/net48", "DotNot"));
    }

    [TestMethod]
    public void TestRenameDirectoryEmptySourcePath()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\mock");

        Assert.ThrowsExactly<ArgumentEmptyException>(() => fileSystem.RenameDirectory(string.Empty, "DotNot"));
    }

    [TestMethod]
    public void TestRenameDirectoryEmptyNewName()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\mock");

        Assert.ThrowsExactly<ArgumentEmptyException>(() => fileSystem.RenameDirectory("/Gorgon.Animation/bin/Debug/net48", string.Empty));
    }

    [TestMethod]
    public void TestRenameDirectoryDirectoryNotFound()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\Another");

        Assert.ThrowsExactly<DirectoryNotFoundException>(() => fileSystem.RenameDirectory("/Gorgon.Animation/bin/Debug/nothere", "DotNot"));
    }

    [TestMethod]
    public void TestRenameDirectoryReadOnlyProvider()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\Another");

        Assert.ThrowsExactly<GorgonException>(() => fileSystem.RenameDirectory("/Gorgon.Animation/bin/Debug/net48", "DotNot"));
    }

    [TestMethod]
    public void TestCopyDirectory()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\mock");

        fileSystem.CopyDirectory("/Gorgon.Animation/bin/", "/Gorgon.Core/Collections");

        Assert.IsNotNull(fileSystem.GetFile("/Gorgon.Core/Collections/bin/dummy.txt"));
        Assert.IsNotNull(fileSystem.GetDirectory("/Gorgon.Core/Collections/bin/Debug/"));
    }

    [TestMethod]
    public void TestCopyDirectoryReadOnlyFileSystem()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);

        Assert.ThrowsExactly<GorgonException>(() => fileSystem.CopyDirectory("/Gorgon.Animation/bin/", "/Gorgon.Core/Collections"));
    }

    [TestMethod]
    public void TestCopyDirectorEmptySource()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);

        Assert.ThrowsExactly<ArgumentEmptyException>(() => fileSystem.CopyDirectory(string.Empty, "/Gorgon.Core/Collections"));
    }

    [TestMethod]
    public void TestCopyDirectoryEmptyDest()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);

        Assert.ThrowsExactly<ArgumentEmptyException>(() => fileSystem.CopyDirectory("/Gorgon.Animation/bin/", string.Empty));
    }

    [TestMethod]
    public void TestCopyDirectorySourceNotFound()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\mock");

        Assert.ThrowsExactly<DirectoryNotFoundException>(() => fileSystem.CopyDirectory("/NotFoundDirectory", "/Gorgon.Core/Collections"));
    }

    [TestMethod]
    public void TestCopyDirectoryDestNotFound()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\mock");

        Assert.ThrowsExactly<DirectoryNotFoundException>(() => fileSystem.CopyDirectory("/Gorgon.Animation/bin/", "/Gorgon.Core/Collections/NotFoundDestination"));
    }

    [TestMethod]
    public void TestCopyDirectoryParentUnderChild()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\mock");

        Assert.ThrowsExactly<IOException>(() => fileSystem.CopyDirectory("/Gorgon.Animation/bin/", "/Gorgon.Animation/bin/Debug"));
    }

    [TestMethod]
    public void TestCopyDirectorySameSourceDest()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\mock");

        Assert.ThrowsExactly<IOException>(() => fileSystem.CopyDirectory("/Gorgon.Animation/bin/", "/Gorgon.Animation/"));
    }

    [TestMethod]
    public void TestCopyDirectoryChildAboveParent()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\mock");

        fileSystem.CopyDirectory("/Gorgon.Animation/bin/", "/");

        Assert.IsNotNull(fileSystem.GetFile("/bin/dummy.txt"));
        Assert.IsNotNull(fileSystem.GetDirectory("/bin/Debug/"));
    }

    [TestMethod]
    public void TestCopyDirectorySibling()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\mock");

        fileSystem.CopyDirectory("/Gorgon.Animation/bin/", "/");

        Assert.IsNotNull(fileSystem.GetFile("/bin/dummy.txt"));
        Assert.IsNotNull(fileSystem.GetDirectory("/bin/Debug/"));
    }

    [TestMethod]
    public void TestMoveDirectory()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\mock");

        fileSystem.MoveDirectory("/Gorgon.Animation/bin/", "/Gorgon.Core/Collections");

        Assert.IsNotNull(fileSystem.GetFile("/Gorgon.Core/Collections/bin/dummy.txt"));
        Assert.IsNotNull(fileSystem.GetDirectory("/Gorgon.Core/Collections/bin/Debug/"));
        Assert.IsNull(fileSystem.GetFile("/Gorgon.Animation/bin/dummy.txt"));
        Assert.IsNull(fileSystem.GetDirectory("/Gorgon.Animation/bin/"));
    }

    [TestMethod]
    public void TestMoveDirectoryReadOnlyFileSystem()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);

        Assert.ThrowsExactly<GorgonException>(() => fileSystem.MoveDirectory("/Gorgon.Animation/bin/", "/Gorgon.Core/Collections"));
    }

    [TestMethod]
    public void TestMoveDirectorEmptySource()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);

        Assert.ThrowsExactly<ArgumentEmptyException>(() => fileSystem.MoveDirectory(string.Empty, "/Gorgon.Core/Collections"));
    }

    [TestMethod]
    public void TestMoveDirectoryEmptyDest()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);

        Assert.ThrowsExactly<ArgumentEmptyException>(() => fileSystem.MoveDirectory("/Gorgon.Animation/bin/", string.Empty));
    }

    [TestMethod]
    public void TestMoveDirectorySourceNotFound()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\mock");

        Assert.ThrowsExactly<DirectoryNotFoundException>(() => fileSystem.MoveDirectory("/NotFoundDirectory", "/Gorgon.Core/Collections"));
    }

    [TestMethod]
    public void TestMoveDirectoryDestNotFound()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\mock");

        Assert.ThrowsExactly<DirectoryNotFoundException>(() => fileSystem.MoveDirectory("/Gorgon.Animation/bin/", "/Gorgon.Core/Collections/NotFoundDestination"));
    }

    [TestMethod]
    public void TestMoveDirectoryReadOnlyProvider()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\Another");

        Assert.ThrowsExactly<GorgonException>(() => fileSystem.MoveDirectory("/Gorgon.Animation/bin/", "/Gorgon.Core/Collections"));
    }

    [TestMethod]
    public void TestMoveDirectoryParentUnderChild()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\mock");

        Assert.ThrowsExactly<IOException>(() => fileSystem.MoveDirectory("/Gorgon.Animation/bin/", "/Gorgon.Animation/bin/Debug"));
    }

    [TestMethod]
    public void TestMoveDirectorySibling()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\mock");

        fileSystem.CopyDirectory("/Gorgon.Animation/bin/Release", "/Gorgon.Animation");

        Assert.IsNotNull(fileSystem.GetFile("/Gorgon.Animation/bin/dummy.txt"));
        Assert.IsNotNull(fileSystem.GetDirectory("/Gorgon.Animation/bin/Debug/"));
    }

    [TestMethod]
    public void TestMoveDirectorySameSourceDest()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\mock");

        Assert.ThrowsExactly<IOException>(() => fileSystem.MoveDirectory("/Gorgon.Animation/bin/", "/Gorgon.Animation/"));
    }

    [TestMethod]
    public void TestOpenStreamCreateFile()
    {
        const string expectedText = "This is a test";
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\mock");

        using Stream stream = fileSystem.OpenStream("/Gorgon.FileSystem/Providers/NewFile.txt", true);
        stream.Write(Encoding.UTF8.GetBytes(expectedText).AsSpan());
        stream.Close();

        Assert.IsNotNull(fileSystem.GetFile("/Gorgon.FileSystem/Providers/NewFile.txt"));

        using Stream readStream = fileSystem.OpenStream("/Gorgon.FileSystem/Providers/NewFile.txt", false);
        Span<byte> buffer = new byte[readStream.Length];
        int readCount = readStream.Read(buffer);
        Assert.AreEqual(Encoding.UTF8.GetByteCount(expectedText), readCount);
        readStream.Close();

        string testText = Encoding.UTF8.GetString(buffer);
        Assert.AreEqual(expectedText, testText);
    }

    [TestMethod]
    public void TestOpenStreamWriteReadOnlyFileSystem()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);

        Assert.ThrowsExactly<GorgonException>(() => fileSystem.OpenStream("/Gorgon.FileSystem/Providers/NewFile.txt", true));
    }

    [TestMethod]
    public void TestOpenStreamWriteEmptyPath()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\mock");

        Assert.ThrowsExactly<ArgumentEmptyException>(() => fileSystem.OpenStream(string.Empty, true));
    }

    [TestMethod]
    public void TestOpenStreamWriteDirectoryNotFound()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\mock");

        Assert.ThrowsExactly<DirectoryNotFoundException>(() => fileSystem.OpenStream("/ThisDoesNotExist/Thefile.txt", true));
    }

    [TestMethod]
    public void TestOpenStreamWriteFileNotFound()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\mock");

        Assert.ThrowsExactly<IOException>(() => fileSystem.OpenStream("/Gorgon.Animation/bin/", true));
        Assert.ThrowsExactly<IOException>(() => fileSystem.OpenStream("/Gorgon.Animation/bin", true));
    }

    [TestMethod]
    public void TestFileCopy()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\mock");

        fileSystem.CopyFile("/Gorgon.Animation/Gorgon2.ico", "Gorgon2_Copy.ico");

        Assert.IsNotNull(fileSystem.GetFile("/Gorgon.Animation/Gorgon2_Copy.ico"));

        IGorgonVirtualFile? expectedFile = fileSystem.GetFile("/Gorgon.Animation/Gorgon2_Copy.ico");

        Assert.IsNotNull(expectedFile);

        fileSystem.CopyFile("/Gorgon.Animation/Gorgon2.ico", "/Gorgon.Animation/bin/");

        Assert.IsNotNull(fileSystem.GetFile("/Gorgon.Animation/bin/Gorgon2.ico"));

        fileSystem.CopyFile("/Gorgon.Animation/Gorgon2.ico", "/Gorgon.Animation/Gorgon2_Copy.ico",
                    new GorgonFileSystemCopyOptions((src, dest) => FileConflictResolution.Rename, CancellationToken.None));

        Assert.IsNotNull(fileSystem.GetFile("/Gorgon.Animation/Gorgon2_Copy (1).ico"));

        fileSystem.CopyFile("/Gorgon.Animation/Gorgon2.ico", "/Gorgon.Animation/Gorgon2_Copy.ico",
                    new GorgonFileSystemCopyOptions((src, dest) => FileConflictResolution.Skip, CancellationToken.None));

        IGorgonVirtualFile? actualFile = fileSystem.GetFile("/Gorgon.Animation/Gorgon2_Copy.ico");
        Assert.IsNotNull(actualFile);

        Assert.AreEqual(expectedFile.CreateDate, actualFile.LastModifiedDate);

        fileSystem.CopyFile("/Gorgon.Animation/Gorgon2.ico", "/Gorgon.Animation/Gorgon2_Copy.ico",
                    new GorgonFileSystemCopyOptions((src, dest) => FileConflictResolution.Overwrite, CancellationToken.None));

        actualFile = fileSystem.GetFile("/Gorgon.Animation/Gorgon2_Copy.ico");
        Assert.IsNotNull(actualFile);

        Assert.AreNotEqual(expectedFile.CreateDate, actualFile.CreateDate);

        Assert.ThrowsExactly<IOException>(() => fileSystem.CopyFile("/Gorgon.Animation/Gorgon2.ico", "/Gorgon.Animation/Gorgon2_Copy.ico",
                    new GorgonFileSystemCopyOptions((src, dest) => FileConflictResolution.Exception, CancellationToken.None)));
    }

    [TestMethod]
    public void TestFileCopyReadOnlyFileSystem()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);

        Assert.ThrowsExactly<GorgonException>(() => fileSystem.CopyFile("/Gorgon.Animation/Gorgon2.ico", "Gorgon2_Copy.ico"));
    }

    [TestMethod]
    public void TestFileCopyEmptySource()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\mock");

        Assert.ThrowsExactly<ArgumentEmptyException>(() => fileSystem.CopyFile(string.Empty, "Gorgon2_Copy.ico"));
    }

    [TestMethod]
    public void TestFileCopyEmptyDest()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\mock");

        Assert.ThrowsExactly<ArgumentEmptyException>(() => fileSystem.CopyFile("/Gorgon.Animation/Gorgon2.ico", string.Empty));
    }

    [TestMethod]
    public void TestFileCopyFileNotFound()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\mock");

        Assert.ThrowsExactly<FileNotFoundException>(() => fileSystem.CopyFile("/Gorgon.Animation/DoesNotExist.file", "Gorgon2_Copy.ico"));
    }

    [TestMethod]
    public void TestFileCopySourceDirectoryNotFound()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\mock");

        Assert.ThrowsExactly<DirectoryNotFoundException>(() => fileSystem.CopyFile("/Gorgon.Animation.Not.Here.Either/Gorgon2.ico", "Gorgon2_Copy.ico"));
    }

    [TestMethod]
    public void TestFileCopyDestinationDirectoryNotFound()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\mock");

        Assert.ThrowsExactly<DirectoryNotFoundException>(() => fileSystem.CopyFile("/Gorgon.Animation/Gorgon2.ico", "/SomePlaceNotHere/Gorgon2_Copy.ico"));
    }

    [TestMethod]
    public void TestFileCopySourceMissingFilename()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\mock");

        Assert.ThrowsExactly<FileNotFoundException>(() => fileSystem.CopyFile("/Gorgon.Animation/", "Gorgon2_Copy.ico"));
    }

    [TestMethod]
    public void TestFileCopySourceAndDestSame()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\mock");

        Assert.ThrowsExactly<IOException>(() => fileSystem.CopyFile("/Gorgon.Animation/Gorgon2.ico", "/Gorgon.Animation/Gorgon2.ico"));
        Assert.ThrowsExactly<IOException>(() => fileSystem.CopyFile("/Gorgon.Animation/Gorgon2.ico", "/Gorgon.Animation/"));
        Assert.ThrowsExactly<IOException>(() => fileSystem.CopyFile("/Gorgon.Animation/Gorgon2.ico", "Gorgon2.ico"));
    }

    [TestMethod]
    public void TestFileMove()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\mock");

        fileSystem.MoveFile("/Gorgon.Animation/Gorgon2.ico", "Gorgon2_Move.ico");

        Assert.IsNotNull(fileSystem.GetFile("/Gorgon.Animation/Gorgon2_Move.ico"));


        fileSystem.MoveFile("/Gorgon.Animation/Gorgon.Animation.csproj", "/Gorgon.Animation/bin/");

        Assert.IsNotNull(fileSystem.GetFile("/Gorgon.Animation/bin/Gorgon.Animation.csproj"));

        fileSystem.MoveFile("/Gorgon.Animation/bin/Debug/Gorgon.Animation.XML", "/Gorgon.Animation/bin/Debug/Gorgon2_Move.ico");

        Assert.IsNotNull(fileSystem.GetFile("/Gorgon.Animation/bin/Debug/Gorgon2_Move.ico"));

        fileSystem.MoveFile("/Gorgon.Animation/bin/Debug/Gorgon2_Move.ico", "/Gorgon.Animation/bin/Debug/Gorgon.Animation.XML");

        fileSystem.MoveFile("/Gorgon.Animation/bin/Debug/net48/Gorgon.Animation.pdb", "/Gorgon.Animation/bin/Debug/net48/Gorgon.Animation.xml",
                    new GorgonFileSystemCopyOptions((src, dest) => FileConflictResolution.Rename, CancellationToken.None));

        Assert.IsNotNull(fileSystem.GetFile("/Gorgon.Animation/bin/Debug/net48/Gorgon.Animation (1).xml"));

        IGorgonVirtualFile? expectedFile = fileSystem.GetFile("/Gorgon.Animation/bin/Debug/net48/Gorgon.Animation (1).xml");
        Assert.IsNotNull(expectedFile);

        fileSystem.MoveFile("/Gorgon.Animation/bin/Debug/net48/Gorgon.Animation (1).xml", "/Gorgon.Animation/bin/Debug/net48/Gorgon.Animation.dll",
                    new GorgonFileSystemCopyOptions((src, dest) => FileConflictResolution.Skip, CancellationToken.None));

        IGorgonVirtualFile? actualFile = fileSystem.GetFile("/Gorgon.Animation/bin/Debug/net48/Gorgon.Animation (1).xml");
        Assert.IsNotNull(actualFile);

        Assert.AreEqual(expectedFile.CreateDate, actualFile.LastModifiedDate);

        fileSystem.MoveFile("/Gorgon.Animation/bin/Debug/net48/Gorgon.Animation (1).xml", "/Gorgon.Animation/bin/Debug/net48/Gorgon.Animation.dll",
                    new GorgonFileSystemCopyOptions((src, dest) => FileConflictResolution.Overwrite, CancellationToken.None));

        actualFile = fileSystem.GetFile("/Gorgon.Animation/bin/Debug/net48/Gorgon.Animation.dll");
        Assert.IsNotNull(actualFile);

        Assert.AreNotEqual(expectedFile.CreateDate, actualFile.CreateDate);

        Assert.ThrowsExactly<IOException>(() => fileSystem.MoveFile("/Gorgon.Animation/bin/Debug/net48/Gorgon.Animation.dll", "/Gorgon.Animation/bin/Debug/net48/Gorgon.Animation.xml",
                    new GorgonFileSystemCopyOptions((src, dest) => FileConflictResolution.Exception, CancellationToken.None)));
    }

    [TestMethod]
    public void TestFileMoveReadOnlyFileSystem()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);

        Assert.ThrowsExactly<GorgonException>(() => fileSystem.MoveFile("/Gorgon.Animation/Gorgon2.ico", "Gorgon2_Move.ico"));
    }

    [TestMethod]
    public void TestFileMoveEmptySource()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\mock");

        Assert.ThrowsExactly<ArgumentEmptyException>(() => fileSystem.MoveFile(string.Empty, "Gorgon2_Move.ico"));
    }

    [TestMethod]
    public void TestFileMoveEmptyDest()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\mock");

        Assert.ThrowsExactly<ArgumentEmptyException>(() => fileSystem.MoveFile("/Gorgon.Animation/Gorgon2.ico", string.Empty));
    }

    [TestMethod]
    public void TestFileMoveFileNotFound()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\mock");

        Assert.ThrowsExactly<FileNotFoundException>(() => fileSystem.MoveFile("/Gorgon.Animation/DoesNotExist.file", "Gorgon2_Move.ico"));
    }

    [TestMethod]
    public void TestFileMoveSourceDirectoryNotFound()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\mock");

        Assert.ThrowsExactly<DirectoryNotFoundException>(() => fileSystem.MoveFile("/Gorgon.Animation.Not.Here.Either/Gorgon2.ico", "Gorgon2_Move.ico"));
    }

    [TestMethod]
    public void TestFileMoveDestinationDirectoryNotFound()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\mock");

        Assert.ThrowsExactly<DirectoryNotFoundException>(() => fileSystem.MoveFile("/Gorgon.Animation/Gorgon2.ico", "/SomePlaceNotHere/Gorgon2_Move.ico"));
    }

    [TestMethod]
    public void TestFileMoveSourceMissingFilename()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\mock");

        Assert.ThrowsExactly<FileNotFoundException>(() => fileSystem.MoveFile("/Gorgon.Animation/", "Gorgon2_Move.ico"));
    }

    [TestMethod]
    public void TestFileMoveReadOnlyProvider()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\Another");

        Assert.ThrowsExactly<GorgonException>(() => fileSystem.MoveFile("/Gorgon.Animation/Gorgon2.ico", "Gorgon2_Move.ico"));
    }

    [TestMethod]
    public void TestFileMoveSourceAndDestSame()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\mock");

        Assert.ThrowsExactly<IOException>(() => fileSystem.MoveFile("/Gorgon.Animation/Gorgon2.ico", "/Gorgon.Animation/Gorgon2.ico"));
        Assert.ThrowsExactly<IOException>(() => fileSystem.MoveFile("/Gorgon.Animation/Gorgon2.ico", "/Gorgon.Animation/"));
        Assert.ThrowsExactly<IOException>(() => fileSystem.MoveFile("/Gorgon.Animation/Gorgon2.ico", "Gorgon2.ico"));
    }

    [TestMethod]
    public void TestFileRename()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\mock");

        fileSystem.RenameFile("/Gorgon.Animation/Gorgon2.ico", "Gorgon2_Renamed.ico");

        Assert.IsNotNull(fileSystem.GetFile("/Gorgon.Animation/Gorgon2_Renamed.ico"));
    }

    [TestMethod]
    public void TestFileRenameFileSystemReadOnly()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);

        Assert.ThrowsExactly<GorgonException>(() => fileSystem.RenameFile("/Gorgon.Animation/Gorgon2.ico", "Gorgon2_Renamed.ico"));
    }

    [TestMethod]
    public void TestFileRenameProviderReadOnly()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\Another");

        Assert.ThrowsExactly<GorgonException>(() => fileSystem.RenameFile("/Gorgon.Animation/Gorgon2.ico", "Gorgon2_Renamed.ico"));
    }

    [TestMethod]
    public void TestFileRenameEmptyPath()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\mock");

        Assert.ThrowsExactly<ArgumentEmptyException>(() => fileSystem.RenameFile(string.Empty, "Gorgon2_Renamed.ico"));
    }

    [TestMethod]
    public void TestFileRenameEmptyName()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\mock");

        Assert.ThrowsExactly<ArgumentEmptyException>(() => fileSystem.RenameFile("/Gorgon.Animation/Gorgon2.ico", string.Empty));
    }

    [TestMethod]
    public void TestFileRenameSameName()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\mock");

        Assert.ThrowsExactly<IOException>(() => fileSystem.RenameFile("/Gorgon.Animation/Gorgon2.ico", "Gorgon2.ico"));
    }

    [TestMethod]
    public void TestFileRenameDirectoryNotFound()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\mock");

        Assert.ThrowsExactly<DirectoryNotFoundException>(() => fileSystem.RenameFile("/Gorgon.Animation.NotExist/Gorgon2.ico", "Gorgon2_Rename.ico"));
    }

    [TestMethod]
    public void TestFileRenameFileNotFound()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\mock");

        Assert.ThrowsExactly<FileNotFoundException>(() => fileSystem.RenameFile("/Gorgon.Animation/Gorgon2.NotHere", "Gorgon2_Rename.ico"));
        Assert.ThrowsExactly<FileNotFoundException>(() => fileSystem.RenameFile("/Gorgon.Animation/", "Gorgon2_Rename.ico"));
    }

    [TestMethod]
    public void TestFileRenameDirectoryExists()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\mock");

        Assert.ThrowsExactly<IOException>(() => fileSystem.RenameFile("/Gorgon.Animation/Gorgon2.ico", "bin"));
    }

    [TestMethod]
    public void TestFileDelete()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\mock");

        fileSystem.DeleteFile("/Gorgon.Animation/Gorgon2.ico");

        Assert.IsNull(fileSystem.GetFile("/Gorgon.Animation/Gorgon2.ico"));
    }

    [TestMethod]
    public void TestFileDeleteReadOnlyFileSystem()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);

        Assert.ThrowsExactly<GorgonException>(() => fileSystem.DeleteFile("/Gorgon.Animation/Gorgon2.ico"));
    }

    [TestMethod]
    public void TestFileDeleteReadOnlyProvider()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\Another");

        Assert.ThrowsExactly<GorgonException>(() => fileSystem.DeleteFile("/Gorgon.Animation/Gorgon2.ico"));
    }

    [TestMethod]
    public void TestFileDeleteEmptyPath()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\mock");

        Assert.ThrowsExactly<ArgumentEmptyException>(() => fileSystem.DeleteFile(string.Empty));
    }

    [TestMethod]
    public void TestFileDeleteDirectoryNotFound()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\mock");

        Assert.ThrowsExactly<DirectoryNotFoundException>(() => fileSystem.DeleteFile("/Gorgon.Animation.Not.Found/Gorgon2.ico"));
    }

    [TestMethod]
    public void TestFileDeleteFileNotFound()
    {
        MockProvider mockProvider = new();
        GorgonFileSystem fileSystem = new(writeProvider: mockProvider);

        fileSystem.Mount(@"::\\mock", provider: mockProvider);
        fileSystem.MountWriteArea(@"::\\mock");

        Assert.ThrowsExactly<FileNotFoundException>(() => fileSystem.DeleteFile("/Gorgon.Animation/Gorgon2.not.here"));
    }
}
