using System;
using System.Collections.Generic;
using System.IO;
using Gorgon.IO.Providers;

namespace Gorgon.FileSystem.Tests;
public class PhysFileInfo
    : IGorgonPhysicalFileInfo
{
    public string FullPath
    {
        get;
    }

    public string Name
    {
        get;
    }

    public DateTime CreateDate => DateTime.Now;

    public DateTime LastModifiedDate => DateTime.Now;

    public long Offset => 0;

    public long Length => 123456;

    public string VirtualPath
    {
        get;
    }

    public long? CompressedLength => null;

    public bool IsEncrypted => false;

    public void Refresh()
    {
    }

    public PhysFileInfo(IReadOnlyList<string> files, IReadOnlyList<string> virtFiles, int index, string mapRoot)
    {
        FullPath = files[index];
        Name = Path.GetFileName(FullPath);
        VirtualPath = mapRoot + virtFiles[index];
    }
}
