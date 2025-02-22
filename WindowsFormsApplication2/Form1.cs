using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net.NetworkInformation;
using Cognex.VisionPro;
using Cognex.VisionPro.ToolBlock;
using Cognex.VisionPro.CalibFix;
using Cognex.VisionPro.ImageFile;
using INIOperationClass;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.Sockets;
using MvCamCtrl.NET;
using System.Runtime.InteropServices;
using DataLogger;

namespace WindowsFormsApplication2
{
    public partial class Form1 : Form
    {
        public Vision Vision = new Vision();
        public HKTOVP[] hk = new HKTOVP[10];
        public string ChoiceCamera;
        public CogAcqFifoTool ChoiceAcq;
        public string ChoiceAcq_Path;
        public CogCalibNPointToNPointTool Choice_Calibate;
        public string Choice_Calibate_Path;
        public CogToolBlock choiceRunTB;
        public string choiceRunTB_path;
        string CameraResultXYA;
        public string 相机硬件传递名称;
        public bool HKstopanniu = true;
        int HKCamerCount;
        public CLS_AsyncTcpServer server;
        int serverCount;
        public TcpClient tc, tc1, 断开通讯IP;
        public string ReceiveDate;
        bool 上相机, 下相机;
        public CogAcqInfo IOinfo;
        public ICogImage IOImage;
        public TcpClient[] RobotTC;
        bool upcame1 = true;
        double robotstandU, robottakeU;
        string[] code;
        string StartTime = "Null";

        public Form1()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 禁用菜单栏
        /// </summary>
        private void DisableMenuItems()
        {
            产品选择ToolStripMenuItem.Enabled = false;
            相机取向ToolStripMenuItem.Enabled = false;
            相机标定ToolStripMenuItem.Enabled = false;
            作业ToolStripMenuItem.Enabled = false;
            参数设置ToolStripMenuItem.Enabled = false;
            通讯ToolStripMenuItem.Enabled = false;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            
                if (moreThanOneInstance)
                {
                    MessageBox.Show("程序已经在运行", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                }
            DisableMenuItems();
            checkBox3.Enabled = false;
            tabControl1.TabPages.Remove(tabPage1);
            tabControl1.TabPages.Remove(tabPage2);
            button2.Enabled = false;
            IniCamera();
            Camerashow();
            Thread t1 = new Thread(ReadProducename);
            t1.IsBackground = true;
            t1.Start();
            Thread t2 = new Thread(ReadCamera);
            t2.IsBackground = true;
            t2.Start();
            delepicture();
            StartTcpServer();
            Client();


        }

        #region 程序运行次数
        bool moreThanOneInstance
        {
            get
            {
                string currentName = Process.GetCurrentProcess().ProcessName;//定义字符串currentName，返回进程的名称
                return Process.GetProcessesByName(currentName).Length > 1;//判断进程名称为currentName的元素总数是否〉1
            }
        }
        #endregion

        #region 检查加密狗
        //bool notLicensed
        //{
        //    get
        //    {
        //        var features = CogMisc.GetLicensedFeatures(false);
        //        if (features.Count > 0) ;
        //        {
        //            return true;
        //            WriteTxt("加密狗加载成功");
        //        }
        //    }
        //}

        #endregion

        #region 判断相机连接是否异常
        bool pingDevice(string ip)
        {
            Ping sender = new Ping();//初始化System.Net.NetworkInformation.Ping类的新实例
            PingReply reply;//提供有关 Overload:System.Net.NetworkInformation.Ping.Send或Overload:System.Net.NetworkInformation.Ping.SendAsync操作的状态及产生的数据的信息
            try
            {
                reply = sender.Send(ip, 1000);//尝试将Internet控制消息协议(ICMP)回送消息发送到具有指定System.Net.IPAddress的计算机，并接收来自该计算机的相应ICMP回送答复消息
            }
            catch (PingException)//捕捉异常
            {
                return false;//返回False
                WriteTxt("相机加载失败");
                MessageBox.Show("查看硬件连接");
            }
            if (reply.Status == IPStatus.Success)//判断来自计算机的相应ICMP回送答复消息是否=Success
            {
                return true;//返回True
            }
            else
            {
                return false;//返回False
                WriteTxt("相机加载失败");
                MessageBox.Show("查看硬件连接");
            }
        }
        #endregion end of 判断相机连接是否异常

        #region 判断相机是否连接
        bool ReadCameraIP()
        {
            try
            {
                int cam = 0;
                Vision.CameraIP = new string[Vision.Cameranum];
                for (int i = 0; i < Vision.Cameranum; i++)
                {
                    Vision.CameraIP[i] = INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Config_Path, "相机功能" + i, "相机IP", "");
                    if (!pingDevice(Vision.CameraIP[i]))
                    {
                        return false;
                        break;
                    }
                    cam++;
                }
                toolStripStatusLabel31.Text = Convert.ToString(cam);
                return true;
            }
            catch (Exception ex)
            {
                WriteTxt("相机加载失败");
                MessageBox.Show("相机数量与IP数量不对应，请查看配置文件");
                return false;
            }
        }
        #endregion

        #region 判断通讯网线状态
        bool RobotIP()
        {
            try
            {
                return true;
            }
            catch (Exception ex)
            {
                return false;

            }


        }
        #endregion

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            INIOperationClass.INIOperationClass.INIWriteValue(Vision.Produce_Date, "Message", "JGlue_GRN-Q'ty",Convert .ToString ( Vision .JGlue_GRN_count));
            INIOperationClass.INIOperationClass.INIWriteValue(Vision.Produce_Date, "Message", "JLens_GRN-Q'ty", Convert.ToString(Vision.JLens_GRN_count));
            INIOperationClass.INIOperationClass.INIWriteValue(Vision.Produce_Date, "Message", "JLens_QR_code-Q'ty", Convert.ToString(Vision.JLens_QR_code_count));

            Application.Exit();
            System.Environment.Exit(System.Environment.ExitCode);
        }

        public  void WriteTxt(string mes)
        {
            string t1 = System.DateTime.Now.ToString("HH:mm:ss");
            if (textBox1.Lines.Count() > 1000)
            {
                Invoke(new MethodInvoker(delegate
                {
                    textBox1.Clear();
                }));

            }
            Invoke(new MethodInvoker(delegate
            {
                textBox1.AppendText(t1 + ":" + mes + "\r\n");
            }));
        }

        #region 读取料号
        private void ReadProducename()
        {
            产品选择ToolStripMenuItem.DropDownItems.Clear();
            Vision.producenum = Convert.ToInt16(INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Config_Path, "产品", "产品数量", ""));
            Vision.ProduceName = new string[Vision.producenum];
         
