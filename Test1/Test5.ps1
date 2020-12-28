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

$outputDir = "O:\Users\usr2\Downloads\output"

# Diagnostics系の結果をもらう
$sim.ToggleMenu([Simulator+SettingToggleMenu]::LowPowerMode, $true)
Start-sleep -Milliseconds 500 # 画面更新のための時間稼ぎ
$val = $sim.GetTimeDiagnostics()
$val.Total
$val.Execution
$val.Graphics
$val.Display
$val = $null
$val = $sim.GetMemoryDiagnostics()
$val.Memory.Current
$val.Memory.Max
$val.Memory.Peak
$val.Objects.Current
$val.Objects.Max
$val.Objects.Peak

# シミュレーターを終了させる
$sim.Close()