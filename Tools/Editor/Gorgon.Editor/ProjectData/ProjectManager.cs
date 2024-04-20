
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: September 1, 2018 8:41:18 PM
// 

using System.Buffers;
using System.Diagnostics;
using System.Text;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Editor.Metadata;
using Gorgon.Editor.Native;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.Properties;
using Gorgon.Editor.Services;
using Gorgon.IO;
using Gorgon.IO.Providers;
using Gorgon.Math;
using System.Text.Json;

namespace Gorgon.Editor.ProjectData;

/// <summary>
/// A project manager used to create, destroy, load and save a project
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ProjectManager"/> class
/// </remarks>
/// <param name="providers">The file system providers used to read and write project files.</param>
/// <param name="contentPlugIns">The plug in service used to manage content.</param>
/// <param name="log">The log interface for debug messages.</param>
/// <exception cref="ArgumentNullException">Thrown when the <paramref name="providers"/> parameter is <b>null</b>.</exception>
internal class ProjectManager(FileSystemProviders providers, IGorgonLog log)
{

    // The temporary directory name.
    private const string TemporaryDirectoryName = "tmp";
    // The source directory name.
    private const string SourceDirectoryName = "src";
    // The file system directory name.
    private const string FileSystemDirectoryName = "fs";

    // The stream used for the lock file.
    private Stream _lockStream;
    // The log interface for debug messages.
    private readonly IGorgonLog _log = log ?? GorgonLog.NullLog;
    // The provider service for handling reading and writing project files.
    private readonly FileSystemProviders _providers = providers ?? throw new ArgumentNullException(nameof(providers));

    /// <summary>
    /// Function to set up the required project directories.
    /// </summary>
    /// <param name="basePath">The base path for the project.</param>
    /// <returns>The required folders for the project.</returns>
    private (DirectoryInfo workspace, DirectoryInfo fileSystemDir, DirectoryInfo tempDir, DirectoryInfo srcDir) SetupProjectFolders(string basePath)
    {
        DirectoryInfo workspace = new(basePath);

        workspace.Create();
        workspace.Refresh();

        DirectoryInfo fileSystemDir = new(Path.Combine(workspace.FullName, FileSystemDirectoryName));
        DirectoryInfo tempDir = new(Path.Combine(workspace.FullName, TemporaryDirectoryName));
        DirectoryInfo srcDir = new(Path.Combine(workspace.FullName, SourceDirectoryName));

        tempDir.Create();
        fileSystemDir.Create();
        srcDir.Create();

        tempDir.Refresh();
        fileSystemDir.Refresh();
        srcDir.Refresh();

        Lock(workspace.FullName);

        return (workspace, fileSystemDir, tempDir, srcDir);
    }

    /// <summary>
    /// Function to place a lock file in the project workspace directory.
    /// </summary>
    /// <param name="projectDir">The project workspace directory.</param>
    private void Lock(string projectDir)
    {
        _lockStream = File.Open(projectDir.FormatDirectory(Path.DirectorySeparatorChar) + ".gorlock", FileMode.Create, FileAccess.Write, FileShare.None);
        _lockStream.WriteByte(1);
        _lockStream.Flush();
    }

    /// <summary>
    /// Function to place a lock file in the project workspace directory.
    /// </summary>
    /// <param name="projectDir">The project workspace directory.</param>
    private void Unlock(string projectDir)
    {
        Stream lockStream = Interlocked.Exchange(ref _lockStream, null);

        lockStream?.Dispose();

        string file = projectDir.FormatDirectory(Path.DirectorySeparatorChar) + ".gorlock";

        if (File.Exists(file))
        {
            File.Delete(file);
        }
    }

    /// <summary>
    /// Function to build the metadata database for our project.
    /// </summary>
    /// <param name="project">The project that contains the initial data to write.</param>
    /// <param name="metadataFile">The metadata file to write.</param>
    private void BuildMetadataDatabase(Project project, string metadataFile)
    {
        if (File.Exists(metadataFile))
        {
            File.Delete(metadataFile);
        }

        PersistMetadata(project, CancellationToken.None);

        File.SetAttributes(metadataFile, FileAttributes.Archive | FileAttributes.Normal);
    }

