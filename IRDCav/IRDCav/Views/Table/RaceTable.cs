using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using IRDCav.Configuration;
using IRDCav.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace IRDCav.Views.Table
{
    /// <summary>
    /// A list of race rows whose columns come from the configuration. Each column's cell is a
    /// DataTemplate keyed "RaceColumn.{name}" in the application resources.
    /// </summary>
    public sealed class RaceTable : UserControl
    {
        public static readonly StyledProperty<IReadOnlyList<RaceColumn>> ColumnsProperty =
            AvaloniaProperty.Register<RaceTable, IReadOnlyList<RaceColumn>>(nameof(Columns), Array.Empty<RaceColumn>());

        public static readonly StyledProperty<bool> ShowColumnTitlesProperty =
            AvaloniaProperty.Register<RaceTable, bool>(nameof(ShowColumnTitles));

        public static readonly StyledProperty<IEnumerable?> RowsProperty =
            AvaloniaProperty.Register<RaceTable, IEnumerable?>(nameof(Rows));

        private const string TemplateKeyPrefix = "RaceColumn.";

        private readonly StackPanel _titles = new() { Orientation = Orientation.Horizontal, Classes = { "columnTitles" }, IsVisible = false };
        private readonly ItemsControl _items = new();

        public RaceTable()
        {
            Content = new StackPanel { Children = { _titles, _items } };
        }

        public IReadOnlyList<RaceColumn> Columns
        {
            get => GetValue(ColumnsProperty);
            set => SetValue(ColumnsProperty, value);
        }

        public bool ShowColumnTitles
        {
            get => GetValue(ShowColumnTitlesProperty);
            set => SetValue(ShowColumnTitlesProperty, value);
        }

        public IEnumerable? Rows
        {
            get => GetValue(RowsProperty);
            set => SetValue(RowsProperty, value);
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == ColumnsProperty)
            {
                BuildColumns();
            }
            else if (change.Property == ShowColumnTitlesProperty)
            {
                _titles.IsVisible = ShowColumnTitles;
            }
            else if (change.Property == RowsProperty)
            {
                _items.ItemsSource = Rows;
            }
        }

        private void BuildColumns()
        {
            var columns = Columns.Select(column => (Layout: RaceColumnLayout.Of(column), Template: CellTemplate(column))).ToList();

            _titles.Children.Clear();
            _titles.Children.AddRange(columns.Select(column => Cell(column.Layout, TitleText(column.Layout))));
            _items.ItemTemplate = new FuncDataTemplate<RaceRowViewModel>((_, _) =>
                new RaceRowView(columns.Select(column => Cell(column.Layout, Build(column.Template)))));
        }

        private static IDataTemplate CellTemplate(RaceColumn column)
        {
            string key = TemplateKeyPrefix + column;
            if (Application.Current?.TryGetResource(key, null, out object? template) != true || template is not IDataTemplate dataTemplate)
            {
                throw new InvalidOperationException($"No cell template '{key}' found in the application resources.");
            }
            return dataTemplate;
        }

        private static Control Build(IDataTemplate template) =>
            template.Build(null) ?? throw new InvalidOperationException("A race column template produced no control.");

        private static Control TitleText(RaceColumnLayout layout) => new TextBlock { Text = layout.Title, Classes = { "columnTitle" } };

        private static Control Cell(RaceColumnLayout layout, Control content)
        {
            content.HorizontalAlignment = layout.Alignment;
            content.VerticalAlignment = VerticalAlignment.Center;
            return new Border { Width = layout.Width, Classes = { "cell" }, Child = content };
        }
    }
}
