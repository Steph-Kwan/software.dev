using System.Windows;
using System.Windows.Controls;
using TrackMaintenance.Models;
using TrackMaintenance.Services;

namespace TrackMaintenance.Dialogs;

public partial class TrackDayDialog : Window
{
    private readonly Vehicle _vehicle;

    public TrackDayDialog(Vehicle vehicle)
    {
        InitializeComponent();
        _vehicle = vehicle;
        VehicleName.Text = vehicle.DisplayName;
        SessionsCombo.SelectionChanged += (_, _) => UpdatePreview();
        UpdatePreview();
    }

    private void UpdatePreview()
    {
        int sessions = SessionsCombo.SelectedIndex + 1;
        var items = DataService.Data.MaintenanceItems
            .Where(i => i.VehicleId == _vehicle.Id && i.IsEnabled).ToList();

        if (items.Count == 0) { PreviewText.Text = "未配置保养项目"; return; }

        var top = items.OrderByDescending(i => i.TrackAdditionPerSession).Take(3)
                       .Select(i => $"{i.ComponentName} +{i.TrackAdditionPerSession * sessions:N0}km");
        PreviewText.Text = $"本次 {sessions} 节：" + string.Join("  |  ", top)
                         + (items.Count > 3 ? $"  等 {items.Count} 项" : "");
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (DatePicker.SelectedDate == null) { MessageBox.Show("请选择日期"); return; }

        DataService.AddTrackDay(new TrackDay
        {
            VehicleId = _vehicle.Id,
            Date      = DatePicker.SelectedDate.Value,
            Sessions  = SessionsCombo.SelectedIndex + 1,
            Track     = NotesBox.Text.Trim(),
        });
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
