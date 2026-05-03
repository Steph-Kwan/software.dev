using System.Windows;
using TrackMaintenance.Models;
using TrackMaintenance.Services;

namespace TrackMaintenance.Dialogs;

public partial class EditItemDialog : Window
{
    private readonly MaintenanceItem? _item;
    private readonly string _vehicleId;
    private readonly bool _isNew;

    // Edit existing item
    public EditItemDialog(MaintenanceItem? item, string vehicleId)
    {
        InitializeComponent();
        _vehicleId = vehicleId;

        if (item == null)
        {
            // Add mode
            _isNew = true;
            _item  = null;
            Title  = "添加保养项目";
            ItemLabel.Text   = "新建项目";
            IntervalBox.Text = "10000";
            AdditionBox.Text = "1000";
            EnabledCheck.IsChecked = true;
            // Show name fields
            NamePanel.Visibility = Visibility.Visible;
        }
        else
        {
            // Edit mode
            _isNew = false;
            _item  = item;
            Title  = "编辑保养项目";
            ItemLabel.Text   = $"{item.ComponentName}  ·  {item.ComponentSubtitle}";
            NameBox.Text     = item.ComponentName;
            SubtitleBox.Text = item.ComponentSubtitle;
            IntervalBox.Text = item.IntervalKm.ToString("N0");
            AdditionBox.Text = item.TrackAdditionPerSession.ToString("N0");
            EnabledCheck.IsChecked = item.IsEnabled;
            NamePanel.Visibility = Visibility.Visible;
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var name = NameBox.Text.Trim();
        if (string.IsNullOrEmpty(name)) { MessageBox.Show("请输入项目名称"); return; }

        if (!double.TryParse(IntervalBox.Text.Replace(",", ""), out var interval) || interval <= 0)
        { MessageBox.Show("请输入有效的保养周期"); return; }

        if (!double.TryParse(AdditionBox.Text.Replace(",", ""), out var addition) || addition < 0)
        { MessageBox.Show("请输入有效的赛道折算里程"); return; }

        if (_isNew)
        {
            DataService.Data.MaintenanceItems.Add(new MaintenanceItem
            {
                VehicleId               = _vehicleId,
                ComponentName           = name,
                ComponentSubtitle       = SubtitleBox.Text.Trim(),
                IntervalKm              = interval,
                TrackAdditionPerSession = addition,
                IsEnabled               = EnabledCheck.IsChecked ?? true
            });
        }
        else if (_item != null)
        {
            _item.ComponentName           = name;
            _item.ComponentSubtitle       = SubtitleBox.Text.Trim();
            _item.IntervalKm              = interval;
            _item.TrackAdditionPerSession = addition;
            _item.IsEnabled               = EnabledCheck.IsChecked ?? true;
        }

        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
