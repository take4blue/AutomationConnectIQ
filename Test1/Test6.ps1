# AutomationConnectIQ.Libの記述が面倒なので以下の設定をしておく
using namespace AutomationConnectIQ.Lib;

# SDK環境の作成
$sdk = New-Object GarminSDK -Property @{
	Key = "H:\Develop\Garmin\key\developer_key"
}
# プロジェクトファイルの読み込み
$proj = New-Object Jungle("H:\Develop\eclipse-workspace\DigiFuse\monkey.jungle")

#$deviceName = "fr45"
$deviceName = "vivoactive4"

# プロジェクトのビルド
# 実行形式はプロジェクトの下のbinの下に配置。
$sdk.BuildProgram($proj, $deviceName)

# シミュレーターの起動と起動待ち
$sim = New-Object Simulator($sdk)
$sim.WaitForInput()

# シミュレーターにプログラムをロードとロード完了待ち
$sdk.StartProgram($proj.DefaultProgramPath, $deviceName)
$sim.WaitForDeviceStart()

# トグル型メニューのON/OFF
$sim.ToggleMenu([Simulator+SettingToggleMenu]::Vibrate, $true);
$sim.ToggleMenu([Simulator+SettingToggleMenu]::ActivityTracking, $false);
$sim.ToggleMenu([Simulator+SettingToggleMenu]::DoNotDisturb, $true);
$sim.ToggleMenu([Simulator+SettingToggleMenu]::LowPowerMode, $false);
$sim.ToggleMenu([Simulator+SettingToggleMenu]::SleepMode, $true);
$sim.ToggleMenu([Simulator+SettingToggleMenu]::Tones, $false);
$sim.ToggleMenu([Simulator+SettingToggleMenu]::UseDeviceHTTPSRequirements, $true);
$sim.ToggleMenu([Simulator+SettingToggleMenu]::AppLockEnabled, $false);
$sim.ToggleMenu([Simulator+SettingToggleMenu]::EnableAlert, $true);

# 言語の設定
$sim.SetLanguage([Simulator+Language]::English)

# BLE/Wifi接続タイプとステータス
$sim.SetBleConnection([Simulator+ConnectionType]::Connected)
$sim.SetWiFiConnection([Simulator+ConnectionType]::Connected)
$sim.SetWiFiStatus([Simulator+WiFiStatus]::Avaliable)

# 12/24時間指定
$sim.SetDisplayHourType($true)

# 単位
$sim.SetDisplayUnit($true)

# 週頭の設定
$sim.SetFirstDayWeek([System.DayOfWeek]::Sunday)

# 受け取る通知種類を設定する
$sim.SetReceiveNotificationType([Simulator+ReceiveNotificationType]::All)