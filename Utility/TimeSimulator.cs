using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Automation;
using System.ComponentModel;

namespace AutomationConnectIQ.Lib
{
    /// <summary>
    /// 時間シミュレーションウィンドウの制御クラス
    /// </summary>
    /// <example>
    /// <code>
    /// var time = simulator.CreateTime();
    /// time.Open();
    /// time.Time = new DateTime(2019, 1, 1, 0, 0, 0);
    /// time.Action(Lib.TimeSimulator.ExecuteType.Start);
    /// Thread.Sleep(500);      // 時間シミュレーションを開始してから画面の更新がされるまで少し時間がかかるためここで一時停止
    /// time.Action(Lib.TimeSimulator.ExecuteType.Pause);   // ポーズをして画面更新がされないようにする
    /// time.Close();
    /// </code>
    /// </example>
    public class TimeSimulator
    {
        /// <summary>
        /// シミュレーターのトップ要素
        /// </summary>
        readonly AutomationElement top_;

        /// <summary>
        /// 時間設定ウィンドウのトップ要素
        /// </summary>
        AutomationElement time_ = null;

        internal TimeSimulator(AutomationElement top)
        {
            top_ = top;
        }

        /// <summary>
        /// 時間設定ウィンドウを開く
        /// </summary>
        public void Open()
        {
            if (time_ is null) {
                List<string> menu = new List<string>()
                {
                    "Simulation", "Time Simulation"
                };
                if (Utility.ActionMenu(top_, menu)) {
                    for (int i = 0; i < 10; i++) {
                        var guiItem = top_.FindAll(TreeScope.Children, 
                            new PropertyCondition(AutomationElement.NameProperty, "Time Simulation")).Cast<AutomationElement>();
                        if (guiItem.Count() == 1) {
                            time_ = guiItem.First();
                            return;
                        }
                        Thread.Sleep(10);
                    }
                }
            }
        }

        /// <summary>
        /// 時間設定ウィンドウを閉じる
        /// </summary>
        /// <remarks>
        /// 時間シミュレーションが開始されている場合、いったんストップしてから閉じるようにしてある。</remarks>
        public void Close()
        {
            if (time_ is not null) {
                this.Action(ExecuteType.Stop);  // 一応ストップしておく
                if (time_.TryGetCurrentPattern(WindowPattern.Pattern, out object obj)) {
                    ((WindowPattern)obj).Close();
                    time_ = null;
                }
            }
        }

        /// <summary>
        /// 時間の設定
        /// </summary>
        /// <remarks>
        /// wxWidgetsの日付・時刻設定は、DTM_GETSYSTEMTIMEを送っただけでは、内部情報との差異でStartボタンを押すとアサートが発生する。<br/>
        /// そのため、日付・時刻のスピンを動かし、内部情報との差異をなくしておく必要がある。
        /// </remarks>
        public DateTime Time {
            set {
                if (time_ is not null) {
                    var guiItem = time_.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Pane)).Cast<AutomationElement>();
                    if (guiItem.Count() == 2) {
                        foreach (var item in guiItem) {
                            Utility.SetDate(item, value);
                            var spins = item.FindAll(TreeScope.Descendants, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button)).Cast<AutomationElement>();
                            if (spins.Count() == 2) {
                                foreach (var spin in spins) {
                                    if (spin.TryGetCurrentPattern(InvokePattern.Pattern, out object obj)) {
                                        ((InvokePattern)obj).Invoke();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 進捗倍率の設定
        /// </summary>
        public uint Factor {
            get {
                if (time_ is not null) {
                    var guiItem = time_.FindAll(TreeScope.Children, 
                        new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Slider)).Cast<AutomationElement>();
                    if (guiItem.Count() == 1) {
                        if (guiItem.First().TryGetCurrentPattern(RangeValuePattern.Pattern, out object obj)) {
                            return (uint)((RangeValuePattern)obj).Current.Value;
                        }
                    }
                }
                return 1;
            }
            set {
                if (time_ is not null) {
                    var guiItem = time_.FindAll(TreeScope.Children,
                        new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Slider)).Cast<AutomationElement>();
                    if (guiItem.Count() == 1) {
                        if (guiItem.First().TryGetCurrentPattern(RangeValuePattern.Pattern, out object obj)) {
                            ((RangeValuePattern)obj).SetValue((double)value);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 時間シミュレーションウィンドウが開いているかどうか
        /// </summary>
        public bool IsOpen {
            get {
                return time_ is not null;
            }
        }

        /// <summary>
        /// 時間シミュレーションが実行中かどうか
        /// </summary>
        public bool IsStarted {
            get {
                if (time_ is not null) {
                    var guiItem = time_.FindAll(TreeScope.Children,
                        new PropertyCondition(AutomationElement.NameProperty, "Start")).Cast<AutomationElement>();
                    if (guiItem.Count() == 1) {
                        if (!guiItem.First().Current.IsEnabled) {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// 押すボタンの種類
        /// </summary>
        public enum ExecuteType
        {
            [Description("Start")]
            Start,
            [Description("Stop")]
            Stop,
            [Description("Pause")]
            Pause,
            [Description("Resume")]
            Resume
        }

        /// <summary>
        /// ボタンを押して、時間のシミュレーションを実施する。
        /// </summary>
        /// <remarks>
        /// Start後Sleep(500)位しないと画面が更新されないかもしれない。<br/>
        /// 実装時に注意すること。
        /// </remarks>
        public void Action(ExecuteType type)
        {
            if (time_ is not null) {
                var guiItem = time_.FindAll(TreeScope.Children,
                   new PropertyCondition(AutomationElement.NameProperty, type.GetDescription())).Cast<AutomationElement>();
                if (guiItem.Count() == 1) {
                    if (guiItem.First().Current.IsEnabled) {
                        if (guiItem.First().TryGetCurrentPattern(InvokePattern.Pattern, out object obj)) {
                            ((InvokePattern)obj).Invoke();
                        }
                    }
                }
            }
        }
    }
}
