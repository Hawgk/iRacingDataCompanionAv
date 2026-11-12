using CommunityToolkit.Mvvm.ComponentModel;
using IRDCav.Configuration;
using IRDCav.Models;
using System.Collections.Generic;

namespace IRDCav.ViewModels
{
    public partial class ResultsViewModel : ViewModelBase
    {
        [ObservableProperty]
        private bool _isVisible;

        [ObservableProperty]
        private SessionInfoModel _sessionInfo = new();

        public ResultsViewModel(ListOverlayConfiguration table)
        {
            Columns = table.Columns;
            ShowColumnTitles = table.ShowColumnTitles;
        }

        public IReadOnlyList<RaceColumn> Columns { get; }

        public bool ShowColumnTitles { get; }

        public RaceRowCollection RaceDataList { get; } = new();
    }
}
