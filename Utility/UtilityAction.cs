using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Automation;
using System.Windows.Forms;

namespace AutomationConnectIQ.Lib
{
    internal static partial class Utility
    {
        #region コンビニエンス関数
        // TryGetCurrentPatternの使用例みたいな感じ
        /// <summary>
        /// ボタンクリック
        /// </summary>
        static public void PushButton(AutomationElement target)
        {
            if (target.TryGetCurrentPattern(InvokePattern.Pattern, out object result)) {
                ((InvokePattern)result).Invoke();
            }
        }

        /// <summary>
        /// メニューの展開
        /// </summary>
        static public void ExpandMenu(AutomationElement target)
        {
            if (target.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out object result)) {
                ((ExpandCollapsePattern)result).Expand();
            }
        }

        public delegate bool DelegateMenuAction(AutomationElement menu, AutomationElement parent, uint pos);

        /// <summary>
        /// メニューのクリック操作
        /// </summary>
        /// <remarks>メニュー文字がおかしかったり、メニューがenableだった場合falseでリターンする</remarks>
        /// <param name="top">アプリケーションのトップ</param>
        /// <param name="menus">クリックするメニュー<br/>階層構造を考慮しListで指定する</param>
        /// <returns>クリックできたらtrue</returns>
        static public bool ActionMenu(AutomationElement top, List<string> menus)
        {
            return ActionMenu(top, menus, (x, y, z) => {
                if (x.TryGetCurrentPattern(InvokePattern.Pattern, out object obj)) {
                    ((InvokePattern)obj).Invoke();
                    return true;
                }
                return false;
            });
        }

        /// <summary>
        /// トグル形式のメニューのON/OFFを制御する
        /// </summary>
        /// <param name="top">アプリケーションのトップ</param>
        /// <param name="menus">クリックするメニュー<br/>階層構造を考慮しListで指定する</param>
        /// <returns>選択できるメニューだったらtrue</returns>
        static public bool ToggleMenu(AutomationElement top, List<string> menus, bool turnOn)
        {
            // menusからmenuを特定しそれのチェック状態を判断してからInvokeを実施する。
            if (Win32Api.GetMenuStatus((IntPtr)top.Current.NativeWindowHandle, menus) != turnOn) {
                return ActionMenu(top, menus, (x, parent, pos) =>
                {
                    if (x.TryGetCurrentPattern(InvokePattern.Pattern, out object obj)) {
                        // 現在のチェック状態と指定されているチェック状態が異なる場合のみInvokeを実行する
                        ((InvokePattern)obj).Invoke();
                        return true;
                    }
                    return false;
                });
            }
            return false;
        }

