using namespace AutomationConnectIQ.Lib

class ConnectIQDevelop {
	[string]$Device			# ビルド対象デバイス名
	[string]$OutputDir		# データの出力先フォルダ名
	[Simulator]$Sim			# シミュレータ操作用オブジェクト
	[TimeSimulator]$Time	# 時間シミュレーション操作用オブジェクト
	[int]$Number			# キャプチャファイル名の連番
	[string]$key			# ビルド用のキーファイル
	[string]$project		# プロジェクトファイル名

	# キャプチャーをするが、番号の後ろにコメントを付与している
	[void]Capture([string]$comment)
	{
		$this.Time.Action([TimeSimulator+ExecuteType]::Start)
		Start-sleep -Milliseconds 500
		$this.Time.Action([TimeSimulator+ExecuteType]::Pause)
	
		$bitmap = $this.Sim.Capture()
		$filename = $this.Device + "_" + $this.Number.ToString("D2") + "_" + $comment + ".png"
		$bitmap.Save($(Join-Path $this.OutputDir $filename))
		$bitmap.Dispose()
		$this.Time.Action([TimeSimulator+ExecuteType]::Stop)
		$this.Number++
	}

	[bool]Action([string]$dev, [Simulator]$simulator)
	{
		# クラス内メソッドでメンバー利用するため、変数を初期化
		# Actionをオーバーライドする場合、([ConnectIQDevelop]$this).Action($dev, $simulator)でこのメソッドを呼び出す
		$this.Sim = $simulator
		$this.Device = $dev
		$this.Number = 0

		return $true	# CheckAllでは$falseでリターンした場合にそこで終了
	}

	[void]Pre([Simulator]$simulator) {
	}

	[void]Simulate([string]$device) {
		$this.Device = $device
		$sdk = New-Object GarminSDK -Property @{
			Key = $this.key
		}
		$this.Sim = New-Object Simulator($sdk)
		$this.Sim.WaitForInput()
		$this.Action($device, $this.Sim)
	}

	[void]BuildAndLoad([string]$device) {
		$this.Device = $device
		$sdk = New-Object GarminSDK -Property @{
			Key = $this.key
		}
		# プロジェクトファイルの読み込み
		$proj = New-Object Jungle($this.project)
		$sdk.BuildProgram($proj, $this.Device)

		# シミュレーターの起動と起動待ち
		$this.Sim = New-Object Simulator($sdk)
		$this.Sim.WaitForInput()

		# シミュレーターにプログラムをロードとロード完了待ち
		$sdk.StartProgram($proj.DefaultProgramPath, $this.Device)
		$this.Sim.WaitForDeviceStart()
	}

	[void]Build([string]$device) {
		$this.Device = $device
		$sdk = New-Object GarminSDK -Property @{
			Key = $this.key
		}
		# プロジェクトファイルの読み込み
		$proj = New-Object Jungle($this.project)
		$sdk.BuildProgram($proj, $this.Device)
	}

	[void]CheckAll() {
		if (-Not (Test-Path -Path $this.OutputDir)) {
			New-Item -Path $this.OutputDir -ItemType Directory
		}

		# プロセス側の作業フォルダも変更するためEnvironment側も変更している
		push-Location -Path $this.OutputDir
		[Environment]::CurrentDirectory = $pwd

		$check = New-Object Checker -Property @{
			Key = $this.key
			Project = $this.project
		}
		$check.Check($true, $this.Action, $this.Pre, $null)

		Pop-Location
		[Environment]::CurrentDirectory = $pwd
	}

	[void]Check([string]$device) {
		if (-Not (Test-Path -Path $this.OutputDir)) {
			New-Item -Path $this.OutputDir -ItemType Directory
		}

		# プロセス側の作業フォルダも変更するためEnvironment側も変更している
		push-Location -Path $this.OutputDir
		[Environment]::CurrentDirectory = $pwd

		$check = New-Object Checker -Property @{
			Key = $this.key
			Project = $this.project
		}
		$check.Check($device, $this.Action, $this.Pre)

		Pop-Location
		[Environment]::CurrentDirectory = $pwd
	}

	[void]UnitTest([string]$device) {
		if (-Not (Test-Path -Path $this.OutputDir)) {
			New-Item -Path $this.OutputDir -ItemType Directory
		}

		# プロセス側の作業フォルダも変更するためEnvironment側も変更している
		push-Location -Path $this.OutputDir
		[Environment]::CurrentDirectory = $pwd

		$check = New-Object Checker -Property @{
			Key = $this.key
			Project = $this.project
		}
		$check.UnitTest($device)

		Pop-Location
		[Environment]::CurrentDirectory = $pwd
	}
}

function New-GarminSDK {
	Param (
		$Key = ""
	)
	New-Object GarminSDK  -Property @{
		Key = $Key
	}
}

function New-Jungle {
	param (
		$Project
	)
	New-Object Jungle($Project)
}

function New-Simulator {
	New-Object Simulator(New-Object GarminSDK)
}