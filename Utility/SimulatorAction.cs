using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        /// ゴール通知の種類
        /// </summary>
        public enum GoalType
        {
#pragma warning disable CS1591 // 公開されている型またはメンバーの XML コメントがありません
            [Description("Steps")]
            Steps,
            [Description("Floors Climbed")]
            FloorClimbed,
            [Description("Active Minutes")]
            ActiveMinutes
#pragma warning restore
        }

        /// <summary>
        /// ゴールの通知
        /// </summary>
        public void TriggerGoal(GoalType type)
        {
            OpenWindow(new List<string>() { "Settings", "Trigger Goal" }, "Select a Goal type", (settingWindow) =>
            {
                // 選択するものを選んだ後はSelectionItemPattern.Selectを使う
                var guiParts = Utility.FindElementsByName(settingWindow, type.GetDescription());
                if (guiParts.Count() == 1) {
                    if (guiParts.First().TryGetCurrentPattern(SelectionItemPattern.Pattern, out object obj)) {
                        ((SelectionItemPattern)obj).Select();
                        return true;
                    }
                }
                return false;
            });
        }

        /// <summary>
        /// Data Fields-Timerの設定値
        /// </summary>
        public enum Activity
        {
#pragma warning disable CS1591 // 公開されている型またはメンバーの XML コメントがありません
            [Description("Discard Activity")]
            Discard,
            [Description("Save Activity")]
            Save,
            [Description("Pause Activity")]
            Pause,
            [Description("Resume Activity")]
            Resume,
            [Description("Start Activity")]
            Start,
            [Description("Lap Activity")]
            Lap,
            [Description("Workout Step")]
            WorkoutStep,
            [Description("Next Multisport")]
            NextMultisport,
#pragma warning restore
        }

        /// <summary>
        /// Data Fields-Timerのメニュークリック
        /// </summary>
        public void SetTimerActivity(Activity type)
        {
            List<string> menus = new List<string>()
            {
                "Data Fields", "Timer", type.GetDescription()
            };
            Utility.ActionMenu(top_, menus);
        }

        /// <summary>
        /// Data Fields-Timerのメニューの状態
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool IsEnableTimerActivity(Activity type)
        {
            List<string> menus = new List<string>()
            {
                "Data Fields", "Timer", type.GetDescription()
            };
            bool result = false;
            Utility.ActionMenu(top_, menus, (x, y, z) =>
            {
                result = x.Current.IsEnabled;
                return true;
            });
            return result;
        }

        /// <summary>
        /// Data Fieldsの背景色
        /// </summary>
        public enum BackgroundColor
        {
#pragma warning disable CS1591 // 公開されている型またはメンバーの XML コメントがありません
            [Description("White")]
            White,
            [Description("Black")]
            Black
#pragma warning restore
        }

        /// <summary>
        /// Data Fieldsの背景色の設定
        /// </summary>
        public void SetBackgroundColor(BackgroundColor type)
        {
            List<string> menus = new List<string>()
            {
                "Data Fields", "Background Color", type.GetDescription()
            };
            Utility.ActionMenu(top_, menus);
        }
    }
}