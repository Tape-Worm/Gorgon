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
// Created: August 27, 2018 9:13:29 PM
// 
#endregion

using System;
using System.IO;
using Gorgon.Diagnostics;
using Gorgon.Editor.ProjectData;
using Gorgon.Editor.Properties;

namespace Gorgon.Editor.Services
{
    /// <summary>
    /// A locator used to find the most suitable workspace.
    /// </summary>
    internal class WorkspaceTester 
        : IWorkspaceTester
    {
        #region Variables.
        // The project manager for the application.
        private readonly IProjectManager _projectManager;
        #endregion

        #region Properties.

        #endregion

        #region Methods.
        /// <summary>
        /// Function to test a location for accessibility.
        /// </summary>
        /// <param name="path">The path to the workspace.</param>
        /// <returns>A tuple containing a <see cref="bool"/> flag to indicate whether the <b>true</b> if the area is accessible or not, and a string containing the reason why it is not acceptable.</returns>
        /// <remarks>
        /// <para>
        /// A workspace path is only accessible if it can be written to, deleted by the current user and is empty. If either of these conditions are not met, then this method will return <b>false</b>.
        /// </para>
        /// </remarks>
        public (bool isAcceptable, string reason) TestForAccessibility(DirectoryInfo path)
        {
            Stream stream = null;

            if (path == null)
            {
                return (false, Resources.GOREDIT_ERR_WORKSPACE_NO_DIRECTORY);
            }

            try
            {

                Program.Log.Print($"Checking the path '{path.FullName}' for use as a workspace...", LoggingLevel.Simple);
                
                if (!path.Exists)
                {
                    Program.Log.Print("Path does not exist. Testing creation and delete...", LoggingLevel.Verbose);

                    path.Create();

                    Program.Log.Print($"The path '{path.FullName}' will be used as the default workspace path.", LoggingLevel.Verbose);
                    return (true, string.Empty);
                }

                if (string.Equals(path.Root.FullName, path.FullName, StringComparison.OrdinalIgnoreCase))
                {
                    Program.Log.Print("The path is the root of the file system on the drive. For security, this directory is unsuitable as a workspace", LoggingLevel.Verbose);
                    return (false, Resources.GOREDIT_ERR_WORKSPACE_DIRECTORY_IS_ROOT);
                }

                Program.Log.Print("Checking for files and subdirectories...", LoggingLevel.Verbose);

                (bool hasProject, string existingProjectName) = _projectManager.HasProject(path);

                // If we have a project in this directory already, then we are allowed to overwrite it.
                if (hasProject)
                {
                    Program.Log.Print($"A project with the name '{existingProjectName}' already exists in this path. This is OK since we can overwrite the project.",
                                      LoggingLevel.Intermediate);
                    Program.Log.Print($"The path '{path.FullName}' will be used as the default workspace path.", LoggingLevel.Simple);
                }
                else
                {
                    FileSystemInfo[] files = path.GetFileSystemInfos();

                    if (files.Length != 0)
                    {
                        Program.Log.Print($"The path '{path.FullName}' is not suitable as a workspace because it is not empty ({files.Length} items found).",
                                          LoggingLevel.Simple);
                        return (false, Resources.GOREDIT_ERR_WORKSPACE_NOT_EMPTY);
                    }
                }

                Program.Log.Print("Checking for security permission retrieval...", LoggingLevel.Verbose);
                // Attempt to read the security.
                path.GetAccessControl();

                Program.Log.Print("Checking for subdirectory creation...", LoggingLevel.Verbose);
                var subDir = new DirectoryInfo(Path.Combine(path.FullName, "Subdir"));
                subDir.Create();
                subDir.Delete();

                Program.Log.Print("Checking for file creation...", LoggingLevel.Verbose);
                // Try to create a file.
                string fileName = $"TempSecurityTestFile{Guid.NewGuid():N}";

                var testFileForWrite = new FileInfo(Path.Combine(path.FullName, fileName));

                stream = testFileForWrite.Open(FileMode.Create);
                stream.WriteByte(1);
                stream.WriteByte(2);
                stream.WriteByte(3);
                stream.WriteByte(4);
                stream.Dispose();

                testFileForWrite.Delete();

                Program.Log.Print($"The path '{path.FullName}' will be used as the default workspace path.", LoggingLevel.Simple);

                return (true, string.Empty);
            }
            catch (UnauthorizedAccessException)
            {
                Program.Log.Print($"The path '{path.FullName}' is not suitable as a workspace because the user lacks read, write, or delete permissions for the directory.", LoggingLevel.Simple);
                return (false, Resources.GOREDIT_ERR_WORKSPACE_NOT_AUTHORIZED);
            }
            catch (IOException ioEx)
            {
                Program.Log.Print($"The path '{path.FullName}' is not suitable as a workspace because this exception was raised: {ioEx.Message}.", LoggingLevel.Simple);
                return (false, string.Format(Resources.GOREDIT_ERR_WORKSPACE_EXCEPTION, ioEx.Message));
            }
            finally
            {
                stream?.Dispose();
            }
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkspaceTester"/> class.
        /// </summary>
        /// <param name="projectManager">The project manager interface for the application.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="projectManager"/> parameter is <b>null</b>.</exception>
        public WorkspaceTester(IProjectManager projectManager) => _projectManager = projectManager ?? throw new ArgumentNullException(nameof(projectManager));
        #endregion
    }
}
