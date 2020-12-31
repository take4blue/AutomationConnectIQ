# AutomationConnectIQ.Libの記述が面倒なので以下の設定をしておく
using namespace AutomationConnectIQ.Lib;

function Capture {
	param (
		$number
	)
	$time.Action([TimeSimulator+ExecuteType]::Start)
	Start-sleep -Milliseconds 500
	$time.Action([TimeSimulator+ExecuteType]::Pause)

	$bitmap = $sim.Capture()
	$filename = $number.ToString("D2") + ".png"
	$bitmap.Save($(Join-Path $outputDir $device $filename))
	$time.Action([TimeSimulator+ExecuteType]::Stop)
}

# SDK環境の作成
$sdk = New-Object GarminSDK -Property @{
	Key = "H:\Develop\Garmin\key\developer_key"
}
$device = "hoge"
$outputDir = "O:\Users\usr2\Downloads\output1"

# シミュレーターの起動と起動待ち
$sim = New-Object Simulator($sdk)
$sim.WaitForInput()

if (-Not (Test-Path -Path $outputDir)) {
	New-Item -Path $outputDir -ItemType Directory
}
if (-not (Test-Path $(Join-Path $outputDir $device))) {
	New-Item -Path $(Join-Path $outputDir $device) -ItemType Directory
}

$i = 0
$sim.SetNotificationCount(0)
$sim.SetAlarmCount(1)
$sim.SetBatteryStatus(100, $false)
$sim.SetBleConnection([Simulator+ConnectionType]::NotConnected)
$sim.ToggleMenu([Simulator+SettingToggleMenu]::LowPowerMode, $true)
$sim.ToggleMenu([Simulator+SettingToggleMenu]::ActivityTracking, $true)

$time = $sim.CreateTime()
$time.Open()
$time.Time = Get-Date "2019-12-31 13:59:59"
$sim.SetLanguage([Simulator+Language]::English)
# GPS座標の設定
$sim.SetGPSPosition(0, 0)
Capture($i++)
# 日の入り
$sim.SetGPSPosition(35.685233, 139.752485)
Capture($i++)
# 日の出
$time.Time = Get-Date "2019-12-30 17:59:59"
Capture($i++)
# 日の出(日をまたいだ場合)
$time.Time = Get-Date "2019-12-31 1:59:59"
Capture($i++)

$time.Time = Get-Date "2019-12-31 13:59:59"
$sim.SetBleConnection([Simulator+ConnectionType]::Connected)
Capture($i++)

$sim.SetNotificationCount(1)
Capture($i++)

$sim.SetBatteryStatus(0, $false)
Capture($i++)

$sim.ToggleMenu([Simulator+SettingToggleMenu]::LowPowerMode, $false);
Capture($i++)

$sim.ToggleMenu([Simulator+SettingToggleMenu]::ActivityTracking, $false)
Capture($i++)

# 日本語
$sim.SetLanguage([Simulator+Language]::Japanese)
Capture($i++)

$time.Close()
