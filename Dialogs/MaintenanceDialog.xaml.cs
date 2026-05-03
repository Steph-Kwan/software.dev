using System.Windows;
using System.Windows.Media;
using TrackMaintenance.Models;
using TrackMaintenance.Services;

namespace TrackMaintenance.Dialogs;

public partial class MaintenanceDialog : Window
{
    private readonly Vehicle         _vehicle;
    private readonly MaintenanceItem _item;

    public MaintenanceDialog(Vehicle vehicle, MaintenanceItem item)
    {
        InitializeComponent();
        _vehicle = vehicle;
        _item    = item;

        ComponentLabel.Text = $"{item.ComponentName}  ·  {vehicle.DisplayName}";
        OdometerBox.Text    = vehicle.CurrentMileage.ToString("N0");

        var status = DataService.GetStatus(item, vehicle);

        // Color the status card by level
        var (bgColor, fgColor, statusMsg) = status.Level switch
        {
            StatusLevel.Overdue => (Color.FromRgb(254,226,226), Color.FromRgb(153,27,27),
                $"已超期  {status.EquivKmUsed:N0} km / {item.IntervalKm:N0} km  ({status.PercentUsed:F0}%)"),
            StatusLevel.Warning => (Color.FromRgb(254,243,199), Color.FromRgb(146,64,14),
                $"即将到期  {status.EquivKmUsed:N0} km / {item.IntervalKm:N0} km  ({status.PercentUsed:F0}%)"),
            _                   => (Color.FromRgb(220,252,231), Color.FromRgb(21,128,61),
                $"状态良好  {status.EquivKmUsed:N0} km / {item.IntervalKm:N0} km  ({status.PercentUsed:F0}%)")
        };

        StatusCard.Background  = new SolidColorBrush(bgColor);
        StatusText.Foreground  = new SolidColorBrush(fgColor);
        StatusText.Text        = statusMsg;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (DatePicker.SelectedDate == null) { MessageBox.Show("请选择保养日期"); return; }
        if (!double.TryParse(OdometerBox.Text.Replace(",", ""), out var odo) || odo < 0)
        { MessageBox.Show("请输入有效的里程数"); return; }

        DataService.RecordMaintenance(_item, _vehicle, DatePicker.SelectedDate.Value, odo, NotesBox.Text.Trim());
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
