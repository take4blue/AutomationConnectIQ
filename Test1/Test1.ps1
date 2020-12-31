using namespace AutomationConnectIQ.Lib;

$outputDir = "O:\Users\usr2\Downloads\output"

function Capture1 {
	$time.Action([TimeSimulator+ExecuteType]::Start)
	Start-sleep -Milliseconds 500
	$time.Action([TimeSimulator+ExecuteType]::Pause)

	$bitmap = $simulator.Capture()
	$bitmap.Save($(Join-Path $outputDir $device".png"))
	$time.Action([TimeSimulator+ExecuteType]::Stop)
}

# チェック用オブジェクトを生成
# パラメータは、デベロッパーキーとモンキープロジェクトファイル
$check = New-Object Checker -Property @{
	Key = "H:\Develop\Garmin\key\developer_key"
	Project = "H:\Develop\eclipse-workspace\DigiFuse\monkey.jungle"
}

# fr45でビルドチェック処理を実行
# シミュレーション処理はdelegate bool Simulation(string device, Simulator sim) となっている
# それに合わせてPowerShellのスクリプトブロックでシミュレーション処理を記述
$check.Check("fr45", {
	#デバイス名とシミュレーターオブジェクトをパラメータで受け取る
	Param($device, $simulator)

	$simulator.SetGPSPosition(35.685233, 139.752485)
	
	# 時間ウィンドウを開いて、言語を英語にしてから所定時間に変更する
	$time = $simulator.CreateTime()
	$time.Open()
	$time.Time = Get-Date "2020-1-1 13:00:00"
	$simulator.SetLanguage([Simulator+Language]::English)
	Capture1
	$time.Close()
	return $true
})