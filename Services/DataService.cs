using System.IO;
using System.Text.Json;
using TrackMaintenance.Models;

namespace TrackMaintenance.Services;

public static class DataService
{
    private static readonly string DataFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "TrackMaintenancePro");

    private static readonly string DataFile = Path.Combine(DataFolder, "data.json");

    private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = true };

    public static AppData Data { get; private set; } = new();

    // ── Persistence ────────────────────────────────────────────────────────────

    public static void Load()
    {
        try
        {
            if (!File.Exists(DataFile)) { Data = new AppData(); return; }
            var json = File.ReadAllText(DataFile);
            Data = JsonSerializer.Deserialize<AppData>(json) ?? new AppData();
        }
        catch { Data = new AppData(); }
    }

    public static void Save()
    {
        Directory.CreateDirectory(DataFolder);
        File.WriteAllText(DataFile, JsonSerializer.Serialize(Data, JsonOpts));
    }

    // ── Calculations ───────────────────────────────────────────────────────────

    /// <summary>
    /// Computes equivalent km used since last service for a given maintenance item.
    /// equivUsed = (currentOdometer - lastServiceOdometer) + sum(sessions * additionPerSession)
    ///             for all track days after the last service date.
    /// </summary>
    public static MaintenanceStatus GetStatus(MaintenanceItem item, Vehicle vehicle)
    {
        var lastDate = item.LastServiceDate ?? DateTime.MinValue;
        var lastOdo  = item.LastServiceMileage;

        var actualDelta = Math.Max(0, vehicle.CurrentMileage - lastOdo);

        var trackEquiv = Data.TrackDays
            .Where(t => t.VehicleId == vehicle.Id && t.Date > lastDate)
            .Sum(t => t.Sessions * item.TrackAdditionPerSession);

        return new MaintenanceStatus
        {
            Item        = item,
            EquivKmUsed = actualDelta + trackEquiv
        };
    }

    /// <summary>Get all maintenance statuses for a vehicle, sorted by % used descending.</summary>
    public static List<MaintenanceStatus> GetAllStatuses(Vehicle vehicle) =>
        Data.MaintenanceItems
            .Where(i => i.VehicleId == vehicle.Id && i.IsEnabled)
            .Select(i => GetStatus(i, vehicle))
            .OrderByDescending(s => s.PercentUsed)
            .ToList();

    // ── Convenience ────────────────────────────────────────────────────────────

    public static Vehicle? GetVehicle(string id) =>
        Data.Vehicles.FirstOrDefault(v => v.Id == id);

    public static List<TrackDay> GetTrackDays(string vehicleId) =>
        Data.TrackDays
            .Where(t => t.VehicleId == vehicleId)
            .OrderByDescending(t => t.Date)
            .ToList();

    public static List<MaintenanceRecord> GetMaintenanceRecords(string vehicleId) =>
        Data.MaintenanceRecords
            .Where(r => r.VehicleId == vehicleId)
            .OrderByDescending(r => r.Date)
            .ToList();

    /// <summary>Add a vehicle and create its default maintenance items.</summary>
    public static void AddVehicle(Vehicle vehicle)
    {
        Data.Vehicles.Add(vehicle);
        Data.MaintenanceItems.AddRange(DefaultItems.CreateForVehicle(vehicle.Id));
        Save();
    }

    public static void UpdateVehicle(Vehicle vehicle) { Save(); }
    public static void DeleteVehicle(string vehicleId)
    {
        Data.Vehicles.RemoveAll(v => v.Id == vehicleId);
        Data.TrackDays.RemoveAll(t => t.VehicleId == vehicleId);
        Data.MaintenanceItems.RemoveAll(i => i.VehicleId == vehicleId);
        Data.MaintenanceRecords.RemoveAll(r => r.VehicleId == vehicleId);
        Save();
    }

    /// <summary>Add a track day and update vehicle odometer.</summary>
    public static void AddTrackDay(TrackDay day)
    {
        Data.TrackDays.Add(day);
        var v = GetVehicle(day.VehicleId);
        {
        }
        Save();
    }

    public static void DeleteTrackDay(string id)
    {
        Data.TrackDays.RemoveAll(t => t.Id == id);
        Save();
    }

    /// <summary>Record a maintenance event: reset the item's last-service info.</summary>
    public static void RecordMaintenance(MaintenanceItem item, Vehicle vehicle,
                                         DateTime date, double odometer, string notes)
    {
        // Update the item
        item.LastServiceDate    = date;
        item.LastServiceMileage = odometer;
        item.LastServiceNotes   = notes;

        // Update vehicle odometer if larger
        if (odometer > vehicle.CurrentMileage) vehicle.CurrentMileage = odometer;

        // Save record
        Data.MaintenanceRecords.Add(new MaintenanceRecord
        {
            VehicleId         = vehicle.Id,
            MaintenanceItemId = item.Id,
            ComponentName     = item.ComponentName,
            Date              = date,
            OdometerAtService = odometer,
            Notes             = notes
        });

        Save();
    }
}
