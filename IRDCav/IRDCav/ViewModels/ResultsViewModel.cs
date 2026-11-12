using CommunityToolkit.Mvvm.ComponentModel;
using IRDCav.Models;
using System;
using System.Collections.Generic;

namespace IRDCav.ViewModels
{
    public partial class ResultsViewModel : ViewModelBase
    {
        [ObservableProperty]
        private bool _isVisible;

        [ObservableProperty]
        private SessionInfoModel _sessionInfo = new();

        [ObservableProperty]
        private IReadOnlyList<RaceDataModel> _raceDataList = Array.Empty<RaceDataModel>();
    }
}
