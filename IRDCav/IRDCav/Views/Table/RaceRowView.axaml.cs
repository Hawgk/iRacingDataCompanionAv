using Avalonia.Controls;
using System;
using System.Collections.Generic;

namespace IRDCav.Views.Table
{
    /// <summary>One row of a <see cref="RaceTable"/>: either a car with its cells or a class header.</summary>
    public partial class RaceRowView : UserControl
    {
        public RaceRowView() : this(Array.Empty<Control>())
        {
        }

        public RaceRowView(IEnumerable<Control> cells)
        {
            InitializeComponent();
            Cells.Children.AddRange(cells);
        }
    }
}
