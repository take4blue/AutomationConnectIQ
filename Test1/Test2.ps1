# AutomationConnectIQ.Libの記述が面倒なので以下の設定をしておく
using namespace AutomationConnectIQ.Lib;

# SDK環境の作成
$sdk = New-Object GarminSDK -Property @{
	Key = "H:\Develop\Garmin\key\developer_key"
}
# プロジェクトファイルの読み込み
$proj = New-Object Jungle("H:\Develop\eclipse-workspace\DigiFuse\monkey.jungle")

# プロジェクトのビルド
# 実行形式はプロジェクトの下のbinの下に配置。
$sdk.BuildProgram($proj, "fr45")

# プロジェクトのビルド
# 実行形式は、パラメータで指定する。(下の例は%TEMP%\hoge.prg)
$sdk.BuildProgram($proj, "fr45", $env:temp + "\hoge.prg")

# 全デバイスのビルド
# 出力ファイルは %TEMP%\devicename.prg
# ビルドのログは、%TEMP%\output.txtに保存される
$filename = Join-Path $env:TEMP "output.txt"
$tw = New-Object System.IO.StreamWriter($filename, $false, [Text.Encoding]::GetEncoding("UTF-8"))
$sdk.Writer = $tw   # GarminSDK内で発生するログの出力先を設定する
# 全ターゲットをビルドする
foreach ($dev in $proj.Devices) {
    $tw.WriteLine($sdk.BuildProgram($proj, $dev, $(Join-Path $env:TEMP $dev".prg")))
}
$sdk.Writer = $null # ログの出力を解除(出力先をクローズする前に必ず実施)
$tw.Close()
