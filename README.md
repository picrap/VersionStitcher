# VersionStitcher
Injects repository (git only for now) information (such as changeset, branch...) in assembly after it is compiled.  
NuGet... Coming soon.  
Build status... Coming soon too (regarding the bill, you don't have the right to complain).  

## How it works

Add special tags in the application strings, such as:
```csharp
Console.WriteLine("Hi, I'm on {Version.CommitShortID}");
```
After build it will output something like:
`Hi, I'm on 1fab21c`

## Recognized commands

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
