using Avalonia.Threading;
using System;

namespace IRDCav.Presentation
{
    internal sealed class AvaloniaUiDispatcher : IUiDispatcher
    {
        public void Post(Action action) => Dispatcher.UIThread.Post(action);
    }
}
