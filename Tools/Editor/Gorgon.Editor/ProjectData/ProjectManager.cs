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
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Editor.Plugins;
using Gorgon.Editor.Properties;
using Gorgon.Editor.Services;
using Gorgon.IO;
using Gorgon.IO.Providers;
using Newtonsoft.Json;

namespace Gorgon.Editor.ProjectData
{
    /// <summary>
    /// A project manager used to create, destroy, load and save a project.
    /// </summary>
    internal class ProjectManager 
        : IProjectManager
    {
        #region Constants
        // The extension applied to editor project directories.
        private const string EditorProjectDirectoryExtension = ".gorEditProj";        
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the provider service for handling reading and writing project files.
        /// </summary>
        public IFileSystemProviders Providers
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to return a temporary scratch directory for the project.
        /// </summary>
        /// <param name="workspace">The workspace location for the directory.</param>
        /// <returns>The temporary scratch directory.</returns>
        private static DirectoryInfo GetScratchDirectory(DirectoryInfo workspace)
        {
            var scratchLocation = new DirectoryInfo(Path.Combine(workspace.FullName, $"Scratch_{Guid.NewGuid().ToString("N")}"));

            if (!scratchLocation.Exists)
            {
                scratchLocation.Create();
                scratchLocation.Refresh();
            }

            return scratchLocation;
        }

        /// <summary>
        /// Function to return a new project workspace directory.
        /// </summary>
        /// <param name="workspace">The base workspace directory.</param>
        /// <returns>The project workspace directory.</returns>
        private static DirectoryInfo GetProjectWorkspace(DirectoryInfo workspace)
        {
            var result = new DirectoryInfo(Path.ChangeExtension(Path.Combine(workspace.FullName, Guid.NewGuid().ToString("N")), EditorProjectDirectoryExtension));

            if (!result.Exists)
            {
                result.Create();
                result.Refresh();
            }

            return result;
        }

        /// <summary>
        /// Function to persist out the metadata for the project.
        /// </summary>
        /// <param name="project">The project to write out.</param>
        /// <param name="path">The path to the metadata file.</param>
        private void PersistMetadata(IProject project)
        {
            // First, write out the metadata so we cans store it in the project file.
            Program.Log.Print("Writing metadata.", LoggingLevel.Verbose);
            using (var jsonWriter = new StreamWriter(Path.Combine(project.ProjectWorkSpace.FullName, CommonEditorConstants.EditorMetadataFileName), false, Encoding.UTF8))
            {
                jsonWriter.Write(JsonConvert.SerializeObject(project));
                jsonWriter.Flush();
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

            PersistMetadata(project);

            metadataFile.Attributes = FileAttributes.Archive | FileAttributes.Normal;
            metadataFile.Refresh();
        }

        /// <summary>
        /// Function to purge old workspace directories if they were left over (e.g. debug break, crash, etc...)
        /// </summary>
        /// <param name="prevDirectory">The previously used directory path for the project.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="prevDirectory"/> parameter is <b>null</b>.</exception>
        public void PurgeStaleDirectories(DirectoryInfo prevDirectory)
        {
            int count = 0;

            // Attempt to delete multiple times if the directory is locked (explorer is a jerk sometimes).
            while (count < 3)
            {
                try
                {
                    if (!prevDirectory.Exists)
                    {
                        return;
                    }

                    prevDirectory.Delete(true);
                    break;
                }
                catch (Exception ex)
                {
                    Program.Log.LogException(ex);
                    ++count;

                    // Give the system some time. Sometimes explorer will release its lock.
                    System.Threading.Thread.Sleep(250);
                }
            }
        }

        /// <summary>
        /// Function to retrieve the connection to the metadata database file.
        /// </summary>
        /// <param name="workspace">The workspace directory.</param>
        /// <returns>The connection object for the database file.</returns>
        private static SQLiteConnection GetConnection(DirectoryInfo workspace)
        {
            // Build a SQLite database.
            var fileLocation = new FileInfo(Path.Combine(workspace.FullName, CommonEditorConstants.EditorMetadataFileName));
            var conn = new SQLiteConnection($"Data Source={fileLocation.FullName}");

            conn.Open();

            return conn;
        }

        /// <summary>
        /// Function to copy the file system data from a file system file.
        /// </summary>
        /// <param name="fileSystemFile">The file system file to copy.</param>
        /// <param name="provider">The provider to use.</param>
        /// <param name="projectWorkspace">The workspace directory to copy the files into.</param>
        /// <returns>The path to the metadata file.</returns>
        private FileInfo CopyFileSystem(FileInfo fileSystemFile, IGorgonFileSystemProvider provider, DirectoryInfo projectWorkspace)
        {
            IGorgonFileSystem fileSystem = new GorgonFileSystem(provider, Program.Log);

            fileSystem.Mount(fileSystemFile.FullName);

            IGorgonVirtualFile metaData = fileSystem.GetFile(Path.Combine("/", CommonEditorConstants.EditorMetadataFileName));

            // Get all directories and replicate them.
            IEnumerable<IGorgonVirtualDirectory> directories = fileSystem.FindDirectories("/", "*")
                                                            .OrderBy(item => item.FullPath.Length);

            foreach (IGorgonVirtualDirectory directory in directories)
            {
                var dirInfo = new DirectoryInfo(Path.Combine(projectWorkspace.FullName, directory.FullPath.FormatDirectory(Path.DirectorySeparatorChar).Substring(1)));

                if (dirInfo.Exists)
                {
                    continue;
                }

                dirInfo.Create();
            }

            // Copy all files into the directories we just created.
            IEnumerable<IGorgonVirtualFile> files = fileSystem.FindFiles("/", "*")
                                                    .Where(item => item != metaData);

            foreach (IGorgonVirtualFile file in files)
            {
                var fileInfo = new FileInfo(Path.Combine(projectWorkspace.FullName, file.Directory.FullPath.FormatDirectory(Path.DirectorySeparatorChar).Substring(1), file.Name));

                using (Stream readStream = file.OpenStream())
                using (Stream writeStream = fileInfo.OpenWrite())
                {
                    readStream.CopyTo(writeStream);
                }
            }

            var metaDataOutput = new FileInfo(Path.Combine(projectWorkspace.FullName, CommonEditorConstants.EditorMetadataFileName));

            if (metaData == null)
            {                
                return metaDataOutput;
            }
                        
            using (Stream readStream = metaData.OpenStream())
            using (Stream writeStream = metaDataOutput.OpenWrite())
            {
                readStream.CopyTo(writeStream);
            }

            metaDataOutput.Attributes = FileAttributes.Archive | FileAttributes.Normal;
            metaDataOutput.Refresh();

            return metaDataOutput;
        }

        /// <summary>
        /// Function to build the project information from the metadata.
        /// </summary>
        /// <param name="metaDataFile">The metadata file to use.</param>
        /// <param name="scratchArea">The scratch area used for temporary files.</param>
        /// <returns>A new project object.</returns>
        private IProject CreateFromMetadata(FileInfo metaDataFile, DirectoryInfo scratchArea)
        {
            Project result = null;

            Program.Log.Print("Loading project metadata.", LoggingLevel.Verbose);

            using (var reader = new StreamReader(metaDataFile.FullName, Encoding.UTF8))
            {
                string readJsonData = reader.ReadToEnd();

                result = JsonConvert.DeserializeObject<Project>(readJsonData);
                result.ProjectWorkSpace = metaDataFile.Directory;
                result.ProjectScratchSpace = scratchArea;
            }

            if (!string.IsNullOrWhiteSpace(result.WriterPluginName))
            {
                string writerName = result.WriterPluginName;

                result.AssignWriter(Providers.GetWriterByName(writerName));

                if (result.Writer == null)
                {
                    Program.Log.Print($"WARNING: Project has a writer plug in named '{writerName}', but no such plug in was loaded.", LoggingLevel.Verbose);
                }
            }

            // Here we'll check the version and upgrade as needed. Not necessary now since we're in the first go around.

            Program.Log.Print($"Project version {result.Version}, writer plugin: {result.WriterPluginName}.", LoggingLevel.Verbose);

            return result;
        }

        /// <summary>
        /// Function to load a project from a file on disk using the supplied provider.
        /// </summary>
        /// <param name="fileSystemFile">The file to load.</param>
        /// <param name="provider">The provider to use.</param>
        /// <param name="workspace">The workspace directory to copy the files into.</param>
        /// <returns>A new project file.</returns>
        private (IProject project, bool hasMetadata, bool isUpgraded) OpenProject(FileInfo fileSystemFile, IGorgonFileSystemProvider provider, DirectoryInfo workspace)
        {
            DirectoryInfo projectWorkspace = GetProjectWorkspace(workspace);
            DirectoryInfo scratchDir = GetScratchDirectory(workspace);

            FileInfo metaData = CopyFileSystem(fileSystemFile, provider, projectWorkspace);

            // Create the project using the metadata (if we have any).
            Project result = null;

            if (!metaData.Exists)
            {
                Program.Log.Print("No metadata file exists. A new one will be created.", LoggingLevel.Verbose);

                result = new Project(projectWorkspace, scratchDir);
                BuildMetadataDatabase(result, metaData);

                var v2Metadata = new FileInfo(Path.Combine(result.ProjectWorkSpace.FullName, V2MetadataImporter.V2MetadataFilename));

                // We have v2 meatdata, upgrade the file.
                if (v2Metadata.Exists)
                {
                    
                    var importer = new V2MetadataImporter(v2Metadata, Providers);
                    importer.Import(result);

                    // No autoimport needed if we imported everything.
                    return (result, true, true);
                }

                return (result, false, true);
            }

            return (CreateFromMetadata(metaData, scratchDir), true, false);
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
                    // Blow away the directory containing the project scratch data.
                    project.ProjectScratchSpace.Delete(true);
                    project.ProjectScratchSpace.Refresh();
                    break;
                }
                catch (Exception ex)
                {
                    Program.Log.LogException(ex);
                    ++count;

                    Thread.Sleep(250);
                }
            }

            count = 0;

            // Try multiple times if explorer is being a jerk.
            while (count < 3)
            {
                try
                {
                    // Blow away the directory containing the project data.
                    project.ProjectWorkSpace.Delete(true);
                    project.ProjectWorkSpace.Refresh();
                    break;
                }
                catch (Exception ex)
                {
                    Program.Log.LogException(ex);
                    ++count;

                    Thread.Sleep(250);
                }
            }
        }

