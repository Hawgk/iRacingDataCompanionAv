namespace IRDCav.Configuration
{
    /// <summary>The configuration in use, which can be replaced while the overlays run.</summary>
    public interface IOverlayConfigurationStore
    {
        OverlayConfiguration Current { get; }

        /// <summary>Validates, saves and applies the configuration.</summary>
        /// <exception cref="System.IO.InvalidDataException">The configuration has an invalid value.</exception>
        /// <exception cref="System.IO.IOException">The file could not be written.</exception>
        /// <exception cref="System.UnauthorizedAccessException">The file or its folder is not writable.</exception>
        void Save(OverlayConfiguration configuration);
    }
}
