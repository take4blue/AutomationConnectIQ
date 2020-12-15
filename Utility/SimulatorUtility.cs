using System;
using System.Collections.Generic;
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
        /// 設定画面内で処理をするためのデリゲート
        /// </summary>
        /// <param name="windowMain">設定画面に関する情報</param>
        /// <returns>設定が完了したらtrueとする</returns>
        private delegate bool WindowSetting(AutomationElement windowMain);

        /// <summary>
        /// メニューを押しモーダルウィンドウを表示する。<br/>
        /// ウィンドウ内の処理はfuncに委譲している。
        /// </summary>
        /// <param name="menuName">選択するメニュー</param>
        /// <param name="windowName">設定ウィンドウ名</param>
        /// <param name="func">設定ウィンドウ内の処理関数</param>
        private void OpenWindow(List<string> menuNames, string windowName, WindowSetting func)
        {
            if (Utility.ActionMenu(top_, menuNames)) {
                // メニュー選択後、品質画面の中から選択するものを探し出し、それをクリック後、OKボタンを押す
                var settingWindow = top_.FindAll(TreeScope.Children | TreeScope.Element,
                        new AndCondition(
                            new PropertyCondition(AutomationElement.NameProperty, windowName),
                            new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window))).Cast<AutomationElement>();
                if (settingWindow.Count() == 1) {
                    string selectList;
                    if (func(settingWindow.First())) {
                        selectList = "OK";
                    }
                    else {
                        selectList = "Cancel";
                    }
                    var guiParts = Utility.FindElementsByName(settingWindow.First(), selectList);
                    if (guiParts.Count() == 1) {
                        if (guiParts.First().TryGetCurrentPattern(InvokePattern.Pattern, out object obj)) {
                            ((InvokePattern)obj).Invoke();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 設定からのウィンドウを開く
        /// </summary>
        private void OpenSettingWindow(string menuName, string windowName, WindowSetting func)
        {
            List<string> menus = new List<string>() { "Settings", menuName };
            OpenWindow(menus, windowName, func);
        }

        /// <summary>
        /// スピンへの値の設定
        /// </summary>
        /// <param name="settingWindow">設定画面</param>
        /// <param name="value">設定値</param>
        /// <param name="spinName">スピンの名前</param>
        /// <returns></returns>
        private bool SetSpinValue(AutomationElement settingWindow, int value, string spinName)
        {
            // 入力フィールドのスピンに値を入れる
            var guiParts = settingWindow.FindAll(TreeScope.Subtree,
                    new AndCondition(
                        new PropertyCondition(AutomationElement.NameProperty, spinName),
                        new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Spinner))).Cast<AutomationElement>();
            if (guiParts.Count() == 1) {
                Utility.SetText(guiParts.First(), value, false);
                return true;
            }
            else {
                return false;
            }
        }

        /// <summary>
        /// デバイスが立ち上がるまで待つ
        /// </summary>
        public void WaitForDeviceStart()
        {

            for (int i = 0; i < 10; i++) {
                var guiParts = top_.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "panel")).Cast<AutomationElement>();
                if (guiParts.Count() == 1) {
                    return;
                }
                Thread.Sleep(100);
            }
            return;
        }
    }
}