    /// <summary>
    /// Function to purge old workspace directories if they were left over (e.g. debug break, crash, etc...)
    /// </summary>
    /// <param name="prevDirectory">The previously used directory path for the project.</param>
    /// <param name="recycle"><b>true</b> to recycle the directory, or <b>false</b> to permenantly delete it.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="prevDirectory"/> parameter is <b>null</b>.</exception>
    private void PurgeStaleDirectories(string prevDirectory, bool recycle)
    {
        int count = 0;

        if (!Directory.Exists(prevDirectory))
        {
            return;
        }

        // To avoid issues with delete being an asynchronous operation, we will first rename the directory, and then delete it.
        // This way we can recreate the directory structure without interference (one would hope at least).
        if (!recycle)
        {
            string parent = Path.GetDirectoryName(prevDirectory).FormatDirectory(Path.DirectorySeparatorChar);
            string newDirectory = Path.Combine(parent, Guid.NewGuid().ToString("N")).FormatDirectory(Path.DirectorySeparatorChar);
            Directory.Move(prevDirectory, newDirectory);
            prevDirectory = newDirectory;

        }

        // Attempt to delete multiple times if the directory is locked (explorer is a jerk sometimes).
        while (count < 4)
        {
            try
            {
                if (!Directory.Exists(prevDirectory))
                {
                    return;
                }

                if (recycle)
                {
                    _log.Print($"Moving '{prevDirectory}' to the recycle bin...", LoggingLevel.Intermediate);
                    Shell32.SendToRecycleBin(prevDirectory, FileOperationFlags.FOF_SILENT | FileOperationFlags.FOF_NOCONFIRMATION | FileOperationFlags.FOF_WANTNUKEWARNING);
                }
                else
                {
                    _log.Print($"Deleting '{prevDirectory}'...", LoggingLevel.Intermediate);
                    Directory.Delete(prevDirectory, true);
                }
                break;
            }
            catch (Exception ex)
            {
                _log.LogException(ex);
                ++count;

                // Give the system some time. Sometimes explorer will release its lock.
                Thread.Sleep(250);
            }
        }
    }

