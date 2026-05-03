using TrackMaintenance.Services;

namespace TrackMaintenance;

public partial class App : System.Windows.Application
{
    protected override void OnStartup(System.Windows.StartupEventArgs e)
    {
        base.OnStartup(e);
        SettingsService.Load();
        DataService.Load();
    }

    protected override void OnExit(System.Windows.ExitEventArgs e)
    {
        DataService.Save();
        SettingsService.Save();
        base.OnExit(e);
    }
}
