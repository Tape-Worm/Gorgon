#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: September 5, 2018 1:49:40 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Gorgon.Core;
using Gorgon.Editor;
using Gorgon.Editor.Metadata;
using Gorgon.Editor.ProjectData;
using Gorgon.Editor.Properties;
using Gorgon.Editor.Support;
using Newtonsoft.Json;

namespace Gorgon.IO
{
    /// <summary>
    /// Extension methods for IO functionality.
    /// </summary>
    public static class IOExtensions
    {
        /// <summary>
        /// Function to load the metadata for the project.
        /// </summary>
        /// <param name="fileSystem">The file system we're working with.</param>
        /// <returns>The project metadata interface.</returns>
        internal static IProjectMetadata GetMetadata(this IGorgonFileSystem fileSystem)
        {
            FileInfo externalProjectData = null;
            IGorgonVirtualFile jsonMetaDataFile = fileSystem.GetFile(CommonEditorConstants.EditorMetadataFileName);

            // If we're loading directly from an editor file system folder, then check in the directory above the mount point.
            // The editor places all file system data in a directory called "fs", and the metadata is located in the directory above that, so 
            // we just need to find the mount point for the "fs" directory, and go up one.
            if (jsonMetaDataFile is null)
            {
                foreach (GorgonFileSystemMountPoint mountPoint in fileSystem.MountPoints.Where(item => item.PhysicalPath.EndsWith(Path.DirectorySeparatorChar + "fs" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)))
                {
                    string dirName = Path.GetDirectoryName(mountPoint.PhysicalPath[0..^1]);
                    if (File.Exists(Path.Combine(dirName, CommonEditorConstants.EditorMetadataFileName)))
                    {
                        externalProjectData = new FileInfo(Path.Combine(dirName, CommonEditorConstants.EditorMetadataFileName));
                    }
                }

                if (externalProjectData is null)
                {
                    throw new GorgonException(GorgonResult.CannotRead, Resources.GOREDIT_ERR_NOT_EDITOR_PROJECT);
                }
            }

            int expectedVersion = Convert.ToInt32(CommonEditorConstants.EditorCurrentProjectVersion.Replace("GOREDIT", string.Empty));
            int fileVersion = int.MaxValue;

            using (Stream stream = externalProjectData is null ? jsonMetaDataFile.OpenStream() : externalProjectData.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            using (var jsonReader = new JsonTextReader(reader))
            {
                // First property must be the version #.
                if ((!jsonReader.Read()) || (!jsonReader.Read()))
                {
                    throw new GorgonException(GorgonResult.CannotRead, Resources.GOREDIT_ERR_NOT_EDITOR_PROJECT);
                }

                if ((jsonReader.TokenType != JsonToken.PropertyName)
                    || (!string.Equals(jsonReader.Value.ToString(), nameof(IProjectMetadata.Version), StringComparison.Ordinal)))
                {
                    throw new GorgonException(GorgonResult.CannotRead, Resources.GOREDIT_ERR_NOT_EDITOR_PROJECT);
                }

                if (!int.TryParse(jsonReader.ReadAsString().Replace("GOREDIT", string.Empty), NumberStyles.Integer, CultureInfo.InvariantCulture, out fileVersion))
                {
                    throw new GorgonException(GorgonResult.CannotRead, Resources.GOREDIT_ERR_NOT_EDITOR_PROJECT);
                }

                // Ensure we have the correct version.
                if (expectedVersion < fileVersion)
                {
                    throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOREDIT_ERR_VERSION_MISMATCH, fileVersion, expectedVersion));
                }
            }

