#
# Script.ps1
#
[System.Reflection.Assembly]::LoadFile("H:\Develop\Visualstudio\repos\AutomationConnectIQ\Utility\bin\Debug\net5.0-windows\Utility.dll")

$garmin = new-object AutomationConnectIQ.Lib.GarminSDK -Property @{
	key = "H:\Develop\Garmin\key\developer_key"
}
$project = New-Object AutomationConnectIQ.Lib.Jungle("H:\Develop\eclipse-workspace\DigiFuse\monkey.jungle")

$garmin.BuildProgram($project, $project.Devices[0])