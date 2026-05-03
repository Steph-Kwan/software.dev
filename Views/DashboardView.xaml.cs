using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using TrackMaintenance.Dialogs;
using TrackMaintenance.Models;
using TrackMaintenance.Services;

namespace TrackMaintenance.Views;

public partial class DashboardView : UserControl
{
    private readonly Vehicle    _vehicle;
    private readonly MainWindow _main;

    public DashboardView(Vehicle vehicle, MainWindow main)
    {
        InitializeComponent();
        _vehicle = vehicle;
        _main    = main;
        Loaded  += (_, _) => Refresh();
    }

    public void Refresh()
    {
        var statuses = DataService.GetAllStatuses(_vehicle);
        CardsPanel.Children.Clear();

        if (statuses.Count == 0)
        {
            EmptyState.Visibility  = Visibility.Visible;
            AlertBanner.Visibility = Visibility.Collapsed;
            return;
        }
        EmptyState.Visibility = Visibility.Collapsed;

        // Alert
        var overdueCount = statuses.Count(s => s.Level == StatusLevel.Overdue);
        var warnCount    = statuses.Count(s => s.Level == StatusLevel.Warning);
        if (overdueCount > 0)
        {
            AlertBanner.Visibility = Visibility.Visible;
            AlertBanner.Background = new SolidColorBrush(Color.FromRgb(254, 226, 226));
            AlertText.Foreground   = new SolidColorBrush(Color.FromRgb(153, 27, 27));
            AlertText.Text = $"！  有 {overdueCount} 个项目已超期，请立即安排保养！";
        }
        else if (warnCount > 0)
        {
            AlertBanner.Visibility = Visibility.Visible;
            AlertBanner.Background = new SolidColorBrush(Color.FromRgb(254, 243, 199));
            AlertText.Foreground   = new SolidColorBrush(Color.FromRgb(146, 64, 14));
            AlertText.Text = $"！  有 {warnCount} 个项目即将到期，请提前安排保养。";
        }
        else
        {
            AlertBanner.Visibility = Visibility.Collapsed;
        }

        foreach (var s in statuses)
            CardsPanel.Children.Add(BuildCard(s));
    }

    private Border BuildCard(MaintenanceStatus status)
    {
        var color = (Color)ColorConverter.ConvertFromString(status.StatusColor);
        var brush = new SolidColorBrush(color);

        // Top strip
        var strip = new Rectangle { Height = 5, Fill = brush };

        // Badge
        var badge = new Border
        {
            Background = brush, CornerRadius = new CornerRadius(4),
            Padding = new Thickness(8, 3, 8, 3),
            Child = new TextBlock
            {
                Text = status.StatusText, Foreground = Brushes.White,
                FontSize = 11, FontWeight = FontWeights.Bold
            }
        };

        // Title row
        var titleRow = new Grid();
        titleRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        titleRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var titleTb = new TextBlock
        {
            Text = status.Item.ComponentName, FontSize = 15, FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromRgb(17, 24, 39)),
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(titleTb, 0); Grid.SetColumn(badge, 1);
        titleRow.Children.Add(titleTb); titleRow.Children.Add(badge);

        // Subtitle
        var sub = new TextBlock
        {
            Text = status.Item.ComponentSubtitle,
            FontSize = 12, Margin = new Thickness(0, 3, 0, 14),
            Foreground = new SolidColorBrush(Color.FromRgb(75, 85, 99))
        };

        // Progress bar
        var trackBar = new Border
        {
            Height = 10, CornerRadius = new CornerRadius(5),
            Background = new SolidColorBrush(Color.FromRgb(229, 231, 235))
        };
        var fillBar = new Border
        {
            Height = 10, CornerRadius = new CornerRadius(5),
            Background = brush, HorizontalAlignment = HorizontalAlignment.Left, Width = 0
        };
        var barGrid = new Grid { Margin = new Thickness(0, 0, 0, 6) };
        barGrid.Children.Add(trackBar);
        barGrid.Children.Add(fillBar);
        double pct = status.PercentCapped / 100.0;
        barGrid.SizeChanged += (_, e) => fillBar.Width = e.NewSize.Width * pct;

        // Percent label
        var pctTb = new TextBlock { FontSize = 12, Margin = new Thickness(0, 0, 0, 12) };
        pctTb.Inlines.Add(new System.Windows.Documents.Run($"{status.PercentUsed:F0}%  已消耗")
            { FontWeight = FontWeights.Bold, Foreground = brush });

        // Stats
        var statsRow = new Grid { Margin = new Thickness(0, 0, 0, 10) };
        statsRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        statsRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        var left  = MakeStat("等效里程", $"{status.EquivKmUsed:N0} km", new SolidColorBrush(Color.FromRgb(17,24,39)));
        var right = MakeStat("距保养", status.RemainingDisplay, brush);
        Grid.SetColumn(left, 0); Grid.SetColumn(right, 1);
        statsRow.Children.Add(left); statsRow.Children.Add(right);

        // Info
        var info = new TextBlock
        {
            FontSize = 12, Margin = new Thickness(0, 0, 0, 4),
            Foreground = new SolidColorBrush(Color.FromRgb(55, 65, 81))
        };
        info.Inlines.Add(new System.Windows.Documents.Run("上次保养：") { FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(17,24,39)) });
        info.Inlines.Add(new System.Windows.Documents.Run(status.LastServiceDisplay) { Foreground = new SolidColorBrush(Color.FromRgb(55,65,81)) });

