using namespace AutomationConnectIQ.Lib;
$sdk = New-Object GarminSDK -Property @{
	Key = "H:\Develop\Garmin\key\developer_key"
}
$proj = New-Object Jungle("H:\Develop\eclipse-workspace\DigiFuse\monkey.jungle")
$sim = New-Object Simulator($sdk)
$sim.WaitForInput()

$deviceName = "fr45"

$sdk.BuildProgram($proj, $deviceName, $proj.DefaultProgramPath, $true)
$sdk.StartProgram($proj.DefaultProgramPath, $deviceName, $true)

remove-variable sdk
remove-variable proj
Remove-Variable deviceName

$check = New-Object Checker -Property @{
	Key = "H:\Develop\Garmin\key\developer_key"
	Project = "H:\Develop\eclipse-workspace\DigiFuse\monkey.jungle"
}

$check.UnitTest($check.Devices[0])
remove-variable check