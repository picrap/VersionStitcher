# VersionStitcher
Injects repository (git only for now) information (such as changeset, branch...) in assembly after it is compiled.  
Available as a [NuGet package](https://www.nuget.org/packages/VersionStitcher/).  
Current build status: [![Build status](https://ci.appveyor.com/api/projects/status/eblfjohu6qsa6cee/branch/master?svg=true)](https://ci.appveyor.com/project/picrap/versionstitcher/branch/master)  

## String injection

VersionStitcher can replace known patterns in assembly attributes and any literal string in the assembly.

### How it works

Add special tags in the application strings, such as:
```csharp
Console.WriteLine("Hi, I'm on {Version.CommitShortID}");
```
After build it will output something like:
`Hi, I'm on 1fab21ca`

### Recognized commands

Pattern | Value
:------ | :------
`{Version.BuildTime}` | Build time
`{Version.BuildTimeUTC}` | Build time, UTC version
`{Version.BranchName}` | Name of current branch
`{Version.BranchRemoteName}` | Name of upstream branch
`{Version.CommitID}` | Full commit ID (the SHA1 in git)
`{Version.CommitShortID}` | Short commit ID
`{Version.CommitMessage}` | Commit message
`{Version.CommitAuthor}` | Author of last commit
`{Version.IsDirty}` | Literal boolean indicating wheter changes are pending
`{Version.IsDirtyLiteral}` | "" or "dirty", depending on the `IsDirty` value

There is also the special command `{Version.Help}` which displays all available commands and their values.

## Dynamic version injection

### How it works

Assembly version, file version and product version can also be injected by code, this allows a better version than the usual "1.0.*" (Yay!).
To do this, you need to write a class named `Assembly` at root namespace (by convention, we suggest to place it in the `AssemblyInfo.cs` file, to be near the other assembly version).

### Sample code

This class can contain up to three methods:

```csharp
// Once used, this class is stripped out of the assembly
internal static class Assembly
{
    // this method is mandatory and must return a System.Version
    // the buildTime parameter is optional. It's OK if you don't require one.
    // This will update the .NET assembly version ...
    // ... as well as the Win32 resource version (the "Assembly Version" String from StringTables)
    public static Version GetVersion(DateTime buildTime)
    {
        return new Version(10, buildTime.Year, buildTime.Month, buildTime.Day);
    }
    
    // This method is optional. If not specified it uses the value returned by GetVersion() above
    // It can also have a parameter of type DateTime which will receive the build time as argument.
    // It can return a System.Version or System.String (in which the string is parsed to get a version when needed or kept literal)
    // It updates or adds the AssemblyFileVersionAttribute ...
    // ... as well as Win32 resource version (the FILEVERSION from VS_FIXEDFILEINFO, and "FileVersion" String from StringTable)
    public static Version GetFileVersion()
    {
        return new Version(30, 40, 50);
    }

    // This method is optional too. If not specified it uses the value returned by GetFileVersion() or GetVersion() above
    // It can also have a parameter of type DateTime which will receive the build time as argument.
    // It can return a System.Version or System.String (in which the string is parsed to get a version when needed or kept literal)
    // It updates or adds the AssemblyInformationalVersionAttribute ...
    // ... as well as Win32 resource version (the PRODUCTVERSION from VS_FIXEDFILEINFO, and "ProductVersion" String from StringTable)
    public static Version GetProductVersion()
    {
        return new Version(60, 70, 80, 90);
    }
}
```
