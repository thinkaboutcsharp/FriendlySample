using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Codeer.Friendly;
using Codeer.Friendly.Dynamic;
using Codeer.Friendly.Windows;
using Codeer.Friendly.Windows.Grasp;

namespace TestByFriendly
{
    [TestClass]
    public class UnitTest3
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
        public void TestMethod1_100Times_Basic()
        {
            var infos = new List<(string id, string name)>();
            dynamic formVar = this.app.Type<Application>().OpenForms[0];
            var checker = new Intruder.RunInApp();

            // テストデータ準備
            checker.Make100TestData(infos);

            // 100個の登録
            var form = new WindowControl(formVar);

            for (int i = 0; i < 100; i++)
            {
                var async = new Async();
                formVar.btn_Add.PerformClick(async);

                var dialog = form.WaitForNextModal();
                dynamic dialogVar = dialog.Dynamic();

                dialogVar.txt_Id.Text = infos[i].id;       // 普通の代入方式はシリアライズする処理
                dialogVar.txt_Name.Text = infos[i].name;
                dialogVar.btn_Ok.PerformClick();

                async.WaitForCompletion();
            }

            // Assert in myself
            Assert.AreEqual<int>(100, (int)formVar.infoList.Count);
            for (int i = 0; i < 100; i++)
            {
                Assert.AreEqual<string>(infos[i].id, (string)formVar.infoList[i].Id);     // シリアライズしまくり
                Assert.AreEqual<string>(infos[i].name, (string)formVar.infoList[i].Name);
            }
        }

        [TestMethod]
        public void TestMethod2_100Times_InApp()
        {
            // Injection
            WindowsAppExpander.LoadAssemblyFromFile(this.app, typeof(Intruder.RunInApp).Assembly.Location);
            WindowsAppExpander.LoadAssemblyFromFile(this.app, typeof(Assert).Assembly.Location);

            // 全部向こう側のオブジェクト
            dynamic infosRemote = this.app.Dim(new NewInfo<List<(string, string)>>()).Dynamic();
            dynamic formVar = this.app.Type<Application>().OpenForms[0];
            dynamic checker = this.app.Dim(new NewInfo<Intruder.RunInApp>()).Dynamic();

            // テストデータ準備
            checker.Make100TestData(infosRemote);  // 全部向こう側なのでシリアライズなし

            // 100個の登録
            var form = new WindowControl(formVar);

            for (int i = 0; i < 100; i++)
            {
                var async = new Async();
                formVar.btn_Add.PerformClick(async);

                var dialog = form.WaitForNextModal();
                dynamic dialogVar = dialog.Dynamic();

                checker.SetDialogText(infosRemote, i, dialogVar.txt_Id, dialogVar.txt_Name); // 向こう側で実行(iはシリアライズ)
                dialogVar.btn_Ok.PerformClick();

                async.WaitForCompletion();
            }

            // Assert in app
            checker.AssertList(formVar.infoList, infosRemote);  // 向こう側で実行(ここならinfoListが楽に取れる)
        }
    }
}