        /// <summary>
        /// Function to create a project object.
        /// </summary>
        /// <param name="workspace">The directory used as a work space location.</param>
        /// <param name="projectName">The name of the project.</param>
        /// <returns>A new project.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="workspace"/>, or the <paramref name="projectName"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the<paramref name="projectName"/> parameter is empty.</exception>
        public IProject CreateProject(DirectoryInfo workspace, string projectName)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            if (projectName == null)
            {
                throw new ArgumentNullException(nameof(projectName));
            }

            if (string.IsNullOrWhiteSpace(projectName))
            {
                throw new ArgumentEmptyException(nameof(workspace));
            }

            DirectoryInfo scratchDir = GetScratchDirectory(workspace);
            workspace = GetProjectWorkspace(workspace);

            var metadataFile = new FileInfo(Path.Combine(workspace.FullName, CommonEditorConstants.EditorMetadataFileName));

            var result = new Project(workspace, scratchDir);

            BuildMetadataDatabase(result, metadataFile);

            return result;
        }

        /// <summary>
        /// Function to save a project to a file on the disk.
        /// </summary>
        /// <param name="project">The project to save.</param>
        /// <param name="path">The path to the project file.</param>
        /// <param name="writer">The writer plug in used to write the file data.</param>
        /// <param name="progressCallback">The callback method that reports the saving progress to the UI.</param>
        /// <param name="cancelToken">The token used for cancellation of the operation.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="project"/>, <paramref name="path"/>, or the <paramref name="writer"/> parameter is <b>null</b>.</exception>        
        public void SaveProject(IProject project, string path, FileWriterPlugin writer, Action<int, int, bool> progressCallback, CancellationToken cancelToken)
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

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentEmptyException(nameof(path));
            }

            PersistMetadata(project);

            var outputFile = new FileInfo(path);
            using (FileStream stream = outputFile.Open(FileMode.Create, FileAccess.Write, FileShare.None))
            {
                writer.Write(stream, project.ProjectWorkSpace, progressCallback, cancelToken);
            }
            outputFile.Refresh();

            // If we cancelled the operation, we should delete the file.
            if ((cancelToken.IsCancellationRequested) && (outputFile.Exists))
            {
                try
                {                        
                    outputFile.Delete();
                }
                catch (Exception ex)
                {
                    Program.Log.Print($"ERROR: Could not delete the file '{outputFile.FullName}' while cancelling save operation.", LoggingLevel.Simple);
                    Program.Log.LogException(ex);
                }
            }
        }

        /// <summary>
        /// Function to open a project from a file on the disk.
        /// </summary>
        /// <param name="path">The path to the project file.</param>
        /// <param name="providers">The providers used to read the project file.</param>
        /// <param name="workspace">The workspace directory that will receive the files from the project file.</param>
        /// <returns>A new project based on the file that was read.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="path"/>, or the <paramref name="workspace"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="path"/> parameter is empty.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the file specified by the <paramref name="path"/> does not exist.</exception>
        /// <exception cref="GorgonException">Thrown if no provider could be found to load the file.</exception>
        public async Task<(IProject project, bool hasMetadata, bool isUpgraded)> OpenProjectAsync(string path, DirectoryInfo workspace)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentEmptyException(nameof(path));
            }

            var file = new FileInfo(path);

            if (!file.Exists)
            {
                throw new FileNotFoundException(string.Format(Resources.GOREDIT_ERR_PROJECT_NOT_FOUND, file.FullName));
            }
                       
            IGorgonFileSystemProvider provider = Providers.GetBestReader(file);
            
            if (provider == null)
            {
                throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOREDIT_ERR_NO_PROVIDER, file.Name));
            }

            return await Task.Run(() => OpenProject(file, provider, workspace));
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectManager"/> class.
        /// </summary>
        /// <param name="providers">The file system providers used to read and write project files.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="providers"/> parameter is <b>null</b>.</exception>
        public ProjectManager(IFileSystemProviders providers) => Providers = providers ?? throw new ArgumentNullException(nameof(providers));
        #endregion
    }
}
