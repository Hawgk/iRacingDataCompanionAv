using System;

namespace IRDCav.Presentation
{
    public interface IUiDispatcher
    {
        /// <summary>Queues <paramref name="action"/> for execution on the UI thread.</summary>
        void Post(Action action);
    }
}
