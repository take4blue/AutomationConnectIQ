using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace AutomationConnectIQ.Lib
{
    /// <summary>
    /// Connect IQ用の環境情報取り出し
    /// </summary>
    public interface IEnvironment
    {
        /// <summary>
        /// Connect IQの格納先情報
        /// </summary>
        public string AppBase { get; }
    }

    internal class IQEnvironment : IEnvironment
    {
        public string AppBase { get => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Garmin\ConnectIQ"; }
    }

    /// <summary>
    /// Connect IQ関連のファイルを取り扱うためのクラス
    /// </summary>
    public class GarminSDK
    {
        private string sdkFolder_;
        private string version_;
        private string key_;
        private string buildOption_;

        /// <summary>
        /// 現在のSDKのフォルダ名
        /// </summary>
        public string SdkFolder { get => sdkFolder_; }

        /// <summary>
        /// 現在のSDKのバージョン
        /// </summary>
        public string Version { get => version_; }

        /// <summary>
        /// バージョンの比較
        /// </summary>
        /// <param name="target">比較対象のバージョンで"4.1.0"とかの文字列で渡す</param>
        /// <returns>targetと同じバージョンなら0、古いバージョンなら-1、新しいバージョンなら1</returns>
        public int CompareVersion(string target)
        {
            return CompareVersion(version_, target);
        }

        /// <summary>
        /// バージョンの比較
        /// </summary>
        /// <param name="current">比較元のバージョンで"4.1.0"とかの文字列で渡す</param>
        /// <param name="target">比較対象のバージョンで"4.1.0"とかの文字列で渡す</param>
        /// <returns>currentとtargetと同じバージョンなら0、targetが古いバージョンなら-1、targetが新しいバージョンなら1</returns>
        static public int CompareVersion(string current, string target)
        {
            var cSplit = current.Split('.');
            var tSplit = target.Split('.');
            if (tSplit.Length <= cSplit.Length) {
                for (int i = 0; i < tSplit.Length; i++) {
                    var cNum = int.Parse(cSplit[i]);
                    var tNum = int.Parse(tSplit[i]);
                    if (cNum != tNum) {
                        return (tNum - cNum) < 0 ? -1 : 1;
                    }
                }
            }
            return 0;
        }

        /// <summary>
        /// ビルド時のキーファイル
        /// </summary>
        /// <remarks>未設定の場合ビルドで必ずfalseになる</remarks>
        public string Key {
            set {
                key_ = value;
            }
        }

        /// <summary>
        /// ビルド時の追加オプションの設定
        /// </summary>
        public string BuildOption {
            set {
                buildOption_ = value;
            }
        }

        /// <summary>
        /// シミュレーター起動、ビルド等での出力のリダイレクト先
        /// </summary>
        /// <remarks>設定したWriterを閉じる前には、こちらに必ずnullを設定する</remarks>
        public System.IO.StreamWriter Writer { set; private get; }

        /// <summary>
        /// ビルド時に使用するjarファイル名
        /// </summary>
        private string BuilderJar {
            get => sdkFolder_ + @"\bin\monkeybrains.jar";
        }

        private void Setup(IEnvironment env)
        {
            string connectiq = env.AppBase;
            if (!Directory.Exists(connectiq)) {
                throw new DirectoryNotFoundException(connectiq);
            }

            string file = connectiq + @"\current-sdk.cfg";
            sdkFolder_ = "";
            using (StreamReader sr = new StreamReader(file)) {
                sdkFolder_ = sr.ReadLine().TrimEnd('\\');
            }
            if (!Directory.Exists(sdkFolder_)) {
                throw new DirectoryNotFoundException(sdkFolder_);
            }
            file = sdkFolder_ + @"\bin\api.db";
            version_ = "";
            using (StreamReader sr = new StreamReader(file)) {
                version_ = sr.ReadLine();
            }
        }

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public GarminSDK()
        {
            Setup(new IQEnvironment());
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <exception cref="DirectoryNotFoundException">Connect IQのSDKフォルダがない場合</exception>
        public GarminSDK(IEnvironment env)
        {
            Setup(env);
        }

        /// <summary>
        /// シミュレーターの実行
        /// </summary>
        /// <remarks>
        /// Writerに何か入っていればそちらに出力をリダイレクトする<br/>
        /// シミュレータのメニューなどの操作が可能になる起動完了を待つ場合は、Simulator.WaitForInputを使う。</remarks>
        public Process StartSimUI()
        {
            var prc = new Process();
            prc.StartInfo.FileName = sdkFolder_ + @"\bin\simulator.exe";
            if (Writer is not null) {
                prc.StartInfo.UseShellExecute = false;
                prc.StartInfo.RedirectStandardOutput = true;
                prc.StartInfo.RedirectStandardError = true;
                prc.OutputDataReceived += (sender, ev) =>
                {
                    if (Writer is not null)
                        Writer.WriteLine(ev.Data);
                };
                prc.ErrorDataReceived += (sender, ev) =>
                {
                    if (Writer is not null)
                        Writer.WriteLine(ev.Data);
                };
            }
            if (prc.Start()) {
                if (Writer is not null) {
                    prc.BeginErrorReadLine();
                    prc.BeginOutputReadLine();
                }
                return prc;
            }
            else {
                return null;
            }
        }

        /// <summary>
        /// シミュレーションでプログラムを実行する
        /// </summary>
        /// <remarks>
        /// シミュレータでウォッチ用プログラムの起動完了を待つ場合は、WaitForDeviceStartを使う。
        /// </remarks>
        /// <param name="progName">プログラム名</param>
        /// <param name="device">デバイス名</param>
        /// <param name="isUnitTest">EvilのUnitTest用プログラムの実行をする場合trueにする<br/>引数が指定されていない場合はfalseで動作する</param>
        public void StartProgram(string progName, string device, bool isUnitTest = false)
        {
            var prc = new Process();
            prc.StartInfo.FileName = sdkFolder_ + @"\bin\monkeydo.bat";
            prc.StartInfo.ArgumentList.Add(progName);
            prc.StartInfo.ArgumentList.Add(device);
            if (isUnitTest) {
                prc.StartInfo.ArgumentList.Add("/t");
            }
            if (Writer is not null) {
                prc.StartInfo.UseShellExecute = false;
                prc.StartInfo.RedirectStandardOutput = true;
                prc.StartInfo.RedirectStandardError = true;
                prc.OutputDataReceived += (sender, ev) =>
                {
                    if (Writer is not null)
                        Writer.WriteLine(ev.Data);
                };
                prc.ErrorDataReceived += (sender, ev) =>
                {
                    if (Writer is not null)
                        Writer.WriteLine(ev.Data);
                };
            }
            if (prc.Start()) {
                if (Writer is not null) {
                    prc.BeginErrorReadLine();
                    prc.BeginOutputReadLine();
                }
            }
        }

        /// <summary>
        /// プログラムのビルド<br/>
        /// プログラムはプロジェクトファイルの下のデフォルトの場所に作成される<br/>
        /// ビルドが完了するまでwaitする。
        /// </summary>
        public bool BuildProgram(Jungle project, string device)
        {
            return BuildProgram(project, device, project.DefaultProgramPath);
        }

        /// <summary>
        /// プログラムのビルド<br/>
        /// ビルドが完了するまでwaitする。
        /// </summary>
        /// <remarks>
        /// デバイス名がプロジェクト内に存在しない場合falseでリターンする<br/>
        /// Writerに何か入っていればそちらに出力をリダイレクトする
        /// </remarks>
        /// <param name="project">プロジェクト情報</param>
        /// <param name="device">ビルド対象のデバイス</param>
        /// <param name="progName">出力実行形式ファイル名</param>
        /// <param name="isUnitTestBuild">EvilのUnitTest用ビルドの場合trueにする<br/>引数が指定されていない場合はfalseで動作する</param>
        /// <returns>ビルドが正しく終了した場合true</returns>
        public bool BuildProgram(Jungle project, string device, string progName, bool isUnitTestBuild = false)
        {
            if (!project.IsValidDevice(device)) {
                return false;
            }
            if (key_ is null) {
                return false;
            }
            // 起動の例
            // java -Dfile.encoding=UTF-8 -Dapple.awt.UIElement=true -jar Connect IQ SDKの下のmonkeybrains.jar -o 実行形式の出力ファイル名(拡張子はprg) -w -y デベロッパーキー -d デバイス名 -f プロジェクトのmonkey.jungleファイル名

            var startinfo = new ProcessStartInfo("java.exe")
            {
                ArgumentList =
                {
                    "-Dfile.encoding=UTF-8",
                    "-Dapple.awt.UIElement=true",
                    "-jar",
                    BuilderJar,
                    "-o",
                    progName,
                    "-w",
                    "-y",
                    key_,
                    "-d",
                    device,
                    "-f",
                    project.JungleFile,
                },
            };
            if (isUnitTestBuild) {
                startinfo.ArgumentList.Add("--unit-test");
            }
            // ビルドオプションを引数に追加する
            if (!string.IsNullOrEmpty(buildOption_)) {
                var work = buildOption_.Split(' ');
                foreach(var item in work) {
                    startinfo.ArgumentList.Add(item);
                }
            }

            if (Writer is not null) {
                startinfo.UseShellExecute = false;
                startinfo.RedirectStandardOutput = true;
                startinfo.RedirectStandardError = true;
            }

            var result = false;
            using (var prc = new Process() { StartInfo = startinfo }) {
                if (Writer is not null) {
                    prc.OutputDataReceived += (sender, ev) =>
                    {
                        if (Writer is not null)
                            Writer.WriteLine(ev.Data);
                    };
                    prc.ErrorDataReceived += (sender, ev) =>
                    {
                        if (Writer is not null)
                            Writer.WriteLine(ev.Data);
                    };
                }
                if (prc.Start()) {
                    if (Writer is not null) {
                        prc.BeginErrorReadLine();
                        prc.BeginOutputReadLine();
                    }
                    // ビルド開始
                    prc.WaitForExit();
                    if (prc.ExitCode == 0) {
                        result = true;
                    }
                    prc.Close();
                }
            }
            return result;
        }
    }
}
