#region MIT.
// 
// Gorgon.
// Copyright (C) 2014 Michael Winsor
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
// Created: Thursday, April 17, 2014 9:01:32 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Gorgon.IO.Properties;

namespace Gorgon.IO
{
    /// <summary>
    /// Loads files from a Gorgon file system created by the Gorgon Editor.
    /// </summary>
    /// <remarks>This interface is meant for end-user applications to help with loading objects from a gorgon editor file.
    /// <para>Since the gorgon editor file is treated as a <see cref="Gorgon.IO.GorgonFileSystem">file system</see>, the interface requires a file system when working with Gorgon editor files.</para>
    /// <para>This interface will load in any type of file from the file system as the correct type provided it knows how to load the file. This means that developer defined file formats and 
    /// objects can be loaded by adding file handlers to the interface. These file handlers should be capable of loading any files that is required by the object being read from the file system.</para>
    /// <para>Gorgon will provide file readers for Textures, sprites, animations and fonts by default, but any of these may be overridden by the user if required.</para>
    /// </remarks>
    public class GorgonEditorFile
    {
        #region Constants.
        private const string MetaDataFile = ".gorgon.editor.metadata";				// Metadata file name.
        private const string MetaDataRootName = "Gorgon.Editor.MetaData";           // Name of the root node in the meta data.
        private const string ContentDependencyFiles = "ContentFileDependencies";    // Dependency files.
        private const string FileNode = "File";										// Name of the file node.
        private const string NameAttr = "Name";										// Name attribute name.
        #endregion

        #region Variables.
        // Handlers to read our files with.
        private Dictionary<Type, Func<Stream, IReadOnlyDictionary<GorgonEditorDependency, object>, object>> _fileHandlers;
        // The XML document containing the meta data.
        private XDocument _metaDataFile;
        // Flag to indicate whether meta data is available for this file system.
        private bool? _hasMetaData;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the file system containing the files to load.
        /// </summary>
        public GorgonFileSystem FileSystem
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return a list of dependencies for the files in the file system.
        /// </summary>
        public IReadOnlyDictionary<string, GorgonEditorDependencyCollection> Dependencies
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the registered types that can be loaded.
        /// </summary>
        public IDictionary<Type, Func<Stream, IReadOnlyDictionary<GorgonEditorDependency, object>, object>> RegisteredEditorTypes
        {
            get
            {
                return _fileHandlers;
            }
        }
        #endregion

        #region Methods.

        /// <summary>
        /// Function to retrieve the dependencies file and process it.
        /// </summary>
        private void GetDependencies()
        {
            if ((_hasMetaData != null)
                && (!_hasMetaData.Value))
            {
                return;
            }

            // If we've already loaded the file system meta data for this file system, then leave.
            if (_metaDataFile != null)
            {
                return;
            }

            GorgonFileSystemFileEntry file = FileSystem.GetFile(MetaDataFile);

            if (file == null)
            {
                _hasMetaData = false;
                return;
            }

            // Create and load the object.
            using(Stream metaFile = file.OpenStream(false))
            {
                _metaDataFile = XDocument.Load(metaFile);
            }

            XElement rootNode = _metaDataFile.Element(MetaDataRootName);

            if (rootNode == null)
            {
                _hasMetaData = false;
                return;
            }

            XElement dependencyNode = rootNode.Element(ContentDependencyFiles);

            if (dependencyNode == null)
            {
                _hasMetaData = false;
                return;
            }

            IEnumerable<XElement> fileNodes = dependencyNode.Elements(FileNode);
            var dependencies = new Dictionary<string, GorgonEditorDependencyCollection>();

            // Lock here because we're updating a collection that's not thread safe.
            foreach (XElement fileNode in fileNodes)
            {
                XAttribute path = fileNode.Attribute(NameAttr);

                if ((path == null)
                    || (string.IsNullOrWhiteSpace(path.Value)))
                {
                    continue;
                }

                dependencies.Add(path.Value,
                                    GorgonEditorDependencyCollection
                                        .Deserialize(fileNode.Elements(GorgonEditorDependency.DependencyNode)));
            }

            Dependencies = dependencies;

            _hasMetaData = Dependencies.Count > 0;
        }

        /// <summary>
        /// Function to load a single dependency for a file.
        /// </summary>
        /// <param name="dependency">Dependency to load.</param>
        /// <param name="childDependencies">The dependencies for this dependency.</param>
        /// <returns>The loaded dependency object.</returns>
        private object LoadDependency(GorgonEditorDependency dependency, IReadOnlyDictionary<GorgonEditorDependency, object> childDependencies)
        {
            GorgonFileSystemFileEntry dependencyFile = FileSystem.GetFile(dependency.Path);

