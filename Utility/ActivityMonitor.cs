using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Automation;
using System.ComponentModel;


namespace AutomationConnectIQ.Lib
{
    public class ActivityMonitor
    {
        /// <summary>
        /// シミュレーターのトップ要素
        /// </summary>
        readonly AutomationElement top_;

        /// <summary>
        /// Activity Monitorウィンドウのトップ要素
        /// </summary>
        AutomationElement monitor_ = null;

        IEnumerable<AutomationElement> grid_;

        internal ActivityMonitor(AutomationElement top)
        {
            top_ = top;
        }

        private void OpenCheck()
        {
            var guiItem = top_.FindAll(TreeScope.Children,
                new PropertyCondition(AutomationElement.NameProperty, "Edit Activity Monitor Info")).Cast<AutomationElement>();
            if (guiItem.Count() == 1) {
                monitor_ = guiItem.First();

                grid_ = monitor_.FindAll(TreeScope.Descendants,
                    new PropertyCondition(AutomationElement.NameProperty, "GridWindow")).Cast<AutomationElement>();
                if (grid_.Count() != 2) {
                    grid_ = null;
                }
            }
        }

        /// <summary>
        /// Activity Monitorウィンドウを開く<br/>
        /// すでに開いている場合それを流用するようにしている<br/>
        /// ウィンドウを閉じる場合は、Ok/Cancelのどちらかのメソッドを呼ぶ
        /// </summary>
        public void Open()
        {
            if (monitor_ is null) {
                OpenCheck();
                if (monitor_ is not null) {
                    monitor_.SetFocus();
                }
            }
            if (monitor_ is null) {
                List<string> menu = new List<string>()
                {
                    "Simulation", "Activity Monitoring", "Set Activity Monitor Info"
                };
                if (Utility.ActionMenu(top_, menu)) {
                    for (int i = 0; i < 10 && monitor_ is null; i++) {
                        OpenCheck();
                        Thread.Sleep(10);
                    }
                }
            }
        }

        /// <summary>
        /// 値を設定し、Activity Monitorウィンドウを閉じる
        /// </summary>
        public void Ok()
        {
            if (monitor_ is not null) {
                var guiItem = monitor_.FindAll(TreeScope.Children,
                        new AndCondition(
                            new PropertyCondition(AutomationElement.NameProperty, "OK"),
                            new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button))).Cast<AutomationElement>();
                if (guiItem.Any()) {
                    if (guiItem.First().TryGetCurrentPattern(InvokePattern.Pattern, out object obj)) {
                        ((InvokePattern)obj).Invoke();
                        monitor_ = null;
                        grid_ = null;
                    }
                }
            }
        }

        /// <summary>
        /// 値を設定せず、Activity Monitorウィンドウを閉じる
        /// </summary>
        public void Cancel()
        {
            if (monitor_ is not null) {
                var guiItem = monitor_.FindAll(TreeScope.Children,
                        new AndCondition(
                            new PropertyCondition(AutomationElement.NameProperty, "Cancel"),
                            new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button))).Cast<AutomationElement>();
                if (guiItem.Any()) {
                    if (guiItem.First().TryGetCurrentPattern(InvokePattern.Pattern, out object obj)) {
                        ((InvokePattern)obj).Invoke();
                        monitor_ = null;
                        grid_ = null;
                    }
                }
            }
        }

        /// <summary>
        /// グリッドへの値の設定
        /// </summary>
        /// <remarks>
        /// Today-Stepsを指定した値にしたい場合、($true,0,0,1)の設定をした後、Simulator.FastForwardを指定歩数になるまで繰り返し呼び出すのがいい<br/>
        /// ($true,0,0,300)と最大値を指定した場合、Okの後秒単位にステップ数が増えていくため、Today-Stepsを指定した値にするのは困難</remarks>
        /// <param name="top">上側(今日)のグリッドを設定する場合true</param>
        /// <param name="row">行位置<br/>
        /// topがtrueの場合0-1ただし1を指定しても値は設定されない、falseの場合0-6</param>
        /// <param name="column">列位置<br/>0-9</param>
        /// <param name="value">設定する値<br/>範囲チェックはやっていない</param>
        public void SetValue(bool top, uint row, uint column, uint value)
        {
            if (grid_ is not null) {
                uint maxColumn = 10;
                uint maxRow = 2;
                AutomationElement target;
                if (top) {
                    target = grid_.Last();
                }
                else {
                    target = grid_.First();
                    maxRow = 7;
                }
                if (row > maxRow || column > maxColumn) {
                    return;
                }

                var rect = (System.Windows.Rect)target.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty);
                var clickX = rect.Width / (double)maxColumn * ((double)column + 0.5) + rect.Left;
                var clickY = rect.Height / (double)maxRow * ((double)row + 0.5) + rect.Top;
                for (int i = 0; i < 2; i++) {
                    Win32Api.Click((int)clickX, (int)clickY);
                    Thread.Sleep(100);
                    var edit = target.FindAll(TreeScope.Children, Condition.TrueCondition).Cast<AutomationElement>();
                    if (edit.Any()) {
                        if (edit.First().TryGetCurrentPattern(ValuePattern.Pattern, out object valuePattern)) {
                            ((ValuePattern)valuePattern).SetValue(value.ToString());
                        }
                        else {
                            Utility.SetSpinText(edit.First(), value.ToString());
                        }
                        Thread.Sleep(100);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// ActiveMinutesGoalの設定
        /// </summary>
        public void SetActiveMinutesGoal(uint value)
        {
            if (monitor_ is not null) {
                var edit = monitor_.FindAll(TreeScope.Children | TreeScope.Element,
                        new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit)).Cast<AutomationElement>();
                if (edit.Any()) {
                    if (edit.First().TryGetCurrentPattern(ValuePattern.Pattern, out object valuePattern)) {
                        ((ValuePattern)valuePattern).SetValue(value.ToString());
                    }
                }
            }
        }
    }
}
