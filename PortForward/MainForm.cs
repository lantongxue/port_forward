using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PortForward
{
    public partial class MainForm : Form
    {
        ForwardItemManage forwardManage = new ForwardItemManage();

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            List<ForwardItem> forwardItems = forwardManage.Select();
            foreach(ForwardItem forward in forwardItems)
            {
                ListViewItem item = new ListViewItem();
                forward.Item = item;
                listView1.Items.Add(item);
                if(forward.State == ForwardState.Runing)
                {
                    forward.Start();
                }
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            EditForm form = new EditForm();
            if(form.ShowDialog() == DialogResult.OK)
            {
                ForwardItem forward = form.GetForwardItem();
                ListViewItem item = new ListViewItem();
                forward.Item = item;
                item.Tag = forward;

                listView1.Items.Add(item);

                forward.Start();
                forwardManage.Add(forward);
            }
        }

    }
}
