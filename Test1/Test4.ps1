# AutomationConnectIQ.Libの記述が面倒なので以下の設定をしておく
using namespace AutomationConnectIQ.Lib;

# SDK環境の作成
$sdk = New-Object GarminSDK -Property @{
	Key = "H:\Develop\Garmin\key\developer_key"
}
# プロジェクトファイルの読み込み
$proj = New-Object Jungle("H:\Develop\eclipse-workspace\DigiFuse\monkey.jungle")

$deviceName = "fr45"


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

# GPSのクオリティをUsableに位置を皇居に設定し、その時の状態をビットマップに保存する
$sim.SetGPSQuality([Simulator+GPSQualityType]::Usable);
$sim.SetGPSPosition(35.685233, 139.752485)
sleep -Milliseconds 500 # 画面更新のための時間稼ぎ
$bitmap = $sim.Capture()
$bitmap.Save($(Join-Path $outputDir $deviceName".png"))

# シミュレーターを終了させる
$sim.Close()