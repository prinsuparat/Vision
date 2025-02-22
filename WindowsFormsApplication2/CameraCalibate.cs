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
using Cognex.VisionPro.CalibFix;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using INIOperationClass;
using Cognex.VisionPro.Display;


namespace WindowsFormsApplication2
{
    public partial class CameraCalibate : Form
    {
        Form1 f;
        public CameraCalibate(Form1 f1)
        {
            InitializeComponent();
            f = f1;
        }
        private CogAcqFifoTool Calibate_acq1;
        private CogToolBlock Calibate_mtb1;
        private CogToolBlock Center_tb;
        private CogCalibNPointToNPointTool Calibate_CalibateTool9;
        private string acq1_Path,mtb1_Path,Calibate_Path,prodocename1,Center_Path;
        private Socket Calibate_Socket,Calibate_handle;
        private ICogImage Cabliateimage;
        private string 旋转中心点;
        int port;
        string  IP;
        Thread t1;
        double  X, Y;
        private CogAcqFifoEditV2 acqTool;
        private CogDisplay display;
        public string 中心X, 中心Y;
        private void CameraCalibate_Load(object sender, EventArgs e)
        {
            Invoke(new MethodInvoker(delegate
                {
                    prodocename1=f.label3.Text;
                    label2.Text = f.ChoiceCamera;
                }));
           
            acq1_Path = Application.StartupPath + "\\" + prodocename1 + "\\Calibate\\" + f.ChoiceCamera + "取图.vpp";
            mtb1_Path = Application.StartupPath + "\\" + prodocename1 + "\\Calibate\\" + f.ChoiceCamera + ".vpp";
            Calibate_Path = Application.StartupPath + "\\" + prodocename1 + "\\Calibate\\" + f.ChoiceCamera + "标定.vpp";
            Center_Path = Application.StartupPath + "\\" + prodocename1 + "\\Calibate\\" + f.ChoiceCamera + "旋转中心.vpp";
            Calibate_acq1=(CogAcqFifoTool)CogSerializer.LoadObjectFromFile(acq1_Path);
            Calibate_mtb1 = (CogToolBlock)CogSerializer.LoadObjectFromFile(mtb1_Path);
            Center_tb = (CogToolBlock)CogSerializer.LoadObjectFromFile(Center_Path);
            Calibate_CalibateTool9 = (CogCalibNPointToNPointTool)CogSerializer.LoadObjectFromFile(Calibate_Path);
            textBox4.Enabled = false;
            textBox5.Enabled = false;
          if(f.ChoiceCamera=="相机一") 
          {
              if (f.Vision.旋转中心[0])
              {
                  cogToolBlockEditV21.Subject = Center_tb;
                  
                  textBox4.Text = INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Config_Path, "相机功能0", "中心X", "");
                  textBox5.Text = INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Config_Path, "相机功能0", "中心Y", "");
              }
              else
              {
                  tabControl2.TabPages.Remove(tabPage1);
              }
          }
          if (f.ChoiceCamera == "相机二")
          {
              if (f.Vision.旋转中心[1])
              {
                  cogToolBlockEditV21.Subject = Center_tb;
                  textBox4.Text = INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Config_Path, "相机功能1", "中心X", "");
                  textBox5.Text = INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Config_Path, "相机功能1", "中心Y", "");
              }
              else
              {
                  tabControl2.TabPages.Remove(tabPage1);
              }
          }
          if (f.ChoiceCamera == "相机三")
          {
              if (f.Vision.旋转中心[2])
              {
                  cogToolBlockEditV21.Subject = Center_tb;
                  textBox4.Text = INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Config_Path, "相机功能2", "中心X", "");
                  textBox5.Text = INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Config_Path, "相机功能2", "中心Y", "");
              }
              else
              {
                  tabControl2.TabPages.Remove(tabPage1);
              }
          }
          if (f.ChoiceCamera == "相机四")
          {
              if (f.Vision.旋转中心[3])
              {
                  cogToolBlockEditV21.Subject = Center_tb;
                  textBox4.Text = INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Config_Path, "相机功能3", "中心X", "");
                  textBox5.Text = INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Config_Path, "相机功能3", "中心Y", "");
              }
              else
              {
                  tabControl2.TabPages.Remove(tabPage1);
              }
          }
            
            if (f.相机硬件传递名称 == "Vision")
            {
                acqTool = new CogAcqFifoEditV2();
                acqTool.Subject = Calibate_acq1;
                tableLayoutPanel10.Controls.Add(acqTool);
                acqTool.Dock = DockStyle.Fill;
            }
            else
            {
                    display = new CogDisplay();
                    tableLayoutPanel10.Controls.Add(display);
                    display.Dock = DockStyle.Fill;
               
            }
            cogToolBlockEditV22.Subject = Calibate_mtb1;
            cogCalibNPointToNPointEditV21.Subject = Calibate_CalibateTool9;
         
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (f.相机硬件传递名称 == "Vision")
            {
                Calibate_acq1.Run();
            }
            else
            {
                Choicecamerpinpai();
            }
           
        }
        public void Choicecamerpinpai()
        {
            if (f.ChoiceCamera == "相机一")
            {
                Thread t1 = new Thread(writeCamera);
                t1.IsBackground = true;
                t1.Start(0);
            }
            else if (f.ChoiceCamera == "相机二")
            {
                Thread t2 = new Thread(writeCamera);
                t2.IsBackground = true;
                t2.Start(1);
            }
            else if (f.ChoiceCamera == "相机三")
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
            f.hk[Convert.ToInt16(i)].RunOnce(Convert.ToInt16(textBox1.Text));
            while (!f.hk[Convert.ToInt16(i)].pictuer)
            {
                Thread.Sleep(100);
            }
            display.Image = f.hk[Convert.ToInt16(i)].HKImage;
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (f.相机硬件传递名称 == "Vision")
                {
                    CogSerializer.SaveObjectToFile(Calibate_acq1, acq1_Path);
                    MessageBox.Show("保存成功");
                }
               
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存失败");
            }
           
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Calibate_acq1.Run();
            if (Calibate_acq1.OutputImage != null)
            {
                Calibate_mtb1.Inputs["Image"].Value = Calibate_acq1.OutputImage;
                Calibate_mtb1.Run();
            }
            else
            {
                MessageBox.Show("请查看相机");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                CogSerializer.SaveObjectToFile(Calibate_mtb1, mtb1_Path);
                MessageBox.Show("保存成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存失败，检查作业");
            }
        }


        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                CogSerializer.SaveObjectToFile(Calibate_mtb1, mtb1_Path);
                MessageBox.Show("保存成功");
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
         
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                CogSerializer.SaveObjectToFile(Calibate_CalibateTool9, Calibate_Path);
                MessageBox.Show("保存成功");
            }
            catch(Exception ex)
            {
                MessageBox.Show("保存失败");
            }
          
        }

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                if (f.相机硬件传递名称 == "Vision")
                {
                    if (checkBox1.Checked)
                    {
                        if (textBox3.Text != null)
                        {
                            Calibate_acq1.Run();
                            Calibate_mtb1.Inputs["Image"].Value = Calibate_acq1.OutputImage;
                            Calibate_mtb1.Run();
                            if (Calibate_mtb1.RunStatus.Result == CogToolResultConstants.Accept)
                            {
                                X = (double)Calibate_mtb1.Outputs["X"].Value;
                                Y = (double)Calibate_mtb1.Outputs["Y"].Value;
                                Calibate_CalibateTool9.Calibration.SetUncalibratedPoint(Convert.ToInt16(textBox3.Text), X, Y);

                            }
                            else
                            {
                                MessageBox.Show("作业运行失败");
                            }
                        }
                        else
                        {
                            MessageBox.Show("请选择点位");
                        }
                    }
                    else
                    {
                        Calibate_acq1.Run();
                        Calibate_mtb1.Inputs["Image"].Value = Calibate_acq1.OutputImage;
                        Calibate_mtb1.Run();

                    }
                }
                else
                {
                    if (f.ChoiceCamera == "相机一")
                    {
                        Thread t1 = new Thread(writeCamera1);
                        t1.IsBackground = true;
                        t1.Start(0);
                    }
                    else if (f.ChoiceCamera == "相机二")
                    {
                        Thread t2 = new Thread(writeCamera1);
                        t2.IsBackground = true;
                        t2.Start(1);
                    }
                    else if (f.ChoiceCamera == "相机三")
                    {
                        Thread t3 = new Thread(writeCamera1);
                        t3.IsBackground = true;
                        t3.Start(2);
                    }
                    else
                    {
                        Thread t4 = new Thread(writeCamera1);
                        t4.IsBackground = true;
                        t4.Start(3);
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Calibate_acq1.Run();
            Calibate_CalibateTool9.InputImage = Calibate_acq1.OutputImage;
        }

    
          bool RunTB()
        {
            X =0;
            Y = 0;
            Calibate_acq1.Run();
            if (Calibate_acq1.OutputImage != null)
            {
                Calibate_mtb1.Inputs["Image"].Value = Calibate_acq1.OutputImage;
                Calibate_mtb1.Run();
                if (Calibate_mtb1.RunStatus.Result == CogToolResultConstants.Accept)
                {
                    X = Convert.ToDouble(Calibate_mtb1.Outputs["X"].Value.ToString());
                    Y =Convert.ToDouble(Calibate_mtb1.Outputs["Y"].Value.ToString());
                    return true;
                }
                else
                {
                    return false;
                    MessageBox.Show("作业运行失败");
                }
            }
            else
            {
                return false;
                MessageBox.Show("请查看相机");
            }
        }

          private void writeCamera1(object i)
          {
              f.hk[Convert.ToInt16(i)].RunOnce(Convert.ToInt16(textBox1.Text));
              while (!f.hk[Convert.ToInt16(i)].pictuer)
              {
                  Thread.Sleep(100);
              }
              Cabliateimage = f.hk[Convert.ToInt16(i)].HKImage;
              Calibate_mtb1.Inputs["Image"].Value = Cabliateimage;
              Calibate_mtb1.Run();
              if (Calibate_mtb1.RunStatus.Result == CogToolResultConstants.Accept)
              {
                  X = (double)Calibate_mtb1.Outputs["X"].Value;
                  Y = (double)Calibate_mtb1.Outputs["Y"].Value;
                  if (checkBox1.Checked)
                  {
                      Calibate_CalibateTool9.Calibration.SetUncalibratedPoint(Convert.ToInt16(textBox3.Text), X, Y);
                  }
                

              }
          }

          private void button3_Click_1(object sender, EventArgs e)
          {
              try
              {
                  CogSerializer.SaveObjectToFile(Calibate_mtb1, mtb1_Path);
                  MessageBox.Show("保存成功");
              }
              catch (Exception ex)
              {
                  MessageBox.Show(ex.ToString());
              }
          }

          private void cogCalibNPointToNPointEditV21_Load(object sender, EventArgs e)
          {
              if (f.相机硬件传递名称 == "Vision")
              {
                  Calibate_acq1.Run();
              }
              else
              {
                  Choicecamerpinpai1();
              }
          }

          public void Choicecamerpinpai1()
          {
              if (f.ChoiceCamera == "相机一")
              {
                  Thread t1 = new Thread(writeCamera2);
                  t1.IsBackground = true;
                  t1.Start(0);
              }
              else if (f.ChoiceCamera == "相机二")
              {
                  Thread t2 = new Thread(writeCamera2);
                  t2.IsBackground = true;
                  t2.Start(1);
              }
              else if (f.ChoiceCamera == "相机三")
              {
                  Thread t3 = new Thread(writeCamera2);
                  t3.IsBackground = true;
                  t3.Start(2);
              }
              else
              {
                  Thread t4 = new Thread(writeCamera2);
                  t4.IsBackground = true;
                  t4.Start(3);
              }
          }

          private void writeCamera2(object i)
          {
              f.hk[Convert.ToInt16(i)].RunOnce(Convert.ToInt16(textBox1.Text));
              while (!f.hk[Convert.ToInt16(i)].pictuer)
              {
                  Thread.Sleep(100);
              }
              Cabliateimage = f.hk[Convert.ToInt16(i)].HKImage;
              Calibate_CalibateTool9.InputImage = Cabliateimage;
          }

          private void button4_Click_1(object sender, EventArgs e)
          {
              if (f.相机硬件传递名称 == "Vision")
              {
                  旋转中心点 = textBox2.Text;
                  Calibate_acq1.Run();
                  Calibate_CalibateTool9.InputImage = Calibate_acq1.OutputImage;
                  Calibate_CalibateTool9.Run();
                  Center_tb.Inputs["Image"].Value = Calibate_CalibateTool9.OutputImage;
                  Center_tb.Inputs["Point"].Value = 旋转中心点;
                  Center_tb.Run();
              }
              else
              {
                  xuanzhuanCamer();
              }
          }

          private void xuanzhuanCamer()
          {
              if (f.ChoiceCamera == "相机一")
              {
                  旋转中心点 = textBox2.Text;
                  f.hk[0].RunOnce(Convert.ToInt16(textBox1.Text));
                  while (!f.hk[Convert.ToInt16(0)].pictuer)
                  {
                      Thread.Sleep(100);
                  }

                  Calibate_CalibateTool9.InputImage = f.hk[Convert.ToInt16(0)].HKImage;
                  Calibate_CalibateTool9.Run();
                  Center_tb.Inputs["Image"].Value = Calibate_CalibateTool9.OutputImage;
                  Center_tb.Inputs["Point"].Value = 旋转中心点;
                  Center_tb.Run();
              }
              if (f.ChoiceCamera == "相机二")
              {
                  旋转中心点 = textBox2.Text;
                  f.hk[1].RunOnce(Convert.ToInt16(textBox1.Text));
                  while (!f.hk[Convert.ToInt16(1)].pictuer)
                  {
                      Thread.Sleep(100);
                  }
                  Cabliateimage = f.hk[Convert.ToInt16(1)].HKImage;
                  Calibate_CalibateTool9.InputImage = Cabliateimage;
                  Calibate_CalibateTool9.Run();
                  Center_tb.Inputs["Image"].Value = Calibate_CalibateTool9.OutputImage;
                  Center_tb.Inputs["Point"].Value = 旋转中心点;
                  Center_tb.Run();
              }
              if (f.ChoiceCamera == "相机三")
              {
                  旋转中心点 = textBox2.Text;
                  f.hk[2].RunOnce(Convert.ToInt16(textBox1.Text));
                  while (!f.hk[Convert.ToInt16(2)].pictuer)
                  {
                      Thread.Sleep(100);
                  }
                  Cabliateimage = f.hk[Convert.ToInt16(2)].HKImage;
                  Calibate_CalibateTool9.InputImage = Cabliateimage;
                  Calibate_CalibateTool9.Run();
                  Center_tb.Inputs["Image"].Value = Calibate_CalibateTool9.OutputImage;
                  Center_tb.Inputs["Point"].Value = 旋转中心点;
                  Center_tb.Run();
              }
              if (f.ChoiceCamera == "相机四")
              {
                  旋转中心点 = textBox2.Text;
                  f.hk[3].RunOnce(Convert.ToInt16(textBox1.Text));
                  while (!f.hk[Convert.ToInt16(3)].pictuer)
                  {
                      Thread.Sleep(100);
                  }
                  Cabliateimage = f.hk[Convert.ToInt16(3)].HKImage;
                  Calibate_CalibateTool9.InputImage = Cabliateimage;
                  Calibate_CalibateTool9.Run();
                  Center_tb.Inputs["Image"].Value = Calibate_CalibateTool9.OutputImage;
                  Center_tb.Inputs["Point"].Value = 旋转中心点;
                  Center_tb.Run();
              }
          }

          private void button6_Click_1(object sender, EventArgs e)
          {
              try
              {
                  CogSerializer.SaveObjectToFile(Center_tb, Center_Path);
                  MessageBox.Show("保存成功");
              }
              catch (Exception ex)
              {
                  MessageBox.Show("保存失败");
              }
             
          }

          private void button9_Click(object sender, EventArgs e)
          {
              if (f.ChoiceCamera=="相机一")
              {
                  if(f.Vision.旋转中心[0])
                  {
                      中心X = textBox4.Text;
                      中心Y = textBox5.Text;
                      INIOperationClass.INIOperationClass.INIWriteValue(Vision.Config_Path, "相机功能0", "中心X", 中心X);
                      INIOperationClass.INIOperationClass.INIWriteValue(Vision.Config_Path, "相机功能0", "中心Y", 中心Y);
                  }
              }
              if (f.ChoiceCamera == "相机二")
              {
                  if (f.Vision.旋转中心[1])
                  {
                      中心X = textBox4.Text;
                      中心Y = textBox5.Text;
                      INIOperationClass.INIOperationClass.INIWriteValue(Vision.Config_Path, "相机功能1", "中心X", 中心X);
                      INIOperationClass.INIOperationClass.INIWriteValue(Vision.Config_Path, "相机功能1", "中心Y", 中心Y);
                  }
              }
              if (f.ChoiceCamera == "相机三")
              {
                  if (f.Vision.旋转中心[2])
                  {
                      中心X = textBox4.Text;
                      中心Y = textBox5.Text;
                      INIOperationClass.INIOperationClass.INIWriteValue(Vision.Config_Path, "相机功能2", "中心X", 中心X);
                      INIOperationClass.INIOperationClass.INIWriteValue(Vision.Config_Path, "相机功能2", "中心Y", 中心Y);
                  }
              }
              if (f.ChoiceCamera == "相机四")
              {
                  if (f.Vision.旋转中心[0])
                  {
                      中心X = textBox4.Text;
                      中心Y = textBox5.Text;
                      INIOperationClass.INIOperationClass.INIWriteValue(Vision.Config_Path, "相机功能3", "中心X", 中心X);
                      INIOperationClass.INIOperationClass.INIWriteValue(Vision.Config_Path, "相机功能3", "中心Y", 中心Y);
                  }
              }
          }

          private void checkBox2_CheckedChanged(object sender, EventArgs e)
          {
              if (checkBox2.Checked)
              {
                  textBox4.Enabled = true;
                  textBox5.Enabled = true;
              }
              else
              {
                  textBox4.Enabled = false;
                  textBox5.Enabled = false;
              }
          }
    }
}
