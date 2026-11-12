using IRDCav.Models;
using IRDCav.Telemetry;
using System.Collections.Generic;

namespace IRDCav.Presentation
{
    /// <summary>
    /// Pushes processed data to the overlays. May be called from any thread; arguments
    /// must not be modified by the caller afterwards.
    /// </summary>
    public interface IOverlayPresenter
    {
        void ShowOverlays();
        void HideOverlays();
        void ShowInputs(InputSample inputs);
        void ShowSession(SessionInfoModel session);
        void ShowStandings(IReadOnlyList<RaceDataModel> rows);
        void ShowRelatives(IReadOnlyList<RaceDataModel> rows);
        void ShowFuel(FuelDataModel fuel);
    }
}
