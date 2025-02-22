using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace WindowsFormsApplication2
{
    public partial class CamerName : Form
    {
        Form1 f;
        public CamerName(Form1 f1)
        {
            InitializeComponent();
            f = f1;
        }
        string 相机 = "";
        string 曝光 = "";
        private void CamerName_Load(object sender, EventArgs e)
        {
            相机 = f.ChoiceCamera;
            this.label1.Text = f.ChoiceCamera;
            if (相机 == "相机一")
            {
                曝光 = f.Vision.相机曝光[0];
                textBox1.Text=曝光;
              
            }
            else if (相机 == "相机二")
            {
                曝光 = f.Vision.相机曝光[1];
                textBox1.Text = 曝光;
            }
            else if (相机 == "相机三")
            {
                曝光 = f.Vision.相机曝光[2];
                textBox1.Text = 曝光;
            }
            else
            {
                曝光 = f.Vision.相机曝光[3];
                textBox1.Text = 曝光;
            }
           
        }

        private void button1_Click(object sender, EventArgs e)
        {
           f. HKstopanniu = true;
            Thread t1 = new Thread(CameraRunCon);
            t1.IsBackground = true;
            t1.Start();
        }

        private void CameraRunCon()
        {
            if (相机 == "相机一")
            {
               f. hk[0].RunContinuing();
                while (f.HKstopanniu)
                {
                    cogDisplay1.Image = f.hk[0].HKImage;
                }
            }
            else if (相机 == "相机二")
            {
                f.hk[1].RunContinuing();
                while (f.HKstopanniu)
                {
                    cogDisplay1.Image = f.hk[1].HKImage;
                  
                }
            }
            else if (相机 == "相机三")
            {
                f.hk[2].RunContinuing();
                while (f.HKstopanniu)
                {
                    cogDisplay1.Image = f.hk[2].HKImage;
                }
            }
            else
            {
                f.hk[3].RunContinuing();
                while (f.HKstopanniu)
                {
                    cogDisplay1.Image = f.hk[3].HKImage;
                }
            }
          
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (相机 == "相机一")
            {
               f. HKstopanniu = false;
              f.  hk[0].stopRun();
            }
            else if (相机 == "相机二")
            {
                f.HKstopanniu = false;
               f. hk[1].stopRun();
            }
            else if (相机 == "相机三")
            {
               f. HKstopanniu = false;
               f. hk[2].stopRun();
            }
            else
            {
                f.HKstopanniu = false;
              f.  hk[3].stopRun();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (相机 == "相机一")
            {
                Thread t1 = new Thread(writeCamera);
                t1.IsBackground = true;
                t1.Start(0);
            }
            else if (相机 == "相机二")
            {
                Thread t2 = new Thread(writeCamera);
                t2.IsBackground = true;
                t2.Start(1);
            }
            else if (相机 == "相机三")
            {
                Thread t3 = new Thread(writeCamera);
                t3.IsBackground = true;
                t3.Start(2);
            }
            else
            {
                Thread t4 = new Thread(writeCamera);
                t4.IsBackground = true;
                t4.Start(3);
            }
        }

        private void writeCamera(object i)
        {
            f.hk[Convert.ToInt16( i)].RunOnce(Convert.ToInt16(textBox1.Text));
            while (!f.hk[Convert.ToInt16(i)].pictuer)
            {
                Thread.Sleep(100);
            }
            cogDisplay1.Image = f.hk[Convert.ToInt16(i)].HKImage;
        }


        private void button4_Click(object sender, EventArgs e)
        {
            if (相机 == "相机一")
            {
                f.Vision.相机曝光[0] = textBox1.Text;
               INIOperationClass.INIOperationClass.INIWriteValue(Vision.Config_Path, " 配置", "曝光0", f.Vision.相机曝光[0]);

            }
            else if (相机 == "相机二")
            {
                f.Vision.相机曝光[1] = textBox1.Text;
                INIOperationClass.INIOperationClass.INIWriteValue(Vision.Config_Path, " 配置", "曝光1", f.Vision.相机曝光[1]);
            }
            else if (相机 == "相机三")
            {
                f.Vision.相机曝光[2] = textBox1.Text;
                INIOperationClass.INIOperationClass.INIWriteValue(Vision.Config_Path, " 配置", "曝光2", f.Vision.相机曝光[2]);
            }
            else
            {
                f.Vision.相机曝光[3] = textBox1.Text;
                INIOperationClass.INIOperationClass.INIWriteValue(Vision.Config_Path, " 配置", "曝光0", f.Vision.相机曝光[3]);
            }
        }


       
    }
}
