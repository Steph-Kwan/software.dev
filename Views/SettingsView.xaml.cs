using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TrackMaintenance.Services;

namespace TrackMaintenance.Views;

public partial class SettingsView : UserControl
{
    private readonly MainWindow _main;

    // Preset background colors
    private static readonly (string Label, string Hex)[] Presets =
    [
        ("浅蓝灰",  "#EEF2F7"),
        ("纯白",    "#FFFFFF"),
        ("浅灰",    "#F3F4F6"),
        ("暖白",    "#FAF9F6"),
        ("浅绿",    "#F0FDF4"),
        ("浅蓝",    "#EFF6FF"),
        ("深灰",    "#1F2937"),
        ("深蓝",    "#0F1F2D"),
    ];

    public SettingsView(MainWindow main)
    {
        InitializeComponent();
        _main = main;
        Loaded += (_, _) => Init();
    }

    private void Init()
    {
        // Language radio
        ChineseRadio.IsChecked = L.IsChinese;
        EnglishRadio.IsChecked = !L.IsChinese;

        // Current color
        CustomColorBox.Text = SettingsService.Current.BackgroundColor;

        // Build color swatches
        foreach (var (label, hex) in Presets)
        {
            var swatch = BuildSwatch(label, hex);
            ColorPanel.Children.Add(swatch);
        }
    }

    private Border BuildSwatch(string label, string hex)
    {
        bool isSelected = hex.Equals(
            SettingsService.Current.BackgroundColor,
            StringComparison.OrdinalIgnoreCase);

        Color c;
        try { c = (Color)ColorConverter.ConvertFromString(hex); }
        catch { c = Colors.White; }

        // Determine text color based on brightness
        double brightness = (c.R * 0.299 + c.G * 0.587 + c.B * 0.114) / 255;
        var textBrush = brightness > 0.5 ? Brushes.Black : Brushes.White;

        var inner = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
        inner.Children.Add(new TextBlock
        {
            Text = label, FontSize = 11, FontWeight = FontWeights.SemiBold,
            Foreground = textBrush, HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 2)
        });
        inner.Children.Add(new TextBlock
        {
            Text = hex, FontSize = 9, Foreground = textBrush,
            HorizontalAlignment = HorizontalAlignment.Center
        });

        var swatch = new Border
        {
            Width = 90, Height = 64,
            Background = new SolidColorBrush(c),
            CornerRadius = new CornerRadius(8),
            Margin = new Thickness(0, 0, 10, 10),
            Cursor = System.Windows.Input.Cursors.Hand,
            BorderBrush = isSelected
                ? new SolidColorBrush(Color.FromRgb(31, 78, 120))
                : new SolidColorBrush(Color.FromRgb(209, 217, 224)),
            BorderThickness = isSelected ? new Thickness(3) : new Thickness(1),
            Child = inner,
            Tag = hex
        };
        swatch.MouseLeftButtonUp += (s, _) =>
        {
            var h = (string)((Border)s).Tag;
            ApplyBackground(h);
        };
        return swatch;
    }

    private void ApplyBackground(string hex)
    {
        try
        {
            var color = (Color)ColorConverter.ConvertFromString(hex);
            var brush  = new SolidColorBrush(color);

            // Apply to app resources and window
            Application.Current.Resources["ContentBg"] = brush;
            _main.Background = brush;

            SettingsService.Current.BackgroundColor = hex;
            SettingsService.Save();
            CustomColorBox.Text = hex;

            // Refresh swatches to show new selection
            ColorPanel.Children.Clear();
            foreach (var (label, h) in Presets)
                ColorPanel.Children.Add(BuildSwatch(label, h));
        }
        catch
        {
            MessageBox.Show(L.IsChinese ? "无效的颜色值，请输入如 #EEF2F7 格式" : "Invalid color. Use format like #EEF2F7");
        }
    }

    private void ApplyColor_Click(object sender, RoutedEventArgs e)
    {
        ApplyBackground(CustomColorBox.Text.Trim());
    }

    private void ChineseRadio_Checked(object sender, RoutedEventArgs e)
    {
        if (!IsLoaded) return;
        L.IsChinese = true;
        SettingsService.Current.IsChineseLanguage = true;
        SettingsService.Save();
    }

    private void EnglishRadio_Checked(object sender, RoutedEventArgs e)
    {
        if (!IsLoaded) return;
        L.IsChinese = false;
        SettingsService.Current.IsChineseLanguage = false;
        SettingsService.Save();
    }
}
