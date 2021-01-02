using namespace AutomationConnectIQ.Lib;

class SimulatorAction {
	[string]$Device
	[string]$OutputDir
	[Simulator]$Sim
	[TimeSimulator]$Time
	[int]$Number
	[string]$key
	[string]$project

	[void]Capture()
	{
		$this.Time.Action([TimeSimulator+ExecuteType]::Start)
		Start-sleep -Milliseconds 500
		$this.Time.Action([TimeSimulator+ExecuteType]::Pause)
	
		$bitmap = $this.Sim.Capture()
		$filename = $this.Number.ToString("D2") + ".png"
		$bitmap.Save($(Join-Path $this.OutputDir $this.Device $filename))
		$this.Time.Action([TimeSimulator+ExecuteType]::Stop)
		$this.Number++	
	}

	[bool]Action([string]$dev, [Simulator]$simulator)
	{
		$this.Sim = $simulator
		$this.Device = $dev
		$this.Number = 0

		if (-Not (Test-Path -Path $this.OutputDir)) {
			New-Item -Path $this.OutputDir -ItemType Directory
		}
		if (-not (Test-Path $(Join-Path $this.OutputDir $this.Device))) {
			New-Item -Path $(Join-Path $this.OutputDir $this.Device) -ItemType Directory
		}
		
		$simulator.SetNotificationCount(0)
		$simulator.SetAlarmCount(1)
		$simulator.SetBatteryStatus(100, $false)
		$simulator.SetBleConnection([Simulator+ConnectionType]::NotConnected)
		$simulator.ToggleMenu([Simulator+SettingToggleMenu]::LowPowerMode, $true)
		$simulator.ToggleMenu([Simulator+SettingToggleMenu]::ActivityTracking, $true)
		
		$this.Time = $simulator.CreateTime()
		$this.Time.Open()
		$this.Time.Time = Get-Date "2019-12-31 13:59:59"
		$simulator.SetLanguage([Simulator+Language]::English)
		# GPS座標の設定
		$simulator.SetGPSPosition(0, 0)
		$this.Capture()
		# 日の入り
		$simulator.SetGPSPosition(35.685233, 139.752485)
		$this.Capture()
		# 日の出
		$this.Time.Time = Get-Date "2019-12-30 17:59:59"
		$this.Capture()
		# 日の出(日をまたいだ場合)
		$this.Time.Time = Get-Date "2019-12-31 1:59:59"
		$this.Capture()
		
		$this.Time.Time = Get-Date "2019-12-31 13:59:59"
		$simulator.SetBleConnection([Simulator+ConnectionType]::Connected)
		$this.Capture()
		
		$simulator.SetNotificationCount(1)
		$this.Capture()
		
		$simulator.SetBatteryStatus(0, $false)
		$this.Capture()
		
		$simulator.ToggleMenu([Simulator+SettingToggleMenu]::LowPowerMode, $false);
		$this.Capture()
		
		$simulator.ToggleMenu([Simulator+SettingToggleMenu]::ActivityTracking, $false)
		$this.Capture()
		
		# 日本語
		$simulator.SetLanguage([Simulator+Language]::Japanese)
		$this.Capture()
		
		$this.Time.Close()

		return $true	# CheckAllでは$falseでリターンした場合にそこで終了
	}

	[void]Pre([Simulator]$simulator)
	{
		$this.Sim = $simulator
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

	[void]CheckAll() {
		$check = New-Object Checker -Property @{
			Key = $this.key
			Project = $this.project
		}
		$check.Check($true, $this.Action, $this.Pre, $null)
	}
}

$action = [SimulatorAction]::new()
$action.key = "H:\Develop\Garmin\key\developer_key"
$action.project = "H:\Develop\eclipse-workspace\DigiFuse\monkey.jungle"
$action.OutputDir = "O:\Users\usr2\Downloads\output1"

$action.Simulate("d2air")