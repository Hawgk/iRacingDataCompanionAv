namespace IRDCav.Converters
{
    /// <summary>Shows a whole number with its sign, e.g. "+12", "-8" or "0".</summary>
    public class SignedNumberConverter : OneWayConverter
    {
        protected override object Convert(object? value, string? parameter)
        {
            if (value is not int number)
            {
                return UnsupportedValue(value, parameter);
            }
            return number > 0 ? "+" + number : number.ToString();
        }
    }
}
