namespace IRDCav.Converters
{
    /// <summary>Shows positions, lap counts and similar numbers; zero and below are blank.</summary>
    public class PositiveNumberConverter : OneWayConverter
    {
        protected override object Convert(object? value, string? parameter)
        {
            if (value is not int number)
            {
                return UnsupportedValue(value, parameter);
            }
            return number > 0 ? number.ToString() : string.Empty;
        }
    }
}
