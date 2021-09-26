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
        public MainForm()
        {
            InitializeComponent();
        }
        
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            EditForm form = new EditForm();
            if(form.ShowDialog() == DialogResult.OK)
            {
                ForwardItem item = form.GetForwardItem();
                ListViewItem listViewItem = new ListViewItem(new string[] { 
                    item.Name,
                    item.LocalListenAddress,
                    item.LocalListenPort.ToString(),
                    item.RemoteAddress,
                    item.RemotePort.ToString(),
                    item.UploadSpeed.ToString(),
                    item.DownloadSpeed.ToString(),
                    item.TotalUpload.ToString(),
                    item.TotalDownload.ToString(),
                });
                item.Start();
                listViewItem.Tag = item;
                listView1.Items.Add(listViewItem);
            }
        }
    }
}
