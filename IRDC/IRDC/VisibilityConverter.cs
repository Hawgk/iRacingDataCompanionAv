using System.Windows;

namespace IRDC
{
    public static class VisibilityConverter
    {
        public static Visibility BoolToVisibilityConverter(bool value)
        {
            if (value)
            {
                return Visibility.Visible;
            }

            return Visibility.Hidden;
        }
    }
}
