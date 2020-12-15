using System;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace AutomationConnectIQ.Cmd
{
    class Program
    {
        static void SimulatorTest(int i)
        {
            var simulator = new Lib.Simulator();
            switch (i) {
            case 0:
                SimulatorTimeTest(simulator);
                break;
            case 1:
                SimulatorActivityTest(simulator);
                break;
            case 2:
                SimulatorGPSQualityTest(simulator);
                break;
            case 3:
                SimulatorMemoryDiagnosticsTest(simulator);
                break;
            case 4:
                SimulatorToggleMenuTest(simulator);
                break;
            case 5:
                simulator.KillDevice();
                Console.ReadLine();
                break;
            }
        }

        /// <summary>
        /// 時間を設定する画面のシミュレーションテスト<br/>
        /// 0
        /// </summary>
        static void SimulatorTimeTest(Lib.Simulator simulator)
        {
            simulator.WaitForInput();

            // 以下は、英語で2019/1/1 0:0:0の文字盤を表示し、次に日本語で文字盤を表示する。
            // その文字盤の情報をキャプチャして残している
            simulator.SetLanguage(Lib.Simulator.Language.English);
            var time = simulator.CreateTime();
            time.Open();
            //Console.WriteLine(time.Time);
            time.Time = new DateTime(2019, 1, 1, 0, 0, 0);
            time.Action(Lib.TimeSimulator.ExecuteType.Start);
            Thread.Sleep(500);      // 時間シミュレーションを開始してから画面の更新がされるまで少し時間がかかるためここで一時停止
            time.Action(Lib.TimeSimulator.ExecuteType.Pause);   // ポーズをして画面更新がされないようにする

            var bitmap = simulator.Capture();
            if (bitmap is not null) {
                bitmap.Save(@"output_eng.png");
            }

            simulator.SetLanguage(Lib.Simulator.Language.Japanese);
            time.Action(Lib.TimeSimulator.ExecuteType.Start);
            Thread.Sleep(500);      // 時間シミュレーションを開始してから画面の更新がされるまで少し時間がかかるためここで一時停止
            time.Action(Lib.TimeSimulator.ExecuteType.Pause);   // ポーズをして画面更新がされないようにする
            bitmap = simulator.Capture();
            if (bitmap is not null) {
                bitmap.Save(@"output_jpn.png");
            }
            time.Close();   // 時間画面を閉じる
        }

        /// <summary>
        /// アクティビティ・バックグラウンド色の設定テスト<br/>
        /// 1
        /// </summary>
        static void SimulatorActivityTest(Lib.Simulator simulator)
        {
            Console.WriteLine(simulator.IsEnableTimerActivity(Lib.Simulator.Activity.Discard));
            simulator.SetTimerActivity(Lib.Simulator.Activity.Start);
            simulator.SetBackgroundColor(Lib.Simulator.BackgroundColor.Black);
            Console.ReadLine();
        }

        /// <summary>
        /// GPS位置設定テスト<br/>
        /// 2
        /// </summary>
        static void SimulatorGPSQualityTest(Lib.Simulator simulator)
        {
            simulator.SetGPSQuality(Lib.Simulator.GPSQualityType.NotAvailable);
        }

        /// <summary>
        /// メニューON/OFFのトグルテスト<br/>
        /// 4
        /// </summary>
        /// <param name="simulator"></param>
        static void SimulatorToggleMenuTest(Lib.Simulator simulator)
        {
            var work = true;
            simulator.ToggleMenu(Lib.Simulator.SettingToggleMenu.Vibrate, work);
            simulator.ToggleMenu(Lib.Simulator.SettingToggleMenu.ActivityTracking, work);
            simulator.ToggleMenu(Lib.Simulator.SettingToggleMenu.DoNotDisturb, work);
            simulator.ToggleMenu(Lib.Simulator.SettingToggleMenu.LowPowerMode, work);
            simulator.ToggleMenu(Lib.Simulator.SettingToggleMenu.SleepMode, work);
            simulator.ToggleMenu(Lib.Simulator.SettingToggleMenu.Tones, work);
            simulator.ToggleMenu(Lib.Simulator.SettingToggleMenu.UseDeviceHTTPSRequirements, work);
            simulator.ToggleMenu(Lib.Simulator.SettingToggleMenu.AppLockEnabled, work);
            simulator.ToggleMenu(Lib.Simulator.SettingToggleMenu.EnableAlert, work);

            Console.ReadLine();
            work = false;
            simulator.ToggleMenu(Lib.Simulator.SettingToggleMenu.Vibrate, work);
            simulator.ToggleMenu(Lib.Simulator.SettingToggleMenu.ActivityTracking, work);
            simulator.ToggleMenu(Lib.Simulator.SettingToggleMenu.DoNotDisturb, work);
            simulator.ToggleMenu(Lib.Simulator.SettingToggleMenu.LowPowerMode, work);
            simulator.ToggleMenu(Lib.Simulator.SettingToggleMenu.SleepMode, work);
            simulator.ToggleMenu(Lib.Simulator.SettingToggleMenu.Tones, work);
            simulator.ToggleMenu(Lib.Simulator.SettingToggleMenu.UseDeviceHTTPSRequirements, work);
            simulator.ToggleMenu(Lib.Simulator.SettingToggleMenu.AppLockEnabled, work);
            simulator.ToggleMenu(Lib.Simulator.SettingToggleMenu.EnableAlert, work);
        }

        /// <summary>
        /// Diagnosticsのテスト<br/>
        /// 3
        /// <param name="simulator"></param>
        static void SimulatorMemoryDiagnosticsTest(Lib.Simulator simulator)
        {
            // メモリ情報を取得する
            Lib.Simulator.MemoryDiagnostics result = null;
            for (int i = 0; i < 5 && result == null; i++) {
                result = simulator.GetMemoryDiagnostics();
            }

            if (result is not null) {
                Console.WriteLine("{0} {1} {2} {3} {4} {5}", result.Memory.Current, result.Memory.Max, result.Memory.Peak, result.Objects.Current, result.Objects.Max, result.Objects.Peak);
            }
            Console.ReadLine();
        }

        /// <summary>
        /// ビルド&シミュレーションテストクラスのチェック
        /// </summary>
        static void CheckerTest()
        {
            AllocConsole();

            if (!Directory.Exists("Output")) {
                Directory.CreateDirectory("Output");
            }
            // Checkのチェック
            var check = new Lib.Checker()
            {
                Key = @"H:\Develop\Garmin\key\developer_key",
                Project = @"H:\Develop\eclipse-workspace\DigiFuse\monkey.jungle",
                LogFile = @"Output\hoge.txt"
            };

            var result = check.Check("fr45", (device, simulator) =>
            {
                simulator.SetGPSPosition(35.685233, 139.752485);    // 皇居

                var time = simulator.CreateTime();
                time.Open();
                time.Time = new DateTime(2019, 1, 1, 0, 0, 0);

                simulator.SetLanguage(Lib.Simulator.Language.English);
                time.Action(Lib.TimeSimulator.ExecuteType.Start);
                Thread.Sleep(500);      // 時間シミュレーションを開始してから画面の更新がされるまで少し時間がかかるためここで一時停止
                time.Action(Lib.TimeSimulator.ExecuteType.Pause);   // ポーズをして画面更新がされないようにする
                var bitmap = simulator.Capture();
                if (bitmap is not null) {
                    bitmap.Save(@"Output\" + device + "_eng.png");
                }
                time.Action(Lib.TimeSimulator.ExecuteType.Stop);

                if (simulator.SetLanguage(Lib.Simulator.Language.Japanese)) {
                    time.Action(Lib.TimeSimulator.ExecuteType.Start);
                    Thread.Sleep(500);      // 時間シミュレーションを開始してから画面の更新がされるまで少し時間がかかるためここで一時停止
                    time.Action(Lib.TimeSimulator.ExecuteType.Pause);   // ポーズをして画面更新がされないようにする
                    bitmap = simulator.Capture();
                    if (bitmap is not null) {
                        bitmap.Save(@"Output\" + device + "_jpn.png");
                    }
                    time.Action(Lib.TimeSimulator.ExecuteType.Stop);
                }
                time.Close();   // 時間画面を閉じる
                return true;
            });

            Console.WriteLine(result);
            Process.Start("notepad", check.LogFile);
            Console.ReadLine();
        }

        /// <summary>
        /// SDK関連のテスト
        /// </summary>
        static void SDKTest()
        {
            AllocConsole();
            var sdk = new Lib.GarminSDK()
            {
                Key = @"H:\Develop\Garmin\key\developer_key"
            };
            var project = new Lib.Jungle(@"H:\Develop\eclipse-workspace\DigiFuse\monkey.jungle");
            Console.WriteLine(sdk.BuildProgram(project, project.Devices[0]));

            using (var stream = new System.IO.StreamWriter(@"hoge.txt")) {
                sdk.Writer = stream;
                Console.WriteLine(sdk.BuildProgram(project, project.Devices[0]));
                sdk.Writer = null;
            }

            Process.Start("notepad", @"hoge.txt");
        }

        /// <summary>
        /// プログラムのビルドとシミュレーターの起動
        /// </summary>
        /// <param name="device"></param>
        static void BuildAndSimulator(string device)
        {
            AllocConsole();
            var sdk = new Lib.GarminSDK() {
                Key = @"H:\Develop\Garmin\key\developer_key"
            };
            var project = new Lib.Jungle(@"H:\Develop\eclipse-workspace\DigiFuse\monkey.jungle");
            Console.WriteLine(sdk.BuildProgram(project, device));
            var simulator = new Lib.Simulator(sdk);
            sdk.StartProgram(project.DefaultProgramPath, device);
        }

        [System.Runtime.InteropServices.DllImport("Kernel32.dll")]
        public static extern bool AllocConsole();

        static void Main(string[] args)
        {
            if (args.Length > 0) {
                _ = int.TryParse(args[0], out int val);
                if (val < 100) {
                    AllocConsole();
                    // 今のところ0～5
                    SimulatorTest(val);
                }
                else {
                    switch (val) {
                    case 101:
                        CheckerTest();
                        break;
                    case 102:
                        SDKTest();
                        break;
                    case 103:
                        if (args.Length > 1) {
                            BuildAndSimulator(args[1]);
                        }
                        break;
                    }
                }
            }
        }
    }
}
