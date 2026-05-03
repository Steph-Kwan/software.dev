using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TrackMaintenance.Dialogs;
using TrackMaintenance.Models;
using TrackMaintenance.Services;

namespace TrackMaintenance.Views;

public partial class VehiclesView : UserControl
{
    private readonly MainWindow _main;

    public VehiclesView(MainWindow main)
    {
        InitializeComponent();
        _main = main;
        Loaded += (_, _) => Refresh();
    }

    public void Refresh()
    {
        VehicleList.Children.Clear();
        var vehicles = DataService.Data.Vehicles;

        if (vehicles.Count == 0)
        {
            EmptyState.Visibility = Visibility.Visible;
            return;
        }
        EmptyState.Visibility = Visibility.Collapsed;

        foreach (var v in vehicles)
            VehicleList.Children.Add(BuildVehicleCard(v));
    }

    private UIElement BuildVehicleCard(Vehicle v)
    {
        var items    = DataService.Data.MaintenanceItems.Where(i => i.VehicleId == v.Id).ToList();
        var statuses = DataService.GetAllStatuses(v);
        var critical = statuses.Count(s => s.Level >= StatusLevel.Overdue);
        var warn     = statuses.Count(s => s.Level == StatusLevel.Warning);

        var summaryColor = critical > 0 ? Color.FromRgb(185, 28, 28)
                         : warn > 0     ? Color.FromRgb(194, 65, 12)
                         : Color.FromRgb(21, 128, 61);
        var summaryText  = critical > 0 ? $"{critical} 项急需保养"
                         : warn > 0     ? $"{warn} 项即将到期"
                         : "所有项目良好";

        // ── Vehicle header ────────────────────────────────────────────────
        var headerGrid = new Grid();
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var titleStack = new StackPanel();
        titleStack.Children.Add(new TextBlock
        {
            Text = v.DisplayName, FontSize = 16, FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromRgb(17, 24, 39))
        });
        titleStack.Children.Add(new TextBlock
        {
            Text = $"{(v.LicensePlate.Length > 0 ? v.LicensePlate + "  |  " : "")}里程 {v.CurrentMileage:N0} km",
            FontSize = 12, Foreground = new SolidColorBrush(Color.FromRgb(75, 85, 99)), Margin = new Thickness(0, 3, 0, 0)
        });