        /// <summary>
        /// メニューの操作
        /// </summary>
        /// <remarks>メニュー文字がおかしかったり、メニューがenableだった場合falseでリターンする</remarks>
        /// <param name="top">アプリケーションのトップ</param>
        /// <param name="menus">クリックするメニュー<br/>階層構造を考慮しListで指定する</param>
        /// <param name="func">展開できないメニューの場合の処理関数</param>
        /// <returns>処理で着たらtrue</returns>
        static public bool ActionMenu(AutomationElement top, List<string> menus, DelegateMenuAction func)
        {
            // トップメニューを検索
            var children = top.FindAll(TreeScope.Children, Condition.TrueCondition).Cast<AutomationElement>();
            AutomationElement menuParent = null;
            foreach (var obj in children) {
                if (obj.Current.ControlType == ControlType.MenuBar) {
                    menuParent = obj;
                }
            }
            if (menuParent is null) {
                // トップメニューがない場合エラー
                return false;
            }


            // メニューの子供を見つけ、expand可能ならとりあえず展開しておく
            foreach (var menu in menus) {
                var items = menuParent.FindAll(TreeScope.Children,
                    new OrCondition(
                        new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.MenuItem),
                        new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Separator))).Cast<AutomationElement>();
                uint i = 0;
                foreach (var item in items) {
                    if (item.Current.Name == menu) {
                        if (!item.Current.IsEnabled) {
                            // メニューが選択不可の場合
                            return false;
                        }
                        if (item.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out object obj)) {
                            // メニューを展開し、ポップアップされたメニューウィンドウを検索する
                            var collapse = obj as ExpandCollapsePattern;
                            try {
                                ((ExpandCollapsePattern)obj).Expand();
                            }
                            catch (InvalidOperationException) {
                                return false;
                            }
                            children = top.FindAll(TreeScope.Descendants, new PropertyCondition(AutomationElement.NameProperty, menu)).Cast<AutomationElement>();
                            foreach (var obj1 in children) {
                                if (obj1.Current.ControlType == ControlType.Menu) {
                                    menuParent = obj1;
                                }
                            }
                        }
                        else if (func(item, menuParent, i)) {
                            return true;
                        }
                    }
                    i++;
                }
            }
            return false;
        }
        #endregion

        /// <summary>
        /// 指定されたAutomationElementにキーボードを叩いた体で文字列を設定する
        /// </summary>
        /// <remarks>
        /// https://docs.microsoft.com/ja-jp/dotnet/framework/ui-automation/add-content-to-a-text-box-using-ui-automation
        /// </remarks>
        /// <param name="focus">設定するためにフォーカスをうつす場合true</param>
        static public void SetText(AutomationElement element, string text, bool focus = true)
        {
            if (focus) {
                element.SetFocus();
            }

            if (!element.TryGetCurrentPattern(ValuePattern.Pattern, out object valuePattern)) {
                Thread.Sleep(200);
                SendKeys.SendWait(text);
                Thread.Sleep(200);
            }
            else {
                ((ValuePattern)valuePattern).SetValue(text);
            }
        }

        /// <summary>
        /// 指定されたAutomationElementにキーボードを叩いた体で文字列を設定する
        /// </summary>
        /// <remarks>
        /// https://docs.microsoft.com/ja-jp/dotnet/framework/ui-automation/add-content-to-a-text-box-using-ui-automation
        /// </remarks>
        /// <param name="focus">設定するためにフォーカスをうつす場合true</param>
        static public void SetText(AutomationElement element, double value, bool focus = true)
        {
            if (focus) {
                element.SetFocus();
            }

            if (element.TryGetCurrentPattern(RangeValuePattern.Pattern, out object valuePattern)) {
                ((RangeValuePattern)valuePattern).SetValue(value);
            }
            else {
                SetText(element, value.ToString(), focus);
            }
        }

        /// <summary>
        /// 指定されたAutomationElementにキーボードを叩いた体で文字列を設定する
        /// </summary>
        /// <remarks>
        /// https://docs.microsoft.com/ja-jp/dotnet/framework/ui-automation/add-content-to-a-text-box-using-ui-automation
        /// </remarks>
        /// <param name="focus">設定するためにフォーカスをうつす場合true</param>
        static public void SetText(AutomationElement element, int value, bool focus = true)
        {
            if (focus) {
                element.SetFocus();
            }

            if (element.TryGetCurrentPattern(RangeValuePattern.Pattern, out object valuePattern)) {
                ((RangeValuePattern)valuePattern).SetValue((double)value);
            }
            else {
                SetText(element, value.ToString(), focus);
            }
        }

        /// <summary>
        /// スピンに登録されているテキストエリアに直接値を入力するための処理<br/>
        /// スピンとテキストエリアがバディになっていないものがあり、それに対応するため、スピンの前のものがテキストとして仮定し、そこに文字列を入れていく。
        /// </summary>
        static public void SetSpinText(AutomationElement element, string value)
        {
            var handle = (int)element.GetCurrentPropertyValue(AutomationElement.NativeWindowHandleProperty);
            var prevHandle = Win32Api.GetWindow((IntPtr)handle, Win32Api.GW_HWNDPREV);
            Win32Api.SetFocus(prevHandle);
            Thread.Sleep(200);
            SendKeys.SendWait(value);
            Thread.Sleep(200);
        }

        /// <summary>
        /// 要素に対して日付を設定する
        /// </summary>
        /// <remarks>
        /// 日付の設定に関しては以下のURLを参照
        /// <br/>
        /// https://stackoverflow.com/questions/5036776/update-datetimepicker-in-another-process-by-dtm-setsystemtime
        /// <br/>
        /// 他のプロセスに対してDTM_SETSYSTEMTIMEを行う場合、そのプロセス空間でのメモリ領域を確保し送信しないといけないようだ
        /// </remarks>
        /// <param name="target">設定する要素</param>
        /// <param name="date">日付</param>
        public static void SetDate(AutomationElement target, DateTime date)
        {
            var value = new Win32Api.SYSTEMTIME()
            {
                wYear = (short)date.Year,
                wMonth = (short)date.Month,
                wDayOfWeek = 0,
                wDay = (short)date.Day,
                wHour = (short)date.Hour,
                wMinute = (short)date.Minute,
                wSecond = (short)date.Second,
                wMilliseconds = 0
            };
            Win32Api.SendMessage((IntPtr)target.Current.NativeWindowHandle, (IntPtr)target.Current.ProcessId, Win32Api.DTM_SETSYSTEMTIME, 0, value);
        }
    }
}
