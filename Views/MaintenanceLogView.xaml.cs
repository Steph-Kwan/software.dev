using System.Windows;
using System.Windows.Controls;
using TrackMaintenance.Models;
using TrackMaintenance.Services;

namespace TrackMaintenance.Views;

public partial class MaintenanceLogView : UserControl
{
    private readonly Vehicle _vehicle;

    public MaintenanceLogView(Vehicle vehicle)
    {
        InitializeComponent();
        _vehicle = vehicle;
        Loaded  += (_, _) => Refresh();
    }

    public void Refresh()
    {
        var records = DataService.GetMaintenanceRecords(_vehicle.Id);
        CountText.Text = $"共 {records.Count} 条记录";

        if (records.Count == 0)
        {
            EmptyState.Visibility  = Visibility.Visible;
            RecordsGrid.Visibility = Visibility.Hidden;
        }
        else
        {
            EmptyState.Visibility  = Visibility.Collapsed;
            RecordsGrid.Visibility = Visibility.Visible;
            RecordsGrid.ItemsSource = null;
            RecordsGrid.ItemsSource = records;
        }
    }
}
