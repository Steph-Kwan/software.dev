using System.Text.Json.Serialization;

namespace TrackMaintenance.Models;

public class Vehicle
{
    public string Id             { get; set; } = Guid.NewGuid().ToString();
    public string Name           { get; set; } = "";
    public string Year           { get; set; } = "";
    public string LicensePlate   { get; set; } = "";
    public double CurrentMileage { get; set; } = 0;
    public string Notes          { get; set; } = "";
    [JsonIgnore] public string DisplayName => Year.Length > 0 ? $"{Year} {Name}" : Name;
}

public class TrackDay
{
    public string   Id            { get; set; } = Guid.NewGuid().ToString();
    public string   VehicleId     { get; set; } = "";
    public DateTime Date          { get; set; } = DateTime.Today;
    public string   Track         { get; set; } = "";
    public int      Sessions      { get; set; } = 1;
    [JsonIgnore] public string DateDisplay => Date.ToString("yyyy-MM-dd");
}

public class MaintenanceItem
{
    public string   Id                      { get; set; } = Guid.NewGuid().ToString();
    public string   VehicleId               { get; set; } = "";
    public string   ComponentName           { get; set; } = "";
    public string   ComponentSubtitle       { get; set; } = "";
    public double   IntervalKm              { get; set; } = 5000;
    public double   TrackAdditionPerSession { get; set; } = 1000;
    public bool     IsCritical              { get; set; } = false;
    public bool     IsEnabled               { get; set; } = true;
    public DateTime? LastServiceDate        { get; set; } = null;
    public double   LastServiceMileage      { get; set; } = 0;
    public string   LastServiceNotes        { get; set; } = "";
}

public class MaintenanceRecord
{
    public string   Id                { get; set; } = Guid.NewGuid().ToString();
    public string   VehicleId         { get; set; } = "";
    public string   MaintenanceItemId { get; set; } = "";
    public string   ComponentName     { get; set; } = "";
    public DateTime Date              { get; set; } = DateTime.Today;
    public double   OdometerAtService { get; set; } = 0;
    public string   Notes             { get; set; } = "";
    [JsonIgnore] public string DateDisplay => Date.ToString("yyyy-MM-dd");
}

public class AppData
{
    public List<Vehicle>          Vehicles          { get; set; } = [];
    public List<TrackDay>         TrackDays         { get; set; } = [];
    public List<MaintenanceItem>  MaintenanceItems  { get; set; } = [];
    public List<MaintenanceRecord> MaintenanceRecords { get; set; } = [];
}

// ── Status: 3 levels only ──────────────────────────────────────────────────────
public enum StatusLevel { Good, Warning, Overdue }

public class MaintenanceStatus
{
    public MaintenanceItem Item        { get; set; } = null!;
    public double          EquivKmUsed { get; set; }

    public double PercentUsed  => Item.IntervalKm > 0 ? EquivKmUsed / Item.IntervalKm * 100.0 : 0;
    public double PercentCapped => Math.Min(100, PercentUsed);
    public double RemainingKm  => Item.IntervalKm - EquivKmUsed;

    // 0–80% Good | 80–99% Warning | 100%+ Overdue
    public StatusLevel Level =>
        PercentUsed >= 100 ? StatusLevel.Overdue  :
        PercentUsed >= 80  ? StatusLevel.Warning  :
        StatusLevel.Good;

    public string StatusText => Level switch
    {
        StatusLevel.Overdue  => "已超期",
        StatusLevel.Warning  => "即将到期",
        _                    => "状态良好"
    };

    public string StatusColor => Level switch
    {
        StatusLevel.Overdue  => "#B91C1C",
        StatusLevel.Warning  => "#C2410C",
        _                    => "#15803D"
    };

    public string RemainingDisplay =>
        RemainingKm < 0 ? $"超期 {-RemainingKm:N0} km" : $"余 {RemainingKm:N0} km";

    public string LastServiceDisplay =>
        Item.LastServiceDate.HasValue
            ? Item.LastServiceDate.Value.ToString("yyyy-MM-dd")
            : "未记录";
}

public static class DefaultItems
{
    public static List<MaintenanceItem> CreateForVehicle(string vehicleId) =>
    [
        new() { VehicleId=vehicleId, ComponentName="机油",       ComponentSubtitle="Engine Oil",               IntervalKm=5000,  TrackAdditionPerSession=1000 },
        new() { VehicleId=vehicleId, ComponentName="机油滤芯",   ComponentSubtitle="Oil Filter",               IntervalKm=5000,  TrackAdditionPerSession=1000 },
        new() { VehicleId=vehicleId, ComponentName="火花塞",     ComponentSubtitle="Spark Plugs",              IntervalKm=80000, TrackAdditionPerSession=2500 },
        new() { VehicleId=vehicleId, ComponentName="刹车油",     ComponentSubtitle="Brake Fluid",              IntervalKm=40000, TrackAdditionPerSession=7500, IsCritical=true },
        new() { VehicleId=vehicleId, ComponentName="离合油",     ComponentSubtitle="Clutch Fluid",             IntervalKm=40000, TrackAdditionPerSession=1500 },
        new() { VehicleId=vehicleId, ComponentName="手动波箱油", ComponentSubtitle="Manual Transmission Fluid",IntervalKm=70000, TrackAdditionPerSession=2500 },
        new() { VehicleId=vehicleId, ComponentName="差速器油",   ComponentSubtitle="Differential Oil",         IntervalKm=60000, TrackAdditionPerSession=2500 },
        new() { VehicleId=vehicleId, ComponentName="冷却液",     ComponentSubtitle="Coolant",                  IntervalKm=60000, TrackAdditionPerSession=750  },
        new() { VehicleId=vehicleId, ComponentName="空气滤清器", ComponentSubtitle="Air Filter",               IntervalKm=20000, TrackAdditionPerSession=1500 },
    ];
}