    /// <summary>
    /// Function to copy the file system data from a file system file.
    /// </summary>
    /// <param name="fileSystemFile">The file system file to copy.</param>
    /// <param name="provider">The provider to use.</param>
    /// <param name="fileSystemDir">The workspace directory to copy the files into.</param>
    /// <returns>The path to the metadata file.</returns>
    private async Task<string> CopyFileSystemAsync(string fileSystemFile, IGorgonFileSystemProvider provider, string fileSystemDir)
    {
        const int blockSize = 524288;
        IGorgonFileSystem fileSystem = new GorgonFileSystem(provider, _log);

        fileSystem.Mount(fileSystemFile);

        IGorgonVirtualFile metaData = fileSystem.GetFile(Path.Combine("/", CommonEditorConstants.EditorMetadataFileName));

        // Get all directories and replicate them.
        IEnumerable<IGorgonVirtualDirectory> directories = fileSystem.FindDirectories("/", "*")
                                                        .OrderBy(item => item.FullPath.Length);

        foreach (IGorgonVirtualDirectory directory in directories)
        {
            string dirPath = Path.Combine(fileSystemDir, directory.FullPath.FormatDirectory(Path.DirectorySeparatorChar)[1..]);

            if (Directory.Exists(dirPath))
            {
                continue;
            }

            Directory.CreateDirectory(dirPath);
        }

        // Copy all files into the directories we just created.
        List<IGorgonVirtualFile> files = [.. fileSystem.FindFiles("/", "*")
                            .Where(item => item != metaData)
                            .OrderByDescending(item => item.Size)];

        int maxJobCount = (Environment.ProcessorCount * 2).Min(32).Max(1);
        int filesPerJob = (int)((float)files.Count / maxJobCount).FastCeiling();
        List<Task> jobs = [];

        if ((files.Count <= 100) || (maxJobCount < 2))
        {
            filesPerJob = files.Count;
        }

        _log.Print($"Copying file system. {filesPerJob} files will be copied in a single job.", LoggingLevel.Verbose);

        void CopyFile(FileCopyJob job)
        {
            foreach (IGorgonVirtualFile file in job.Files)
            {
                string outPath = Path.Combine(fileSystemDir, file.Directory.FullPath.FormatDirectory(Path.DirectorySeparatorChar)[1..], file.Name);

                using Stream readJobStream = file.OpenStream();
                using Stream writeJobStream = File.Open(outPath, FileMode.Create, FileAccess.Write, FileShare.None);
                readJobStream.CopyToStream(writeJobStream, (int)readJobStream.Length, job.ReadBuffer);
            }
        }

        List<byte[]> buffers = new(files.Count);

        // Build up the tasks for our jobs.
        while (files.Count > 0)
        {
            FileCopyJob jobData = new()
            {
                ReadBuffer = ArrayPool<byte>.Shared.Rent(blockSize)
            };
            buffers.Add(jobData.ReadBuffer);

            // Copy the file information to the file copy job data.
            int length = filesPerJob.Min(files.Count);
            for (int i = 0; i < length; ++i)
            {
                jobData.Files.Add(files[i]);
            }
            files.RemoveRange(0, length);

            jobs.Add(Task.Run(() => CopyFile(jobData)));
        }

        _log.Print($"{jobs.Count} jobs running for file copy from '{fileSystemFile}'.", LoggingLevel.Verbose);

        // Wait for the file copy to finish.
        await Task.WhenAll(jobs);

        foreach (byte[] buffer in buffers)
        {
            ArrayPool<byte>.Shared.Return(buffer, true);
        }
        buffers.Clear();

        string metaDataOutput = Path.Combine(fileSystemDir, CommonEditorConstants.EditorMetadataFileName);

        if (metaData is null)
        {
            _log.Print($"'{fileSystemFile}' has no metadata. A new metadata index will be generated.", LoggingLevel.Verbose);
            return metaDataOutput;
        }

        _log.Print($"'{fileSystemFile}' has metadata. Copying to the .", LoggingLevel.Verbose);

        byte[] writeBuffer = ArrayPool<byte>.Shared.Rent(blockSize);
        Stream readStream = metaData.OpenStream();
        Stream writeStream = File.Open(metaDataOutput, FileMode.Create, FileAccess.Write, FileShare.None);
        try
        {
            readStream.CopyToStream(writeStream, blockSize, writeBuffer);
        }
        finally
        {
            readStream.Dispose();
            writeStream.Dispose();
            ArrayPool<byte>.Shared.Return(writeBuffer, true);
        }

        File.SetAttributes(metaDataOutput, FileAttributes.Archive | FileAttributes.Normal);

        return metaDataOutput;
    }

