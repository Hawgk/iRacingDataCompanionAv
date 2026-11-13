using IRDCav.Configuration;

namespace IRDCav.Presentation
{
    /// <summary>Saves configurations to config.json and applies them to the running overlays.</summary>
    internal sealed class LiveOverlayConfiguration : IOverlayConfigurationStore
    {
        private readonly JsonOverlayConfigurationFile _file;
        private readonly OverlayConfigurationApplier _applier;

        public LiveOverlayConfiguration(JsonOverlayConfigurationFile file, OverlayConfigurationApplier applier)
        {
            _file = file;
            _applier = applier;
        }

        public OverlayConfiguration Current { get; private set; } = new();

        /// <summary>Applies the configuration without saving it, e.g. the one read at startup.</summary>
        public void Load(OverlayConfiguration configuration)
        {
            _applier.Apply(configuration);
            Current = configuration;
        }

        public void Save(OverlayConfiguration configuration)
        {
            OverlayConfigurationValidator.Validate(configuration);
            _file.Write(configuration);
            Load(configuration);
        }
    }
}
