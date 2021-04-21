using System;
using System.Globalization;

Console.WriteLine("Hi, I'm on {Version.CommitShortID} and {this} is not a {Version}");
Console.ReadKey();

internal static class Assembly
{
    public static Version GetVersion(DateTime buildTime)
    {
        const int major = 9;
        const int minor = 0;

        DateTime versionTime;

        var literalCommitTime = "{Version.CommitTimeIso}";
        const DateTimeStyles style = DateTimeStyles.AssumeLocal | DateTimeStyles.AllowWhiteSpaces;
        if (DateTime.TryParseExact(literalCommitTime, "o", CultureInfo.InvariantCulture, style, out var commitTime))
            versionTime = commitTime;
        else
        {
            versionTime = buildTime;
#if DEBUG
            versionTime = new DateTime(versionTime.Year, versionTime.Month, versionTime.Day, 0, 0, 0, versionTime.Kind);
#endif
        }

        var versionLocalTime = versionTime.ToLocalTime();
        var fullMonth = versionLocalTime.Month + (versionLocalTime.Year - 2009) * 12;
        return new Version(
            major, minor,
            versionLocalTime.Day + fullMonth * 100,
            versionLocalTime.Hour * 100 + versionLocalTime.Minute
        );
    }
}
