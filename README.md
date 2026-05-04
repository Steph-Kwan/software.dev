# 开发者信息
- Stephen Kwan 2026
- kwan.stephen@outlook.com

# 赛道保养管家 Track Maintenance Pro

一款专为性能车赛道使用设计的保养记录工具。
基于「每节赛道 = 折算等效里程」的方法，精准追踪各部件的实际损耗。


## 功能

- **仪表盘**：彩色状态卡显示每个部件的折算里程进度（绿/橙/红/紫）
- **赛道日志**：记录每次下赛道的节数、里程，自动折算各部件损耗
- **保养记录**：一键标记保养完成，自动重置该部件计数器
- **多车管理**：支持管理多辆车，每车独立计算
- **可调参数**：每个部件的保养周期和赛道折算系数均可自定义

## 用户界面DEMO
<img width="2559" height="1529" alt="影像 (1)" src="https://github.com/user-attachments/assets/7ddcaa47-935b-481a-b013-ea828f00b290" />
<img width="2559" height="1532" alt="影像" src="https://github.com/user-attachments/assets/8be84ecc-05fa-4c65-9239-3f74dad74422" />


## 内置保养项目（默认参数）

| 项目 | 保养周期 | 每节赛道折算 |
| 机油 | 5000 km | +1000 km |
| 机油滤芯 | 5000 km | +1000 km |
| 火花塞（铱金） | 80000 km | +2500 km |
| 刹车油 ⚠ | 40000 km | +7500 km |
| 离合油 | 40000 km | +1500 km |
| 手动波箱油 | 70000 km | +2500 km |
| 差速器油 / LSD | 60000 km | +2500 km |
| 冷却液 | 60000 km | +750 km |
| 空气滤清器 | 20000 km | +1500 km |


## 构建与运行

### 前置要求

- **Windows 10/11**（64位）
- **.NET 8 SDK**（https://dotnet.microsoft.com/download）
- Visual Studio 2022 或 VS Code

### 方法一：CLI命令行

```powershell
# 进入项目目录
cd TrackMaintenance

# 构建并运行
dotnet run

# 或者发布为独立 EXE（无需安装 .NET）
dotnet publish -c Release -r win-x64 --self-contained -o publish
# 生成的 EXE 在 publish\ 目录中
```

### 方法二：Visual Studio 2022

1. 打开  TrackMaintenance.csproj
2. 按 **F5** 运行

### 发布单文件ONLY (.EXE)

```powershell
dotnet publish -c Release -r win-x64 --self-contained `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -o publish
```

## 数据存储

保养数据保存在：
```
%LOCALAPPDATA%\TrackMaintenancePro\data.json
```
（通常为 `C:\Users\<你的用户名>\AppData\Local\TrackMaintenancePro\`）
文件为 JSON 格式，可手动编辑或备份。


## 更新日志

- **v1.0** - 初始版本，包含全部核心功能
