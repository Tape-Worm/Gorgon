
// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA2213:'Form' contains field '_gallery' that is of IDisposable type 'ImageGallery', but it is never disposed. Change the Dispose method on 'Form' to call Close or Dispose on this field.", Justification = "<Pending>", Scope = "member", Target = "~F:Gorgon.Examples.Form._gallery")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA2007:Do not directly await a Task without calling ConfigureAwait", Justification = "<Pending>", Scope = "member", Target = "~M:Gorgon.Examples.Form.OnFormClosing(System.Windows.Forms.FormClosingEventArgs)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1031:Modify 'OnLoad' to catch a more specific exception type, or rethrow the exception.", Justification = "<Pending>", Scope = "member", Target = "~M:Gorgon.Examples.Form.OnLoad(System.EventArgs)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA2007:Do not directly await a Task without calling ConfigureAwait", Justification = "<Pending>", Scope = "member", Target = "~M:Gorgon.Examples.GifAnimator.CancelAsync~System.Threading.Tasks.Task")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1031:Modify 'Main' to catch a more specific exception type, or rethrow the exception.", Justification = "<Pending>", Scope = "member", Target = "~M:Gorgon.Examples.Program.Main")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1001:Type 'GifAnimator' owns disposable field(s) '_animationTask, _cancel' but is not disposable", Justification = "<Pending>", Scope = "type", Target = "~T:Gorgon.Examples.GifAnimator")]