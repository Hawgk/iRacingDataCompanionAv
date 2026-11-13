using System;

namespace IRDCav.Converters
{
    /// <summary>
    /// Formats a float time difference with sign, e.g. "+1.2" or "-1:02.3". Zero and NaN are blank.
    /// Created by the composition root, because the number of decimals is a user setting.
    /// </summary>
    public sealed class SignedTimeConverter : OneWayConverter
    {
        private const float SecondsPerMinute = 60;

        private readonly int _decimals;
        private readonly string _secondsPattern;
        private readonly string _minutesPattern;

        public SignedTimeConverter(int decimals)
        {
            _decimals = decimals;
            string fraction = decimals > 0 ? @"\." + new string('f', decimals) : string.Empty;
            _secondsPattern = "%s" + fraction;
            _minutesPattern = @"m\:ss" + fraction;
        }

        protected override object Convert(object? value, string? parameter)
        {
            if (value is not float seconds)
            {
                return UnsupportedValue(value, parameter);
            }
            return seconds == 0 || float.IsNaN(seconds) ? string.Empty : FormatSigned(seconds);
        }

        private string FormatSigned(float seconds)
        {
            double magnitude = Math.Round(Math.Abs(seconds), _decimals);
            string pattern = magnitude >= SecondsPerMinute ? _minutesPattern : _secondsPattern;
            return (seconds < 0 ? "-" : "+") + TimeFormat.Format(magnitude, _decimals, pattern);
        }
    }
}
