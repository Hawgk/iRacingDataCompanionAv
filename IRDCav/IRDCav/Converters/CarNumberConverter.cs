namespace IRDCav.Converters
{
    public class CarNumberConverter : OneWayConverter
    {
        protected override object Convert(object? value, string? parameter)
        {
            if (value is not string carNumber)
            {
                return UnsupportedValue(value, parameter);
            }
            string text = carNumber == string.Empty ? string.Empty : "#" + carNumber;
            return text.PadRight(4);
        }
    }
}
