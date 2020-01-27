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
// Created: September 1, 2018 8:41:18 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Editor.Native;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.Properties;
using Gorgon.Editor.Services;
using Gorgon.IO;
using Gorgon.IO.Providers;
using Gorgon.Math;
using Newtonsoft.Json;

namespace Gorgon.Editor.ProjectData
{
    /// <summary>
    /// A project manager used to create, destroy, load and save a project.
    /// </summary>
    internal class ProjectManager
    {
        #region Constants
        // The temporary directory name.
        private const string TemporaryDirectoryName = "tmp";
        // The source directory name.
        private const string SourceDirectoryName = "src";
        // The file system directory name.
        private const string FileSystemDirectoryName = "fs";
        #endregion

        #region Variables.
        // The stream used for the lock file.
        private Stream _lockStream;
        // The log interface for debug messages.
        private readonly IGorgonLog _log;
        // The provider service for handling reading and writing project files.
        public FileSystemProviders _providers;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to set up the required project directories.
        /// </summary>
        /// <param name="basePath">The base path for the project.</param>
        /// <returns>The required folders for the project.</returns>
        private (DirectoryInfo workspace, DirectoryInfo fileSystemDir, DirectoryInfo tempDir, DirectoryInfo srcDir) SetupProjectFolders(string basePath)
        {
            var workspace = new DirectoryInfo(basePath);

            workspace.Create();
            workspace.Refresh();

            var fileSystemDir = new DirectoryInfo(Path.Combine(workspace.FullName, FileSystemDirectoryName));
            var tempDir = new DirectoryInfo(Path.Combine(workspace.FullName, TemporaryDirectoryName));
            var srcDir = new DirectoryInfo(Path.Combine(workspace.FullName, SourceDirectoryName));

            tempDir.Create();
            fileSystemDir.Create();
            srcDir.Create();

            tempDir.Refresh();
            fileSystemDir.Refresh();
            srcDir.Refresh();

            Lock(workspace);

            return (workspace, fileSystemDir, tempDir, srcDir);
        }

        /// <summary>
        /// Function to place a lock file in the project workspace directory.
        /// </summary>
        /// <param name="projectDir">The project workspace directory.</param>
        private void Lock(DirectoryInfo projectDir)
        {
            _lockStream = File.Open(projectDir.FullName.FormatDirectory(Path.DirectorySeparatorChar) + ".gorlock", FileMode.Create, FileAccess.Write, FileShare.None);
            _lockStream.WriteByte(1);
            _lockStream.Flush();
        }

        /// <summary>
        /// Function to place a lock file in the project workspace directory.
        /// </summary>
        /// <param name="projectDir">The project workspace directory.</param>
        private void Unlock(DirectoryInfo projectDir)
        {
            Stream lockStream = Interlocked.Exchange(ref _lockStream, null);

            if (lockStream != null)
            {
                lockStream.Dispose();
            }

            var file = new FileInfo(projectDir.FullName.FormatDirectory(Path.DirectorySeparatorChar) + ".gorlock");

            if (file.Exists)
            {
                file.Delete();
            }
        }

        /// <summary>
        /// Function to build the metadata database for our project.
        /// </summary>
        /// <param name="project">The project that contains the initial data to write.</param>
        /// <param name="metadataFile">The metadata file to write.</param>
        private void BuildMetadataDatabase(Project project, FileInfo metadataFile)
        {
            if (metadataFile.Exists)
            {
                metadataFile.Delete();
                metadataFile.Refresh();
            }

            PersistMetadata(project, CancellationToken.None);

            metadataFile.Attributes = FileAttributes.Archive | FileAttributes.Normal;
            metadataFile.Refresh();
        }

