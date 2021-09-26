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
    public partial class EditForm : Form
    {
        public EditForm()
        {
            InitializeComponent();
        }

        public ForwardItem forwardItem = new ForwardItem();

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("本地监听地址不能为空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (numericUpDown1.Value <= 0)
            {
                MessageBox.Show("本地监听端口错误", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(textBox2.Text))
            {
                MessageBox.Show("目标地址不能为空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (numericUpDown2.Value <= 0)
            {
                MessageBox.Show("目标端口错误", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(textBox3.Text))
            {
                MessageBox.Show("名字不能为空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            forwardItem.Name = textBox3.Text;
            forwardItem.LocalListenAddress = textBox1.Text;
            forwardItem.LocalListenPort = Convert.ToInt32(numericUpDown1.Value);
            forwardItem.RemoteAddress = textBox2.Text;
            forwardItem.RemotePort = Convert.ToInt32(numericUpDown2.Value);
            if(radioButton1.Checked)
            {
                forwardItem.Protocol = ForwardProtocol.Tcp;
            }
            if (radioButton2.Checked)
            {
                forwardItem.Protocol = ForwardProtocol.Udp;
            }

            // 设置对话框返回值后，窗体会自动关闭
            DialogResult = DialogResult.OK;
        }


        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            Close();
        }

        public ForwardItem GetForwardItem()
        {
            return forwardItem;
        }
    }
}
