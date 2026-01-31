using Humanizer;
using System;
using System.Globalization;

namespace BlockSense.Desktop.Utilities.Formatting
{
    public static class DateTimeFormatter
    {
        public static string ToOrdinalDate(DateTime date)
            => $"{date.ToString("MMM", CultureInfo.InvariantCulture)} {date.Day.Ordinalize()}, {date.ToString("yyyy", CultureInfo.InvariantCulture)}";
    }
}
