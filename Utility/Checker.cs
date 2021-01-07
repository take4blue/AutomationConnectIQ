using System.IO;
using System.Collections.Generic;

// マクロ UsePSFILE はPowerShellスクリプトファイルをシミュレーター動作で使用するためのマクロ

namespace AutomationConnectIQ.Lib
{
    /// <summary>
    /// ビルドしてチェックを自動で行うユーティリティクラス
    /// </summary>
    public class Checker
    {
#if UsePSFILE
        /// <summary>
        /// PSファイルにシミュレーション操作を任せたチェッカー
        /// </summary>
        private static bool Check(GarminSDK sdk, Jungle project, string device, string psFile, Simulator simulator, StreamWriter writer)
        {
            var programName = Jungle.MakeProgramPath(project.EntryName);
            if (!sdk.BuildProgram(project, device, programName)) {
                return false;
            }
            if (psFile.Length > 0) {
                using Runspace rs = RunspaceFactory.CreateRunspace();
                rs.Open();
                using (PowerShell ps = PowerShell.Create()) {
                    ps.Runspace = rs;
                    var para = new List<Object>()
                        {
                            device,
                            simulator,
                        };
                    if (writer is not null) {
                        para.Add(writer);
                    }
                    simulator.KillDevice();
                    sdk.StartProgram(programName, device);
                    simulator.WaitForDeviceStart();

                    var result = ps.AddCommand(psFile).Invoke(para);
                    foreach (var val in result) {
                        if (val is null) {
                            Console.WriteLine("null");
                        }
                        else {
                            Console.WriteLine(val.ToString());
                        }
                    }
                }

                rs.Close();
            }
            return true;
        }
#endif

        private static bool Check(GarminSDK sdk, Jungle project, string device, Simulation func, Simulator simulator)
        {
            var programName = Jungle.MakeProgramPath(project.EntryName);
            if (!sdk.BuildProgram(project, device, programName)) {
                return false;
            }
            simulator.KillDevice();
            sdk.StartProgram(programName, device);
            simulator.WaitForDeviceStart();
            var result = func(device, simulator);
            simulator.KillDevice();
            return result;
        }

        private static bool UnitTest(GarminSDK sdk, Jungle project, string device, Simulator simulator)
        {
            var programName = Jungle.MakeProgramPath(project.EntryName);
            if (!sdk.BuildProgram(project, device, programName, true)) {
                return false;
            }
            simulator.KillDevice();
            sdk.StartProgram(programName, device, true);
            simulator.WaitForDeviceStart();
            return true;
        }

        /// <summary>
        /// チェックで使うシミュレーション部分のデリゲート処理
        /// </summary>
        /// <param name="device">チェック対象としているデバイス名</param>
        /// <param name="sim">シミュレーター処理用</param>
        /// <returns>シミュレーションが完了したらtrueを返すようにする</returns>
        public delegate bool Simulation(string device, Simulator sim);

        /// <summary>
        /// チェック時の前後で行う処理部分のデリゲート処理<br/>
        /// 主にシミュレーターの事前設定などをやらせるための物
        /// </summary>
        public delegate void PrePostAction(Simulator sim);

#region パラメータ類
        /// <summary>
        /// ビルド時のキーファイル
        /// </summary>
        public string Key { set; private get; } = "";

        /// <summary>
        /// ビルド時のプロジェクト(monkey.jungle)ファイル
        /// </summary>
        public string Project { set; private get; } = "";
#if UsePSFILE
        /// <summary>
        /// テスト用のPowerShellスクリプトファイル<br/>
        /// ファイルが設定されていない場合はビルドのみ
        /// </summary>
        public string PSFile { set; private get; } = "";
#endif

        /// <summary>
        /// 出力用のログファイル<br/>
        /// Writerがnull以外の場合、そちらが優先される
        /// </summary>
        public string LogFile { set; get; } = "";