        /// <summary>
        /// Function to purge old workspace directories if they were left over (e.g. debug break, crash, etc...)
        /// </summary>
        /// <param name="prevDirectory">The previously used directory path for the project.</param>
        /// <param name="recycle"><b>true</b> to recycle the directory, or <b>false</b> to permenantly delete it.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="prevDirectory"/> parameter is <b>null</b>.</exception>
        private void PurgeStaleDirectories(DirectoryInfo prevDirectory, bool recycle)
        {
            int count = 0;

            if (!prevDirectory.Exists)
            {
                return;
            }

            var deleteDir = new DirectoryInfo(prevDirectory.FullName);

            // To avoid issues with delete being an asynchronous operation, we will first rename the directory, and then delete it.
            // This way we can recreate the directory structure without interference (one would hope at least).
            if (!recycle)
            {
                deleteDir.MoveTo(Path.Combine(deleteDir.Parent.FullName, Guid.NewGuid().ToString("N")));
            }

            // Attempt to delete multiple times if the directory is locked (explorer is a jerk sometimes).
            while (count < 4)
            {
                try
                {
                    if (!deleteDir.Exists)
                    {
                        return;
                    }

                    if (recycle)
                    {
                        _log.Print($"Moving '{deleteDir.FullName}' to the recycle bin...", LoggingLevel.Intermediate);
                        Shell32.SendToRecycleBin(deleteDir.FullName, Shell32.FileOperationFlags.FOF_SILENT | Shell32.FileOperationFlags.FOF_NOCONFIRMATION | Shell32.FileOperationFlags.FOF_WANTNUKEWARNING);
                    }
                    else
                    {
                        _log.Print($"Deleting '{deleteDir.FullName}'...", LoggingLevel.Intermediate);
                        deleteDir.Delete(true);
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

            prevDirectory.Refresh();
        }

        /// <summary>
        /// Function to copy the file system data from a file system file.
        /// </summary>
        /// <param name="fileSystemFile">The file system file to copy.</param>
        /// <param name="provider">The provider to use.</param>
        /// <param name="fileSystemDir">The workspace directory to copy the files into.</param>
        /// <returns>The path to the metadata file.</returns>
        private async Task<FileInfo> CopyFileSystemAsync(FileInfo fileSystemFile, IGorgonFileSystemProvider provider, DirectoryInfo fileSystemDir)
        {
            IGorgonFileSystem fileSystem = new GorgonFileSystem(provider, _log);

            fileSystem.Mount(fileSystemFile.FullName);

            IGorgonVirtualFile metaData = fileSystem.GetFile(Path.Combine("/", CommonEditorConstants.EditorMetadataFileName));

            // Get all directories and replicate them.
            IEnumerable<IGorgonVirtualDirectory> directories = fileSystem.FindDirectories("/", "*")
                                                            .OrderBy(item => item.FullPath.Length);

            foreach (IGorgonVirtualDirectory directory in directories)
            {
                var dirInfo = new DirectoryInfo(Path.Combine(fileSystemDir.FullName, directory.FullPath.FormatDirectory(Path.DirectorySeparatorChar).Substring(1)));

                if (dirInfo.Exists)
                {
                    continue;
                }

                dirInfo.Create();
            }

            // Copy all files into the directories we just created.
            var files = fileSystem.FindFiles("/", "*")
                                .Where(item => item != metaData)
                                .OrderByDescending(item => item.Size)
                                .ToList();

            int maxJobCount = (Environment.ProcessorCount * 2).Min(32).Max(1);
            int filesPerJob = (int)((float)files.Count / maxJobCount).FastCeiling();
            var jobs = new List<Task>();

            if ((files.Count <= 100) || (maxJobCount < 2))
            {
                filesPerJob = files.Count;
            }

            _log.Print($"Copying file system. {filesPerJob} files will be copied in a single job.", LoggingLevel.Verbose);

            void CopyFile(FileCopyJob job)
            {
                foreach (IGorgonVirtualFile file in job.Files)
                {
                    var fileInfo = new FileInfo(Path.Combine(fileSystemDir.FullName, file.Directory.FullPath.FormatDirectory(Path.DirectorySeparatorChar).Substring(1), file.Name));

                    using (Stream readStream = file.OpenStream())
                    using (Stream writeStream = fileInfo.OpenWrite())
                    {
                        readStream.CopyToStream(writeStream, (int)readStream.Length, job.ReadBuffer);
                    }
                }
            }

            // Build up the tasks for our jobs.
            while (files.Count > 0)
            {
                var jobData = new FileCopyJob();

                // Copy the file information to the file copy job data.
                int length = filesPerJob.Min(files.Count);
                for (int i = 0; i < length; ++i)
                {
                    jobData.Files.Add(files[i]);
                }
                files.RemoveRange(0, length);

                jobs.Add(Task.Run(() => CopyFile(jobData)));
            }

            _log.Print($"{jobs.Count} jobs running for file copy from '{fileSystemFile.FullName}'.", LoggingLevel.Verbose);

            // Wait for the file copy to finish.
            await Task.WhenAll(jobs);

            var metaDataOutput = new FileInfo(Path.Combine(fileSystemDir.FullName, CommonEditorConstants.EditorMetadataFileName));

            if (metaData == null)
            {
                _log.Print($"'{fileSystemFile.FullName}' has no metadata. A new metadata index will be generated.", LoggingLevel.Verbose);
                return metaDataOutput;
            }

            _log.Print($"'{fileSystemFile.FullName}' has metadata. Copying to the .", LoggingLevel.Verbose);
            byte[] writeBuffer = new byte[81920];
            using (Stream readStream = metaData.OpenStream())
            using (Stream writeStream = metaDataOutput.OpenWrite())
            {
                readStream.CopyToStream(writeStream, (int)readStream.Length, writeBuffer);
            }

            metaDataOutput.Attributes = FileAttributes.Archive | FileAttributes.Normal;
            metaDataOutput.Refresh();

            return metaDataOutput;
        }

        /// <summary>
        /// Function to build the project information from the metadata.
        /// </summary>
        /// <param name="metaDataFile">The metadata file to use.</param>
        /// <returns>A new project object.</returns>
        private IProject CreateFromMetadata(FileInfo metaDataFile)
        {
            Project result = null;

            _log.Print("Loading project metadata.", LoggingLevel.Verbose);

            using (var reader = new StreamReader(metaDataFile.FullName, Encoding.UTF8))
            {
                string readJsonData = reader.ReadToEnd();

                result = JsonConvert.DeserializeObject<Project>(readJsonData);
                result.ProjectWorkSpace = metaDataFile.Directory;
                result.FileSystemDirectory = new DirectoryInfo(Path.Combine(result.ProjectWorkSpace.FullName, FileSystemDirectoryName));
                result.TempDirectory = new DirectoryInfo(Path.Combine(result.ProjectWorkSpace.FullName, TemporaryDirectoryName));
                result.SourceDirectory = new DirectoryInfo(Path.Combine(result.ProjectWorkSpace.FullName, SourceDirectoryName));

                if (!result.FileSystemDirectory.Exists)
                {
                    _log.Print("[WARNING] The file system directory was not found.", LoggingLevel.Verbose);
                    result.FileSystemDirectory.Create();
                    result.FileSystemDirectory.Refresh();
                }

                if (!result.TempDirectory.Exists)
                {
                    _log.Print("[WARNING] The temporary directory was not found.", LoggingLevel.Verbose);
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
        public bool IsGorgonProject(DirectoryInfo directory)
        {
            if (directory == null)
            {
                throw new ArgumentNullException(nameof(directory));
            }

            try
            {
                if (!directory.Exists)
                {
                    return false;
                }

                return File.Exists(Path.Combine(directory.FullName, CommonEditorConstants.EditorMetadataFileName));
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
        public bool IsDirectoryLocked(DirectoryInfo workspace)
        {
            Stream fileStream = null;

            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            workspace.Refresh();

            if (!workspace.Exists)
            {
                return false;
            }

            FileInfo file = null;

            try
            {
                file = new FileInfo(workspace.FullName.FormatDirectory(Path.DirectorySeparatorChar) + ".gorlock");

                if (!file.Exists)
                {
                    return false;
                }

                fileStream = file.Open(FileMode.Append, FileAccess.Write, FileShare.None);
                fileStream.WriteByte(2);
                fileStream.Dispose();

                file.Refresh();

                // If the file exists, but is not locked by the OS, then it's orphaned, and we can continue.
                // Just clean up after ourselves before we do.
                if (file.Exists)
                {
                    file.Delete();
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
            if (project == null)
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
                    Unlock(project.ProjectWorkSpace);

                    // Blow away the directory containing the project temporary data.
                    PurgeStaleDirectories(project.TempDirectory, false);
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
        /// <exception cref="GorgonException">Thrown if the <paramref name="workspace"/> directory is locked, and in use by other Gorgon editor instance.</exception>
        public IProject CreateProject(DirectoryInfo workspace)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            workspace.Refresh();

            // Unlock our own lock prior to creation.
            Unlock(workspace);

            if (IsDirectoryLocked(workspace))
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GOREDIT_ERR_PROJECT_OPEN_LOCKED, workspace.FullName));
            }

            // If the workspace directory is already in place, then we need to delete the contents of it.
            if (workspace.Exists)
            {
                PurgeStaleDirectories(workspace, true);
            }

            (DirectoryInfo projectWorkspace, DirectoryInfo projectFileSystem, DirectoryInfo projectTemp, DirectoryInfo projectSource) = SetupProjectFolders(workspace.FullName);

            // Set up the directory that will contain our file system.

            var metadataFile = new FileInfo(Path.Combine(projectWorkspace.FullName, CommonEditorConstants.EditorMetadataFileName));

            var result = new Project(projectWorkspace, projectTemp, projectFileSystem, projectSource);

            BuildMetadataDatabase(result, metadataFile);

            return result;
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
        public async Task SavePackedFileAsync(IProject project, FileInfo path, FileWriterPlugIn writer, Action<int, int, bool> progressCallback, CancellationToken cancelToken)
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            // Move the metadata file into the file system so it can be captured.
            var metadataFile = new FileInfo(Path.Combine(project.ProjectWorkSpace.FullName, CommonEditorConstants.EditorMetadataFileName));
            metadataFile = metadataFile.CopyTo(Path.Combine(project.FileSystemDirectory.FullName, CommonEditorConstants.EditorMetadataFileName), true);

            try
            {
                await writer.WriteAsync(path.FullName, project.FileSystemDirectory, progressCallback, cancelToken);
                path.Refresh();
            }
            finally
            {
                // Remove the copy of the metadata file.
                if (metadataFile.Exists)
                {
                    metadataFile.Delete();
                }
            }
        }

        /// <summary>
        /// Function to open a project workspace directory.
        /// </summary>
        /// <param name="projectWorkspace">The directory pointing at the project workspace.</param>
        /// <returns>The project information for the project data in the work space.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="projectWorkspace"/> parameter is <b>null</b>.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown when the directory in <paramref name="projectWorkspace"/> was not found.</exception>
        /// <exception cref="GorgonException">Thrown when the project is locked by another instance of the Gorgon editor.</exception>
        public IProject OpenProject(DirectoryInfo projectWorkspace)
        {
            if (projectWorkspace == null)
            {
                throw new ArgumentNullException(nameof(projectWorkspace));
            }

            projectWorkspace.Refresh();

            if (!projectWorkspace.Exists)
            {
                throw new DirectoryNotFoundException(string.Format(Resources.GOREDIT_ERR_DIRECTORY_NOT_FOUND, projectWorkspace.FullName));
            }

            // Unlock our own lock prior to opening.
            Unlock(projectWorkspace);

            if (IsDirectoryLocked(projectWorkspace))
            {
                throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOREDIT_ERR_PROJECT_OPEN_LOCKED, projectWorkspace.FullName));
            }

            Lock(projectWorkspace);

            try
            {

                var metaDataFile = new FileInfo(Path.Combine(projectWorkspace.FullName, CommonEditorConstants.EditorMetadataFileName));

                if (!metaDataFile.Exists)
                {
                    throw new DirectoryNotFoundException(string.Format(Resources.GOREDIT_ERR_DIRECTORY_NO_PROJECT, projectWorkspace.FullName));
                }

                return CreateFromMetadata(metaDataFile);
            }
            catch
            {
                // If we have an error, remove the lock right away.
                Unlock(projectWorkspace);
                throw;
            }
        }

        /// <summary>
        /// Function to open a project from a file on the disk.
        /// </summary>
        /// <param name="path">The path to the project file.</param>
        /// <param name="providers">The providers used to read the project file.</param>
        /// <param name="workspace">The workspace directory that will receive the files from the project file.</param>
        /// <returns>A task for asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="path"/>, or the <paramref name="workspace"/> parameter is <b>null</b>.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the file specified by the <paramref name="path"/> does not exist.</exception>
        /// <exception cref="GorgonException">Thrown if no provider could be found to load the file.</exception>
        public async Task OpenPackFileProjectAsync(FileInfo path, DirectoryInfo workspace)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            if (!path.Exists)
            {
                throw new FileNotFoundException(string.Format(Resources.GOREDIT_ERR_PROJECT_NOT_FOUND, path.FullName));
            }

            IGorgonFileSystemProvider provider = _providers.GetBestReader(path);

            if (provider == null)
            {
                throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOREDIT_ERR_NO_PROVIDER, path.Name));
            }

            // Unlock the directory if we have it locked.
            // Unlock our own lock prior to opening.
            Unlock(workspace);

            if (IsDirectoryLocked(workspace))
            {
                throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOREDIT_ERR_PROJECT_OPEN_LOCKED, workspace.FullName));
            }

            if (workspace.Exists)
            {
                PurgeStaleDirectories(workspace, true);
            }

            (DirectoryInfo _, DirectoryInfo fsDir, DirectoryInfo tempDir, DirectoryInfo srcDir) = SetupProjectFolders(workspace.FullName);

            try
            {
                FileInfo metaData = await CopyFileSystemAsync(path, provider, fsDir);

                // Pull the meta data file into the root of the project directory.
                if (metaData.Exists)
                {
                    metaData.MoveTo(Path.Combine(workspace.FullName, metaData.Name));
                    return;
                }

                _log.Print("No metadata file exists. A new one will be created.", LoggingLevel.Verbose);

                // Create a dummy project, so we have something to serialize.
                var dummyProject = new Project(workspace, tempDir, fsDir, srcDir);

                // If we have v2 meatdata, upgrade the file.
                var v2Metadata = new FileInfo(Path.Combine(workspace.FullName, V2MetadataImporter.V2MetadataFilename));
                if (!v2Metadata.Exists)
                {
                    metaData = new FileInfo(Path.Combine(workspace.FullName, metaData.Name));
                    BuildMetadataDatabase(dummyProject, metaData);
                    return;
                }

                var importer = new V2MetadataImporter(v2Metadata, _log);
                importer.Import(dummyProject);
                PersistMetadata(dummyProject, CancellationToken.None);
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
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            // First, write out the metadata so we cans store it in the project file.
            var tempPath = new FileInfo(Path.Combine(project.ProjectWorkSpace.FullName, Guid.NewGuid().ToString("N")));
            StreamWriter jsonWriter = null;

            _log.Print("Writing metadata.", LoggingLevel.Verbose);
            try
            {
                // Write to a transactional file first so we don't corrupt the original.
                jsonWriter = new StreamWriter(tempPath.Open(FileMode.Create, FileAccess.Write, FileShare.None), Encoding.UTF8, 81920, false);
                jsonWriter.Write(JsonConvert.SerializeObject(project));
                jsonWriter.Flush();
                jsonWriter.Close();

                if (cancelToken.IsCancellationRequested)
                {
                    return;
                }

                tempPath.Refresh();

                Debug.Assert(tempPath.Exists, "Transaction file not found for metadata.");

                // When the metadata file is finalized, copy the transaction file over old one to replace (commit phase).
                tempPath.CopyTo(Path.Combine(project.ProjectWorkSpace.FullName, CommonEditorConstants.EditorMetadataFileName), true);
            }
            finally
            {
                jsonWriter?.Dispose();

                if (tempPath.Exists)
                {
                    tempPath.Delete();
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
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            // We've already got this locked, so leave.
            if (_lockStream != null)
            {
                return;
            }

            Lock(project.ProjectWorkSpace);
        }

        /// <summary>
        /// Function to delete a project.
        /// </summary>
        /// <param name="projectPath">The path to the project directory.</param>
        /// <returns><b>true</b> if the directory was found, or <b>false</b> if not.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="projectPath"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="projectPath"/> parameter is empty.</exception>
        public bool DeleteProject(string projectPath)
        {
            if (projectPath == null)
            {
                throw new ArgumentNullException(nameof(projectPath));
            }

            if (string.IsNullOrWhiteSpace(projectPath))
            {
                throw new ArgumentEmptyException(nameof(projectPath));
            }

            if ((!Directory.Exists(projectPath))
                || ((File.GetAttributes(projectPath) & FileAttributes.Directory) != FileAttributes.Directory))
            {
                return false;
            }

            // Send the project to the recycle bin so we can recover it if need be.
            Shell32.SendToRecycleBin(projectPath, Shell32.FileOperationFlags.FOF_SILENT | Shell32.FileOperationFlags.FOF_NOCONFIRMATION | Shell32.FileOperationFlags.FOF_WANTNUKEWARNING);
            return true;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectManager"/> class.
        /// </summary>
        /// <param name="providers">The file system providers used to read and write project files.</param>
        /// <param name="log">The log interface for debug messages.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="providers"/> parameter is <b>null</b>.</exception>
        public ProjectManager(FileSystemProviders providers, IGorgonLog log)
        {
            _log = log ?? GorgonLog.NullLog;
            _providers = providers ?? throw new ArgumentNullException(nameof(providers));            
        }
        #endregion
    }
}
