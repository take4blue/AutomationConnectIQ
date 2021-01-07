using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Automation;

namespace AutomationConnectIQ.Lib
{
    /// <summary>
    /// シミュレーターの操作
    /// </summary>
    public partial class Simulator
    {
        // このファイルでは設定以外のメニューでの設定を行うものを入れてある
        /// <summary>
        /// View Screen Heat Mapが選択可能かどうかを返す<br/>
        /// trueの場合Low Power ModeをTrueにするとHeat Mapが表示されてしまうので注意が必要
        /// </summary>
        public bool IsEnabledHeatMap => Win32Api.IsEnabledMenu((IntPtr)top_.Current.NativeWindowHandle, new List<string>() { "File", "View Screen Heat Map" });

        /// <summary>
        /// Activity Monitorの設定で時間を進める
        /// </summary>
        /// <param name="miniuete">進める時間(分:1～600)</param>
        /// <exception cref="ArgumentOutOfRangeException">miniueteが1-600以外</exception>
        public void FastForward(uint miniuete)
        {
            if (miniuete < 1 || miniuete > 600) {
                throw new ArgumentOutOfRangeException(miniuete.ToString());
            }
            OpenWindow(new List<string>() { "Simulation", "Activity Monitoring", "Fast Forward" }, "Fast Forward Activity Tracking", (settingWindow) =>
            {
                // 入力フィールドのスピンに値を入れる
                var guiParts = settingWindow.FindAll(TreeScope.Subtree,
                       new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Spinner)).Cast<AutomationElement>();
                if (guiParts.Count() == 1) {
                    Utility.SetSpinText(guiParts.First(), miniuete.ToString());
                    return true;
                }
                return false;
            });
        }
    }
}