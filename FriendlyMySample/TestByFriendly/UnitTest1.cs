using Codeer.Friendly;                // Asyncクラス
using Codeer.Friendly.Dynamic;        // Dyamic操作系
using Codeer.Friendly.Windows;        // Windows操作系
using Codeer.Friendly.Windows.Grasp;  // WindowControlクラス
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Forms;

namespace TestByFriendly
{
    [TestClass]
    public class UnitTest1
    {
        private WindowsAppFriend app;

        [TestInitialize]
        public void Initialize()
        {
            this.app = Helper.RunProcess();
        }

        [TestCleanup]
        public void Cleanup()
        {
            Helper.ShutdownProcess(app);
        }

        [TestMethod]
        public void TestMethod1_Dynamic()
        {
            dynamic formVar = this.app.Type<Application>().OpenForms[0];   // Formオブジェクトへの参照を持つDynamicAppVar
            string text = formVar.txt_Id.Text;                             // formVarからアクセスする

            Assert.AreEqual<string>(string.Empty, text);
        }

        [TestMethod]
        public void TestMethod2_Dynamic()
        {
            dynamic formVar = this.app.Type<Application>().OpenForms[0];
            formVar.txt_Id.Text = "10";                                    // formVarから普通に設定できる

            string text = formVar.txt_Id.Text;

            Assert.AreEqual<string>("10", text);
        }

        [TestMethod]
        public void TestMethod3_Dynamic()
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
        public void TestMethod4_Dynamic()
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
            Helper.SendKey(Keys.Enter);           // キーボードエミュレートはFriendlyの範囲外
            System.Threading.Thread.Sleep(10);    // ちょっとだけ待ってみる

            int selectLength = formVar.txt_Id.SelectionLength;
            string name = formVar.txt_Name.Text;

            Assert.AreEqual<int>(2, selectLength);
            Assert.AreEqual<string>("あいうえお", name);
        }
    }
}
