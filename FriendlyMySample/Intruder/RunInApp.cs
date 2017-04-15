using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;

// FriendlyMySample、Codeerのいずれも参照していない

namespace Intruder
{
    public class RunInApp
    {
        public void AssertList(IList infoList, List<(string id, string name)> infos)
        {
            // 非公開クラスなので一工夫
            Type infoType = infoList[0].GetType();  // FriendlyMySample.MainForm.Infoクラス
            PropertyInfo idProp = infoType.GetProperty("Id", BindingFlags.NonPublic | BindingFlags.Instance);
            PropertyInfo nameProp = infoType.GetProperty("Name", BindingFlags.NonPublic | BindingFlags.Instance);
            Func<object, string> getId = info => idProp.GetValue(info) as string;
            Func<object, string> getName = info => nameProp.GetValue(info) as string;

            Assert.AreEqual<int>(100, infoList.Count);
            for (int i = 0; i < 100; i++)
            {
                Assert.AreEqual<string>(infos[i].id, getId(infoList[i]));
                Assert.AreEqual<string>(infos[i].name, getName(infoList[i]));
            }
        }

        public void Make100TestData(List<(string id, string name)> infos)
        {
            for (int i = 0; i < 100; i++)
            {
                infos.Add((i.ToString(), "Someone" + i.ToString("00")));
            }

            //Shuffle
            (infos[5], infos[23]) = (infos[23], infos[5]);
            (infos[11], infos[97]) = (infos[97], infos[11]);
            (infos[39], infos[74]) = (infos[74], infos[39]);
        }

        public void SetDialogText(List<(string id, string name)> infos, int index, TextBox id, TextBox name)
        {
            id.Text = infos[index].id;
            name.Text = infos[index].name;
        }
    }
}
