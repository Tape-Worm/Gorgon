#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Tuesday, June 05, 2012 1:20:34 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Aga.Controls.Tree;
using GorgonLibrary.FileSystem;

namespace GorgonLibrary.GorgonEditor
{
	/// <summary>
	/// Project object.
	/// </summary>
	class Project
	{
		#region Variables.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the root node for the tree.
		/// </summary>
		public Node RootNode
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the type registry for the documents.
		/// </summary>
		public IDictionary<string, Type> DocumentTypes
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return a description for a specific document type.
		/// </summary>
		public IDictionary<string, Type> DocumentDescriptions
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the folders for this project.
		/// </summary>
		public ProjectFolderCollection Folders
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the documents at the root of this project.
		/// </summary>
		public DocumentCollection Documents
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the name of the project.
		/// </summary>
		public string Name
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the path to the file holding the project.
		/// </summary>
		public string ProjectPath
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the file system for the project.
		/// </summary>
		public GorgonFileSystem FileSystem
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to build the type registry.
		/// </summary>
		private void BuildTypeRegistry()
		{
			DocumentTypes = new Dictionary<string, Type>();
			DocumentDescriptions = new Dictionary<string, Type>();

			var docTypes = (from type in GetType().Assembly.GetTypes()
							let extAttrib = type.GetCustomAttributes(typeof(DocumentExtensionAttribute), false) as IList<DocumentExtensionAttribute>
							where (type.IsSubclassOf(typeof(Document))) && (!type.IsAbstract) && (extAttrib != null) && (extAttrib.Count > 0)
							select new {type, extAttrib});

			// Only add types once.
			foreach (var docType in docTypes)
			{	
				foreach (var attrib in docType.extAttrib.Where(item => item.CanOpen))
				{
					if (!DocumentDescriptions.ContainsKey(attrib.DocumentDescription))
						DocumentDescriptions.Add(attrib.DocumentDescription, docType.type);

					foreach (var ext in attrib.Extensions)
					{
						if (!DocumentTypes.ContainsKey(ext))
							DocumentTypes.Add(ext, docType.type);						
					}
				}
			}
		}

		/// <summary>
		/// Function to build the folder layout.
		/// </summary>
		/// <param name="directories">The directories to enumerate.</param>
		/// <param name="owner">Owner of the current set of directories.</param>
		private void CreateFolders(IEnumerable<GorgonFileSystemDirectory> directories, ProjectFolder owner)
		{
			foreach (var directory in directories)
			{
				ProjectFolder folder = new ProjectFolder(this, owner, directory.Name);

				if (directory.Directories.Count > 0)
					CreateFolders(directory.Directories.OrderBy(item => item.Name), folder);

				if (owner == null)
					Folders.Add(folder);
				else
					owner.Folders.Add(folder);

				CreateDocuments(directory, folder);
			}
		}

		/// <summary>
		/// Function to create the documents for each file.
		/// </summary>
		/// <param name="directory">Directory to enumerate.</param>
		/// <param name="owner">Folder that owns the document.</param>
		private void CreateDocuments(GorgonFileSystemDirectory directory, ProjectFolder owner)
		{
			foreach (var file in directory.Files.OrderBy((GorgonFileSystemFileEntry item) => item.Name))
			{
				Document document = null;
				string fileExtension = file.Extension.ToLower();

				if (fileExtension.StartsWith("."))
					fileExtension = fileExtension.Substring(1);
				
				// Find the type.
				var docType = (from documentType in DocumentTypes
							   where string.Compare(documentType.Key, fileExtension, true) == 0
							   select documentType.Value).FirstOrDefault();

				if (docType != null)
					document = (Document)(Activator.CreateInstance(docType, new object[] {file.Name, true, owner }));
				else
					document = new DocumentUnknown(file.Name, true, owner);

				if (owner == null)
					Documents.Add(document);
				else
					owner.Documents.Add(document);
			}
		}

		/// <summary>
		/// Function to destroy the data for the project.
		/// </summary>
		/// <param name="folder">Folder to start searching at.</param>
		private void DestroyData(ProjectFolder folder)
		{
			DocumentCollection documents = Documents;
			ProjectFolderCollection folders = Folders;

			if ((Documents == null) || (Folders == null))
				return;

			if (folder != null)
			{
				documents = folder.Documents;
				folders = folder.Folders;
			}

			foreach (var subFolder in folders)
				DestroyData(subFolder);

			foreach (var doc in documents)
				doc.Dispose();

			folders.Clear();
			documents.Clear();
		}

		/// <summary>
		/// Function to create the specified folder path.
		/// </summary>
		/// <param name="path">Path to the folder.</param>
		/// <returns>The newly created folder, or the folder if it already exists.</returns>
		public ProjectFolder CreateFolder(string path)
		{
			ProjectFolder folder = null;
			ProjectFolderCollection folders = null;

			path = path.FormatDirectory('/');

			if ((Path.GetInvalidPathChars().Any(item => path.Contains(item))) || (Path.GetInvalidFileNameChars().Any(item => path.Contains(item) && (item != '/'))))
				throw new ArgumentException("Cannot create the path, it contains invalid characters.", "path");			

			if (path == "/")
				return null;

			folder = GetFolder(null, path);

			if (folder != null)
				return folder;

			folders = Folders;
			var parts = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

			foreach (var part in parts)
			{
				if (folders.Contains(part))
				{
					folder = Folders[part];
					folders = folder.Folders;
				}
				else
				{
					folder = new ProjectFolder(this, folder, part);
					folders.Add(folder);
					folders = folder.Folders;
				}
			}

			return folder;
		}

