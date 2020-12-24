using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Automation;

namespace AutomationConnectIQ.Lib
{
    /// <summary>
    /// シミュレーターの操作
    /// </summary>
    public partial class Simulator
    {
        /// <summary>
        /// Watchface Diagnosticsの内容情報
        /// </summary>
        public struct TimeDiagnostics
        {
            /// <summary>
            /// 総合時間
            /// </summary>
            public int Total;
            /// <summary>
            /// 実行時時間
            /// </summary>
            public int Execution;
            /// <summary>
            /// グラフィック時間
            /// </summary>
            public int Graphics;
            /// <summary>
            /// 表示時間
            /// </summary>
            public int Display;
        }

        /// <summary>
        /// 時間計測画面(Watchface Diagnostics)の内容を取得する
        /// </summary>
        /// <param name="doClose">開いたWatchface Diagnosticsを閉じる場合trueにする</param>
        /// <returns>Watchface Diagnosticsの設定内容</returns>
        public TimeDiagnostics GetTimeDiagnostics(bool doClose = true)
        {
            var menu = new List<string>() {
                "File", "View Watchface Diagnostics"
            };
            var result = new TimeDiagnostics();

            if (Utility.ActionMenu(top_, menu)) {
                // メニュー選択後、品質画面の中から選択するものを探し出し、それをクリック後、OKボタンを押す
                var settingWindow = top_.FindAll(TreeScope.Children | TreeScope.Element,
                        new AndCondition(
                            new PropertyCondition(AutomationElement.NameProperty, "Watchface Diagnostics"),
                            new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window))).Cast<AutomationElement>();
                if (settingWindow.Count() == 1) {
                    var window = settingWindow.First();
                    var guiParts = window.FindAll(TreeScope.Children | TreeScope.Element,
                            new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Text)).Cast<AutomationElement>();
                    foreach(var gui in guiParts) {
                        var split = gui.Current.Name.Split(':');
                        if (split.Length == 2) {
                            _ = int.TryParse(split[1].Trim(), out int val);
                            switch (split[0]) {
                            case "Total Time":
                                result.Total = val;
                                break;
                            case "Execution Time":
                                result.Execution = val;
                                break;
                            case "Graphics Time":
                                result.Graphics = val;
                                break;
                            case "Display Time":
                                result.Display = val;
                                break;
                            }
                        }
                    }
                    if (doClose) {
                        guiParts = Utility.FindElementsByName(window, "OK");
                        if (guiParts.Count() == 1) {
                            if (guiParts.First().TryGetCurrentPattern(InvokePattern.Pattern, out object obj)) {
                                ((InvokePattern)obj).Invoke();
                            }
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// MemoryDiagnosticsで使うための現在の値、最大値、ピーク値を保持する構造体
        /// </summary>
        /// <typeparam name="T">値の型、intとかdoubleを想定している</typeparam>
        public class Usage<T>
        {
            /// <summary>
            /// 現在の値
            /// </summary>
            public T Current;
            /// <summary>
            /// 最大
            /// </summary>
            public T Max;
            /// <summary>
            /// Peak
            /// </summary>
            public T Peak;
        }

#if false
        /// <summary>
        /// Treeの詳細内容<br/>
        /// 今は出力できない
        /// </summary>
        public class Status
        {
            public List<string> Name;
            public string Type;
            public string Value;
            public int Size;
            public string Notes;
        }
#endif

        /// <summary>
        /// メモリ使用量情報
        /// </summary>
        public class MemoryDiagnostics
        {
            /// <summary>
            /// メモリに関する情報
            /// </summary>
            public Usage<double> Memory;
            /// <summary>
            /// オブジェクトに関する情報
            /// </summary>
            public Usage<int> Objects;

#if false
            public List<Status> Status;
#endif
        }

        /// <summary>
        /// メモリ情報を取り出す<br/>
        /// 時々失敗する場合がある。
        /// </summary>
        /// <returns>nullの場合失敗しているので、リトライすると取得できるかも</returns>
        public MemoryDiagnostics GetMemoryDiagnostics()
        {
            var menu = new List<string>() {
                "File", "View Memory"
            };
            MemoryDiagnostics result = null;

            if (Utility.ActionMenu(top_, menu)) {
                // メニュー選択後、品質画面の中から選択するものを探し出し、それをクリック後、OKボタンを押す
                var settingWindow = AutomationElement.RootElement.FindAll(TreeScope.Subtree,
                        new AndCondition(
                            new PropertyCondition(AutomationElement.NameProperty, "Active Memory"),
                            new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window))).Cast<AutomationElement>();
                if (settingWindow.Count() == 1) {
                    var window = settingWindow.First();
                    result = new MemoryDiagnostics();
                    // ウィンドウ先頭にあるメモリ・オブジェクトに関する情報をパースする
                    var guiParts = window.FindAll(TreeScope.Children,
                        new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Text)).Cast<AutomationElement>();
                    foreach(var gui in guiParts) {
                        var split = gui.Current.Name.Split(':');
                        if (split.Length == 2) {
                            switch(split[0]) {
                            case "Memory Usage":
                                var split2 = split[1].Trim().Split('/');
                                if (split2.Length == 2) {
                                    _ = double.TryParse(split2[0], out result.Memory.Current);
                                    _ = double.TryParse(split2[1].Remove(split2[1].Length - 2), out result.Memory.Max);
                                }
                                break;
                            case "Peak Memory":
                                _ = double.TryParse(split[1].Remove(split[1].Length - 2), out result.Memory.Peak);
                                break;
                            case "Object Usage":
                                split2 = split[1].Trim().Split('/');
                                if (split2.Length == 2) {
                                    _ = int.TryParse(split2[0], out result.Objects.Current);
                                    _ = int.TryParse(split2[1], out result.Objects.Max);
                                }
                                break;
                            case "Peak Objects":
                                _ = int.TryParse(split[1], out result.Objects.Peak);
                                break;
                            }
                        }
                    }
                    // Expandボタンを押し、展開されたツリー情報をパースする
                    Object objTmp;
                    guiParts = window.FindAll(TreeScope.Subtree,
                            new PropertyCondition(AutomationElement.NameProperty, "Expand All")).Cast<AutomationElement>();
                    if (guiParts.Count() == 1) {
                        if (guiParts.First().TryGetCurrentPattern(InvokePattern.Pattern, out objTmp)) {
                            ((InvokePattern)objTmp).Invoke();
                        }
                    }

                    // 以下、TreeView内の情報を取りたいのだが、LegacyIAccessible.DescriptionのAPIがないので、コメント化
                    //guiParts = window.FindAll(TreeScope.Subtree,
                    //        new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.TreeItem)).Cast<AutomationElement>();
                    //foreach(var item in guiParts) {
                    //}

                    if (window.TryGetCurrentPattern(WindowPattern.Pattern, out objTmp)) {
                        ((WindowPattern)objTmp).Close();
                    }
                }
            }
            return result;
        }
    }
}