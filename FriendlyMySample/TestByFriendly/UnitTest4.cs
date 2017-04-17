using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Windows.Forms;
using Codeer.Friendly;
using Codeer.Friendly.Windows;
using Codeer.Friendly.Windows.Grasp;
using Ong.Friendly.FormsStandardControls;
using VSHTC.Friendly.PinInterface;

namespace TestByFriendly
{
    [TestClass]
    public class UnitTest4
    {
        interface IApplicationStatic
        {
            IFormCollection OpenForms { get; } // FormCollectionオブジェクトを返すので配列ではない
        }

        interface IFormCollection
        {
            IMainForm this[int index] { get; } // FormCollectionクラスがこの形のindexerを持っているからこうなる
        }

        interface IMainForm
        {
            FormsTextBox txt_Id { get; }    // AppVarをラップした形でないとシリアライズでエラーになる
            FormsTextBox txt_Name { get; }
            FormsButton btn_Add { get; }

            string Text { get; }

            IInfoList infoList { get; }     // List<Info>はシリアライズできない
        }

        interface IInputForm
        {
            FormsTextBox txt_Id { get; }
            FormsTextBox txt_Name { get; }
            FormsButton btn_Ok { get; }
            FormsButton btn_Cancel { get; }
        }

        interface IInfoList
        {
            int Count { get; }
            IInfo this[int index] { get; }
        }

        interface IInfo
        {
            string Id { get; }
            string Name { get; }
        }

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
        public void TestMethod1_Pin()
        {
            IApplicationStatic application = this.app.Pin<IApplicationStatic, Application>();
            IMainForm mainForm = application.OpenForms[0];

            Assert.AreEqual<string>(string.Empty, mainForm.txt_Id.Text as string);
            Assert.AreEqual<string>("MainForm", mainForm.Text as string);
            Assert.AreEqual<int>(0, mainForm.infoList.Count);
        }

        [TestMethod]
        public void TestMethod2_Pin()
        {
            IApplicationStatic application = this.app.Pin<IApplicationStatic, Application>();
            IMainForm mainForm = application.OpenForms[0];
            WindowControl mainFormVar = new WindowControl(PinHelper.GetAppVar(mainForm));

            //Async async = PinHelper.AsyncNext(mainForm);  // こうやるとなぜかbtn_Addがnullになって死ぬ
            Async async = new Async();
            mainForm.btn_Add.EmulateClick(async);
            WindowControl inputFormVar = mainFormVar.WaitForNextModal();
            IInputForm inputForm = inputFormVar.AppVar.Pin<IInputForm>();

            inputForm.txt_Id.EmulateChangeText("10");
            inputForm.txt_Name.EmulateChangeText("あいうえお");
            inputForm.btn_Ok.EmulateClick();

            async.WaitForCompletion();

            Assert.AreEqual<int>(1, mainForm.infoList.Count);
            Assert.AreEqual<string>("10", mainForm.infoList[0].Id);
            Assert.AreEqual<string>("あいうえお", mainForm.infoList[0].Name);
        }
    }
}
