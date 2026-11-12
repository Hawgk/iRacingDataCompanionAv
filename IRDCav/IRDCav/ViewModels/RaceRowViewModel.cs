using CommunityToolkit.Mvvm.ComponentModel;
using IRDCav.Models;

namespace IRDCav.ViewModels
{
    /// <summary>
    /// One reusable list row. Updating it in place only notifies the properties that
    /// actually changed, so the list does not rebuild its item containers on every update.
    /// </summary>
    public partial class RaceRowViewModel : ViewModelBase
    {
        [ObservableProperty] private bool _isMe;
        [ObservableProperty] private bool _isFastest;
        [ObservableProperty] private bool _isBehind;
        [ObservableProperty] private bool _isInfront;
        [ObservableProperty] private bool _onPitRoad;

        [ObservableProperty] private string _name = string.Empty;
        [ObservableProperty] private string _carNumber = string.Empty;
        [ObservableProperty] private string _license = string.Empty;
        [ObservableProperty] private string _classColor = RaceRow.Empty.ClassColor;
        [ObservableProperty] private string _licenseColor = RaceRow.Empty.LicenseColor;

        [ObservableProperty] private int _classPosition;
        [ObservableProperty] private int _lapsCompleted;

        [ObservableProperty] private float _interval;
        [ObservableProperty] private float _gap;
        [ObservableProperty] private float _lastLapTime;
        [ObservableProperty] private float _fastestLapTime;

        public void Apply(RaceRow row)
        {
            IsMe = row.IsMe;
            IsFastest = row.IsFastest;
            IsBehind = row.IsBehind;
            IsInfront = row.IsInfront;
            OnPitRoad = row.OnPitRoad;
            Name = row.Name;
            CarNumber = row.CarNumber;
            License = row.License;
            ClassColor = row.ClassColor;
            LicenseColor = row.LicenseColor;
            ClassPosition = row.ClassPosition;
            LapsCompleted = row.LapsCompleted;
            Interval = row.Interval;
            Gap = row.Gap;
            LastLapTime = row.LastLapTime;
            FastestLapTime = row.FastestLapTime;
        }
    }
}
