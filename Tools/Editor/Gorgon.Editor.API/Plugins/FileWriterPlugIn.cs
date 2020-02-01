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
// Created: October 12, 2018 1:08:11 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Gorgon.Collections;
using Gorgon.Core;
using Gorgon.Editor.Properties;
using Gorgon.IO;

namespace Gorgon.Editor.PlugIns
{
    /// <summary>
    /// Flags to indicate the capabilities of the writer.
    /// </summary>
    [Flags]
    public enum WriterCapabilities
    {
        /// <summary>
        /// No special capabilities are available.
        /// </summary>
        None = 0,
        /// <summary>
        /// Provider supports compression.
        /// </summary>
        Compression = 1,
        /// <summary>
        /// Provider supports encryption.
        /// </summary>
        Encryption = 2
    }

    /// <summary>
    /// An interface for file output plug ins.
    /// </summary>
    public abstract class FileWriterPlugIn
        : EditorPlugIn
    {
        #region Variables.
        // Default compression amount.
        private float _compressAmount = 0.5f;        
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the equivalent type name for v2 of the Gorgon file writer plugin.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is here to facilitate importing of file metadata from v2 of the gorgon editor files. Only specify a compatible type here, otherwise things will go wrong.
        /// </para>
        /// </remarks>
        public virtual string V2PlugInName => string.Empty;

        /// <summary>
        /// Property to return the capabilities of the writer.
        /// </summary>
        public abstract WriterCapabilities Capabilities
        {
            get;
        }

        /// <summary>
        /// Property to set or return the percentage for compression (if supported).
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value is within a range of 0..1 where 0 means no compression, and 1 means use the maximum compression available by the compression provider.
        /// </para>
        /// <para>
        /// Higher compression values will mean that it takes longer to write the file.
        /// </para>
        /// <para>
        /// The default value is 0.5f.
        /// </para>
        /// </remarks>
        public virtual float Compression
        {
            get => _compressAmount;
            set
            {
                if (value < 0)
                {
                    value = 0;
                }
                if (value > 1.0f)
                {
                    value = 1.0f;
                }

                _compressAmount = value;
            }
        }

        /// <summary>
        /// Property to return the file extensions (and descriptions) for this content type.
        /// </summary>
        /// <remarks>
        /// Plug in developers must provide common file extensions supported by the plug in type, or else this plug in cannot be used.
        /// </remarks>
        public IGorgonNamedObjectReadOnlyDictionary<GorgonFileExtension> FileExtensions
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the type of plug in.
        /// </summary>
        public sealed override PlugInType PlugInType => PlugInType.Writer;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to write the file to the specified path.
        /// </summary>
        /// <param name="filePath">The path to the file to save the data into.</param>
        /// <param name="workspace">The directory that represents the workspace for our file system data.</param> 
        /// <param name="progressCallback">The method used to report progress back to the application.</param>
        /// <param name="cancelToken">The token used for cancelling the operation.</param>
        /// <returns>A task for asynchronous operation.</returns>
        /// <remarks>
        /// <para>
        /// The <paramref name="progressCallback"/> is a method that takes 3 parameters:
        /// <list type="number">
        /// <item>
        ///     <description>The current item number being written.</description>
        /// </item>
        /// <item>
        ///     <description>The total number of items to write.</description>
        /// </item>
        /// <item>
        ///     <description><b>true</b> if the operation can be cancelled, or <b>false</b> if not.</description>
        /// </item>
        /// </list>
        /// This progress method is optional, and if <b>null</b> is passed, then no progress is reported.
        /// </para>
        /// </remarks>
        protected abstract Task OnWriteAsync(string filePath, DirectoryInfo workspace, Action<int, int, bool> progressCallback, CancellationToken cancelToken);

        /// <summary>
        /// Function to determine if the type of file specified can be written by this plug in.
        /// </summary>
        /// <param name="filePath">The path to the file to evaluate.</param>
        /// <returns><b>true</b> if the writer can write the type of file, or <b>false</b> if it cannot.</returns>
        /// <remarks>
        /// <para>
        /// The <paramref name="filePath"/> parameter is never <b>null</b>, and is guaranteed to exist when this method is called. If neither of these are true, then this method is not called.
        /// </para>
        /// </remarks>
        protected abstract bool OnEvaluateCanWriteFile(string filePath);

        /// <summary>
        /// Function to determine if this writer can write the type of file specified in the path.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns><b>true</b> if the writer can write the type of file, or <b>false</b> if it cannot.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="path"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="path"/> parameter is empty.</exception>
        public bool CanWriteFile(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentEmptyException(nameof(path));
            }

            FileAttributes attribs = File.GetAttributes(path);

            return (!File.Exists(path)) || (((attribs & FileAttributes.Directory) != FileAttributes.Directory) && (OnEvaluateCanWriteFile(path)));
        }

        /// <summary>
        /// Function to write the application data into a stream.
        /// </summary>
        /// <param name="filePath">The path to the file to save the data into.</param>
        /// <param name="workspace">The directory that represents the workspace for our file system data.</param>
        /// <param name="progressCallback">The method used to report progress back to the application.</param>
        /// <param name="cancelToken">The token used for cancelling the operation.</param>
        /// <returns>A task for asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="filePath"/>, or the <paramref name="workspace"/> parameter is empty.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown if the <paramref name="workspace"/> directory does not exist.</exception>
        /// <exception cref="IOException">Thrown if the stream is read only.</exception>
        /// <remarks>
        /// <para>
        /// The <paramref name="progressCallback"/> is a method that takes 3 parameters:
        /// <list type="number">
        /// <item>
        ///     <description>The current item number being written.</description>
        /// </item>
        /// <item>
        ///     <description>The total number of items to write.</description>
        /// </item>
        /// <item>
        ///     <description><b>true</b> if the operation can be cancelled, or <b>false</b> if not.</description>
        /// </item>
        /// </list>
        /// This progress method is optional, and if <b>null</b> is passed, then no progress is reported.
        /// </para>
        /// </remarks>
        public Task WriteAsync(string filePath, DirectoryInfo workspace, Action<int, int, bool> progressCallback, CancellationToken cancelToken)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            if (!workspace.Exists)
            {
                throw new DirectoryNotFoundException(string.Format(Resources.GOREDIT_ERR_DIR_NOT_FOUND, workspace.FullName));
            }

            return OnWriteAsync(filePath, workspace, progressCallback, cancelToken);
        }

        /// <summary>
        /// Function to initialize the plug in.
        /// </summary>
        /// <param name="hostServices">The services to pass from the host application to the plug in.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="hostServices"/> parameter is <b>null</b>.</exception>
        public void Initialize(IHostServices hostServices) => HostServices = hostServices ?? throw new ArgumentNullException(nameof(hostServices));
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="FileWriterPlugIn"/> class.
        /// </summary>
        /// <param name="description">Friendly description of the plug in.</param>
        /// <param name="fileExtensions">The file of common file name extensions supported by this writer.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="fileExtensions"/> parameter is <b>null</b>.</exception>
        protected FileWriterPlugIn(string description, IEnumerable<GorgonFileExtension> fileExtensions)
            : base(description)
        {
            if (fileExtensions == null)
            {
                throw new ArgumentNullException(nameof(fileExtensions));
            }

            var extensions = new GorgonFileExtensionCollection();

            foreach (GorgonFileExtension extension in fileExtensions)
            {
                extensions[extension.Extension] = extension;
            }

            FileExtensions = extensions;
        }
        #endregion
    }
}