        /// <summary>
        /// 出力のログファイルへのストリーム
        /// </summary>
        public System.IO.StreamWriter Writer { set; private get; } = null;
        #endregion

#if UsePSFILE
        /// <summary>
        /// 指定されたデバイスに対して、ビルド＆チェックを実施する
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public bool Check(string device)
        {
            // キーとプロジェクトが設定されていない場合はエラー、またそれぞれのファイルがない場合はエラー
            if (Key.Length == 0 || Project.Length == 0) {
                return false;
            }
            if (!File.Exists(Key) || !File.Exists(Project)) {
                return false;
            }
            // PSファイルが存在しない場合はエラー
            if (PSFile.Length > 0 && !File.Exists(PSFile)) {
                return false;
            }
            var sdk = new GarminSDK() { Key = Key };
            if (Writer is not null) {
                sdk.Writer = Writer;
            }
            // デバイスがプロジェクト内になければエラー
            var project = new Jungle(Project);
            if (!project.IsValidDevice(device)) {
                return false;
            }

            var result = false;
            if (Writer is null && LogFile.Length > 0) {
                using var stream = new StreamWriter(LogFile);
                sdk.Writer = stream;
                Simulator sim = new Simulator(sdk);
                sim.Open(sdk);
                result = Check(sdk, project, device, PSFile, sim, stream);
                sim.Close();
                sdk.Writer = null;
            }
            else {
                Simulator sim = new Simulator(sdk);
                sim.Open(sdk);
                result = Check(sdk, project, device, PSFile, sim, Writer);
                sim.Close();
            }
            return result;
        }
#endif

        /// <summary>
        /// 指定されたデバイスに対して、ビルド＆チェックを実施する
        /// </summary>
        /// <param name="device">処理するデバイス名</param>
        /// <param name="func">シミュレーションチェック関数</param>
        /// <param name="pre">デバイスシミュレーションの前処理</param>
        public bool Check(string device, Simulation func, PrePostAction pre = null)
        {
            // キーとプロジェクトが設定されていない場合はエラー、またそれぞれのファイルがない場合はエラー
            if (Key.Length == 0 || Project.Length == 0) {
                return false;
            }
            if (!File.Exists(Key) || !File.Exists(Project)) {
                return false;
            }
            var sdk = new GarminSDK() { Key = Key };
            if (Writer is not null) {
                sdk.Writer = Writer;
            }
            // デバイスがプロジェクト内になければエラー
            var project = new Jungle(Project);
            if (!project.IsValidDevice(device)) {
                return false;
            }

            var result = false;
            if (Writer is null && LogFile.Length > 0) {
                using var stream = new StreamWriter(LogFile);
                sdk.Writer = stream;
                Simulator sim = new Simulator(sdk);
                sim.Open(sdk);
                if (pre is not null) {
                    pre(sim);
                }
                result = Check(sdk, project, device, func, sim);
                sim.Close();
                sdk.Writer = null;
            }
            else {
                Simulator sim = new Simulator(sdk);
                sim.Open(sdk);
                if (pre is not null) {
                    pre(sim);
                }
                result = Check(sdk, project, device, func, sim);
                sim.Close();
            }
            return result;
        }

#if UsePSFILE
        /// <summary>
        /// 全デバイスに対してチェックを実施する
        /// </summary>
        /// <param name="isBreak">どれか一つのデバイスでエラーが出たらそこで停止する</param>
        /// <returns></returns>
        public bool Check(bool isBreak)
        {
            // キーとプロジェクトが設定されていない場合はエラー、またそれぞれのファイルがない場合はエラー
            if (Key.Length == 0 || Project.Length == 0) {
                return false;
            }
            if (!File.Exists(Key) || !File.Exists(Project)) {
                return false;
            }
            // PSファイルが存在しない場合はエラー
            if (PSFile.Length > 0 && !File.Exists(PSFile)) {
                return false;
            }
            var sdk = new GarminSDK() { Key = Key };
            if (Writer is not null) {
                sdk.Writer = Writer;
            }
            // デバイスがプロジェクト内になければエラー
            var project = new Jungle(Project);
            var result = true;
            if (Writer is null && LogFile.Length > 0) {
                using var stream = new StreamWriter(LogFile);
                sdk.Writer = stream;
                Simulator sim = new Simulator(sdk);
                sim.Open(sdk);
                foreach (var device in project.Devices) {
                    if (!Check(sdk, project, device, PSFile, sim, stream)) {
                        result = false;
                        if (isBreak) {
                            break;
                        }
                    }
                }
                sim.Close();
                sdk.Writer = null;
            }
            else {
                Simulator sim = new Simulator(sdk);
                sim.Open(sdk);
                foreach (var device in project.Devices) {
                    if (!Check(sdk, project, device, PSFile, sim, Writer)) {
                        result = false;
                        if (isBreak) {
                            break;
                        }
                    }
                }
                sim.Close();
            }
            return result;
        }
#endif

