using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
#if DEBUG
[assembly: AssemblyTitle("Gorgon File System [DEBUG VERSION]")]
[assembly: AssemblyDescription("A unified virtual file system for the Gorgon library.  [DEBUG VERSION]")]
[assembly: AssemblyProduct("Gorgon.FileSystem [DEBUG VERSION]")]
[assembly: AssemblyConfiguration("DEBUG")]
#else
[assembly: AssemblyTitle("Gorgon File System")]
[assembly: AssemblyDescription("A unified virtual file system for the Gorgon library.")]
[assembly: AssemblyProduct("Gorgon.FileSystem")]
[assembly: AssemblyConfiguration("RELEASE")]
#endif
[assembly: AssemblyCompany("Michael Winsor")]
[assembly: AssemblyCopyright("Copyright © Michael Winsor 2011")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("7deafaa8-26a3-47f8-9687-42e4b16710b7")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("2.0.*")]
[assembly: AssemblyFileVersion("2.0.0.0")]
