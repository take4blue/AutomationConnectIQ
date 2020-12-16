#
# Script.ps1
#

using namespace AutomationConnectIQ.Lib;

# チェック用オブジェクトを生成
# パラメータは、デベロッパーキーとモンキープロジェクトファイル
$check = New-Object Checker -Property @{
	Key = "H:\Develop\Garmin\key\developer_key"
	Project = "H:\Develop\eclipse-workspace\DigiFuse\monkey.jungle"
}

# fr45でビルドチェック処理を実行
# シミュレーション処理はdelegate bool Simulation(string device, Simulator sim) となっている
# のでそれに合わせてPowerShellのラムダ式にしてある
$check.Check("fr45", {
	#デバイス名とシミュレーターオブジェクトをパラメータで受け取る
	Param($device, $simulator)

	# GPS座標を、日本・皇居に設定
	$simulator.SetGPSPosition(35.685233, 139.752485)
	
	# 時間ウィンドウを開いて、言語を英語にしてから所定時間に変更する
	$time = $simulator.CreateTime()
	$time.Open()
	$time.Time = Get-Date "2020-1-1 13:00:00"
	$simulator.SetLanguage([Simulator+Language]::English)
	$time.Action([TimeSimulator+ExecuteType]::Start)

	sleep -Milliseconds 500	# 時間シミュレーションを開始直後、画面が更新されるまで待つ

	# 画面をキャプチャー
	$bitmap = $simulator.Capture()
	$bitmap.Save($device+".png")
	$time.Close()
	$true 	# 処理継続のためtrueを返す。これはデバイス名が指定されていない場合の連続処理と同じdelegateを採用しているためのおまじない
})