            for (int i = 0; i < Vision.producenum; i++)
            {
                Vision.ProduceName[i] = INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Config_Path, "产品", "产品名称" + i, "");
               
            }
            Invoke(new MethodInvoker(delegate
            {
                label3.Text = Vision.ProduceName[0];
            }));
            Thread t1 = new Thread(new ParameterizedThreadStart(LoadVpp));
            t1.IsBackground = true;
            t1.Start(Vision.ProduceName[0]);
          
            foreach (string pName in Vision.ProduceName)
            {
                ToolStripMenuItem subMenu = new ToolStripMenuItem(pName);
                subMenu.Tag = pName;
                产品选择ToolStripMenuItem.DropDownItems.Add(subMenu);
                subMenu.Click += subMenuProduct_Click;
            }

        }

        private void subMenuProduct_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem subMenu = (ToolStripMenuItem)sender;
            string pName = subMenu.Tag.ToString();
            this.Text = pName;
            Invoke(new MethodInvoker(delegate
            {
                label3.Text = pName;
            }));
            WriteTxt("切换产品" + pName + "请等待。。。");
            LoadVpp(pName);
        }
        #endregion

        #region 读取IP地址

        #endregion

        #region 读取相机
        private void ReadCamera()
        {
            相机取向ToolStripMenuItem.DropDownItems.Clear();
            相机标定ToolStripMenuItem.DropDownItems.Clear();
            作业ToolStripMenuItem.DropDownItems.Clear();
            Vision.Cameraname = new string[Vision.Cameranum];
           
             HKCamerCount = 0;
            for (int i = 0; i < Vision.Cameranum; i++)
            {
                Vision.Cameraname[i] = INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Config_Path, " 配置", "相机名称" + i, "");
                if (Vision.相机品牌[i] != "Vision")
                {
                    HKCamerCount++;
                  
                }
            }
         
            foreach (string pName in Vision.Cameraname)
            {
                ToolStripMenuItem subMenu = new ToolStripMenuItem(pName);
                subMenu.Tag = pName;
                相机取向ToolStripMenuItem.DropDownItems.Add(subMenu);
                subMenu.Click += Cameraname_Click;
            }
            foreach (string pName in Vision.Cameraname)
            {
                ToolStripMenuItem subMenu = new ToolStripMenuItem(pName);
                subMenu.Tag = pName;
                相机标定ToolStripMenuItem.DropDownItems.Add(subMenu);
                subMenu.Click += CameraBoard_Click;
            }
            foreach (string pName in Vision.Cameraname)
            {
                ToolStripMenuItem subMenu = new ToolStripMenuItem(pName);
                subMenu.Tag = pName;
                 作业ToolStripMenuItem.DropDownItems.Add(subMenu);
                 subMenu.Click += CameraWorkTB_Click;
            }
        }
        private void Cameraname_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem subMenu = (ToolStripMenuItem)sender;
            ChoiceCamera = subMenu.Tag.ToString();
          
            if (ChoiceCamera == "相机一" )
            {
                if (Vision.相机品牌[0] == "Vision")
                {
                    ChoiceAcq = Vision.camera[0];
                    ChoiceAcq_Path = Vision.camera_Path[0];
                    CameraAcq c1 = new CameraAcq(this);
                    c1.ShowDialog();
                }
                else
                {
                    CamerName n1 = new CamerName(this);
                    n1.ShowDialog();
                }
               
            }
            else if (ChoiceCamera == "相机二")
            {
                if (Vision.相机品牌[1] == "Vision")
                {
                    ChoiceAcq = Vision.camera[1];
                    ChoiceAcq_Path = Vision.camera_Path[1];
                    CameraAcq c2 = new CameraAcq(this);
                    c2.ShowDialog();
                }
                else
                {
                    CamerName n2 = new CamerName(this);
                    n2.ShowDialog();
                }
              
            }
            else if (ChoiceCamera == "相机三")
            {
                if (Vision.相机品牌[2] == "Vision")
                {
                    ChoiceAcq = Vision.camera[2];
                    ChoiceAcq_Path = Vision.camera_Path[2];
                    CameraAcq c3 = new CameraAcq(this);
                    c3.ShowDialog();
                }
                else
                {
                    CamerName n3 = new CamerName(this);
                    n3.ShowDialog();
                }
            }
            else
            {
                if (Vision.相机品牌[3] == "Vision")
                {
                    ChoiceAcq = Vision.camera[3];
                    ChoiceAcq_Path = Vision.camera_Path[3];
                    CameraAcq c4 = new CameraAcq(this);
                    c4.ShowDialog();
                }
                else
                {
                    CamerName n4 = new CamerName(this);
                    n4.ShowDialog();
                }
               
            }
        }
        #endregion

        #region 读取标定
    
        private void CameraBoard_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem subMenu = (ToolStripMenuItem)sender;
            ChoiceCamera = subMenu.Tag.ToString();
            if (ChoiceCamera == "相机一")
            {
                相机硬件传递名称 = Vision.相机品牌[0];
            }
            else if(ChoiceCamera == "相机二")
            {
                相机硬件传递名称 = Vision.相机品牌[1];
            }
            else if (ChoiceCamera == "相机三")
            {
                相机硬件传递名称 = Vision.相机品牌[2];
            }
            else
            {
                相机硬件传递名称 = Vision.相机品牌[3];
            }
            CameraCalibate c1 = new CameraCalibate(this);
            c1.ShowDialog();

        }

        #endregion

        #region 初始化数据
        private void IniCamera()
        {
            try
            {
                Vision.Cameranum = Convert.ToInt16(INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Config_Path, "配置", "相机数量", ""));
                Vision.机械手数量 = INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Config_Path, "配置", "机械手数量", "");
                RobotTC = new TcpClient[Convert.ToInt16(Vision.机械手数量)];
                Vision.机械手IP = new string[Convert.ToInt16(Vision.机械手数量)];
                Vision.相机品牌 = new string[Vision.Cameranum];
                Vision.相机曝光 = new string[Vision.Cameranum];
                Vision.旋转中心 = new bool[Vision.Cameranum];
                Vision.拍照信号 = new string[Vision.Cameranum];
                Vision.相机功能 = new string[Vision.Cameranum];
                Vision.显示图层 = new string[Vision.Cameranum];
                Vision.中心X = new double[Vision.Cameranum];
                Vision.中心Y = new double[Vision.Cameranum];
                Vision.基准A = new double[Vision.Cameranum];
                Vision.基准X = new double[Vision.Cameranum];
                Vision.基准Y = new double[Vision.Cameranum];
                Vision.运行X = new double[Vision.Cameranum];
                Vision.运行Y = new double[Vision.Cameranum];
                Vision.运行A = new double[Vision.Cameranum];
                Vision.偏移X = new double[Vision.Cameranum];
                Vision.偏移Y = new double[Vision.Cameranum];
                Vision.偏移A = new double[Vision.Cameranum];

                for (int i = 0; i < Convert.ToInt16(Vision.Cameranum); i++)
                {
                    
                    Vision.显示图层[i] = INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Config_Path, "相机功能" + i, "显示图层", "");
                    Vision.相机品牌[i] = INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Config_Path, " 配置", "相机品牌" + i, "");
                    Vision.相机曝光[i] = INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Config_Path, " 配置", "曝光" + i, "");
                    Vision.旋转中心[i] = Convert.ToBoolean(INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Config_Path, "相机功能" + i, "旋转中心", ""));
                    if (Vision.旋转中心[i])
                    {
                        Vision.中心X[i] = Convert.ToDouble(INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Config_Path, "相机功能" + i, "中心X", ""));
                        Vision.中心Y[i] = Convert.ToDouble(INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Config_Path, "相机功能" + i, "中心Y", ""));
                        
                    }
                    Vision.基准A[i] = Convert.ToDouble(INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Config_Path, "相机功能" + i, "基准A", ""));
                    Vision.基准X[i] = Convert.ToDouble(INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Config_Path, "相机功能" + i, "基准X", ""));
                    Vision.基准Y[i] = Convert.ToDouble(INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Config_Path, "相机功能" + i, "基准Y", ""));
                   
                    Vision.拍照信号[i] = INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Config_Path, " 相机功能" + i, "相机拍照信号", "");
                    Vision.相机功能[i] = INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Config_Path, " 相机功能" + i, "相机功能", "");
                    if (Vision.相机功能[i] == "飞拍"&&Vision.相机品牌[i]=="Vision")
                    {
                        Vision.camera[i].Operator.OwnedTriggerParams.TriggerModel = CogAcqTriggerModelConstants.Auto;
                        Vision.camera[i].Operator.OwnedTriggerParams.TriggerEnabled = true;
                        Vision.camera[i].Operator.Complete += new CogCompleteEventHandler(CCD_Completel);
                    }
                    Vision.显示图层[i] = INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Config_Path, " 相机功能" + i, "显示图层", "");
                   
                }
                for (int a = 0; a < Convert.ToInt16(Vision.机械手数量); a++)
                {
                    Vision.机械手IP[a] = INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Config_Path, "配置", "机械手IP" + a, "");
                    if (Vision.机械手IP[a] == null)
                    {
                        MessageBox.Show("配置文件错误");
                    }
                }
               
                Vision.机械手X = Convert.ToDouble(INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Config_Path, "位置参数", "组装点X", ""));
                Vision.机械手Y = Convert.ToDouble(INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Config_Path, "位置参数", "组装点Y", ""));
                Vision.机械手A = Convert.ToDouble(INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Config_Path, "位置参数", "组装点A", ""));
                Vision.补偿X = Convert.ToDouble(INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Config_Path, "位置参数", "补偿X", ""));
                Vision.补偿Y = Convert.ToDouble(INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Config_Path, "位置参数", "补偿Y", ""));
                Vision.补偿A = Convert.ToDouble(INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Config_Path, "位置参数", "补偿A", ""));
                Vision.BayNo = INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Produce_Date, "Message", "Bay No.", "");
                Vision .ProductModel= INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Produce_Date, "Message", "Product Model", "");
                Vision.Version= INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Produce_Date, "Message", "Version", "");
                Vision .ProcessStation= INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Produce_Date, "Message", "Process Station", "");
                Vision .JGlue_GRN= INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Produce_Date, "Message", "JGlue_GRN", "");
                Vision.JLens_GRN = INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Produce_Date, "Message", "JLens_GRN", "");
                Vision.JLens_QR_code = INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Produce_Date, "Message", "JLens_QR_code", "");
                Vision.JGlue_GRN_count = Convert.ToInt64(INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Produce_Date, "Message", "JGlue_GRN-Q'ty", ""));
                Vision.JLens_GRN_count = Convert.ToInt64(INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Produce_Date, "Message", "JLens_GRN-Q'ty", ""));
                Vision.JLens_QR_code_count = Convert.ToInt64(INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Produce_Date, "Message", "JLens_QR_code-Q'ty", ""));
                Vision.JGlue_GRN_length = Convert.ToInt16 (INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Produce_Date, "Message", "JGlue_GRN_length", ""));
                Vision.JLens_GRN_length = Convert.ToInt16(INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Produce_Date, "Message", "JLens_GRN_length", ""));
                Vision.JLens_QR_code_length = Convert.ToInt16(INIOperationClass.INIOperationClass.INIGetStringValue(Vision.Produce_Date, "Message", "JLens_QR_code_length", ""));
                label9.Text = Convert.ToString(Vision.JGlue_GRN_count);
                label11.Text = Convert.ToString(Vision.JLens_GRN_count);
                label13.Text = Convert.ToString(Vision.JLens_QR_code_count);
                WriteTxt("配置文件加载成功");
                //if (GetHardDiskFreeSpace("D") < 50)
                //{
                //    MessageBox.Show("内存不足，请查看D盘");
                //    this.Close();
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show("配置文件加载失败");
            }
          
          
        }

        #endregion

        #region 读取作业名称
        private void CameraWorkTB_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem subMenu = (ToolStripMenuItem)sender;
            Vision.choiceWorkTB = subMenu.Tag.ToString();
            if (Vision.choiceWorkTB == "相机一")
            {
                 ChoiceCamera = "相机一";
                 ChoiceAcq = Vision.camera[0];
                 Choice_Calibate = Vision.CalibCamera[0];
                 choiceRunTB = Vision.RunTB[0];
                 choiceRunTB_path = Vision.RunTB_Path[0];
            }
            else if(Vision.choiceWorkTB == "相机二")
            {
                 ChoiceCamera = "相机二";
                 ChoiceAcq = Vision.camera[1];
                 Choice_Calibate = Vision.CalibCamera[1];
                 choiceRunTB = Vision.RunTB[1];
                 choiceRunTB_path = Vision.RunTB_Path[1];
            }
            else if (Vision.choiceWorkTB == "相机三")
            {
                 ChoiceCamera = "相机三";
                 ChoiceAcq = Vision.camera[2];
                 Choice_Calibate = Vision.CalibCamera[2];
                 choiceRunTB = Vision.RunTB[2];
                 choiceRunTB_path = Vision.RunTB_Path[2];
            }
            else
            {
                 ChoiceCamera = "相机四"; 
                 ChoiceAcq = Vision.camera[3];
                 Choice_Calibate = Vision.CalibCamera[3];
                 choiceRunTB = Vision.RunTB[3];
                 choiceRunTB_path = Vision.RunTB_Path[3];
            }
            if (ChoiceCamera == "相机一")
            {
                相机硬件传递名称 = Vision.相机品牌[0];
            }
            else if (ChoiceCamera == "相机二")
            {
                相机硬件传递名称 = Vision.相机品牌[1];
            }
            else if (ChoiceCamera == "相机三")
            {
                相机硬件传递名称 = Vision.相机品牌[2];
            }
            else
            {
                相机硬件传递名称 = Vision.相机品牌[3];
            }

            RunTB c1 = new RunTB(this);
            c1.ShowDialog();
        }
        #endregion

        #region 加载作业
        private void LoadVpp( object name)
        {
            try
            {
                Vision.camera_Path = new string[Vision.Cameranum];
                Vision.CalibateTB_Path = new string[Vision.Cameranum];
                Vision.CalibCamera_Path = new string[Vision.Cameranum];
                Vision.RunTB_Path = new string[Vision.Cameranum];
                Vision.camera = new CogAcqFifoTool[Vision.Cameranum];
                Vision.CalibateTB=new CogToolBlock[Vision.Cameranum];
                Vision.RunTB=new CogToolBlock[Vision.Cameranum];
                Vision.CalibCamera = new CogCalibNPointToNPointTool[Vision.Cameranum];
              if(ReadCameraIP())
                {
                    WriteTxt("相机加载成功");
                    toolStripStatusLabel31.BackColor = Color.Green;
                    toolStripStatusLabel31.Text = "OK";
                }
              else
                {
                    WriteTxt("相机加载失败");
                    toolStripStatusLabel31.BackColor = Color.Red;
                    toolStripStatusLabel31.Text = "NG";
                }
                for (int i = 0; i < Vision.Cameranum; i++)
                {
                    Vision.camera_Path[i] = Application.StartupPath + "\\" + name + "\\ACQ\\" + Vision.Cameraname[i] + ".vpp";
                    Vision.CalibateTB_Path[i] = Application.StartupPath + "\\" + name + "\\Calibate\\" + Vision.Cameraname[i] + ".vpp";
                    Vision.CalibCamera_Path[i] = Application.StartupPath + "\\" + name + "\\Calibate\\" + Vision.Cameraname[i] + "标定.vpp";
                    Vision.RunTB_Path[i] = Application.StartupPath + "\\" + name + "\\TB\\" + Vision.Cameraname[i] + ".vpp";
                    Vision.camera[i] = (CogAcqFifoTool)CogSerializer.LoadObjectFromFile(Vision.camera_Path[i]);
                  //  Vision.CalibateTB[i] = (CogToolBlock)CogSerializer.LoadObjectFromFile(Vision.CalibateTB_Path[i]);
                    Vision.RunTB[i] = (CogToolBlock)CogSerializer.LoadObjectFromFile(Vision.RunTB_Path[i]);
                    Vision.CalibCamera[i] = (CogCalibNPointToNPointTool)CogSerializer.LoadObjectFromFile(Vision.CalibCamera_Path[i]);
                }
                Invoke(new MethodInvoker(delegate
                {
                    toolStripStatusLabel26.Text = "加载成功";
                    toolStripStatusLabel26.BackColor = Color.Green;
                    toolStripStatusLabel7.Text = "加载成功";
                    toolStripStatusLabel7.BackColor = Color.Green;

                }));
                WriteTxt("作业加载成功");
            }
            catch (Exception ex)
            {
                toolStripStatusLabel26.Text = "加载失败";
                toolStripStatusLabel26.BackColor = Color.Red;
                WriteTxt("作业加载失败");
                MessageBox.Show("加载失败"+ex.ToString());
            }
           
        }
        #endregion

        #region 保存作业

        private void SaveWork()
        {
            try
            {
                for (int i = 0; i < Vision.Cameranum; i++)
                {
                    CogSerializer.SaveObjectToFile(Vision.camera[i], Vision.camera_Path[i]);
                    CogSerializer.SaveObjectToFile(Vision.CalibateTB[i], Vision.CalibateTB_Path[i]);
                    CogSerializer.SaveObjectToFile(Vision.RunTB[i], Vision.RunTB_Path[i]);
                    CogSerializer.SaveObjectToFile(Vision.CalibCamera[i], Vision.CalibCamera_Path[i]);
                }
                MessageBox.Show("保存成功");
            }
            catch (Exception ex)
            {
                WriteTxt("保存失败");
                MessageBox.Show("保存失败，请检查作业");
            }
            
        }
        #endregion

        #region 保存图片
        private void SaveRawImage(ICogImage myImage,string Camera,string CameraTBresult)
        {
            if (myImage == null)
                return;
            string Proname = null;
            Invoke(new MethodInvoker(delegate
                {
                    Proname = label3.Text;
                }));
            string folderName = System.DateTime.Now.ToString("yyyyMMdd");
            string imgSaveFullPath = Vision.imgSavePath + "\\" + Proname + "\\" + Camera + "\\";
            string saveName =CameraTBresult +  DateTime.Now.ToString("HHmmssfff") + ".bmp";

            try
            {
                if (!System.IO.Directory.Exists(imgSaveFullPath))
                {
                    System.IO.Directory.CreateDirectory(imgSaveFullPath);
                }
                 CogImageFile imagefileWrite = new CogImageFile();
                 imagefileWrite.Open(imgSaveFullPath + saveName, CogImageFileModeConstants.Write);
                imagefileWrite.Append(myImage);
                imagefileWrite.Close();
            }
            catch (Exception ex)
            {
               
            }
        }

        #endregion

        #region 删除图片
        private void delepicture()
        {
            if (Directory.Exists(Vision.imgSavePath))
            {
                DirectoryInfo dyInfo = new DirectoryInfo(Vision.imgSavePath);
                DirectoryInfo[] dir = dyInfo.GetDirectories();
                foreach (DirectoryInfo dy in dir)
                {
                    if (dy.CreationTime.AddDays(Vision.imgSaveDays) < DateTime.Now)
                    {
                        dy.Delete(true);
                    }
                }
            }
        }
        #endregion

        #region 保存日志
         private static object _lock_1 = new object();
        public static string strErrorLogPath;
        public static void SaveErrorLog(string ErrMsgEvent)
        {
            lock (_lock_1)
            {
                strErrorLogPath = System.AppDomain.CurrentDomain.BaseDirectory + "\\RunLog\\ErrorLog"; ;
                if (!Directory.Exists(strErrorLogPath)) //判断文件夹是否存在
                {
                    Directory.CreateDirectory(strErrorLogPath);
                }
                strErrorLogPath = strErrorLogPath + "\\" + DateTime.Now.ToString("yyyy年M月d日") + ".txt";
                StreamWriter sw = new StreamWriter(strErrorLogPath, true);
                sw.Flush();
                sw.WriteLine(DateTime.Now.ToString() + "：" + ErrMsgEvent);
                sw.Close();
            }
        }
        #endregion

        private void button2_Click(object sender, EventArgs e)
        {
            SaveWork();
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void 登录ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (登录ToolStripMenuItem.Text == "登 录")
            {
                login lg = new login(this);
                lg.ShowDialog();
            }
            else
            {
                tabControl1.TabPages.Remove(this.tabPage1);
                //  tabControl1.TabPages.Remove(this.tabPage2);
                DisableMenuItems();
                button2.Enabled = false;
                checkBox3.Enabled = false;
                登录ToolStripMenuItem.Text = "登 录";
            }
           
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (checkBox1.Checked)
                {
                    Vision.display[0].StartLiveDisplay(Vision.camera[0].Operator);
                    Vision.display[0].StaticGraphics.Clear();
                    Vision.display[0].InteractiveGraphics.Clear();
                    WriteTxt("实时打开");
                }
                else
                {
                    Vision.display[0].StopLiveDisplay();
                    Vision.camera[0].Run();
                    Vision.display[0].Image = Vision.camera[0].OutputImage;
                    Vision.display[0].Fit(true);
                    WriteTxt("关闭实时");
                }
            }
            catch (Exception ex)
            {
                WriteTxt("相机打开失败，请查看相机连接是否正常");
            }

        }

        #region 串口通讯
       /* private void SerialP()
        {
            string [] ports=System.IO.Ports.SerialPort.GetPortNames();
            if(ports.Length==0)
            {
                MessageBox.Show("没有串口");
            }
            serialPort1.PortName= "串口号";
            serialPort1.BaudRate = "波特率";
            serialPort1.DataBits='数据未';
            serialPort1.StopBits=System.IO.Ports.StopBits.One;//停止位
            serialPort1.Encoding=System.Text.Encoding.GetEncoding("GB2312");
            serialPort1.Open();
        }*/

        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            string txt=serialPort1.ReadExisting();
        }
        #endregion

        #region 界面显示
        private void Camerashow()
        {
           Vision.display = new CogRecordDisplay[Vision.Cameranum];
           Vision.cameraTxtlabel = new Label[Vision.Cameranum];
           Vision.cameraResultlabel = new Label[Vision.Cameranum];
          
           for (int i = 0; i < Vision.Cameranum;i++ )
           {
               Vision.display[i] = new CogRecordDisplay();
               Vision.cameraTxtlabel[i] = new Label();
               Vision.cameraResultlabel[i] = new Label();
           }

           if (Vision.Cameranum ==1)
           {
               tableLayoutPanel7.ColumnCount = 1;
               tableLayoutPanel7.RowCount = 1;
               tableLayoutPanel7.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
               tableLayoutPanel7.Controls.Add(Vision.display[0], 0, 0);
               Invoke(new MethodInvoker(delegate
                   {
                       Vision.cameraTxtlabel[0].Text = "相机一结果：";
                       Vision.cameraResultlabel[0].Text = "Null";
                     
                   }));
               tableLayoutPanel8.Controls.Add(Vision.cameraTxtlabel[0], 0, 0);
               tableLayoutPanel8.Controls.Add(Vision.cameraResultlabel[0], 1, 0);
               Vision.cameraTxtlabel[0].Dock = DockStyle.Fill;
               Vision.cameraResultlabel[0].Dock = DockStyle.Fill;
               Vision.display[0].Dock = DockStyle.Fill;
               Vision.cameraTxtlabel[0].Dock = DockStyle.Fill;
               Vision.cameraResultlabel[0].Dock = DockStyle.Fill;

           }
         else  if (Vision.Cameranum == 2)
           {
               tableLayoutPanel7.ColumnCount = 2;
               tableLayoutPanel7.RowCount = 1;
               tableLayoutPanel7.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
               tableLayoutPanel7.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
               tableLayoutPanel7.Controls.Add(Vision.display[0], 0, 0);
               tableLayoutPanel7.Controls.Add(Vision.display[1], 1, 0);
               Invoke(new MethodInvoker(delegate
               {
                   Vision.cameraTxtlabel[0].Text = "相机一结果：";
                   Vision.cameraResultlabel[0].Text = "Null";
                   Vision.cameraTxtlabel[1].Text = "相机二结果：";
                   Vision.cameraResultlabel[1].Text = "Null";

               }));
               tableLayoutPanel8.Controls.Add(Vision.cameraTxtlabel[0], 0, 0);
               tableLayoutPanel8.Controls.Add(Vision.cameraResultlabel[0], 1, 0);
               tableLayoutPanel8.Controls.Add(Vision.cameraTxtlabel[1], 0, 1);
               tableLayoutPanel8.Controls.Add(Vision.cameraResultlabel[1], 1, 1);
               Vision.cameraTxtlabel[0].Dock = DockStyle.Fill;
               Vision.cameraResultlabel[0].Dock = DockStyle.Fill;
               Vision.cameraTxtlabel[1].Dock = DockStyle.Fill;
               Vision.cameraResultlabel[1].Dock = DockStyle.Fill;
               Vision.cameraTxtlabel[1].Dock = DockStyle.Fill;
               Vision.cameraResultlabel[1].Dock = DockStyle.Fill;
               Vision.display[0].Dock = DockStyle.Fill;
               Vision.display[1].Dock = DockStyle.Fill;
           }
      else     if (Vision.Cameranum == 3 || Vision.Cameranum == 4)
           {
               tableLayoutPanel7.ColumnCount = 2;
               tableLayoutPanel7.RowCount = 2;
               tableLayoutPanel7.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
               tableLayoutPanel7.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
               tableLayoutPanel7.ColumnStyles.Add(new RowStyle(SizeType.Percent, 50F));
               tableLayoutPanel7.ColumnStyles.Add(new RowStyle(SizeType.Percent, 50F));
               tableLayoutPanel7.Controls.Add(Vision.display[0], 0, 0);
               tableLayoutPanel7.Controls.Add(Vision.display[1], 1, 0);
               tableLayoutPanel7.Controls.Add(Vision.display[2], 0, 1);
               Invoke(new MethodInvoker(delegate
               {
                   Vision.cameraTxtlabel[0].Text = "相机一结果：";
                   Vision.cameraResultlabel[0].Text = "Null";
                   Vision.cameraTxtlabel[1].Text = "相机二结果：";
                   Vision.cameraResultlabel[1].Text = "Null";
                   Vision.cameraTxtlabel[2].Text = "相机三结果：";
                   Vision.cameraResultlabel[2].Text = "Null";

               }));
               tableLayoutPanel8.Controls.Add(Vision.cameraTxtlabel[0], 0, 0);
               tableLayoutPanel8.Controls.Add(Vision.cameraResultlabel[0], 1, 0);
               tableLayoutPanel8.Controls.Add(Vision.cameraTxtlabel[1], 0, 1);
               tableLayoutPanel8.Controls.Add(Vision.cameraResultlabel[1], 1, 1);
               tableLayoutPanel8.Controls.Add(Vision.cameraTxtlabel[2], 0, 2);
               tableLayoutPanel8.Controls.Add(Vision.cameraResultlabel[2], 1, 2);
               Vision.cameraTxtlabel[0].Dock = DockStyle.Fill;
               Vision.cameraResultlabel[0].Dock = DockStyle.Fill;
               Vision.cameraTxtlabel[1].Dock = DockStyle.Fill;
               Vision.cameraResultlabel[1].Dock = DockStyle.Fill;
               Vision.cameraTxtlabel[2].Dock = DockStyle.Fill;
               Vision.cameraResultlabel[2].Dock = DockStyle.Fill;
               if (Vision.Cameranum == 4)
               {
                   Invoke(new MethodInvoker(delegate
                   {
                       Vision.cameraTxtlabel[0].Text = "相机一结果：";
                       Vision.cameraResultlabel[0].Text = "Null";
                       Vision.cameraTxtlabel[1].Text = "相机二结果：";
                       Vision.cameraResultlabel[1].Text = "Null";
                       Vision.cameraTxtlabel[2].Text = "相机三结果：";
                       Vision.cameraResultlabel[2].Text = "Null";
                       Vision.cameraTxtlabel[3].Text = "相机四结果：";
                       Vision.cameraResultlabel[3].Text = "Null";
                   }));
                   tableLayoutPanel8.Controls.Add(Vision.cameraTxtlabel[0], 0, 0);
                   tableLayoutPanel8.Controls.Add(Vision.cameraResultlabel[0], 1, 0);
                   tableLayoutPanel8.Controls.Add(Vision.cameraTxtlabel[1], 0, 1);
                   tableLayoutPanel8.Controls.Add(Vision.cameraResultlabel[1], 1, 1);
                   tableLayoutPanel8.Controls.Add(Vision.cameraTxtlabel[2], 0, 2);
                   tableLayoutPanel8.Controls.Add(Vision.cameraResultlabel[2], 1, 2);
                   tableLayoutPanel8.Controls.Add(Vision.cameraTxtlabel[3], 0, 3);
                   tableLayoutPanel8.Controls.Add(Vision.cameraResultlabel[3], 1, 3);
                   Vision.cameraTxtlabel[0].Dock = DockStyle.Fill;
                   Vision.cameraResultlabel[0].Dock = DockStyle.Fill;
                   Vision.cameraTxtlabel[1].Dock = DockStyle.Fill;
                   Vision.cameraResultlabel[1].Dock = DockStyle.Fill;
                   Vision.cameraTxtlabel[2].Dock = DockStyle.Fill;
                   Vision.cameraResultlabel[2].Dock = DockStyle.Fill;
                   Vision.cameraTxtlabel[3].Dock = DockStyle.Fill;
                   Vision.cameraResultlabel[3].Dock = DockStyle.Fill;
                   tableLayoutPanel7.Controls.Add(Vision.display[3], 1, 1);
                   Vision.display[3].Dock = DockStyle.Fill;

               }
               Vision.display[0].Dock = DockStyle.Fill;
               Vision.display[1].Dock = DockStyle.Fill;
               Vision.display[2].Dock = DockStyle.Fill;
           }
           else
           {
               MessageBox.Show("超出数量");
               this.Close();
           }
        }

        #endregion

        #region Display显示字体
        public static void ShowResult(CogGraphicLabel myCglCaption, double X, double Y, CogColorConstants myColor, String Msg, CogRecordDisplay myRecordDisplay)//在图像框显示结果
        {
            myCglCaption = new CogGraphicLabel();
            myCglCaption.Text = Msg;//要显示文字
            myCglCaption.Alignment = CogGraphicLabelAlignmentConstants.TopLeft;//显示在标签的位置

            myCglCaption.Font = new Font("楷体", 18, FontStyle.Bold);//显示字体
            myCglCaption.Color = myColor;//显示颜色
            myCglCaption.SelectedSpaceName = "#";
            myCglCaption.X = X;//显示位置
            myCglCaption.Y = Y;
            myRecordDisplay.StaticGraphics.Add(myCglCaption, myCglCaption.Text);//将显示字体添加到图层中
        }
        #endregion

        #region TCp服务器
        private void StartTcpServer()
        {
            try
            {
                server = new CLS_AsyncTcpServer(11000);
                server.Encoding = Encoding.UTF8;
                server.ClientConnected += new EventHandler<TcpClientConnectedEventArgs>(ClientConnected);
                server.ClientDisconnected += new EventHandler<TcpClientDisconnectedEventArgs>(ClientDisconnected);
                server.PlaintextReceived += new EventHandler<TcpDatagramReceivedEventArgs<string>>(ServerPlaintextReceived);
                try
                {
                    server.Start();
                }
                catch
                {
                    WriteTxt("服务器开启失败");
                    Invoke(new MethodInvoker(delegate
                    {
                        toolStripStatusLabel11.Text = "服务器开启失败";
                        toolStripStatusLabel1.BackColor = Color.Red;
                        SaveErrorLog("服务器开启失败");
                    }));
                }
                if (server.IsRunning)
                {
                    WriteTxt("服务器开启成功");
                    Invoke(new MethodInvoker(delegate
                    {
                        toolStripStatusLabel11.Text = "服务器开启成功";
                        toolStripStatusLabel11.BackColor = Color.Green;
                    }));

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        private void ServerPlaintextReceived(object sender, TcpDatagramReceivedEventArgs<string> e)
        {
            ReceiveDate = e.Datagram.Replace("\r", "");
            ReceiveDate = ReceiveDate.Replace("\0", "");
            string clientIP = ((IPEndPoint)tc.Client.RemoteEndPoint).Address.ToString();
            WriteTxt(clientIP + "接受数据:" + ReceiveDate);
            Thread t1 = new Thread(Camerafunction);
            t1.IsBackground = true;
            t1.Start(ReceiveDate);
              
        }

        private void ClientDisconnected(object sender, TcpClientDisconnectedEventArgs e)
        {
            try
            {
                serverCount--;
                Invoke(new MethodInvoker(delegate
                {
                    toolStripStatusLabel15.Text = Convert.ToString(serverCount);
                    if (serverCount == 0)
                    {
                        toolStripStatusLabel19.Text = "未连接";
                        toolStripStatusLabel19.BackColor = Color.White;
                    }

                    断开通讯IP = e.TcpClient;
                    string clientIP = ((IPEndPoint)断开通讯IP.Client.RemoteEndPoint).Address.ToString();
                    WriteTxt("客户端：" + clientIP + "断开连接");

                    if (serverCount != 2)
                    {
                        toolStripStatusLabel15.BackColor = Color.Red;
                    }
                    else
                    {
                        toolStripStatusLabel15.BackColor = Color.Green;
                    }

                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        private void ClientConnected(object sender, TcpClientConnectedEventArgs e)
        {
            serverCount++;
            if (serverCount > 2)
            {
                MessageBox.Show("机械手通讯逻辑异常");
            }
            Invoke(new MethodInvoker(delegate
            {
                if (serverCount == 0)
                {
                    toolStripStatusLabel19.Text = "未连接";
                    toolStripStatusLabel19.BackColor = Color.White;
                }
                else
                {
                    tc = e.TcpClient;
                    string clientIP1 = ((IPEndPoint)e.TcpClient.Client.RemoteEndPoint).Address.ToString();
                    for (int i = 0; i < Convert.ToInt16(Vision.机械手数量); i++)
                    {
                        if (clientIP1 == Vision.机械手IP[i])
                        {
                            WriteTxt("客户端：" + clientIP1);
                            RobotTC[i] = e.TcpClient;
                        }
                    }
                        toolStripStatusLabel19.Text = "连接成功";
                    toolStripStatusLabel19.BackColor = Color.Green;
                    toolStripStatusLabel15.Text = Convert.ToString(serverCount);
                    if (serverCount == Convert.ToInt16(Vision.机械手数量))
                    {
                        toolStripStatusLabel15.BackColor = Color.Green;
                    }
                    else
                    {
                        toolStripStatusLabel15.BackColor = Color.Red;
                    }
                    Invoke(new MethodInvoker(delegate
                    {
                        toolStripStatusLabel15.Text = Convert.ToString(serverCount);
                    }));
                }
            }));
        }
        #endregion

        #region TCP IP客户端
        private void Client()
        {
            try
            {
                Vision.tcpClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress ipaddress = IPAddress.Parse(Vision.clientIP);
                EndPoint point = new IPEndPoint(ipaddress, Convert.ToInt16(Vision.clientPort));
                Vision.tcpClient.Connect(point);
                toolStripStatusLabel19.BackColor = Color.Green ;
                toolStripStatusLabel19.Text = "连接成功";
                Thread t1 = new Thread(ClientReceiveDate);
                t1.IsBackground = true;
                t1.Start(Vision.tcpClient);
            }
            catch (Exception EX)
            {
                WriteTxt("机械手连接失败");
                toolStripStatusLabel19.BackColor = Color.Red;
                toolStripStatusLabel19.Text = "连接失败";
            }
            

        }

        private void ClientReceiveDate(object o)
        {
            try
            {
                Socket client = o as Socket;
                while (true)
                {
                    byte[] data = new byte[1024];
                    int length = Vision.tcpClient.Receive(data);
                    if (length != 0)
                    {
                        string message = Encoding.UTF8.GetString(data, 0, length);
                        WriteTxt("接受数据：" + message);
                        if (message == "T1")
                        {
                            if (checkBox3.Checked)
                            {
                                Thread.Sleep(500);
                                ClientSend("SetVar B009 1");
                            }
                            else
                            {
                                Thread t1 = new Thread(readCode);
                                t1.IsBackground = true;
                                t1.Start();
                            }
                        }
                    }
                    else
                    {
                        client.Close();
                        WriteTxt("服务器已关闭");
                        break;
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("上传失败");
            }
          
        }

        private void ClientSend(string mes)
        {
           Vision.tcpClient.Send(Encoding.UTF8.GetBytes(mes));
        }
        
        #endregion

        #region 飞拍抓取
        private void CCD_Completel(object obj, CogCompleteEventArgs e)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    CogCompleteEventHandler mydel = new CogCompleteEventHandler(CCD_Completel);
                    object[] args = new object[] { obj, e };
                    Invoke(mydel, args);
                    return;
                }
                Vision.camera[0].Operator.OwnedTriggerParams.TriggerModel = CogAcqTriggerModelConstants.Auto;
                Vision.camera[0].Operator.OwnedTriggerParams.TriggerEnabled = true;
                int numReadyVal = 0, numPendingVal = 0;
                bool busyVal = false;
                IOinfo = new CogAcqInfo();
                Vision.camera[0].Operator.GetFifoState(out numPendingVal, out numReadyVal, out busyVal);
                if (numReadyVal > 0)
                {
                    WriteTxt("相机一拍照");
                    Vision.camera[0].Operator.OwnedTriggerParams.TriggerModel = CogAcqTriggerModelConstants.FreeRun;
                    IOImage = Vision.camera[0].Operator.CompleteAcquireEx(IOinfo);
                    WorkTBRun(IOImage);
                    Vision.camera[0].Operator.OwnedTriggerParams.TriggerModel = CogAcqTriggerModelConstants.Auto;
                    Vision.camera[0].Operator.OwnedTriggerParams.TriggerEnabled = true;
                    WriteTxt("相机一完成");
                }

            }
            catch (Exception ex)
            {
                SaveErrorLog(ex.ToString());
            }
        }
        #endregion

        private void button5_Click(object sender, EventArgs e)
        { Thread t1 = new Thread(readCode);
                t1.IsBackground = true;
            t1.Start();
          
        }

        private void CameraRunCon()
        {
            hk[0].RunContinuing();
            while (HKstopanniu)
            {
                Vision.display[0].Image = hk[0].HKImage;
            }
        }

        private  void SendRoto(string mes)
        {
         
        }

        private void button3_Click(object sender, EventArgs e)
        {
            server.Send(RobotTC[0], "1,1,1,1,1" + "\r\n");
        }

        private void 参数设置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            参数设定 lg = new 参数设定(this);
            lg.ShowDialog();
        }

        private void 作业ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {
            UpXY();

        }

        #region 相机功能解析
        public  void Camerafunction(object  RobotMessage)
        {
           /* for (int i = 0; i < Vision.Cameranum; i++)
            {
                if (ReceiveDate == Vision.拍照信号[i])
                {
                    if (Vision.相机功能[i] =="双向引导0")
                    {
                        下相机 = false;
                        UpCamera(i);
                       
                    }
                    else if (Vision.相机功能[i] == "双向引导1")
                    {
                        上相机 = false;
                        UpCamera(i);
                    }
                    else
                    {
                        OneCamera(i);
                    }
                }
            }*/
            if (ReceiveDate == "TAKE1")
            {
                jiaodun();
            }
            else if (ReceiveDate == "TAKE2")
            {
                Thread t1 = new Thread(UpXY);
              t1.IsBackground =true;
                t1.Start();
            }
            else if (ReceiveDate == "TAKE3")
            {
                XYA();
            }
            else
            {

            }
        }
        #endregion

        #region 双向引导
       void UpCamera(int i)
        {
            Vision.display[i].Image = null;
            Vision.运行X[i] = 0;
            Vision.运行Y[i] = 0;
            Vision.运行A[i] = 0;
            if (Vision.相机品牌[Convert.ToInt16(i)] == "Vision")
            {
                WriteTxt("相机运行");
                Vision.camera[i].Run();
                Vision.CalibCamera[i].InputImage = Vision.camera[i].OutputImage;
            }
            else
            {
                int 曝光 =Convert .ToInt16( Vision.相机曝光[i]);
                hk[i].RunOnce(曝光);
                while (!hk[i].pictuer)
                {
                    Thread.Sleep(100);
                }
                Vision.CalibCamera[i].InputImage = hk[i].HKImage;
            }
            Vision.CalibCamera[i].Run();
            Vision.RunTB[i].Inputs["Image"].Value = Vision.CalibCamera[i].OutputImage;
            Vision.RunTB[i].Run();
            if (Vision.RunTB[i].RunStatus.Result == CogToolResultConstants.Accept)
            {
                Vision.display[i].Record = Vision.RunTB[i].CreateLastRunRecord().SubRecords[Vision.显示图层[i]];
                Vision.display[i].Fit(true);
                Invoke(new MethodInvoker(delegate
                {
                    Vision.cameraResultlabel[i].Text = "OK";
                    Vision.cameraResultlabel[i].BackColor = Color.Green;
                }));
                Vision.运行X[i] =Convert .ToDouble( Vision.RunTB[i].Outputs["X"].Value);
                Vision.运行Y[i] = Convert.ToDouble(Vision.RunTB[i].Outputs["Y"].Value);
                Vision.运行A[i] = Convert.ToDouble(Vision.RunTB[i].Outputs["A"].Value);
                if (Vision.旋转中心[i])
                {
                    下相机 = true;
                }
                else
                {
                    上相机 = true;
                }
                if (Vision.Cameraname[i] == "相机一")
                {
                    server.Send(RobotTC[0], "2" + "\r\n");
                }
                WriteTxt("X:" + Vision.运行X[i].ToString("0.000") + ";" + "Y:" + Vision.运行Y[i].ToString("0.000") + ";" + "A:" + Vision.运行A[i].ToString("0.000") + ".");
                if (上相机 && 下相机 && Vision.旋转中心[0])
                {
                    上相机 = false;
                    下相机 = false;
                    Thread t1 = new Thread(DoubleCameraCenter);
                    t1.IsBackground = true;
                    t1.Start();
                }
                else if (上相机 && 下相机 && !Vision.旋转中心[0])
                {
                    上相机 = false;
                    下相机 = false;
                    DoubleCamera();
                }
            }
            else
            {
                Vision.display[i].Image = Vision.CalibCamera[i].OutputImage;
                Vision.display[i].Fit(true);
                if (Vision.Cameraname[i] == "相机一")
                {
                    server.Send(RobotTC[0], "1" + "\r\n");
                }
                else
                {
                    server.Send(RobotTC[0], "2,1" + "\r\n");
                }
                Invoke(new MethodInvoker(delegate
                {
                    Vision.cameraResultlabel[i].Text = "NG";
                    Vision.cameraResultlabel[i].BackColor = Color.Red;
                }));
            }
        }
        #endregion

        #region 双向引导旋转中心
       /*RXO、RYO为旋转中心，a为旋转弧度，X、Y为被旋转的点，X0，Y0旋转后的点
        //X0=Cos(a)*(X-RX0)-sin(a)*(Y-RY0)+RX0
         Y0=Cos(a)*(Y-RY0)+sin(a)*(X-RX0)+RY0*/
       private void DoubleCameraCenter()
       {
          
           Vision.偏移A[0] =  Vision.基准A[0]-Vision.运行A[0] ;
           Vision.偏移A[1] = Vision.运行A[1] - Vision.基准A[1];
           Vision.坐标A = Vision.偏移A[1] + Vision.偏移A[0] + Vision.补偿A;
           double DownEndX = Math.Cos(Vision.坐标A) * (Vision.运行X[0] - Vision.中心X[0]) - Math.Sin(Vision.坐标A) * (Vision.运行Y[0] - Vision.中心Y[0]) + Vision.中心X[0];
           double DownEndY = Math.Cos(Vision.坐标A) * (Vision.运行Y[0] - Vision.中心Y[0]) + Math.Sin(Vision.坐标A) * (Vision.运行X[0] - Vision.中心X[0]) + Vision.中心Y[0];
        
           Vision.偏移X[0] = Vision.基准X[0] - DownEndX;
           Vision.偏移Y[0] = Vision.基准Y[0] - DownEndY;
           Vision.偏移X[1] = Vision.运行X[1] - Vision.基准X[1];
           Vision.偏移Y[1] = Vision.运行Y[1] - Vision.基准Y[1];
           Vision.坐标A = CogMisc.RadToDeg(Vision.坐标A)+Vision.机械手A;
           Vision.坐标X = Vision.偏移X[1] + Vision.偏移X[0] + Vision.补偿X + Vision.机械手X;
           Vision.坐标Y = Vision.偏移Y[0] + Vision.偏移Y[1] + Vision.补偿Y + Vision.机械手Y;
           server.Send(RobotTC[0], "2,2," + Vision.坐标X + "," + Vision.坐标Y + "," + Vision.坐标A + "\r\n");
           WriteTxt("X:" + Vision.坐标X.ToString("0.000") + ";" + "Y:" + Vision.坐标Y.ToString("0.000") + ";" + "A:" + Vision.坐标A.ToString("0.000") + ".");
       

       }

       private void DoubleCamera()
       {

           Vision.坐标X = (Vision.基准X[0] - Vision.运行X[0]) + (Vision.基准X[1] - Vision.运行X[1]) + Vision.机械手X + Vision.补偿X;
           Vision.坐标Y = (Vision.基准Y[0] - Vision.运行Y[0]) + (Vision.基准Y[1] - Vision.运行Y[1]) + Vision.机械手X + Vision.补偿Y;
       }
     
        #endregion

        #region 单向引导
       void OneCamera(int i)
       {
            Vision.运行X[i] = 0;
            Vision.运行Y[i] = 0;
            Vision.运行A[i] = 0;
            if (Vision.相机品牌[Convert.ToInt16(i)] == "Vision")
            {
                WriteTxt("相机运行");
                Vision.camera[i].Run();
                Vision.CalibCamera[i].InputImage = Vision.camera[i].OutputImage;

            }
            else
            {
               hk[i].RunOnce(Convert.ToInt16( Vision.相机曝光[i]));
                while (hk[Convert.ToInt16(i)].pictuer)
                {
                    Thread.Sleep(100);
                }
                Vision.CalibCamera[i].InputImage = hk[i].HKImage;
            }
            Vision.CalibCamera[i].Run();
            Vision.RunTB[i].Inputs["Image"].Value = Vision.CalibCamera[i].OutputImage;
            Vision.RunTB[i].Run();
            if (Vision.RunTB[i].RunStatus.Result == CogToolResultConstants.Accept)
            {
                Vision.display[i].Record = Vision.RunTB[i].CreateLastRunRecord().SubRecords[Vision.显示图层[i]];
                Vision.display[i].Fit(true);
                Invoke(new MethodInvoker(delegate
                {
                    Vision.cameraResultlabel[i].Text = "OK";
                    Vision.cameraResultlabel[i].BackColor = Color.Green;
                }));
                Vision.运行X[i] = Convert.ToDouble(Vision.RunTB[i].Outputs["X"].Value);
                Vision.运行Y[i] = Convert.ToDouble(Vision.RunTB[i].Outputs["Y"].Value);
                Vision.运行A[i] = Convert.ToDouble(Vision.RunTB[i].Outputs["A"].Value);
                if (Vision.旋转中心[i])
                {
                    Vision.坐标A = Vision.运行A[i] - Vision.基准A[i] + Vision.补偿A;
                    double DownEndX = Math.Cos(Vision.坐标A) * (Vision.运行X[i] - Vision.中心X[i]) - Math.Sin(Vision.坐标A) * (Vision.运行Y[i] - Vision.中心Y[i]) + Vision.中心X[i];
                    double DownEndY = Math.Cos(Vision.坐标A) * (Vision.运行Y[i] - Vision.中心Y[i]) + Math.Sin(Vision.坐标A) * (Vision.运行X[i] - Vision.中心X[i]) + Vision.中心Y[i];
                    Vision.坐标A = CogMisc.RadToDeg(Vision.坐标A) + Vision.机械手A;
                    Vision.坐标X = Vision.基准X[i] - DownEndX + Vision.机械手X + Vision.补偿X;
                    Vision.坐标Y = Vision.基准Y[i] - DownEndY + Vision.机械手Y + Vision.补偿Y;
                    WriteTxt("X:" + Vision.坐标X.ToString("0.000") + ";" + "Y:" + Vision.坐标Y.ToString("0.000") + ";" + "A:" + Vision.坐标A.ToString("0.000") + ".");

                }
                else
                {
                    Vision.坐标X = Vision.基准X[i] - Vision.运行X[i] + Vision.机械手X + Vision.补偿X;
                    Vision.坐标Y = Vision.基准Y[i] - Vision.运行Y[i] + Vision.机械手Y + Vision.补偿Y;

                }
                
            }
            else
            {
                Vision.display[i].Image = Vision.CalibCamera[i].OutputImage;
                Vision.display[i].Fit(true);
                Invoke(new MethodInvoker(delegate
                {
                    Vision.cameraResultlabel[i].Text = "NG";
                    Vision.cameraResultlabel[i].BackColor = Color.Red;
                }));
            }

       }
        #endregion

        #region 飞拍
       void WorkTBRun(ICogImage IOoutImage)
       {
           Vision.CalibCamera[0].InputImage = IOoutImage;
           Vision.CalibCamera[0].Run();
           Vision.RunTB[0].Inputs["Image"].Value = Vision.CalibCamera[0].OutputImage;
           Vision.RunTB[0].Run();
           if (Vision.RunTB[0].RunStatus.Result == CogToolResultConstants.Accept)
           {
               Vision.坐标X = Convert.ToDouble(Vision.RunTB[0].Outputs["X"].Value);
               Vision.坐标Y = Convert.ToDouble(Vision.RunTB[0].Outputs["Y"].Value);
               Vision.坐标A = Convert.ToDouble(Vision.RunTB[0].Outputs["A"].Value);
               Vision.坐标X = Vision.坐标X + Vision.补偿X;
               Vision.坐标Y = Vision.坐标Y + Vision.补偿Y;
               Vision.坐标A = Vision.坐标A + Vision.补偿A;
               Vision.坐标A = CogMisc.RadToDeg(Vision.坐标A);
               WriteTxt("X:" + Vision.坐标X.ToString("0.000") + ";" + "Y:" + Vision.坐标Y.ToString("0.000") + ";" + "A:" + Vision.坐标A.ToString("0.000") + ".");
           }
       }

        #endregion

        #region 拍摄2次
       #region 下相机拍摄角度
       private void jiaodun()
       {
           int 曝光 = Convert.ToInt16(Vision.相机曝光[0]);
           hk[0].RunOnce(曝光);
           while (!hk[0].pictuer)
           {
               Thread.Sleep(100);
           }
           Vision.CalibCamera[0].InputImage = hk[0].HKImage;
           Vision.CalibCamera[0].Run();
           Vision.RunTB[0].Inputs["Image"].Value = Vision.CalibCamera[0].OutputImage;
           Vision.RunTB[0].Run();
           if (Vision.RunTB[0].RunStatus.Result == CogToolResultConstants.Accept)
           {
               Vision.display[0].Record = Vision.RunTB[0].CreateLastRunRecord().SubRecords[Vision.显示图层[0]];
               Vision.display[0].Fit(true);
               Invoke(new MethodInvoker(delegate
               {
                   Vision.cameraResultlabel[0].Text = "OK";
                   Vision.cameraResultlabel[0].BackColor = Color.Green;
               }));
               while (upcame1)
               {
                   Thread.Sleep(50);
               }
               Vision.运行A[0] = Convert.ToDouble(Vision.RunTB[0].Outputs["A"].Value);
               Vision.偏移A[0] = Vision.基准A[0] - Vision.运行A[0];
               Vision.坐标A = Vision.偏移A[1] + Vision.偏移A[0] + Vision.补偿A;
               Vision.坐标A = CogMisc.RadToDeg(Vision.坐标A);
               server.Send(RobotTC[0], Vision.坐标A + "\r\n");
               WriteTxt(Convert.ToString(Vision.坐标A));
           }
       }
       #endregion

       #region 下相机补偿X，Y
       private void XYA()
       {
           int 曝光 = Convert.ToInt16(Vision.相机曝光[0]);
           hk[0].RunOnce(曝光);
           while (!hk[0].pictuer)
           {
               Thread.Sleep(20);
           }
           Vision.CalibCamera[0].InputImage = hk[0].HKImage;
           Vision.CalibCamera[0].Run();
           Vision.RunTB[0].Inputs["Image"].Value = Vision.CalibCamera[0].OutputImage;
           Vision.RunTB[0].Run();
           if (Vision.RunTB[0].RunStatus.Result == CogToolResultConstants.Accept)
           {
               Vision.display[0].Record = Vision.RunTB[0].CreateLastRunRecord().SubRecords[Vision.显示图层[0]];
               Vision.display[0].Fit(true);
               Invoke(new MethodInvoker(delegate
               {
                   Vision.cameraResultlabel[0].Text = "OK";
                   Vision.cameraResultlabel[0].BackColor = Color.Green;
               }));
               Vision.运行X[0] = Convert.ToDouble(Vision.RunTB[0].Outputs["X"].Value);
               Vision.运行Y[0] = Convert.ToDouble(Vision.RunTB[0].Outputs["Y"].Value);
               Vision.偏移X[0] = Vision.基准X[0] - Vision.运行X[0];
               Vision.偏移Y[0] = Vision.基准Y[0] - Vision.运行Y[0];
               Vision.坐标X = Vision.偏移X[1] + Vision.偏移X[0] + Vision.补偿X + 516.424;
               Vision.坐标Y = Vision.偏移Y[0] + Vision.偏移Y[1] + Vision.补偿Y + 365.461;
               double a = -35.069;
               Vision.坐标A = Vision.坐标A + a;

               //X=516.424 Y=365.461 A=-35.069

               server.Send(RobotTC[0], "2,2," + Vision.坐标X + "," + Vision.坐标Y + "," + Vision.坐标A + "\r\n");
               WriteTxt("X:" + Vision.坐标X + " Y:" + Vision.坐标Y + " A:" + Vision.坐标A);
           }
       }

       #endregion

       #region 上相机拍摄 X，Y，A
       private void UpXY()
       {
           upcame1 = true;
           int 曝光 = Convert.ToInt16(Vision.相机曝光[1]);
           hk[1].RunOnce(曝光);
           while (!hk[1].pictuer)
           {
               Thread.Sleep(100);
           }

           Vision.CalibCamera[1].InputImage = hk[1].HKImage;
           Vision.CalibCamera[1].Run();
           Vision.RunTB[1].Inputs["Image"].Value = Vision.CalibCamera[1].OutputImage;
           Vision.RunTB[1].Run();
           if (Vision.RunTB[1].RunStatus.Result == CogToolResultConstants.Accept)
           {
               Vision.display[1].Record = Vision.RunTB[1].CreateLastRunRecord().SubRecords[Vision.显示图层[1]];
               Vision.display[1].Fit(true);
               Invoke(new MethodInvoker(delegate
               {
                   Vision.cameraResultlabel[1].Text = "OK";
                   Vision.cameraResultlabel[1].BackColor = Color.Green;
               }));
               Vision.运行X[1] = Convert.ToDouble(Vision.RunTB[1].Outputs["X"].Value);
               Vision.运行Y[1] = Convert.ToDouble(Vision.RunTB[1].Outputs["Y"].Value);
               Vision.运行A[1] = Convert.ToDouble(Vision.RunTB[1].Outputs["A"].Value);
               Vision.偏移A[1] = Vision.运行A[1] - Vision.基准A[1];
               double upA = CogMisc.RadToDeg(Vision.偏移A[1]);
               Vision.偏移X[1] = Vision.运行X[1] - Vision.基准X[1];
               Vision.偏移Y[1] = Vision.运行Y[1] - Vision.基准Y[1];
               upcame1 = false;
               WriteTxt("偏移X1：" + Vision.偏移X[1] + "偏移Y1：" + Vision.偏移Y[1]);
           }
           else
           {
               Vision.display[1].Image = Vision.CalibCamera[1].OutputImage;
               Vision.display[1].Fit(true);
           }
       }
       #endregion
       #endregion

        #region 读码

       private void readCode()
       {
            StartTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); 
            code = new string[16];
            string station = null;
            Vision.display[0].Image = null;
            Vision.camera[0].Run();
            Vision.RunTB[0].Inputs["Image"].Value = Vision.camera[0].OutputImage;
            Vision.RunTB[0].Run();
            if(Vision .RunTB [0].RunStatus .Result ==CogToolResultConstants.Accept )
            {
                if (Convert.ToString(Vision.RunTB[0].Outputs["Result"].Value) == "OK")
                {
                    Vision.display[0].Record = Vision.RunTB[0].CreateLastRunRecord().SubRecords["CogFixtureTool1.OutputImage"];
                    Vision.display[0].Fit(true);
                    for (int i=0;i<16;i++)
                    {
                        code[i] = Convert.ToString(Vision.RunTB[0].Outputs["ID" + (1 + i)].Value);
                        /* Thread t1 = new Thread(delegate () { writeDate(code[i]); });
                         t1.IsBackground = true;
                         t1.Start();*/
                        writeDate(code[i]);
                        WriteTxt(code[i]);
                    }
                    ClientSend("SetVar B009 1");
                    Vision.cameraResultlabel[0].Text = "OK";
                    Vision.cameraResultlabel[0].BackColor = Color.Green;
                }
                else
                {
                    ClientSend("SetVar B009 2");
                    Vision.display[0].Record = Vision.RunTB[0].CreateLastRunRecord().SubRecords["CogFixtureTool1.OutputImage"];
                    Vision.display[0].Fit(true);
                    station = Convert.ToString(Vision.RunTB[0].Outputs["Station"].Value);

                    WriteTxt(station);
                    Vision.cameraResultlabel[0].Text = "NG";
                    Vision.cameraResultlabel[0].BackColor = Color.Red;
                    MessageBox.Show(station);
                }
            }
            else
            {
                Vision.display[0].Image = Vision.camera[0].OutputImage;
                ClientSend("SetVar B009 2");
                WriteTxt("作业运行失败");
                Vision.cameraResultlabel[0].Text = "NG";
                Vision.cameraResultlabel[0].BackColor = Color.Red;
            }


        }
        #endregion

        #region 读取空间内存
       public static long GetHardDiskFreeSpace(string str_HardDiskName)
       {

           long freeSpace = new long();
           str_HardDiskName = str_HardDiskName + ":\\";
           System.IO.DriveInfo[] drives = System.IO.DriveInfo.GetDrives();
           foreach (System.IO.DriveInfo drive in drives)
           {
               if (drive.Name == str_HardDiskName)
               {
                   freeSpace = drive.TotalFreeSpace / (1024 * 1024 * 1024);
               }
           }
           return freeSpace;
       }
        #endregion

        #region 写入数据
        bool numstation = false;
       private void writeDate(string code)
       {
            try
            {
                if (Vision.JGlue_GRN_count == 0 || Vision.JLens_GRN_count == 0 || Vision.JLens_QR_code_count == 0)
                {
                    numstation = true;
                    ClientSend("SetVar B009 3");
                    MessageBox.Show("补充数据");
                }
                else
                {
                    numstation = false;
                }
                while (numstation)
                {
                    if (Vision.JGlue_GRN_count == 0 || Vision.JLens_GRN_count == 0 || Vision.JLens_QR_code_count == 0)
                    {
                        numstation = true;
                    }
                    else
                    {
                        numstation = false;
                    }
                }
                string txtpath = @"C:\Users\3929768\Desktop\Dyson扫码\扫码20250220\autotars";
                string txtpath1 = @"C:\Users\3929768\Desktop\Dyson扫码\扫码20250220\TarBackup\\" + DateTime.Now.ToString("MM月dd日");
                string time =System. DateTime.Now.ToString("yyyyMMddHHhhss");
                if (Directory.Exists(txtpath) == false)
                {
                    Directory.CreateDirectory(txtpath);
                }
                if (Directory.Exists(txtpath1) == false)
                {
                    Directory.CreateDirectory(txtpath1);
                }
                txtpath = txtpath + "\\" + time +"_"+code + ".txt";
                txtpath1 = txtpath1 + "\\"  + time + "_" + code + ".txt";
                StreamWriter sw = new StreamWriter(txtpath, true);
                StreamWriter sw1 = new StreamWriter(txtpath1, true);
                sw.Flush();
                sw1.Flush();
                sw.WriteLine("S" + code);
                sw1.WriteLine("S" + code);
                sw.WriteLine("CDyson");
                sw1.WriteLine("CDyson");
                sw.WriteLine("N"+Vision.BayNo);
                sw1.WriteLine("N" + Vision.BayNo);
                sw.WriteLine("nDY-424433-01-10");
                sw1.WriteLine("nDY-424433-01-10");

                sw.WriteLine("r0001");
                sw1.WriteLine("r0001");
                sw.WriteLine("PLens_Link");
                sw1.WriteLine("PLens_Link");

                sw.WriteLine("TP");
                sw1.WriteLine("TP");

                sw.WriteLine("[" + StartTime);
                sw1.WriteLine("[" + StartTime);
              //  string endtime1234=System. DateTime.Now.ToString("HH:hh:ss");//yyyy-MM-dd
                string endtime12345 = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                sw.WriteLine("]" + endtime12345);
                sw.WriteLine("MGlue GRN");
                sw.WriteLine("d" + Vision.JGlue_GRN);
                sw.WriteLine(">string");
                sw.WriteLine("MLens GRN");
                sw.WriteLine("d" + Vision.JLens_GRN);
                sw.WriteLine(">string");
                sw.WriteLine("MLens QR code");
                sw.WriteLine("d" + Vision.JLens_QR_code);
                sw.WriteLine(">string");
                sw.WriteLine("JGlue GRN");
                sw.WriteLine("K" + Vision.JGlue_GRN);
                sw.WriteLine("JLens GRN");
                sw.WriteLine("K" + Vision.JLens_GRN);
                sw.WriteLine("JLens QR code");
                sw.WriteLine("K" + Vision.JLens_QR_code);
                sw1.WriteLine("]" + endtime12345);
                sw1.WriteLine("MGlue GRN");
                sw1.WriteLine("d" + Vision.JGlue_GRN);
                sw1.WriteLine(">string");
                sw1.WriteLine("MLens GRN");
                sw1.WriteLine("d" + Vision.JLens_GRN);
                sw1.WriteLine(">string");
                sw1.WriteLine("MLens QR code");
                sw1.WriteLine("d" + Vision.JLens_QR_code);
                sw1.WriteLine(">string");
                sw1.WriteLine("JGlue GRN");
                sw1.WriteLine("K" + Vision.JGlue_GRN);
                sw1.WriteLine("JLens GRN");
                sw1.WriteLine("K" + Vision.JLens_GRN);
                sw1.WriteLine("JLens QR code");
                sw1.WriteLine("K" + Vision.JLens_QR_code);
                sw.Close();
                sw1.Close();
                Vision.JGlue_GRN_count--;
                Vision.JLens_GRN_count--;
                Vision.JLens_QR_code_count--;
                label9.Text = Convert.ToString(Vision.JGlue_GRN_count);
                label11.Text = Convert.ToString(Vision.JLens_GRN_count);
                label13.Text = Convert.ToString(Vision.JLens_QR_code_count);
                //if (GetHardDiskFreeSpace("D") < 50)
                //{
                //    MessageBox.Show("内存不足，请查看D盘");
                //    this.Close();
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        
        }
        #endregion





    }
}
