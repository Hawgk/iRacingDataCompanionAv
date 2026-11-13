using CommunityToolkit.Mvvm.ComponentModel;
using IRDCav.Models;
using System.Collections.ObjectModel;

namespace IRDCav.ViewModels
{
    /// <summary>The sector view: the best lap times of the car and its class, the recent average, and one reusable row per sector.</summary>
    public partial class LapSectorsViewModel : ViewModelBase
    {
        [ObservableProperty]
        private bool _isVisible;

        [ObservableProperty]
        private float _bestLapTime;

        [ObservableProperty]
        private float _classBestLapTime;

        [ObservableProperty]
        private float _averageLapTime;

        /// <summary>One row per sector of the track; rows are reused and only added or removed at the end.</summary>
        public ObservableCollection<SectorRowViewModel> Sectors { get; } = new();

        public void Apply(LapSectorsModel lapSectors)
        {
            BestLapTime = lapSectors.BestLapTime;
            ClassBestLapTime = lapSectors.ClassBestLapTime;
            AverageLapTime = lapSectors.AverageLapTime;
            Resize(lapSectors.Sectors.Count);
            for (int i = 0; i < Sectors.Count; i++)
            {
                Sectors[i].Apply(lapSectors.Sectors[i]);
            }
        }

        private void Resize(int count)
        {
            while (Sectors.Count > count)
            {
                Sectors.RemoveAt(Sectors.Count - 1);
            }
            while (Sectors.Count < count)
            {
                Sectors.Add(new SectorRowViewModel(Sectors.Count + 1));
            }
        }
    }

    /// <summary>One sector: its time on the best lap and the delta of the lap being driven.</summary>
    public partial class SectorRowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private float _bestTime;

        /// <summary>NaN while there is nothing to compare.</summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsGain), nameof(IsLoss))]
        private float _delta = float.NaN;

        public SectorRowViewModel(int number)
        {
            Number = number;
        }

        public int Number { get; }

        public bool IsGain => Delta < 0;
        public bool IsLoss => Delta > 0;

        public void Apply(SectorModel sector)
        {
            BestTime = sector.BestTime;
            Delta = sector.Delta;
        }
    }
}
