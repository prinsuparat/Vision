using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cognex;
using Cognex.VisionPro;
using Cognex.VisionPro.ToolBlock;
using System.Threading;

namespace WindowsFormsApplication2
{
    public partial class RunTB : Form
    {
        Form1 f;
        public RunTB(Form1 f1)
        {
            InitializeComponent();
            f = f1;
        }
        double  基准X, 基准Y, 基准A;
        private void RunTB_Load(object sender, EventArgs e)
        {
            textBox1.Enabled = false;
            textBox2.Enabled = false;
            textBox3.Enabled = false;
            button3.Enabled = false;
            if (f.ChoiceCamera == "相机一")
            {
                textBox1.Text= INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Config_Path, "相机功能0", "基准X","" );

                textBox2.Text=INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Config_Path, "相机功能0", "基准Y","" );

                textBox3.Text = INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Config_Path, "相机功能0", "基准A", "");
            }
            else if (f.ChoiceCamera == "相机二")
            {
                textBox1.Text = INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Config_Path, "相机功能1", "基准X", "");

                textBox2.Text = INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Config_Path, "相机功能1", "基准Y", "");

                textBox3.Text = INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Config_Path, "相机功能1", "基准A", "");
            }
            else if (f.ChoiceCamera == "相机三")
            {
                textBox1.Text = INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Config_Path, "相机功能2", "基准X", "");

                textBox2.Text = INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Config_Path, "相机功能2", "基准Y", "");

                textBox3.Text = INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Config_Path, "相机功能2", "基准A", "");
            }
            else
            {
                textBox1.Text = INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Config_Path, "相机功能3", "基准X", "");

                textBox2.Text = INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Config_Path, "相机功能3", "基准Y", "");

                textBox3.Text = INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Config_Path, "相机功能3", "基准A", "");
            }
            Invoke(new MethodInvoker(delegate
                {
                    label2.Text = f.ChoiceCamera;
                }));
            try
            {
                cogToolBlockEditV21.Subject = f.choiceRunTB;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (f.相机硬件传递名称 == "Vision")
            {
                f.ChoiceAcq.Run();
                f.choiceRunTB.Inputs["Image"].Value = f.ChoiceAcq.OutputImage; ;
                f.choiceRunTB.Run();

            }
            else
            {
                if (f.ChoiceCamera == "相机一")
                {
                    HKCamera(0);
                }
                else if (f.ChoiceCamera == "相机二")
                {
                    HKCamera(1);
                }
                else if (f.ChoiceCamera == "相机三")
                {
                    HKCamera(2);
                }
                else
                {
                    HKCamera(3);
                }

            }
        }

        private void HKCamera(object i)
        {
            f.hk[Convert.ToInt16(i)].RunOnce(Convert.ToInt16(f.Vision.相机曝光[Convert.ToInt16(i)]));
            while (!f.hk[Convert.ToInt16(i)].pictuer)
            {
                Thread.Sleep(100);
            }
            f.Choice_Calibate.InputImage = f.hk[Convert.ToInt16(i)].HKImage;
            f.Choice_Calibate.Run();
            f.choiceRunTB.Inputs["Image"].Value = f.Choice_Calibate.OutputImage;
            f.choiceRunTB.Run();

        }
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                CogSerializer.SaveObjectToFile(f.choiceRunTB, f.choiceRunTB_path);
                MessageBox.Show("保存成功");
            }  
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void RunTB_FormClosed(object sender, FormClosedEventArgs e)
        {
            cogToolBlockEditV21.Subject = null;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                textBox1.Enabled = true;
                textBox2.Enabled = true;
                textBox3.Enabled = true;
                button3.Enabled = true;
            }
            else
            {
                textBox1.Enabled = false;
                textBox2.Enabled = false;
                textBox3.Enabled = false ;
                button3.Enabled = false;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            基准X =Convert .ToDouble(textBox1.Text);
            基准Y =Convert .ToDouble( textBox2.Text);
            基准A =Convert .ToDouble( textBox3.Text);
            if (f.ChoiceCamera == "相机一")
            {
                INIOperationClass.INIOperationClass.INIWriteValue(Vision.Config_Path, "相机功能0", "基准X", 基准X.ToString("0.000"));
                INIOperationClass.INIOperationClass.INIWriteValue(Vision.Config_Path, "相机功能0", "基准Y", 基准Y.ToString("0.000"));
                INIOperationClass.INIOperationClass.INIWriteValue(Vision.Config_Path, "相机功能0", "基准A", 基准A.ToString("0.000"));
                MessageBox.Show("保存完成");
            }
            else if (f.ChoiceCamera == "相机二")
            {
                INIOperationClass.INIOperationClass.INIWriteValue(Vision.Config_Path, "相机功能1", "基准X", 基准X.ToString("0.000"));
                INIOperationClass.INIOperationClass.INIWriteValue(Vision.Config_Path, "相机功能1", "基准Y", 基准Y.ToString("0.000"));
                INIOperationClass.INIOperationClass.INIWriteValue(Vision.Config_Path, "相机功能1", "基准A", 基准A.ToString("0.000"));
                MessageBox.Show("保存完成");
            }
            else if (f.ChoiceCamera == "相机三")
            {
                INIOperationClass.INIOperationClass.INIWriteValue(Vision.Config_Path, "相机功能2", "基准X", 基准X.ToString("0.000"));
                INIOperationClass.INIOperationClass.INIWriteValue(Vision.Config_Path, "相机功能2", "基准Y", 基准Y.ToString("0.000"));
                INIOperationClass.INIOperationClass.INIWriteValue(Vision.Config_Path, "相机功能2", "基准A", 基准A.ToString("0.000"));
                MessageBox.Show("保存完成");
            }
            else
            {
                INIOperationClass.INIOperationClass.INIWriteValue(Vision.Config_Path, "相机功能3", "基准X", 基准X.ToString("0.000"));
                INIOperationClass.INIOperationClass.INIWriteValue(Vision.Config_Path, "相机功能3", "基准Y", 基准Y.ToString("0.000"));
                INIOperationClass.INIOperationClass.INIWriteValue(Vision.Config_Path, "相机功能3", "基准A", 基准A.ToString("0.000"));
                MessageBox.Show("保存完成");
            }

        }
          
    }
}