            using (Stream stream = externalProjectData is null ? jsonMetaDataFile.OpenStream() : externalProjectData.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                string jsonString = reader.ReadToEnd();

                switch (fileVersion)
                {
                    case 30:
                        EditorProjectMetadata30 oldProjectData = JsonConvert.DeserializeObject<EditorProjectMetadata30>(jsonString);
                        return new EditorProjectMetadata31(oldProjectData);
                    case 31:
                        return JsonConvert.DeserializeObject<EditorProjectMetadata31>(jsonString);
                    default:
                        throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOREDIT_ERR_VERSION_MISMATCH, fileVersion, expectedVersion));
                }
            }
        }

        /// <summary>
        /// Function to convert a file system information object into a relative file system path.
        /// </summary>
        /// <param name="filesystemObject">The file system object containing the path to convert.</param>
        /// <param name="rootDirectory">The physical file system directory that is the root of the relative file system.</param>
        /// <param name="pathSeparator">[Optional] The desired path separator.</param>
        /// <returns>The path to the file system object, relative to the root of the file system. Or, an empty string if the file system object is not on the path of the <paramref name="rootDirectory"/>.</returns>
        public static string ToFileSystemPath(this FileSystemInfo filesystemObject, DirectoryInfo rootDirectory, char pathSeparator = '/')
        {
            if (filesystemObject is null)
            {
                throw new ArgumentNullException(nameof(filesystemObject));
            }

            if (rootDirectory is null)
            {
                throw new ArgumentNullException(nameof(rootDirectory));
            }

            if (!filesystemObject.FullName.StartsWith(rootDirectory.FullName, StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            string fileName = null;
            string dirPath;
            bool isDirectory = ((filesystemObject.Attributes & FileAttributes.Directory) == FileAttributes.Directory) && (filesystemObject is DirectoryInfo);

            if (isDirectory)
            {
                dirPath = filesystemObject.FullName.FormatDirectory(Path.DirectorySeparatorChar);
            }
            else
            {
                dirPath = Path.GetDirectoryName(filesystemObject.FullName).FormatDirectory(Path.DirectorySeparatorChar);
                fileName = filesystemObject.Name;
            }

            string rootDirPath = rootDirectory.FullName.FormatDirectory(Path.DirectorySeparatorChar);

            if ((string.Equals(dirPath, rootDirPath, StringComparison.OrdinalIgnoreCase)) && (isDirectory))
            {
                return pathSeparator.ToString();
            }

            var pathBuilder = new StringBuilder();
            pathBuilder.Append(pathSeparator);
            pathBuilder.Append(dirPath, rootDirPath.Length, dirPath.Length - rootDirPath.Length);
            if (pathSeparator != Path.DirectorySeparatorChar)
            {
                pathBuilder.Replace(Path.DirectorySeparatorChar, pathSeparator);
            }

            return string.IsNullOrWhiteSpace(fileName) ? pathBuilder.ToString() : Path.Combine(pathBuilder.ToString(), fileName);
        }

        /// <summary>
        /// Function to locate specific types of editor content files contained within the file system.
        /// </summary>
        /// <param name="fileSystem">The file system containing the content items.</param>
        /// <param name="path">Path to the directory containing the files to evaluate.</param>
        /// <param name="contentType">The type of content to locate.</param>
        /// <param name="searchMask">[Optional] A mask for filtering the search by file name.</param>
        /// <param name="recursive">[Optional] <b>true</b> to recursively search, <b>false</b> to only search the specified path.</param>
        /// <returns>A list of <see cref="IGorgonVirtualFile"/> items for each content item.</returns>
        /// <remarks>
        /// <para>
        /// Applications can use this to locate specific types of content files within a file system. If <b>null</b> is passed to the <paramref name="contentType"/>, then all content files with 
        /// no content type associated will be returned.
        /// </para>
        /// </remarks>
        public static IReadOnlyList<IGorgonVirtualFile> GetContentItems(this IGorgonFileSystem fileSystem, string path, string contentType, string searchMask = "*", bool recursive = false)
        {
            var result = new List<IGorgonVirtualFile>();

            if (fileSystem is null)
            {
                throw new ArgumentNullException(nameof(fileSystem));
            }

            if (path is null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentEmptyException(nameof(path));
            }

            IProjectMetadata metaData = fileSystem.GetMetadata();

            if (metaData is null)
            {
                return result;
            }

            // We passed in a file name, extract it for a seach pattern.
            if (string.IsNullOrWhiteSpace(searchMask))
            {
                searchMask = "*";
            }

            path = path.FormatDirectory('/');

            IGorgonVirtualDirectory directory = fileSystem.GetDirectory(path);

            if (directory is null)
            {
                throw new DirectoryNotFoundException();
            }

            IEnumerable<IGorgonVirtualFile> files = fileSystem.FindFiles(directory.FullPath, searchMask, recursive);

            // Handle the unassociated files.
            if (string.IsNullOrWhiteSpace(contentType))
            {
                foreach (IGorgonVirtualFile file in files)
                {
                    if (!metaData.ProjectItems.TryGetValue(file.FullPath, out ProjectItemMetadata metaDataItem))
                    {
                        continue;
                    }

                    IReadOnlyDictionary<string, string> attributes = metaDataItem.Attributes;

                    if ((attributes is not null) && (attributes.Count > 0) && (attributes.TryGetValue(CommonEditorConstants.ContentTypeAttr, out _)))
                    {
                        continue;
                    }

                    result.Add(file);
                }
                return result;
            }

            // Filter the list based on the content type we ask for.
            foreach (IGorgonVirtualFile file in files)
            {
                if (!metaData.ProjectItems.TryGetValue(file.FullPath, out ProjectItemMetadata metaDataItem))
                {
                    continue;
                }

                IReadOnlyDictionary<string, string> attributes = metaDataItem.Attributes;

                if ((attributes is null)
                    || (attributes.Count == 0)
                    || (!attributes.TryGetValue(CommonEditorConstants.ContentTypeAttr, out string type))
                    || (!string.Equals(contentType, type, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                result.Add(file);
            }

            return result;
        }
    }
}