            if (dependencyFile == null)
            {
                throw new IOException(string.Format(Resources.GORFS_ERR_DEPENDENCY_NOT_FOUND, dependency.Path));
            }

            // Find a handler for this file.
            KeyValuePair<Type, Func<Stream, IReadOnlyDictionary<GorgonEditorDependency, object>, object>> handler =
                _fileHandlers.FirstOrDefault(item =>
                                             string.Equals(item.Key.FullName,
                                                           dependency.Type,
                                                           StringComparison.OrdinalIgnoreCase));

            if (handler.Key == null)
            {
                throw new IOException(string.Format(Resources.GORFS_ERR_FILE_NO_HANDLER, dependency.Path));
            }

            using (Stream dependencyStream = dependencyFile.OpenStream(false))
            {
                return handler.Value(dependencyStream, childDependencies);
            }
        }

        /// <summary>
        /// Function to load any dependencies required by the file being loaded.
        /// </summary>
        /// <param name="dependencies">The dependencies for the file.</param>
        /// <returns>The a list of loaded dependencies.</returns>
        private IReadOnlyDictionary<GorgonEditorDependency, object> LoadDependencies(IEnumerable<GorgonEditorDependency> dependencies)
        {
            var result = new Dictionary<GorgonEditorDependency, object>();

            foreach (GorgonEditorDependency dependency in dependencies)
            {
                GorgonEditorDependencyCollection childDependencies;
                IReadOnlyDictionary<GorgonEditorDependency, object> dependencyObjects = new Dictionary<GorgonEditorDependency, object>();

                if (Dependencies.TryGetValue(dependency.Path, out childDependencies))
                {
                    dependencyObjects = LoadDependencies(childDependencies);
                }

                result[dependency] = LoadDependency(dependency, dependencyObjects);
            }

            return result;
        }

        /// <summary>
        /// Function to load a file from the editor file. 
        /// </summary>
        /// <typeparam name="T">The type of object to load.  Must be a reference type.</typeparam>
        /// <param name="path">Path to the file to load.</param>
        /// <remarks>
        /// Use this method to load a file from the file system generated by the editor. The type parameter T is used to determine how to load an object through a registered type loader.
        /// <para>The type loader is registered by supplying a type and a function to read in the file. This allows the developer to load in their own file formats if required. New loader types 
        /// can be added through the <see cref="RegisteredEditorTypes"/> parameter.</para>
        /// <para>Without adding any new loader types Gorgon will support loading of its supported image codecs, sprites (for the 2D renderer), animations and fonts.  The loader will attempt to load 
        /// any dependencies required by the requested file as well.</para>
        /// </remarks>
        public T ReadFile<T>(string path)
            where T : class
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException(Resources.GORFS_ERR_PARAMETER_MUST_NOT_BE_EMPTY, "path");
            }

            Type type = typeof(T);
            Func<Stream, IReadOnlyDictionary<GorgonEditorDependency, object>, object> loader;

            if (!_fileHandlers.TryGetValue(type, out loader))
            {
                throw new IOException(string.Format(Resources.GORFS_ERR_FILE_NO_HANDLER, path));
            }

            GorgonFileSystemFileEntry file = FileSystem.GetFile(path);

            if (file == null)
            {
                throw new IOException(string.Format(Resources.GORFS_ERR_FILE_NOT_FOUND, path));
            }

            // Load the meta data if it's not been loaded yet.
            if (_hasMetaData == null)
            {
                GetDependencies();
            }

            GorgonEditorDependencyCollection dependencies;
            IReadOnlyDictionary<GorgonEditorDependency, object> dependencyObjects = new Dictionary<GorgonEditorDependency, object>();

            // Check to see if the file has dependencies.
            if (Dependencies.TryGetValue(file.FullPath, out dependencies))
            {
                dependencyObjects = LoadDependencies(dependencies);
            }

            using(Stream fileStream = file.OpenStream(false))
            {
                return (T)loader(fileStream, dependencyObjects);
            }
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes the <see cref="GorgonEditorFile"/> class.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="fileSystem"/> parameter is NULL (Nothing in VB.Net).</exception>
        public GorgonEditorFile(GorgonFileSystem fileSystem)
        {
            if (fileSystem == null)
            {
                throw new ArgumentNullException("fileSystem");    
            }

            FileSystem = fileSystem;
            Dependencies = new Dictionary<string, GorgonEditorDependencyCollection>();
            _fileHandlers = new Dictionary<Type, Func<Stream, IReadOnlyDictionary<GorgonEditorDependency, object>, object>>();
        }
        #endregion
    }
}
