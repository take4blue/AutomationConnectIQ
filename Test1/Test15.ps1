using namespace AutomationConnectIQ.Lib;
$check = New-Object Checker -Property @{
	Key = "H:\Develop\Garmin\key\developer_key"
	Project = "H:\Develop\eclipse-workspace\DigiFuse\monkey.jungle"
}

$check.UnitTest($check.Devices[0])