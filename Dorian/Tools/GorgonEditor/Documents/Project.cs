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
		/// Property to return the type registry for the documents.
		/// </summary>
		public IDictionary<Type, DocumentExtensionAttribute> DocumentTypes
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
			DocumentTypes = new Dictionary<Type, DocumentExtensionAttribute>();

			var docTypes = (from type in GetType().Assembly.GetTypes()
							let extAttrib = type.GetCustomAttributes(typeof(DocumentExtensionAttribute), false) as IList<DocumentExtensionAttribute>
							where (type.IsSubclassOf(typeof(Document))) && (!type.IsAbstract) && (extAttrib != null) && (extAttrib.Count > 0)
							select extAttrib[0]);

			// Only add types once.
			foreach (var docType in docTypes)
			{
				if (!DocumentTypes.ContainsKey(docType.DocumentType))
					DocumentTypes.Add(docType.DocumentType, docType);
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
							   where documentType.Value.Extensions.Contains(fileExtension)
							   select documentType.Key).FirstOrDefault();

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
		}

		/// <summary>
		/// Function to clear the project.
		/// </summary>
		public void Clear()
		{
			FileSystem = new GorgonFileSystem();
			ProjectPath = string.Empty;
			Name = "Untitled";
			Documents = new DocumentCollection();
			Folders = new ProjectFolderCollection();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Project"/> class.
		/// </summary>
		public Project()
		{
			Clear();
			BuildTypeRegistry();			
		}
		#endregion
	}
}
