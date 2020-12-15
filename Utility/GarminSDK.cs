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

    public class IQEnvironment : IEnvironment
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

        /// <summary>
        /// 現在のSDKのフォルダ名
        /// </summary>
        public string SdkFolder { get => sdkFolder_; }

        /// <summary>
        /// 現在のSDKのバージョン
        /// </summary>
        public string Version { get => version_; }

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
        /// Writerに何か入っていればそちらに出力をリダイレクトする</remarks>
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
        /// <param name="progName">プログラム名</param>
        /// <param name="device">デバイス名</param>
        public void StartProgram(string progName, string device)
        {
            List<string> arg = new List<string>()
            {
                progName,
                device
            };
            Process.Start(sdkFolder_ + @"\bin\monkeydo.bat", arg);
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
        /// <param name="device">ビルド対象のデバイス</param>
        /// <param name="output">出力実行形式ファイル名</param>
        /// <returns>ビルドが正しく終了した場合true</returns>
        public bool BuildProgram(Jungle project, string device, string output)
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
                    output,
                    "-w",
                    "-y",
                    key_,
                    "-d",
                    device,
                    "-f",
                    project.JungleFile,
                },
            };

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
