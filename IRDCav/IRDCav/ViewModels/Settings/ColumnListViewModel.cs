using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IRDCav.Configuration;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace IRDCav.ViewModels.Settings
{
    public partial class ColumnChoiceViewModel : ViewModelBase
    {
        [ObservableProperty]
        private bool _isShown;

        public ColumnChoiceViewModel(RaceColumn column, bool isShown)
        {
            Column = column;
            Label = DisplayName.Of(column.ToString());
            _isShown = isShown;
        }

        public RaceColumn Column { get; }
        public string Label { get; }
    }

    /// <summary>
    /// The columns of one list: shown columns first in their order, then the hidden ones.
    /// The checked columns from top to bottom become the configured column list.
    /// </summary>
    public partial class ColumnListViewModel : ViewModelBase
    {
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(MoveUpCommand), nameof(MoveDownCommand))]
        private ColumnChoiceViewModel? _selectedColumn;

        [ObservableProperty]
        private bool _showColumnTitles;

        public ObservableCollection<ColumnChoiceViewModel> Columns { get; } = new();

        public void Load(ListOverlayConfiguration table)
        {
            Columns.Clear();
            foreach (RaceColumn column in table.Columns.Distinct())
            {
                Columns.Add(new ColumnChoiceViewModel(column, isShown: true));
            }
            foreach (RaceColumn column in Enum.GetValues<RaceColumn>().Except(table.Columns))
            {
                Columns.Add(new ColumnChoiceViewModel(column, isShown: false));
            }
            ShowColumnTitles = table.ShowColumnTitles;
        }

        public ListOverlayConfiguration ToConfiguration() => new()
        {
            ShowColumnTitles = ShowColumnTitles,
            Columns = Columns.Where(choice => choice.IsShown).Select(choice => choice.Column).ToList(),
        };

        [RelayCommand(CanExecute = nameof(CanMoveUp))]
        private void MoveUp() => MoveSelected(-1);

        [RelayCommand(CanExecute = nameof(CanMoveDown))]
        private void MoveDown() => MoveSelected(1);

        private bool CanMoveUp() => SelectedColumn != null && Columns.IndexOf(SelectedColumn) > 0;

        private bool CanMoveDown() => SelectedColumn != null && Columns.IndexOf(SelectedColumn) < Columns.Count - 1;

        private void MoveSelected(int offset)
        {
            int index = Columns.IndexOf(SelectedColumn!);
            Columns.Move(index, index + offset);
            MoveUpCommand.NotifyCanExecuteChanged();
            MoveDownCommand.NotifyCanExecuteChanged();
        }
    }
}
