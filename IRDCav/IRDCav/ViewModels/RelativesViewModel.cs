using CommunityToolkit.Mvvm.ComponentModel;
using IRDCav.Configuration;
using System.Collections.Generic;

namespace IRDCav.ViewModels
{
    public partial class RelativesViewModel : ViewModelBase
    {
        [ObservableProperty]
        private bool _isVisible;

        public RelativesViewModel(ListOverlayConfiguration table)
        {
            Columns = table.Columns;
            ShowColumnTitles = table.ShowColumnTitles;
        }

        public IReadOnlyList<RaceColumn> Columns { get; }

        public bool ShowColumnTitles { get; }

        public RaceRowCollection RaceDataList { get; } = new();
    }
}
