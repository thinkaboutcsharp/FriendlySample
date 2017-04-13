using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Codeer.Friendly;
using Codeer.Friendly.Windows;
using Codeer.Friendly.Windows.Grasp;
// Dynamicを使わない

namespace TestByFriendly
{
    [TestClass]
    public class UnitTest2
    {
        private WindowsAppFriend app;

        [TestInitialize]
        public void Initialize()
        {
            this.app = this.RunProcess();
        }

        [TestCleanup]
        public void Cleanup()
        {
            this.ShutdownProcess(app);
        }

        [TestMethod]
        public void TestMethod1_AppVar()
        {
            AppVar formVars = this.app[typeof(Application), "OpenForms"]();  // この時点では配列
            AppVar formVar = formVars["[]"](0);                              // Formオブジェクトへの参照を持つAppVar
            string text = (string)formVar["txt_Id"]()["Text"]().Core;        // 段階的にアクセスする

            Assert.AreEqual<string>(string.Empty, text);
        }

        [TestMethod]
        public void TestMethod2_AppVar()
        {
            AppVar formVars = this.app[typeof(Application), "OpenForms"]();
            AppVar formVar = formVars["[]"](0);
            formVar["txt_Id"]()["Text"]("10");        // プロパティーには関数形式で設定する

            string text = (string)formVar["txt_Id"]()["Text"]().Core;

            Assert.AreEqual<string>("10", text);
        }

        [TestMethod]
        public void TestMethod3_AppVar()
        {
            AppVar formVars = this.app[typeof(Application), "OpenForms"]();
            AppVar formVar = formVars["[]"](0);
            var form = new WindowControl(formVar);

            var async = new Async();
            formVar["btn_Add"]()["PerformClick", async]();  // AsyncはPerformClickの引数ではない

            var dialog = form.WaitForNextModal();
            AppVar dialogVar = dialog.AppVar;

            dialogVar["txt_Id"]()["Text"]("10");            // ダイアログ上の操作
            dialogVar["txt_Name"]()["Text"]("あいうえお");
            dialogVar["btn_Ok"]()["PerformClick"]();

            async.WaitForCompletion();

            int count = (int)formVar["infoList"]()["Count"]().Core;

            Assert.AreEqual<int>(1, count);
        }


        [TestMethod]
        public void TestMethod4_AppVar()
        {
            AppVar formVars = this.app[typeof(Application), "OpenForms"]();
            AppVar formVar = formVars["[]"](0);
            var form = new WindowControl(formVar);

            var async = new Async();
            formVar["btn_Add"]()["PerformClick", async]();

            var dialog = form.WaitForNextModal();
            AppVar dialogVar = dialog.AppVar;

            dialogVar["txt_Id"]()["Text"]("10");
            dialogVar["txt_Name"]()["Text"]("あいうえお");
            dialogVar["btn_Ok"]()["PerformClick"]();

            async.WaitForCompletion();

            // ここから本題

            //formVar.Activate();
            formVar["txt_Id"]()["Focus"]();
            formVar["txt_Id"]()["Text"]("10");
            this.SendKey(Keys.Enter);
            System.Threading.Thread.Sleep(10);

            int selectLength = (int)formVar["txt_Id"]()["SelectionLength"]().Core;
            string name = (string)formVar["txt_Name"]()["Text"]().Core;

            Assert.AreEqual<int>(2, selectLength);
            Assert.AreEqual<string>("あいうえお", name);
        }

        private WindowsAppFriend RunProcess()
        {
            //Debug Only
            var path = Directory.GetCurrentDirectory() + "\\..\\..\\..\\FriendlyMySample\\bin\\Debug\\FriendlyMySample.exe";
            var process = Process.Start(path);
            return new WindowsAppFriend(process);
        }

        private void ShutdownProcess(WindowsAppFriend app)
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

        private void SendKey(Keys key)
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
