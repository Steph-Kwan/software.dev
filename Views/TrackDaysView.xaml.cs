using System.Windows;
using System.Windows.Controls;
using TrackMaintenance.Dialogs;
using TrackMaintenance.Models;
using TrackMaintenance.Services;

namespace TrackMaintenance.Views;

public partial class TrackDaysView : UserControl
{
    private readonly Vehicle    _vehicle;
    private readonly MainWindow _main;

    public TrackDaysView(Vehicle vehicle, MainWindow main)
    {
        InitializeComponent();
        _vehicle = vehicle;
        _main    = main;
        Loaded  += (_, _) => Refresh();
    }

    public void Refresh()
    {
        var days = DataService.GetTrackDays(_vehicle.Id);
        SubtitleText.Text      = _vehicle.DisplayName;
        TotalSessionsText.Text = days.Sum(d => d.Sessions).ToString();
        TotalDaysText.Text     = days.Count.ToString();

        if (days.Count == 0)
        {
            EmptyState.Visibility = Visibility.Visible;
            TrackGrid.Visibility  = Visibility.Hidden;
        }
        else
        {
            EmptyState.Visibility   = Visibility.Collapsed;
            TrackGrid.Visibility    = Visibility.Visible;
            TrackGrid.ItemsSource   = null;
            TrackGrid.ItemsSource   = days;
        }
    }

    private void AddBtn_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new TrackDayDialog(_vehicle) { Owner = _main };
        if (dlg.ShowDialog() == true) Refresh();
    }

    private void DeleteBtn_Click(object sender, RoutedEventArgs e)
    {
        var day = (TrackDay)((Button)sender).Tag;
        var r   = MessageBox.Show($"确认删除 {day.DateDisplay} 的赛道记录？",
            "确认删除", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (r == MessageBoxResult.Yes) { DataService.DeleteTrackDay(day.Id); Refresh(); }
    }
}
