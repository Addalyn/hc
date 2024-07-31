using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;

[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[module: UnverifiableCode]

[assembly: AssemblyVersion (
    ThisAssembly.Git.BaseVersion.Major + "." +
    ThisAssembly.Git.BaseVersion.Minor + "." +
    ThisAssembly.Git.BaseVersion.Patch + "." +
    ThisAssembly.Git.Commits
    )]

[assembly: AssemblyFileVersion (
    ThisAssembly.Git.BaseVersion.Major + "." +
    ThisAssembly.Git.BaseVersion.Minor + "." +
    ThisAssembly.Git.BaseVersion.Patch + "." +
    ThisAssembly.Git.Commits
    )]

[assembly: AssemblyInformationalVersion (
    ThisAssembly.Git.BaseVersion.Major + "." + 
    ThisAssembly.Git.BaseVersion.Minor + 
    (ThisAssembly.Git.BaseVersion.Patch != "0" ? "." + ThisAssembly.Git.BaseVersion.Patch : "") +
    (ThisAssembly.Git.SemVer.DashLabel != "-client" ? ThisAssembly.Git.SemVer.DashLabel : "")
    )]

[assembly: AssemblyProduct (
#if SERVER
    "Atlas Reactor Custom Server"
#else
    "Atlas Reactor Custom Client"
#endif
    )]