        /// <summary>
        /// 全デバイスに対してチェックを実施する
        /// </summary>
        /// <param name="isBreak">どれか一つのデバイスでエラーが出たらそこで停止する</param>
        /// <param name="func">シミュレーションチェック関数</param>
        /// <param name="pre">デバイスシミュレーションの前処理</param>
        /// <param name="post">デバイスシミュレーションの後処理</param>
        public bool Check(bool isBreak, Simulation func, PrePostAction pre, PrePostAction post)
        {
            // キーとプロジェクトが設定されていない場合はエラー、またそれぞれのファイルがない場合はエラー
            if (Key.Length == 0 || Project.Length == 0) {
                return false;
            }
            if (!File.Exists(Key) || !File.Exists(Project)) {
                return false;
            }
            var sdk = new GarminSDK() { Key = Key };
            if (Writer is not null) {
                sdk.Writer = Writer;
            }
            // デバイスがプロジェクト内になければエラー
            var project = new Jungle(Project);
            var result = true;
            if (Writer is null && LogFile.Length > 0) {
                using var stream = new StreamWriter(LogFile);
                sdk.Writer = stream;
                Simulator sim = new Simulator(sdk);
                sim.Open(sdk);
                if (pre is not null) {
                    pre(sim);
                }
                foreach (var device in project.Devices) {
                    if (!Check(sdk, project, device, func, sim)) {
                        result = false;
                        if (isBreak) {
                            break;
                        }
                    }
                }
                if (post is not null) {
                    post(sim);
                }
                sim.Close();
                sdk.Writer = null;
            }
            else {
                Simulator sim = new Simulator(sdk);
                sim.Open(sdk);
                if (pre is not null) {
                    pre(sim);
                }
                foreach (var device in project.Devices) {
                    if (!Check(sdk, project, device, func, sim)) {
                        result = false;
                        if (isBreak) {
                            break;
                        }
                    }
                }
                if (post is not null) {
                    post(sim);
                }
                sim.Close();
            }
            return result;
        }

        /// <summary>
        /// UnitTestの実施
        /// </summary>
        /// <param name="device">処理するデバイス名</param>
        public bool UnitTest(string device)
        {
            if (Key.Length == 0 || Project.Length == 0) {
                return false;
            }
            if (!File.Exists(Key) || !File.Exists(Project)) {
                return false;
            }
            var sdk = new GarminSDK() { Key = Key };
            if (Writer is not null) {
                sdk.Writer = Writer;
            }
            // デバイスがプロジェクト内になければエラー
            var project = new Jungle(Project);
            if (!project.IsValidDevice(device)) {
                return false;
            }
            var result = false;
            if (Writer is null && LogFile.Length > 0) {
                using var stream = new StreamWriter(LogFile);
                sdk.Writer = stream;
                Simulator sim = new Simulator(sdk);
                sim.Open(sdk);
                result = UnitTest(sdk, project, device, sim);
                sim.Close();
                sdk.Writer = null;
            }
            else {
                Simulator sim = new Simulator(sdk);
                sim.Open(sdk);
                result = UnitTest(sdk, project, device, sim);
                sim.Close();
            }
            return result;
        }

        /// <summary>
        /// ターゲットとするデバイス情報
        /// </summary>
        public List<string> Devices {
            get {
                var project = new Jungle(Project);
                return project.Devices;
            }
        }
    }
}
