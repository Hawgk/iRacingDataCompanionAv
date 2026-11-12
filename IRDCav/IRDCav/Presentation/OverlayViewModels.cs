using IRDCav.ViewModels;

namespace IRDCav.Presentation
{
    public sealed record OverlayViewModels(
        ResultsViewModel Results,
        RelativesViewModel Relatives,
        InputGraphViewModel InputGraph);
}
