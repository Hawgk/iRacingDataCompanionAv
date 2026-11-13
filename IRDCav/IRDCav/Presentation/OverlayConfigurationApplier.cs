using Avalonia;
using IRDCav.Configuration;
using IRDCav.Views.Theme;

namespace IRDCav.Presentation
{
    /// <summary>
    /// Brings a configuration onto the screen: the theme first, then the view models, so the
    /// tables rebuild their columns with the new font measurements. Runs on the UI thread.
    /// </summary>
    internal sealed class OverlayConfigurationApplier
    {
        private readonly Application _application;
        private readonly OverlayViewModels _viewModels;

        public OverlayConfigurationApplier(Application application, OverlayViewModels viewModels)
        {
            _application = application;
            _viewModels = viewModels;
        }

        public void Apply(OverlayConfiguration configuration)
        {
            OverlayTheme.Apply(_application, configuration);
            _viewModels.Results.ApplyTable(configuration.Standings);
            _viewModels.Relatives.ApplyTable(configuration.Relatives);
            _viewModels.Fuel.RefuelEstimate = configuration.Fuel.RefuelEstimate;
        }
    }
}
