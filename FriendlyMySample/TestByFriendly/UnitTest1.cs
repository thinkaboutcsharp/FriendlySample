using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Codeer.Friendly;                // Asyncクラス
using Codeer.Friendly.Dynamic;        // Dyamic操作系
using Codeer.Friendly.Windows;        // Windows操作系
using Codeer.Friendly.Windows.Grasp;  // WindowControlクラス

namespace TestByFriendly
{
    [TestClass]
    public class UnitTest1
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
        public void TestMethod1()
        {
            dynamic formVar = this.app.Type<Application>().OpenForms[0];   // Formオブジェクトへの参照を持つDynamicAppVar
            string text = formVar.txt_Id.Text;                             // formVarからアクセスする

            Assert.AreEqual<string>(string.Empty, text);
        }

        [TestMethod]
        public void TestMethod2()
        {
            dynamic formVar = this.app.Type<Application>().OpenForms[0];
            formVar.txt_Id.Text = "10";                                    // formVarから普通に設定できる

            string text = formVar.txt_Id.Text;

            Assert.AreEqual<string>("10", text);
        }

        [TestMethod]
        public void TestMethod3()
        {
            dynamic formVar = this.app.Type<Application>().OpenForms[0];
            var form = new WindowControl(formVar);                    // これを使うことで後のWaitが楽になる

            var async = new Async();                                  // ダイアログが完全に閉じるのを待つため
            formVar.btn_Add.PerformClick(async);                      // ボタンクリック

            var dialog = form.WaitForNextModal();                     // これがしたかった
            dynamic dialogVar = dialog.Dynamic();

            dialogVar.txt_Id.Text = "10";                             // ダイアログ上の操作
            dialogVar.txt_Name.Text = "あいうえお";
            dialogVar.btn_Ok.PerformClick();

            async.WaitForCompletion();                                // ダイアログ完了の待機

            int count = formVar.infoList.Count;                       // 内部データにも余裕でアクセス

            Assert.AreEqual<int>(1, count);
        }

        [TestMethod]
        public void TestMethod4()
        {
            dynamic formVar = this.app.Type<Application>().OpenForms[0];
            var form = new WindowControl(formVar);

            var async = new Async();
            formVar.btn_Add.PerformClick(async);

            var dialog = form.WaitForNextModal();
            dynamic dialogVar = dialog.Dynamic();

            dialogVar.txt_Id.Text = "10";
            dialogVar.txt_Name.Text = "あいうえお";
            dialogVar.btn_Ok.PerformClick();

            async.WaitForCompletion();

            // ここから本題

            //formVar.Activate();
            formVar.txt_Id.Focus();               // フォーカスしてタイプまでは可能
            formVar.txt_Id.Text = "10";
            this.SendKey(Keys.Enter);             // キーボードエミュレートはFriendlyの範囲外
            System.Threading.Thread.Sleep(10);    // ちょっとだけ待ってみる

            int selectLength = formVar.txt_Id.SelectionLength;
            string name = formVar.txt_Name.Text;

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
