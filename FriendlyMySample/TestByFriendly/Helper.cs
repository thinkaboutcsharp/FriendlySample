using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using Codeer.Friendly.Windows;

namespace TestByFriendly
{
    class Helper
    {
        internal static WindowsAppFriend RunProcess()
        {
            //Debug Only
            var path = Directory.GetCurrentDirectory() + "\\..\\..\\..\\FriendlyMySample\\bin\\Debug\\FriendlyMySample.exe";
            var process = Process.Start(path);
            return new WindowsAppFriend(process);
        }

        internal static void ShutdownProcess(WindowsAppFriend app)
        {
            Process.GetProcessById(app.ProcessId).CloseMainWindow();
        }

        #region SendInput用の定義

        [DllImport("user32.dll")]
        private extern static void SendInput(int nInputs, INPUT[] pInputs, int cbsize);

        [DllImport("user32.dll", EntryPoint = "MapVirtualKeyA")]
        private extern static int MapVirtualKey(int wCode, int wMapType);

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public short wVk;
            public short wScan;
            public int dwFlags;
            public int time;
            public int dwExtraInfo;
        }

        // これは使わないけど、この構造体のサイズにならないとSendInputの中で何かが失敗するらしいので
        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public int mouseData;
            public int dwFlags;
            public int time;
            public int dwExtraInfo;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct INPUT
        {
            [FieldOffset(0)]
            public int type;
            [FieldOffset(4)]
            public MOUSEINPUT mi;
            [FieldOffset(4)]
            public KEYBDINPUT ki;
            // さすがにHARDWAREは省略
        }

        #endregion

        internal static void SendKey(Keys key)
        {
            var keyStroke = new INPUT[2];

            keyStroke[0].type = keyStroke[1].type = 1;
            keyStroke[0].ki.wVk = keyStroke[1].ki.wVk = (short)key;
            keyStroke[0].ki.wScan = keyStroke[1].ki.wScan = (short)MapVirtualKey((int)key, 0);
            keyStroke[0].ki.time = keyStroke[1].ki.time = 0;
            keyStroke[0].ki.dwExtraInfo = keyStroke[1].ki.dwExtraInfo = 0;
            keyStroke[0].ki.dwFlags = 0; // KEYDOWN
            keyStroke[1].ki.dwFlags = 2; // KEYUP

            SendInput(2, keyStroke, Marshal.SizeOf<INPUT>());
        }
    }
}
