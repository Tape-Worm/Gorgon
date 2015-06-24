// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.
//
// To add a suppression to this file, right-click the message in the 
// Code Analysis results, point to "Suppress Message", and click 
// "In Suppression File".
// You do not need to add suppressions to this file manually.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Scope = "member", Target = "Gorgon.IO.GorgonChunkedFormat.#TempBuffer")]
[assembly: SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "397*previousHash", Scope = "member", Target = "Gorgon.Core.GorgonHashGenerationExtension.#GenerateHash`1(System.Int32,!!0)")]
[assembly: SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Gorgon.Native.Win32API.GetWindowThreadProcessId(System.IntPtr,System.UInt32@)", Scope = "member", Target = "Gorgon.Diagnostics.GorgonProcess.#GetActiveProcess()")]
[assembly: SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "startPointIndex+1", Scope = "member", Target = "Gorgon.Math.GorgonSpline.#GetInterpolatedValue(System.Int32,System.Single)")]
[assembly: SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Gorgon.Native.Win32API.GetWindowThreadProcessId(System.IntPtr,System.UInt32@)", Scope = "member", Target = "Gorgon.Diagnostics.GorgonProcess.#GetProcessByWindow(System.IntPtr)")]
[assembly: SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.Control.set_Text(System.String)", Scope = "member", Target = "Gorgon.UI.FlatForm.#InitializeComponent()")]
[assembly: SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.Control.set_Text(System.String)", Scope = "member", Target = "Gorgon.UI.ErrorDialog.#InitializeComponent()")]
[assembly: SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.Control.set_Text(System.String)", Scope = "member", Target = "Gorgon.UI.WarningDialog.#InitializeComponent()")]
[assembly: SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "397*previousHash", Scope = "member", Target = "Gorgon.Core.Extensions.GorgonHashGenerationExtension.#GenerateHash`1(System.Int32,!!0)")]
[assembly: SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Scope = "type", Target = "Gorgon.Diagnostics.GorgonLogFile")]
[assembly: SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Scope = "type", Target = "Gorgon.Plugins.GorgonPlugInFactory")]
[assembly: SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Scope = "type", Target = "Gorgon.IO.GorgonChunkReader")]
[assembly: SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Scope = "type", Target = "Gorgon.IO.GorgonChunkWriter")]
[assembly: SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Scope = "type", Target = "Gorgon.IO.GorgonChunkFileWriter")]
[assembly: SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Scope = "type", Target = "Gorgon.IO.GorgonChunkFileReader")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "startPointIndex+1", Scope = "member", Target = "Gorgon.Math.GorgonCatmullRomSpline.#GetInterpolatedValue(System.Int32,System.Single)")]