        var trackInfo = new TextBlock
        {
            FontSize = 12, Margin = new Thickness(0, 0, 0, 14),
            Foreground = new SolidColorBrush(Color.FromRgb(55, 65, 81))
        };
        trackInfo.Inlines.Add(new System.Windows.Documents.Run("赛道折算：") { FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(17,24,39)) });
        trackInfo.Inlines.Add(new System.Windows.Documents.Run($"+{status.Item.TrackAdditionPerSession:N0} km / 节") { Foreground = new SolidColorBrush(Color.FromRgb(55,65,81)) });

        // Button
        var btn = new Button
        {
            Content = "√  记录保养完成",
            Style = (Style)Application.Current.Resources["CardBtn"],
            Tag = status
        };
        btn.Click += (s, _) =>
        {
            var st  = (MaintenanceStatus)((Button)s).Tag;
            var dlg = new MaintenanceDialog(_vehicle, st.Item) { Owner = _main };
            if (dlg.ShowDialog() == true) Refresh();
        };

        // Body
        var body = new StackPanel { Margin = new Thickness(16, 14, 16, 14) };
        body.Children.Add(titleRow);
        body.Children.Add(sub);
        body.Children.Add(barGrid);
        body.Children.Add(pctTb);
        body.Children.Add(statsRow);
        body.Children.Add(info);
        body.Children.Add(trackInfo);
        body.Children.Add(btn);

        var inner = new Grid();
        inner.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        inner.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        Grid.SetRow(strip, 0); Grid.SetRow(body, 1);
        inner.Children.Add(strip); inner.Children.Add(body);

        return new Border
        {
            Width = 268, Background = Brushes.White,
            CornerRadius = new CornerRadius(10),
            Margin = new Thickness(8), ClipToBounds = true,
            Effect = new System.Windows.Media.Effects.DropShadowEffect
                { Color = Colors.Black, BlurRadius = 10, ShadowDepth = 2, Opacity = 0.10 },
            Child = inner
        };
    }

    private static StackPanel MakeStat(string label, string value, Brush valueBrush)
    {
        var sp = new StackPanel();
        sp.Children.Add(new TextBlock
        {
            Text = label, FontSize = 10, FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromRgb(55, 65, 81)), Margin = new Thickness(0, 0, 0, 2)
        });
        sp.Children.Add(new TextBlock
        {
            Text = value, FontSize = 14, FontWeight = FontWeights.Bold, Foreground = valueBrush
        });
        return sp;
    }
}
