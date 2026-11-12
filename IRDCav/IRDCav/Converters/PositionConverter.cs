namespace IRDCav.Converters
{
    public class PositionConverter : OneWayConverter
    {
        protected override object Convert(object? value, string? parameter)
        {
            if (value is not int position)
            {
                return UnsupportedValue(value, parameter);
            }
            string text = position > 0 ? position.ToString("0") : string.Empty;
            return text.PadLeft(2);
        }
    }
}
