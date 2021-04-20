rem %1 ConfigurationName
rem %2 ProjectPath

rem dotnet publish -p:IsTool=true --no-build --framework net5.0 --runtime win-x86  -c $(ConfigurationName) $(ProjectPath)
dotnet pack -p:IsTool=true --no-build -c %1 %2
