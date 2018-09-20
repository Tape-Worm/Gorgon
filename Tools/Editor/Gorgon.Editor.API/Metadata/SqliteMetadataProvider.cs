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
        private const string DeleteIncludedPaths = "DELETE FROM IncludedPathMetadata";
        // Query to add an included path to the metadata.
        private const string InsertIncludedPath = "INSERT INTO IncludedPathMetadata (FullPath) VALUES (@PPath)";
        // Query to retrieve all included paths from the metadata.
        private const string SelectIncludedPaths = "SELECT FullPath FROM IncludedPathMetadata";
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
        /// <param name="exclusions">The paths to update.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="exclusions" /> parameter is <b>null</b>.</exception>
        public void UpdateIncludedPaths(IEnumerable<IncludedFileSystemPathMetadata> exclusions)
        {
            if (exclusions == null)
            {
                throw new ArgumentNullException(nameof(exclusions));
            }

            SQLiteConnection conn = GetConnection();
            SQLiteCommand command = null;
            SQLiteTransaction trans = null;

            try
            {
                trans = conn.BeginTransaction();

                command = new SQLiteCommand(DeleteIncludedPaths, conn, trans);
                command.ExecuteNonQuery();

                SQLiteParameter param = command.Parameters.Add("@PPath", DbType.String);
                command.CommandText = InsertIncludedPath;

                foreach (IncludedFileSystemPathMetadata exclusion in exclusions)
                {
                    param.Value = exclusion.Path;
                    command.ExecuteNonQuery();
                }

                trans.Commit();
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
                    result.Add(new IncludedFileSystemPathMetadata(reader.GetString(0)));
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
