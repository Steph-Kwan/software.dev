using System.Windows;
using TrackMaintenance.Models;
using TrackMaintenance.Services;

namespace TrackMaintenance.Dialogs;

public partial class VehicleDialog : Window
{
    private readonly Vehicle? _existing;
    public Vehicle? Result { get; private set; }

    // Add mode
    public VehicleDialog()
    {
        InitializeComponent();
        DialogTitle.Text = "添加车辆";
    }

    // Edit mode
    public VehicleDialog(Vehicle vehicle) : this()
    {
        _existing = vehicle;
        DialogTitle.Text  = "编辑车辆信息";
        NameBox.Text      = vehicle.Name;
        YearBox.Text      = vehicle.Year;
        PlateBox.Text     = vehicle.LicensePlate;
        MileageBox.Text   = vehicle.CurrentMileage.ToString("N0");
        NotesBox.Text     = vehicle.Notes;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text))
        {
            MessageBox.Show("请输入车辆名称。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            NameBox.Focus();
            return;
        }

        if (!double.TryParse(MileageBox.Text.Replace(",", ""), out var mileage) || mileage < 0)
        {
            MessageBox.Show("请输入有效的里程数。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            MileageBox.Focus();
            return;
        }

        if (_existing != null)
        {
            // Edit mode
            _existing.Name           = NameBox.Text.Trim();
            _existing.Year           = YearBox.Text.Trim();
            _existing.LicensePlate   = PlateBox.Text.Trim();
            _existing.CurrentMileage = mileage;
            _existing.Notes          = NotesBox.Text.Trim();
            Result = _existing;
        }
        else
        {
            // Add mode
            Result = new Vehicle
            {
                Name           = NameBox.Text.Trim(),
                Year           = YearBox.Text.Trim(),
                LicensePlate   = PlateBox.Text.Trim(),
                CurrentMileage = mileage,
                Notes          = NotesBox.Text.Trim()
            };
            DataService.AddVehicle(Result);
        }

        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