    /// <summary>
    /// Function to retrieve the version from the database file.
    /// </summary>
    /// <param name="json">The JSON document to parse.</param>
    /// <param name="location">The directory containing the metadata file.</param>
    /// <returns>The version of the project.</returns>
    /// <exception cref="GorgonException">Thrown if the project was not a valid editor project.</exception>
    private string GetVersion(JsonDocument json, string location)
    {
        JsonProperty versionElement = json.RootElement.EnumerateObject().FirstOrDefault();

        // If the first element is not the version, then this is not a valid project.
        if (!string.Equals(versionElement.Name, nameof(IProjectMetadata.Version), StringComparison.Ordinal))
        {
            throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOREDIT_ERR_DIRECTORY_NO_PROJECT, location));
        }

        string version = versionElement.Value.GetString();

        return !version.StartsWith(CommonEditorConstants.EditorProjectHeader, StringComparison.Ordinal)
            ? throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOREDIT_ERR_DIRECTORY_NO_PROJECT, location))
            : version;
    }

    /// <summary>
    /// Function to build the project information from the metadata.
    /// </summary>
    /// <param name="metaDataFile">The metadata file to use.</param>
    /// <returns>A new project object.</returns>
    private IProject CreateFromMetadata(string metaDataFile)
    {
        Project result = null;
        string location = Path.GetDirectoryName(metaDataFile).FormatDirectory(Path.DirectorySeparatorChar);

        _log.Print("Loading project metadata.", LoggingLevel.Verbose);

        using (StreamReader reader = new(metaDataFile, Encoding.UTF8))
        {
            using JsonDocument jsonDoc = JsonDocument.Parse(reader.ReadToEnd());
            string projectVersion = GetVersion(jsonDoc, location);

            switch (projectVersion)
            {
                case CommonEditorConstants.Editor30ProjectVersion:
                    Project30 project30 = jsonDoc.Deserialize<Project30>();
                    result = new Project(project30);
                    break;
                case CommonEditorConstants.EditorCurrentProjectVersion:
                    result = jsonDoc.Deserialize<Project>();
                    break;
                default:
                    throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOREDIT_ERR_DIRECTORY_NO_PROJECT, location));
            }

            result.ProjectWorkSpace = new DirectoryInfo(Path.GetDirectoryName(metaDataFile));
            result.FileSystemDirectory = new DirectoryInfo(Path.Combine(result.ProjectWorkSpace.FullName, FileSystemDirectoryName));
            result.TempDirectory = new DirectoryInfo(Path.Combine(result.ProjectWorkSpace.FullName, TemporaryDirectoryName));
            result.SourceDirectory = new DirectoryInfo(Path.Combine(result.ProjectWorkSpace.FullName, SourceDirectoryName));

            if (!result.FileSystemDirectory.Exists)
            {
                _log.PrintWarning("The file system directory was not found.", LoggingLevel.Verbose);
                result.FileSystemDirectory.Create();
                result.FileSystemDirectory.Refresh();
            }

            if (!result.TempDirectory.Exists)
            {
                _log.PrintWarning("The temporary directory was not found.", LoggingLevel.Verbose);
                result.TempDirectory.Create();
                result.TempDirectory.Refresh();
            }

            if (!result.SourceDirectory.Exists)
            {
                result.SourceDirectory.Create();
                result.SourceDirectory.Refresh();
            }
        }

        return result;
    }

    /// <summary>
    /// Function to determine if a directory is a Gorgon project directory or not.
    /// </summary>
    /// <param name="directory">The directory to examine.</param>
    /// <returns><b>true</b> if the directory is a Gorgon directory, <b>false</b> if it is not.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="directory"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Throw whent he <paramref name="directory"/> parameter is empty.</exception>
    public bool IsGorgonProject(string directory)
    {
        if (directory is null)
        {
            throw new ArgumentNullException(nameof(directory));
        }

        if (string.IsNullOrWhiteSpace(directory))
        {
            throw new ArgumentEmptyException(nameof(directory));
        }

        try
        {
            directory = Path.GetFullPath(directory).FormatDirectory(Path.DirectorySeparatorChar);

            return Directory.Exists(directory) && File.Exists(Path.Combine(directory, CommonEditorConstants.EditorMetadataFileName));
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Function to determine if the project workspace directory is locked.
    /// </summary>
    /// <param name="workspace">The directory to use as a project workspace.</param>
    /// <returns><b>true</b> if the project is locked, <b>false</b> if not.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="workspace"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="workspace"/> parameter is empty.</exception>
    public bool IsDirectoryLocked(string workspace)
    {
        Stream fileStream = null;

        if (workspace is null)
        {
            throw new ArgumentNullException(nameof(workspace));
        }

        if (string.IsNullOrWhiteSpace(workspace))
        {
            throw new ArgumentEmptyException(nameof(workspace));
        }

        workspace = Path.GetFullPath(workspace).FormatDirectory(Path.DirectorySeparatorChar);

        if (!Directory.Exists(workspace))
        {
            return false;
        }

        string file = workspace + ".gorlock";

        try
        {

            if (!File.Exists(file))
            {
                return false;
            }

            fileStream = File.Open(file, FileMode.Append, FileAccess.Write, FileShare.None);
            fileStream.WriteByte(2);
            fileStream.Dispose();

            // If the file exists, but is not locked by the OS, then it's orphaned, and we can continue.
            // Just clean up after ourselves before we do.
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }
        catch
        {
            // If the file is open, then the directory is considered locked.
            // Security exceptions also indicate that the directory is locked.
            return true;
        }
        finally
        {
            fileStream?.Dispose();
        }

        return false;
    }

    /// <summary>
    /// Function to close the project and clean up its working data.
    /// </summary>
    /// <param name="project">The project to close.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="workspace" /> parameter is <b>null</b>.</exception>
    public void CloseProject(IProject project)
    {
        if (project is null)
        {
            throw new ArgumentNullException(nameof(project));
        }

        int count = 0;

        // Try multiple times if explorer is being a jerk.
        while (count < 3)
        {
            try
            {
                // Ensure the project directory is unlocked.
                Unlock(project.ProjectWorkSpace.FullName);

                // Blow away the directory containing the project temporary data.
                PurgeStaleDirectories(project.TempDirectory.FullName, false);
                project.TempDirectory.Create();
                project.TempDirectory.Refresh();
                break;
            }
            catch (Exception ex)
            {
                _log.LogException(ex);
                ++count;

                Thread.Sleep(250);
            }
        }
    }

    /// <summary>
    /// Function to create a project object.
    /// </summary>
    /// <param name="workspace">The directory used as a work space location.</param>
    /// <returns>A new project.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="workspace"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="workspace"/> parameter is empty.</exception>
    /// <exception cref="GorgonException">Thrown if the <paramref name="workspace"/> directory is locked, and in use by other Gorgon editor instance.</exception>
    public IProject CreateProject(string workspace)
    {
        if (workspace is null)
        {
            throw new ArgumentNullException(nameof(workspace));
        }

        if (string.IsNullOrWhiteSpace(workspace))
        {
            throw new ArgumentEmptyException(nameof(workspace));
        }

        workspace = Path.GetFullPath(workspace).FormatDirectory(Path.DirectorySeparatorChar);

        // Unlock our own lock prior to creation.
        Unlock(workspace);

        if (IsDirectoryLocked(workspace))
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GOREDIT_ERR_PROJECT_OPEN_LOCKED, workspace));
        }

        // If the workspace directory is already in place, then we need to delete the contents of it.
        if (Directory.Exists(workspace))
        {
            PurgeStaleDirectories(workspace, true);
        }

        (DirectoryInfo projectWorkspace, DirectoryInfo projectFileSystem, DirectoryInfo projectTemp, DirectoryInfo projectSource) = SetupProjectFolders(workspace);

        // Set up the directory that will contain our file system.

        string metadataFile = Path.Combine(projectWorkspace.FullName, CommonEditorConstants.EditorMetadataFileName);

        Project result = new(projectWorkspace, projectTemp, projectFileSystem, projectSource);

        BuildMetadataDatabase(result, metadataFile);

        return result;
    }

    /// <summary>
    /// Function to exclude the directories that won't be used in the packed file.
    /// </summary>
    /// <param name="excludedDirs">The list of excluded directories.</param>
    /// <param name="exclude"><b>true</b> to exclude the directories, <b>false</b> to restore.</param>
    private void ExcludeDirs(IReadOnlyList<(string virtDir, string physDir)> excludedDirs, bool exclude)
    {
        foreach ((_, string physDir) in excludedDirs)
        {
            FileAttributes curAttr = File.GetAttributes(physDir);

            if (exclude)
            {
                curAttr |= FileAttributes.Hidden;
            }
            else
            {
                curAttr &= ~FileAttributes.Hidden;
            }

            File.SetAttributes(physDir, curAttr);
        }
    }

    /// <summary>
    /// Function to save a project to a packed file on the disk.
    /// </summary>
    /// <param name="project">The project to save.</param>
    /// <param name="path">The path to the project file.</param>
    /// <param name="writer">The writer plug in used to write the file data.</param>
    /// <param name="progressCallback">The callback method that reports the saving progress to the UI.</param>
    /// <param name="cancelToken">The token used for cancellation of the operation.</param>
    /// <returns>A task for asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="project"/>, <paramref name="path"/>, or the <paramref name="writer"/> parameter is <b>null</b>.</exception>        
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="path"/> parameter is empty.</exception>
    public async Task SavePackedFileAsync(IProject project, string path, FileWriterPlugIn writer, Action<int, int, bool> progressCallback, CancellationToken cancelToken)
    {
        if (project is null)
        {
            throw new ArgumentNullException(nameof(project));
        }

        if (path is null)
        {
            throw new ArgumentNullException(nameof(path));
        }

        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentEmptyException(nameof(path));
        }

        if (writer is null)
        {
            throw new ArgumentNullException(nameof(writer));
        }

        path = Path.GetFullPath(path);
        string destPath = Path.Combine(project.FileSystemDirectory.FullName, CommonEditorConstants.EditorMetadataFileName);

        // Move the metadata file into the file system so it can be captured.
        string metadataFile = Path.Combine(project.ProjectWorkSpace.FullName, CommonEditorConstants.EditorMetadataFileName);
        File.Copy(metadataFile, destPath, true);

        try
        {
            List<(string virtDir, string physDir)> excludedPaths = [];

            foreach (KeyValuePair<string, ProjectItemMetadata> metaData in project.ProjectItems)
            {
                if ((metaData.Value.Attributes.TryGetValue(CommonEditorConstants.ExcludedAttrName, out string excluded))
                    && (string.Equals(excluded, bool.TrueString, StringComparison.OrdinalIgnoreCase)))
                {
                    excludedPaths.Add((metaData.Key, project.FileSystemDirectory.FullName + metaData.Key.FormatDirectory(Path.DirectorySeparatorChar)));
                }
            }

            await Task.Run(() => ExcludeDirs(excludedPaths, true), cancelToken);

            await writer.WriteAsync(path, project.FileSystemDirectory, progressCallback, cancelToken);

            await Task.Run(() => ExcludeDirs(excludedPaths, false), cancelToken);
        }
        finally
        {
            // Remove the copy of the metadata file.
            if (File.Exists(destPath))
            {
                File.Delete(destPath);
            }
        }
    }

    /// <summary>
    /// Function to open a project workspace directory.
    /// </summary>
    /// <param name="projectWorkspace">The directory pointing at the project workspace.</param>
    /// <returns>The project information for the project data in the work space.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="projectWorkspace"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="projectWorkspace"/> parameter is empty.</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown when the directory in <paramref name="projectWorkspace"/> was not found.</exception>
    /// <exception cref="GorgonException">Thrown when the project is locked by another instance of the Gorgon editor.</exception>
    public IProject OpenProject(string projectWorkspace)
    {
        if (projectWorkspace is null)
        {
            throw new ArgumentNullException(nameof(projectWorkspace));
        }

        if (string.IsNullOrWhiteSpace(projectWorkspace))
        {
            throw new ArgumentEmptyException(nameof(projectWorkspace));
        }

        projectWorkspace = Path.GetFullPath(projectWorkspace).FormatDirectory(Path.DirectorySeparatorChar);

        if (!Directory.Exists(projectWorkspace))
        {
            throw new DirectoryNotFoundException(string.Format(Resources.GOREDIT_ERR_DIRECTORY_NOT_FOUND, projectWorkspace));
        }

        // Unlock our own lock prior to opening.
        Unlock(projectWorkspace);

        if (IsDirectoryLocked(projectWorkspace))
        {
            throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOREDIT_ERR_PROJECT_OPEN_LOCKED, projectWorkspace));
        }

        Lock(projectWorkspace);

        try
        {

            string metaDataFile = Path.Combine(projectWorkspace, CommonEditorConstants.EditorMetadataFileName);

            return !File.Exists(metaDataFile)
                ? throw new DirectoryNotFoundException(string.Format(Resources.GOREDIT_ERR_DIRECTORY_NO_PROJECT, projectWorkspace))
                : CreateFromMetadata(metaDataFile);
        }
        catch
        {
            // If we have an error, remove the lock right away.
            Unlock(projectWorkspace);
            throw;
        }
    }

    /// <summary>
    /// Function to extract a project from a packed file on the disk.
    /// </summary>
    /// <param name="path">The path to the project file.</param>
    /// <param name="workspace">The workspace directory that will receive the files from the project file.</param>
    /// <returns><b>true</b> if the file extracted is an editor file system. <b>false</b> if it's an older, or unknown file system.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="path"/>, or the <paramref name="workspace"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="path"/>, or the <paramref name="workspace"/> parameter is empty.</exception>
    /// <exception cref="FileNotFoundException">Thrown if the file specified by the <paramref name="path"/> does not exist.</exception>
    /// <exception cref="GorgonException">Thrown if no provider could be found to load the file.</exception>
    public async Task<bool> ExtractPackFileProjectAsync(string path, string workspace)
    {
        if (path is null)
        {
            throw new ArgumentNullException(nameof(path));
        }

        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentEmptyException(nameof(path));
        }

        if (workspace is null)
        {
            throw new ArgumentNullException(nameof(workspace));
        }

        if (string.IsNullOrWhiteSpace(workspace))
        {
            throw new ArgumentEmptyException(nameof(workspace));
        }

        path = Path.GetFullPath(path);
        workspace = Path.GetFullPath(workspace).FormatDirectory(Path.DirectorySeparatorChar);

        if (!File.Exists(path))
        {
            throw new FileNotFoundException(string.Format(Resources.GOREDIT_ERR_PROJECT_NOT_FOUND, path));
        }

        IGorgonFileSystemProvider provider = _providers.GetBestReader(path) ?? throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOREDIT_ERR_NO_PROVIDER, Path.GetFileNameWithoutExtension(path)));

        // Unlock the directory if we have it locked.
        // Unlock our own lock prior to opening.
        if (IsDirectoryLocked(workspace))
        {
            throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOREDIT_ERR_PROJECT_OPEN_LOCKED, workspace));
        }

        if (Directory.Exists(workspace))
        {
            PurgeStaleDirectories(workspace, true);
        }

        (DirectoryInfo _, DirectoryInfo fsDir, DirectoryInfo tempDir, DirectoryInfo srcDir) = SetupProjectFolders(workspace);

        try
        {
            string metaData = await CopyFileSystemAsync(path, provider, fsDir.FullName);
            string metaDataFileName = Path.GetFileName(metaData);

            // Pull the meta data file into the root of the project directory.
            if (File.Exists(metaData))
            {
                File.Move(metaData, Path.Combine(workspace, metaDataFileName));
                return true;
            }

            _log.Print("No metadata file exists. A new one will be created.", LoggingLevel.Verbose);

            // Create a dummy project, so we have something to serialize.
            Project dummyProject = new(new DirectoryInfo(workspace), tempDir, fsDir, srcDir);

            // If we have v2 meatdata, upgrade the file.
            string v2Metadata = Path.Combine(fsDir.FullName, V2MetadataImporter.V2MetadataFilename);
            if (!File.Exists(v2Metadata))
            {
                metaData = Path.Combine(workspace, metaDataFileName);
                BuildMetadataDatabase(dummyProject, metaData);
                return false;
            }

            V2MetadataImporter importer = new(v2Metadata, _log);
            importer.Import(dummyProject);
            PersistMetadata(dummyProject, CancellationToken.None);

            return false;
        }
        finally
        {
            // For the file import, we will unlock the directory once we're done writing into it.
            // The open project functionality will re-establish the lock after this method completes.
            Unlock(workspace);
        }
    }

    /// <summary>
    /// Function to persist out the metadata for the project.
    /// </summary>
    /// <param name="project">The project to write out.</param>
    /// <param name="cancelToken">The token used to cancel the operation.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="project"/> parameter is <b>null</b>.</exception>
    public void PersistMetadata(IProject project, CancellationToken cancelToken)
    {
        if (project is null)
        {
            throw new ArgumentNullException(nameof(project));
        }

        // First, write out the metadata so we cans store it in the project file.
        string tempPath = Path.Combine(project.ProjectWorkSpace.FullName, Guid.NewGuid().ToString("N"));
        StreamWriter jsonWriter = null;

        _log.Print("Writing metadata.", LoggingLevel.Verbose);
        try
        {
            // Write to a transactional file first so we don't corrupt the original.
            jsonWriter = new StreamWriter(File.Open(tempPath, FileMode.Create, FileAccess.Write, FileShare.None), Encoding.UTF8, 81920, false);
            jsonWriter.Write(JsonSerializer.Serialize(project));
            jsonWriter.Flush();
            jsonWriter.Close();

            if (cancelToken.IsCancellationRequested)
            {
                return;
            }

            Debug.Assert(File.Exists(tempPath), "Transaction file not found for metadata.");

            // When the metadata file is finalized, copy the transaction file over old one to replace (commit phase).
            File.Copy(tempPath, Path.Combine(project.ProjectWorkSpace.FullName, CommonEditorConstants.EditorMetadataFileName), true);
        }
        finally
        {
            jsonWriter?.Dispose();

            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }

    /// <summary>
    /// Function used to lock the project to this instance of the application only.
    /// </summary>
    /// <param name="project">The project to lock.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="project"/> parameter is <b>null</b>.</exception>
    /// <remarks>
    /// <para>
    /// This method locks down the project working directory so that only this instance of the editor may use it. All other instances will exception if an attempt to open the directory is made.
    /// </para>
    /// </remarks>
    public void LockProject(IProject project)
    {
        if (project is null)
        {
            throw new ArgumentNullException(nameof(project));
        }

        // We've already got this locked, so leave.
        if (_lockStream is not null)
        {
            return;
        }

        Lock(project.ProjectWorkSpace.FullName);
    }

    /// <summary>
    /// Function to delete a project.
    /// </summary>
    /// <param name="projectPath">The path to the project directory.</param>
    /// <returns><b>true</b> if the directory was found, or <b>false</b> if not.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="projectPath"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="projectPath"/> parameter is empty.</exception>
    /// <exception cref="GorgonException">Thrown when the <paramref name="projectPath"/> is open in another instance (or the same instance) of the editor.</exception>
    public bool DeleteProject(string projectPath)
    {
        if (projectPath is null)
        {
            throw new ArgumentNullException(nameof(projectPath));
        }

        if (string.IsNullOrWhiteSpace(projectPath))
        {
            throw new ArgumentEmptyException(nameof(projectPath));
        }

        projectPath = Path.GetFullPath(projectPath).FormatDirectory(Path.DirectorySeparatorChar);

        if ((!Directory.Exists(projectPath))
            || ((File.GetAttributes(projectPath) & FileAttributes.Directory) != FileAttributes.Directory))
        {
            return false;
        }

        if (IsDirectoryLocked(projectPath))
        {
            throw new GorgonException(GorgonResult.AccessDenied, string.Format(Resources.GOREDIT_ERR_PROJECT_OPEN_LOCKED, projectPath));
        }

        // Send the project to the recycle bin so we can recover it if need be.
        Shell32.SendToRecycleBin(projectPath, FileOperationFlags.FOF_SILENT | FileOperationFlags.FOF_NOCONFIRMATION | FileOperationFlags.FOF_WANTNUKEWARNING);
        return true;
    }
}
