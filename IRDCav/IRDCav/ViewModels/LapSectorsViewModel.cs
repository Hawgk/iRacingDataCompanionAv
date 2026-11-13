using CommunityToolkit.Mvvm.ComponentModel;
using IRDCav.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IRDCav.ViewModels
{
    /// <summary>The sector view: the best lap times of the car and its class, and one reusable row per sector.</summary>
    public partial class LapSectorsViewModel : ViewModelBase
    {
        [ObservableProperty]
        private bool _isVisible;

        [ObservableProperty]
        private float _bestLapTime;

        [ObservableProperty]
        private float _classBestLapTime;

        public IReadOnlyList<SectorRowViewModel> Sectors { get; } = Enumerable.Range(1, LapSectorsModel.SectorCount)
            .Select(number => new SectorRowViewModel(number))
            .ToArray();

        public void Apply(LapSectorsModel lapSectors)
        {
            BestLapTime = lapSectors.BestLapTime;
            ClassBestLapTime = lapSectors.ClassBestLapTime;
            int count = Math.Min(Sectors.Count, lapSectors.Sectors.Count);
            for (int i = 0; i < count; i++)
            {
                Sectors[i].Apply(lapSectors.Sectors[i]);
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
