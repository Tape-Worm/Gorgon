using System;
using System.IO;
using Gorgon.IO.FileSystem;
using Gorgon.IO.FileSystem.Providers;

namespace Gorgon.FileSystem.Tests;

internal sealed class MockFileStream(IGorgonPhysicalFileInfo fileInfo, Stream baseStream, Action<string, string>? onClose)
    : GorgonFileSystemStream(fileInfo, baseStream, onClose)
{

}