		/// <summary>
		/// Function to retrieve all the folders in the specified folder and sub folders.
		/// </summary>
		/// <param name="folder">Folder to start searching with.</param>
		/// <returns>The folders and the sub folders.</returns>
		public IEnumerable<ProjectFolder> GetFolders(ProjectFolder folder)
		{
			List<ProjectFolder> result = new List<ProjectFolder>();
			ProjectFolderCollection folders = null;

			if (folder == null)
			{
				folders = Folders;
				result.AddRange(Folders);
			}
			else
			{
				folders = folder.Folders;
				result.AddRange(folder.Folders);
			}

			foreach (var subFolder in folders)
				result.AddRange(GetFolders(subFolder));

			return result;
		}

		/// <summary>
		/// Function to find a folder.
		/// </summary>
		/// <param name="folder">Folder to start searching from.</param>
		/// <param name="path">Path to the folder.</param>
		/// <returns></returns>
		public ProjectFolder GetFolder(ProjectFolder rootFolder, string path)
		{
			var folders = GetFolders(rootFolder);

			if (!path.StartsWith("/"))
				path = "/" + path;

			if (!path.EndsWith("/"))
				path += "/";

			return (from folder in folders
				   where string.Compare(folder.Path, path, true) == 0
				   select folder).SingleOrDefault();
		}

		/// <summary>
		/// Function to retrieve all the documents in the specified folder and sub folders.
		/// </summary>
		/// <param name="folder">Folder to start searching with.</param>
		/// <returns>The documents in the folder and the sub-folders.</returns>
		public IEnumerable<Document> GetDocuments(ProjectFolder folder)
		{
			List<Document> result = new List<Document>();
			ProjectFolderCollection folders = null;

			if (folder == null)
			{
				folders = Folders;
				result.AddRange(Documents);
			}
			else
			{
				folders = folder.Folders;
				result.AddRange(folder.Documents);
			}

			foreach (var subFolder in folders)
				result.AddRange(GetDocuments(subFolder));

			return result;
		}

		/// <summary>
		/// Function to create a new document.
		/// </summary>
		/// <param name="name">Name of the document.</param>
		/// <param name="folder">Folder in which to store the document.</param>
		/// <param name="allowClose">TRUE to allow the document to close, FALSE to keep it open.</param>
		/// <returns>The new document.</returns>
		public T CreateDocument<T>(string name, ProjectFolder folder, bool allowClose)
			where T : Document
		{
			Type type = typeof(T);

			type = (from docType in Program.Project.DocumentTypes
					where docType.Value == type
					select docType.Value).SingleOrDefault();

			if (type == null)
				throw new ArgumentException("The type '" + type.FullName + "' is not supported.", "type");

			T result = (T)Activator.CreateInstance(type, new object[] { name, allowClose, folder });

			if (folder == null)
				Documents.Add(result);
			else
				folder.Documents.Add(result);

			return result;
		}

		/// <summary>
		/// Function to return whether the project needs to be saved or not.
		/// </summary>
		/// <returns>TRUE if the project needs to be saved, FALSE if not.</returns>
		public bool GetProjectState()
		{
			return GetDocuments(null).Count(item => item.NeedsSave) > 0;			
		}

		/// <summary>
		/// Function to import documents into the project.
		/// </summary>
		/// <param name="paths">Paths to the files to import.</param>
		/// <param name="folder">Folder to contain the documents.</param>
		public IList<Tuple<Document, string>> ImportDocuments(IEnumerable<string> paths, ProjectFolder folder)
		{
			List<Tuple<Document, string>> result = new List<Tuple<Document, string>>();

			foreach (var file in paths)
			{
				Document document = null;
				string extension = Path.GetExtension(file);
				string fileName = Path.GetFileName(file);

				if ((extension.Length > 0) && (extension.StartsWith(".")))
					extension = extension.Substring(1);

				var type = (from types in DocumentTypes
						   where string.Compare(types.Key, extension, true) == 0
						   select types.Value).FirstOrDefault();
								
				if (type == null)
					document = new DocumentUnknown(fileName, true, folder);
				else
					document = (Document)Activator.CreateInstance(type, new object[] { fileName, true, folder });

				result.Add(new Tuple<Document, string>(document, file));
			}

			return result;
		}

		/// <summary>
		/// Function to load a project.
		/// </summary>
		/// <param name="path">Path to the project.</param>
		public void LoadProject(string path)
		{
			Clear();

			if ((!path.EndsWith(Path.DirectorySeparatorChar.ToString())) && (!path.EndsWith(Path.AltDirectorySeparatorChar.ToString())))
				Name = Path.GetFileNameWithoutExtension(path);
			else
				Name = Path.GetFileNameWithoutExtension(path.Substring(0, path.Length - 1));

			FileSystem.Mount(path);
			ProjectPath = path;
			
			// Retrieve all directories and files.
			CreateFolders(FileSystem.RootDirectory.Directories, null);
			CreateDocuments(FileSystem.RootDirectory, null);

			RootNode.Text = Name;
		}

		/// <summary>
		/// Function to clear the project.
		/// </summary>
		public void Clear()
		{
			DestroyData(null);
			FileSystem = new GorgonFileSystem();
			ProjectPath = string.Empty;
			Name = "Untitled";
			Documents = new DocumentCollection();
			Folders = new ProjectFolderCollection();
			RootNode.Nodes.Clear();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Project"/> class.
		/// </summary>
		public Project()
		{
			RootNode = new Node("Untitled");
			RootNode.Image = Properties.Resources.project_node_16x16;
			RootNode.Tag = this;

			Clear();
			BuildTypeRegistry();
		}
		#endregion
	}
}
