using System;
using System.Collections.Generic;

namespace AteChips.Host.Audio;
public static class StringExtensions
{
    public static bool IsOneOf(this string? source, IEnumerable<string> options, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
    {
        if (source == null)
            return false;

        foreach (var option in options)
        {
            if (string.Equals(source, option, comparison))
                return true;
        }

        return false;
    }
}