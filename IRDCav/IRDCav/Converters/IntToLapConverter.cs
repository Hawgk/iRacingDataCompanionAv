namespace IRDCav.Converters
{
    public class IntToLapConverter : OneWayConverter
    {
        protected override object Convert(object? value, string? parameter)
        {
            if (value is not int laps)
            {
                return UnsupportedValue(value, parameter);
            }
            string text = laps > 0 ? laps + "L" : string.Empty;
            return text.PadLeft(4);
        }
    }
}
