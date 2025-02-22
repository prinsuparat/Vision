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
    public partial class 参数设定 : Form
    {
        Form1 f;
        public 参数设定(Form1 f1)
        {
            InitializeComponent();
            f = f1;
        }
        string 机械X, 机械Y, 机械A;
        string   BX, BY, BA;
        bool zifulength = true;
        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            if(button2.Text =="登录"&&textBox11.Text=="Jabil")
            {
                textBox1.Enabled = true;
                textBox2.Enabled = true;
                textBox4.Enabled = true;
                textBox3.Enabled = true;
                textBox12.Enabled = true;
                textBox13.Enabled = true;
                textBox14.Enabled = true;
                button2.Text = "登出";
            }
            else
            {
                textBox1.Enabled = false;
                textBox2.Enabled = false;
                textBox4.Enabled = false;
                textBox3.Enabled = false;
                textBox12.Enabled = false;
                textBox13.Enabled = false ;
                textBox14.Enabled = false ;
                textBox11.Text = "";
                button2.Text = "登录";
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            try
            {
                if(textBox12 .Enabled )
                {
               /*  INIOperationClass.INIOperationClass.INIWriteValue(Vision.Produce_Date, "Message", "JGlue_GRN_length", textBox12.Text);
                INIOperationClass.INIOperationClass.INIWriteValue(Vision.Produce_Date, "Message", "JLens_GRN_length", textBox13.Text);
                INIOperationClass.INIOperationClass.INIWriteValue(Vision.Produce_Date, "Message", "JLens_QR_code_length", textBox14.Text);*/
                f.Vision.JGlue_GRN_length = Convert.ToInt16(textBox12.Text);
                f.Vision.JLens_GRN_length = Convert.ToInt16(textBox13.Text);
                f.Vision.JLens_QR_code_length = Convert.ToInt16(textBox14.Text);
                }
                if (textBox5 .Text .Length !=f.Vision.JGlue_GRN_length)
                {
                 
                    MessageBox.Show("保存失败，请查看JGlue_GRN字符长度");
                  
                }
                else if(textBox6.Text.Length != f.Vision.JLens_GRN_length)
                {
                  
                    MessageBox.Show("保存失败，请查看JLens_GRN字符长度");
                }
                else if(textBox7.Text.Length != f.Vision.JLens_QR_code_length)
                {
                    MessageBox.Show("保存失败，请查看JLens_QR_code字符长度");
                }
                else
                {
                    INIOperationClass.INIOperationClass.INIWriteValue(Vision.Produce_Date, "Message", "Bay No.", textBox1.Text);
                    INIOperationClass.INIOperationClass.INIWriteValue(Vision.Produce_Date, "Message", "Product Model", textBox2.Text);
                    INIOperationClass.INIOperationClass.INIWriteValue(Vision.Produce_Date, "Message", "Version", textBox3.Text);
                    INIOperationClass.INIOperationClass.INIWriteValue(Vision.Produce_Date, "Message", "Process Station", textBox4.Text);
                    INIOperationClass.INIOperationClass.INIWriteValue(Vision.Produce_Date, "Message", "JGlue_GRN", textBox5.Text);
                    INIOperationClass.INIOperationClass.INIWriteValue(Vision.Produce_Date, "Message", "JLens_GRN", textBox6.Text);
                    INIOperationClass.INIOperationClass.INIWriteValue(Vision.Produce_Date, "Message", "JLens_QR_code", textBox7.Text);
                    INIOperationClass.INIOperationClass.INIWriteValue(Vision.Produce_Date, "Message", "JGlue_GRN-Q'ty", textBox8.Text);
                    INIOperationClass.INIOperationClass.INIWriteValue(Vision.Produce_Date, "Message", "JLens_GRN-Q'ty", textBox9.Text);
                    INIOperationClass.INIOperationClass.INIWriteValue(Vision.Produce_Date, "Message", "JLens_QR_code-Q'ty", textBox10.Text);
                    Invoke(new MethodInvoker(delegate
                    {
                        f.Vision.BayNo = textBox1.Text;
                        f.Vision.ProductModel = textBox2.Text;
                        f.Vision.Version = textBox3.Text;
                        f.Vision.ProcessStation = textBox4.Text;
                        f.Vision.JGlue_GRN = textBox5.Text;
                        f.Vision.JLens_GRN = textBox6.Text;
                        f.Vision.JLens_QR_code = textBox7.Text;
                        f.Vision.JGlue_GRN_count = Convert.ToInt64(textBox8.Text);
                        f.Vision.JLens_GRN_count = Convert.ToInt64(textBox9.Text);
                        f.Vision.JLens_QR_code_count = Convert.ToInt64(textBox10.Text);
                        f.label9.Text = Convert.ToString(f.Vision.JGlue_GRN_count);
                        f.label11.Text = Convert.ToString(f.Vision.JLens_GRN_count);
                        f.label13.Text = Convert.ToString(f.Vision.JLens_QR_code_count);
                    }));
                    if (textBox12.Enabled)
                    {
                         INIOperationClass.INIOperationClass.INIWriteValue(Vision.Produce_Date, "Message", "JGlue_GRN_length", textBox12.Text);
                         INIOperationClass.INIOperationClass.INIWriteValue(Vision.Produce_Date, "Message", "JLens_GRN_length", textBox13.Text);
                         INIOperationClass.INIOperationClass.INIWriteValue(Vision.Produce_Date, "Message", "JLens_QR_code_length", textBox14.Text);
                         }
                        MessageBox.Show("保存成功");
                }
                
            }
           catch(Exception ex)
            {

                MessageBox.Show("保存失败，查看数据格式");
            }
        
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        
        private void 参数设定_Load(object sender, EventArgs e)
        {
            textBox1.Enabled = false;
            textBox2.Enabled = false;
            textBox4.Enabled = false;
            textBox3.Enabled = false;
            textBox12.Enabled = false;
            textBox13.Enabled = false;
            textBox14.Enabled = false;
            Invoke(new MethodInvoker(delegate
                {
                    textBox1.Text = f.Vision.BayNo;
                    textBox2.Text = f.Vision.ProductModel;
                    textBox3.Text = f.Vision.Version;
                    textBox4.Text = f.Vision.ProcessStation;

                    textBox8.Text =Convert .ToString( f.Vision.JGlue_GRN_count);
                    textBox9.Text = Convert.ToString(f.Vision.JLens_GRN_count);
                    textBox10.Text = Convert.ToString(f.Vision.JLens_QR_code_count);
                    textBox12.Text = Convert.ToString(f.Vision.JGlue_GRN_length);
                    textBox13 .Text = Convert.ToString(f.Vision.JLens_GRN_length);
                    textBox14 .Text = Convert.ToString(f.Vision.JLens_QR_code_length);

                    if (f.Vision.JGlue_GRN_count == 0)
                    {
                        f.Vision.JGlue_GRN = "";
                        INIOperationClass.INIOperationClass.INIWriteValue(Vision.Produce_Date, "Message", "JGlue_GRN", "");
                    }
                    if(f.Vision.JLens_GRN_count == 0)
                    {
                        f.Vision.JLens_GRN = "";
                        INIOperationClass.INIOperationClass.INIWriteValue(Vision.Produce_Date, "Message", "JLens_GRN", "");
                    }
                    if(f.Vision.JLens_QR_code_count==0)
                    {
                        f.Vision.JLens_QR_code = "";
                        INIOperationClass.INIOperationClass.INIWriteValue(Vision.Produce_Date, "Message", "JLens_QR_code", "");
                    }
                   
                        textBox5.Text = f.Vision.JGlue_GRN;
                        textBox6.Text = f.Vision.JLens_GRN;
                        textBox7.Text = f.Vision.JLens_QR_code;
                   
                }));
           
        }

        private void button1_Click(object sender, EventArgs e)
        {
            BX = textBox4.Text;
            f.Vision.补偿X=Convert.ToDouble(BX);
            INIOperationClass.INIOperationClass.INIWriteValue(Vision.Config_Path, "位置参数", "补偿X", BX);
            BY = textBox5.Text;
            f.Vision.补偿Y = Convert.ToDouble(BY);
            INIOperationClass.INIOperationClass.INIWriteValue(Vision.Config_Path, "位置参数", "补偿Y", BY);
            BA = textBox6.Text;
            f.Vision.补偿A = Convert.ToDouble(BA);
            INIOperationClass.INIOperationClass.INIWriteValue(Vision.Config_Path, "位置参数", "补偿A", BA);
            机械X = textBox1.Text;
            机械Y = textBox2.Text;
            机械A = textBox3.Text;
            f.Vision.机械手X = Convert.ToDouble(机械X);
            f.Vision.机械手Y = Convert.ToDouble(机械Y);
            f.Vision.机械手A = Convert.ToDouble(机械A);
            INIOperationClass.INIOperationClass.INIWriteValue(Vision.Config_Path, "位置参数", "组装点X", 机械X);
            INIOperationClass.INIOperationClass.INIWriteValue(Vision.Config_Path, "位置参数", "组装点Y", 机械Y);
            INIOperationClass.INIOperationClass.INIWriteValue(Vision.Config_Path, "位置参数", "组装点A", 机械A);
            MessageBox.Show("保存成功");
           
        }
    }
}
