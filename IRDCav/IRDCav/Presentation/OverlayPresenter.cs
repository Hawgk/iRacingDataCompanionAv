using IRDCav.Models;
using IRDCav.Telemetry;
using System;
using System.Collections.Generic;

namespace IRDCav.Presentation
{
    /// <summary>
    /// The only place where view models are touched from outside the UI thread:
    /// every update is marshalled through the <see cref="IUiDispatcher"/>.
    /// </summary>
    internal sealed class OverlayPresenter : IOverlayPresenter
    {
        private readonly IUiDispatcher _dispatcher;
        private readonly OverlayViewModels _viewModels;

        public OverlayPresenter(IUiDispatcher dispatcher, OverlayViewModels viewModels)
        {
            _dispatcher = dispatcher;
            _viewModels = viewModels;
        }

        public void ShowOverlays() => _dispatcher.Post(() => SetVisible(true));

        public void HideOverlays() => _dispatcher.Post(() =>
        {
            SetVisible(false);
            ClearData();
        });

        public void ShowInputs(InputSample inputs) =>
            _dispatcher.Post(() => _viewModels.InputGraph.AddSample(inputs));

        public void ShowSession(SessionInfoModel session) => _dispatcher.Post(() =>
        {
            _viewModels.Results.SessionInfo = session;
            _viewModels.Relatives.SessionInfo = session;
        });

        public void ShowStandings(IReadOnlyList<RaceDataModel> rows) =>
            _dispatcher.Post(() => _viewModels.Results.RaceDataList = rows);

        public void ShowRelatives(IReadOnlyList<RaceDataModel> rows) =>
            _dispatcher.Post(() => _viewModels.Relatives.RaceDataList = rows);

        public void ShowFuel(FuelDataModel fuel) =>
            _dispatcher.Post(() => _viewModels.Relatives.FuelData = fuel);

        private void SetVisible(bool isVisible)
        {
            _viewModels.Results.IsVisible = isVisible;
            _viewModels.Relatives.IsVisible = isVisible;
            _viewModels.InputGraph.IsVisible = isVisible;
        }

        private void ClearData()
        {
            _viewModels.Results.RaceDataList = Array.Empty<RaceDataModel>();
            _viewModels.Results.SessionInfo = new SessionInfoModel();
            _viewModels.Relatives.RaceDataList = Array.Empty<RaceDataModel>();
            _viewModels.Relatives.SessionInfo = new SessionInfoModel();
            _viewModels.Relatives.FuelData = new FuelDataModel();
        }
    }
}
