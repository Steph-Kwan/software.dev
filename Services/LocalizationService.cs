using System.IO;
using System.Text.Json;

namespace TrackMaintenance.Services;

public static class L
{
    private static bool _isChinese = true;

    public static bool IsChinese
    {
        get => _isChinese;
        set { _isChinese = value; LanguageChanged?.Invoke(); }
    }

    public static event Action? LanguageChanged;

    // ── Strings ───────────────────────────────────────────────────────────────
    public static string AppName        => _isChinese ? "赛道保养管家"        : "Track Maintenance Pro";
    public static string AppSub         => _isChinese ? "Track Maintenance Pro" : "Track Maintenance Pro";

    // Tabs
    public static string TabDashboard   => _isChinese ? "仪表盘"   : "Dashboard";
    public static string TabHistory     => _isChinese ? "保养记录" : "Maintenance Log";
    public static string TabTrack       => _isChinese ? "赛道日志" : "Track Days";
    public static string TabVehicles    => _isChinese ? "车辆管理" : "Vehicles";
    public static string TabSettings    => _isChinese ? "系统设置" : "Settings";

    // Dashboard
    public static string Mileage        => _isChinese ? "里程："     : "ODO:";
    public static string EditMileageTip => _isChinese ? "点击编辑里程" : "Click to edit mileage";
    public static string KmUnit         => _isChinese ? "km  ✎"      : "km  ✎";
    public static string NoVehicleMsg   => _isChinese ? "请先前往「车辆管理」添加车辆" : "Please add a vehicle in Vehicles tab";
    public static string StatusGood     => _isChinese ? "状态良好"   : "Good";
    public static string StatusWarn     => _isChinese ? "即将到期"   : "Due Soon";
    public static string StatusOverdue  => _isChinese ? "已超期"     : "Overdue";
    public static string EquivUsed      => _isChinese ? "等效里程"   : "Equiv. Used";
    public static string TillService    => _isChinese ? "距保养"     : "Until Service";
    public static string LastService    => _isChinese ? "上次保养："  : "Last Service:";
    public static string TrackAdd       => _isChinese ? "赛道折算："  : "Track Add:";
    public static string PerSession     => _isChinese ? " km/节"     : " km/session";
    public static string RecordService  => _isChinese ? "√  记录保养完成" : "√  Record Service";
    public static string AlertOverdue   => _isChinese ? "！  有 {0} 个项目已超期，请立即安排保养！" : "！  {0} item(s) overdue – please service immediately!";
    public static string AlertWarn      => _isChinese ? "！  有 {0} 个项目即将到期，请提前安排保养。" : "！  {0} item(s) due soon – please schedule service.";
    public static string NoItems        => _isChinese ? "暂无保养项目" : "No maintenance items";
    public static string NoItemsSub     => _isChinese ? "前往「车辆管理」为该车辆添加保养项目" : "Go to Vehicles to add items for this vehicle";

    // Track days
    public static string TrackDays      => _isChinese ? "赛道日志"    : "Track Days";
    public static string AddTrackDay    => _isChinese ? "+ 记录赛道日" : "+ Add Track Day";
    public static string TotalSessions  => _isChinese ? "总节数"      : "Total Sessions";
    public static string TotalDays      => _isChinese ? "赛道天数"    : "Track Days";
    public static string ColDate        => _isChinese ? "日期"        : "Date";
    public static string ColSessions    => _isChinese ? "节数"        : "Sessions";
    public static string ColNotes       => _isChinese ? "备注"        : "Notes";
    public static string ColActions     => _isChinese ? "操作"        : "Actions";
    public static string Delete         => _isChinese ? "删除"        : "Delete";
    public static string NoTrackDays    => _isChinese ? "暂无赛道记录" : "No track days recorded";
    public static string NoTrackDaysSub => _isChinese ? "点击「记录赛道日」开始记录" : "Click 'Add Track Day' to start";

    // Vehicles
    public static string Vehicles       => _isChinese ? "车辆管理"    : "Vehicle Management";
    public static string AddVehicle     => _isChinese ? "+ 添加车辆"  : "+ Add Vehicle";
    public static string NoVehicles     => _isChinese ? "暂无车辆"    : "No vehicles";
    public static string NoVehiclesSub  => _isChinese ? "点击右上角按钮添加你的爱车" : "Click the button above to add a vehicle";
    public static string MaintenanceItems => _isChinese ? "保养项目"  : "Maintenance Items";
    public static string AddItem        => _isChinese ? "+ 添加项目"  : "+ Add Item";
    public static string EditVehicle    => _isChinese ? "编辑"        : "Edit";
    public static string DeleteVehicle  => _isChinese ? "删除车辆"    : "Delete Vehicle";

    // Maintenance log
    public static string MaintenanceLog => _isChinese ? "保养记录"    : "Maintenance Log";
    public static string Records        => _isChinese ? "条记录"      : "record(s)";
    public static string ColComponent   => _isChinese ? "保养项目"    : "Component";
    public static string ColMileage     => _isChinese ? "里程 (km)"   : "Mileage (km)";
    public static string NoRecords      => _isChinese ? "暂无保养记录" : "No maintenance records";
    public static string NoRecordsSub   => _isChinese ? "在仪表盘点击「记录保养完成」即可记录" : "Click 'Record Service' on dashboard cards";

    // Settings
    public static string Settings       => _isChinese ? "系统设置"    : "Settings";
    public static string ThemeSection   => _isChinese ? "外观主题"    : "Appearance";
    public static string BgColor        => _isChinese ? "背景颜色"    : "Background Color";
    public static string Language       => _isChinese ? "语言"        : "Language";
    public static string Chinese        => _isChinese ? "中文"        : "Chinese";
    public static string English        => _isChinese ? "英文"        : "English";
    public static string PreviewNote    => _isChinese ? "更改语言后请重启软件以完全生效" : "Please restart the app after changing language";

    // Copyright
    public static string Copyright => "© Stephen Kwan  |  kwan.stephen@outlook.com";

    // Dialogs
    public static string Save           => _isChinese ? "保存"     : "Save";
    public static string Cancel         => _isChinese ? "取消"     : "Cancel";
    public static string Confirm        => _isChinese ? "确认保存" : "Confirm";
    public static string ConfirmDelete  => _isChinese ? "确认删除" : "Confirm Delete";
}
