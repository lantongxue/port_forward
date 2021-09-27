using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.ListViewItem;

namespace PortForward
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }
        
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            EditForm form = new EditForm();
            if(form.ShowDialog() == DialogResult.OK)
            {
                ForwardItem forward = form.GetForwardItem();
                ListViewItem item = new ListViewItem();
                forward.Item = item;
                listView1.Items.Add(item);
                forward.Start();
            }
        }
    }
}