        var statusBadge = new Border
        {
            Background = new SolidColorBrush(summaryColor), CornerRadius = new CornerRadius(5),
            Padding = new Thickness(10, 4, 10, 4), VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0),
            Child = new TextBlock { Text = summaryText, Foreground = Brushes.White, FontSize = 12, FontWeight = FontWeights.Bold }
        };

        var editBtn = new Button
        {
            Content = "编辑", Style = (Style)Application.Current.Resources["OutlineBtn"],
            Height = 32, Padding = new Thickness(12, 0, 12, 0),
            Margin = new Thickness(0, 0, 8, 0), Tag = v
        };
        editBtn.Click += EditVehicle_Click;

        var delBtn = new Button
        {
            Content = "删除车辆", Style = (Style)Application.Current.Resources["DangerBtn"],
            Tag = v
        };
        delBtn.Click += DeleteVehicle_Click;

        Grid.SetColumn(titleStack,   0);
        Grid.SetColumn(statusBadge,  1);
        Grid.SetColumn(editBtn,      1);
        Grid.SetColumn(delBtn,       2);

        // rearrange: title | badge | edit | del
        headerGrid.ColumnDefinitions.Clear();
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        Grid.SetColumn(titleStack,   0);
        Grid.SetColumn(statusBadge,  1);
        Grid.SetColumn(editBtn,      2);
        Grid.SetColumn(delBtn,       3);
        headerGrid.Children.Add(titleStack);
        headerGrid.Children.Add(statusBadge);
        headerGrid.Children.Add(editBtn);
        headerGrid.Children.Add(delBtn);

        // ── Divider ────────────────────────────────────────────────────────
        var divider = new Border { Height = 1, Background = new SolidColorBrush(Color.FromRgb(229, 231, 235)), Margin = new Thickness(0, 14, 0, 12) };

        // ── Items header row ───────────────────────────────────────────────
        var itemsHeader = new Grid { Margin = new Thickness(0, 0, 0, 8) };
        itemsHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        itemsHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var itemsTitle = new TextBlock
        {
            Text = "保养项目", FontSize = 13, FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromRgb(55, 65, 81)),
            VerticalAlignment = VerticalAlignment.Center
        };
        var addItemBtn = new Button
        {
            Content = "+ 添加项目", Style = (Style)Application.Current.Resources["GreenBtn"],
            Height = 30, Padding = new Thickness(12, 0, 12, 0), Tag = v
        };
        addItemBtn.Click += AddItem_Click;

        Grid.SetColumn(itemsTitle,  0);
        Grid.SetColumn(addItemBtn,  1);
        itemsHeader.Children.Add(itemsTitle);
        itemsHeader.Children.Add(addItemBtn);

        // ── Items list ─────────────────────────────────────────────────────
        var itemsPanel = new StackPanel();

        if (items.Count == 0)
        {
            itemsPanel.Children.Add(new TextBlock
            {
                Text = "暂无保养项目，点击「添加项目」开始添加",
                FontSize = 12, Foreground = new SolidColorBrush(Color.FromRgb(156, 163, 175)),
                Margin = new Thickness(0, 4, 0, 0)
            });
        }
        else
        {
            // Column header
            var colHeader = new Grid { Margin = new Thickness(0, 0, 0, 4) };
            colHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(160) });
            colHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            colHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(130) });
            colHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
            colHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });

            void AddColHeader(string text, int col)
            {
                var tb = new TextBlock
                {
                    Text = text, FontSize = 11, FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(107, 114, 128))
                };
                Grid.SetColumn(tb, col);
                colHeader.Children.Add(tb);
            }
            AddColHeader("项目名称", 0);
            AddColHeader("英文名", 1);
            AddColHeader("周期 / 赛道折算", 2);
            AddColHeader("状态", 3);
            AddColHeader("操作", 4);
            itemsPanel.Children.Add(colHeader);

            foreach (var item in items)
            {
                var status = DataService.GetStatus(item, v);
                var row    = new Border
                {
                    Background   = item.IsEnabled ? Brushes.White : new SolidColorBrush(Color.FromRgb(249, 250, 251)),
                    CornerRadius = new CornerRadius(6),
                    BorderBrush  = new SolidColorBrush(Color.FromRgb(229, 231, 235)),
                    BorderThickness = new Thickness(1),
                    Padding      = new Thickness(10, 8, 10, 8),
                    Margin       = new Thickness(0, 0, 0, 4)
                };

                var rowGrid = new Grid();
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(160) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(130) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });

                var nameColor  = item.IsEnabled
                    ? (Color)ColorConverter.ConvertFromString(status.StatusColor)
                    : Color.FromRgb(156, 163, 175);
                var nameTb = new TextBlock
                {
                    Text = item.ComponentName, FontSize = 13, FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush(nameColor), VerticalAlignment = VerticalAlignment.Center
                };
                var subTb = new TextBlock
                {
                    Text = item.ComponentSubtitle, FontSize = 11,
                    Foreground = new SolidColorBrush(Color.FromRgb(107, 114, 128)),
                    VerticalAlignment = VerticalAlignment.Center
                };
                var infoTb = new TextBlock
                {
                    Text = $"{item.IntervalKm:N0} km  /  +{item.TrackAdditionPerSession:N0} km/节",
                    FontSize = 12, Foreground = new SolidColorBrush(Color.FromRgb(55, 65, 81)),
                    VerticalAlignment = VerticalAlignment.Center
                };
                var statusTb = new TextBlock
                {
                    Text = item.IsEnabled ? status.StatusText : "已禁用",
                    FontSize = 11, FontWeight = FontWeights.Bold,
                    Foreground = item.IsEnabled
                        ? new SolidColorBrush(nameColor)
                        : new SolidColorBrush(Color.FromRgb(156, 163, 175)),
                    VerticalAlignment = VerticalAlignment.Center
                };

                var btnPanel = new StackPanel { Orientation = Orientation.Horizontal };
                var editItemBtn = new Button
                {
                    Content = "编", Style = (Style)Application.Current.Resources["OutlineBtn"],
                    Height = 26, Width = 26, Padding = new Thickness(0),
                    FontSize = 11, Margin = new Thickness(0, 0, 4, 0), Tag = item
                };
                editItemBtn.Click += EditItem_Click;

                var delItemBtn = new Button
                {
                    Content = "删", Style = (Style)Application.Current.Resources["DangerBtn"],
                    Height = 26, Width = 26, Padding = new Thickness(0),
                    FontSize = 11, Tag = item
                };
                delItemBtn.Click += DeleteItem_Click;

                btnPanel.Children.Add(editItemBtn);
                btnPanel.Children.Add(delItemBtn);

                Grid.SetColumn(nameTb,   0);
                Grid.SetColumn(subTb,    1);
                Grid.SetColumn(infoTb,   2);
                Grid.SetColumn(statusTb, 3);
                Grid.SetColumn(btnPanel, 4);
                rowGrid.Children.Add(nameTb);
                rowGrid.Children.Add(subTb);
                rowGrid.Children.Add(infoTb);
                rowGrid.Children.Add(statusTb);
                rowGrid.Children.Add(btnPanel);

                row.Child = rowGrid;
                itemsPanel.Children.Add(row);
            }
        }

        // ── Assemble card ──────────────────────────────────────────────────
        var body = new StackPanel { Margin = new Thickness(20, 18, 20, 18) };
        body.Children.Add(headerGrid);
        body.Children.Add(divider);
        body.Children.Add(itemsHeader);
        body.Children.Add(itemsPanel);

        return new Border
        {
            Background = Brushes.White, CornerRadius = new CornerRadius(10),
            Margin = new Thickness(0, 0, 0, 16),
            BorderBrush = new SolidColorBrush(Color.FromRgb(209, 213, 219)),
            BorderThickness = new Thickness(1), Child = body,
            Effect = new System.Windows.Media.Effects.DropShadowEffect
            { Color = Colors.Black, BlurRadius = 8, ShadowDepth = 1, Opacity = 0.08 }
        };
    }

    // ── Event handlers ─────────────────────────────────────────────────────────

    private void AddVehicle_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new VehicleDialog { Owner = _main };
        if (dlg.ShowDialog() == true) { _main.RefreshVehicleList(dlg.Result?.Id); Refresh(); }
    }

    private void EditVehicle_Click(object sender, RoutedEventArgs e)
    {
        var v = (Vehicle)((Button)sender).Tag;
        var dlg = new VehicleDialog(v) { Owner = _main };
        if (dlg.ShowDialog() == true) { DataService.Save(); _main.RefreshVehicleList(v.Id); Refresh(); }
    }

    private void DeleteVehicle_Click(object sender, RoutedEventArgs e)
    {
        var v = (Vehicle)((Button)sender).Tag;
        var r = MessageBox.Show($"确认删除「{v.DisplayName}」？所有相关记录将一并删除，不可恢复！",
            "确认删除", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (r == MessageBoxResult.Yes) { DataService.DeleteVehicle(v.Id); _main.RefreshVehicleList(); Refresh(); }
    }

    private void AddItem_Click(object sender, RoutedEventArgs e)
    {
        var v   = (Vehicle)((Button)sender).Tag;
        var dlg = new EditItemDialog(null, v.Id) { Owner = _main };
        if (dlg.ShowDialog() == true) { DataService.Save(); Refresh(); }
    }

    private void EditItem_Click(object sender, RoutedEventArgs e)
    {
        var item = (MaintenanceItem)((Button)sender).Tag;
        var dlg  = new EditItemDialog(item, item.VehicleId) { Owner = _main };
        if (dlg.ShowDialog() == true) { DataService.Save(); Refresh(); }
    }

    private void DeleteItem_Click(object sender, RoutedEventArgs e)
    {
        var item = (MaintenanceItem)((Button)sender).Tag;
        var r    = MessageBox.Show($"确认删除保养项目「{item.ComponentName}」？",
            "确认删除", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (r == MessageBoxResult.Yes)
        {
            DataService.Data.MaintenanceItems.Remove(item);
            DataService.Save();
            Refresh();
        }
    }
}
