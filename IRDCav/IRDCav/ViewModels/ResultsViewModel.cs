using CommunityToolkit.Mvvm.ComponentModel;
using IRDCav.Models;

namespace IRDCav.ViewModels
{
    public partial class ResultsViewModel : ViewModelBase
    {
        [ObservableProperty]
        private bool _isVisible;

        [ObservableProperty]
        private SessionInfoModel _sessionInfo = new();

        public RaceRowCollection RaceDataList { get; } = new();
    }
}
