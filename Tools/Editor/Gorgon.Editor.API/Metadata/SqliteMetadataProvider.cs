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
// Created: September 5, 2018 12:35:03 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using Gorgon.Core;
using Gorgon.Editor.ProjectData;

namespace Gorgon.Editor.Metadata
{
    /// <summary>
    /// Metadata access interface.
    /// </summary>
    public class SqliteMetadataProvider 
        : IMetadataProvider
    {
        #region Queries.
        // Query to remove all included paths from the metadata.
        private const string DeleteIncludedPaths = "DELETE FROM IncludedPathMetadata WHERE FullPath = @PPath";
        // Query to add an included path to the metadata.
        private const string InsertIncludedPath = @"INSERT OR REPLACE INTO IncludedPathMetadata (FullPath, PluginName ) VALUES (@PPath, @PPlugin)";
        // Query to retrieve all included paths from the metadata.
        private const string SelectIncludedPaths = "SELECT FullPath, PluginName FROM IncludedPathMetadata";
        // Query to update the project metadata header.
        private const string UpdateProjectHeader = "UPDATE ProjectMetadata SET ProjectName=@PName, Version=@PVersion, WriterName=@PWriter, ShowExternalObjects=@PShowExtern";
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the file pointing to the metadata database.
        /// </summary>
        public FileInfo MetadataFile
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve the SQLite connection to the metadata database file.
        /// </summary>
        /// <returns>The open connection to the database file.</returns>
        private SQLiteConnection GetConnection()
        {
            var conn = new SQLiteConnection($"Data Source={MetadataFile.FullName}");
            conn.Open();

            return conn;
        }

        /// <summary>
        /// Function to update the included paths.
        /// </summary>
        /// <param name="command">The command to execute against the database.</param>
        /// <param name="updated">The list of updated paths.</param>
        /// <param name="deleted">The list of deleted paths.</param>
        private void UpdateIncludedPaths(SQLiteCommand command, IEnumerable<IncludedFileSystemPathMetadata> updated, IEnumerable<string> deleted)
        {                               
            command.CommandText = DeleteIncludedPaths;
            SQLiteParameter param = command.Parameters.Add("@PPath", DbType.String);            

            foreach (string deletePath in deleted)
            {
                param.Value = deletePath;
                command.ExecuteNonQuery();
            }
                                               
            command.CommandText = InsertIncludedPath;
            SQLiteParameter param1 = command.Parameters.Add("@PPlugin", DbType.String);

            foreach (IncludedFileSystemPathMetadata path in updated)
            {
                param.Value = path.Path;
                param1.Value = path.PluginName;
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Function to update project data and metadata.
        /// </summary>
        /// <param name="title">The title of the project.</param>
        /// <param name="writerType">The type of writer used to write the project data.</param>
        /// <param name="project">The project data to send.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="title"/>, <paramref name="writerType"/>, or the <paramref name="project"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="title"/>, or the <paramref name="writerType"/> parameter is empty.</exception>
        public void UpdateProjectData(string title, string writerType, IProject project)
        {
            if (title == null)
            {
                throw new ArgumentNullException(nameof(title));
            }

            if (writerType == null)
            {
                throw new ArgumentNullException(nameof(writerType));
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentEmptyException(nameof(title));
            }

            if (string.IsNullOrWhiteSpace(writerType))
            {
                throw new ArgumentEmptyException(nameof(writerType));
            }

            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            SQLiteConnection conn = GetConnection();
            SQLiteCommand command = null;
            SQLiteTransaction trans = null;

            try
            {
                var deleted = new HashSet<string>(GetIncludedPaths().Select(item => item.Path), StringComparer.OrdinalIgnoreCase);

                // Get all items that are deleted from the updated list.
                deleted.ExceptWith(project.Metadata.IncludedPaths.Select(item => item.Path));

                trans = conn.BeginTransaction();
                command = new SQLiteCommand(UpdateProjectHeader, conn, trans);
                command.Parameters.AddWithValue("PName", title);
                command.Parameters.AddWithValue("PVersion", CommonEditorConstants.EditorCurrentProjectVersion);
                command.Parameters.AddWithValue("PWriter", writerType);
                command.Parameters.AddWithValue("PShowExtern", project.ShowExternalItems ? 1 : 0);

                command.ExecuteNonQuery();

                command.Parameters.Clear();

                UpdateIncludedPaths(command, project.Metadata.IncludedPaths, deleted);

                trans.Commit();

                command.Parameters.Clear();
                command.CommandText = "VACUUM; ANALYZE";
                command.ExecuteNonQuery();
            }
            catch
            {
                trans?.Rollback();
                throw;
            }
            finally
            {
                trans?.Dispose();
                command?.Dispose();
                conn.Dispose();
            }
        }

        /// <summary>
        /// Function to retrieve the list of files included in this project.
        /// </summary>
        /// <returns>A list of included paths.</returns>
        public IList<IncludedFileSystemPathMetadata> GetIncludedPaths()
        {
            using (SQLiteConnection conn = GetConnection())
            using (var command = new SQLiteCommand(SelectIncludedPaths, conn))
            using (SQLiteDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection))
            {
                var result = new List<IncludedFileSystemPathMetadata>();

                while (reader.Read())
                {
                    result.Add(new IncludedFileSystemPathMetadata(reader.GetString(0))
                    {
                        PluginName = reader.GetValue(1).IfNull<string>(null)
                    });
                }

                return result;
            }
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteMetadataProvider"/> class.
        /// </summary>
        /// <param name="metadataFile">The metadata file.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="metadataFile"/> parameter is <b>null</b>.</exception>
        public SqliteMetadataProvider(FileInfo metadataFile) => MetadataFile = metadataFile ?? throw new ArgumentNullException(nameof(metadataFile));
        #endregion
    }
}
