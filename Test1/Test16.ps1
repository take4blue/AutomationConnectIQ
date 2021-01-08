using namespace AutomationConnectIQ.Lib;
$sdk = New-Object GarminSDK -Property @{
	Key = "H:\Develop\Garmin\key\developer_key"
}
$proj = New-Object Jungle("H:\Develop\eclipse-workspace\DigiFuse\monkey.jungle")

$sim = New-Object Simulator($sdk)
$sim.WaitForInput()
$sim.KillDevice()

$deviceName = "fr45"
$sdk.BuildProgram($proj, $deviceName)
$sdk.StartProgram($proj.DefaultProgramPath, $deviceName)
$sim.WaitForDeviceStart()

$sim.IsEnabledHeatMap		# <= V0.3追加項目

$sim.KillDevice()

$deviceName = "d2air"
$sdk.BuildProgram($proj, $deviceName)
$sdk.StartProgram($proj.DefaultProgramPath, $deviceName)
$sim.WaitForDeviceStart()

$sim.IsEnabledHeatMap		# <= V0.3追加項目
