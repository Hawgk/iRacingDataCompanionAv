using CommunityToolkit.Mvvm.ComponentModel;
using IRDCav.Configuration;
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
        private IReadOnlyList<RaceColumn> _columns = Array.Empty<RaceColumn>();

        [ObservableProperty]
        private bool _showColumnTitles;

        public RaceRowCollection RaceDataList { get; } = new();

        /// <summary>Always replaces the column list, so the table rebuilds its rows, e.g. for a new font.</summary>
        public void ApplyTable(ListOverlayConfiguration table)
        {
            Columns = new List<RaceColumn>(table.Columns);
            ShowColumnTitles = table.ShowColumnTitles;
        }
    }
}
