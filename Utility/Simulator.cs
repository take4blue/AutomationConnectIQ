using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Automation;
using System.Threading;

namespace AutomationConnectIQ.Lib
{
    /// <summary>
    /// シミュレーターの操作
    /// </summary>
    public partial class Simulator
    {
        /// <summary>
        /// シミュレーターのトップ要素
        /// </summary>
        private AutomationElement top_;

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public Simulator()
        {
            Open(new GarminSDK());
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <exception cref="NullReferenceException">シミュレーターの起動失敗時</exception>
        public Simulator(GarminSDK sdk)
        {
            Open(sdk);
        }

        /// <summary>
        /// シミュレーターの起動<br/>
        /// すでに起動していたらそれを探すが、起動していなかった場合は自分で起動する
        /// </summary>
        /// <param name="sdk"></param>
        public void Open(GarminSDK sdk)
        {
            if (top_ is not null) {
                try {
                    var current = top_.Current;
                    return;
                }
                catch (ElementNotAvailableException) {
                    top_ = null;
                }
            }
            var process = Utility.FindTitleProcess("Connect IQ Device Simulator", 10);
            if (process is null) {
                process = sdk.StartSimUI();
                for (int i = 0; i < 1000 && (process.MainWindowHandle == IntPtr.Zero || !process.Responding); i++) {
                    process.Refresh();
                    Thread.Sleep(10);
                }
            }

            top_ = Utility.FindRootElement(process);
            if (top_.TryGetCurrentPattern(WindowPattern.Pattern, out object obj)) {
                // ウィンドウが最小化されている場合、起こす
                // そうしないと、その後の操作ができないので
                var win = obj as WindowPattern;
                if (win.Current.WindowVisualState == WindowVisualState.Minimized) {
                    win.SetWindowVisualState(WindowVisualState.Normal);
                }
            }
        }

        /// <summary>
        /// GarminSDK.StartProgramを呼び出したのち、シミュレーター側のプログラムロード完了まで待つ<br/>
        /// ステータスバーにReadyが出るまでまつだけ。
        /// </summary>
        public void WaitForInput()
        {
            Utility.FindElementsByName(top_, "Ready");
        }

        /// <summary>
        /// 起動中のシミュレーターを終了する
        /// </summary>
        public void Close()
        {
            Utility.ActionMenu(top_, new List<string>() { "File", "Quit" });
        }

        /// <summary>
        /// 起動中デバイスのシミュレーションを終了させる<br/>
        /// シミュレーター自体は閉じない<br/>別途違うプログラム・デバイスの読み込みを行わせる前に実行する関数。
        /// </summary>
        public void KillDevice()
        {
            Utility.ActionMenu(top_, new List<string>() { "File", "Kill Device" });
        }

        /// <summary>
        /// 時間シミュレーションクラスの生成
        /// </summary>
        public TimeSimulator CreateTime()
        {
            return new TimeSimulator(top_);
        }

        /// <summary>
        /// グラフィック画面のキャプチャ
        /// </summary>
        /// <returns></returns>
        public Bitmap Capture()
        {
            var guiItem = top_.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "panel")).Cast<AutomationElement>();
            if (guiItem.Count() == 1) {
                return Win32Api.CaptureWindow((IntPtr)guiItem.First().Current.NativeWindowHandle);
            }
            return null;
        }
    }
}

