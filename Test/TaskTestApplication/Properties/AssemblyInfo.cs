using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;


internal static class Assembly
{
    public static Version GetVersion(DateTime buildTime)
    {
        return new Version(10, buildTime.Year, buildTime.Month, buildTime.Day);
    }
    public static Version GetFileVersion()
    {
        return new Version(30, 40, 50);
    }
    public static Version GetProductVersion()
    {
        return new Version(60, 70, 80, 90);
    }
}
