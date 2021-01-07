using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Drawing;

namespace AutomationConnectIQ.Lib
{
    /// <summary>
    /// Win32APIのラッパー用管理クラス
    /// </summary>
    internal static class Win32Api
    {
        // UI automation系以外に、Win32APIも使いますのでその宣言。 
        [DllImport("USER32.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern void SetCursorPos(int X, int Y);
        [DllImport("USER32.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = false)]
        public static extern int SendMessage(IntPtr hWnd, int msg, int Param, string s);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern IntPtr SetFocus(IntPtr hWnd);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern uint GetMenuState(IntPtr hMenu, uint uItem, uint uFlags);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern IntPtr GetMenu(IntPtr hWnd);
        [DllImport("kernel32")]
        public static extern int CloseHandle(IntPtr hPort);
        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);
        [DllImport("User32.dll")]
        public extern static bool PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags);
        [DllImport("User32.dll")]
        public extern static IntPtr GetParent(IntPtr hWnd);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumChildWindows(IntPtr handle, EnumWindowsDelegate enumProc, IntPtr lParam);
        public delegate bool EnumWindowsDelegate(IntPtr hWnd, IntPtr lparam);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowInfo(IntPtr hwnd, ref WINDOWINFO pwi);

        [DllImport("kernel32")]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, int bInheritHandle, uint dwProcessId);
        [DllImport("kernel32")]
        private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, int flAllocationType, int flProtect);
        [DllImport("kernel32")]
        private static extern int WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [In] byte[] lpBuffer, uint nSize, out uint lpNumberOfBytesWritten);
        [DllImport("User32.dll")]
        private extern static bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, int dwFreeType);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private extern static int GetMenuString(IntPtr hMenu, uint uIDItem, [Out] StringBuilder lpString, int nMaxCount, uint uFlag);
        [DllImport("user32.dll")]
        private extern static int GetMenuItemCount(IntPtr hMenu);
        [DllImport("user32.dll")]
        private extern static IntPtr GetSubMenu(IntPtr hMenu, int nPos);

        //　マウスイベント
        //　定義は以下に
        //  https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-mouse_event
        //
        public const int MOUSEEVENTF_LEFTDOWN = 0x2;
        public const int MOUSEEVENTF_LEFTUP = 0x4;

        public const uint GW_HWNDPREV = 3;
        public const int WM_SETTEXT = 0x000c;
        public const int EM_SETMODIFY = 0x00b9;

        public const int GDT_VALID = 0;
        public const int DTM_GETSYSTEMTIME = (0x1000 + 1);
        public const int DTM_SETSYSTEMTIME = (0x1000 + 2);

        public const int MEM_COMMIT = 0x00001000;
        public const int MEM_RESERVE = 0x00002000;
        public const int PAGE_EXECUTE_READWRITE = 0x40;
        public const int MEM_DECOMMIT = 0x00004000;
        public const int MEM_RELEASE = 0x00008000;

        public const uint MF_CHECKED = 0x00000008;
        public const uint MF_DISABLED = 0x00000002;
        public const uint MF_GRAYED = 0x00000001;

        public const uint MF_BYPOSITION = 0x00000400;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct SYSTEMTIME
        {
            public short wYear;
            public short wMonth;
            public short wDayOfWeek;
            public short wDay;
            public short wHour;
            public short wMinute;
            public short wSecond;
            public short wMilliseconds;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWINFO
        {
            public uint cbSize;
            public RECT rcWindow;
            public RECT rcClient;
            public int dwStyle;
            public int dwExStyle;
            public int dwWindowStatus;
            public uint cxWindowBorders;
            public uint cyWindowBorders;
            public short atomWindowType;
            public short wCreatorVersion;

            public WINDOWINFO(bool? filler)
             : this()   // Allows automatic initialization of "cbSize" with "new WINDOWINFO(null/true/false)".
            {
                cbSize = (UInt32)(Marshal.SizeOf(typeof(WINDOWINFO)));
            }

        }

        /// <summary>
        /// 相手側のプロセスのメモリ空間に領域を確保しそこに情報を書き込む
        /// </summary>
        /// <param name="processid">相手プロセス</param>
        /// <param name="buffer">コピー元の情報</param>
        /// <param name="lpAddress">確保したメモリ領域のポインタ</param>
        /// <returns>プロセスハンドル</returns>
        public static IntPtr InjectMemory(IntPtr processid, byte[] buffer, out IntPtr lpAddress)
        {
            lpAddress = IntPtr.Zero;
            //open local process object
            IntPtr hndProc = OpenProcess(
                0x2 | 0x8 | 0x10 | 0x20 | 0x400, //create thread, query info, operation 
                                                 //write, and read 
                1,
                (uint)processid);
            if (hndProc == (IntPtr)0) {
                return IntPtr.Zero;
            }
            //allocate memory for process object
            lpAddress = VirtualAllocEx(hndProc, (IntPtr)null, (uint)buffer.Length, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
            if (lpAddress == IntPtr.Zero) {
                _ = CloseHandle(hndProc);
                return IntPtr.Zero;
            }
            //wite data
            _ = WriteProcessMemory(hndProc, lpAddress, buffer, (uint)buffer.Length, out _);
            if (Marshal.GetLastWin32Error() != 0) {
                _ = VirtualFreeEx(hndProc, lpAddress, (uint)buffer.Length, MEM_DECOMMIT | MEM_RELEASE);
                _ = CloseHandle(hndProc);
                return IntPtr.Zero;
            }
            return hndProc;
        }

        /// <summary>
        /// SYSTEMTIMEを送信するためのSendMessageのラッパー
        /// </summary>
        /// <param name="handle">ウィンドウハンドル</param>
        /// <param name="processid">プロセスハンドル</param>
        /// <param name="lParam">lparam</param>
        /// <param name="msg">メッセージID</param>
        /// <param name="wParam">wparam</param>
        public static IntPtr SendMessage(IntPtr handle, IntPtr processid, int msg, int wParam, SYSTEMTIME lParam)
        {
            int structMemLen = Marshal.SizeOf(typeof(SYSTEMTIME));
            byte[] buffer = new byte[structMemLen];
            IntPtr dataPtr = Marshal.AllocHGlobal(structMemLen);
            Marshal.StructureToPtr(lParam, dataPtr, true);
            Marshal.Copy(dataPtr, buffer, 0, structMemLen);
            Marshal.FreeHGlobal(dataPtr);

            IntPtr hndProc;
            IntPtr ret = IntPtr.Zero;
            if ((hndProc = InjectMemory(processid, buffer, out IntPtr lpAddress)) != IntPtr.Zero) {
                ret = SendMessage(handle, msg, wParam, lpAddress);
                _ = CloseHandle(hndProc);
            }
            return ret;
        }

        /// <summary>
        /// ウィンドウのキャプチャ
        /// </summary>
        /// <remarks>
        /// 画像の取得はFormat32bppRgbで実施。<br/>デフォルトだと時計フレームワークと間に隙間が発生してしまったため。
        /// </remarks>
        /// <param name="handle">キャプチャ対象のウィンドウ</param>
        public static Bitmap CaptureWindow(IntPtr handle)
        {
            //ウィンドウサイズ取得
            GetWindowRect(handle, out RECT rect);

            int width = rect.right - rect.left;
            int height = rect.bottom - rect.top;

            //ウィンドウをキャプチャする
            Bitmap img = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            Graphics memg = Graphics.FromImage(img);
            IntPtr dc = memg.GetHdc();
            PrintWindow(handle, dc, 0);
            memg.ReleaseHdc(dc);
            memg.Dispose();
            return img;
        }

        /// <summary>
        /// 指定された要素をクリックする
        /// </summary>
        static public void Click(int x, int y)
        {
            SetCursorPos(x, y);
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }

        /// <summary>
        /// メニューのステータスを取得する
        /// </summary>
        /// <returns>trueの場合チェックがON状態</returns>
        static public bool GetMenuStatus(IntPtr topWindow, List<string> menus)
        {
            bool result = false;
            var hMenu = GetMenu(topWindow);
            foreach(var menu in menus) {
                for (uint i = 0; i < GetMenuItemCount(hMenu); i++) {
                    StringBuilder menuName = new StringBuilder(128);
                    _ = GetMenuString(hMenu, i, menuName, menuName.Capacity, MF_BYPOSITION);

                    // ニーモニック文字(&)を削る
                    var work = menuName.ToString();
                    var pos = work.IndexOf('&');
                    if (pos >= 0) {
                        work = work.Remove(pos, 1);
                    }

                    if (menu == work) {
                        result = (GetMenuState(hMenu, i, MF_BYPOSITION) & MF_CHECKED) != 0;
                        hMenu = GetSubMenu(hMenu, (int)i);
                        break;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// メニューが操作可能か確認する
        /// </summary>
        /// <returns>trueの場合チェックがON状態</returns>
        static public bool IsEnabledMenu(IntPtr topWindow, List<string> menus)
        {
            bool result = false;
            var hMenu = GetMenu(topWindow);
            foreach (var menu in menus) {
                for (uint i = 0; i < GetMenuItemCount(hMenu); i++) {
                    StringBuilder menuName = new StringBuilder(128);
                    _ = GetMenuString(hMenu, i, menuName, menuName.Capacity, MF_BYPOSITION);

                    // ニーモニック文字(&)を削る
                    var work = menuName.ToString();
                    var pos = work.IndexOf('&');
                    if (pos >= 0) {
                        work = work.Remove(pos, 1);
                    }
                    var split = work.Split('\t');
                    if (split.Any()) {
                        work = split[0];
                    }

                    if (menu == work) {
                        result = (GetMenuState(hMenu, i, MF_BYPOSITION) & (MF_DISABLED | MF_GRAYED)) == 0;
                        if (!result) {
                            return result;
                        }
                        hMenu = GetSubMenu(hMenu, (int)i);
                        break;
                    }
                }
            }
            return result;
        }
    }
}
