using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Automation;

namespace AutomationConnectIQ.Lib
{
    internal static partial class Utility
    {
        private const int MaxRetryNum = 1000;

        /// <summary>
        /// プロセスに対してのオートメーション要素を取得する
        /// </summary>
        static public AutomationElement FindRootElement(Process p)
        {
            return AutomationElement.FromHandle(p.MainWindowHandle);
        }

        /// <summary>
        /// 指定したタイトルを持つプロセスを引っ張ってくる
        /// </summary>
        /// <remarks>
        /// UWPなどのプロセスの場合、アプリケーション起動直後ではタイトルを持つプロセスを見つけることができない場合があるので、スリープを入れてリトライするようにしている
        /// </remarks>
        /// <param name="title">タイトル文字</param>
        /// <param name="maxRetry">最大試行回数</param>
        /// <returns>プロセス情報</returns>
        static public Process FindTitleProcess(string title, int maxRetry = MaxRetryNum)
        {
            for (int i = 0; i < maxRetry; i++) {
                foreach (Process p in Process.GetProcesses()) {
                    if (p.MainWindowTitle.Contains(title)) {
                        return p;
                    }
                }
                Thread.Sleep(10);
            }
            return null;
        }

        /// <summary>
        /// automationIdに一致するAutomationElementを取得する
        /// </summary>
        /// <remarks>
        /// WPFが起動直後は部品の生成がされていないケースがあるため、リトライする機能が設けられている
        /// </remarks>
        /// <param name="root">探索元</param>
        /// <param name="id">部品のID</param>
        static public AutomationElement FindElementById(AutomationElement root, string id)
        {
            for (int i = 0; i < MaxRetryNum; i++) {
                var result = root.FindFirst(TreeScope.Element | TreeScope.Descendants,
                    new PropertyCondition(AutomationElement.AutomationIdProperty, id));

                if (result is not null) {
                    return result;
                }
                Thread.Sleep(10);
            }
            return null;
        }

        /// <summary>
        /// 指定された名前に一致するAutomationElement達をIEnumerableで取得する
        /// </summary>
        /// <remarks>
        /// WPFが起動直後は部品の生成がされていないケースがあるため、リトライする機能が設けられている
        /// </remarks>
        /// <param name="root">探索元</param>
        /// <param name="name">部品の名前</param>
        static public IEnumerable<AutomationElement> FindElementsByName(AutomationElement root, string name)
        {
            for (int i = 0; i < MaxRetryNum; i++) {
                var result = root.FindAll(TreeScope.Element | TreeScope.Descendants,
                    new PropertyCondition(AutomationElement.NameProperty, name))
                    .Cast<AutomationElement>();
                if (result is not null) {
                    return result;
                }
                Thread.Sleep(10);
            }
            return null;
        }

    }
}
