using namespace AutomationConnectIQ.Lib;
$sdk = New-Object GarminSDK -Property @{
	Key = "H:\Develop\Garmin\key\developer_key"
}
$sim = New-Object Simulator($sdk)
$sim.WaitForInput()

$monitor = $sim.CreateActivityMonitor()
$monitor.Open()
$monitor.SetActiveMinutesGoal(123)

$monitor.SetValue($true, 0, 0, 300)
$monitor.SetValue($true, 0, 1, 5050)
$monitor.SetValue($true, 0, 5, 5)
$monitor.SetValue($true, 0, 6, 6)
$monitor.SetValue($true, 0, 7, 7)
$monitor.SetValue($true, 0, 8, 8)
$monitor.SetValue($true, 0, 9, 9)
for ($row = 0; $row -lt 7; $row++) {
	for ($column = 0; $column -lt 9; $column++) {
		$monitor.SetValue($false, $row, $column, $row * 10 + $column)
	}
}
$monitor.Ok()

$sim.FastForward(5)
