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
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using Gorgon.Core;
using Gorgon.Editor.Properties;

namespace Gorgon.Editor.ProjectData
{
    /// <summary>
    /// A project manager used to create, destroy, load and save a project.
    /// </summary>
    internal class ProjectManager : IProjectManager
    {
        #region Queries.
        // The query used to create a table for our base metadata.
        private const string ProjectMetadataCreate = @"CREATE TABLE IF NOT EXISTS ProjectMetadata (
                                                            ProjectName TEXT NOT NULL,
                                                            Version TEXT NOT NULL,
                                                            WorkspaceLocation TEXT NOT NULL,
                                                            ShowExternalObjects INT NOT NULL
                                                       ) ";

        // The query used to create a table for file/folder exclusions in the project.
        private const string ExclusionMetadataCreate = @"CREATE TABLE IF NOT EXISTS IncludedPathMetadata (
                                                            FullPath TEXT NOT NULL,
                                                            PRIMARY KEY (FullPath)
                                                       ) ";

        // The query used to create the inital project metadata.
        private const string AddProjectMetadata = @"INSERT INTO ProjectMetadata (ProjectName, Version, WorkspaceLocation, ShowExternalObjects) VALUES (@PName, @PVersion, @PWorkspace, @PShowExtern)";

        // The query used to retrieve the name of the project from the metadata.
        private const string GetProjectNameFromMetadata = @"SELECT ProjectName FROM ProjectMetadata";
        #endregion

        #region Properties.

        #endregion

        #region Methods.
        /// <summary>
        /// Function to build the metadata database for our project.
        /// </summary>
        /// <param name="project">The project that contains the initial data to write.</param>
        private static void BuildMetadataDatabase(Project project)
        {
            if (project.MetadataFile.Exists)
            {
                project.MetadataFile.Delete();
                project.MetadataFile.Refresh();
            }

            using (SQLiteConnection conn = GetConnection(project.ProjectWorkSpace))
            {
                using (var command = new SQLiteCommand(ProjectMetadataCreate, conn))
                {
                    command.ExecuteNonQuery();
                }

                using (var command = new SQLiteCommand(ExclusionMetadataCreate, conn))
                {
                    command.ExecuteNonQuery();
                }

                using (var command = new SQLiteCommand(AddProjectMetadata, conn))
                {
                    command.Parameters.AddWithValue("PName", project.ProjectName);
                    command.Parameters.AddWithValue("PVersion", CommonEditorConstants.EditorCurrentProjectVersion);
                    command.Parameters.AddWithValue("PWorkspace", project.ProjectWorkSpace.FullName);
                    command.Parameters.AddWithValue("PShowExtern", Convert.ToInt32(project.ShowExternalItems));

                    command.ExecuteNonQuery();
                }
            }

            project.MetadataFile.Attributes = FileAttributes.Hidden | FileAttributes.Archive | FileAttributes.Normal;
            project.MetadataFile.Refresh();
        }

        /// <summary>
        /// Function to destroy the project directory.
        /// </summary>
        /// <param name="directory">The directory to destroy.</param>
        /// <remarks>
        /// <para>
        /// This method will destroy any and all files and/or subdirectories contained within the <paramref name="directory"/> path. Use with caution.
        /// </para>
        /// </remarks>
        private static void DestroyProjectDirectory(DirectoryInfo directory)
        {
            directory.Refresh();

            if (!directory.Exists)
            {
                return;
            }

            DirectoryInfo[] subDirs = directory.GetDirectories("*", SearchOption.AllDirectories)
                                               .OrderByDescending(item => item.FullName.Length)
                                               .ToArray();
            FileInfo[] files = directory.GetFiles("*", SearchOption.AllDirectories);

            // If we have nothing in this directory, then there's no need to continue.
            if ((files.Length == 0) && (subDirs.Length == 0))
            {
                return;
            }

            // Delete every file in the directories first.
            foreach (FileInfo file in files)
            {
                file.Delete();
            }

            // Go through each of the subdirectories repeat the process.
            foreach (DirectoryInfo subDir in subDirs)
            {
                subDir.Delete();
            }

            directory.Refresh();
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
        /// Function to close the project and clean up its working data.
        /// </summary>
        /// <param name="workspace">The workspace directory for the project.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="workspace"/> parameter is <b>null</b>.</exception>
        public void CloseProject(DirectoryInfo workspace)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            try
            {
                // TODO: Actually, do we really want to blow away the directory all the time?  Doesn't really make sense and 
                // TODO: could serve as a backup should something go wrong in the saved file.
                // TODO: Instead, whenever we open a file, we should blow away the directory contents and unpack accordingly.
                // TODO: Same with creating a new file.  
                // TODO: We'll leave this here for now, but we'll pull it off of the interface. If we need it, we can use it.
                // TODO: Otherwise we'll drop it.
                //DestroyProjectDirectory(workspace);
            }
            catch (Exception ex)
            {
                Program.Log.LogException(ex);
            }
        }

        /// <summary>
        /// Function to indicate that the selected directory already contains a project.
        /// </summary>
        /// <param name="path">The path to evaluate.</param>
        /// <returns>A flag to indicate whether a project is located in the directory, and the name of the project if there's one present.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="path"/> parameter is <b>null</b>.</exception>
        /// <remarks>
        /// <para>
        /// If this method returns <b>false</b>, but the <c>projectName</c> return value is populated, then an exception was thrown during the check. In this case, the <c>projectName</c> value will contain
        /// the exception error message.
        /// </para>
        /// </remarks>
        public (bool hasProject, string projectName) HasProject(DirectoryInfo path)
        {
            SQLiteConnection conn = null;
            SQLiteCommand command = null;

            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            try
            {
                path.Refresh();
                if (!path.Exists)
                {
                    return (false, null);
                }

                // Look for the project metadata file.
                FileInfo file = path.GetFiles(CommonEditorConstants.EditorMetadataFileName).FirstOrDefault();

                if (file == null)
                {
                    return (false, null);
                }

                conn = GetConnection(path);
                command = new SQLiteCommand(GetProjectNameFromMetadata, conn);
                object val = command.ExecuteScalar(CommandBehavior.CloseConnection);

                return (true, val.IfNull(Resources.GOREDIT_NEW_PROJECT));
            }
            catch (Exception ex)
            {
                Program.Log.LogException(ex);
                return (false, ex.Message);
            }
            finally
            {
                command?.Dispose();
                conn?.Dispose();
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

            // TODO: Disabled for testing.
            // Remove the previously built directory structure.
            //DestroyProjectDirectory(workspace);

            // Build the root of our work space.
            //workspace.Create();
            workspace.Refresh();

            var metadataFile = new FileInfo(Path.Combine(workspace.FullName, CommonEditorConstants.EditorMetadataFileName));

            var result = new Project(projectName, metadataFile)
                            {
                                ShowExternalItems = true
                            };

            BuildMetadataDatabase(result);

            return result;
        }
        #endregion

        #region Constructor/Finalizer.
        
        #endregion
    }
}
