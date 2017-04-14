using Codeer.Friendly;
using Codeer.Friendly.Windows;
using Codeer.Friendly.Windows.Grasp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Forms;
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
            this.app = Helper.RunProcess();
        }

        [TestCleanup]
        public void Cleanup()
        {
            Helper.ShutdownProcess(app);
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
            Helper.SendKey(Keys.Enter);
            System.Threading.Thread.Sleep(10);

            int selectLength = (int)formVar["txt_Id"]()["SelectionLength"]().Core;
            string name = (string)formVar["txt_Name"]()["Text"]().Core;

            Assert.AreEqual<int>(2, selectLength);
            Assert.AreEqual<string>("あいうえお", name);
        }
    }
}
