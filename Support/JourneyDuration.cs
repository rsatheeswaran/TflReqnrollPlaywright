using System.Globalization;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace TflReqnrollPlaywright.Support;

internal static class JourneyDuration
{
    public static int ParseMinutes(string text)
    {
        var match = Regex.Match(text, @"(?:Total time:\s*)?(\d+)\s*mins?\b", RegexOptions.IgnoreCase);
        Assert.That(match.Success, Is.True, $"Expected a duration in minutes, but found '{text}'.");
        var minutes = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
        Assert.That(minutes, Is.GreaterThan(0), "Journey durations must be positive.");
        return minutes;
    }
}
