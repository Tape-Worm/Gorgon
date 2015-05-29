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

[assembly: SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "width+3", Scope = "member", Target = "Gorgon.Graphics.GorgonBufferFormatInfo+FormatData.#GetPitch(System.Int32,System.Int32,Gorgon.Graphics.PitchFlags)")]
[assembly: SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "height+3", Scope = "member", Target = "Gorgon.Graphics.GorgonBufferFormatInfo+FormatData.#GetPitch(System.Int32,System.Int32,Gorgon.Graphics.PitchFlags)")]
[assembly: SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "width+1", Scope = "member", Target = "Gorgon.Graphics.GorgonBufferFormatInfo+FormatData.#GetPitch(System.Int32,System.Int32,Gorgon.Graphics.PitchFlags)")]
[assembly: SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "height+3", Scope = "member", Target = "Gorgon.Graphics.GorgonBufferFormatInfo+FormatData.#Scanlines(System.Int32)")]
[assembly: SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Scope = "member", Target = "Gorgon.IO.GorgonCodecGIF.#FrameDelays")]
[assembly: SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Scope = "type", Target = "Gorgon.Graphics.GorgonFonts")]
[assembly: SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Gorgon.Native.Win32API.DwmEnableComposition(System.Int32)", Scope = "member", Target = "Gorgon.Graphics.GorgonGraphics.#IsDWMCompositionEnabled")]
[assembly: SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Gorgon.Native.Win32API.DwmIsCompositionEnabled(System.Boolean@)", Scope = "member", Target = "Gorgon.Graphics.GorgonGraphics.#.cctor()")]
[assembly: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "Gorgon.Graphics.GorgonOutputBufferBinding.#OutputBuffer")]
[assembly: SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Scope = "member", Target = "Gorgon.Graphics.GorgonVertexBufferBinding.#VertexBuffer")]
[assembly: SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Gorgon.Native.Win32API.DeleteObject(System.IntPtr)", Scope = "member", Target = "Gorgon.Native.Win32API.#RestoreActiveObject()")]
