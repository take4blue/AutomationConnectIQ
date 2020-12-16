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
        // このファイルでは設定関連のメニューを単独クリックするようなやつを記述
        /// <summary>
        /// 言語の設定
        /// </summary>
        public enum Language
        {
#pragma warning disable CS1591 // 公開されている型またはメンバーの XML コメントがありません
            Arabic,
            Bulgarian,
            Czech,
            Danish,
            German,
            Dutch,
            English,
            Estonian,
            Finnish,
            French,
            Greek,
            Hebrew,
            Croatian,
            Hungarian,
            Italian,
            Latvian,
            Lithuanian,
            Norwegian,
            Polish,
            Portuguese,
            Romanian,
            Russian,
            Slovak,
            Slovenian,
            Spanish,
            Swedish,
            Turkish,
            Ukrainian,
            BahasaIndonesia,
            BahasaMalaysia,
            ChineseSimplified,
            ChineseTraditional,
            Japanese,
            Korean,
            Thai,
            Vietnamese,
#pragma warning restore
        }

        /// <summary>
        /// UI言語の設定
        /// </summary>
        public bool SetLanguage(Language lang)
        {
            var menu = new List<string>() {
                "Settings", "Language"
            };
            switch (lang) {
            case Language.Arabic:
                menu.Add("Arabic (ara)");
                break;
            case Language.Bulgarian:
                menu.Add("Bulgarian (bul)");
                break;
            case Language.Czech:
                menu.Add("Czech (ces)");
                break;
            case Language.Danish:
                menu.Add("Danish (dan)");
                break;
            case Language.German:
                menu.Add("German (deu)");
                break;
            case Language.Dutch:
                menu.Add("Dutch (dut)");
                break;
            case Language.English:
                menu.Add("English (eng)");
                break;
            case Language.Estonian:
                menu.Add("Estonian (est)");
                break;
            case Language.Finnish:
                menu.Add("Finnish (fin)");
                break;
            case Language.French:
                menu.Add("French (fre)");
                break;
            case Language.Greek:
                menu.Add("Greek (gre)");
                break;
            case Language.Hebrew:
                menu.Add("Hebrew (heb)");
                break;
            case Language.Croatian:
                menu.Add("Croatian (hrv)");
                break;
            case Language.Hungarian:
                menu.Add("Hungarian (hun)");
                break;
            case Language.Italian:
                menu.Add("Italian (ita)");
                break;
            case Language.Latvian:
                menu.Add("Latvian (lav)");
                break;
            case Language.Lithuanian:
                menu.Add("Lithuanian (lit)");
                break;
            case Language.Norwegian:
                menu.Add("Norwegian (nob)");
                break;
            case Language.Polish:
                menu.Add("Polish (pol)");
                break;
            case Language.Portuguese:
                menu.Add("Portuguese (por)");
                break;
            case Language.Romanian:
                menu.Add("Romanian (ron)");
                break;
            case Language.Russian:
                menu.Add("Russian (rus)");
                break;
            case Language.Slovak:
                menu.Add("Slovak (slo)");
                break;
            case Language.Slovenian:
                menu.Add("Slovenian (slv)");
                break;
            case Language.Spanish:
                menu.Add("Spanish (spa)");
                break;
            case Language.Swedish:
                menu.Add("Swedish (swe)");
                break;
            case Language.Turkish:
                menu.Add("Turkish (tur)");
                break;
            case Language.Ukrainian:
                menu.Add("Ukrainian (ukr)");
                break;
            case Language.BahasaIndonesia:
                menu.Add("Bahasa Indonesia (ind)");
                break;
            case Language.BahasaMalaysia:
                menu.Add("Bahasa Malaysia (zsm)");
                break;
            case Language.ChineseSimplified:
                menu.Add("Chinese (Simplified) (zhs)");
                break;
            case Language.ChineseTraditional:
                menu.Add("Chinese (Traditional) (zht)");
                break;
            case Language.Japanese:
                menu.Add("Japanese (jpn)");
                break;
            case Language.Korean:
                menu.Add("Korean (kor)");
                break;
            case Language.Thai:
                menu.Add("Thai (tha)");
                break;
            case Language.Vietnamese:
                menu.Add("Vietnamese (vie)");
                break;
            }

            return Utility.ActionMenu(top_, menu);
        }

        /// <summary>
        /// BLE/WIFIの接続タイプ
        /// </summary>
        public enum ConnectionType
        {
#pragma warning disable CS1591 // 公開されている型またはメンバーの XML コメントがありません
            [Description("Not Initialized")]
            NotInitialized,
            [Description("Not Connected")]
            NotConnected,
            [Description("Connected")]
            Connected,
#pragma warning restore
        }

        /// <summary>
        /// BLEの接続タイプの設定
        /// </summary>
        /// <param name="type"></param>
        public void SetBleConnection(ConnectionType type)
        {
            var menu = new List<string>() {
                "Settings", "Connection Type", "BLE", type.GetDescription()
            };
            Utility.ActionMenu(top_, menu);
        }

        /// <summary>
        /// WIFIの接続タイプの設定
        /// </summary>
        /// <param name="type"></param>
        public void SetWiFiConnection(ConnectionType type)
        {
            var menu = new List<string>() {
                "Settings", "Connection Type", "WiFi (Bulk Download/Media Sync)", type.GetDescription()
            };
            Utility.ActionMenu(top_, menu);
        }

        /// <summary>
        /// WIFIステータス
        /// </summary>
        public enum WiFiStatus
        {
#pragma warning disable CS1591 // 公開されている型またはメンバーの XML コメントがありません
            [Description("Avaliable")]
            Avaliable,
            [Description("Low Battery")]
            LowBattery,
            [Description("No Access Points")]
            NoAccessPoints,
            [Description("Unsupported")]
            Unsupported,
            [Description("User Disabled")]
            UserDisabled,
            [Description("Battery Saver Active")]
            BatterySaverActive,
            [Description("Stealth Mode Active")]
            StealthModeActive,
            [Description("Airplane Mode Active")]
            AirplaneModeActive,
            [Description("Powered Down")]
            PoweredDown,
            [Description("Unknown")]
            Unknown,
#pragma warning restore
        }

        /// <summary>
        /// WIFIステータスの設定
        /// </summary>
        public void SetWiFiStatus(WiFiStatus type)
        {
            var menu = new List<string>() {
                "Settings", "Connection Type", "WiFi (Bulk Download/Media Sync)", "Status", type.GetDescription()
            };
            Utility.ActionMenu(top_, menu);
        }

        /// <summary>
        /// 時間表記の設定
        /// </summary>
        /// <param name="is24Type">trueの場合24時間表記、falseの場合は12時間表記</param>
        public void SetDisplayHourType(bool is24Type)
        {
            var menu = new List<string>() {
                "Settings", "Time Display"
            };
            if (is24Type) {
                menu.Add("24 Hour");
            }
            else {
                menu.Add("12 Hour");
            }
            Utility.ActionMenu(top_, menu);
        }

        /// <summary>
        /// 表記単位の設定
        /// </summary>
        /// <param name="isMetric">trueの場合メートル表記、falseの場合はマイル表記</param>
        public void SetDisplayUnit(bool isMetric)
        {
            var menu = new List<string>() {
                "Settings", "Units Display"
            };
            if (isMetric) {
                menu.Add("Metric");
            }
            else {
                menu.Add("Statute");
            }
            Utility.ActionMenu(top_, menu);
        }

        /// <summary>
        /// 週頭とする曜日の設定
        /// </summary>
        /// <param name="type">曜日の指定は土曜日、日曜日、月曜日のみ</param>
        /// <exception cref="ArgumentException">指定された曜日が範囲外の場合</exception>
        public void SetFirstDayWeek(DayOfWeek type)
        {
            var menu = new List<string>() {
                "Settings", "First Day of Week"
            };
            switch (type) {
            case DayOfWeek.Saturday:
                menu.Add("Saturday");
                break;
            case DayOfWeek.Sunday:
                menu.Add("Sunday");
                break;
            case DayOfWeek.Monday:
                menu.Add("Monday");
                break;
            default:
                throw new ArgumentException(type.ToString());
            }
            Utility.ActionMenu(top_, menu);
        }

        /// <summary>
        /// 受け付ける通知種類
        /// </summary>
        public enum ReceiveNotificationType
        {
#pragma warning disable CS1591 // 公開されている型またはメンバーの XML コメントがありません
            [Description("Receive All")]
            All,
            [Description("Receive Actionable")]
            Actionable,
            [Description("Receive Actionable on LTE")]
            ActionableLTE,
            [Description("Receive None")]
            None
#pragma warning restore
        }

        /// <summary>
        /// 受け取る通知種類を設定する
        /// </summary>
        public void SetReceiveNotificationType(ReceiveNotificationType type)
        {
            var menu = new List<string>() {
                "Settings", "Push Notifications", type.GetDescription()
            };
            Utility.ActionMenu(top_, menu);
        }
        // ウィンドウに設定するタイプの項目を記述
        /// <summary>
        /// GPSの品質種類
        /// </summary>
        public enum GPSQualityType
        {
#pragma warning disable CS1591 // 公開されている型またはメンバーの XML コメントがありません
            [Description("Not Available")]
            NotAvailable,
            [Description("Last Known")]
            LastKnown,
            [Description("Poor")]
            Poor,
            [Description("Usable")]
            Usable,
            [Description("Good")]
            Good
#pragma warning restore
        }

        /// <summary>
        /// GPSの品質種類を設定する
        /// </summary>
        public void SetGPSQuality(GPSQualityType type)
        {
            OpenSettingWindow("Set GPS Quality", "Select a GPS quality level", (settingWindow) =>
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
        /// 通知数を設定する
        /// </summary>
        /// <param name="num">通知数(0-20)</param>
        /// <exception cref="ArgumentOutOfRangeException">numが0-20以外</exception>
        public void SetNotificationCount(int num)
        {
            if (num < 0 || num > 20) {
                throw new ArgumentOutOfRangeException(num.ToString());
            }
            OpenSettingWindow("Set Notification Countt", "Set number of notifications", (settingWindow) =>
            {
                return SetSpinValue(settingWindow, num, "Number notifications:");
            });
        }

        /// <summary>
        /// アラームの数を設定する
        /// </summary>
        /// <param name="num">アラーム(0-3)</param>
        /// <exception cref="ArgumentOutOfRangeException">numが0-3以外</exception>
        public void SetAlarmCount(int num)
        {
            if (num < 0 || num > 3) {
                throw new ArgumentOutOfRangeException(num.ToString());
            }
            OpenSettingWindow("Set Alarm Count", "Set number of alarms", (settingWindow) =>
            {
                return SetSpinValue(settingWindow, num, "Number alarms:");
            });
        }

        /// <summary>
        /// バッテリーの充電状態、充電率を設定する
        /// </summary>
        public void SetBatteryStatus(double chargingRate, bool isCharging)
        {
            if (chargingRate < 0.0 || chargingRate > 100.0) {
                throw new ArgumentOutOfRangeException(chargingRate.ToString());
            }
            OpenSettingWindow("Set Battery Status", "Set Battery Status", (settingWindow) =>
            {
                // 入力フィールドのスピンに値を入れる
                bool result = false;
                var guiParts = settingWindow.FindAll(TreeScope.Subtree,
                       new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Spinner)).Cast<AutomationElement>();
                if (guiParts.Count() == 1) {
                    Utility.SetSpinText(guiParts.First(), chargingRate.ToString());
                    result = true;
                }
                if (result) {
                    guiParts = settingWindow.FindAll(TreeScope.Subtree,
                         new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.CheckBox)).Cast<AutomationElement>();
                    if (guiParts.Count() == 1) {
                        if (guiParts.First().TryGetCurrentPattern(TogglePattern.Pattern, out object obj)) {
                            var toggle = obj as TogglePattern;
                            if ((toggle.Current.ToggleState == ToggleState.Off && isCharging) || (toggle.Current.ToggleState == ToggleState.On && !isCharging)) {
                                toggle.Toggle();
                            }
                            result = true;
                        }
                    }
                }

                return result;
            });
        }

        /// <summary>
        /// BLEの接続ポートを設定する
        /// </summary>
        /// <returns>ポートに接続できない場合false<br/>ただし同じポート名を連続して入力した場合、2回目はtrueになってしまうので注意</returns>
        public bool SetBleSettings(string portName)
        {
            OpenSettingWindow("BLE Settings", "Set Nordic COM Port", (settingWindow) =>
            {
                var guiParts = settingWindow.FindAll(TreeScope.Subtree, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit)).Cast<AutomationElement>();
                if (guiParts.Count() == 1) {
                    Utility.SetText(guiParts.First(), portName, false);
                    return true;
                }
                else {
                    return false;
                }
            });

            Thread.Sleep(200);

            var confirm = top_.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Confirm")).Cast<AutomationElement>();
            if (confirm.Count() == 1) {
                if (confirm.First().TryGetCurrentPattern(WindowPattern.Pattern, out object obj)) {
                    ((WindowPattern)obj).Close();
                }
                return false;
            }
            else {
                return true;
            }
        }

        /// <summary>
        /// GPS座標を設定する。<br/>
        /// 座標値は、google mapでクリックした時に画面下に出てくる2つの実数値をそのまま設定すればいいようにしてある
        /// </summary>
        public void SetGPSPosition(double latitude, double longitude)
        {
            OpenSettingWindow("Set Position", "Set current position", (settingWindow) =>
            {
                var guiParts = settingWindow.FindAll(TreeScope.Subtree, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit)).Cast<AutomationElement>();
                if (guiParts.Count() == 1) {
                    Utility.SetText(guiParts.First(), $"{latitude},{longitude}", false);
                    return true;
                }
                else {
                    return false;
                }
            });
        }

        /// <summary>
        /// Settings->Force on????の設定
        /// </summary>
        /// <param name="toHide">onHideを選択する場合true、onShowを選択する場合はfalse</param>
        /// <returns>選択できなかった場合false</returns>
        public bool SetForceOn(bool toHide)
        {
            var menu = new List<string>() {
                "Settings"
            };
            if (toHide) {
                menu.Add("Force onHide");
            }
            else {
                menu.Add("Force onShow");
            }
            return Utility.ActionMenu(top_, menu);
        }

        /// <summary>
        /// On/Offするトグルメニュー
        /// </summary>
        public enum SettingToggleMenu
        {
#pragma warning disable CS1591 // 公開されている型またはメンバーの XML コメントがありません
            [Description("Tones")]
            Tones,
            [Description("Vibrate")]
            Vibrate,
            [Description("Sleep Mode")]
            SleepMode,
            [Description("Activity Tracking")]
            ActivityTracking,
            [Description("Do Not Disturb")]
            DoNotDisturb,
            [Description("Use Device HTTPS Requirements")]
            UseDeviceHTTPSRequirements,
            [Description("Low Power Mode")]
            LowPowerMode,
            [Description("App Lock Enabled")]
            AppLockEnabled,
            [Description("Enable Alert")]
            EnableAlert,
#pragma warning restore
        }

        /// <summary>
        /// メニューのON/OFFを設定する<br/>
        /// まだ未完成
        /// </summary>
        public void ToggleMenu(SettingToggleMenu type, bool turnOn)
        {
            var menu = new List<string>();
            if (type == SettingToggleMenu.AppLockEnabled) {
                menu.Add("Simulation");
            }
            else if (type == SettingToggleMenu.EnableAlert) {
                menu.Add("Data Fields");
            }
            else {
                menu.Add("Settings");
            }
            menu.Add(type.GetDescription());
            Utility.ToggleMenu(top_, menu, turnOn);
        }
    }
}