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
    public partial class InputForm : Form
    {
        internal (string id, string name) Info { get; private set; }

        public InputForm()
        {
            InitializeComponent();
        }

        private void SetReturnInfo()
        {
            this.Info = (txt_Id.Text, txt_Name.Text);
        }

        private void InputForm_Load(object sender, EventArgs e)
        {
            this.Info = (null, null);
        }

        private void btn_Ok_Click(object sender, EventArgs e)
        {
            this.SetReturnInfo();
        }

        private void InputForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F12)
            {
                this.SetReturnInfo();
                this.DialogResult = DialogResult.OK;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
            }
        }
    }
}
