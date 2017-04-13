using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FriendlyMySample
{
    public partial class MainForm : Form
    {
        private class Info
        {
            internal string Id { get; }
            internal string Name { get; }

            internal Info(string id, string name)
                => (this.Id, this.Name) = (id, name);

            internal Info((string id, string name) info)
                => (this.Id, this.Name) = info;
        }

        private List<Info> infoList = new List<Info>();

        public MainForm()
        {
            InitializeComponent();
        }

        private void Clear()
        {
            txt_Id.Text = string.Empty;
            txt_Name.Text = string.Empty;
        }

        private void SetName(string id)
        {
            var name = from info in this.infoList where info.Id == id select info.Name;
            if (name.Count() > 0)
            {
                txt_Name.Text = name.First();
                txt_Id.SelectAll();
            }
            else
            {
                MessageBox.Show("ないよ");
                this.Clear();
            }
        }

        private void AddInfo()
        {
            using (var form = new InputForm())
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    var newInfo = new Info(form.Info);
                    this.infoList.Add(newInfo);
                }
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.Clear();
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && txt_Id.Focused)
            {
                this.SetName(txt_Id.Text);
            }
            else if (e.KeyCode == Keys.F4)
            {
                this.AddInfo();
            }
        }

        private void btn_Add_Click(object sender, EventArgs e)
        {
            this.AddInfo();
        }
    }
}
