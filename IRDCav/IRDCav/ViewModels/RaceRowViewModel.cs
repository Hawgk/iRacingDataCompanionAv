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
        [ObservableProperty] private bool _isCar;
        [ObservableProperty] private bool _isClassHeader;

        [ObservableProperty] private bool _isMe;
        [ObservableProperty] private bool _isFastest;
        [ObservableProperty] private bool _isLapAhead;
        [ObservableProperty] private bool _isLapBehind;
        [ObservableProperty] private bool _onPitRoad;

        [ObservableProperty] private string _name = string.Empty;
        [ObservableProperty] private string _carNumber = string.Empty;
        [ObservableProperty] private string _className = string.Empty;
        [ObservableProperty] private string _license = string.Empty;
        [ObservableProperty] private string _classColor = string.Empty;
        [ObservableProperty] private string _licenseColor = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsIRatingGain), nameof(IsIRatingLoss))]
        private int _iRatingChange;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsIRatingGain), nameof(IsIRatingLoss))]
        private bool _hasIRatingChange;

        [ObservableProperty] private int _classPosition;
        [ObservableProperty] private int _lapsCompleted;

        [ObservableProperty] private float _interval;
        [ObservableProperty] private float _gap;
        [ObservableProperty] private float _lastLapTime;
        [ObservableProperty] private float _fastestLapTime;

        public bool IsIRatingGain => HasIRatingChange && IRatingChange > 0;
        public bool IsIRatingLoss => HasIRatingChange && IRatingChange < 0;

        public void Apply(RaceRow row)
        {
            ApplyState(row);
            ApplyIdentity(row);
            ApplyTiming(row);
        }

        private void ApplyState(RaceRow row)
        {
            IsCar = row.Kind == RaceRowKind.Car;
            IsClassHeader = row.Kind == RaceRowKind.ClassHeader;
            IsMe = row.IsMe;
            IsFastest = row.IsFastest;
            IsLapAhead = row.IsLapAhead;
            IsLapBehind = row.IsLapBehind;
            OnPitRoad = row.OnPitRoad;
        }

        private void ApplyIdentity(RaceRow row)
        {
            Name = row.Name;
            CarNumber = row.CarNumber;
            ClassName = row.ClassName;
            License = row.License;
            ClassColor = row.ClassColor;
            LicenseColor = row.LicenseColor;
        }

        private void ApplyTiming(RaceRow row)
        {
            IRatingChange = row.IRatingChange;
            HasIRatingChange = row.HasIRatingChange;
            ClassPosition = row.ClassPosition;
            LapsCompleted = row.LapsCompleted;
            Interval = row.Interval;
            Gap = row.Gap;
            LastLapTime = row.LastLapTime;
            FastestLapTime = row.FastestLapTime;
        }
    }
}
