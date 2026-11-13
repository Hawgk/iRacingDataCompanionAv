using System.IO;
using System.Linq;

namespace IRDCav.Configuration
{
    /// <summary>Checks a configuration for values the overlays can't use.</summary>
    public static class OverlayConfigurationValidator
    {
        /// <exception cref="InvalidDataException">With a message that tells the user what to fix.</exception>
        public static void Validate(OverlayConfiguration configuration)
        {
            ValidateSections(configuration);
            if (string.IsNullOrWhiteSpace(configuration.Font))
            {
                throw new InvalidDataException("The font name is empty.");
            }
            ValidateFontSizes(configuration.FontSizes);
            ValidateDecimals(configuration.Decimals);
        }

        // Explicit nulls in a file bypass the defaults, so they are rejected here.
        private static void ValidateSections(OverlayConfiguration configuration)
        {
            if (configuration.ColorScheme == null || configuration.FontSizes == null || configuration.Fuel == null
                || configuration.Decimals == null || configuration.Standings?.Columns == null || configuration.Relatives?.Columns == null)
            {
                throw new InvalidDataException("A section is empty or set to null.");
            }
        }

        private static void ValidateFontSizes(FontSizesConfiguration sizes)
        {
            double[] all = { sizes.Small, sizes.Normal, sizes.Large };
            if (all.Any(size => size < FontSizesConfiguration.Min || size > FontSizesConfiguration.Max))
            {
                throw new InvalidDataException($"Font sizes must be between {FontSizesConfiguration.Min} and {FontSizesConfiguration.Max}.");
            }
            if (sizes.Small > sizes.Normal || sizes.Normal > sizes.Large)
            {
                throw new InvalidDataException("Font sizes must grow from small to normal to large.");
            }
        }

        private static void ValidateDecimals(DecimalsConfiguration settings)
        {
            int[] decimals = { settings.Gap, settings.Interval };
            if (decimals.Any(value => value < 0 || value > DecimalsConfiguration.Max))
            {
                throw new InvalidDataException($"Decimals must be between 0 and {DecimalsConfiguration.Max}.");
            }
        }
    }
}
