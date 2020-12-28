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

$time = $sim.CreateTime()
$time.Open()

$time.Time = Get-Date "2020-1-1 13:00:00"
$time.Factor = 3
$time.Action([TimeSimulator+ExecuteType]::Start)

Start-sleep -Seconds 10

if ($time.IsStarted) {
	$time.Action([TimeSimulator+ExecuteType]::Stop)
}

if ($time.IsOpen) {
	$time.Close()
}