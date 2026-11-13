using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using IRDCav.Configuration;
using IRDCav.ViewModels;
using IRDCav.Views.Theme;
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
        private const double FallbackCharacterWidth = 8;

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
            var metrics = new ColumnMetrics(Measure(OverlayTheme.DigitWidthKey), Measure(OverlayTheme.TitleCharacterWidthKey));
            List<SizedColumn> columns = Columns.Select(column => SizedColumn.Of(column, metrics)).ToList();

            _titles.Children.Clear();
            _titles.Children.AddRange(columns.Select(column => Cell(column, TitleText(column.Layout))));
            _items.ItemTemplate = new FuncDataTemplate<RaceRowViewModel>((_, _) =>
                new RaceRowView(columns.Select(column => Cell(column, Build(column.Template)))));
        }

        /// <summary>A character width published by the theme; a typical value when no theme was applied.</summary>
        private static double Measure(string resourceKey) =>
            Application.Current?.TryGetResource(resourceKey, null, out object? width) == true && width is double value
                ? value
                : FallbackCharacterWidth;

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

        private static Control Cell(SizedColumn column, Control content)
        {
            content.HorizontalAlignment = column.Layout.Alignment;
            content.VerticalAlignment = VerticalAlignment.Center;
            return new Border { Width = column.Width, Classes = { "cell" }, Child = content };
        }

        private sealed record SizedColumn(RaceColumnLayout Layout, double Width, IDataTemplate Template)
        {
            public static SizedColumn Of(RaceColumn column, ColumnMetrics metrics)
            {
                RaceColumnLayout layout = RaceColumnLayout.Of(column);
                return new SizedColumn(layout, layout.WidthFor(metrics), CellTemplate(column));
            }
        }
    }
}
