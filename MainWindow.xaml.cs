using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TrackMaintenance.Models;
using TrackMaintenance.Services;
using TrackMaintenance.Views;

namespace TrackMaintenance;

public partial class MainWindow : Window
{
    private bool _suppressNav = false;

    public Vehicle? CurrentVehicle => VehicleComboBox.SelectedItem as Vehicle;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += (_, _) => Init();
    }

    private void Init()
    {
        // Apply saved background
        try
        {
            var color = (Color)ColorConverter.ConvertFromString(SettingsService.Current.BackgroundColor);
            var brush  = new SolidColorBrush(color);
            Application.Current.Resources["ContentBg"] = brush;
            Background = brush;
        }
        catch { }

        // Apply saved language
        L.IsChinese = SettingsService.Current.IsChineseLanguage;
        ApplyLanguage();

        _suppressNav = true;
        VehicleComboBox.ItemsSource = DataService.Data.Vehicles;

        if (DataService.Data.Vehicles.Count > 0)
        {
            VehicleComboBox.SelectedIndex = 0;
            OdoBorder.Visibility = Visibility.Visible;
            TrackBtn.IsEnabled   = true;
            TabDash.IsChecked    = true;
            RefreshOdo();
        }
        else
        {
            OdoBorder.Visibility  = Visibility.Collapsed;
            TrackBtn.IsEnabled    = false;
            TabVehicles.IsChecked = true;
        }

        _suppressNav = false;
        ShowPage(GetCurrentTab());
    }

    private void ApplyLanguage()
    {
        AppNameText.Text = L.AppName;
        OdoLabel.Text    = L.Mileage;
        TrackBtn.Content = L.TabTrack;
        FooterAppName.Text = L.IsChinese ? "赛道保养管家  v1.0" : "Track Maintenance Pro  v1.0";

        TabDash.Content     = L.TabDashboard;
        TabHistory.Content  = L.TabHistory;
        TabTrack.Content    = L.TabTrack;
        TabVehicles.Content = L.TabVehicles;
        TabSettings.Content = L.TabSettings;
    }

    // ── Vehicle refresh ───────────────────────────────────────────────────────

    public void RefreshVehicleList(string? selectId = null)
    {
        _suppressNav = true;
        VehicleComboBox.ItemsSource = null;
        VehicleComboBox.ItemsSource = DataService.Data.Vehicles;

        if (DataService.Data.Vehicles.Count > 0)
        {
            var target = selectId != null
                ? DataService.Data.Vehicles.FirstOrDefault(v => v.Id == selectId)
                : DataService.Data.Vehicles[0];
            VehicleComboBox.SelectedItem = target ?? DataService.Data.Vehicles[0];
            OdoBorder.Visibility = Visibility.Visible;
            TrackBtn.IsEnabled   = true;
        }
        else
        {
            OdoBorder.Visibility = Visibility.Collapsed;
            TrackBtn.IsEnabled   = false;
        }

        _suppressNav = false;
        RefreshOdo();
        ShowPage(GetCurrentTab());
    }

    public void RefreshOdo()
    {
        OdoText.Text = CurrentVehicle != null ? $"{CurrentVehicle.CurrentMileage:N0}" : "—";
    }

    private void VehicleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_suppressNav) return;
        RefreshOdo();
        ShowPage(GetCurrentTab());
    }

    // ── Odometer edit ─────────────────────────────────────────────────────────

    private void OdoBorder_Click(object sender, MouseButtonEventArgs e)
    {
        if (CurrentVehicle == null) return;
        OdoBorder.Visibility     = Visibility.Collapsed;
        OdoEditBorder.Visibility = Visibility.Visible;
        OdoEditBox.Text          = CurrentVehicle.CurrentMileage.ToString("N0");
        OdoEditBox.SelectAll();
        OdoEditBox.Focus();
    }

    private void OdoEditBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            var raw = OdoEditBox.Text.Replace(",", "").Trim();
            if (double.TryParse(raw, out var km) && km >= 0 && CurrentVehicle != null)
            {
                CurrentVehicle.CurrentMileage = km;
                DataService.Save();
                RefreshOdo();
                if (MainContent.Content is DashboardView dv) dv.Refresh();
            }
            OdoEditBorder.Visibility = Visibility.Collapsed;
            OdoBorder.Visibility     = Visibility.Visible;
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            OdoEditBorder.Visibility = Visibility.Collapsed;
            OdoBorder.Visibility     = Visibility.Visible;
        }
    }

    private void OdoEditBox_LostFocus(object sender, RoutedEventArgs e)
    {
        OdoEditBorder.Visibility = Visibility.Collapsed;
        OdoBorder.Visibility     = Visibility.Visible;
    }

    // ── Navigation ────────────────────────────────────────────────────────────

    private string GetCurrentTab()
    {
        if (TabHistory.IsChecked  == true) return "history";
        if (TabTrack.IsChecked    == true) return "track";
        if (TabVehicles.IsChecked == true) return "vehicles";
        if (TabSettings.IsChecked == true) return "settings";
        return "dash";
    }

    private void ShowPage(string page)
    {
        MainContent.Content = null;

        bool needsVehicle = page != "vehicles" && page != "settings";
        if (needsVehicle && CurrentVehicle == null)
        {
            MainContent.Content = MakeNoVehiclePrompt();
            return;
        }

        MainContent.Content = page switch
        {
            "history"  => new MaintenanceLogView(CurrentVehicle!),
            "track"    => new TrackDaysView(CurrentVehicle!, this),
            "vehicles" => new VehiclesView(this),
            "settings" => new SettingsView(this),
            _          => new DashboardView(CurrentVehicle!, this),
        };
    }

    private UIElement MakeNoVehiclePrompt()
    {
        var sp = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment   = VerticalAlignment.Center
        };
        sp.Children.Add(new TextBlock
        {
            Text = L.NoVehicleMsg, FontSize = 18, FontWeight = FontWeights.Bold,
            Foreground = Brushes.Gray, HorizontalAlignment = HorizontalAlignment.Center
        });
        var btn = new Button
        {
            Content = L.TabVehicles, Style = (Style)Application.Current.Resources["PrimaryBtn"],
            Height = 40, Margin = new Thickness(0, 20, 0, 0)
        };
        btn.Click += (_, _) => { _suppressNav = true; TabVehicles.IsChecked = true; _suppressNav = false; ShowPage("vehicles"); };
        sp.Children.Add(btn);
        return sp;
    }

    private void TabDash_Checked(object sender, RoutedEventArgs e)     { if (_suppressNav || !IsLoaded) return; ShowPage("dash"); }
    private void TabHistory_Checked(object sender, RoutedEventArgs e)  { if (_suppressNav || !IsLoaded) return; ShowPage("history"); }
    private void TabTrack_Checked(object sender, RoutedEventArgs e)    { if (_suppressNav || !IsLoaded) return; ShowPage("track"); }
    private void TabVehicles_Checked(object sender, RoutedEventArgs e) { if (_suppressNav || !IsLoaded) return; ShowPage("vehicles"); }
    private void TabSettings_Checked(object sender, RoutedEventArgs e) { if (_suppressNav || !IsLoaded) return; ShowPage("settings"); }

    private void TrackBtn_Click(object sender, RoutedEventArgs e)
    {
        _suppressNav = true; TabTrack.IsChecked = true; _suppressNav = false;
        ShowPage("track");
    }
}
