using IRDCav.Models;
using System;
using System.Collections.Generic;

namespace IRDCav.Presentation
{
    /// <summary>Everything the overlays display, as one immutable snapshot.</summary>
    public sealed record OverlayState
    {
        public static readonly OverlayState Disconnected = new();

        public bool IsConnected { get; init; }
        public SessionInfoModel Session { get; init; } = new();
        public FuelDataModel Fuel { get; init; } = new();
        public IReadOnlyList<RaceRow> Relatives { get; init; } = Array.Empty<RaceRow>();
        public IReadOnlyList<RaceRow> Standings { get; init; } = Array.Empty<RaceRow>();
    }
}
