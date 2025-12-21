using System.Globalization;

namespace ShiftTrack.Api.Helpers
{
    public static class TimeParser
    {
        // "HH:mm" -> TimeSpan
        public static bool TryParseHm(string value, out TimeSpan time)
        {
            time = default;

            if (string.IsNullOrWhiteSpace(value))
                return false;

            // HH:mm
            if (TimeSpan.TryParseExact(value.Trim(), @"hh\:mm", CultureInfo.InvariantCulture, out var ts))
            {
                time = ts;
                return true;
            }

            return false;
        }

        public static string ToHm(TimeSpan ts) => ts.ToString(@"hh\:mm");
    }
}
