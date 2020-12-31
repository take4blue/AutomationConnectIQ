# AutomationConnectIQ.Libの記述が面倒なので以下の設定をしておく
using namespace AutomationConnectIQ.Lib;

# チェック用オブジェクトを生成
# パラメータは、デベロッパーキーとモンキープロジェクトファイル
$check = New-Object Checker -Property @{
	Key = "H:\Develop\Garmin\key\developer_key"
	Project = "H:\Develop\eclipse-workspace\DigiFuse\monkey.jungle"
}

$outputDir = "O:\Users\usr2\Downloads\output"
function Capture {
	$time.Action([TimeSimulator+ExecuteType]::Start)
	Start-sleep -Milliseconds 500
	$time.Action([TimeSimulator+ExecuteType]::Pause)

	$bitmap = $sim.Capture()
	$bitmap.Save($(Join-Path $outputDir $device".png"))
	$time.Action([TimeSimulator+ExecuteType]::Stop)
}

# 全デバイスでビルドチェック処理を実行
# シミュレーション処理はdelegate bool Simulation(string device, Simulator sim) となっている
# のでそれに合わせてPowerShellのラムダ式にしてある
$check.Check($true, 
{
	#デバイス名とシミュレーターオブジェクトをパラメータで受け取る
	Param($device, $sim)

	# GPS座標を、日本・皇居に設定、言語を英語に
	$sim.SetGPSPosition(35.685233, 139.752485)
	$sim.SetLanguage([Simulator+Language]::English)

	$time = $sim.CreateTime()
	$time.Open()
	$time.Time = Get-Date "2019-12-31 13:59:59"
	Capture
	$time.Close()

	return $true
},
{
	# 前処理
	Param($sim)
	$sim.SetBatteryStatus(50, $false)
},
{
	# 後処理
	Param($sim)
}
)