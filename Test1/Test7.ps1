# AutomationConnectIQ.Libの記述が面倒なので以下の設定をしておく
using namespace AutomationConnectIQ.Lib;

# SDK環境の作成
$sdk = New-Object GarminSDK -Property @{
	Key = "H:\Develop\Garmin\key\developer_key"
}
# プロジェクトファイルの読み込み
$proj = New-Object Jungle("H:\Develop\eclipse-workspace\Analog\monkey.jungle")

$deviceName = "fr45"
#$deviceName = "vivoactive4"

# プロジェクトのビルド
# 実行形式はプロジェクトの下のbinの下に配置。
$sdk.BuildProgram($proj, $deviceName)

# シミュレーターの起動と起動待ち
$sim = New-Object Simulator($sdk)
$sim.WaitForInput()

# シミュレーターにプログラムをロードとロード完了待ち
$sdk.StartProgram($proj.DefaultProgramPath, $deviceName)
$sim.WaitForDeviceStart()

# 通知数の設定
$sim.SetNotificationCount(4)

# アラーム数の設定
$sim.SetAlarmCount(3)

# バッテリー情報の設定
$sim.SetBatteryStatus(20, $false)

# 各種ゴールの通知
$sim.TriggerGoal([Simulator+GoalType]::Steps)
Start-sleep -Seconds 11
$sim.TriggerGoal([Simulator+GoalType]::FloorClimbed)
Start-sleep -Seconds 11
$sim.TriggerGoal([Simulator+GoalType]::ActiveMinutes)
