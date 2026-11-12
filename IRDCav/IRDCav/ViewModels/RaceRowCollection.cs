using IRDCav.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace IRDCav.ViewModels
{
    /// <summary>Row list that reuses its row view models and only adds or removes rows at the end.</summary>
    public sealed class RaceRowCollection : ObservableCollection<RaceRowViewModel>
    {
        public void Update(IReadOnlyList<RaceRow> rows)
        {
            while (Count > rows.Count)
            {
                RemoveAt(Count - 1);
            }
            while (Count < rows.Count)
            {
                Add(new RaceRowViewModel());
            }
            for (int i = 0; i < rows.Count; i++)
            {
                this[i].Apply(rows[i]);
            }
        }
    }
}
