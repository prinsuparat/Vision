using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using INIOperationClass;

namespace WindowsFormsApplication2
{
    public partial class login : Form
    {
        Form1 f;
        public login(Form1 f1)
        {
            InitializeComponent();
            f = f1;
        }
        string 作业员, 生计, 工程师;
        Vision Vision = new Vision();
        private void login_Load(object sender, EventArgs e)
        {

            作业员 = INIOperationClass.INIOperationClass.INIGetStringValue(Vision.user_path, "权限", "作业员", "");
            生计 = INIOperationClass.INIOperationClass.INIGetStringValue(Vision.user_path, "权限", "生计", "");
            工程师 = INIOperationClass.INIOperationClass.INIGetStringValue(Vision.user_path, "权限", "工程师", "");
         //   Vision.Cameranum = Convert.ToInt16(INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Config_Path, "相机", "相机数量", ""));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            logged();
        }

        private enum permission
        {
            作业员 = 0,
            生计 = 1,
            工程师 = 2
        }
        private void logged()
        {
            int k = comboBox1.SelectedIndex;
            string password1 = textBox2.Text;
            switch (k)
            {
                case (int)permission.作业员:
                    if (password1 == 作业员)
                    {
                        f.tabControl1.TabPages.Add(f.tabPage1);
                     //   f.tabControl1.TabPages.Add(f.tabPage2);
                        f.登录ToolStripMenuItem.Text = "登出";
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("密码错误");
                    }

                    break;
                case (int)permission.生计:
                    if (password1 == 生计)
                    {
                        f.tabControl1.TabPages.Add(f.tabPage1);
                      //  f.tabControl1.TabPages.Add(f.tabPage2);
                        f.产品选择ToolStripMenuItem.Enabled = true;
                        f.登录ToolStripMenuItem.Text = "登出";
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("密码错误");
                    }

                    break;
                case (int)permission.工程师:
                    if (password1 == 工程师)
                    {
                        f.tabControl1.TabPages.Add(f.tabPage1);
                    //    f.tabControl1.TabPages.Add(f.tabPage2);
                        f.产品选择ToolStripMenuItem.Enabled = true;
                        f.相机取向ToolStripMenuItem.Enabled = true;
                        f.相机标定ToolStripMenuItem.Enabled = true;
                        f.作业ToolStripMenuItem.Enabled = true;
                        f.参数设置ToolStripMenuItem.Enabled = true;
                        f.通讯ToolStripMenuItem.Enabled = true;
                        f.button2.Enabled = true;
                        f.checkBox3.Enabled = true;
                        f.登录ToolStripMenuItem.Text = "登出";
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("密码错误");
                    }

                    break;
            }

        }
    }
}
